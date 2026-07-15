
# UsersAPI

## Overview

The UsersAPI is responsible for user management and authentication within the gaming platform.

## Responsibilities

- User registration
- User authentication
- JWT token generation
- User administration

## Technologies

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- RabbitMQ
- MassTransit
- JWT Authentication

## Main Endpoints

### Register User

POST `/users`

### Login

POST `/auth/login`

### List Users

GET `/users`

## Published Events

### UserCreatedEvent

Triggered when a new user is successfully created.

## Environment Variables

```text
ConnectionStrings__DefaultConnection
Jwt__Issuer
Jwt__Key
