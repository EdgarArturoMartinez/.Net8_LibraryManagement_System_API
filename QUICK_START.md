# 🚀 Quick Start Guide - Library API

## Run the API
```powershell
cd C:\Arthur\Development\2026\CoPilotAPI\LibraryAPI\LibraryAPI
dotnet run
```
**Swagger UI**: https://localhost:5124/swagger

## Run Tests
```powershell
cd C:\Arthur\Development\2026\CoPilotAPI\LibraryAPI\LibraryAPI.Tests
dotnet test
```
**Expected**: 28 tests pass ✅

## Test Authentication

### 1. Register User
```json
POST /api/auth/register
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "password": "SecurePass123",
  "confirmPassword": "SecurePass123"
}
```

### 2. Copy Token from Response
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "john@example.com",
  "fullName": "John Doe",
  "expiresAt": "2024-01-13T10:00:00Z"
}
```

### 3. Authorize in Swagger
- Click **"Authorize"** button
- Enter: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
- Click **"Authorize"**

### 4. Test Protected Endpoints
```
GET /api/books        → 200 OK (with token)
GET /api/books        → 401 Unauthorized (without token)
```

## GitHub Repository
**URL**: https://github.com/EdgarArturoMartinez/.Net8_LibraryManagement_System_API

**Commits**:
1. Initial: Clean Architecture
2. **Auth**: JWT Authentication
3. **Tests**: Unit Tests (28 tests)

## Interview Prep
Read these BEFORE your interview:
1. **AUTHENTICATION_GUIDE.md** - JWT deep dive
2. **UNIT_TESTING_GUIDE.md** - Testing patterns
3. **PROJECT_SUMMARY.md** - Complete overview

## Quick Stats
- **Tests**: 28 passing
- **Coverage**: ~85%
- **Commits**: 3 (all pushed)
- **Documentation**: 3 comprehensive guides

## One-Liner Explanations

### Authentication
"I implemented JWT authentication with User model, AuthService for token generation, AuthController for login/register, and [Authorize] to protect endpoints. Tokens have 24-hour expiration with claims (userId, email). Production would use BCrypt, Key Vault, and refresh tokens."

### Unit Testing
"I wrote 28 unit tests using xUnit, Moq, and AAA pattern. Tests cover BookService (13), BooksController (12), and AuthService (3) with ~85% coverage. Mocks isolate units, tests run in 1.5 seconds with no database. Production would add integration tests."

## Key Files
- `AUTHENTICATION_GUIDE.md` - 400+ lines of JWT documentation
- `UNIT_TESTING_GUIDE.md` - 500+ lines of testing guide
- `PROJECT_SUMMARY.md` - Complete project overview
- `Controllers/AuthController.cs` - Login/Register endpoints
- `Services/AuthService.cs` - JWT token generation
- `LibraryAPI.Tests/` - All unit tests

Good luck! 🎯
