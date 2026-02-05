# Arslan Project Manager

A full-stack **team-based project and task management** application built with .NET 9. It supports **web** (ASP.NET Core MVC) and **mobile** (MAUI) clients that consume a shared REST API with JWT authentication and role-based permissions.

---

## Features

- **Teams** – Create teams, invite members, manage team roles and permissions
- **Projects** – Projects belong to teams; track name, detail, and start date
- **Tasks** – Tasks with categories (User Story, Task, Issue, Bug, Ticket), priorities (Low/Medium/High), board status (To Do, In Progress, Done), assignees, and due dates
- **Roles & permissions** – System roles (Manager, Member, Viewer, Guest) and custom team roles with granular permissions (view/edit/delete tasks and projects, invite/remove members, manage roles and permissions)
- **Team invites** – Invite users by email; accept/decline from Web or Mobile
- **Task comments & activity logs** – Comments and log categories (Created, Updated, Assigned, Status Changed, etc.)
- **User profiles** – Name, email, profile picture, change password
- **Authentication** – Register, login, JWT access + refresh tokens, cookie support for Web UI

---

## Architecture

The solution uses a **layered architecture**:

| Project | Description |
|---------|-------------|
| **ArslanProjectManager.Core** | Domain models, DTOs, repository/service interfaces, constants. No external dependencies beyond EF and configuration. |
| **ArslanProjectManager.Repository** | EF Core `DbContext`, entity configurations, migrations, concrete repositories, `UnitOfWork`, database seeder (board tags, log categories, task categories, system roles). |
| **ArslanProjectManager.Service** | Business logic: auth, users, teams, team users, roles, projects, tasks, comments, logs, tags, tokens. Uses AutoMapper, FluentValidation, BCrypt. |
| **ArslanProjectManager.API** | ASP.NET Core Web API (.NET 9): JWT auth, controllers (Auth, User, Teams, Projects, Tasks, Home), OpenAPI/Swagger, Scalar, ReDoc, CORS, custom exception and token-expiration middleware. |
| **ArslanProjectManager.WebUI** | ASP.NET Core MVC app: cookie + JWT, calls API via `HttpClient` with `AuthenticatedHttpClientHandler` and token refresh. Views for Login, Register, Teams, Projects, Tasks, User profile, permissions, roles. |
| **ArslanProjectManager.MobileUI** | .NET MAUI app (Android, iOS, Windows): CommunityToolkit.Mvvm, same API via HTTP + JWT; pages for auth, home, teams, projects, tasks, profile, invites. |

**Data flow:** WebUI and MobileUI → **API** → **Service** → **Repository** → **SQL Server** (Core used by Repository and Service; Mobile references Core + Service for DTOs and shared types).

---

## Tech Stack

- **.NET 9**
- **ASP.NET Core** (Web API + MVC)
- **Entity Framework Core 9** + **SQL Server**
- **JWT** (access + refresh tokens), **BCrypt** for passwords
- **AutoMapper**, **FluentValidation**, **Autofac** (API)
- **OpenAPI / Swagger**, **Scalar**, **ReDoc** (API docs)
- **.NET MAUI** (Android, iOS, Windows)
- **CommunityToolkit.Maui**, **CommunityToolkit.Mvvm** (Mobile)

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- **SQL Server** (LocalDB, Express, or full) for the API and WebUI
- For **Mobile**: Visual Studio 2022 with MAUI workload (or VS Code + .NET MAUI extension); Android SDK / Xcode / Windows SDK as needed per platform

---

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/your-username/ArslanProjectManager.git
cd ArslanProjectManager
```

### 2. Configure the API

- Open `ArslanProjectManager.API/appsettings.json` (or `appsettings.Development.json` for local overrides).
- Set the connection string and JWT settings (do **not** commit real secrets; use User Secrets or environment variables in production).

Example structure (replace with your values):

```json
{
  "ConnectionStrings": {
    "SqlConnection": "Server=YOUR_SERVER;Database=ProjectManagerDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=true;"
  },
  "Jwt": {
    "Issuer": "ArslanProjectManager",
    "Audience": "ArslanProjectManagerUser",
    "SecurityKey": "YOUR_SECRET_KEY_AT_LEAST_32_BYTES_FOR_HMAC_SHA256",
    "ExpirationInMinutes": 60
  }
}
```

The API signs JWTs with **HMAC-SHA256** (see `TokenHandler`); the key is used as UTF-8 bytes, so use a secret that is **at least 32 bytes** (256 bits). If the key is ASCII-only, that means at least 32 characters.

- For **development**, you can use LocalDB in `appsettings.Development.json`:

```json
"ConnectionStrings": {
  "SqlConnection": "Server=(LocalDB)\\MSSQLLocalDB;Database=ProjectManagerDB;Integrated Security=True;"
}
```

### 3. Apply database migrations

From the solution directory:

```bash
dotnet ef database update --project ArslanProjectManager.Repository --startup-project ArslanProjectManager.API
```

To add a new migration:

```bash
dotnet ef migrations add YourMigrationName --context ProjectManagerDbContext --project ArslanProjectManager.Repository --startup-project ArslanProjectManager.API
```

On first run, the **database seeder** (invoked in `Program.cs`) will populate board tags, log categories, task categories, and system roles if the tables are empty.

### 4. Run the API

```bash
dotnet run --project ArslanProjectManager.API
```

- API: typically `https://localhost:7xxx` (check console or `Properties/launchSettings.json`).
- **API docs:** `/api-docs` (Swagger UI), `/api-docs/redoc` (ReDoc), Scalar at `/scalar/v1`.

### 5. Run the Web UI

- Set `ArslanProjectManagerAPI:BaseUrl` in `ArslanProjectManager.WebUI/appsettings.json` to your API base URL (e.g. `https://localhost:7069/api/`).
- JWT `Issuer`, `Audience`, and `SecurityKey` must match the API so cookie-based auth works.

```bash
dotnet run --project ArslanProjectManager.WebUI
```

Open the URL shown (e.g. `https://localhost:7xxx`) and use Login/Register.

### 6. Run the Mobile app (optional)

- In `ArslanProjectManager.MobileUI`, set the API base URL in your config (e.g. in a constants file or `MauiProgram` depending on your setup) to point to your API (use your machine’s IP for an emulator/device, not `localhost`).
- Build and run for your target platform (e.g. Android emulator, Windows).

```bash
dotnet build -f net9.0-android   # or net9.0-ios, net9.0-windows10.0.19041.0
```

---

## Solution structure (summary)

```
ArslanProjectManager.sln
├── ArslanProjectManager.Core/          # Models, DTOs, interfaces, constants
├── ArslanProjectManager.Repository/    # DbContext, EF configs, migrations, repositories, seeder
├── ArslanProjectManager.Service/       # Business logic, validation, token handling
├── ArslanProjectManager.API/           # REST API, JWT, Swagger/Scalar/ReDoc
├── ArslanProjectManager.WebUI/         # MVC web app (consumes API)
└── ArslanProjectManager.MobileUI/      # MAUI app (consumes API)
```

---

## API overview

|   Area   |     Controllers      | Notes                                          |
|----------|----------------------|------------------------------------------------|
| Auth     | `AuthController`     | Register, login, refresh token, logout         |
| User     | `UserController`     | Profile, change password                       |
| Teams    | `TeamsController`    | CRUD, invites, members, roles, permissions     |
| Projects | `ProjectsController` | CRUD, scoped to teams                          |
| Tasks    | `TasksController`    | CRUD, comments, logs, assignment, board status |
| Home     | `HomeController`     | Dashboard-style data for the current user      |

Authentication: **Bearer** token in `Authorization` header or **cookie** `AccessToken` (e.g. Web UI). Use the **refresh token** endpoint when the access token expires.

---

## Configuration notes

- **Secrets**: Prefer **User Secrets** (development) and **environment variables** or a secure vault (production) for connection strings and JWT keys. Do not commit `appsettings.json` with real credentials to source control.

---

## License

This project is provided as-is. Adjust license and attribution as needed for your use.
