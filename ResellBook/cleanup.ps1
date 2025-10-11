<#
.SYNOPSIS
    Safe cleanup of ResellBook project files
.DESCRIPTION
    Removes build artifacts, old backups, and temporary files while preserving critical data
.PARAMETER KeepBackups
    Number of backup folders to keep (default: 3)
.PARAMETER MaxLogSizeMB
    Maximum log file size before rotation (default: 10MB)
.PARAMETER DryRun
    Show what would be deleted without actually deleting (default: true for safety)
.EXAMPLE
    .\cleanup.ps1 -DryRun:$false
    # Actually performs cleanup
.EXAMPLE
    .\cleanup.ps1 -KeepBackups 5 -MaxLogSizeMB 20
    # Keep 5 backups and rotate logs at 20MB
#>

param(
    [int]$KeepBackups = 3,              # Number of backup folders to keep
    [int]$MaxLogSizeMB = 10,            # Max log file size before rotation
    [switch]$DryRun = $true             # Default to dry run for safety
)

Write-Host "🧹 ResellBook Project Cleanup" -ForegroundColor Green
Write-Host "=============================" -ForegroundColor Green

if ($DryRun) {
    Write-Host "🔍 DRY RUN MODE - No files will be deleted" -ForegroundColor Yellow
    Write-Host "Use -DryRun:`$false to actually delete files" -ForegroundColor Yellow
    Write-Host ""
}

# Check if we're in the right directory
if (-not (Test-Path "ResellBook.csproj")) {
    Write-Host "❌ Error: Please run this script from the ResellBook project directory" -ForegroundColor Red
    Write-Host "Expected to find: ResellBook.csproj" -ForegroundColor Red
    exit 1
}

$totalSpaceSaved = 0

# 1. Clean build artifacts (always safe)
Write-Host "🏗️ Cleaning build artifacts..." -ForegroundColor Yellow

$buildFolders = @("bin", "obj", "publish")
foreach ($folder in $buildFolders) {
    if (Test-Path $folder) {
        $size = (Get-ChildItem -Path $folder -Recurse -File | Measure-Object -Property Length -Sum).Sum / 1MB
        Write-Host "  📁 $folder`: $([math]::Round($size, 2)) MB" -ForegroundColor Gray
        
        if (-not $DryRun) {
            Remove-Item -Path $folder -Recurse -Force -ErrorAction SilentlyContinue
        }
        $totalSpaceSaved += $size
    }
}

# Also run dotnet clean for thorough cleanup
if (-not $DryRun) {
    dotnet clean --verbosity quiet 2>&1 | Out-Null
}

Write-Host "✅ Build artifacts cleaned" -ForegroundColor Green

# 2. Clean old deployment backups
Write-Host "💾 Managing deployment backups..." -ForegroundColor Yellow

$backupFolders = Get-ChildItem -Directory -Name "wwwroot-backup-*" | Sort-Object -Descending
Write-Host "  📋 Found $($backupFolders.Count) backup folders" -ForegroundColor Gray

if ($backupFolders.Count -gt $KeepBackups) {
    $toKeep = $backupFolders | Select-Object -First $KeepBackups
    $toDelete = $backupFolders | Select-Object -Skip $KeepBackups
    
    Write-Host "  ✅ Keeping newest $KeepBackups backups:" -ForegroundColor Green
    foreach ($folder in $toKeep) {
        Write-Host "    📁 $folder" -ForegroundColor Green
    }
    
    Write-Host "  🗑️ Marking for deletion:" -ForegroundColor Red
    foreach ($folder in $toDelete) {
        if (Test-Path $folder) {
            $size = (Get-ChildItem -Path $folder -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
            Write-Host "    📁 $folder ($([math]::Round($size, 2)) MB)" -ForegroundColor Red
            
            if (-not $DryRun) {
                Remove-Item -Path $folder -Recurse -Force
            }
            $totalSpaceSaved += $size
        }
    }
} else {
    Write-Host "  ✅ All backup folders within limit ($($backupFolders.Count) ≤ $KeepBackups)" -ForegroundColor Green
}

# 3. Clean temporary download folders
Write-Host "📥 Cleaning temporary download folders..." -ForegroundColor Yellow

$tempFolders = Get-ChildItem -Directory -Name "temp-download-*"
if ($tempFolders.Count -gt 0) {
    Write-Host "  📋 Found $($tempFolders.Count) temporary folders" -ForegroundColor Gray
    
    foreach ($folder in $tempFolders) {
        if (Test-Path $folder) {
            $size = (Get-ChildItem -Path $folder -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
            Write-Host "  🗑️ $folder ($([math]::Round($size, 2)) MB)" -ForegroundColor Red
            
            if (-not $DryRun) {
                Remove-Item -Path $folder -Recurse -Force
            }
            $totalSpaceSaved += $size
        }
    }
} else {
    Write-Host "  ✅ No temporary download folders found" -ForegroundColor Green
}

# 4. Clean loose .zip files
Write-Host "📦 Cleaning deployment ZIP files..." -ForegroundColor Yellow

$zipFiles = Get-ChildItem -Name "*.zip" | Where-Object { $_ -match "(deploy|backup|temp)" }
if ($zipFiles.Count -gt 0) {
    foreach ($zip in $zipFiles) {
        $size = (Get-Item $zip).Length / 1MB
        Write-Host "  🗑️ $zip ($([math]::Round($size, 2)) MB)" -ForegroundColor Red
        
        if (-not $DryRun) {
            Remove-Item -Path $zip -Force
        }
        $totalSpaceSaved += $size
    }
} else {
    Write-Host "  ✅ No deployment ZIP files found" -ForegroundColor Green
}

# 5. Manage log files
Write-Host "📊 Managing log files..." -ForegroundColor Yellow

if (Test-Path "AppLogs") {
    $logFiles = Get-ChildItem -Path "AppLogs" -Filter "*.txt"
    $rotatedLogs = 0
    
    foreach ($log in $logFiles) {
        $sizeMB = [math]::Round($log.Length / 1MB, 2)
        
        if ($sizeMB -gt $MaxLogSizeMB) {
            $newName = $log.BaseName + "_" + (Get-Date -Format "yyyyMMdd_HHmmss") + $log.Extension
            Write-Host "  📋 $($log.Name) ($sizeMB MB) → $newName" -ForegroundColor Yellow
            
            if (-not $DryRun) {
                Rename-Item $log.FullName -NewName (Join-Path "AppLogs" $newName)
                New-Item -Path $log.FullName -ItemType File -Force | Out-Null
            }
            $rotatedLogs++
        } else {
            Write-Host "  ✅ $($log.Name) ($sizeMB MB) - OK" -ForegroundColor Green
        }
    }
    
    if ($rotatedLogs -eq 0) {
        Write-Host "  ✅ All log files within size limits" -ForegroundColor Green
    }
} else {
    Write-Host "  📋 No AppLogs folder found" -ForegroundColor Gray
}

# 6. Summary
Write-Host "`n📊 Cleanup Summary:" -ForegroundColor Cyan
Write-Host "===================" -ForegroundColor Cyan

$remainingBackups = (Get-ChildItem -Directory -Name "wwwroot-backup-*" -ErrorAction SilentlyContinue).Count
$remainingTempFolders = (Get-ChildItem -Directory -Name "temp-download-*" -ErrorAction SilentlyContinue).Count

Write-Host "📁 Backup folders remaining: $remainingBackups" -ForegroundColor White
Write-Host "📥 Temp folders remaining: $remainingTempFolders" -ForegroundColor White
Write-Host "💾 Estimated space saved: $([math]::Round($totalSpaceSaved, 2)) MB" -ForegroundColor White

# 7. File safety reminders
Write-Host "`n🛡️ Critical Files Protected:" -ForegroundColor Green
$protectedFolders = @("Controllers", "Models", "Services", "Data", "wwwroot/uploads", "Migrations")
foreach ($folder in $protectedFolders) {
    if (Test-Path $folder) {
        Write-Host "  ✅ $folder" -ForegroundColor Green
    }
}

if ($DryRun) {
    Write-Host "`n🔍 This was a DRY RUN. No files were actually deleted." -ForegroundColor Yellow
    Write-Host "To perform actual cleanup, run:" -ForegroundColor Yellow
    Write-Host ".\cleanup.ps1 -DryRun:`$false" -ForegroundColor Cyan
} else {
    Write-Host "`n✅ Cleanup completed successfully!" -ForegroundColor Green
    Write-Host "Your project is now clean and optimized." -ForegroundColor Green
}

Write-Host "`n📋 Quick Status Check:" -ForegroundColor Cyan
Write-Host "Current directory size: " -NoNewline -ForegroundColor White
$currentSize = (Get-ChildItem -Recurse -File | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "$([math]::Round($currentSize, 2)) MB" -ForegroundColor Yellow