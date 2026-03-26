# Lensmania 📸

**Photo sharing platform (Blazor WebAssembly + ASP.NET Core + PostgreSQL)**

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)
![Blazor WASM](https://img.shields.io/badge/Blazor-WebAssembly-5C2D91)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-0C8CE9)
![EF Core](https://img.shields.io/badge/EF%20Core-PostgreSQL-3C873A)
![JWT](https://img.shields.io/badge/Auth-JWT-000000)

Lensmania is a .NET solution built around a **Blazor WebAssembly client** and an **ASP.NET Core Web API** backed by **PostgreSQL**. It currently includes a basic posts feed, database seeding for development, and a JWT-based authentication API (register/login) designed to unlock protected endpoints (e.g. `/api/user/me`).

## Overview

This repository is organized as a multi-project .NET solution:

- **LensmaniaClient**: Blazor WebAssembly frontend (runs on `http://localhost:5135`)
- **LensmaniaServer**: ASP.NET Core Web API + EF Core + JWT auth (runs on `http://localhost:5078`)
- **LensmaniaLibrary**: shared models (e.g. `Post`) referenced by client and server
- **LensmaniaTests**: test project (currently minimal)

## Key Features

- **Posts feed**: fetches posts from the API (`GET /api/posts`) and displays them in a masonry grid (client-side).
- **JWT Authentication API**:
  - Register: `POST /api/auth/register`
  - Login: `POST /api/auth/login`
  - Protected “me” endpoint: `GET /api/user/me` (requires `Authorization: Bearer <token>`)
- **PostgreSQL persistence** with EF Core.
- **Development seeding**: the server seeds posts at startup.
- **CORS configured** to allow the Blazor dev origin (`http://localhost:5135`).

## Architecture

### Technology Stack

**Frontend (LensmaniaClient)**

- Blazor WebAssembly (`net10.0`)
- `HttpClient` configured to call the API at `http://localhost:5078/`
- Masonry layout via `Soenneker.Blazor.Masonry`

**Backend (LensmaniaServer)**

- ASP.NET Core Web API (`net10.0`)
- EF Core + `Npgsql` provider for PostgreSQL
- JWT Bearer authentication (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- Password hashing via `BCrypt.Net-Next`
- OpenAPI enabled in development

**Shared library (LensmaniaLibrary)**

- Shared DTOs/models used on both sides (e.g. `LensmaniaLibrary.Models.Post`)

## Project Structure

```
Lensmania/
├── Lensmania.sln
├── LensmaniaClient/                # Blazor WebAssembly frontend
│   ├── Layout/                     # Main layout + navigation
│   ├── Pages/                      # Razor pages (Home, Posts, Weather, ...)
│   ├── wwwroot/                    # Static assets
│   ├── Program.cs                  # DI + HttpClient base address
│   └── LensmaniaClient.csproj
├── LensmaniaServer/                # ASP.NET Core Web API backend
│   ├── Controllers/                # Auth, User, Health controllers
│   ├── Features/Posts/             # Posts feature (controller + service)
│   ├── Database/                   # EF Core DbContext
│   ├── Models/                     # User + auth request/response models
│   ├── Services/                   # AuthService + TokenService
│   ├── Migrations/                 # EF Core migrations
│   ├── SeederPost.cs               # Development seed
│   ├── appsettings.json            # Local settings (DB + JWT)
│   ├── appsettings.example.json    # Example settings (safe template)
│   ├── Program.cs                  # HTTP pipeline + auth + CORS + seeding
│   └── LensmaniaServer.csproj
├── LensmaniaLibrary/               # Shared library (models)
│   ├── Models/
│   └── LensmaniaLibrary.csproj
├── LensmaniaTests/                 # Tests (minimal scaffold)
│   └── LensmaniaTests.csproj
└── docs/
    └── use-cases/
        └── lensmania_use_case.drawio.png
```

## Getting Started

### Prerequisites

- **.NET SDK 10.0** (to match `net10.0` targets)
- **PostgreSQL** (local instance)

### Configuration

The server reads its configuration from `LensmaniaServer/appsettings.json`.

- **Database**
  - `ConnectionStrings:DefaultConnection`
- **JWT**
  - `Jwt:Key`
  - `Jwt:Issuer`
  - `Jwt:Audience`

You can use `LensmaniaServer/appsettings.example.json` as a template.

### Run the backend API

From the repository root:

```bash
dotnet restore
dotnet run --project LensmaniaServer
```

Default dev URLs (from launch settings):

- API: `http://localhost:5078`

### Run the Blazor WebAssembly client

In another terminal:

```bash
dotnet run --project LensmaniaClient
```

Default dev URL:

- Client: `http://localhost:5135`

## API Endpoints

### Health

- `GET /api/health` → `{ status, timestamp }`

### Posts

- `GET /api/posts` → list of posts

### Authentication

- `POST /api/auth/register`

Request:

```json
{
  "username": "alice",
  "email": "alice@example.com",
  "password": "your-password"
}
```

Response:

```json
{
  "token": "<jwt>",
  "username": "alice",
  "isAdmin": false,
  "isPremium": false
}
```

- `POST /api/auth/login`

Request:

```json
{
  "email": "alice@example.com",
  "password": "your-password"
}
```

Response: same as register.

### User (Protected)

- `GET /api/user/me` (requires `Authorization: Bearer <jwt>`)

## OpenAPI (Development)

In development, the server enables OpenAPI via `app.MapOpenApi()`.
Depending on your .NET OpenAPI configuration, the document is typically served at:

- `GET /openapi/v1.json`

## Tests

```bash
dotnet test
```

## Documentation

- Use cases diagram: `docs/use-cases/lensmania_use_case.drawio.png`

## Author

**kenant68**
**sophie-rud**

- GitHub: [@kenant68](https://github.com/kenant68)
- GitHub: [@kenant68](https://github.com/sophie-rud)

**Note**: This project is under active development and some features (notably full frontend auth integration) are in progress.