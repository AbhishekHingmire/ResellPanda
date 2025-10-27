param(
    [string]$ResourceGroup = "resell-panda-rg",
    [string]$Location = "East US",
    [string]$SqlServerName = "resellbook-server",
    [string]$DatabaseName = "ResellBook-db",
    [string]$AppName = "ResellBook20250929183655",
    [string]$AdminLogin = "reselladmin",
    [string]$AdminPassword = "YourStrongPassword123!"  # Change this!
)

Write-Host "🚀 Creating Azure Resources..." -ForegroundColor Green

# Create Resource Group
Write-Host "📁 Creating Resource Group..." -ForegroundColor Yellow
az group create --name $ResourceGroup --location $Location

# Create SQL Server
Write-Host "🗄️ Creating SQL Server..." -ForegroundColor Yellow
az sql server create `
    --name $SqlServerName `
    --resource-group $ResourceGroup `
    --location $Location `
    --admin-user $AdminLogin `
    --admin-password $AdminPassword

# Create SQL Database (Basic Tier)
Write-Host "📊 Creating SQL Database..." -ForegroundColor Yellow
az sql db create `
    --resource-group $ResourceGroup `
    --server $SqlServerName `
    --name $DatabaseName `
    --edition Basic `
    --compute-model Serverless `
    --auto-pause-delay 60 `
    --min-capacity 0.5 `
    --capacity 1

# Configure Firewall (Allow Azure services)
Write-Host "🔥 Configuring Firewall..." -ForegroundColor Yellow
az sql server firewall-rule create `
    --resource-group $ResourceGroup `
    --server $SqlServerName `
    --name "AllowAllWindowsAzureIps" `
    --start-ip-address "0.0.0.0" `
    --end-ip-address "0.0.0.0"

# Create App Service Plan (Free)
Write-Host "📋 Creating App Service Plan..." -ForegroundColor Yellow
az appservice plan create `
    --name "resellbook-plan" `
    --resource-group $ResourceGroup `
    --location $Location `
    --sku FREE

# Create App Service
Write-Host "🌐 Creating App Service..." -ForegroundColor Yellow
az webapp create `
    --resource-group $ResourceGroup `
    --plan "resellbook-plan" `
    --name $AppName `
    --runtime "dotnet:8"

# Get Connection String
Write-Host "🔗 Getting Connection String..." -ForegroundColor Yellow
$connectionString = az sql db show-connection-string `
    --server $SqlServerName `
    --name $DatabaseName `
    --client ado.net

# Replace password placeholder
$connectionString = $connectionString -replace "<username>", $AdminLogin
$connectionString = $connectionString -replace "<password>", $AdminPassword

Write-Host "✅ Resources Created Successfully!" -ForegroundColor Green
Write-Host "🔑 Database Connection String:" -ForegroundColor Cyan
Write-Host $connectionString -ForegroundColor White
Write-Host "" -ForegroundColor White
Write-Host "📝 Update your appsettings.json with this connection string" -ForegroundColor Yellow</content>
<parameter name="filePath">c:\Repos\ResellPanda\ResellBook\AZURE_RESOURCE_SETUP_GUIDE.md