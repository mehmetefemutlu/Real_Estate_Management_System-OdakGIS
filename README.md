# Real Estate Management System - OdakGIS

Full project repository containing the ASP.NET Core backend, Angular frontend,
PostgreSQL/PostGIS-related application data, Docker setup, migrations, and project documents.

## Structure

- `srs/`: .NET 10 API, services, repositories, migrations, and seed data.
- `srs_frontend/`: Angular 22 application with OpenLayers mapping.
- `guide_documents/`: requirements and project guides.
- `docker-compose.yml`: PostgreSQL 16, pgAdmin, and API services.

## Configuration

Actual local configuration files and credentials are excluded from Git.
Copy `.env.example` to `.env`, set database and pgAdmin passwords, and generate
an independent random JWT signing key of at least 32 bytes.
Never commit `.env`, local `appsettings*.json`, or signing keys.

Copy each `srs/appsettings*.json.example` file to the matching name without
`.example`. For example, copy `srs/appsettings.json.example` to
`srs/appsettings.json`. Set the placeholders for local development.
The existing developer configuration files remain untouched locally.

## Run with Docker

Install Docker with Compose. After preparing the configuration above, run from
the repository root:

```sh
docker compose -f docker-compose.yml up --build
```

The API is available on port 8080, PostgreSQL on 5432, and pgAdmin on 5050.
The API applies its EF Core migrations and seeds administrative data on startup.
The explicit Compose filename avoids loading the Visual Studio-specific override.
JWT and database credentials are supplied to the API through environment variables.

Run the frontend separately with a Node.js version supported by Angular 22:

```sh
cd srs_frontend
npm ci
npm start
```

Open http://localhost:4200. Its API URL is configured in
`srs_frontend/src/environment.ts` and defaults to http://localhost:8080/api.

## Run the API without Docker

Install the .NET 10 SDK and PostgreSQL, configure the local appsettings files,
then run:

```sh
dotnet run --project srs/srs.csproj
```

For this mode, `.env` is not loaded automatically: configure `Jwt:Key` and the
connection string in local appsettings, .NET user secrets, or environment variables.

## Repository layout

This repository stores frontend source as ordinary files, not a Git submodule.
The existing local frontend Git repository and its remote are preserved.
The new repository starts with an independent snapshot of the full project.
Dependency folders, build outputs, editor caches, and local credentials are excluded.
