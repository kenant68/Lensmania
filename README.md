# Lensmania 📸

**Photo sharing & photo-contest platform (Blazor WebAssembly + ASP.NET Core + PostgreSQL)**

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)
![Blazor WASM](https://img.shields.io/badge/Blazor-WebAssembly-5C2D91)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-0C8CE9)
![EF Core](https://img.shields.io/badge/EF%20Core-PostgreSQL-3C873A)
![JWT](https://img.shields.io/badge/Auth-JWT-000000)
![Google OAuth](https://img.shields.io/badge/Auth-Google%20OAuth-4285F4)

Lensmania is a .NET solution built around a **Blazor WebAssembly client** and an **ASP.NET Core Web API** backed by **PostgreSQL**. It lets users share photos in a feed, like posts, and take part in **themed photo contests (events)** that are automatically closed and awarded a winner. It ships with JWT and **Google OAuth** authentication, a **password-reset flow** over e-mail, file uploads, an **admin area** for managing users and events, and a **badge / rewards** model.

## Overview

This repository is organized as a multi-project .NET solution:

- **LensmaniaClient**: Blazor WebAssembly frontend (runs on `http://localhost:5135`)
- **LensmaniaServer**: ASP.NET Core Web API + EF Core + JWT/Google auth (runs on `http://localhost:5078`)
- **LensmaniaLibrary**: shared DTOs and enums referenced by client and server
- **LensmaniaTests**: test project

## Key Features

- **Posts feed**: paginated, sortable posts (`GET /api/posts`) displayed in a masonry grid, with cursor/offset pagination and filtering by user or event.
- **Likes**: authenticated users can like posts (`POST /api/posts/{id}/likes`).
- **Photo upload**: authenticated users upload post photos and badge images (`POST /api/uploads/photo`, `POST /api/uploads/badge`).
- **Events (photo contests)**: themed contests with a date range, optional cover photo, and `active` / `past` status. A background service automatically closes finished events and designates a winner.
- **Themes**: a catalog of contest themes (`GET /api/themes`).
- **Badges & rewards**: badge model and per-user earnings (`Badge` / `Earn`) awarded through events.
- **Authentication**:
  - Local register / login with JWT (`POST /api/auth/register`, `POST /api/auth/login`)
  - **Google OAuth** sign-in (`POST /api/auth/google`)
  - **Password reset** by e-mail (`POST /api/auth/forgot-password`, `POST /api/auth/reset-password`)
- **User profiles**: public profiles, self profile management (update / delete), and admin user management.
- **Admin area**: role-based (`AdminOnly` policy) management of events and users (activate/deactivate, delete).
- **PostgreSQL persistence** with EF Core migrations.
- **Development seeding**: the server seeds posts and themes at startup.
- **CORS configured** to allow the Blazor dev origin (`http://localhost:5135`).

## Architecture

### Technology Stack

**Frontend (LensmaniaClient)**

- Blazor WebAssembly (`net10.0`)
- `HttpClient` configured to call the API at `http://localhost:5078/`
- Component authorization via `Microsoft.AspNetCore.Components.Authorization`
- Masonry layout via `Soenneker.Blazor.Masonry`

**Backend (LensmaniaServer)**

- ASP.NET Core Web API (`net10.0`)
- EF Core + `Npgsql` provider for PostgreSQL
- JWT Bearer authentication (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- Google token validation via `Google.Apis.Auth`
- Password hashing via `BCrypt.Net-Next`
- E-mail sending via `MailKit` (Gmail SMTP)
- HTML sanitization via `HtmlSanitizer`
- Hosted `EventClosingBackgroundService` that closes events and picks winners
- OpenAPI enabled in development

**Shared library (LensmaniaLibrary)**

- Shared DTOs (Posts, Users, Events, Themes, Badges) and enums (`LoginStatus`, `GoogleAuthStatus`, `PostSortOrder`) used on both sides

## Project Structure

```
Lensmania/
├── Lensmania.sln
├── LensmaniaClient/                # Blazor WebAssembly frontend
│   ├── Layout/                     # Main layout + navigation
│   ├── Pages/
│   │   ├── Auth/                   # Login, Register, Forgot/Reset password
│   │   ├── Events/                 # Public events list + detail
│   │   ├── Admin/                  # Events & users management
│   │   ├── Home.razor
│   │   ├── PostsFeedPage.razor
│   │   └── ProfilePage.razor
│   ├── wwwroot/                    # Static assets
│   ├── Program.cs                  # DI + HttpClient base address
│   └── LensmaniaClient.csproj
├── LensmaniaServer/                # ASP.NET Core Web API backend
│   ├── Controllers/                # Auth, User, Posts, Events, Themes, Upload, Health
│   ├── Services/                   # Auth, Google, Posts, Events, Themes, Email,
│   │                               #   PasswordReset, FileStorage, EventClosing, Token
│   ├── Database/                   # EF Core DbContext
│   ├── Models/                     # User, Post, Event, Theme, Badge, Earn, PostLike, ...
│   ├── Migrations/                 # EF Core migrations
│   ├── SeederPost.cs               # Development seed (posts)
│   ├── SeederTheme.cs              # Development seed (themes)
│   ├── appsettings.json            # Local settings (DB, JWT, Email, Google, ...)
│   ├── appsettings.example.json    # Example settings (safe template)
│   ├── Program.cs                  # HTTP pipeline + auth + CORS + seeding + hosted service
│   └── LensmaniaServer.csproj
├── LensmaniaLibrary/               # Shared library (DTOs + enums)
│   ├── DTOs/                       # Posts, Users, Events, Themes, Badges
│   ├── Enums/                      # LoginStatus, GoogleAuthStatus, PostSortOrder
│   └── LensmaniaLibrary.csproj
├── LensmaniaTests/                 # Tests
│   └── LensmaniaTests.csproj
└── docs/
    ├── database/                   # MCD / MLD diagrams
    ├── mockups/                    # UI mockups (home, events, profile, admin)
    └── use-cases/                  # Use-case diagram
```

## Getting Started

### Prerequisites

- **.NET SDK 10.0** (to match `net10.0` targets)
- **PostgreSQL** (local instance)
- *(Optional)* a **Google OAuth client ID** for Google sign-in
- *(Optional)* a **Gmail account + app password** for the password-reset e-mails

### Configuration

The server reads its configuration from `LensmaniaServer/appsettings.json`. Use `LensmaniaServer/appsettings.example.json` as a template.

| Section | Key(s) | Purpose |
| --- | --- | --- |
| `ConnectionStrings` | `DefaultConnection` | PostgreSQL connection string |
| `Jwt` | `Key`, `Issuer`, `Audience`, `ExpirationMinutes` | JWT signing & validation |
| `Email` | `SmtpHost`, `SmtpPort`, `UseStartTls`, `FromAddress`, `FromDisplayName`, `Username`, `Password` | Gmail SMTP for password-reset e-mails |
| `PasswordReset` | `TokenLifetimeMinutes`, `ClientBaseUrl` | Reset-token lifetime + link base URL |
| `EventClosing` | `IntervalSeconds` | Polling interval of the event-closing background service |
| `Google` | `ClientId` | Google OAuth client ID used to validate ID tokens |

> ⚠️ Never commit real secrets to `appsettings.json`. Keep them out of version control and rely on the example file as a reference.

### Run the backend API

From the repository root:

```bash
dotnet restore
dotnet run --project LensmaniaServer
```

Default dev URLs (from launch settings):

- API (HTTP): `http://localhost:5078`
- API (HTTPS): `https://localhost:7274`

### Run the Blazor WebAssembly client

In another terminal:

```bash
dotnet run --project LensmaniaClient
```

Default dev URLs:

- Client (HTTP): `http://localhost:5135`
- Client (HTTPS): `https://localhost:7213`

## API Endpoints

> Endpoints marked 🔒 require `Authorization: Bearer <jwt>`. Endpoints marked 👑 require the **AdminOnly** policy.

### Health

- `GET /api/health` → `{ status, timestamp }`

### Authentication

- `POST /api/auth/register` — create an account, returns a JWT
- `POST /api/auth/login` — log in, returns a JWT
- `POST /api/auth/google` — sign in with a Google ID token
- `POST /api/auth/forgot-password` — request a password-reset e-mail (always returns a generic message)
- `POST /api/auth/reset-password` — reset the password using a token

`POST /api/auth/register` request:

```json
{
  "username": "alice",
  "email": "alice@example.com",
  "password": "your-password"
}
```

Response (also returned by `login` / `google`):

```json
{
  "token": "<jwt>",
  "username": "alice",
  "isAdmin": false,
  "isPremium": false
}
```

### Posts

- `GET /api/posts` — paginated feed. Query params: `userId`, `eventId`, `cursor`, `offset`, `limit` (1–50, default 10), `sort` (`date_desc` *(default)*, `date_asc`, `likes_desc`, `likes_asc`)
- `GET /api/posts/{id}` — single post
- `GET /api/posts/{username}` — posts of a given user
- 🔒 `POST /api/posts` — create a post
- 🔒 `POST /api/posts/{id}/likes` — like a post
- 🔒 `DELETE /api/posts/{id}` — delete own post

### Events

- `GET /api/events` — paginated events. Query params: `offset` (default 0), `limit` (default 10), `status` (`active` | `past`)
- `GET /api/events/{id}` — event detail
- 👑 `POST /api/events` — create an event
- 👑 `PUT /api/events/{id}` — update an event
- 👑 `PUT /api/events/{id}/cover` — set the event cover photo
- 👑 `DELETE /api/events/{id}` — delete an event

### Themes

- `GET /api/themes` — list contest themes
- `GET /api/themes/{id}` — theme detail

### Uploads

- 🔒 `POST /api/uploads/photo` — upload a post photo
- 🔒 `POST /api/uploads/badge` — upload a badge image

### Users

- 🔒 `GET /api/user/me` — current user
- 🔒 `GET /api/user/{username}` — public profile
- 🔒 `PUT /api/user/me` — update own profile
- 🔒 `DELETE /api/user/me` — delete own account
- 👑 `GET /api/user` — paginated user list (admin)
- 👑 `PATCH /api/user/{id}/active` — activate / deactivate a user
- 👑 `DELETE /api/user/{id}` — delete a user

## Database

Schema is managed with EF Core migrations under `LensmaniaServer/Migrations/`. Migrations are applied automatically at startup in development; you can also run them manually:

```bash
dotnet ef database update --project LensmaniaServer
```

The up-to-date relational schema (reflecting the actual EF Core model) is documented in [`docs/database/schema.md`](docs/database/schema.md). Legacy conceptual/logical diagrams are also available under `docs/database/` (`MCD_Lensmania.jpg`, `MLD_Lensmania.jpg`).

## OpenAPI (Development)

In development, the server enables OpenAPI via `app.MapOpenApi()`. The document is typically served at:

- `GET /openapi/v1.json`

## Tests

```bash
dotnet test
```

## Documentation

- Database schema (current): [`docs/database/schema.md`](docs/database/schema.md)
- Legacy database diagrams (MCD / MLD): `docs/database/`
- UI mockups: `docs/mockups/`
- Use-case diagram: `docs/use-cases/lensmania_use_case.drawio.png`

## Authors

**kenant68** & **sophie-rud**

- GitHub: [@kenant68](https://github.com/kenant68)
- GitHub: [@sophie-rud](https://github.com/sophie-rud)

**Note**: This project is under active development; some features are still in progress.
