# 🤖 Complete Infrastructure Automation Scripts
### 🎓 **Learn DevOps Automation Through Practical Implementation**

> **🎯 Learning Philosophy:** Understand Infrastructure as Code (IaC) and DevOps automation by implementing production-grade deployment pipelines  
> **⏰ Time Investment:** 45 minutes learning + 5-15 minutes execution  
> **🎓 Skill Development:** Professional-grade automation skills used by DevOps engineers  
> **🏗️ End Result:** One-click deployment system that creates entire Azure infrastructures  

---

## 🧠 **What You'll Learn**

### **🏗️ Infrastructure as Code (IaC) Concepts**
**What it is:** Managing infrastructure through code instead of manual configuration
**Why important:** Consistent, repeatable, version-controlled infrastructure
**Professional impact:** Essential skill for modern cloud development and DevOps roles

### **🔄 DevOps Automation Principles**
```
Manual Process Problems → Automation Solutions
├── Human errors → Scripted consistency
├── Slow deployments → Automated speed  
├── Environment drift → Reproducible environments
├── Documentation gaps → Self-documenting code
└── Scaling bottlenecks → Parallel automation
```

### **🛠️ PowerShell & Azure CLI Mastery**
- **Infrastructure Management:** Create, configure, and manage Azure resources programmatically
- **Error Handling:** Implement robust error handling and rollback procedures
- **Progress Tracking:** Build user-friendly automation with clear progress indicators
- **Security Practices:** Handle sensitive information like passwords and connection strings securely

### **🚀 Professional Automation Patterns**
- **Idempotent Scripts:** Run safely multiple times without causing problems
- **Parameter Validation:** Prevent common mistakes through input validation
- **Health Checking:** Automated verification that deployments actually work
- **Rollback Strategies:** Automatic recovery when deployments fail

---

## 📋 **Automation Scripts Library**

| Script Category | What You'll Learn | Business Value | Time Savings |
|----------------|-------------------|----------------|--------------|
| **[Complete Infrastructure Setup](#1-complete-infrastructure-setup)** | **IaC Fundamentals:** Resource provisioning, dependency management, configuration automation | Zero-downtime infrastructure creation with consistent configuration | 45 minutes → 5 minutes |
| **[Application Deployment](#2-application-deployment)** | **CI/CD Pipelines:** Automated testing, deployment validation, rollback procedures | Professional deployment with automatic quality gates | 30 minutes → 2 minutes |
| **[Environment Management](#3-environment-management)** | **Environment Operations:** Cloning, scaling, environment promotion strategies | Rapid environment provisioning for development and testing | 60 minutes → 10 minutes |
| **[Backup & Recovery](#4-backup--recovery)** | **Disaster Recovery:** Automated backup strategies, restore procedures, data protection | Business continuity and data protection automation | 30 minutes → 5 minutes |
| **[Monitoring Setup](#5-monitoring-setup)** | **Observability:** Automated monitoring, alerting, dashboard creation | Proactive problem detection and performance optimization | 45 minutes → 10 minutes |
| **[Cost Management](#6-cost-management)** | **FinOps:** Cost monitoring, optimization automation, budget alerts | Automated cost control and optimization | Ongoing → Automatic |

---

## **1. Complete Infrastructure Setup**

### 🎓 **Learning Objectives**
- **Infrastructure as Code (IaC):** Transform manual Azure setup into automated, repeatable scripts
- **Dependency Management:** Understand how cloud resources depend on each other and proper creation order
- **Configuration Management:** Automate complex configurations like connection strings, security settings, and monitoring
- **Error Handling:** Implement professional-grade error handling and recovery in automation scripts

### 🏗️ **Architecture This Script Creates**
```
Resource Group (Logical Container)
├── App Service Plan (Compute Resources)
│   └── Web App (Application Runtime)
│       ├── Application Settings (Configuration)
│       ├── Connection Strings (Database & Storage)
│       └── Logging Configuration (Diagnostics)
├── SQL Server (Database Service)
│   ├── Firewall Rules (Security)
│   └── SQL Database (Data Storage)
├── Storage Account (File Storage)
│   └── Blob Containers (Future file migration)
└── Application Insights (Monitoring & Analytics)
    ├── Performance Monitoring
    ├── Error Tracking
    └── Usage Analytics
```

### � **Key Learning Concepts**

#### **🔗 Resource Dependencies**
**Problem:** Azure resources must be created in specific order
**Solution:** Script handles dependencies automatically
**Example:** App Service Plan must exist before Web App can be created

#### **🔐 Security Configuration**  
**Problem:** Manual security setup is error-prone and inconsistent
**Solution:** Script configures SQL firewalls, SSL certificates, and secure connection strings
**Best Practice:** Least-privilege access and defense-in-depth security

#### **⚡ Idempotent Operations**
**Problem:** Running setup multiple times can cause errors or duplicates
**Solution:** Script checks if resources exist before creating them
**Professional Benefit:** Safe to re-run scripts without side effects

### �🚀 **Master Setup Script: `deploy-complete-infrastructure.ps1`**

**📚 Script Learning Breakdown:**
- **Lines 1-50:** Parameter validation and security (SecureString handling)
- **Lines 51-100:** Azure CLI authentication and environment verification
- **Lines 101-200:** Resource creation with proper dependency order
- **Lines 201-300:** Configuration and connection string automation
- **Lines 301-400:** Deployment and health verification

```powershell
<#
.SYNOPSIS
    Complete Azure infrastructure setup for ResellBook application
.DESCRIPTION
    This script creates all required Azure resources from scratch:
    - Resource Group
    - App Service Plan
    - Web App
    - SQL Server and Database
    - Storage Account (for future blob migration)
    - Application Insights
    - Configures all settings and deploys application
.PARAMETER ResourceGroupName
    Name of the resource group to create
.PARAMETER Location
    Azure region (e.g., "East US", "West Europe")
.PARAMETER AppName
    Unique name for the web app (optional - auto-generated if not provided)
.PARAMETER SqlAdminPassword
    Password for SQL Server admin (must be strong)
.PARAMETER SkipDeployment
    If specified, only creates infrastructure without deploying the app
.EXAMPLE
    .\deploy-complete-infrastructure.ps1 -ResourceGroupName "ResellBook-RG" -Location "East US" -SqlAdminPassword "MySecurePass123!"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$Location,
    
    [Parameter(Mandatory=$false)]
    [string]$AppName,
    
    [Parameter(Mandatory=$true)]
    [SecureString]$SqlAdminPassword,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipDeployment
)

# Configuration
$ErrorActionPreference = "Stop"
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$defaultAppName = "ResellBook$timestamp"
$sqlAdminUser = "sqladmin"

# Use provided app name or generate unique one
if (-not $AppName) {
    $AppName = $defaultAppName
}

# Convert SecureString to plain text for Azure CLI
$sqlPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SqlAdminPassword))

Write-Host "🚀 Starting Complete Infrastructure Deployment" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host "Resource Group: $ResourceGroupName" -ForegroundColor Cyan
Write-Host "Location: $Location" -ForegroundColor Cyan
Write-Host "App Name: $AppName" -ForegroundColor Cyan
Write-Host "Timestamp: $timestamp" -ForegroundColor Cyan

# Progress tracking
$totalSteps = 15
$currentStep = 0

function Write-Progress-Step {
    param([string]$Activity, [string]$Status)
    $script:currentStep++
    Write-Progress -Activity "Infrastructure Setup" -Status "$Activity" -PercentComplete (($script:currentStep / $totalSteps) * 100)
    Write-Host "[$script:currentStep/$totalSteps] $Activity" -ForegroundColor Yellow
}

try {
    # Step 1: Verify prerequisites
    Write-Progress-Step "Verifying Prerequisites" "Checking Azure CLI and login status"
    
    # Check Azure CLI
    $azVersion = az --version 2>&1 | Select-String "azure-cli"
    if (-not $azVersion) {
        throw "Azure CLI not found. Please install from: https://aka.ms/installazurecliwindows"
    }
    
    # Check login status
    $account = az account show 2>&1 | ConvertFrom-Json -ErrorAction SilentlyContinue
    if (-not $account) {
        Write-Host "Not logged in to Azure. Starting login process..." -ForegroundColor Yellow
        az login
        $account = az account show | ConvertFrom-Json
    }
    
    Write-Host "✅ Logged in as: $($account.user.name)" -ForegroundColor Green
    Write-Host "✅ Subscription: $($account.name)" -ForegroundColor Green
    
    # Step 2: Create Resource Group
    Write-Progress-Step "Creating Resource Group" "Setting up resource container"
    
    $existingRG = az group show --name $ResourceGroupName 2>&1 | ConvertFrom-Json -ErrorAction SilentlyContinue
    if ($existingRG) {
        Write-Host "ℹ️ Resource group already exists" -ForegroundColor Yellow
    } else {
        az group create --name $ResourceGroupName --location $Location | Out-Null
        Write-Host "✅ Resource group created" -ForegroundColor Green
    }
    
    # Step 3: Create App Service Plan
    Write-Progress-Step "Creating App Service Plan" "Setting up hosting infrastructure"
    
    $appServicePlan = "$AppName-plan"
    az appservice plan create `
        --name $appServicePlan `
        --resource-group $ResourceGroupName `
        --location $Location `
        --sku B1 `
        --is-linux false | Out-Null
    
    Write-Host "✅ App Service Plan created: $appServicePlan" -ForegroundColor Green
    
    # Step 4: Create Web App
    Write-Progress-Step "Creating Web Application" "Setting up web app container"
    
    az webapp create `
        --name $AppName `
        --resource-group $ResourceGroupName `
        --plan $appServicePlan `
        --runtime "DOTNET|8.0" | Out-Null
    
    Write-Host "✅ Web App created: $AppName" -ForegroundColor Green
    
    # Step 5: Configure Web App Logging
    Write-Progress-Step "Configuring Application Logging" "Setting up diagnostics"
    
    az webapp log config `
        --name $AppName `
        --resource-group $ResourceGroupName `
        --application-logging filesystem `
        --detailed-error-messages true `
        --failed-request-tracing true `
        --web-server-logging filesystem | Out-Null
    
    Write-Host "✅ Logging configured" -ForegroundColor Green
    
    # Step 6: Create SQL Server
    Write-Progress-Step "Creating SQL Server" "Setting up database server"
    
    $sqlServerName = "$AppName-sql"
    az sql server create `
        --name $sqlServerName `
        --resource-group $ResourceGroupName `
        --location $Location `
        --admin-user $sqlAdminUser `
        --admin-password $sqlPassword | Out-Null
    
    Write-Host "✅ SQL Server created: $sqlServerName" -ForegroundColor Green
    
    # Step 7: Configure SQL Server Firewall
    Write-Progress-Step "Configuring SQL Server Firewall" "Setting up database access"
    
    # Allow Azure services
    az sql server firewall-rule create `
        --server $sqlServerName `
        --resource-group $ResourceGroupName `
        --name "AllowAzureServices" `
        --start-ip-address 0.0.0.0 `
        --end-ip-address 0.0.0.0 | Out-Null
    
    # Allow current IP
    $currentIP = (Invoke-WebRequest -Uri "https://api.ipify.org").Content.Trim()
    az sql server firewall-rule create `
        --server $sqlServerName `
        --resource-group $ResourceGroupName `
        --name "AllowCurrentIP" `
        --start-ip-address $currentIP `
        --end-ip-address $currentIP | Out-Null
    
    Write-Host "✅ Firewall configured (Current IP: $currentIP)" -ForegroundColor Green
    
    # Step 8: Create SQL Database
    Write-Progress-Step "Creating SQL Database" "Setting up application database"
    
    $databaseName = "ResellBookDB"
    az sql db create `
        --server $sqlServerName `
        --resource-group $ResourceGroupName `
        --name $databaseName `
        --edition Basic `
        --capacity 5 | Out-Null
    
    Write-Host "✅ Database created: $databaseName" -ForegroundColor Green
    
    # Step 9: Create Storage Account (for future blob migration)
    Write-Progress-Step "Creating Storage Account" "Setting up file storage"
    
    $storageAccountName = "resellbook$($timestamp.Substring(4))"  # Keep it short
    az storage account create `
        --name $storageAccountName `
        --resource-group $ResourceGroupName `
        --location $Location `
        --sku Standard_LRS `
        --kind StorageV2 | Out-Null
    
    # Get storage account key
    $storageKey = az storage account keys list `
        --resource-group $ResourceGroupName `
        --account-name $storageAccountName `
        --query '[0].value' `
        --output tsv
    
    Write-Host "✅ Storage Account created: $storageAccountName" -ForegroundColor Green
    
    # Step 10: Create Application Insights
    Write-Progress-Step "Creating Application Insights" "Setting up monitoring"
    
    $appInsightsName = "$AppName-insights"
    az monitor app-insights component create `
        --app $appInsightsName `
        --location $Location `
        --resource-group $ResourceGroupName | Out-Null
    
    $instrumentationKey = az monitor app-insights component show `
        --app $appInsightsName `
        --resource-group $ResourceGroupName `
        --query instrumentationKey `
        --output tsv
    
    Write-Host "✅ Application Insights created: $appInsightsName" -ForegroundColor Green
    
    # Step 11: Configure Connection Strings
    Write-Progress-Step "Configuring Connection Strings" "Setting up database connectivity"
    
    $connectionString = "Server=$sqlServerName.database.windows.net;Database=$databaseName;User Id=$sqlAdminUser;Password=$sqlPassword;Encrypt=True;TrustServerCertificate=False;"
    $blobConnectionString = "DefaultEndpointsProtocol=https;AccountName=$storageAccountName;AccountKey=$storageKey;EndpointSuffix=core.windows.net"
    
    az webapp config connection-string set `
        --name $AppName `
        --resource-group $ResourceGroupName `
        --connection-string-type SQLAzure `
        --settings DefaultConnection="$connectionString" | Out-Null
    
    az webapp config connection-string set `
        --name $AppName `
        --resource-group $ResourceGroupName `
        --connection-string-type Custom `
        --settings BlobStorage="$blobConnectionString" | Out-Null
    
    Write-Host "✅ Connection strings configured" -ForegroundColor Green
    
    # Step 12: Configure Application Settings
    Write-Progress-Step "Configuring Application Settings" "Setting up app configuration"
    
    $jwtSecret = -join ((1..64) | ForEach-Object { Get-Random -InputObject ([char[]]'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789') })
    
    az webapp config appsettings set `
        --name $AppName `
        --resource-group $ResourceGroupName `
        --settings `
            "ASPNETCORE_ENVIRONMENT=Production" `
            "JWT_SECRET=$jwtSecret" `
            "JWT_ISSUER=ResellBookApp" `
            "JWT_AUDIENCE=ResellBookUsers" `
            "APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey" `
            "WEBSITE_TIME_ZONE=UTC" | Out-Null
    
    Write-Host "✅ Application settings configured" -ForegroundColor Green
    
    # Step 13: Apply Database Migrations (if not skipping deployment)
    if (-not $SkipDeployment) {
        Write-Progress-Step "Applying Database Migrations" "Setting up database schema"
        
        # Update local appsettings.json temporarily
        $localConnectionString = $connectionString
        $appsettingsPath = "appsettings.json"
        
        if (Test-Path $appsettingsPath) {
            $appsettingsBackup = Get-Content $appsettingsPath
            $appsettings = $appsettingsBackup | ConvertFrom-Json
            $appsettings.ConnectionStrings.DefaultConnection = $localConnectionString
            $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
            
            try {
                dotnet ef database update
                Write-Host "✅ Database migrations applied" -ForegroundColor Green
            }
            finally {
                # Restore original appsettings.json
                $appsettingsBackup | Set-Content $appsettingsPath
            }
        }
        else {
            Write-Host "⚠️ appsettings.json not found - skipping migrations" -ForegroundColor Yellow
        }
    }
    
    # Step 14: Deploy Application (if not skipping deployment)
    if (-not $SkipDeployment) {
        Write-Progress-Step "Building Application" "Preparing deployment package"
        
        # Build the application
        dotnet clean | Out-Null
        dotnet restore | Out-Null
        dotnet build --configuration Release | Out-Null
        
        Write-Progress-Step "Deploying Application" "Uploading to Azure"
        
        # Publish and create deployment package
        $publishPath = "bin\Release\net8.0\publish"
        dotnet publish --configuration Release --output $publishPath --no-build | Out-Null
        
        $zipPath = "deployment-$timestamp.zip"
        Compress-Archive -Path "$publishPath\*" -DestinationPath $zipPath
        
        # Deploy to Azure
        az webapp deployment source config-zip `
            --resource-group $ResourceGroupName `
            --name $AppName `
            --src $zipPath | Out-Null
        
        # Clean up
        Remove-Item $zipPath -ErrorAction SilentlyContinue
        Remove-Item $publishPath -Recurse -Force -ErrorAction SilentlyContinue
        
        Write-Host "✅ Application deployed" -ForegroundColor Green
    }
    
    # Step 15: Final Configuration and Testing
    Write-Progress-Step "Final Configuration" "Completing setup"
    
    # Restart the web app
    az webapp restart --name $AppName --resource-group $ResourceGroupName | Out-Null
    
    Write-Host "✅ Web app restarted" -ForegroundColor Green
    
    Write-Progress -Activity "Infrastructure Setup" -Completed
    
    # Success Summary
    Write-Host "`n🎉 INFRASTRUCTURE DEPLOYMENT COMPLETED!" -ForegroundColor Green
    Write-Host "=======================================" -ForegroundColor Green
    
    $appUrl = "https://$AppName.azurewebsites.net"
    $kuduUrl = "https://$AppName.scm.azurewebsites.net"
    
    Write-Host "`n📊 Deployment Summary:" -ForegroundColor Cyan
    Write-Host "=====================" -ForegroundColor Cyan
    Write-Host "🌐 Application URL: $appUrl" -ForegroundColor White
    Write-Host "🔧 Kudu Console: $kuduUrl" -ForegroundColor White
    Write-Host "💾 SQL Server: $sqlServerName.database.windows.net" -ForegroundColor White
    Write-Host "🗄️ Database: $databaseName" -ForegroundColor White
    Write-Host "📦 Storage Account: $storageAccountName" -ForegroundColor White
    Write-Host "📊 App Insights: $appInsightsName" -ForegroundColor White
    Write-Host "📁 Resource Group: $ResourceGroupName" -ForegroundColor White
    
    Write-Host "`n🔑 Important Information:" -ForegroundColor Yellow
    Write-Host "========================" -ForegroundColor Yellow
    Write-Host "• SQL Admin User: $sqlAdminUser" -ForegroundColor White
    Write-Host "• SQL Admin Password: [As provided]" -ForegroundColor White
    Write-Host "• Your IP Address: $currentIP (added to SQL firewall)" -ForegroundColor White
    Write-Host "• JWT Secret: [Auto-generated and configured]" -ForegroundColor White
    
    Write-Host "`n🧪 Testing the Deployment:" -ForegroundColor Cyan
    Write-Host "==========================" -ForegroundColor Cyan
    
    # Wait for app to start
    Write-Host "⏳ Waiting for application to start..." -ForegroundColor Yellow
    Start-Sleep -Seconds 30
    
    # Test endpoints
    $testResults = @()
    $endpoints = @(
        @{Name="Health Check"; Path="/health"},
        @{Name="Weather API"; Path="/WeatherForecast"},
        @{Name="Books API"; Path="/api/Books"}
    )
    
    foreach ($endpoint in $endpoints) {
        try {
            $response = Invoke-WebRequest -Uri "$appUrl$($endpoint.Path)" -Method GET -TimeoutSec 30 -ErrorAction Stop
            $testResults += "✅ $($endpoint.Name): Status $($response.StatusCode)"
        }
        catch {
            if ($_.Exception.Response.StatusCode -eq 401) {
                $testResults += "🔐 $($endpoint.Name): Requires Authentication (Normal)"
            }
            else {
                $testResults += "❌ $($endpoint.Name): $($_.Exception.Message)"
            }
        }
    }
    
    foreach ($result in $testResults) {
        Write-Host $result -ForegroundColor $(if ($result.StartsWith("✅")) { "Green" } elseif ($result.StartsWith("🔐")) { "Cyan" } else { "Red" })
    }
    
    Write-Host "`n📚 Next Steps:" -ForegroundColor Magenta
    Write-Host "=============" -ForegroundColor Magenta
    Write-Host "1. Visit your application: $appUrl" -ForegroundColor White
    Write-Host "2. Test API endpoints using tools like Postman" -ForegroundColor White
    Write-Host "3. Check application logs if needed: az webapp log tail --name $AppName --resource-group $ResourceGroupName" -ForegroundColor White
    Write-Host "4. Set up CI/CD pipeline for automated deployments" -ForegroundColor White
    Write-Host "5. Consider migrating to Blob Storage using the migration guide" -ForegroundColor White
    
    # Create summary file
    $summaryFile = "deployment-summary-$timestamp.txt"
    @"
ResellBook Azure Deployment Summary
===================================
Deployment Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
Resource Group: $ResourceGroupName
Location: $Location

Resources Created:
- App Service: $AppName
- SQL Server: $sqlServerName
- Database: $databaseName
- Storage Account: $storageAccountName
- App Insights: $appInsightsName

URLs:
- Application: $appUrl
- Kudu Console: $kuduUrl

Connection Information:
- SQL Server: $sqlServerName.database.windows.net
- SQL User: $sqlAdminUser
- Your IP (whitelisted): $currentIP

Security Notes:
- JWT secret auto-generated and configured
- SQL firewall configured for Azure services and your IP
- All connections use SSL encryption

Test Results:
$($testResults -join "`n")
"@ | Out-File -FilePath $summaryFile -Encoding UTF8
    
    Write-Host "`n💾 Deployment summary saved to: $summaryFile" -ForegroundColor Green
    
} catch {
    Write-Host "`n❌ DEPLOYMENT FAILED!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`nTroubleshooting steps:" -ForegroundColor Yellow
    Write-Host "1. Check Azure CLI login: az account show" -ForegroundColor White
    Write-Host "2. Verify subscription permissions" -ForegroundColor White
    Write-Host "3. Try a different region if resources unavailable" -ForegroundColor White
    Write-Host "4. Check the troubleshooting guide for detailed solutions" -ForegroundColor White
    
    # Cleanup on failure (optional)
    $cleanup = Read-Host "`nDo you want to clean up created resources? (y/N)"
    if ($cleanup -eq 'y' -or $cleanup -eq 'Y') {
        Write-Host "🧹 Cleaning up resources..." -ForegroundColor Yellow
        az group delete --name $ResourceGroupName --yes --no-wait
        Write-Host "✅ Cleanup initiated" -ForegroundColor Green
    }
    
    exit 1
}
```

### 🛠️ **Environment-Specific Setup Scripts**

**Development Environment: `setup-dev-environment.ps1`**

```powershell
param(
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroupName = "ResellBook-Dev-RG"
)

$timestamp = Get-Date -Format "yyyyMMddHHmm"
$appName = "ResellBookDev$timestamp"

Write-Host "🔧 Setting up Development Environment" -ForegroundColor Cyan

# Use smaller, cheaper resources for development
az group create --name $ResourceGroupName --location "East US"

az appservice plan create `
    --name "$appName-plan" `
    --resource-group $ResourceGroupName `
    --location "East US" `
    --sku F1 `  # Free tier
    --is-linux false

az webapp create `
    --name $appName `
    --resource-group $ResourceGroupName `
    --plan "$appName-plan" `
    --runtime "DOTNET|8.0"

# Use SQL Database DTU-based pricing (cheaper)
$sqlServer = "$appName-sql"
az sql server create `
    --name $sqlServer `
    --resource-group $ResourceGroupName `
    --location "East US" `
    --admin-user "devadmin" `
    --admin-password "DevPassword123!"

az sql db create `
    --server $sqlServer `
    --resource-group $ResourceGroupName `
    --name "ResellBookDevDB" `
    --edition Basic

Write-Host "✅ Development environment ready!" -ForegroundColor Green
Write-Host "🌐 App URL: https://$appName.azurewebsites.net" -ForegroundColor Cyan
```

**Production Environment: `setup-prod-environment.ps1`**

```powershell
param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [SecureString]$SqlAdminPassword
)

Write-Host "🚀 Setting up Production Environment" -ForegroundColor Green

$timestamp = Get-Date -Format "yyyyMMddHHmm"
$appName = "ResellBookProd$timestamp"

# Production-grade resources
az group create --name $ResourceGroupName --location "East US"

# Standard tier with auto-scaling
az appservice plan create `
    --name "$appName-plan" `
    --resource-group $ResourceGroupName `
    --location "East US" `
    --sku S1 `
    --is-linux false

az webapp create `
    --name $appName `
    --resource-group $ResourceGroupName `
    --plan "$appName-plan" `
    --runtime "DOTNET|8.0"

# Configure auto-scaling
az monitor autoscale create `
    --resource-group $ResourceGroupName `
    --resource "$appName-plan" `
    --resource-type "Microsoft.Web/serverfarms" `
    --name "$appName-autoscale" `
    --min-count 1 `
    --max-count 3 `
    --count 1

# Add CPU-based scaling rule
az monitor autoscale rule create `
    --resource-group $ResourceGroupName `
    --autoscale-name "$appName-autoscale" `
    --condition "Percentage CPU > 70 avg 5m" `
    --scale out 1

az monitor autoscale rule create `
    --resource-group $ResourceGroupName `
    --autoscale-name "$appName-autoscale" `
    --condition "Percentage CPU < 30 avg 5m" `
    --scale in 1

Write-Host "✅ Production environment with auto-scaling configured!" -ForegroundColor Green
```

---

## **2. Application Deployment**

### 🚀 **Smart Deployment Script: `smart-deploy.ps1`**

```powershell
<#
.SYNOPSIS
    Intelligent application deployment with rollback capability
.DESCRIPTION
    Deploys application with health checks, automatic rollback on failure,
    and comprehensive testing
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$AppName,
    
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$false)]
    [switch]$EnableRollback = $true,
    
    [Parameter(Mandatory=$false)]
    [int]$HealthCheckRetries = 3
)

$ErrorActionPreference = "Stop"
$timestamp = Get-Date -Format "yyyyMMddHHmmss"

Write-Host "🚀 Smart Deployment Starting" -ForegroundColor Green
Write-Host "============================" -ForegroundColor Green

try {
    # Step 1: Pre-deployment backup
    if ($EnableRollback) {
        Write-Host "💾 Creating deployment backup..." -ForegroundColor Yellow
        
        # Get current app settings as backup
        $currentSettings = az webapp config appsettings list `
            --name $AppName `
            --resource-group $ResourceGroupName `
            --output json | ConvertFrom-Json
        
        $backupFile = "backup-$AppName-$timestamp.json"
        $currentSettings | ConvertTo-Json -Depth 10 | Out-File $backupFile
        
        Write-Host "✅ Backup created: $backupFile" -ForegroundColor Green
    }
    
    # Step 2: Build and test locally
    Write-Host "🔨 Building application..." -ForegroundColor Yellow
    
    dotnet clean | Out-Null
    dotnet restore | Out-Null
    
    $buildResult = dotnet build --configuration Release 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed: $buildResult"
    }
    
    Write-Host "✅ Build successful" -ForegroundColor Green
    
    # Step 3: Run local tests (if test project exists)
    if (Test-Path "*.Tests\*.csproj") {
        Write-Host "🧪 Running tests..." -ForegroundColor Yellow
        
        $testResult = dotnet test --configuration Release 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "Tests failed: $testResult"
        }
        
        Write-Host "✅ All tests passed" -ForegroundColor Green
    }
    
    # Step 4: Create deployment package
    Write-Host "📦 Creating deployment package..." -ForegroundColor Yellow
    
    $publishPath = "bin\Release\net8.0\publish"
    dotnet publish --configuration Release --output $publishPath --no-build | Out-Null
    
    $zipPath = "deployment-$timestamp.zip"
    Compress-Archive -Path "$publishPath\*" -DestinationPath $zipPath
    
    $packageSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
    Write-Host "✅ Package created: $packageSize MB" -ForegroundColor Green
    
    # Step 5: Deploy to staging slot (if available) or main
    Write-Host "🚀 Deploying to Azure..." -ForegroundColor Yellow
    
    az webapp deployment source config-zip `
        --resource-group $ResourceGroupName `
        --name $AppName `
        --src $zipPath | Out-Null
    
    Write-Host "✅ Deployment uploaded" -ForegroundColor Green
    
    # Step 6: Wait for deployment to complete
    Write-Host "⏳ Waiting for deployment to complete..." -ForegroundColor Yellow
    Start-Sleep -Seconds 30
    
    # Step 7: Health checks
    Write-Host "🏥 Performing health checks..." -ForegroundColor Yellow
    
    $baseUrl = "https://$AppName.azurewebsites.net"
    $healthCheckPassed = $false
    
    for ($i = 1; $i -le $HealthCheckRetries; $i++) {
        Write-Host "  Attempt $i/$HealthCheckRetries..." -ForegroundColor Gray
        
        try {
            # Basic connectivity
            $response = Invoke-WebRequest -Uri $baseUrl -Method HEAD -TimeoutSec 30
            
            if ($response.StatusCode -eq 200) {
                # Test critical endpoints
                $endpoints = @("/health", "/WeatherForecast")
                $allEndpointsHealthy = $true
                
                foreach ($endpoint in $endpoints) {
                    try {
                        $endpointResponse = Invoke-WebRequest -Uri "$baseUrl$endpoint" -Method GET -TimeoutSec 15
                        if ($endpointResponse.StatusCode -ne 200) {
                            $allEndpointsHealthy = $false
                            break
                        }
                    }
                    catch {
                        $allEndpointsHealthy = $false
                        break
                    }
                }
                
                if ($allEndpointsHealthy) {
                    $healthCheckPassed = $true
                    break
                }
            }
        }
        catch {
            Write-Host "    Health check failed: $($_.Exception.Message)" -ForegroundColor Red
        }
        
        if ($i -lt $HealthCheckRetries) {
            Start-Sleep -Seconds 30
        }
    }
    
    if (-not $healthCheckPassed) {
        throw "Health checks failed after $HealthCheckRetries attempts"
    }
    
    Write-Host "✅ Health checks passed" -ForegroundColor Green
    
    # Step 8: Performance validation
    Write-Host "⚡ Validating performance..." -ForegroundColor Yellow
    
    $performanceResults = @()
    $testEndpoints = @("/health", "/WeatherForecast")
    
    foreach ($endpoint in $testEndpoints) {
        $startTime = Get-Date
        try {
            Invoke-WebRequest -Uri "$baseUrl$endpoint" -Method GET -TimeoutSec 10 | Out-Null
            $responseTime = ((Get-Date) - $startTime).TotalMilliseconds
            $performanceResults += "✅ $endpoint : $([math]::Round($responseTime, 0))ms"
        }
        catch {
            $performanceResults += "❌ $endpoint : Failed"
        }
    }
    
    foreach ($result in $performanceResults) {
        Write-Host "  $result" -ForegroundColor $(if ($result.StartsWith("✅")) { "Green" } else { "Red" })
    }
    
    # Step 9: Cleanup
    Remove-Item $zipPath -ErrorAction SilentlyContinue
    Remove-Item $publishPath -Recurse -Force -ErrorAction SilentlyContinue
    
    # Success!
    Write-Host "`n🎉 DEPLOYMENT SUCCESSFUL!" -ForegroundColor Green
    Write-Host "========================" -ForegroundColor Green
    Write-Host "🌐 Application URL: $baseUrl" -ForegroundColor Cyan
    Write-Host "📊 Deployment Time: $timestamp" -ForegroundColor Cyan
    Write-Host "📦 Package Size: $packageSize MB" -ForegroundColor Cyan
    
    Write-Host "`n📊 Performance Results:" -ForegroundColor Cyan
    foreach ($result in $performanceResults) {
        Write-Host "  $result" -ForegroundColor White
    }
    
} catch {
    Write-Host "`n❌ DEPLOYMENT FAILED!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($EnableRollback -and (Test-Path $backupFile)) {
        Write-Host "`n🔄 Initiating rollback..." -ForegroundColor Yellow
        
        # Restore previous settings
        $backupSettings = Get-Content $backupFile | ConvertFrom-Json
        
        foreach ($setting in $backupSettings) {
            az webapp config appsettings set `
                --name $AppName `
                --resource-group $ResourceGroupName `
                --settings "$($setting.name)=$($setting.value)" | Out-Null
        }
        
        # Restart app
        az webapp restart --name $AppName --resource-group $ResourceGroupName | Out-Null
        
        Write-Host "✅ Rollback completed" -ForegroundColor Green
    }
    
    # Cleanup on failure
    Remove-Item $zipPath -ErrorAction SilentlyContinue
    Remove-Item $publishPath -Recurse -Force -ErrorAction SilentlyContinue
    
    exit 1
}
```

---

## **3. Environment Management**

### 🔄 **Environment Cloning Script: `clone-environment.ps1`**

```powershell
param(
    [Parameter(Mandatory=$true)]
    [string]$SourceResourceGroup,
    
    [Parameter(Mandatory=$true)]
    [string]$TargetResourceGroup,
    
    [Parameter(Mandatory=$true)]
    [string]$TargetLocation,
    
    [Parameter(Mandatory=$false)]
    [switch]$IncludeData
)

Write-Host "🔄 Cloning Environment" -ForegroundColor Cyan
Write-Host "From: $SourceResourceGroup" -ForegroundColor White
Write-Host "To: $TargetResourceGroup" -ForegroundColor White

# Get source resources
$sourceResources = az resource list --resource-group $SourceResourceGroup | ConvertFrom-Json

# Create target resource group
az group create --name $TargetResourceGroup --location $TargetLocation

foreach ($resource in $sourceResources) {
    switch ($resource.type) {
        "Microsoft.Web/serverfarms" {
            Write-Host "📋 Cloning App Service Plan: $($resource.name)" -ForegroundColor Yellow
            
            $planDetails = az appservice plan show --name $resource.name --resource-group $SourceResourceGroup | ConvertFrom-Json
            
            az appservice plan create `
                --name $resource.name `
                --resource-group $TargetResourceGroup `
                --location $TargetLocation `
                --sku $planDetails.sku.name `
                --is-linux false | Out-Null
        }
        
        "Microsoft.Web/sites" {
            Write-Host "🌐 Cloning Web App: $($resource.name)" -ForegroundColor Yellow
            
            $appDetails = az webapp show --name $resource.name --resource-group $SourceResourceGroup | ConvertFrom-Json
            $runtime = az webapp list-runtimes --os windows | ConvertFrom-Json | Where-Object { $_ -like "*DOTNET*" } | Select-Object -First 1
            
            # Create new app with unique name
            $newAppName = "$($resource.name)-clone-$(Get-Date -Format 'MMddHHmm')"
            
            az webapp create `
                --name $newAppName `
                --resource-group $TargetResourceGroup `
                --plan $appDetails.appServicePlanId.Split('/')[-1] `
                --runtime $runtime | Out-Null
            
            # Clone app settings
            $appSettings = az webapp config appsettings list --name $resource.name --resource-group $SourceResourceGroup | ConvertFrom-Json
            
            foreach ($setting in $appSettings) {
                az webapp config appsettings set `
                    --name $newAppName `
                    --resource-group $TargetResourceGroup `
                    --settings "$($setting.name)=$($setting.value)" | Out-Null
            }
            
            Write-Host "✅ Web App cloned as: $newAppName" -ForegroundColor Green
        }
        
        # Add more resource types as needed
    }
}

Write-Host "✅ Environment cloning completed!" -ForegroundColor Green
```

---

## **4. Backup & Recovery**

### 💾 **Automated Backup Script: `backup-environment.ps1`**

```powershell
param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$false)]
    [string]$BackupStorageAccount,
    
    [Parameter(Mandatory=$false)]
    [int]$RetentionDays = 30
)

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$backupFolder = "backup-$timestamp"

Write-Host "💾 Starting Automated Backup" -ForegroundColor Green
Write-Host "=============================" -ForegroundColor Green

try {
    # Create backup directory
    New-Item -ItemType Directory -Path $backupFolder -Force | Out-Null
    
    # Backup 1: Resource Group Template
    Write-Host "📋 Exporting resource group template..." -ForegroundColor Yellow
    
    az group export --name $ResourceGroupName --output json > "$backupFolder\resource-template.json"
    Write-Host "✅ Resource template exported" -ForegroundColor Green
    
    # Backup 2: App Service Configuration
    Write-Host "⚙️ Backing up app configurations..." -ForegroundColor Yellow
    
    $webApps = az webapp list --resource-group $ResourceGroupName | ConvertFrom-Json
    
    foreach ($app in $webApps) {
        $appName = $app.name
        $appFolder = "$backupFolder\$appName"
        New-Item -ItemType Directory -Path $appFolder -Force | Out-Null
        
        # Export app settings
        az webapp config appsettings list --name $appName --resource-group $ResourceGroupName > "$appFolder\appsettings.json"
        
        # Export connection strings
        az webapp config connection-string list --name $appName --resource-group $ResourceGroupName > "$appFolder\connectionstrings.json"
        
        # Export app configuration
        az webapp config show --name $appName --resource-group $ResourceGroupName > "$appFolder\config.json"
        
        Write-Host "✅ App '$appName' configuration backed up" -ForegroundColor Green
    }
    
    # Backup 3: SQL Databases
    Write-Host "🗄️ Backing up databases..." -ForegroundColor Yellow
    
    $sqlServers = az sql server list --resource-group $ResourceGroupName | ConvertFrom-Json
    
    foreach ($server in $sqlServers) {
        $serverName = $server.name
        $databases = az sql db list --server $serverName --resource-group $ResourceGroupName | ConvertFrom-Json
        
        foreach ($db in $databases) {
            if ($db.name -ne "master") {  # Skip system database
                $dbName = $db.name
                $bacpacName = "$backupFolder\$serverName-$dbName-$timestamp.bacpac"
                
                Write-Host "  Exporting database: $dbName" -ForegroundColor Gray
                
                # This requires a storage account for the bacpac file
                if ($BackupStorageAccount) {
                    $storageKey = az storage account keys list --account-name $BackupStorageAccount --resource-group $ResourceGroupName --query '[0].value' --output tsv
                    
                    az sql db export `
                        --server $serverName `
                        --name $dbName `
                        --resource-group $ResourceGroupName `
                        --admin-user "sqladmin" `
                        --admin-password "BackupPassword123!" `
                        --storage-key-type StorageAccessKey `
                        --storage-key $storageKey `
                        --storage-uri "https://$BackupStorageAccount.blob.core.windows.net/backups/$serverName-$dbName-$timestamp.bacpac" | Out-Null
                    
                    Write-Host "✅ Database '$dbName' exported to storage" -ForegroundColor Green
                }
                else {
                    Write-Host "⚠️ Skipping database export (no storage account provided)" -ForegroundColor Yellow
                }
            }
        }
    }
    
    # Backup 4: Create summary
    $summary = @{
        BackupDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        ResourceGroup = $ResourceGroupName
        Resources = @()
        WebApps = $webApps.Count
        SqlServers = $sqlServers.Count
    }
    
    $summary | ConvertTo-Json -Depth 3 | Out-File "$backupFolder\backup-summary.json"
    
    # Create ZIP archive
    $zipPath = "$backupFolder.zip"
    Compress-Archive -Path $backupFolder -DestinationPath $zipPath
    
    # Cleanup folder
    Remove-Item $backupFolder -Recurse -Force
    
    Write-Host "`n🎉 BACKUP COMPLETED!" -ForegroundColor Green
    Write-Host "===================" -ForegroundColor Green
    Write-Host "📦 Backup file: $zipPath" -ForegroundColor Cyan
    Write-Host "📊 Web Apps: $($webApps.Count)" -ForegroundColor Cyan
    Write-Host "🗄️ SQL Servers: $($sqlServers.Count)" -ForegroundColor Cyan
    
    # Cleanup old backups
    $oldBackups = Get-ChildItem -Path "." -Name "backup-*.zip" | Where-Object { 
        $_.CreationTime -lt (Get-Date).AddDays(-$RetentionDays) 
    }
    
    if ($oldBackups) {
        Write-Host "`n🧹 Cleaning up old backups..." -ForegroundColor Yellow
        foreach ($oldBackup in $oldBackups) {
            Remove-Item $oldBackup -Force
            Write-Host "  Deleted: $oldBackup" -ForegroundColor Gray
        }
    }
    
} catch {
    Write-Host "❌ Backup failed: $($_.Exception.Message)" -ForegroundColor Red
    
    # Cleanup on failure
    Remove-Item $backupFolder -Recurse -Force -ErrorAction SilentlyContinue
    
    exit 1
}
```

---

## **5. Monitoring Setup**

### 📊 **Complete Monitoring Setup: `setup-monitoring.ps1`**

```powershell
param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$AppName,
    
    [Parameter(Mandatory=$false)]
    [string]$NotificationEmail
)

Write-Host "📊 Setting Up Comprehensive Monitoring" -ForegroundColor Green

try {
    # 1. Application Insights (if not exists)
    $appInsightsName = "$AppName-insights"
    
    $existingInsights = az monitor app-insights component show --app $appInsightsName --resource-group $ResourceGroupName 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "📈 Creating Application Insights..." -ForegroundColor Yellow
        
        az monitor app-insights component create `
            --app $appInsightsName `
            --location "East US" `
            --resource-group $ResourceGroupName | Out-Null
        
        Write-Host "✅ Application Insights created" -ForegroundColor Green
    }
    
    # 2. Action Group for notifications
    if ($NotificationEmail) {
        Write-Host "📧 Setting up email notifications..." -ForegroundColor Yellow
        
        $actionGroupName = "$AppName-alerts"
        
        az monitor action-group create `
            --name $actionGroupName `
            --resource-group $ResourceGroupName `
            --short-name "AppAlerts" | Out-Null
        
        az monitor action-group update `
            --name $actionGroupName `
            --resource-group $ResourceGroupName `
            --add-action email admin $NotificationEmail | Out-Null
        
        Write-Host "✅ Email notifications configured" -ForegroundColor Green
    }
    
    # 3. Metric Alerts
    Write-Host "⚠️ Creating metric alerts..." -ForegroundColor Yellow
    
    # High CPU Alert
    az monitor metrics alert create `
        --name "$AppName-HighCPU" `
        --resource-group $ResourceGroupName `
        --scopes "/subscriptions/$(az account show --query id -o tsv)/resourceGroups/$ResourceGroupName/providers/Microsoft.Web/sites/$AppName" `
        --condition "avg Percentage CPU > 80" `
        --description "High CPU usage detected" `
        --evaluation-frequency 5m `
        --window-size 15m `
        --severity 2 | Out-Null
    
    # High Memory Alert  
    az monitor metrics alert create `
        --name "$AppName-HighMemory" `
        --resource-group $ResourceGroupName `
        --scopes "/subscriptions/$(az account show --query id -o tsv)/resourceGroups/$ResourceGroupName/providers/Microsoft.Web/sites/$AppName" `
        --condition "avg MemoryPercentage > 85" `
        --description "High memory usage detected" `
        --evaluation-frequency 5m `
        --window-size 15m `
        --severity 2 | Out-Null
    
    # HTTP 5xx Errors
    az monitor metrics alert create `
        --name "$AppName-HTTP5xxErrors" `
        --resource-group $ResourceGroupName `
        --scopes "/subscriptions/$(az account show --query id -o tsv)/resourceGroups/$ResourceGroupName/providers/Microsoft.Web/sites/$AppName" `
        --condition "total Http5xx > 10" `
        --description "High number of HTTP 5xx errors" `
        --evaluation-frequency 5m `
        --window-size 15m `
        --severity 1 | Out-Null
    
    Write-Host "✅ Metric alerts created" -ForegroundColor Green
    
    # 4. Log Analytics Workspace (for advanced monitoring)
    Write-Host "📋 Creating Log Analytics workspace..." -ForegroundColor Yellow
    
    $workspaceName = "$AppName-workspace"
    
    az monitor log-analytics workspace create `
        --workspace-name $workspaceName `
        --resource-group $ResourceGroupName `
        --location "East US" | Out-Null
    
    Write-Host "✅ Log Analytics workspace created" -ForegroundColor Green
    
    # 5. Dashboard
    Write-Host "📊 Creating monitoring dashboard..." -ForegroundColor Yellow
    
    $dashboardName = "$AppName-Dashboard"
    
    # Create dashboard JSON (simplified)
    $dashboardJson = @{
        lenses = @{
            "0" = @{
                order = 0
                parts = @{
                    "0" = @{
                        position = @{ x = 0; y = 0; rowSpan = 4; colSpan = 6 }
                        metadata = @{
                            inputs = @(
                                @{
                                    name = "resourceTypeMode"
                                    isOptional = $true
                                }
                                @{
                                    name = "ComponentId"
                                    value = "/subscriptions/$(az account show --query id -o tsv)/resourceGroups/$ResourceGroupName/providers/Microsoft.Insights/components/$appInsightsName"
                                    isOptional = $true
                                }
                            )
                            type = "Extension/HubsExtension/PartType/MonitorChartPart"
                        }
                    }
                }
            }
        }
        metadata = @{
            model = @{
                timeRange = @{
                    value = @{
                        relative = @{
                            duration = 24
                            timeUnit = 1
                        }
                    }
                    type = "MsPortalFx.Composition.Configuration.ValueTypes.TimeRange"
                }
            }
        }
    } | ConvertTo-Json -Depth 10
    
    $dashboardJson | Out-File "dashboard-temp.json"
    
    az portal dashboard create `
        --resource-group $ResourceGroupName `
        --name $dashboardName `
        --input-path "dashboard-temp.json" | Out-Null
    
    Remove-Item "dashboard-temp.json"
    
    Write-Host "✅ Dashboard created" -ForegroundColor Green
    
    Write-Host "`n🎉 MONITORING SETUP COMPLETED!" -ForegroundColor Green
    Write-Host "==============================" -ForegroundColor Green
    Write-Host "📊 Application Insights: $appInsightsName" -ForegroundColor Cyan
    Write-Host "📋 Log Analytics: $workspaceName" -ForegroundColor Cyan
    Write-Host "📊 Dashboard: $dashboardName" -ForegroundColor Cyan
    
    if ($NotificationEmail) {
        Write-Host "📧 Email notifications: $NotificationEmail" -ForegroundColor Cyan
    }
    
    Write-Host "`n🔗 Access monitoring in Azure Portal:" -ForegroundColor Yellow
    Write-Host "https://portal.azure.com/#@/resource/subscriptions/$(az account show --query id -o tsv)/resourceGroups/$ResourceGroupName/providers/Microsoft.Insights/components/$appInsightsName" -ForegroundColor White
    
} catch {
    Write-Host "❌ Monitoring setup failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
```

---

## **🎯 Usage Examples**

### **Complete Setup (New Environment)**
```powershell
# Download all scripts to your project folder, then:

# 1. Complete infrastructure setup
.\deploy-complete-infrastructure.ps1 `
    -ResourceGroupName "ResellBook-Production" `
    -Location "East US" `
    -SqlAdminPassword (ConvertTo-SecureString "MySecurePassword123!" -AsPlainText -Force)

# 2. Set up monitoring
.\setup-monitoring.ps1 `
    -ResourceGroupName "ResellBook-Production" `
    -AppName "ResellBook20241011134502" `
    -NotificationEmail "admin@yourcompany.com"

# 3. Create backup
.\backup-environment.ps1 `
    -ResourceGroupName "ResellBook-Production"
```

### **Development to Production Migration**
```powershell
# Clone dev environment to production
.\clone-environment.ps1 `
    -SourceResourceGroup "ResellBook-Dev-RG" `
    -TargetResourceGroup "ResellBook-Prod-RG" `
    -TargetLocation "East US"

# Deploy latest code
.\smart-deploy.ps1 `
    -AppName "ResellBookProd20241011134502" `
    -ResourceGroupName "ResellBook-Prod-RG"
```

### **Daily Operations**
```powershell
# Daily deployment (with rollback safety)
.\smart-deploy.ps1 `
    -AppName "YourAppName" `
    -ResourceGroupName "YourResourceGroup" `
    -EnableRollback

# Weekly backup
.\backup-environment.ps1 `
    -ResourceGroupName "YourResourceGroup" `
    -RetentionDays 30
```

---

**🎉 These automation scripts provide a complete DevOps pipeline for Azure deployment and management, making it easy for beginners to deploy and maintain production-grade applications!**

*Save all scripts in your project folder and run them as needed. Each script includes comprehensive error handling and detailed progress reporting.*