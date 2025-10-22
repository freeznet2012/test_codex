using System;
using backend.Data;
using backend.Repositories;
using backend.Repositories.AzureSql;
using backend.Repositories.Supabase;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var provider = builder.Configuration["DataProviders:Active"] ?? "AzureSql";

switch (provider)
{
    case "AzureSql":
        var connectionString = builder.Configuration.GetConnectionString("AzureSql")
            ?? builder.Configuration["DataProviders:AzureSql:ConnectionString"]
            ?? throw new InvalidOperationException("Azure SQL connection string is not configured.");

        builder.Services.AddDbContext<NotesDbContext>(options =>
            options.UseSqlServer(connectionString));
        builder.Services.AddScoped<INoteRepository, AzureSqlNoteRepository>();
        break;
    case "Supabase":
        builder.Services.AddSingleton<INoteRepository, SupabaseNoteRepository>();
        break;
    default:
        throw new InvalidOperationException($"Unsupported data provider '{provider}'.");
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
