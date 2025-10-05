# Advanced deployment script with wwwroot preservation using Kudu API
param(
    [string]$ResourceGroup = "resell-panda-rg",
    [string]$AppName = "ResellBook20250929183655"
)

Write-Host "🔄 Advanced deployment with wwwroot preservation starting..." -ForegroundColor Green

# Get deployment credentials
Write-Host "🔐 Getting deployment credentials..." -ForegroundColor Yellow
$deployUser = az webapp deployment list-publishing-credentials --name $AppName --resource-group $ResourceGroup --query "publishingUserName" -o tsv
$deployPass = az webapp deployment list-publishing-credentials --name $AppName --resource-group $ResourceGroup --query "publishingPassword" -o tsv

# Base64 encode credentials
$credentials = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("$deployUser`:$deployPass"))
$kuduUrl = "https://$AppName.scm.azurewebsites.net"

# Function to call Kudu API
function Invoke-KuduApi {
    param($Uri, $Method = "GET", $Body = $null)
    
    $headers = @{
        "Authorization" = "Basic $credentials"
        "Content-Type" = "application/json"
    }
    
    try {
        if ($Body) {
            return Invoke-RestMethod -Uri $Uri -Method $Method -Headers $headers -Body $Body
        } else {
            return Invoke-RestMethod -Uri $Uri -Method $Method -Headers $headers
        }
    } catch {
        Write-Host "❌ Kudu API Error: $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# Step 1: Check if wwwroot exists and create backup list
Write-Host "📋 Checking existing wwwroot contents..." -ForegroundColor Yellow
$wwwrootFiles = Invoke-KuduApi -Uri "$kuduUrl/api/vfs/site/wwwroot/" -Method "GET"

if ($wwwrootFiles -and $wwwrootFiles.Count -gt 0) {
    Write-Host "📁 Found $($wwwrootFiles.Count) items in wwwroot folder" -ForegroundColor Cyan
    $wwwrootFiles | ForEach-Object { Write-Host "  - $($_.name)" -ForegroundColor Gray }
} else {
    Write-Host "📭 No existing files in wwwroot folder" -ForegroundColor Yellow
}

# Step 2: Build application
Write-Host "🔨 Building application..." -ForegroundColor Yellow
dotnet publish -c Release -o publish

# Step 3: Create deployment package excluding wwwroot
Write-Host "📦 Creating deployment package (excluding wwwroot)..." -ForegroundColor Yellow

$tempDir = "temp-deploy-$(Get-Date -Format 'yyyyMMddHHmmss')"
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

# Copy everything except wwwroot
Get-ChildItem "publish" | Where-Object { $_.Name -ne "wwwroot" } | Copy-Item -Destination $tempDir -Recurse

# Create zip for deployment
$deployZip = "deploy-preserve-wwwroot.zip"
Compress-Archive -Path "$tempDir\*" -DestinationPath $deployZip -Force

# Step 4: Deploy using ZIP deployment
Write-Host "🚀 Deploying application..." -ForegroundColor Green
az webapp deployment source config-zip --resource-group $ResourceGroup --name $AppName --src $deployZip --timeout 600

# Step 5: Verify wwwroot still exists
Write-Host "🔍 Verifying wwwroot preservation..." -ForegroundColor Yellow
Start-Sleep -Seconds 10  # Give deployment time to complete

$postDeployFiles = Invoke-KuduApi -Uri "$kuduUrl/api/vfs/site/wwwroot/" -Method "GET"

if ($postDeployFiles -and $postDeployFiles.Count -gt 0) {
    Write-Host "✅ wwwroot folder preserved! Found $($postDeployFiles.Count) items" -ForegroundColor Green
    $postDeployFiles | ForEach-Object { Write-Host "  ✓ $($_.name)" -ForegroundColor Green }
} else {
    Write-Host "⚠️ wwwroot folder appears empty after deployment" -ForegroundColor Yellow
}

# Cleanup
Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item $deployZip -Force -ErrorAction SilentlyContinue

Write-Host "🎉 Deployment completed with wwwroot preservation!" -ForegroundColor Green
Write-Host "🌐 App URL: https://$AppName.azurewebsites.net" -ForegroundColor Cyan