# 🚀 Complete Azure Deployment Guide for Beginners
### From Zero to Production in 60 Minutes

> **Target Audience:** Developers with 2+ months experience  
> **Time Required:** 45-60 minutes  
> **Prerequisites:** Visual Studio Code, Git, PowerShell  

---

## 📋 **Table of Contents**

1. [Prerequisites & Setup](#1-prerequisites--setup)
2. [Azure Account & Subscription Setup](#2-azure-account--subscription-setup)
3. [Local Development Environment](#3-local-development-environment)
4. [Azure Resource Creation](#4-azure-resource-creation)
5. [Database Setup & Migration](#5-database-setup--migration)
6. [Application Configuration](#6-application-configuration)
7. [Deployment Process](#7-deployment-process)
8. [Post-Deployment Verification](#8-post-deployment-verification)
9. [Troubleshooting Guide](#9-troubleshooting-guide)
10. [Maintenance & Monitoring](#10-maintenance--monitoring)

---

## **1. Prerequisites & Setup**

### 🖥️ **Required Software**
```powershell
# Check if you have these installed:
dotnet --version          # Should be 8.0 or higher
git --version            # Any recent version
code --version          # Visual Studio Code
```

### 📦 **Install Required Tools**
1. **Install Azure CLI:**
   ```powershell
   # Download and install Azure CLI from: https://aka.ms/installazurecliwindows
   # Verify installation:
   az --version
   ```

2. **Install SQL Server Management Studio (Optional but Recommended):**
   - Download from: https://aka.ms/ssmsfullsetup

### 🔑 **Required Information Checklist**
Before starting, gather these details:
- [ ] Azure Subscription ID
- [ ] Preferred Azure Region (e.g., East US, West Europe)
- [ ] App Name (must be globally unique)
- [ ] SQL Server Admin Username
- [ ] SQL Server Admin Password (Strong password required)

---

## **2. Azure Account & Subscription Setup**

### 🌐 **Step 1: Create Azure Account**
1. Go to https://portal.azure.com
2. Sign up for free account (includes $200 credit)
3. Complete verification process

### 🎯 **Step 2: Find Your Subscription**
```powershell
# Login to Azure
az login

# List your subscriptions
az account list --output table

# Set default subscription (replace with your subscription ID)
az account set --subscription "your-subscription-id-here"
```

### 💡 **Beginner Tip:**
> Your Subscription ID looks like: `12345678-1234-1234-1234-123456789abc`  
> Copy this ID - you'll need it multiple times!

---

## **3. Local Development Environment**

### 📁 **Step 1: Clone the Project**
```powershell
# Navigate to your projects folder
cd C:\Projects

# Clone the repository
git clone https://github.com/your-username/ResellPanda.git
cd ResellPanda\ResellBook
```

### 🔧 **Step 2: Install Dependencies**
```powershell
# Restore NuGet packages
dotnet restore

# Build the project to check for issues
dotnet build
```

**Expected Output:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### ❌ **If Build Fails:**
```powershell
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build --verbosity detailed
```

---

## **4. Azure Resource Creation**

### 🏗️ **Step 1: Create Resource Group**
```powershell
# Set variables (replace these with your values)
$resourceGroup = "ResellBook-RG"
$location = "East US"
$appName = "ResellBook$(Get-Date -Format 'yyyyMMddHHmmss')"

# Create resource group
az group create --name $resourceGroup --location $location
```

### 🖥️ **Step 2: Create App Service Plan**
```powershell
# Create App Service Plan (B1 tier - good for development)
az appservice plan create `
  --name "$appName-plan" `
  --resource-group $resourceGroup `
  --location $location `
  --sku B1 `
  --is-linux false
```

### 🌐 **Step 3: Create Web App**
```powershell
# Create Web App
az webapp create `
  --name $appName `
  --resource-group $resourceGroup `
  --plan "$appName-plan" `
  --runtime "DOTNET|8.0"

# Enable detailed logging
az webapp log config `
  --name $appName `
  --resource-group $resourceGroup `
  --application-logging filesystem `
  --detailed-error-messages true `
  --failed-request-tracing true `
  --web-server-logging filesystem
```

### 💾 **Step 4: Create SQL Server**
```powershell
# Set SQL Server variables
$sqlServerName = "$appName-sql"
$sqlAdminUser = "sqladmin"
$sqlAdminPassword = "YourStrongPassword123!"  # Change this!

# Create SQL Server
az sql server create `
  --name $sqlServerName `
  --resource-group $resourceGroup `
  --location $location `
  --admin-user $sqlAdminUser `
  --admin-password $sqlAdminPassword

# Configure firewall to allow Azure services
az sql server firewall-rule create `
  --server $sqlServerName `
  --resource-group $resourceGroup `
  --name "AllowAzureServices" `
  --start-ip-address 0.0.0.0 `
  --end-ip-address 0.0.0.0
```

### 🗄️ **Step 5: Create SQL Database**
```powershell
# Create database
$databaseName = "ResellBookDB"
az sql db create `
  --server $sqlServerName `
  --resource-group $resourceGroup `
  --name $databaseName `
  --edition Basic `
  --capacity 5
```

---

## **5. Database Setup & Migration**

### 🔗 **Step 1: Get Connection String**
```powershell
# Get the connection string
$connectionString = az sql db show-connection-string `
  --server $sqlServerName `
  --name $databaseName `
  --client ado.net `
  --output tsv

# Replace placeholders in connection string
$connectionString = $connectionString.Replace('<username>', $sqlAdminUser)
$connectionString = $connectionString.Replace('<password>', $sqlAdminPassword)

Write-Host "Connection String: $connectionString"
```

### ⚙️ **Step 2: Configure Application Settings**
```powershell
# Set connection string in Azure Web App
az webapp config connection-string set `
  --name $appName `
  --resource-group $resourceGroup `
  --connection-string-type SQLAzure `
  --settings DefaultConnection="$connectionString"

# Set other app settings
az webapp config appsettings set `
  --name $appName `
  --resource-group $resourceGroup `
  --settings `
    "ASPNETCORE_ENVIRONMENT=Production" `
    "JWT_SECRET=YourSuperSecretJWTKeyThatIsAtLeast32CharactersLong!" `
    "JWT_ISSUER=ResellBookApp" `
    "JWT_AUDIENCE=ResellBookUsers"
```

### 🏃 **Step 3: Apply Database Migrations**
```powershell
# Update connection string in appsettings.json for local migration
$localConnectionString = "Server=$sqlServerName.database.windows.net;Database=$databaseName;User Id=$sqlAdminUser;Password=$sqlAdminPassword;Encrypt=True;TrustServerCertificate=False;"

# Update appsettings.json
$appsettingsPath = "appsettings.json"
$appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
$appsettings.ConnectionStrings.DefaultConnection = $localConnectionString
$appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath

# Apply migrations
dotnet ef database update
```

### ✅ **Verify Database Creation:**
```sql
-- Connect to your database using SQL Server Management Studio
-- Server name: your-server-name.database.windows.net
-- Authentication: SQL Server Authentication
-- Username: sqladmin
-- Password: YourStrongPassword123!

-- Run this query to verify tables were created:
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';
```

---

## **6. Application Configuration**

### 📝 **Step 1: Update Configuration Files**

**appsettings.Production.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=#{SQL_SERVER_NAME}#;Database=#{DATABASE_NAME}#;User Id=#{SQL_USER}#;Password=#{SQL_PASSWORD}#;Encrypt=True;TrustServerCertificate=False;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "Secret": "#{JWT_SECRET}#",
    "Issuer": "#{JWT_ISSUER}#",
    "Audience": "#{JWT_AUDIENCE}#"
  }
}
```

### 🔐 **Step 2: Security Configuration**
```powershell
# Allow your IP to access SQL Server (for development)
$myIP = (Invoke-WebRequest -Uri "https://api.ipify.org").Content
az sql server firewall-rule create `
  --server $sqlServerName `
  --resource-group $resourceGroup `
  --name "AllowMyIP" `
  --start-ip-address $myIP `
  --end-ip-address $myIP
```

---

## **7. Deployment Process**

### 🚀 **Step 1: Create Deployment Script**

Create `deploy-azure.ps1`:
```powershell
param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$WebAppName,
    
    [Parameter(Mandatory=$false)]
    [string]$BuildConfiguration = "Release"
)

Write-Host "🚀 Starting Azure Deployment..." -ForegroundColor Green

try {
    # Step 1: Build the application
    Write-Host "📦 Building application..." -ForegroundColor Yellow
    dotnet clean
    dotnet restore
    dotnet build --configuration $BuildConfiguration
    
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed with exit code $LASTEXITCODE"
    }

    # Step 2: Publish the application
    Write-Host "📤 Publishing application..." -ForegroundColor Yellow
    $publishPath = "bin\Release\net8.0\publish"
    dotnet publish --configuration $BuildConfiguration --output $publishPath --no-build

    # Step 3: Create deployment package
    Write-Host "📦 Creating deployment package..." -ForegroundColor Yellow
    $zipPath = "deployment.zip"
    if (Test-Path $zipPath) { Remove-Item $zipPath }
    
    Compress-Archive -Path "$publishPath\*" -DestinationPath $zipPath

    # Step 4: Deploy to Azure
    Write-Host "🌐 Deploying to Azure..." -ForegroundColor Yellow
    az webapp deployment source config-zip `
        --resource-group $ResourceGroupName `
        --name $WebAppName `
        --src $zipPath

    # Step 5: Restart the web app
    Write-Host "🔄 Restarting web app..." -ForegroundColor Yellow
    az webapp restart --name $WebAppName --resource-group $ResourceGroupName

    # Step 6: Test the deployment
    Write-Host "🧪 Testing deployment..." -ForegroundColor Yellow
    $appUrl = "https://$WebAppName.azurewebsites.net"
    Start-Sleep -Seconds 10
    
    try {
        $response = Invoke-WebRequest -Uri "$appUrl/api/health" -Method GET -TimeoutSec 30
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ Deployment successful!" -ForegroundColor Green
            Write-Host "🌐 Your app is available at: $appUrl" -ForegroundColor Cyan
        } else {
            Write-Host "⚠️ App deployed but health check failed. Status: $($response.StatusCode)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "⚠️ App deployed but health check failed: $($_.Exception.Message)" -ForegroundColor Yellow
        Write-Host "🌐 Try accessing: $appUrl" -ForegroundColor Cyan
    }

    # Clean up
    Remove-Item $zipPath -ErrorAction SilentlyContinue
    Remove-Item $publishPath -Recurse -Force -ErrorAction SilentlyContinue

} catch {
    Write-Host "❌ Deployment failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
```

### 🎯 **Step 2: Deploy Application**
```powershell
# Run the deployment script
.\deploy-azure.ps1 -ResourceGroupName $resourceGroup -WebAppName $appName
```

---

## **8. Post-Deployment Verification**

### 🧪 **Step 1: Test API Endpoints**
```powershell
# Test health endpoint
$baseUrl = "https://$appName.azurewebsites.net"
Invoke-WebRequest -Uri "$baseUrl/api/health"

# Test WeatherForecast endpoint
Invoke-WebRequest -Uri "$baseUrl/WeatherForecast"
```

### 📊 **Step 2: Check Application Logs**
```powershell
# Stream logs (press Ctrl+C to stop)
az webapp log tail --name $appName --resource-group $resourceGroup

# Download logs
az webapp log download --name $appName --resource-group $resourceGroup
```

### 🔍 **Step 3: Verify Database Connection**
```powershell
# Check if migrations were applied
# Go to Azure Portal > SQL Database > Query Editor
# Run: SELECT * FROM __EFMigrationsHistory;
```

---

## **9. Troubleshooting Guide**

### 🚨 **Common Issues & Solutions**

#### **Issue 1: Build Fails**
```
Error: The project file could not be loaded
```
**Solution:**
```powershell
# Clean everything and start fresh
dotnet clean
Remove-Item -Path "bin","obj" -Recurse -Force -ErrorAction SilentlyContinue
dotnet restore
dotnet build
```

#### **Issue 2: SQL Connection Fails**
```
Error: Cannot open server 'xxx' requested by the login
```
**Solutions:**
```powershell
# 1. Check firewall rules
az sql server firewall-rule list --server $sqlServerName --resource-group $resourceGroup

# 2. Add your current IP
$currentIP = (Invoke-WebRequest -Uri "https://api.ipify.org").Content
az sql server firewall-rule create `
  --server $sqlServerName `
  --resource-group $resourceGroup `
  --name "CurrentIP" `
  --start-ip-address $currentIP `
  --end-ip-address $currentIP

# 3. Test connection
Test-NetConnection -ComputerName "$sqlServerName.database.windows.net" -Port 1433
```

#### **Issue 3: App Won't Start**
```powershell
# Check app settings
az webapp config appsettings list --name $appName --resource-group $resourceGroup

# Check logs
az webapp log tail --name $appName --resource-group $resourceGroup

# Restart the app
az webapp restart --name $appName --resource-group $resourceGroup
```

#### **Issue 4: 500 Internal Server Error**
**Solutions:**
1. **Check detailed logs:**
   ```powershell
   # Enable detailed error messages
   az webapp config set --name $appName --resource-group $resourceGroup --detailed-error-messages-enabled true
   ```

2. **Common causes:**
   - Missing connection string
   - Wrong JWT configuration
   - Database migration not applied
   - Missing environment variables

#### **Issue 5: Entity Framework Migrations**
```powershell
# If migrations fail, reset and recreate
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 🔧 **Diagnostic Commands**
```powershell
# Check all Azure resources
az resource list --resource-group $resourceGroup --output table

# Check web app configuration
az webapp config show --name $appName --resource-group $resourceGroup

# Check SQL server status
az sql server show --name $sqlServerName --resource-group $resourceGroup

# Test database connectivity
sqlcmd -S "$sqlServerName.database.windows.net" -d $databaseName -U $sqlAdminUser -P $sqlAdminPassword -Q "SELECT 1"
```

---

## **10. Maintenance & Monitoring**

### 📊 **Set Up Application Insights**
```powershell
# Create Application Insights
$appInsightsName = "$appName-insights"
az monitor app-insights component create `
  --app $appInsightsName `
  --location $location `
  --resource-group $resourceGroup

# Get instrumentation key
$instrumentationKey = az monitor app-insights component show `
  --app $appInsightsName `
  --resource-group $resourceGroup `
  --query instrumentationKey `
  --output tsv

# Configure app to use Application Insights
az webapp config appsettings set `
  --name $appName `
  --resource-group $resourceGroup `
  --settings "APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=$instrumentationKey"
```

### 🔄 **Automated Backups**
```powershell
# Create backup configuration
az sql db export `
  --server $sqlServerName `
  --name $databaseName `
  --resource-group $resourceGroup `
  --admin-user $sqlAdminUser `
  --admin-password $sqlAdminPassword `
  --storage-key-type StorageAccessKey `
  --storage-key "your-storage-key" `
  --storage-uri "https://yourstorageaccount.blob.core.windows.net/backups/backup.bacpac"
```

### 📈 **Scaling Configuration**
```powershell
# Scale up the App Service Plan when needed
az appservice plan update `
  --name "$appName-plan" `
  --resource-group $resourceGroup `
  --sku P1V2

# Scale out (add more instances)
az appservice plan update `
  --name "$appName-plan" `
  --resource-group $resourceGroup `
  --number-of-workers 2
```

---

## **🎯 Quick Reference**

### **Essential Commands**
```powershell
# Login to Azure
az login

# Deploy application
.\deploy-azure.ps1 -ResourceGroupName "ResellBook-RG" -WebAppName "YourAppName"

# Check logs
az webapp log tail --name "YourAppName" --resource-group "ResellBook-RG"

# Restart app
az webapp restart --name "YourAppName" --resource-group "ResellBook-RG"

# Update database
dotnet ef database update
```

### **Important URLs**
- **Azure Portal:** https://portal.azure.com
- **Your App:** https://yourappname.azurewebsites.net
- **App Logs:** https://yourappname.scm.azurewebsites.net
- **SQL Server:** yourserver.database.windows.net

---

## **🏁 Completion Checklist**

- [ ] Azure account created and verified
- [ ] Resource group created
- [ ] App Service Plan created
- [ ] Web App created and configured
- [ ] SQL Server created
- [ ] Database created and migrations applied
- [ ] Application deployed successfully
- [ ] All API endpoints working
- [ ] Logs configured and accessible
- [ ] Application Insights set up (optional)
- [ ] Backup strategy implemented

**🎉 Congratulations! Your ResellBook API is now live in Azure!**

---

*Need help? Check the troubleshooting section or review the logs for specific error messages.*