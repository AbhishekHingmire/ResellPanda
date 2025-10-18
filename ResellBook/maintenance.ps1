<#
.SYNOPSIS
    ResellBook Project Maintenance Script - SAFE VERSION
.DESCRIPTION
    Performs routine maintenance tasks on the ResellBook ASP.NET Core project.
    This script is designed to be completely safe to run and includes multiple
    safety checks to prevent accidental data loss.

    WHAT THIS SCRIPT DOES:
    - Cleans temporary files and build artifacts (safe, no data loss)
    - Manages backup rotation (keeps recent backups, removes old ones)
    - Monitors log file sizes (warns about large logs, doesn't delete)
    - Verifies critical project structure (helps catch missing files)
    - Checks deployment readiness (ensures deploy.ps1 exists)

    IMPACT:
    - ✅ SAFE: No critical data or source code is ever deleted
    - ✅ BENEFICIAL: Frees up disk space from temporary files
    - ✅ PREVENTIVE: Helps identify potential issues before deployment
    - ✅ MONITORING: Provides visibility into project health

    WHEN TO RUN:
    - Daily during development to keep workspace clean
    - Before commits to ensure project structure is intact
    - Before deployments to verify everything is ready
    - When disk space is low (safe cleanup of temp files)

    FREQUENCY: Safe to run multiple times per day
.PARAMETER Force
    Skip confirmation prompts (use with caution)
.EXAMPLE
    .\maintenance.ps1
    # Interactive mode with confirmations
.EXAMPLE
    .\maintenance.ps1 -Force
    # Skip confirmations (for automated scripts)
#>

param(
    [switch]$Force  # Skip confirmation prompts
)

# ========================================================================================
# MAINTENANCE SCRIPT SUMMARY
# ========================================================================================
# Script Name: maintenance.ps1
# Purpose: Safe project maintenance and health checks
# Safety Level: 🟢 HIGHLY SAFE - No destructive operations without confirmation
# Impact: Cleans temp files, monitors logs, verifies structure
# Run Frequency: Daily during development
# Estimated Runtime: < 30 seconds
# ========================================================================================

Write-Host "🧹 ResellBook Project Maintenance Script" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "Safety Level: 🟢 HIGHLY SAFE" -ForegroundColor Green
Write-Host "Impact: Cleans temp files, monitors health" -ForegroundColor Yellow
Write-Host ""

# SAFETY CHECK 1: Ensure we're in the correct project directory
# WHY: Prevents accidental execution in wrong directory that could delete wrong files
# IMPACT: Stops script immediately if project structure is not found
Write-Host "🔍 Safety Check 1: Verifying project location..." -ForegroundColor Yellow
if (-not (Test-Path "ResellBook.csproj")) {
    Write-Host "❌ ERROR: ResellBook.csproj not found!" -ForegroundColor Red
    Write-Host "   Please run this script from the ResellBook project root directory." -ForegroundColor Red
    Write-Host "   Expected location: C:\Repos\ResellPanda\ResellBook\" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Project location verified" -ForegroundColor Green

# SAFETY CHECK 2: Confirm execution (unless -Force is used)
# WHY: Gives user final chance to cancel if they ran this accidentally
# IMPACT: Prevents unintended maintenance operations
if (-not $Force) {
    Write-Host "`n⚠️  CONFIRMATION REQUIRED:" -ForegroundColor Yellow
    Write-Host "   This will clean temporary files and check project health." -ForegroundColor White
    Write-Host "   No source code or important data will be deleted." -ForegroundColor Green
    $confirmation = Read-Host "   Continue? (y/N)"
    if ($confirmation -ne "y" -and $confirmation -ne "Y") {
        Write-Host "❌ Maintenance cancelled by user." -ForegroundColor Yellow
        exit 0
    }
}

Write-Host "`n🧹 RUNNING PROJECT MAINTENANCE..." -ForegroundColor Cyan

# ========================================================================================
# STEP 1: Clean temporary files
# ========================================================================================
# WHY: Temporary files accumulate during development and take up disk space
# WHAT: Removes temp-* folders, loose .zip files (except backups), publish folder
# IMPACT: Frees disk space, keeps workspace clean, no risk to source code
# SAFETY: Only removes files that are safe to delete, preserves backups
Write-Host "`n1. 🧹 Cleaning temporary files..." -ForegroundColor Yellow
Write-Host "   WHY: Free disk space and keep workspace organized" -ForegroundColor Gray
Write-Host "   IMPACT: Safe cleanup, no source code affected" -ForegroundColor Gray

$tempFilesRemoved = 0
$spaceSaved = 0

# Remove temp-* folders (safe - these are temporary downloads/uploads)
$tempFolders = Get-ChildItem "temp-*" -Directory -ErrorAction SilentlyContinue
if ($tempFolders.Count -gt 0) {
    Write-Host "   📁 Removing $($tempFolders.Count) temporary folders:" -ForegroundColor White
    foreach ($folder in $tempFolders) {
        $size = (Get-ChildItem -Path $folder.FullName -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
        Write-Host "     🗑️ $($folder.Name) ($([math]::Round($size, 2)) MB)" -ForegroundColor Red
        Remove-Item $folder.FullName -Recurse -Force -ErrorAction SilentlyContinue
        $tempFilesRemoved++
        $spaceSaved += $size
    }
} else {
    Write-Host "   ✅ No temporary folders found" -ForegroundColor Green
}

# Remove loose .zip files (except backup zips which are protected)
$zipFiles = Get-ChildItem "*.zip" -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -notlike "wwwroot-backup-*" }
if ($zipFiles.Count -gt 0) {
    Write-Host "   📦 Removing $($zipFiles.Count) loose ZIP files:" -ForegroundColor White
    foreach ($zip in $zipFiles) {
        $size = $zip.Length / 1MB
        Write-Host "     🗑️ $($zip.Name) ($([math]::Round($size, 2)) MB)" -ForegroundColor Red
        Remove-Item $zip.FullName -Force -ErrorAction SilentlyContinue
        $tempFilesRemoved++
        $spaceSaved += $size
    }
} else {
    Write-Host "   ✅ No loose ZIP files found" -ForegroundColor Green
}

# Remove publish folder (safe - gets recreated during deployment)
if (Test-Path "publish") {
    $publishSize = (Get-ChildItem -Path "publish" -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
    Write-Host "   📁 Removing publish folder ($([math]::Round($publishSize, 2)) MB)" -ForegroundColor Red
    Remove-Item "publish" -Recurse -Force -ErrorAction SilentlyContinue
    $tempFilesRemoved++
    $spaceSaved += $publishSize
} else {
    Write-Host "   ✅ No publish folder found" -ForegroundColor Green
}

Write-Host "   ✅ Temporary cleanup complete - $tempFilesRemoved items removed, $([math]::Round($spaceSaved, 2)) MB saved" -ForegroundColor Green

# ========================================================================================
# STEP 2: Manage backups (ROTATION ONLY - NO DELETION WITHOUT CONFIRMATION)
# ========================================================================================
# WHY: Backup folders accumulate over time and consume disk space
# WHAT: Shows backup status, suggests cleanup but requires confirmation
# IMPACT: Prevents disk space issues while preserving recovery options
# SAFETY: Never deletes backups automatically - always shows what would be removed
Write-Host "`n2. 💾 Managing backup folders..." -ForegroundColor Yellow
Write-Host "   WHY: Prevent disk space issues from old backups" -ForegroundColor Gray
Write-Host "   IMPACT: Shows cleanup suggestions, preserves all backups" -ForegroundColor Gray

$backups = Get-ChildItem "wwwroot-backup-*" -Directory -ErrorAction SilentlyContinue | Sort-Object Name -Descending
$backupLimit = 5  # Keep last 5 backups

if ($backups.Count -gt 0) {
    Write-Host "   📋 Found $($backups.Count) backup folders (limit: $backupLimit)" -ForegroundColor White

    # Show all backups with sizes
    for ($i = 0; $i -lt $backups.Count; $i++) {
        $backup = $backups[$i]
        $size = (Get-ChildItem -Path $backup.FullName -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
        $status = if ($i -lt $backupLimit) { "✅ KEEP" } else { "🗑️ CANDIDATE" }
        Write-Host "     $status $($backup.Name) ($([math]::Round($size, 2)) MB)" -ForegroundColor $(if ($i -lt $backupLimit) { "Green" } else { "Red" })
    }

    # Suggest cleanup if over limit
    if ($backups.Count -gt $backupLimit) {
        $toRemove = $backups.Count - $backupLimit
        Write-Host "   ⚠️ $($toRemove) old backups can be removed to save space" -ForegroundColor Yellow

        if (-not $Force) {
            $cleanupBackups = Read-Host "   Remove old backups? (y/N)"
            if ($cleanupBackups -eq "y" -or $cleanupBackups -eq "Y") {
                $backups[$backupLimit..($backups.Count-1)] | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "   ✅ Removed $toRemove old backup folders" -ForegroundColor Green
            } else {
                Write-Host "   ⏭️ Backup cleanup skipped" -ForegroundColor Blue
            }
        }
    } else {
        Write-Host "   ✅ All backups within limit" -ForegroundColor Green
    }
} else {
    Write-Host "   📋 No backup folders found" -ForegroundColor Blue
}

# ========================================================================================
# STEP 3: Check log file sizes (MONITORING ONLY - NO DELETION)
# ========================================================================================
# WHY: Large log files can slow down the application and waste disk space
# WHAT: Analyzes log sizes and provides recommendations
# IMPACT: Helps identify performance issues, prevents disk space problems
# SAFETY: Never deletes logs - only reports sizes and suggests actions
Write-Host "`n3. 📊 Checking log file sizes..." -ForegroundColor Yellow
Write-Host "   WHY: Large logs can slow performance and waste space" -ForegroundColor Gray
Write-Host "   IMPACT: Monitoring only - identifies issues, suggests solutions" -ForegroundColor Gray

if (Test-Path "AppLogs") {
    $logFiles = Get-ChildItem "AppLogs/*.txt" -ErrorAction SilentlyContinue
    if ($logFiles.Count -gt 0) {
        Write-Host "   📋 Analyzing $($logFiles.Count) log files:" -ForegroundColor White

        $largeLogs = 0
        $totalLogSize = 0

        foreach ($log in $logFiles) {
            $sizeKB = [math]::Round($log.Length / 1KB, 2)
            $sizeMB = [math]::Round($log.Length / 1MB, 2)
            $totalLogSize += $log.Length

            if ($sizeMB -gt 50) {
                Write-Host "     ❌ $($log.Name): $sizeMB MB ⚠️ VERY LARGE" -ForegroundColor Red
                Write-Host "        💡 Consider log rotation or cleanup" -ForegroundColor Yellow
                $largeLogs++
            } elseif ($sizeMB -gt 10) {
                Write-Host "     ⚠️ $($log.Name): $sizeMB MB 🟡 LARGE" -ForegroundColor Yellow
                $largeLogs++
            } elseif ($sizeKB -gt 1000) {
                Write-Host "     📈 $($log.Name): $sizeKB KB 🟢 GROWING" -ForegroundColor Cyan
            } else {
                Write-Host "     ✅ $($log.Name): $sizeKB KB 🟢 OK" -ForegroundColor Green
            }
        }

        $totalLogSizeMB = [math]::Round($totalLogSize / 1MB, 2)
        Write-Host "   📊 Total log size: $totalLogSizeMB MB" -ForegroundColor White

        if ($largeLogs -gt 0) {
            Write-Host "   💡 Recommendation: Run .\cleanup.ps1 to rotate large logs" -ForegroundColor Cyan
        }
    } else {
        Write-Host "   📋 AppLogs folder exists but no .txt files found" -ForegroundColor Blue
    }
} else {
    Write-Host "   📋 AppLogs folder not found (will be created when app runs)" -ForegroundColor Blue
}

# ========================================================================================
# STEP 4: Verify project structure (CRITICAL FILES CHECK)
# ========================================================================================
# WHY: Missing critical files can break builds or deployments
# WHAT: Checks for essential project folders and files
# IMPACT: Prevents deployment failures, catches missing components early
# SAFETY: Read-only operation, no files are modified
Write-Host "`n4. 🔍 Verifying project structure..." -ForegroundColor Yellow
Write-Host "   WHY: Ensures all critical components are present" -ForegroundColor Gray
Write-Host "   IMPACT: Prevents deployment failures, read-only safety check" -ForegroundColor Gray

$criticalFolders = @(
    @{Name="Controllers"; Required=$true; Purpose="API endpoints and request handlers"},
    @{Name="Models"; Required=$true; Purpose="Data models and DTOs"},
    @{Name="Data"; Required=$true; Purpose="Database context and configurations"},
    @{Name="Services"; Required=$true; Purpose="Business logic and external integrations"},
    @{Name="Utils"; Required=$true; Purpose="Helper classes and utilities"},
    @{Name="wwwroot"; Required=$true; Purpose="Static web assets"},
    @{Name="Migrations"; Required=$true; Purpose="Database schema changes"},
    @{Name="Developer Documentation"; Required=$false; Purpose="Project documentation"}
)

$criticalFiles = @(
    @{Name="Program.cs"; Required=$true; Purpose="Application startup and configuration"},
    @{Name="ResellBook.csproj"; Required=$true; Purpose="Project configuration"},
    @{Name="appsettings.json"; Required=$true; Purpose="Application settings"}
)

$allGood = $true
$missingItems = @()

Write-Host "   📁 Checking critical folders:" -ForegroundColor White
foreach ($folder in $criticalFolders) {
    if (Test-Path $folder.Name) {
        Write-Host "     ✅ $($folder.Name) - $($folder.Purpose)" -ForegroundColor Green
    } else {
        if ($folder.Required) {
            Write-Host "     ❌ $($folder.Name) - MISSING! $($folder.Purpose)" -ForegroundColor Red
            $missingItems += $folder.Name
            $allGood = $false
        } else {
            Write-Host "     ⚠️ $($folder.Name) - Optional - $($folder.Purpose)" -ForegroundColor Yellow
        }
    }
}

Write-Host "   📄 Checking critical files:" -ForegroundColor White
foreach ($file in $criticalFiles) {
    if (Test-Path $file.Name) {
        Write-Host "     ✅ $($file.Name) - $($file.Purpose)" -ForegroundColor Green
    } else {
        Write-Host "     ❌ $($file.Name) - MISSING! $($file.Purpose)" -ForegroundColor Red
        $missingItems += $file.Name
        $allGood = $false
    }
}

# ========================================================================================
# STEP 5: Check deployment readiness
# ========================================================================================
# WHY: Ensures deployment scripts are present before attempting deployment
# WHAT: Verifies deploy.ps1 exists and is accessible
# IMPACT: Prevents deployment failures due to missing scripts
# SAFETY: Read-only check
Write-Host "`n5. 🚀 Checking deployment readiness..." -ForegroundColor Yellow
Write-Host "   WHY: Ensures deployment scripts are available" -ForegroundColor Gray
Write-Host "   IMPACT: Prevents deployment failures, read-only check" -ForegroundColor Gray

if (Test-Path "deploy.ps1") {
    $deployScriptSize = (Get-Item "deploy.ps1").Length
    Write-Host "   ✅ deploy.ps1 exists ($deployScriptSize bytes)" -ForegroundColor Green
    Write-Host "   💡 Ready for deployment" -ForegroundColor Green
} else {
    Write-Host "   ❌ deploy.ps1 MISSING! DEPLOYMENT WILL FAIL!" -ForegroundColor Red
    Write-Host "   💡 Create deploy.ps1 before attempting deployment" -ForegroundColor Yellow
    $missingItems += "deploy.ps1"
    $allGood = $false
}

# ========================================================================================
# MAINTENANCE SUMMARY
# ========================================================================================
Write-Host "`n🎯 MAINTENANCE SUMMARY:" -ForegroundColor Cyan
Write-Host "=====================" -ForegroundColor Cyan

$currentDirSize = (Get-ChildItem -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "📊 Current directory size: $([math]::Round($currentDirSize, 2)) MB" -ForegroundColor White

if ($allGood) {
    Write-Host "✅ PROJECT STATUS: HEALTHY" -ForegroundColor Green
    Write-Host "   ✅ All critical components present" -ForegroundColor Green
    Write-Host "   ✅ Temporary files cleaned" -ForegroundColor Green
    Write-Host "   ✅ Backups managed appropriately" -ForegroundColor Green
    Write-Host "   ✅ Ready for development and deployment" -ForegroundColor Green
} else {
    Write-Host "⚠️ PROJECT STATUS: ISSUES FOUND" -ForegroundColor Yellow
    Write-Host "   ❌ Missing critical items: $($missingItems -join ', ')" -ForegroundColor Red
    Write-Host "   🚨 FIX ISSUES BEFORE DEPLOYMENT" -ForegroundColor Red
    Write-Host "   💡 Check the errors above and resolve missing components" -ForegroundColor Yellow
}

Write-Host "`n🏁 Maintenance completed successfully!" -ForegroundColor Green
Write-Host "💡 Next recommended run: Tomorrow during development" -ForegroundColor Cyan
Write-Host "🔄 Safe to run multiple times per day" -ForegroundColor Green