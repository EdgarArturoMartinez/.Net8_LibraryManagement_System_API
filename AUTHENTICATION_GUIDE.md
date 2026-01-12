# JWT Authentication Implementation Guide

## 📋 Interview Explanation - What We Built

This document explains the JWT (JSON Web Token) authentication system added to the Library API. Use this to explain the architecture during technical interviews.

---

## 🎯 Why JWT Authentication?

### The Problem
- Without authentication, **anyone** can access our API endpoints
- We need to know **who** is making requests
- We need to control **what** they can do

### The Solution - JWT
JWT is like a **digital passport** that proves your identity:
- **Login once**, get a token (passport)
- **Send the token** with each request (show your passport)
- **Server validates** the token without hitting the database every time

### JWT vs Sessions
| JWT | Session |
|-----|---------|
| Stateless (server doesn't store tokens) | Stateful (server stores session data) |
| Scalable (works across multiple servers) | Harder to scale |
| Client stores token (localStorage/cookies) | Server stores session ID |
| No database lookup on each request | Database lookup on each request |

---

## 🏗️ Architecture - Components Created

### 1️⃣ User Model (`Models/User.cs`)
**Purpose**: Database entity representing a user account

```csharp
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }           // Unique identifier
    public string PasswordHash { get; set; }    // NEVER store plain passwords!
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
}
```

**Interview Points**:
- ✅ Store password **hashes**, never plain text
- ✅ Email is unique (configured with index in DbContext)
- ✅ Track last login for security auditing
- ✅ `IsActive` flag for soft deletion (don't lose data)

---

### 2️⃣ Authentication DTOs (`DTOs/AuthDtos.cs`)
**Purpose**: Data Transfer Objects for API requests/responses

#### LoginDto
```csharp
public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }
}
```

#### RegisterDto
```csharp
public class RegisterDto
{
    [Required] public string FirstName { get; set; }
    [Required] public string LastName { get; set; }
    [Required][EmailAddress] public string Email { get; set; }
    [Required][StringLength(100, MinimumLength = 6)] public string Password { get; set; }
    [Required][Compare("Password")] public string ConfirmPassword { get; set; }
}
```

#### AuthResponseDto
```csharp
public class AuthResponseDto
{
    public string Token { get; set; }        // JWT token
    public string Email { get; set; }
    public string FullName { get; set; }
    public DateTime ExpiresAt { get; set; }  // When token expires
}
```

**Interview Points**:
- ✅ DTOs protect internal models from being exposed
- ✅ Validation attributes ([Required], [EmailAddress], [Compare])
- ✅ Never return sensitive data (password hash)

---

### 3️⃣ User Repository (`Repositories/UserRepository.cs`)
**Purpose**: Data access layer for User entities

```csharp
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}

public class UserRepository : Repository<User>, IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }
}
```

**Interview Points**:
- ✅ Follows Repository Pattern (consistent with existing code)
- ✅ Custom method `GetByEmailAsync` for login lookup
- ✅ Returns `Task<User?>` (nullable - user might not exist)

---

### 4️⃣ Authentication Service (`Services/AuthService.cs`)
**Purpose**: Business logic for authentication

#### Key Methods

**Login Flow**:
```csharp
public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
{
    // 1. Find user by email
    var user = await _userRepository.GetByEmailAsync(loginDto.Email);
    if (user == null || !user.IsActive) return null;

    // 2. Verify password
    if (!VerifyPassword(loginDto.Password, user.PasswordHash))
        return null;

    // 3. Update last login
    user.LastLoginAt = DateTime.UtcNow;
    await _userRepository.UpdateAsync(user);

    // 4. Generate JWT token
    var token = GenerateJwtToken(user);
    
    return new AuthResponseDto { Token = token, ... };
}
```

**Register Flow**:
```csharp
public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
{
    // 1. Check if user exists
    if (await UserExistsAsync(registerDto.Email)) return null;

    // 2. Create user with hashed password
    var user = new User
    {
        Email = registerDto.Email,
        PasswordHash = HashPassword(registerDto.Password), // ⚠️ Hash it!
        ...
    };

    // 3. Save to database
    await _userRepository.AddAsync(user);

    // 4. Generate JWT token
    return new AuthResponseDto { Token = GenerateJwtToken(user), ... };
}
```

**JWT Token Generation**:
```csharp
private string GenerateJwtToken(User user)
{
    var secretKey = _configuration["JwtSettings:SecretKey"];
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    // Claims = information stored IN the token
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Name, user.FullName),
        new Claim("userId", user.Id.ToString())
    };

    var token = new JwtSecurityToken(
        issuer: "LibraryAPI",
        audience: "LibraryAPIUsers",
        claims: claims,
        expires: DateTime.UtcNow.AddHours(24),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

**Password Hashing**:
```csharp
// ⚠️ NOTE: SHA256 used for simplicity
// Production should use BCrypt or Argon2!
private static string HashPassword(string password)
{
    using var sha256 = SHA256.Create();
    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
    return Convert.ToBase64String(hashedBytes);
}
```

**Interview Points**:
- ✅ **Claims**: Data embedded in the token (userId, email, name)
- ✅ **Signing Key**: Secret key to sign/verify tokens (stored in appsettings)
- ✅ **Expiration**: Tokens expire after 24 hours for security
- ✅ **Password Hashing**: One-way hash (can't reverse it)
- 🔴 **Production Note**: Use BCrypt/Argon2 instead of SHA256

---

### 5️⃣ Auth Controller (`Controllers/AuthController.cs`)
**Purpose**: REST API endpoints for authentication

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        if (result == null)
            return Unauthorized(new { message = "Invalid email or password" });
        
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
    {
        if (await _authService.UserExistsAsync(registerDto.Email))
            return BadRequest(new { message = "User already exists" });

        var result = await _authService.RegisterAsync(registerDto);
        return CreatedAtAction(nameof(Login), result);
    }
}
```

**API Endpoints**:
| Method | Endpoint | Purpose | Returns |
|--------|----------|---------|---------|
| POST | `/api/auth/login` | Authenticate user | JWT token or 401 Unauthorized |
| POST | `/api/auth/register` | Create new user | JWT token or 400 Bad Request |

**Interview Points**:
- ✅ Returns **401 Unauthorized** for invalid credentials
- ✅ Returns **400 Bad Request** if user already exists
- ✅ Returns **201 Created** with token on successful registration
- ✅ No [Authorize] attribute (public endpoints!)

---

### 6️⃣ JWT Configuration (`Program.cs`)
**Purpose**: Configure authentication middleware

```csharp
// 1. Read JWT settings from appsettings.json
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

// 2. Configure Authentication Service
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,          // Check token issuer
        ValidateAudience = true,        // Check token audience
        ValidateLifetime = true,        // Check expiration
        ValidateIssuerSigningKey = true,// Check signature
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// 3. Register services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 4. Add middleware (ORDER MATTERS!)
app.UseAuthentication();  // ⚠️ MUST come before UseAuthorization
app.UseAuthorization();
```

**Interview Points**:
- ✅ **Middleware order**: Authentication → Authorization
- ✅ **Token validation**: Checks issuer, audience, expiration, signature
- ✅ **Dependency Injection**: Services registered as Scoped

---

### 7️⃣ Protecting Endpoints
**Purpose**: Require authentication for specific controllers/actions

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]  // ⚠️ ALL endpoints in this controller require authentication
public class BooksController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetAll()
    {
        // This endpoint now requires a valid JWT token!
        var books = await _bookService.GetAllBooksAsync();
        return Ok(books);
    }

    [AllowAnonymous]  // Override [Authorize] for specific endpoint
    [HttpGet("public")]
    public async Task<ActionResult> GetPublicBooks()
    {
        // This endpoint is accessible without authentication
        return Ok("Public data");
    }
}
```

**Interview Points**:
- ✅ `[Authorize]` at controller level = all endpoints protected
- ✅ `[Authorize]` at action level = specific endpoint protected
- ✅ `[AllowAnonymous]` overrides `[Authorize]`
- ✅ Returns **401 Unauthorized** if no token provided

---

### 8️⃣ Swagger Configuration
**Purpose**: Test authentication in Swagger UI

```csharp
builder.Services.AddSwaggerGen(c =>
{
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference
          { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
});
```

**Interview Points**:
- ✅ Adds "Authorize" button to Swagger UI
- ✅ Can test protected endpoints with JWT token
- ✅ Token format: `Bearer <token>`

---

## 🔄 Authentication Flow Diagram

### Registration Flow
```
1. User submits registration form
   ↓
2. POST /api/auth/register { email, password, ... }
   ↓
3. AuthController validates input
   ↓
4. AuthService checks if user exists
   ↓
5. Hash password with SHA256
   ↓
6. Save user to database
   ↓
7. Generate JWT token with claims (userId, email, name)
   ↓
8. Return { token, email, fullName, expiresAt }
```

### Login Flow
```
1. User submits login form
   ↓
2. POST /api/auth/login { email, password }
   ↓
3. AuthController validates input
   ↓
4. AuthService finds user by email
   ↓
5. Verify password hash
   ↓
6. Update LastLoginAt timestamp
   ↓
7. Generate JWT token
   ↓
8. Return { token, email, fullName, expiresAt }
```

### Protected Endpoint Access
```
1. User makes request to /api/books
   ↓
2. Include header: Authorization: Bearer <token>
   ↓
3. JWT Middleware extracts token from header
   ↓
4. Validate token signature, expiration, issuer, audience
   ↓
5. Extract claims (userId, email) from token
   ↓
6. Set HttpContext.User with claims
   ↓
7. Controller action executes
   ↓
8. Can access user info: User.FindFirst("userId")
```

---

## 🧪 Testing the Authentication

### 1. Register a New User
```http
POST https://localhost:5124/api/auth/register
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "password": "SecurePass123",
  "confirmPassword": "SecurePass123"
}
```

**Expected Response (201 Created)**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "john@example.com",
  "fullName": "John Doe",
  "expiresAt": "2024-01-13T10:00:00Z"
}
```

### 2. Login with Existing User
```http
POST https://localhost:5124/api/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "SecurePass123"
}
```

### 3. Access Protected Endpoint
```http
GET https://localhost:5124/api/books
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Without Token → 401 Unauthorized**:
```json
{
  "status": 401,
  "title": "Unauthorized"
}
```

**With Valid Token → 200 OK**:
```json
[
  {
    "id": 1,
    "title": "Harry Potter and the Philosopher's Stone",
    ...
  }
]
```

---

## 🎤 Interview Questions & Answers

### Q1: What is JWT and how does it work?
**A**: JWT (JSON Web Token) is a compact, URL-safe token format for securely transmitting information between parties. It consists of three parts:
1. **Header**: Algorithm (HS256) and type (JWT)
2. **Payload**: Claims (userId, email, expiration)
3. **Signature**: HMAC hash of header + payload + secret key

The server signs the token with a secret key. On each request, the client sends the token, and the server validates the signature without hitting the database.

### Q2: Where is the JWT stored on the client side?
**A**: Common options:
- **localStorage**: Simple but vulnerable to XSS attacks
- **sessionStorage**: Cleared when browser closes
- **httpOnly cookie**: Most secure (JavaScript can't access it)

For this project, the client (React/Angular/etc.) would store it in localStorage and send it in the `Authorization: Bearer <token>` header.

### Q3: How do you invalidate a JWT token?
**A**: JWTs are **stateless**, so you can't invalidate them server-side. Solutions:
1. **Short expiration** (our approach: 24 hours)
2. **Token blacklist**: Store revoked tokens in Redis/database
3. **Refresh tokens**: Issue short-lived access tokens + long-lived refresh tokens
4. **Version number**: Add version to claims, increment on logout

### Q4: Why hash passwords instead of encrypting them?
**A**: 
- **Hashing** is one-way (can't reverse it)
- **Encryption** is two-way (can decrypt with key)

We **never** need to see the original password, so hashing is correct. Even if the database is stolen, attackers can't get plain passwords. We verify passwords by hashing the input and comparing hashes.

### Q5: What's the difference between Authentication and Authorization?
**A**: 
- **Authentication**: "**Who** are you?" (Login, JWT token)
- **Authorization**: "**What** can you do?" (Roles, permissions)

Our current implementation handles authentication. For authorization, we'd add:
```csharp
[Authorize(Roles = "Admin")]  // Only admins can access
```

### Q6: How would you implement roles?
**A**: Add a `Role` property to the User model:
```csharp
public class User
{
    ...
    public string Role { get; set; } = "User";  // User, Admin, Librarian
}

// In AuthService.GenerateJwtToken:
new Claim(ClaimTypes.Role, user.Role)

// In controller:
[Authorize(Roles = "Admin,Librarian")]
```

### Q7: What if the secret key is leaked?
**A**: 
- Attacker can generate valid tokens for any user!
- **Mitigation**:
  1. Store secret in **Azure Key Vault** or **environment variables** (not appsettings.json)
  2. Use different keys for Dev/Prod
  3. Rotate keys periodically
  4. Use asymmetric keys (RS256) instead of symmetric (HS256)

### Q8: Why use Repository Pattern with EF Core?
**A**: 
- **Abstraction**: Controllers don't know about DbContext
- **Testability**: Can mock IUserRepository in unit tests
- **Consistency**: All data access follows same pattern
- **Flexibility**: Easy to swap EF Core for Dapper/ADO.NET

---

## 🚀 Production Improvements

### What We'd Change for Production:

1. **Password Hashing**:
   ```csharp
   // Current: SHA256 (simple but weak)
   // Production: BCrypt or Argon2
   using BCrypt.Net;
   var hash = BCrypt.HashPassword(password);
   var isValid = BCrypt.Verify(password, hash);
   ```

2. **Secret Key Storage**:
   ```csharp
   // Current: appsettings.json
   // Production: Azure Key Vault, AWS Secrets Manager, or Environment Variables
   var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
   ```

3. **Refresh Tokens**:
   ```csharp
   // Issue short-lived access token (15 min) + long-lived refresh token (7 days)
   // When access token expires, use refresh token to get new access token
   ```

4. **Email Verification**:
   ```csharp
   public bool IsEmailVerified { get; set; }
   // Send verification email on registration
   // Require verification before allowing login
   ```

5. **Rate Limiting**:
   ```csharp
   // Prevent brute-force attacks
   builder.Services.AddRateLimiter(...);
   ```

6. **Logging**:
   ```csharp
   _logger.LogWarning("Failed login attempt for {Email}", loginDto.Email);
   ```

---

## 📝 Summary for Interview

"I implemented JWT authentication using a layered architecture:

1. **Data Layer**: User model, UserRepository with GetByEmailAsync
2. **Business Logic**: AuthService handles password hashing, token generation, login/register
3. **API Layer**: AuthController exposes POST /login and /register endpoints
4. **Security**: JWT tokens with claims (userId, email), 24-hour expiration
5. **Protection**: [Authorize] attribute on BooksController requires valid token
6. **Configuration**: JWT middleware validates tokens, Swagger UI for testing

The flow is: User registers → Password hashed with SHA256 → Stored in database → JWT token generated with claims → Token returned to client → Client sends token with each request → Middleware validates token → Controller executes.

In production, I'd use BCrypt for password hashing, store secrets in Azure Key Vault, implement refresh tokens, and add email verification."

---

## 🔗 Useful Resources

- [JWT.io](https://jwt.io/) - Decode and inspect JWT tokens
- [Microsoft Docs: ASP.NET Core Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)
- [OWASP Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)
