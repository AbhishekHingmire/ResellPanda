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
} else {
    Write-Host "   $($backups.Count) backup folders (within limit)" -ForegroundColor Blue
}

# 3. Check log sizes
Write-Host "`n3. Checking log file sizes..." -ForegroundColor Yellow
if (Test-Path "AppLogs") {
    Get-ChildItem "AppLogs/*.txt" -ErrorAction SilentlyContinue | ForEach-Object {
        $sizeKB = [math]::Round($_.Length / 1KB, 2)
        $status = if ($sizeKB -gt 1000) { "⚠️ LARGE" } elseif ($sizeKB -gt 500) { "⚡ GROWING" } else { "✅ OK" }
        Write-Host "   $($_.Name): $sizeKB KB $status" -ForegroundColor $(if ($sizeKB -gt 1000) { "Red" } elseif ($sizeKB -gt 500) { "Yellow" } else { "Green" })
    }
} else {
    Write-Host "   AppLogs folder not found (will be created on first run)" -ForegroundColor Blue
}

# 4. Verify project structure
Write-Host "`n4. Verifying project structure..." -ForegroundColor Yellow
$criticalFolders = @("Controllers", "Models", "Utils", "wwwroot", "Developer Documentation")
$allGood = $true
foreach ($folder in $criticalFolders) {
    if (Test-Path $folder) {
        Write-Host "   ✅ $folder" -ForegroundColor Green
    } else {
        Write-Host "   ❌ $folder - MISSING!" -ForegroundColor Red
        $allGood = $false
    }
}

# 5. Check deployment script
Write-Host "`n5. Checking deployment script..." -ForegroundColor Yellow
if (Test-Path "deploy.ps1") {
    Write-Host "   ✅ deploy.ps1 exists" -ForegroundColor Green
} else {
    Write-Host "   ❌ deploy.ps1 MISSING! DO NOT DEPLOY WITHOUT IT!" -ForegroundColor Red
    $allGood = $false
}

# 6. Summary
Write-Host "`n🎯 MAINTENANCE SUMMARY:" -ForegroundColor Cyan
if ($allGood) {
    Write-Host "   ✅ Project structure healthy" -ForegroundColor Green
    Write-Host "   ✅ Temporary files cleaned" -ForegroundColor Green
    Write-Host "   ✅ Backups managed" -ForegroundColor Green
    Write-Host "   ✅ Ready for development/deployment" -ForegroundColor Green
} else {
    Write-Host "   ⚠️ Some issues found - check above" -ForegroundColor Yellow
    Write-Host "   🚨 FIX ISSUES BEFORE DEPLOYMENT" -ForegroundColor Red
}

Write-Host "`n🏁 Maintenance completed!" -ForegroundColor Green