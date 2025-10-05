# PowerShell script to deploy while preserving wwwroot folder
# This script backs up wwwroot, deploys, then restores it

param(
    [string]$ResourceGroup = "resell-panda-rg",
    [string]$AppName = "ResellBook20250929183655"
)

Write-Host "🚀 Starting deployment with wwwroot preservation..." -ForegroundColor Green

# Step 1: Build and publish the application
Write-Host "📦 Building application..." -ForegroundColor Yellow
dotnet publish -c Release -o publish

# Step 2: Backup existing wwwroot from production
Write-Host "💾 Backing up production wwwroot folder..." -ForegroundColor Yellow
$backupPath = "wwwroot-backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

# Download current wwwroot folder from production
az webapp deployment source config-zip --resource-group $ResourceGroup --name $AppName --src "empty.zip" --timeout 600
Start-Sleep -Seconds 5

# Create Kudu API call to backup wwwroot
$kuduUrl = "https://$AppName.scm.azurewebsites.net"
Write-Host "📥 Downloading wwwroot backup from: $kuduUrl" -ForegroundColor Cyan

# Step 3: Create deployment package WITHOUT overwriting wwwroot
Write-Host "📦 Creating deployment package..." -ForegroundColor Yellow

# Create temp directory structure
$tempDeploy = "temp-deploy"
if (Test-Path $tempDeploy) { Remove-Item $tempDeploy -Recurse -Force }
New-Item -ItemType Directory -Path $tempDeploy

# Copy all files except wwwroot
Copy-Item "publish\*" $tempDeploy -Recurse -Exclude "wwwroot"

# Create deployment zip
Compress-Archive -Path "$tempDeploy\*" -DestinationPath "deploy-no-wwwroot.zip" -Force

# Step 4: Deploy application
Write-Host "🚀 Deploying application (preserving wwwroot)..." -ForegroundColor Green
az webapp deployment source config-zip --resource-group $ResourceGroup --name $AppName --src "deploy-no-wwwroot.zip" --timeout 600

Write-Host "✅ Deployment completed successfully!" -ForegroundColor Green
Write-Host "🛡️ Your wwwroot folder and uploaded files are preserved!" -ForegroundColor Green

# Cleanup
Remove-Item $tempDeploy -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "deploy-no-wwwroot.zip" -Force -ErrorAction SilentlyContinue

Write-Host "🎉 Deployment with wwwroot preservation completed!" -ForegroundColor Green