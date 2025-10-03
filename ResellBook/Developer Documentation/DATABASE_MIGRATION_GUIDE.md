# Database Migration Guide - ResellPanda API

## 📋 **Table of Contents**

1. [Understanding Database Migrations](#understanding-database-migrations)
2. [Why Migrations Are Important](#why-migrations-are-important)
3. [Best Practices for Model Changes](#best-practices-for-model-changes)
4. [Step-by-Step Migration Process](#step-by-step-migration-process)
5. [Common Issues and Solutions](#common-issues-and-solutions)
6. [Emergency Database Reset](#emergency-database-reset)
7. [Production Deployment Considerations](#production-deployment-considerations)

---

## 🎯 **Understanding Database Migrations**

### **What are Migrations?**
Database migrations are version control for your database schema. They allow you to:
- Track changes to database structure over time
- Apply schema changes consistently across environments
- Rollback changes if needed
- Collaborate with team members without schema conflicts

### **How Entity Framework Core Handles Migrations**
- **Migrations Folder**: Contains C# code that defines schema changes
- **__EFMigrationsHistory Table**: Tracks which migrations have been applied
- **Model Snapshot**: Represents current state of your data model

---

## 💡 **Why Migrations Are Important**

### **The Problem We Solved:**
- **Issue**: Adding new model properties caused 500 Internal Server Errors
- **Root Cause**: Database schema didn't match the application model
- **Symptoms**: 
  - New properties not found in database tables
  - Foreign key constraints errors
  - Migration conflicts between environments

### **Why This Happens:**
1. **Schema Mismatch**: Application expects columns that don't exist in database
2. **Migration History Conflicts**: Old migration records cause confusion
3. **Multiple Environments**: Local vs. Azure database inconsistencies
4. **Incremental Changes**: Each model change needs corresponding database update

### **The Solution Benefits:**
- ✅ Consistent database schema across all environments
- ✅ Trackable database changes
- ✅ Automated deployment of schema updates
- ✅ Rollback capability if issues occur

---

## 🔧 **Best Practices for Model Changes**

### **1. Before Making Model Changes**
```bash
# Always check current database state
dotnet ef database update --verbose

# List existing migrations
dotnet ef migrations list
```

### **2. Model Development Workflow**
1. **Make Model Changes** (Add/Remove/Modify properties)
2. **Create Migration** immediately after model changes
3. **Review Migration** code before applying
4. **Test Migration** in development environment
5. **Apply to Production** only after thorough testing

### **3. Naming Conventions**
```bash
# Use descriptive migration names
dotnet ef migrations add AddBookSubCategoryProperty
dotnet ef migrations add UpdateUserPhoneValidation
dotnet ef migrations add CreateBookReviewsTable

# Avoid generic names
# ❌ dotnet ef migrations add Update1
# ❌ dotnet ef migrations add NewChanges
```

---

## 📝 **Step-by-Step Migration Process**

### **For Adding New Model Properties**

#### **Step 1: Modify Your Model**
```csharp
// Example: Adding new property to Book model
public class Book
{
    public Guid Id { get; set; }
    public string BookName { get; set; }
    // ... existing properties ...
    
    // NEW PROPERTY
    public string ISBN { get; set; } // Added this new property
    public DateTime? PublishedDate { get; set; } // Added this too
}
```

#### **Step 2: Create Migration**
```bash
# Navigate to project root
cd C:\Repos\ResellPanda\ResellBook

# Create migration with descriptive name
dotnet ef migrations add AddISBNAndPublishedDateToBook

# Expected output:
# Build succeeded.
# Done. To undo this action, use 'ef migrations remove'
```

#### **Step 3: Review Generated Migration**
```csharp
// Check Migrations/[Timestamp]_AddISBNAndPublishedDateToBook.cs
public partial class AddISBNAndPublishedDateToBook : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ISBN",
            table: "Books",
            nullable: true);
            
        migrationBuilder.AddColumn<DateTime>(
            name: "PublishedDate",
            table: "Books",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ISBN", table: "Books");
        migrationBuilder.DropColumn(name: "PublishedDate", table: "Books");
    }
}
```

#### **Step 4: Apply Migration**
```bash
# Apply to database
dotnet ef database update

# Expected output showing SQL commands being executed:
# info: Microsoft.EntityFrameworkCore.Database.Command[20101]
#       Executed DbCommand (XXXms) [Parameters=[], CommandType='Text', CommandTimeout='30']
#       ALTER TABLE [Books] ADD [ISBN] nvarchar(max) NULL;
# ...
# Done.
```

#### **Step 5: Verify Migration**
```bash
# List applied migrations to confirm
dotnet ef migrations list

# Check if your new migration appears as (Applied)
```

### **For Creating New Models**

#### **Step 1: Create New Model Class**
```csharp
// Models/BookReview.cs
public class BookReview
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; } // 1-5 stars
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Book Book { get; set; }
    public User User { get; set; }
}
```

#### **Step 2: Add to DbContext**
```csharp
// Data/AppDbContext.cs
public class AppDbContext : DbContext
{
    // ... existing DbSets ...
    public DbSet<BookReview> BookReviews { get; set; } // Add this line

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // ... existing configurations ...
        
        // Configure new model relationships
        modelBuilder.Entity<BookReview>()
            .HasOne(br => br.Book)
            .WithMany()
            .HasForeignKey(br => br.BookId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<BookReview>()
            .HasOne(br => br.User)
            .WithMany()
            .HasForeignKey(br => br.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

#### **Step 3: Create and Apply Migration**
```bash
# Create migration for new table
dotnet ef migrations add CreateBookReviewsTable

# Apply migration
dotnet ef database update
```

---

## 🚨 **Common Issues and Solutions**

### **Issue 1: "There is already an object named 'TableName' in the database"**

**Cause:** Migration history mismatch between local and database

**Solution:**
```bash
# Check what migrations are actually applied in database
dotnet ef migrations list

# If you see conflicting migrations, reset migration history
dotnet ef database update 0  # Rollback all migrations
dotnet ef migrations remove  # Remove problematic migration
dotnet ef migrations add FreshStart  # Create new migration
dotnet ef database update    # Apply clean migration
```

### **Issue 2: "A network-related or instance-specific error occurred"**

**Cause:** Connection string pointing to wrong database

**Solution:**
```json
// Check appsettings.json - ensure Azure connection string is active
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:resellbookdbserver2001.database.windows.net,1433;Initial Catalog=ResellBook_db;Persist Security Info=False;User ID=pandaseller;Password=Password@2001;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    // ❌ Local connection should be commented out:
    // "DefaultConnection": "Server=LOCALHOST\\SQLEXPRESS;Database=LocalDb;Trusted_Connection=True;"
  }
}
```

### **Issue 3: "The name 'MigrationName' is used by an existing migration"**

**Cause:** Migration with same name already exists

**Solution:**
```bash
# Use different migration name
dotnet ef migrations add AddBookISBNProperty_v2

# Or remove existing migration first
dotnet ef migrations remove
dotnet ef migrations add AddBookISBNProperty
```

### **Issue 4: Model validation warnings (decimal precision)**

**Cause:** Decimal properties without explicit precision

**Solution:**
```csharp
// In AppDbContext.OnModelCreating()
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Configure decimal precision to avoid warnings
    modelBuilder.Entity<Book>()
        .Property(b => b.SellingPrice)
        .HasPrecision(18, 2); // 18 total digits, 2 decimal places
        
    // For all decimal properties, specify precision
    modelBuilder.Entity<BookReview>()
        .Property(br => br.AverageRating)
        .HasPrecision(3, 2); // e.g., 4.75
}
```

---

## 🔥 **Emergency Database Reset**

### **When to Use This:**
- Multiple migration conflicts
- Database schema completely corrupted
- Starting fresh after major model restructure
- Cannot resolve migration issues through normal process

### **⚠️ WARNING: This will delete all data!**

#### **Step 1: Delete Existing Database**
```bash
# Delete Azure database
az sql db delete --name ResellBook_db --server resellbookdbserver2001 --resource-group resell-panda-rg --yes
```

#### **Step 2: Recreate Database**
```bash
# Recreate with same configuration
az sql db create --name ResellBook_db --server resellbookdbserver2001 --resource-group resell-panda-rg --service-objective GP_Gen5_2
```

#### **Step 3: Clean Local Migrations**
```bash
# Remove migrations folder completely
Remove-Item -Path "Migrations" -Recurse -Force

# Clean build artifacts
dotnet clean
if (Test-Path "publish") { Remove-Item -Path "publish" -Recurse -Force }
```

#### **Step 4: Create Fresh Migration**
```bash
# Create new initial migration
dotnet ef migrations add InitialCreate

# Apply to fresh database
dotnet ef database update
```

#### **Step 5: Verify Success**
```bash
# Verify migration applied successfully
dotnet ef migrations list

# Should show:
# InitialCreate (Applied)

# Test application
dotnet build --configuration Release
```

---

## 🚀 **Production Deployment Considerations**

### **Before Deploying Model Changes:**

#### **1. Backup Production Data**
```bash
# Create backup before major changes
az sql db export \
  --server resellbookdbserver2001 \
  --name ResellBook_db \
  --resource-group resell-panda-rg \
  --admin-user pandaseller \
  --admin-password Password@2001 \
  --storage-key-type StorageAccessKey \
  --storage-key "your-storage-key" \
  --storage-uri "https://yourstorageaccount.blob.core.windows.net/backups/db-backup-$(date +%Y%m%d).bacpac"
```

#### **2. Test in Staging Environment**
```bash
# Create staging database for testing
az sql db create --name ResellBook_db_staging --server resellbookdbserver2001 --resource-group resell-panda-rg --service-objective GP_Gen5_1

# Update connection string to staging
# Test all migrations and functionality
# Only proceed to production after successful staging tests
```

#### **3. Deployment Checklist**
- [ ] Model changes tested locally
- [ ] Migration created and reviewed
- [ ] Migration tested in staging environment
- [ ] Production backup created
- [ ] Deployment scheduled during low-traffic time
- [ ] Rollback plan prepared

### **Deployment Process:**
```bash
# 1. Build with new models
dotnet build --configuration Release

# 2. Publish application
dotnet publish -c Release -o publish

# 3. Create deployment package
Compress-Archive -Path "publish\*" -DestinationPath "deploy.zip" -Force

# 4. Deploy to Azure (migrations run automatically via Program.cs)
az webapp deploy --resource-group resell-panda-rg --name ResellBook20250929183655 --src-path "deploy.zip" --type zip
```

### **Automatic Migration in Production**
Your `Program.cs` already includes automatic migration:
```csharp
// This runs migrations automatically on startup
try 
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Starting database migration check...");
        
        if (dbContext.Database.IsSqlServer())
        {
            var canConnect = dbContext.Database.CanConnect();
            logger.LogInformation($"Database connection test: {canConnect}");
            
            if (canConnect)
            {
                logger.LogInformation("Connection successful, applying migrations...");
                dbContext.Database.Migrate(); // 🔥 This applies pending migrations automatically
                logger.LogInformation("Database migration completed successfully!");
            }
        }
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Database migration failed: {ErrorMessage}", ex.Message);
}
```

---

## 📋 **Quick Reference Checklist**

### **For Every Model Change:**
- [ ] Make model changes in code
- [ ] Create descriptive migration: `dotnet ef migrations add DescriptiveName`
- [ ] Review generated migration code
- [ ] Apply locally: `dotnet ef database update`
- [ ] Test application locally
- [ ] Build and deploy: `dotnet build && dotnet publish && deploy`

### **Monthly Maintenance:**
- [ ] Review and clean up old migrations
- [ ] Backup production database
- [ ] Monitor migration performance logs
- [ ] Update this documentation with new patterns

### **Emergency Contacts:**
- **Database Issues**: Check Azure Portal > SQL Database > Query Performance Insight
- **Migration Failures**: Check Application Logs in Azure App Service
- **Connection Issues**: Verify firewall rules and connection strings

---

## 🔍 **Monitoring and Troubleshooting**

### **Check Migration Status:**
```bash
# See which migrations are applied
dotnet ef migrations list

# Check database connection
dotnet ef database update --verbose
```

### **View Application Logs:**
```bash
# Stream Azure App Service logs
az webapp log tail --resource-group resell-panda-rg --name ResellBook20250929183655
```

### **Common Log Messages:**
- ✅ `"Database migration completed successfully!"` - Migrations applied
- ⚠️ `"Could not connect to database. Migration skipped."` - Connection issue
- ❌ `"Database migration failed"` - Migration error, check details

---

**Last Updated:** October 4, 2025  
**Database Server:** resellbookdbserver2001.database.windows.net  
**Database Name:** ResellBook_db  
**Migration Strategy:** Code-First with Entity Framework Core