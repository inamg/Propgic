# Clean Architecture .NET 8 Project

This is a boilerplate project implementing Clean Architecture principles with .NET 8.

## Project Structure

```
CleanArchitecture/
├── src/
│   ├── Domain/              # Enterprise business rules
│   │   ├── Entities/        # Domain entities
│   │   ├── Interfaces/      # Repository and service interfaces
│   │   └── Common/          # Base classes and common types
│   │
│   ├── Application/         # Application business rules
│   │   ├── DTOs/            # Data Transfer Objects
│   │   ├── Interfaces/      # Application service interfaces
│   │   ├── Services/        # Application services implementation
│   │   └── Mappings/        # AutoMapper profiles
│   │
│   ├── Infrastructure/      # External concerns
│   │   ├── Data/            # Database context and configurations
│   │   ├── Repositories/    # Repository implementations
│   │   └── Services/        # External services implementation
│   │
│   └── WebApi/             # API layer
│       ├── Controllers/     # API controllers
│       └── Middleware/      # Custom middleware
│
└── tests/                  # Test projects
```

## Architecture Layers

### 1. Domain Layer
- Contains enterprise business logic
- Defines entities, value objects, and domain events
- Defines repository interfaces (dependency inversion)
- Has no dependencies on other layers

### 2. Application Layer
- Contains application-specific business logic
- Implements use cases and orchestrates data flow
- Depends only on Domain layer
- Defines service interfaces and DTOs

### 3. Infrastructure Layer
- Implements data persistence (EF Core)
- Implements external services
- Depends on Domain and Application layers
- Contains database context and repository implementations

### 4. WebApi Layer
- Entry point for the application
- Contains controllers and middleware
- Depends on Application and Infrastructure layers
- Handles HTTP requests and responses

## Technologies Used

- **.NET 8**: Latest .NET framework
- **Entity Framework Core 8**: ORM for data access
- **AutoMapper**: Object-to-object mapping
- **MediatR**: Mediator pattern implementation (configured but not fully implemented)
- **FluentValidation**: Input validation
- **Swagger/OpenAPI**: API documentation

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server or SQL Server LocalDB

### Installation

1. Clone the repository
2. Update the connection string in `appsettings.json`
3. Run migrations:
```bash
cd src/Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../WebApi
dotnet ef database update --startup-project ../WebApi
```

4. Run the application:
```bash
cd src/WebApi
dotnet run
```

The API will be available at `https://localhost:5001` (or the port specified in launchSettings.json)

### API Endpoints

#### Products
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

## Design Patterns Used

- **Repository Pattern**: Abstracts data access layer
- **Unit of Work Pattern**: Manages transactions
- **Dependency Injection**: Manages dependencies
- **CQRS** (partial): Separation of read and write operations through services
- **Mediator Pattern**: Decouples request/response handling (MediatR ready)

## Best Practices

- ✅ Separation of Concerns
- ✅ Dependency Inversion Principle
- ✅ Single Responsibility Principle
- ✅ Don't Repeat Yourself (DRY)
- ✅ Interface Segregation
- ✅ Global Exception Handling
- ✅ Logging
- ✅ API Versioning Ready
- ✅ Swagger Documentation

## Next Steps

Consider adding:
- Authentication & Authorization (JWT)
- Caching (Redis)
- API Versioning
- Rate Limiting
- Health Checks
- Comprehensive validation with FluentValidation
- Unit and Integration Tests
- Docker support
- CI/CD pipeline

## License

This project is licensed under the MIT License.
