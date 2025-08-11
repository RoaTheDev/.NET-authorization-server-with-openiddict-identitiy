# Enterprise Authentication Server

## Project Structure
```
AuthServer/
│   ├── AuthServer.Domain/
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── Role.cs
│   │   │   ├── Permission.cs
│   │   │   ├── RefreshToken.cs
│   │   │   └── AuditLog.cs
│   │   ├── ValueObjects/
│   │   │   ├── Email.cs
│   │   │   └── UserId.cs
│   │   ├── Events/
│   │   │   ├── UserCreatedEvent.cs
│   │   │   └── UserLoggedInEvent.cs
│   │   └── Interfaces/
│   │       ├── IUserRepository.cs
│   │       └── IAuthenticationService.cs
│   ├── AuthServer.Application/
│   │   ├── Commands/
│   │   │   ├── Authentication/
│   │   │   │   ├── LoginCommand.cs
│   │   │   │   ├── RefreshTokenCommand.cs
│   │   │   │   └── LogoutCommand.cs
│   │   │   └── Users/
│   │   │       ├── CreateUserCommand.cs
│   │   │       └── UpdateUserCommand.cs
│   │   ├── Queries/
│   │   │   ├── GetUserQuery.cs
│   │   │   └── GetUserPermissionsQuery.cs
│   │   ├── Handlers/
│   │   ├── DTOs/
│   │   ├── Validators/
│   │   ├── Behaviors/
│   │   │   ├── LoggingBehavior.cs
│   │   │   ├── ValidationBehavior.cs
│   │   │   └── CachingBehavior.cs
│   │   └── Services/
│   │       ├── TokenService.cs
│   │       └── PasswordService.cs
│   ├── AuthServer.Infrastructure/
│   │   ├── Data/
│   │   │   ├── AuthDbContext.cs
│   │   │   └── Repositories/
│   │   ├── Identity/
│   │   │   ├── IdentityService.cs
│   │   │   └── JwtTokenGenerator.cs
│   │   ├── External/
│   │   │   ├── EmailService.cs
│   │   │   └── SmsService.cs
│   │   └── Configuration/
│   └── AuthServer.WebApi/
│       ├── Controllers/
│       ├── Middleware/
│       ├── Configuration/
│       └── Program.cs
└── docker-compose.yml
```

## Key Technologies & Patterns
- **.NET 8** with minimal APIs and controllers
- **OpenIdDict** for OAuth2/OpenID Connect
- **MediatR** for CQRS implementation
- **FluentValidation** for request validation
- **Entity Framework Core** with PostgreSQL
- **Redis** for caching and rate limiting
- **Serilog** for structured logging
- **Polly** for resilience patterns
- **AutoMapper** for object mapping
- **xUnit** for testing