# 🚀 ASP.NET Core Azure Deployment Guide

## 📋 What This Guide Covers
Simple step-by-step deployment of ASP.NET Core apps to Azure App Service

## 🚨 Common Problem We're Solving
- **Error**: HTTP 500.30 - App failed to start
- **Cause**: .NET version mismatch, package issues, configuration errors
- **Solution**: Follow this guide exactly

## ✅ Before You Start
Make sure you have:
1. Azure CLI installed → [Download here](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
2. .NET 8.0 SDK → [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
3. Text editor (VS Code, Visual Studio, etc.)
4. Azure subscription access

## 🔧 Step-by-Step Deployment

### STEP 1: Setup & Login to Azure

#### 1.1 Login to Azure
Open **Command Prompt** or **PowerShell** (any folder):

```bash
# Login to Azure
az login

# Check you're logged in
az account show

# If you have multiple subscriptions, list them
az account list --output table

# Set the right subscription (replace with your subscription ID)
az account set --subscription "YOUR_SUBSCRIPTION_ID"
```

#### 1.2 Go to Your Project Folder
```bash
# Navigate to your project (replace with your actual path)
cd C:\Repos\ResellPanda\ResellBook

# Make sure you're in the right place (should show your .csproj file)
dir *.csproj
```

### STEP 2: Fix .NET Version & Packages

#### 2.1 Change .NET Version (CRITICAL!)
Open your project file `YourProject.csproj` in any text editor:

**FIND THIS:**
```xml
<TargetFramework>net9.0</TargetFramework>
```

**REPLACE WITH:**
```xml
<TargetFramework>net8.0</TargetFramework>
```

#### 2.2 Update Package Versions
In the same `YourProject.csproj` file, **FIND** the `<PackageReference>` section:

**REPLACE ALL package versions with these:**
```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.11" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.11" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.11" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.11" />
<PackageReference Include="Swashbuckle.AspNetCore.Swagger" Version="6.9.0" />
<PackageReference Include="Swashbuckle.AspNetCore.SwaggerGen" Version="6.9.0" />
<PackageReference Include="Swashbuckle.AspNetCore.SwaggerUI" Version="6.9.0" />
```

#### 2.3 Fix NuGet Issues (If You Get Errors)
In **Command Prompt** (from your project folder):

```bash
# See what NuGet sources you have
dotnet nuget list source

# If you see corporate/work sources causing errors, disable them
dotnet nuget disable source "Package sourceWK.Observability.Global"
```

### STEP 3: Fix Your Code

#### 3.1 Add Logging (Better Error Messages)
Open `Program.cs` and **ADD THIS** right after `var builder = WebApplication.CreateBuilder(args);`:

```csharp
// Add better logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
```

#### 3.2 Fix Database Migration (Prevents Crashes)
In `Program.cs`, **FIND** this section:
```csharp
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (dbContext.Database.IsSqlServer())
    {
        dbContext.Database.Migrate();
    }
}
```

**REPLACE WITH:**
```csharp
var app = builder.Build();

// Safer database migration
try 
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (dbContext.Database.IsSqlServer())
        {
            dbContext.Database.Migrate();
        }
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Database migration failed - but app will continue");
}
```

#### 3.3 Remove Passwords from Config (Security!)
Open `appsettings.json` and **REMOVE** real passwords:

**CHANGE FROM:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=tcp:...;Password=YourRealPassword;..."
},
"SMTP": {
  "Pass": "your-real-smtp-password"
}
```

**TO:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=tcp:...;Password=***REMOVED***;..."
},
"SMTP": {
  "Pass": "***REMOVED***"
}
```
> **Note**: We'll add the real passwords in Azure later (more secure)

#### 3.4 Add Production Settings
In `Program.cs`, **FIND** the pipeline section and **MAKE SURE** it looks like this:

```csharp
// Configure the HTTP request pipeline
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
app.UseRouting();
app.MapControllers();

var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("App is starting...");

app.Run();
```

#### 3.5 Fix Model Issues (If You Have User Model)
Open `Models/User.cs` and **ADD** `required` to string properties:

```csharp
public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required]
    public required string Name { get; set; }        // ← Add 'required'
    [Required]
    public required string Email { get; set; }       // ← Add 'required'
    [Required]
    public required string PasswordHash { get; set; } // ← Add 'required'
    public bool IsEmailVerified { get; set; } = false;
    public required string EmailVerificationCode { get; set; } // ← Add 'required'
    public DateTime? VerificationCodeExpiry { get; set; }
}
```

#### 3.6 Fix Email Service (If You Have One)
Open `Services/EmailService.cs` and **FIND** this line:
```csharp
await client.ConnectAsync(_config["SMTP:Host"], int.Parse(_config["SMTP:Port"]), SecureSocketOptions.StartTls);
```

**CHANGE TO:**
```csharp
await client.ConnectAsync(_config["SMTP:Host"], int.Parse(_config["SMTP:Port"] ?? "587"), SecureSocketOptions.StartTls);
```

### STEP 4: Configure Azure

#### 4.1 Find Your Azure Resources
In **Command Prompt**:

```bash
# See all your resource groups
az group list --output table

# See all your web apps
az webapp list --output table
```
> **Write down** your resource group name and app service name from the output

#### 4.2 Add Database Connection (Secure Way)
Replace `[YOUR_APP_NAME]`, `[YOUR_RESOURCE_GROUP]`, and `[YOUR_DB_PASSWORD]`:

```bash
az webapp config connection-string set \
    --name [YOUR_APP_NAME] \
    --resource-group [YOUR_RESOURCE_GROUP] \
    --connection-string-type SQLAzure \
    --settings DefaultConnection="Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=YourDB;User ID=yourusername;Password=[YOUR_DB_PASSWORD];Encrypt=True;"
```

#### 4.3 Add Email Settings (If You Use Email)
Replace the placeholders with your actual values:

```bash
az webapp config appsettings set \
    --name [YOUR_APP_NAME] \
    --resource-group [YOUR_RESOURCE_GROUP] \
    --settings SMTP__Host="smtp.gmail.com" SMTP__Port="587" SMTP__User="youremail@gmail.com" SMTP__Pass="[YOUR_EMAIL_PASSWORD]"
```

#### 4.4 Turn On Logging (For Debugging)
```bash
az webapp log config \
    --name [YOUR_APP_NAME] \
    --resource-group [YOUR_RESOURCE_GROUP] \
    --application-logging filesystem \
    --level information
```

### STEP 5: Build & Deploy

#### 5.1 Clean & Restore
In **Command Prompt** (from your project folder):

```bash
# Clean old builds
dotnet clean

# Get fresh packages
dotnet restore
```
> Should say "Restore succeeded" with no errors

#### 5.2 Build Your App
```bash
# Build for production
dotnet build --configuration Release
```
> Should say "Build succeeded" - if not, fix errors first!

#### 5.3 Create Deployment Files
```bash
# Create files ready for Azure
dotnet publish --configuration Release

# Check files were created
dir bin\Release\net8.0\publish
```
> You should see your DLL files and other app files

#### 5.4 Make ZIP File
In **PowerShell**:

```powershell
# Create a zip file
Compress-Archive -Path "bin\Release\net8.0\publish\*" -DestinationPath ".\myapp.zip" -Force

# Check it was created
dir myapp.zip
```

#### 5.5 Deploy to Azure
Replace with your actual app name and resource group:

```bash
# Upload your app to Azure
az webapp deploy --resource-group resell-panda-rg --name ResellBook20250929183655 --src-path ".\myapp.zip" --type zip
```
> Wait for "Deployment has completed successfully"

### STEP 6: Test Your App

#### 6.1 Check If It's Working
Replace `[YOUR_APP_NAME]` with your actual app name:

```bash
# Test your app (should return JSON data)
curl https://[YOUR_APP_NAME].azurewebsites.net/weatherforecast
```
> If this works, your app is running! 🎉

#### 6.2 Check Logs (If Something's Wrong)
```bash
# See live logs (press Ctrl+C to stop)
az webapp log tail --name [YOUR_APP_NAME] --resource-group [YOUR_RESOURCE_GROUP]

# Download logs to a file
az webapp log download --name [YOUR_APP_NAME] --resource-group [YOUR_RESOURCE_GROUP] --log-file logs.zip
```

#### 6.3 Test Your API Endpoints
Use **Postman** or **curl**:

**Test Signup:**
```bash
curl -X POST https://[YOUR_APP_NAME].azurewebsites.net/api/auth/signup \
  -H "Content-Type: application/json" \
  -d '{"name":"Test User","email":"test@example.com","password":"Test123!"}'
```

**Test Login (after signup):**
```bash
curl -X POST https://[YOUR_APP_NAME].azurewebsites.net/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}'
```

## 🔄 Quick Checklist for Future Deployments

### ✅ Before Every Deployment
```bash
# 1. Check you have .NET 8.0
dotnet --version

# 2. Make sure you're logged into Azure
az account show

# 3. Go to your project folder
cd C:\Path\To\Your\Project
```

### 🚀 Quick Deploy Process

#### A. Prepare
```bash
# Get latest code
git pull

# Clean and restore
dotnet clean
dotnet restore
```

#### B. Check These Files
- [ ] `YourProject.csproj` → Target .NET 8.0
- [ ] Package versions → Use .NET 8.0 compatible versions  
- [ ] `appsettings.json` → No real passwords
- [ ] `Program.cs` → Has error handling

#### C. Build & Deploy
```bash
# Build
dotnet build --configuration Release

# Create files
dotnet publish --configuration Release

# Make zip (PowerShell)
Compress-Archive -Path "bin\Release\net8.0\publish\*" -DestinationPath "deploy.zip" -Force

# Upload to Azure
az webapp deploy \
    --resource-group [YOUR_RG] \
    --name [YOUR_APP] \
    --src-path "deploy.zip" \
    --type zip
```

#### D. Test
```bash
# Quick test
curl https://[YOUR_APP].azurewebsites.net/weatherforecast

# Check logs if needed
az webapp log tail --name [YOUR_APP] --resource-group [YOUR_RG]
```

## 💡 Best Practices (Do This!)

### 🔒 Security
- ✅ Never put passwords in code
- ✅ Use Azure App Service settings for secrets
- ✅ Rotate passwords regularly
- ✅ Keep .NET version updated

### ⚡ Performance  
- ✅ Use Application Insights for monitoring
- ✅ Add health check endpoints
- ✅ Monitor your app regularly
- ✅ Use caching where appropriate

### 🛡️ Reliability
- ✅ Always test locally first
- ✅ Have error handling in your code
- ✅ Keep backups of your database
- ✅ Test your deployment process

### 👩‍💻 Developer Tips
- ✅ Use Git for version control
- ✅ Create deployment scripts
- ✅ Document your setup
- ✅ Have a staging environment

## 🚨 When Things Go Wrong

### App Won't Start (500.30 Error)
**Problem**: Your app shows "HTTP 500.30" error

**Fix It:**
```bash
# 1. Check the logs first
az webapp log tail --name [YOUR_APP] --resource-group [YOUR_RG]

# 2. Most common fix: Change .csproj to .NET 8.0
# Edit YourProject.csproj and change:
# <TargetFramework>net9.0</TargetFramework> 
# TO: <TargetFramework>net8.0</TargetFramework>

# 3. Rebuild and redeploy
dotnet clean
dotnet restore
dotnet publish --configuration Release
```

### Can't Restore Packages
**Problem**: `dotnet restore` fails

**Fix It:**
```bash
# 1. See what sources you have
dotnet nuget list source

# 2. Disable work/corporate sources
dotnet nuget disable source "[BAD_SOURCE_NAME]"

# 3. Clear cache and try again
dotnet nuget locals all --clear
dotnet restore
```

### Database Won't Connect
**Problem**: App starts but database errors

**Fix It:**
```bash
# 1. Check your connection string in Azure
az webapp config connection-string list --name [YOUR_APP] --resource-group [YOUR_RG]

# 2. Make sure your database allows Azure connections
# (Check firewall rules in Azure portal)

# 3. Test the connection string manually
sqlcmd -S yourserver.database.windows.net -d yourdb -U yourusername -P yourpassword
```

### Build Errors
**Problem**: `dotnet build` fails

**Fix It:**
```bash
# 1. Clean everything
dotnet clean

# 2. Force restore
dotnet restore --force

# 3. Build with details to see what's wrong
dotnet build --verbosity detailed

# 4. Check your .NET version
dotnet --info
```

### Email Not Working
**Problem**: Emails not sending

**Fix It:**
```bash
# 1. Check your SMTP settings in Azure
az webapp config appsettings list --name [YOUR_APP] --resource-group [YOUR_RG]

# 2. Test manually with Postman or curl
curl -X POST https://[YOUR_APP].azurewebsites.net/api/auth/signup \
  -H "Content-Type: application/json" \
  -d '{"name":"Test","email":"test@test.com","password":"Test123!"}'

# 3. Check logs for SMTP errors
az webapp log tail --name [YOUR_APP] --resource-group [YOUR_RG]
```

## 📊 Keep Your App Healthy

### Enable App Insights (Optional)
```bash
# Add monitoring to your app
az monitor app-insights component create \
    --app [YOUR_APP] \
    --location [YOUR_LOCATION] \
    --resource-group [YOUR_RG]
```

### Regular Monitoring
```bash
# Watch live logs
az webapp log tail --name [YOUR_APP] --resource-group [YOUR_RG]

# Download logs for analysis
az webapp log download --name [YOUR_APP] --resource-group [YOUR_RG] --log-file logs.zip
```

---

## 📚 Quick Reference

### 🔧 Most Used Commands
```bash
# Login to Azure
az login

# Build and deploy (in project folder)
dotnet clean
dotnet restore
dotnet build --configuration Release
dotnet publish --configuration Release
Compress-Archive -Path "bin\Release\net8.0\publish\*" -DestinationPath "app.zip" -Force
az webapp deploy --resource-group [RG] --name [APP] --src-path "app.zip" --type zip

# Check if it worked
curl https://[YOUR_APP].azurewebsites.net/weatherforecast
```

### 📁 Important Files to Check
- `YourProject.csproj` → Must target .NET 8.0
- `Program.cs` → Must have error handling
- `appsettings.json` → No real passwords
- `Models/User.cs` → Use `required` for string properties

### 🎯 Success Checklist
- [ ] App targets .NET 8.0
- [ ] Packages are .NET 8.0 compatible
- [ ] No passwords in code files
- [ ] Error handling added to Program.cs
- [ ] Database connection set in Azure
- [ ] App deploys without errors
- [ ] `/weatherforecast` endpoint returns JSON
- [ ] Auth endpoints work (if you have them)

---

**✨ That's it! You now have a working Azure deployment process.**

*Document created: September 29, 2025*  
*App: ResellBook - ResellPanda Project*