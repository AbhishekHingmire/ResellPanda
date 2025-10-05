# Simple method - Deploy everything except wwwroot folder
Write-Host "🚀 Simple deployment preserving wwwroot..." -ForegroundColor Green

# Build the application
Write-Host "🔨 Building application..." -ForegroundColor Yellow
dotnet publish -c Release -o publish

# Create deployment folder without wwwroot
Write-Host "📦 Preparing deployment package..." -ForegroundColor Yellow
$deployFolder = "deploy-temp"

# Remove existing deploy folder
if (Test-Path $deployFolder) {
    Remove-Item $deployFolder -Recurse -Force
}

# Create new deploy folder
New-Item -ItemType Directory -Path $deployFolder | Out-Null

# Copy all files except wwwroot directory
Write-Host "📋 Copying files (excluding wwwroot)..." -ForegroundColor Cyan
Get-ChildItem "publish" -Exclude "wwwroot" | Copy-Item -Destination $deployFolder -Recurse -Force

# Create zip file
Write-Host "🗜️ Creating deployment zip..." -ForegroundColor Yellow
if (Test-Path "deploy-safe.zip") {
    Remove-Item "deploy-safe.zip" -Force
}
Compress-Archive -Path "$deployFolder\*" -DestinationPath "deploy-safe.zip" -Force

# Deploy to Azure
Write-Host "☁️ Deploying to Azure (wwwroot will be preserved)..." -ForegroundColor Green
az webapp deploy --resource-group "resell-panda-rg" --name "ResellBook20250929183655" --src-path "deploy-safe.zip" --type zip

# Cleanup temporary files
Write-Host "🧹 Cleaning up..." -ForegroundColor Yellow
Remove-Item $deployFolder -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "deploy-safe.zip" -Force -ErrorAction SilentlyContinue

Write-Host "✅ Deployment completed! wwwroot folder preserved." -ForegroundColor Green
Write-Host "🔗 Your app: https://resellbook20250929183655.azurewebsites.net" -ForegroundColor Cyan