# UsersAPI

## Overview
The **UsersAPI** is responsible for user management, role-based access control, and authentication within the Gaming Platform ecosystem.

This microservice provides endpoints for user registration, authentication via JWT, session/read caching, and administration. It acts as the entry point for all user-related operations and publishes domain events when significant business actions occur.

## Responsibilities
- User registration and profile management
- User authentication and JWT token generation
- User administration and role validation
- Distributed caching with Redis
- Publishing user-related events asynchronously via RabbitMQ

## Technologies
- **.NET 8** / ASP.NET Core Web API
- **Entity Framework Core** & **PostgreSQL** (Relational database)
- **Redis** (`StackExchange.Redis` — Distributed cache)
- **RabbitMQ** & **MassTransit** (Event-driven messaging)
- **JWT Authentication** (Bearer token security)
- **Kubernetes (K3s)** & **GitHub Actions** (CI/CD with Amazon ECR)

## Architecture Role
This service manages platform users, validates credentials, issues JWT tokens consumed across the microservices ecosystem, and broadcasts user creation events to downstream services.

When a new user is registered, a `UserCreatedEvent` is published to RabbitMQ so other microservices (such as `NotificationsAPI`) can react asynchronously without direct coupling.

## Event-Driven Communication

### Published Events

#### `UserCreatedEvent`
Published whenever a new user is successfully registered.

Example:
```json
{
  "userId": "guid",
  "email": "user@example.com"
}
```

### Event Flow
```text
UsersAPI
    │
    ▼
UserCreatedEvent
    │
    ▼
NotificationsAPI
```

## Main Endpoints

### Create User
- **Endpoint:** `POST /users`
- Creates a new user in PostgreSQL and publishes a `UserCreatedEvent` to RabbitMQ.

### Authenticate User
- **Endpoint:** `POST /auth/login`
- Validates user credentials and returns a signed JWT token for authenticated requests across the platform.

### Get Users
- **Endpoint:** `GET /users`
- Returns registered users (leveraging distributed caching where applicable).

## Environment Configuration (Zero Hardcoded Credentials)
No secrets or credentials are hardcoded in `appsettings.json`. All sensitive configuration values are injected at runtime via environment variables (Kubernetes `Secrets` and `ConfigMaps`):

- `ConnectionStrings__DefaultConnection`
- `Redis__ConnectionString`
- `RabbitMq__Host`
- `RabbitMq__Username`
- `RabbitMq__Password`
- `Jwt__Key`
- `Jwt__Issuer`

## Running the Service

### Using Visual Studio
Set `UsersAPI` as the startup project and run using the `http` or `https` launch profile.

### Using CLI
```bash
dotnet restore
dotnet build
dotnet run
```

### Swagger UI
- **Local:** `http://localhost:5001/swagger`
- **Kubernetes Cluster (via Ingress):** `http://<EC2_PUBLIC_IP>/users/swagger/index.html`

## Dependencies
- PostgreSQL
- Redis
- RabbitMQ
