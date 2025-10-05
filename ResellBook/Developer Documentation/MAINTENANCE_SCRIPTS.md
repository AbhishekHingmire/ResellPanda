# 🧹 Project Maintenance Scripts

## Quick Cleanup Script
```powershell
# Clean temporary deployment files
Remove-Item "temp-deploy-*" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "temp-download-*" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "deploy-*.zip" -Force -ErrorAction SilentlyContinue
Remove-Item "bulletproof-deploy.zip" -Force -ErrorAction SilentlyContinue
Remove-Item "app-logs.zip" -Force -ErrorAction SilentlyContinue
Remove-Item "publish/" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "✅ Temporary files cleaned" -ForegroundColor Green
```

## Backup Management Script
```powershell
# Keep only the 2 most recent backup folders
$backups = Get-ChildItem "wwwroot-backup-*" | Sort-Object Name -Descending
if ($backups.Count -gt 2) {
    $backups[2..($backups.Count-1)] | Remove-Item -Recurse -Force
    Write-Host "✅ Kept 2 most recent backups, removed $($backups.Count - 2) old ones" -ForegroundColor Green
} else {
    Write-Host "ℹ️ $($backups.Count) backup folders found (within limit)" -ForegroundColor Blue
}
```

## Log Size Monitor
```powershell
# Check log file sizes
if (Test-Path "AppLogs") {
    Get-ChildItem "AppLogs/*.txt" | ForEach-Object {
        $sizeKB = [math]::Round($_.Length / 1KB, 2)
        $status = if ($sizeKB -gt 1000) { "⚠️ LARGE" } elseif ($sizeKB -gt 500) { "⚡ GROWING" } else { "✅ OK" }
        Write-Host "$($_.Name): $sizeKB KB $status" -ForegroundColor $(if ($sizeKB -gt 1000) { "Red" } elseif ($sizeKB -gt 500) { "Yellow" } else { "Green" })
    }
} else {
    Write-Host "⚠️ AppLogs folder not found" -ForegroundColor Yellow
}
```

## Project Health Check
```powershell
# Verify critical folders exist
$criticalFolders = @("Controllers", "Models", "Utils", "wwwroot", "AppLogs", "Developer Documentation")
$missingFolders = @()

foreach ($folder in $criticalFolders) {
    if (Test-Path $folder) {
        Write-Host "✅ $folder" -ForegroundColor Green
    } else {
        Write-Host "❌ $folder - MISSING!" -ForegroundColor Red
        $missingFolders += $folder
    }
}

if ($missingFolders.Count -eq 0) {
    Write-Host "`n🎉 Project structure is healthy!" -ForegroundColor Green
} else {
    Write-Host "`n⚠️ Missing folders detected. Check deployment or restore from backup." -ForegroundColor Yellow
}
```

## Complete Maintenance Script
Save this as `maintenance.ps1`:

```powershell
Write-Host "🧹 RUNNING PROJECT MAINTENANCE..." -ForegroundColor Cyan

# 1. Clean temporary files
Write-Host "`n1. Cleaning temporary files..." -ForegroundColor Yellow
Remove-Item "temp-*" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "*.zip" -Exclude "wwwroot-backup-*/wwwroot-backup.zip" -Force -ErrorAction SilentlyContinue
Remove-Item "publish/" -Recurse -Force -ErrorAction SilentlyContinue

# 2. Manage backups
Write-Host "`n2. Managing backup folders..." -ForegroundColor Yellow
$backups = Get-ChildItem "wwwroot-backup-*" | Sort-Object Name -Descending
if ($backups.Count -gt 2) {
    $backups[2..($backups.Count-1)] | Remove-Item -Recurse -Force
    Write-Host "   Removed $($backups.Count - 2) old backup folders" -ForegroundColor Green
}

# 3. Check log sizes
Write-Host "`n3. Checking log file sizes..." -ForegroundColor Yellow
if (Test-Path "AppLogs") {
    Get-ChildItem "AppLogs/*.txt" | ForEach-Object {
        $sizeKB = [math]::Round($_.Length / 1KB, 2)
        Write-Host "   $($_.Name): $sizeKB KB" -ForegroundColor Green
    }
}

# 4. Verify project structure
Write-Host "`n4. Verifying project structure..." -ForegroundColor Yellow
$criticalFolders = @("Controllers", "Models", "Utils", "wwwroot", "AppLogs")
$allGood = $true
foreach ($folder in $criticalFolders) {
    if (Test-Path $folder) {
        Write-Host "   ✅ $folder" -ForegroundColor Green
    } else {
        Write-Host "   ❌ $folder - MISSING!" -ForegroundColor Red
        $allGood = $false
    }
}

# 5. Summary
Write-Host "`n🎯 MAINTENANCE SUMMARY:" -ForegroundColor Cyan
if ($allGood) {
    Write-Host "   ✅ Project structure healthy" -ForegroundColor Green
    Write-Host "   ✅ Temporary files cleaned" -ForegroundColor Green
    Write-Host "   ✅ Backups managed" -ForegroundColor Green
    Write-Host "   ✅ Ready for development/deployment" -ForegroundColor Green
} else {
    Write-Host "   ⚠️ Some issues found - check above" -ForegroundColor Yellow
}

Write-Host "`n🏁 Maintenance completed!" -ForegroundColor Green
```