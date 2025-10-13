# 🚀 Complete Azure Deployment Guide for Beginners
### 🎓 **Learn Cloud Development While Building Production Applications**

> **🎯 Learning Philosophy:** Understand cloud concepts through hands-on implementation  
> **⏰ Time Investment:** 90 minutes learning + 60 minutes implementation  
> **🎓 Skill Level:** Beginner-friendly with professional-grade results  
> **🏗️ End Goal:** Production-ready web application in Microsoft Azure  

---

## 🧠 **What You'll Learn**

### **☁️ Cloud Computing Fundamentals**
- **Infrastructure as a Service (IaaS) vs Platform as a Service (PaaS):** Why Azure App Service is better than managing your own servers
- **Managed Services:** How Azure handles database maintenance, security updates, and backups automatically
- **Scalability:** How cloud applications handle traffic spikes and growth
- **Security:** Modern authentication, SSL certificates, and firewall configuration

### **🏗️ Azure Architecture You'll Build**
```
Internet ←→ [Load Balancer] ←→ [App Service] ←→ [SQL Database]
                    ↓                ↓              ↓
               [SSL Certificate]  [Application]  [Automatic Backups]
                    ↓              [Insights]         ↓  
               [Custom Domain]        ↓         [Point-in-Time Recovery]
                                 [Monitoring]
                                     ↓
                              [File Storage Account]
```

### **🛠️ Professional Skills You'll Develop**
- **Azure Resource Management:** Creating and configuring cloud infrastructure
- **Database Administration:** SQL Server setup, migrations, connection security
- **Application Deployment:** Modern CI/CD practices and deployment strategies  
- **Monitoring & Diagnostics:** Setting up alerts and reading application logs
- **Security Configuration:** SSL, authentication, and access control
- **Cost Management:** Understanding and optimizing cloud spending

---

## 📋 **Table of Contents**

1. [Prerequisites & Setup](#1-prerequisites--setup) - *Tool installation and workspace preparation*
2. [Azure Account & Subscription Setup](#2-azure-account--subscription-setup) - *Cloud account configuration and billing*
3. [Local Development Environment](#3-local-development-environment) - *Project setup and dependency management*
4. [Azure Resource Creation](#4-azure-resource-creation) - *Infrastructure provisioning and architecture*
5. [Database Setup & Migration](#5-database-setup--migration) - *Data persistence and schema management*
6. [Application Configuration](#6-application-configuration) - *Security, connections, and environment settings*
7. [Deployment Process](#7-deployment-process) - *Code publishing and release management*
8. [Post-Deployment Verification](#8-post-deployment-verification) - *Testing and validation procedures*
9. [Troubleshooting Guide](#9-troubleshooting-guide) - *Problem diagnosis and resolution*
10. [Maintenance & Monitoring](#10-maintenance--monitoring) - *Ongoing operations and optimization*

---

## **1. Prerequisites & Setup**

### 🎓 **Learning Objective**
Understand the tools that enable modern cloud development and why each is essential for professional development workflows.

### 🖥️ **Required Software & Their Purpose**

#### **💻 Development Environment**
```powershell
# Check if you have these installed:
dotnet --version          # Should be 8.0 or higher
git --version            # Any recent version  
code --version          # Visual Studio Code
```

**🤔 Why These Tools?**
- **`.NET SDK`:** Compiles your C# code into applications that can run on any platform
- **`Git`:** Version control system that tracks code changes and enables collaboration
- **`VS Code`:** Modern code editor with built-in debugging and Azure integration

#### **☁️ Cloud Management Tools**
```powershell
# Install Azure CLI for cloud resource management
# Download from: https://aka.ms/installazurecliwindows
az --version
```

**🤔 Why Azure CLI?**
- **Command Line Interface:** Manage Azure resources programmatically (faster than clicking in portal)
- **Automation Ready:** Scripts can create entire infrastructures consistently
- **Professional Standard:** Used by DevOps engineers and cloud architects globally

#### **🗄️ Database Management (Optional but Recommended)**
- **SQL Server Management Studio:** Visual interface for database operations
- **Download:** https://aka.ms/ssmsfullsetup

**🤔 Why SSMS?**
- **Visual Database Management:** See tables, run queries, view data easily
- **Troubleshooting:** Debug database issues and performance problems
- **Learning Tool:** Understand how your data is structured and accessed

### 📝 **Azure Concepts You'll Use**

#### **🏗️ Resource Group**
**What it is:** Logical container that groups related Azure resources
**Why important:** Organizes resources, manages permissions, tracks costs
**Real-world analogy:** Like a folder that contains all files for a project

#### **📍 Azure Region**
**What it is:** Geographic location where your resources will be hosted
**Why important:** Affects performance (latency), compliance, and availability
**How to choose:** Pick the region closest to your users for best performance

#### **🌐 App Service**
**What it is:** Platform-as-a-Service (PaaS) for hosting web applications
**Why use it:** No server management needed - Azure handles infrastructure
**Cost benefit:** Pay only for what you use, automatic scaling

#### **�️ Azure SQL Database**
**What it is:** Managed SQL Server database in the cloud
**Why use it:** Automatic backups, security patches, high availability
**Business benefit:** Focus on your app, not database administration

### 🔑 **Information Gathering Checklist**

Before starting, prepare these details:

**📋 Azure Account Information**
- [ ] **Azure Subscription ID** *(Format: 12345678-1234-1234-1234-123456789abc)*
  - *Found in:* Azure Portal → Subscriptions
  - *Purpose:* Identifies which billing account to use

**🌍 Infrastructure Decisions**  
- [ ] **Preferred Azure Region** *(e.g., East US, West Europe)*
  - *Recommendation:* Choose closest to your target users
  - *Cost Impact:* Some regions are more expensive than others

**🏷️ Naming Conventions**
- [ ] **App Name** *(Must be globally unique)*
  - *Format:* yourappname + random numbers (e.g., resellbook20241011)
  - *Why unique:* Azure creates URLs like yourapp.azurewebsites.net
  
**🔐 Security Credentials**
- [ ] **SQL Server Admin Username** *(e.g., sqladmin)*
  - *Requirements:* No special characters, not 'admin' or 'root'
- [ ] **SQL Server Admin Password** 
  - *Requirements:* 12+ characters, uppercase, lowercase, numbers, special chars
  - *Security tip:* Use a password manager to generate strong passwords

**💡 Pro Tips for Beginners:**
- **Keep a notepad:** Write down the resource names and passwords you create
- **Use consistent naming:** All resources should follow same pattern (e.g., resellbook-prod-app, resellbook-prod-sql)
- **Screenshot everything:** Capture important configuration screens for reference

---

## **2. Azure Account & Subscription Setup**

### � **Learning Objective**
Understand Azure's billing and resource organization model, and learn how to authenticate and manage cloud resources securely.

### 💰 **Understanding Azure Pricing Model**

**🆓 Free Tier Benefits:**
- **$200 credit** for first 30 days (enough for several production apps)
- **12 months free services** (App Service, SQL Database, Storage)
- **Always free services** (some monitoring, authentication features)

**📊 Cost Management Concepts:**
- **Pay-as-you-use:** Only pay for resources while they're running
- **Scaling costs:** More traffic = higher costs, but automatic scaling handles this
- **Resource optimization:** Choose right-sized resources to control costs

### �🌐 **Step 1: Create Azure Account**

**🔗 Action:** Go to https://portal.azure.com

**📝 Account Setup Process:**
1. **Sign up for free account** - Choose personal or work email
2. **Phone verification** - Required for security and fraud prevention  
3. **Credit card verification** - Required but won't be charged during free trial
4. **Complete profile** - Choose appropriate region and preferences

**🤔 Why Credit Card Required?**
- **Identity verification:** Prevents abuse of free credits
- **Seamless transition:** Automatic billing when free credits expire
- **No surprise charges:** Clear warnings before any billing occurs

### 🎯 **Step 2: Understand Your Subscription**

**💡 What is an Azure Subscription?**
- **Billing container:** Groups resources for cost tracking and payment
- **Security boundary:** Controls who can create and manage resources
- **Resource limit container:** Prevents accidental overspending

```powershell
# Login to Azure (opens browser for authentication)
az login

# List your subscriptions with details
az account list --output table
```

**📋 Expected Output:**
```
Name                     CloudName    SubscriptionId                       State    IsDefault
-----------------------  -----------  -----------------------------------  -------  -----------
Pay-As-You-Go           AzureCloud   12345678-1234-1234-1234-123456789abc  Enabled  True
```

**🔍 Understanding the Output:**
- **Name:** Human-readable subscription name
- **SubscriptionId:** Unique identifier for billing and resource management
- **State:** Enabled means you can create resources
- **IsDefault:** Which subscription CLI commands will use by default

### 🎛️ **Step 3: Set Active Subscription**

```powershell
# Set default subscription (replace with YOUR subscription ID)
az account set --subscription "12345678-1234-1234-1234-123456789abc"

# Verify it's set correctly
az account show --output table
```

**🤔 Why Set Default Subscription?**
- **Consistency:** All Azure CLI commands will use this subscription
- **Safety:** Prevents accidentally creating resources in wrong subscription
- **Efficiency:** No need to specify subscription in every command

### 🔐 **Step 4: Understand Authentication**

**🎫 Authentication Methods:**
1. **Interactive Login:** `az login` (opens browser - most common for development)
2. **Service Principal:** For automation and scripts (advanced)
3. **Managed Identity:** For applications running in Azure (production)

**🛡️ Security Best Practices:**
- **Use personal account for learning:** Easy setup and management
- **Use work account for enterprise:** Centralized access control
- **Never share credentials:** Each developer should have their own account
- **Enable MFA:** Multi-factor authentication for additional security

### 📊 **Step 5: Check Your Quotas and Limits**

```powershell
# Check compute quotas in your preferred region
az vm list-usage --location "East US" --output table

# Check your current subscription details
az account show --query "{Name:name, Id:id, State:state}" --output table
```

**🤔 Why Check Quotas?**
- **Resource limits:** Free accounts have limited compute cores, storage
- **Planning:** Understand what you can deploy
- **Troubleshooting:** Quota issues are common deployment failures

### 💡 **Common Beginner Mistakes to Avoid**

**❌ Wrong Region Selection:**
- **Problem:** Creating resources in expensive or distant regions
- **Solution:** Always specify your preferred region in commands
- **Best practice:** Use same region for all related resources

**❌ Multiple Subscriptions Confusion:**
- **Problem:** Creating resources in different subscriptions accidentally
- **Solution:** Always set and verify default subscription first
- **Best practice:** Use descriptive names for subscriptions

**❌ Ignoring Cost Alerts:**
- **Problem:** Surprise bills when free credits run out
- **Solution:** Set up billing alerts from day one
- **Best practice:** Monitor spending weekly, especially during learning

### 🎯 **Success Validation**
Before continuing, verify:
- [ ] You can run `az account show` without errors
- [ ] The displayed subscription is the one you want to use
- [ ] You have an active subscription with "Enabled" state
- [ ] You understand your current credit balance (if using free account)

**🔍 Quick Health Check:**
```powershell
# This command should show your subscription details without errors
az account show --query "{Name:name, Id:id, User:user.name}" --output table
```

If this works, you're ready for the next step! 🚀

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

#### **Issue 6: Nested Publish Folders (MSB3030 Error)**
```
Error MSB3030: Could not copy the file "C:\Repos\ResellPanda\ResellBook\publish\publish\publish\publish\publish\publish\ResellBook.runtimeconfig.json" because it was not found.
```
**Root Cause:** 
- `dotnet publish -o publish` creates output **inside** existing `publish` folder
- Multiple deployments without cleanup create nested directories: `publish/publish/publish/...`
- PowerShell preserves directory structure unlike some build systems

**Solution:**
```powershell
# Clean publish directory before building (added to deploy.ps1)
if (Test-Path "publish") {
    Write-Host "🧹 Cleaning previous publish directory..." -ForegroundColor Yellow
    Remove-Item "publish" -Recurse -Force
}

# Then publish normally
dotnet publish -c Release -o publish
```

**Prevention:**
- Always clean publish directory before deployment
- Use unique output directories for each build
- Add cleanup step to deployment scripts

**Technical Details:**
- **Issue Date:** October 13, 2025
- **Affected Script:** `deploy.ps1` (build step)
- **Impact:** Deployment failures with nested folder paths
- **Resolution:** Added directory cleanup before `dotnet publish`
- **Testing:** Verified deployment succeeds after fix
- **Prevention:** Always clean publish directory before deployment

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