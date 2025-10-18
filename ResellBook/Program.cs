
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ResellBook.Data;
using ResellBook.Services;
using ResellBook.Utils; // Add this for SimpleLogger
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure services with error handling
try
{
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
}
catch (Exception ex)
{
    // Log critical service configuration failures
    SimpleLogger.LogCritical("Program", "ConfigureServices", "Service configuration failed", ex);
    throw; // Critical configuration failure - rethrow
}

var app = builder.Build();

// Apply database migrations with enhanced error handling
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (dbContext.Database.IsSqlServer())
        {
            // Test connection first
            var canConnect = dbContext.Database.CanConnect();

            if (canConnect)
            {
                dbContext.Database.Migrate();
            }
        }
    }
}
catch (Exception ex)
{
    SimpleLogger.LogCritical("Program", "Main", "Database migration failed", ex);
    // Don't throw - allow app to start even if migration fails
}

// Configure the HTTP request pipeline with error handling
try
{
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

    app.Run();
}
catch (Exception ex)
{
    SimpleLogger.LogCritical("Program", "Main", "Application startup failed", ex);
    throw; // Critical startup failure - rethrow to fail fast
}

