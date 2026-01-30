using ContactManagement.Api.Data;
using ContactManagement.Api.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ============ SERVICE CONFIGURATION ============

// Database
builder.Services.AddDatabase(builder.Configuration);

// Identity (User Management)
builder.Services.AddIdentityServices();

// JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Application Services (DI)
builder.Services.AddApplicationServices();

// FluentValidation
builder.Services.AddValidation();

// Controllers
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddSwaggerDocumentation();

// CORS (for frontend integration)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ============ MIDDLEWARE PIPELINE ============

// Development-only middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Contact Management API v1");
    });
}

// HTTPS redirection (comment out for Docker/local development if needed)
// app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Authentication & Authorization (order matters!)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ============ DATABASE MIGRATION ============
// Automatically apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Applying database migrations...");
        db.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database");
        throw;
    }
}

app.Run();
