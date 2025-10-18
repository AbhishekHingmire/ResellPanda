
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ResellBook.Data;
using ResellBook.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Enhanced logging for Azure App Service
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configure SQL Server connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Register EmailService
builder.Services.AddScoped<EmailService>();

// Add Memory Cache for performance optimization
builder.Services.AddMemoryCache();

// Add Response Caching
builder.Services.AddResponseCaching();
// Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVercel", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add response compression for cost savings
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = new[] { "text/plain", "text/css", "application/javascript", "text/html", "application/xml", "text/xml", "application/json", "text/json" };
});

var app = builder.Build();

// Apply database migrations with enhanced error handling
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Starting database migration check...");

        if (dbContext.Database.IsSqlServer())
        {
            logger.LogInformation("SQL Server detected, checking database connection...");

            // Test connection first
            var canConnect = dbContext.Database.CanConnect();
            logger.LogInformation($"Database connection test: {canConnect}");

            if (canConnect)
            {
                logger.LogInformation("Connection successful, applying migrations...");
                dbContext.Database.Migrate();
                logger.LogInformation("Database migration completed successfully!");
            }
            else
            {
                logger.LogWarning("Could not connect to database. Migration skipped.");
            }
        }
        else
        {
            logger.LogInformation("Not using SQL Server - migration skipped.");
        }
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Database migration failed: {ErrorMessage}", ex.Message);
    logger.LogWarning("App will continue without migration - manual database setup may be required");
    // Don't throw - allow app to start even if migration fails
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression(); // Enable response compression

// Enable response caching
app.UseResponseCaching();

// Optimize static files with caching headers
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static files for 1 hour
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600");
    }
});

app.UseRouting();

// Enable CORS (optional, but usually before auth)
app.UseCors("AllowVercel");

// ? Must come AFTER routing, BEFORE endpoints
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireAuthorization();

var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("Application starting up...");

app.Run();

