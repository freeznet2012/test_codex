# Deployment Guide

This guide walks through deploying the backend and frontend applications to Azure, provisioning Azure SQL, and preparing for future alternative data providers.

## Prerequisites

- Azure subscription with contributor access.
- Azure CLI (`az`) installed and logged in (`az login`).
- GitHub CLI (`gh`) installed if you plan to configure GitHub Actions workflows.
- Existing Azure resource group (or permissions to create one).
- Local copy of the project repository.

### Environment Variables

Set the following environment variables locally before running commands:

```bash
export AZURE_RESOURCE_GROUP="<resource-group-name>"
export AZURE_LOCATION="<azure-region>"   # e.g., eastus, westeurope
export AZURE_SQL_SERVER="<sql-server-name>"  # globally unique
export AZURE_SQL_DB="<database-name>"
export AZURE_APP_SERVICE_PLAN="<app-service-plan-name>"
export AZURE_BACKEND_APP="<backend-app-service-name>"
export AZURE_FRONTEND_APP="<frontend-app-service-name>"  # for App Service deployments
```

## Backend Deployment (Azure App Service)

### 1. Create (or select) the Resource Group

```bash
az group create \
  --name "$AZURE_RESOURCE_GROUP" \
  --location "$AZURE_LOCATION"
```

### 2. Provision Azure SQL

1. Create the logical SQL server and database:

    ```bash
    az sql server create \
      --resource-group "$AZURE_RESOURCE_GROUP" \
      --name "$AZURE_SQL_SERVER" \
      --location "$AZURE_LOCATION" \
      --admin-user <sql-admin-user> \
      --admin-password <sql-admin-password>

    az sql db create \
      --resource-group "$AZURE_RESOURCE_GROUP" \
      --server "$AZURE_SQL_SERVER" \
      --name "$AZURE_SQL_DB" \
      --service-objective S0
    ```

2. Configure firewall rules so App Service and your IP can connect:

    ```bash
    az sql server firewall-rule create \
      --resource-group "$AZURE_RESOURCE_GROUP" \
      --server "$AZURE_SQL_SERVER" \
      --name AllowAppServiceAccess \
      --start-ip-address 0.0.0.0 \
      --end-ip-address 0.0.0.0

    az sql server firewall-rule create \
      --resource-group "$AZURE_RESOURCE_GROUP" \
      --server "$AZURE_SQL_SERVER" \
      --name AllowLocalIP \
      --start-ip-address <your-public-ip> \
      --end-ip-address <your-public-ip>
    ```

3. Capture the connection string:

    ```bash
    SQL_CONNECTION_STRING=$(az sql db show-connection-string \
      --name "$AZURE_SQL_DB" \
      --server "$AZURE_SQL_SERVER" \
      --client ado.net \
      --output tsv)
    ```

    Update it with your admin username and password.

### 3. Create an App Service Plan and Web App

```bash
az appservice plan create \
  --name "$AZURE_APP_SERVICE_PLAN" \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --sku B1 \
  --is-linux

az webapp create \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --plan "$AZURE_APP_SERVICE_PLAN" \
  --name "$AZURE_BACKEND_APP" \
  --runtime "DOTNET:8"
```

Adjust the runtime to match your backend (e.g., `PYTHON:3.10`, `NODE:18LTS`).

### 4. Configure Connection Strings and Settings

```bash
az webapp config connection-string set \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --name "$AZURE_BACKEND_APP" \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="$SQL_CONNECTION_STRING"

az webapp config appsettings set \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --name "$AZURE_BACKEND_APP" \
  --settings ASPNETCORE_ENVIRONMENT=Production
```

Add any other required environment variables (e.g., `JWT_SECRET`, `STORAGE_ACCOUNT`, etc.).

### 5. Deploy the Backend

#### Option A: Zip Deploy

```bash
zip -r backend.zip backend/
az webapp deployment source config-zip \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --name "$AZURE_BACKEND_APP" \
  --src backend.zip
```

#### Option B: GitHub Actions

```bash
az webapp deployment source config \
  --name "$AZURE_BACKEND_APP" \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --repo-url https://github.com/<org>/<repo> \
  --branch main \
  --manual-integration
```

### 6. Run Database Migrations

Use your application's migration command. Examples:

```bash
# .NET EF Core
dotnet ef database update --connection "$SQL_CONNECTION_STRING"

# Node.js Prisma
npx prisma migrate deploy

# Django
python manage.py migrate
```

If you run migrations in CI/CD, store the commands in a GitHub Actions workflow using `azure/webapps-deploy` and a post-deploy script.

## Frontend Deployment Options

### Option 1: Azure Static Web Apps

1. Create the Static Web App:

    ```bash
    az staticwebapp create \
      --resource-group "$AZURE_RESOURCE_GROUP" \
      --name "$AZURE_FRONTEND_APP" \
      --location "$AZURE_LOCATION" \
      --source https://github.com/<org>/<repo> \
      --branch main \
      --app-location frontend \
      --output-location dist
    ```

    Adjust `--app-location` and `--output-location` for your framework.

2. The command provisions a GitHub Actions workflow. Review `.github/workflows/azure-static-web-apps.yml` and ensure build commands (e.g., `npm install`, `npm run build`) and environment variables are correct.

3. Configure API routes (if any) via the workflow file or `staticwebapp.config.json`.

### Option 2: Azure App Service (for SSR or hosted SPA)

1. Reuse the App Service plan or create a new one.

    ```bash
    az webapp create \
      --resource-group "$AZURE_RESOURCE_GROUP" \
      --plan "$AZURE_APP_SERVICE_PLAN" \
      --name "$AZURE_FRONTEND_APP" \
      --runtime "NODE:18LTS"
    ```

2. Set environment variables for API base URLs or feature flags:

    ```bash
    az webapp config appsettings set \
      --resource-group "$AZURE_RESOURCE_GROUP" \
      --name "$AZURE_FRONTEND_APP" \
      --settings VITE_API_BASE_URL="https://$AZURE_BACKEND_APP.azurewebsites.net"
    ```

3. Deploy using zip deploy or GitHub Actions similarly to the backend.

## Managing Secrets and Configuration

- Store secrets in Azure Key Vault and reference them using App Service managed identity.
- For GitHub Actions, add secrets under the repository settings (`AZURE_WEBAPP_PUBLISH_PROFILE`, `AZURE_STATIC_WEB_APPS_API_TOKEN`, connection strings, etc.).
- Ensure the connection string is updated when rotating SQL passwords.

## Switching to Alternative Data Providers

The backend should abstract data access through repositories or ORM configuration. To support alternative providers:

1. Identify data access code (e.g., Entity Framework `DbContext`, Prisma schema, Django `DATABASES`).
2. Externalize provider configuration via environment variables (e.g., `DATABASE_PROVIDER=SqlServer`).
3. Document connection string formats for new providers (e.g., PostgreSQL, MySQL).
4. Create migration scripts compatible with the provider (EF Core migrations per provider, Prisma datasource configurations).
5. Update CI/CD pipelines to run provider-specific migrations and tests.

When adding a new provider, document:

- Required drivers or SDKs.
- Connection string and secret management.
- Migration tooling commands.
- Rollback strategy (backups, point-in-time restore).

## Maintenance Checklist

- Monitor App Service health via Azure Monitor and Application Insights.
- Schedule automated backups for Azure SQL (`az sql db ltr-policy set`).
- Regularly review GitHub Actions workflows and Azure deployments for stale credentials.
- Run load tests and update SKU tiers (`az appservice plan update --sku S1`) as needed.

## Additional Resources

- [Azure App Service Documentation](https://learn.microsoft.com/azure/app-service/)
- [Azure Static Web Apps Documentation](https://learn.microsoft.com/azure/static-web-apps/)
- [Azure SQL Database Documentation](https://learn.microsoft.com/azure/azure-sql/database/)
- [Azure CLI Reference](https://learn.microsoft.com/cli/azure/)
