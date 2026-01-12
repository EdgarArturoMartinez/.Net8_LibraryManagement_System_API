# 📚 Library API Service - Clean Architecture

## 🎯 Project Overview

A comprehensive **Library Management System** REST API built with **.NET 8**, implementing **Clean Architecture** principles and best practices.

## 🏗️ Architecture

This project follows a **layered architecture** pattern:

```
LibraryAPI/
├── Models/              # Domain Entities
│   ├── Book.cs
│   ├── Author.cs
│   ├── Member.cs
│   └── Loan.cs
├── DTOs/                # Data Transfer Objects
│   ├── BookDtos.cs
│   ├── AuthorDtos.cs
│   ├── MemberDtos.cs
│   └── LoanDtos.cs
├── Data/                # Database Context
│   └── LibraryDbContext.cs
├── Repositories/        # Data Access Layer
│   ├── IRepository.cs   (Generic)
│   ├── Repository.cs    (Generic Implementation)
│   ├── IBookRepository.cs
│   ├── BookRepository.cs
│   └── ... (others)
├── Services/            # Business Logic Layer
│   ├── IBookService.cs
│   ├── BookService.cs
│   └── ... (others)
└── Controllers/         # API Endpoints
    ├── BooksController.cs
    └── ... (others)
```

## 📋 Layers Explained

### 1. **Models (Domain Layer)**
- Core business entities
- No dependencies on other layers
- Contains domain logic (calculated properties, validation rules)

**Example:**
```csharp
public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int AvailableCopies { get; set; }
    // Relationships
    public Author Author { get; set; }
}
```

### 2. **DTOs (Data Transfer Objects)**
- Used for API requests/responses
- Keeps internal models separate from API contracts
- Includes validation attributes

**Example:**
```csharp
public class CreateBookDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; }
    
    [Required]
    public int AuthorId { get; set; }
}
```

### 3. **Repository Pattern (Data Access)**
- Abstracts database operations
- Generic repository for common CRUD
- Specific repositories for entity-specific queries

**Benefits:**
- ✅ Testable (can mock repositories)
- ✅ Centralized data access logic
- ✅ Easier to switch databases

**Example:**
```csharp
public interface IBookRepository : IRepository<Book>
{
    Task<IEnumerable<Book>> GetAvailableBooksAsync();
    Task<Book?> GetBookByISBNAsync(string isbn);
}
```

### 4. **Service Layer (Business Logic)**
- Implements business rules
- Coordinates between repositories
- Maps entities to DTOs

**Example:**
```csharp
public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    
    public async Task<BookDto> CreateBookAsync(CreateBookDto dto)
    {
        // Business logic here
        // Validation, mapping, etc.
    }
}
```

### 5. **Controllers (Presentation Layer)**
- HTTP endpoints
- Handles requests/responses
- Delegates to services
- Returns appropriate HTTP status codes

**Example:**
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<BookDto>> GetById(int id)
{
    var book = await _bookService.GetBookByIdAsync(id);
    return book != null ? Ok(book) : NotFound();
}
```

## 🔧 Technology Stack

- **.NET 8** - Latest framework
- **Entity Framework Core 8** - ORM
- **SQL Server** - Database
- **Swagger/OpenAPI** - API documentation
- **Dependency Injection** - Built-in DI container

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server or LocalDB
- Visual Studio Code or Visual Studio 2022

### Installation

1. **Restore packages:**
```powershell
dotnet restore
```

2. **Update connection string in `appsettings.json`:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LibraryDB;Trusted_Connection=true"
}
```

3. **Create database and apply migrations:**
```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

4. **Run the application:**
```powershell
dotnet run
```

5. **Access Swagger UI:**
```
https://localhost:5001/swagger
```

## 📡 API Endpoints

### Books
- `GET /api/books` - Get all books
- `GET /api/books/{id}` - Get book by ID
- `GET /api/books/isbn/{isbn}` - Get book by ISBN
- `GET /api/books/author/{authorId}` - Get books by author
- `GET /api/books/available` - Get available books
- `POST /api/books` - Create new book
- `PUT /api/books/{id}` - Update book
- `DELETE /api/books/{id}` - Delete book

### (Authors, Members, Loans - Similar structure)

## 🎨 Design Patterns Used

### 1. **Repository Pattern**
- Separates data access from business logic
- Provides abstraction over data storage

### 2. **Dependency Injection**
- Loose coupling between layers
- Easier testing and maintenance
- Configured in `Program.cs`:
```csharp
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBookService, BookService>();
```

### 3. **Service Layer Pattern**
- Encapsulates business logic
- Prevents controllers from becoming too complex

### 4. **DTO Pattern**
- Separates internal models from API contracts
- Provides validation layer

## 🧪 Benefits of This Architecture

### Testability
```csharp
// Easy to mock dependencies
var mockRepo = new Mock<IBookRepository>();
var service = new BookService(mockRepo.Object);
```

### Maintainability
- Each layer has single responsibility
- Changes in one layer don't affect others
- Easy to locate and fix bugs

### Scalability
- Can add caching layer without changing services
- Can swap SQL Server for MongoDB
- Can add authentication/authorization easily

### Reusability
- Services can be used by multiple controllers
- Repositories can be shared across services
- DTOs can be reused in different contexts

## 📚 Learning Resources

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Repository Pattern](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)

## 🎓 Interview Talking Points

When explaining this architecture in an interview:

1. **Separation of Concerns**: "Each layer has a specific responsibility"
2. **Dependency Inversion**: "We depend on abstractions (interfaces), not concrete implementations"
3. **Testability**: "We can easily unit test each layer independently"
4. **Maintainability**: "Changes in the database don't affect business logic"
5. **Scalability**: "We can add features without modifying existing code"

## 🛠️ Next Steps

- [ ] Add authentication/authorization
- [ ] Implement unit tests
- [ ] Add logging with Serilog
- [ ] Implement pagination
- [ ] Add caching with Redis
- [ ] Create Docker container
- [ ] Set up CI/CD pipeline

## 👤 Author

**Arturo**  
Software Developer with 10+ years in .NET

---

**Note**: This is a production-ready template demonstrating best practices for ASP.NET Core Web API development with Clean Architecture.
