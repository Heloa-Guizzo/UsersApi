# UsersAPI

## Overview

The UsersAPI is responsible for user management and authentication within the Gaming Platform ecosystem.

This microservice provides endpoints for user registration, authentication, and administration. It acts as the entry point for all user-related operations and publishes events when significant business actions occur.

---

## Responsibilities

- User registration
- User authentication
- JWT token generation
- User data management
- User administration
- Publishing user-related events

---

## Technologies

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- RabbitMQ
- MassTransit
- JWT Authentication

---

## Architecture Role

This service is responsible for managing platform users and broadcasting user creation events to other services.

When a new user is created, a `UserCreatedEvent` is published to RabbitMQ.

Other microservices can subscribe to this event without creating direct dependencies on UsersAPI.

---

## Event-Driven Communication

### Published Events

#### UserCreatedEvent

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

---

## Main Endpoints

### Create User

```http
POST /users
```

Creates a new user and publishes a UserCreatedEvent.

---

### Authenticate User

```http
POST /auth/login
```

Returns a JWT token for authenticated users.

---

### Get Users

```http
GET /users
```

Returns all registered users.

---

## Environment Configuration

The following configuration values are required:

```text
ConnectionStrings__DefaultConnection
Jwt__Issuer
Jwt__Key
```

---

## Running the Service

### Using Visual Studio

Set UsersAPI as startup project and execute.

---

### Using CLI

```bash
dotnet restore
dotnet build
dotnet run
```

---

## Swagger

```text
http://localhost:5001/swagger
```

---

## Dependencies

- PostgreSQL
- RabbitMQ

---

## Author

FIAP Tech Challenge – Phase 2
