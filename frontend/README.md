# Frontend

This project contains the Angular frontend for the notes application. It provides note entry, search, and paginated listing experiences backed by the backend API.

## Prerequisites

- Node.js 18 LTS or later
- npm 9 or later

Install dependencies:

```bash
npm install
```

## Development server

Run the app in development mode:

```bash
npm start
```

The application will be available at <http://localhost:4200/>. The development server automatically reloads the page when you change files.

To run with Azure environment settings use:

```bash
npm run start:azure
```

## Build

Generate a production build:

```bash
npm run build
```

The build artifacts are stored in the `dist/frontend` directory.

For an Azure-ready build with Azure API configuration:

```bash
npm run build:azure
```

## Additional scripts

- `npm test` &mdash; run unit tests via Karma.
- `npm run watch` &mdash; rebuild the app in watch mode using the development configuration.
