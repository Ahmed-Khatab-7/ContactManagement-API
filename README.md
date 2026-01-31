# 📇 Contact Management API

### A Production-Ready .NET 9 Web API for User Registration & Personal Address Book Management

## 🎯 Architecture Decisions

The project is built on a **3-Layer Architecture** (Presentation, Business, Data) to ensure a clean separation of concerns, maintainability, and testability.

| Decision | Rationale |
| :--- | :--- |
| **3-Layer Architecture** | Clean separation without over-engineering (no CQRS/MediatR needed for this scope). |
| **DTOs for all I/O** | Prevents over-posting attacks and hides internal data structures. |
| **UserId on every Contact** | Ensures complete data isolation between users. |
| **FluentValidation** | Provides more expressive and testable validation logic than Data Annotations. |
| **Soft Delete** | Preserves data for audit purposes by marking records as deleted instead of permanent removal. |
| **Factory Methods in Domain** | Encapsulates domain models and controls the creation process. |

---

## 📁 Project Structure

The solution follows a standard, clean structure for an ASP.NET Core Web API:

```
ContactManagement/
├── ContactManagement.sln
├── docker-compose.yml
├── .gitignore
├── .dockerignore
│
├── src/
│   └── ContactManagement.Api/
│       ├── Program.cs                  # Application entry point
│       ├── Controllers/                # API Endpoints (Auth, Contacts)
│       ├── Services/                   # Business Logic (AuthService, ContactService)
│       ├── Models/                     # Domain Entities (ApplicationUser, Contact)
│       ├── DTOs/                       # Data Transfer Objects
│       ├── Data/                       # Database Layer (DbContext, Configurations, Migrations)
│       ├── Validators/                 # FluentValidation rules
│       ├── Middleware/                 # Custom Exception Handling Middleware
│       └── Extensions/                 # Extension Methods for DI and User Context
│
└── tests/
    └── ContactManagement.Tests/        # Unit Test Project (xUnit)
        ├── Services/
        ├── Validators/
        └── Controllers/
```

---

## 🚀 Getting Started

### Prerequisites

Before running the application, ensure you have the following installed:

| Requirement | Version | Download Link |
| :--- | :--- | :--- |
| **.NET SDK** | 9.0 or later | [Download .NET 9](https://dotnet.microsoft.com/download/dotnet/9.0) |
| **Docker Desktop** | Latest | [Download Docker](https://www.docker.com/products/docker-desktop/) |
| **Git** | Latest | [Download Git](https://git-scm.com/downloads) |

---

### Option 1: Docker Compose (Recommended) 🐳

The easiest way to run the complete application with all dependencies (API and SQL Server):

**Step 1: Clone the Repository**

```bash
git clone https://github.com/Ahmed-Khatab-7/ContactManagement.git
cd ContactManagement
```

**Step 2: Build and Run**

```bash
# Build and start all services in detached mode (background)
docker-compose up --build -d
```

**Step 3: Access the Application**

| Service | URL |
| :--- | :--- |
| 🌐 API Base URL | `http://localhost:5000` |
| 📖 Swagger UI | `http://localhost:5000/swagger` |
| 🗄️ SQL Server | `localhost:1433` |

**Step 4: Stop the Application**

```bash
# Stop and remove containers, networks, and images
docker-compose down

# Stop and remove all data (including the SQL Server volume)
docker-compose down -v
```

---

### Option 2: Local Development

This option requires a locally running SQL Server instance or a different database setup.

**Step 1: Clone the Repository**

```bash
git clone https://github.com/Ahmed-Khatab-7/ContactManagement.git
cd ContactManagement
```

**Step 2: Start SQL Server (using Docker)**

```bash
docker run -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=Corelia_DB@2026!" \
  -p 1433:1433 \
  --name sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

**Step 3: Update Connection String**

Update the `DefaultConnection` string in `src/ContactManagement.Api/appsettings.json` to match your local SQL Server setup if necessary.

**Step 4: Run the Application**

```bash
cd src/ContactManagement.Api
dotnet restore
dotnet run
```

The API will be available at `http://localhost:5000` and the Swagger UI at `http://localhost:5000/swagger`.

---

## 📡 API Documentation

### Base URL

```
http://localhost:5000/api
```

### 🔐 Authentication Endpoints

#### 1. Create an Account (Register)

Creates a new user account with secure password hashing.

| Property | Value |
| :--- | :--- |
| **Endpoint** | `POST /api/auth/register` |
| **Auth Required** | ❌ No |

**Request Body:**

```json
{
  "email": "ahmed@example.com",
  "password": "Ahmed@123456",
  "firstName": "Ahmed",
  "lastName": "Khatab"
}
```

**Success Response (200 OK):**

```json
{
  "succeeded": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-01-30T23:00:00Z",
  "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "email": "ahmed@example.com",
  "errors": null
}
```

#### 2. Sign In (Login)

Authenticates a user and returns a JWT token.

| Property | Value |
| :--- | :--- |
| **Endpoint** | `POST /api/auth/login` |
| **Auth Required** | ❌ No |

**Request Body:**

```json
{
  "email": "ahmed@example.com",
  "password": "Ahmed@123456"
}
```

**Success Response (200 OK):**

```json
{
  "succeeded": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-01-30T23:00:00Z",
  "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "email": "ahmed@example.com",
  "errors": null
}
```

### 📇 Contact Endpoints

⚠️ **All contact endpoints require authentication.** Include the JWT token in the `Authorization` header:

```
Authorization: Bearer <your-jwt-token>
```

#### 3. Add a Contact

Adds a new contact to the user's address book.

| Property | Value |
| :--- | :--- |
| **Endpoint** | `POST /api/contacts` |
| **Auth Required** | ✅ Yes (Bearer Token) |

**Request Body:**

```json
{
  "firstName": "Mohamed",
  "lastName": "Ali",
  "email": "mohamed.ali@example.com",
  "phoneNumber": "+20-100-111-1111",
  "birthDate": "1995-03-15",
  "address": "Cairo, Egypt",
  "notes": "Best friend from university"
}
```

**Success Response (201 Created):**

```json
{
  "id": 1,
  "firstName": "Mohamed",
  "lastName": "Ali",
  "email": "mohamed.ali@example.com",
  "phoneNumber": "+20-100-111-1111",
  "birthDate": "1995-03-15",
  "address": "Cairo, Egypt",
  "notes": "Best friend from university"
}
```

#### 4. List All Contacts

Returns all contacts for the authenticated user with pagination and sorting support.

| Property | Value |
| :--- | :--- |
| **Endpoint** | `GET /api/contacts` |
| **Auth Required** | ✅ Yes (Bearer Token) |

**Query Parameters:**

| Parameter | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `page` | `int` | `1` | Page number (starts at 1) |
| `pageSize` | `int` | `10` | Items per page (max: 100) |
| `sortBy` | `string` | `name` | Sort by: `name`, `birthdate`, `email`, `createdat` |
| `sortDescending` | `bool` | `false` | Sort in descending order |
| `search` | `string` | `null` | Search in name, email, phone |

**Example Request:**

```
GET /api/contacts?page=1&pageSize=10&sortBy=name&sortDescending=false&search=mohamed
```

#### 5. Retrieve Contact by ID

| Property | Value |
| :--- | :--- |
| **Endpoint** | `GET /api/contacts/{id}` |
| **Auth Required** | ✅ Yes (Bearer Token) |

#### 6. Update Contact

| Property | Value |
| :--- | :--- |
| **Endpoint** | `PUT /api/contacts/{id}` |
| **Auth Required** | ✅ Yes (Bearer Token) |

#### 7. Delete Contact (Soft Delete)

| Property | Value |
| :--- | :--- |
| **Endpoint** | `DELETE /api/contacts/{id}` |
| **Auth Required** | ✅ Yes (Bearer Token) |

---

## 🔒 Security Implementation

### Data Isolation

Every database query is automatically filtered by the authenticated user's ID, which is extracted securely from the JWT token.

```csharp
// All queries are filtered by UserId extracted from JWT token
var contacts = await _context.Contacts
    .Where(c => c.UserId == currentUserId)  // Data isolation
    .ToListAsync();
```

**Security Guarantees:**

*   ✅ Users can only see their own contacts.
*   ✅ Users cannot access, modify, or delete other users' contacts.
*   ✅ `UserId` is extracted from the JWT token, not from the request body.
*   ✅ Soft-deleted contacts are automatically filtered out.

### Additional Security Measures

| Feature | Status |
| :--- | :--- |
| **SQL Injection Prevention** | ✅ EF Core parameterized queries |
| **Input Validation** | ✅ FluentValidation |
| **Exception Handling** | ✅ Global middleware |
| **HTTPS Support** | ✅ Built-in |
| **Non-root Docker User** | ✅ Security best practice |

---

## 🐳 Docker Configuration

The `docker-compose.yml` file defines the services for the API and the SQL Server database.

```yaml
version: '3.8'

services:
  api:
    # ... build and environment configuration
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=ContactManagementDb;User Id=sa;Password=Corelia_DB@2026!;TrustServerCertificate=True;MultipleActiveResultSets=true
    depends_on:
      sqlserver:
        condition: service_healthy

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    # ... environment and healthcheck configuration
    environment:
      - ACCEPT_EULA=Y
      - MSSQL_SA_PASSWORD=Corelia_DB@2026!
```

**Docker Commands:**

```bash
# Build and start
docker-compose up --build

# Start in background
docker-compose up -d

# Stop containers
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

---

## ⚙️ Configuration

### `appsettings.json`

Key configuration settings for the application:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=ContactManagementDb;User Id=sa;Password=Corelia_DB@2026!;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "SecretKey": "YourSecretKeyHere-MustBeAtLeast32Characters!",
    "Issuer": "ContactManagementApi",
    "Audience": "ContactManagementClient",
    "ExpirationInMinutes": 60
  }
}
```

### Environment Variables

| Variable | Description |
| :--- | :--- |
| `ASPNETCORE_ENVIRONMENT` | `Development` or `Production` |
| `ConnectionStrings__DefaultConnection` | Database connection string (used by Docker Compose) |

---


## 👨‍💻 Author

**Ahmed Khatab**

[![LinkedIn](https://img.shields.io/badge/LinkedIn-Ahmed%20Khatab-0077B5?style=for-the-badge&logo=linkedin)](https://www.linkedin.com/in/akhatab0)


