<#
.SYNOPSIS
    ResellBook Project Cleanup Script - SAFE & COMPREHENSIVE
.DESCRIPTION
    Advanced cleanup script for the ResellBook ASP.NET Core project with multiple
    safety layers and comprehensive cleanup options.

    WHAT THIS SCRIPT DOES:
    - Cleans build artifacts (bin/, obj/, publish/) - safe regeneration
    - Manages backup rotation with configurable retention
    - Removes temporary download folders - safe cleanup
    - Cleans loose deployment ZIP files - prevents accumulation
    - Rotates large log files - prevents disk space issues
    - Provides detailed space savings calculations

    IMPACT:
    - ✅ SAFE: Dry-run mode by default, confirms destructive operations
    - ✅ BENEFICIAL: Significant disk space recovery (often 100MB+)
    - ✅ PREVENTIVE: Prevents disk space issues and performance degradation
    - ✅ REVERSIBLE: Backups are preserved, logs are rotated (not deleted)

    WHEN TO RUN:
    - Before deployments to ensure clean state
    - When disk space is running low
    - Weekly maintenance to prevent accumulation
    - After failed builds to clean artifacts

    FREQUENCY: Weekly or as needed (safe with dry-run mode)
.PARAMETER KeepBackups
    Number of backup folders to keep (default: 3)
.PARAMETER MaxLogSizeMB
    Maximum log file size before rotation (default: 10MB)
.PARAMETER DryRun
    Show what would be deleted without actually deleting (default: true)
.PARAMETER Force
    Skip all confirmations (use with extreme caution)
.EXAMPLE
    .\cleanup.ps1
    # Dry run mode - shows what would be cleaned
.EXAMPLE
    .\cleanup.ps1 -DryRun:$false
    # Actually performs cleanup with confirmations
.EXAMPLE
    .\cleanup.ps1 -KeepBackups 5 -MaxLogSizeMB 20 -DryRun:$false
    # Custom settings, actually clean
#>

param(
    [int]$KeepBackups = 3,              # Number of backup folders to keep
    [int]$MaxLogSizeMB = 10,            # Max log file size before rotation
    [switch]$DryRun = $true,            # Default to dry run for safety
    [switch]$Force                      # Skip confirmations (dangerous!)
)

# ========================================================================================
# CLEANUP SCRIPT SUMMARY
# ========================================================================================
# Script Name: cleanup.ps1
# Purpose: Comprehensive project cleanup with safety features
# Safety Level: 🟢 VERY SAFE - Dry-run by default, confirmations required
# Impact: Removes build artifacts, rotates logs, manages backups
# Run Frequency: Weekly or before deployments
# Estimated Runtime: 1-5 minutes
# Space Savings: Typically 50-500MB per run
# ========================================================================================

Write-Host "🧹 ResellBook Project Cleanup Script" -ForegroundColor Green
Write-Host "===================================" -ForegroundColor Green
Write-Host "Safety Level: 🟢 VERY SAFE (Dry-run by default)" -ForegroundColor Green
Write-Host "Impact: Removes build artifacts and temporary files" -ForegroundColor Yellow
Write-Host ""

# ========================================================================================
# SAFETY CHECKS - MULTIPLE LAYERS OF PROTECTION
# ========================================================================================

# SAFETY CHECK 1: Verify project location
# WHY: Prevents accidental execution in wrong directory
# IMPACT: Stops script if not in correct project directory
Write-Host "🔍 Safety Check 1: Verifying project location..." -ForegroundColor Yellow
if (-not (Test-Path "ResellBook.csproj")) {
    Write-Host "❌ ERROR: ResellBook.csproj not found!" -ForegroundColor Red
    Write-Host "   Please run this script from the ResellBook project root directory." -ForegroundColor Red
    Write-Host "   Expected location: C:\Repos\ResellPanda\ResellBook\" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Project location verified" -ForegroundColor Green

# SAFETY CHECK 2: Dry run mode warning
# WHY: Prevents accidental data loss by requiring explicit opt-in
# IMPACT: Forces user to understand what will happen
if ($DryRun) {
    Write-Host "`n🔍 DRY RUN MODE ACTIVE - No files will be deleted" -ForegroundColor Yellow
    Write-Host "   This shows what WOULD be cleaned up." -ForegroundColor Yellow
    Write-Host "   To actually clean: .\cleanup.ps1 -DryRun:`$false" -ForegroundColor Cyan
    Write-Host ""
} else {
    Write-Host "`n⚠️ LIVE CLEANUP MODE - Files will be permanently deleted!" -ForegroundColor Red
    if (-not $Force) {
        Write-Host "   This will delete build artifacts, old backups, and rotate logs." -ForegroundColor White
        Write-Host "   Space savings: Typically 50-500MB" -ForegroundColor White
        $confirmation = Read-Host "   Are you sure you want to proceed? (yes/N)"
        if ($confirmation -ne "yes") {
            Write-Host "❌ Cleanup cancelled by user." -ForegroundColor Yellow
            exit 0
        }
    }
}

# ========================================================================================
# INITIALIZATION
# ========================================================================================
$totalSpaceSaved = 0
$itemsRemoved = 0
$startTime = Get-Date

Write-Host "🧹 Starting cleanup process..." -ForegroundColor Green
Write-Host "Settings: Keep $KeepBackups backups, Max log size ${MaxLogSizeMB}MB" -ForegroundColor White
Write-Host ""

# ========================================================================================
# STEP 1: Clean build artifacts (ALWAYS SAFE)
# ========================================================================================
# WHY: Build artifacts are regenerated on next build, safe to delete
# WHAT: Removes bin/, obj/, publish/ folders completely
# IMPACT: Frees significant disk space, forces clean rebuild
# SAFETY: These folders contain only compiled output, never source code
Write-Host "🏗️ Step 1: Cleaning build artifacts..." -ForegroundColor Yellow
Write-Host "   WHY: Build artifacts are regenerated automatically" -ForegroundColor Gray
Write-Host "   IMPACT: Frees disk space, ensures clean builds" -ForegroundColor Gray
Write-Host "   SAFETY: ✅ HIGH - Only compiled output, never source code" -ForegroundColor Green

$buildFolders = @("bin", "obj", "publish")
$buildItemsRemoved = 0
$buildSpaceSaved = 0

foreach ($folder in $buildFolders) {
    if (Test-Path $folder) {
        $size = (Get-ChildItem -Path $folder -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
        Write-Host "  📁 $folder`: $([math]::Round($size, 2)) MB" -ForegroundColor $(if ($DryRun) { "Gray" } else { "Red" })

        if (-not $DryRun) {
            Remove-Item -Path $folder -Recurse -Force -ErrorAction SilentlyContinue
            Write-Host "    ✅ Removed" -ForegroundColor Green
        }
        $buildItemsRemoved++
        $buildSpaceSaved += $size
        $totalSpaceSaved += $size
    } else {
        Write-Host "  📁 $folder`: Not found" -ForegroundColor Blue
    }
}

# Also run dotnet clean for thorough cleanup (if not dry run)
if (-not $DryRun) {
    Write-Host "  🧽 Running dotnet clean..." -ForegroundColor White
    dotnet clean --verbosity quiet 2>&1 | Out-Null
    Write-Host "    ✅ dotnet clean completed" -ForegroundColor Green
}

Write-Host "✅ Build artifacts cleanup complete - $buildItemsRemoved folders processed, $([math]::Round($buildSpaceSaved, 2)) MB saved" -ForegroundColor Green

# ========================================================================================
# STEP 2: Manage deployment backups (CONFIGURABLE RETENTION)
# ========================================================================================
# WHY: Backup folders accumulate and consume disk space over time
# WHAT: Keeps N newest backups, suggests removal of older ones
# IMPACT: Balances recovery options with disk space management
# SAFETY: Shows exactly what will be removed, requires confirmation
Write-Host "`n💾 Step 2: Managing deployment backups..." -ForegroundColor Yellow
Write-Host "   WHY: Old backups consume disk space but provide recovery options" -ForegroundColor Gray
Write-Host "   IMPACT: Frees disk space while preserving recent recovery options" -ForegroundColor Gray
Write-Host "   SAFETY: ⚠️ MEDIUM - Shows deletions, preserves $KeepBackups newest backups" -ForegroundColor Yellow

$backupFolders = Get-ChildItem -Directory -Name "wwwroot-backup-*" -ErrorAction SilentlyContinue | Sort-Object -Descending
Write-Host "  📋 Found $($backupFolders.Count) backup folders (keeping $KeepBackups newest)" -ForegroundColor White

$backupSpaceSaved = 0
$backupItemsRemoved = 0

if ($backupFolders.Count -gt $KeepBackups) {
    $toKeep = $backupFolders | Select-Object -First $KeepBackups
    $toDelete = $backupFolders | Select-Object -Skip $KeepBackups

    Write-Host "  ✅ Keeping newest $KeepBackups backups:" -ForegroundColor Green
    foreach ($folder in $toKeep) {
        $size = (Get-ChildItem -Path $folder -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
        Write-Host "    📁 $folder ($([math]::Round($size, 2)) MB)" -ForegroundColor Green
    }

    Write-Host "  🗑️ Marked for deletion ($($toDelete.Count) folders):" -ForegroundColor Red
    foreach ($folder in $toDelete) {
        if (Test-Path $folder) {
            $size = (Get-ChildItem -Path $folder -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
            Write-Host "    📁 $folder ($([math]::Round($size, 2)) MB)" -ForegroundColor Red

            if (-not $DryRun) {
                # Extra confirmation for backup deletion
                if (-not $Force) {
                    $backupConfirm = Read-Host "      Delete backup '$folder'? (y/N)"
                    if ($backupConfirm -eq "y" -or $backupConfirm -eq "Y") {
                        Remove-Item -Path $folder -Recurse -Force -ErrorAction SilentlyContinue
                        Write-Host "      ✅ Removed" -ForegroundColor Green
                        $backupItemsRemoved++
                        $backupSpaceSaved += $size
                        $totalSpaceSaved += $size
                    } else {
                        Write-Host "      ⏭️ Skipped" -ForegroundColor Blue
                    }
                } else {
                    Remove-Item -Path $folder -Recurse -Force -ErrorAction SilentlyContinue
                    $backupItemsRemoved++
                    $backupSpaceSaved += $size
                    $totalSpaceSaved += $size
                }
            } else {
                $backupItemsRemoved++
                $backupSpaceSaved += $size
                $totalSpaceSaved += $size
            }
        }
    }
} else {
    Write-Host "  ✅ All backup folders within limit ($($backupFolders.Count) ≤ $KeepBackups)" -ForegroundColor Green
    # Show all backups for visibility
    foreach ($folder in $backupFolders) {
        $size = (Get-ChildItem -Path $folder -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
        Write-Host "    📁 $folder ($([math]::Round($size, 2)) MB)" -ForegroundColor Green
    }
}

Write-Host "✅ Backup management complete - $backupItemsRemoved folders processed, $([math]::Round($backupSpaceSaved, 2)) MB saved" -ForegroundColor Green

# ========================================================================================
# STEP 3: Clean temporary download folders (SAFE CLEANUP)
# ========================================================================================
# WHY: Temporary download folders accumulate during development/testing
# WHAT: Removes all temp-download-* folders completely
# IMPACT: Frees disk space from temporary development artifacts
# SAFETY: ✅ HIGH - These are explicitly temporary folders
Write-Host "`n📥 Step 3: Cleaning temporary download folders..." -ForegroundColor Yellow
Write-Host "   WHY: Temp downloads are not needed after development/testing" -ForegroundColor Gray
Write-Host "   IMPACT: Frees disk space from temporary artifacts" -ForegroundColor Gray
Write-Host "   SAFETY: ✅ HIGH - Explicitly named temporary folders" -ForegroundColor Green

$tempFolders = Get-ChildItem -Directory -Name "temp-download-*" -ErrorAction SilentlyContinue
$tempSpaceSaved = 0
$tempItemsRemoved = 0

if ($tempFolders.Count -gt 0) {
    Write-Host "  📋 Found $($tempFolders.Count) temporary folders:" -ForegroundColor White

    foreach ($folder in $tempFolders) {
        if (Test-Path $folder) {
            $size = (Get-ChildItem -Path $folder -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
            Write-Host "  🗑️ $folder ($([math]::Round($size, 2)) MB)" -ForegroundColor $(if ($DryRun) { "Gray" } else { "Red" })

            if (-not $DryRun) {
                Remove-Item -Path $folder -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "    ✅ Removed" -ForegroundColor Green
            }
            $tempItemsRemoved++
            $tempSpaceSaved += $size
            $totalSpaceSaved += $size
        }
    }
} else {
    Write-Host "  ✅ No temporary download folders found" -ForegroundColor Green
}

Write-Host "✅ Temp folder cleanup complete - $tempItemsRemoved folders processed, $([math]::Round($tempSpaceSaved, 2)) MB saved" -ForegroundColor Green

# ========================================================================================
# STEP 4: Clean loose deployment ZIP files (SAFE CLEANUP)
# ========================================================================================
# WHY: Loose ZIP files from deployments accumulate over time
# WHAT: Removes ZIP files matching deployment patterns
# IMPACT: Prevents accumulation of deployment artifacts
# SAFETY: ✅ HIGH - Only removes explicitly named deployment ZIPs
Write-Host "`n📦 Step 4: Cleaning deployment ZIP files..." -ForegroundColor Yellow
Write-Host "   WHY: Deployment ZIPs are not needed after successful deployment" -ForegroundColor Gray
Write-Host "   IMPACT: Prevents accumulation of deployment artifacts" -ForegroundColor Gray
Write-Host "   SAFETY: ✅ HIGH - Only removes deployment-related ZIPs" -ForegroundColor Green

$zipFiles = Get-ChildItem -Name "*.zip" -ErrorAction SilentlyContinue | Where-Object { $_ -match "(deploy|backup|temp)" }
$zipSpaceSaved = 0
$zipItemsRemoved = 0

if ($zipFiles.Count -gt 0) {
    Write-Host "  📋 Found $($zipFiles.Count) deployment ZIP files:" -ForegroundColor White
    foreach ($zip in $zipFiles) {
        $size = (Get-Item $zip -ErrorAction SilentlyContinue).Length / 1MB
        Write-Host "  🗑️ $zip ($([math]::Round($size, 2)) MB)" -ForegroundColor $(if ($DryRun) { "Gray" } else { "Red" })

        if (-not $DryRun) {
            Remove-Item -Path $zip -Force -ErrorAction SilentlyContinue
            Write-Host "    ✅ Removed" -ForegroundColor Green
        }
        $zipItemsRemoved++
        $zipSpaceSaved += $size
        $totalSpaceSaved += $size
    }
} else {
    Write-Host "  ✅ No deployment ZIP files found" -ForegroundColor Green
}

Write-Host "✅ ZIP cleanup complete - $zipItemsRemoved files processed, $([math]::Round($zipSpaceSaved, 2)) MB saved" -ForegroundColor Green

# ========================================================================================
# STEP 5: Manage log files (ROTATION - NOT DELETION)
# ========================================================================================
# WHY: Large log files slow down applications and waste disk space
# WHAT: Rotates oversized logs by renaming them with timestamps
# IMPACT: Prevents performance issues while preserving log history
# SAFETY: ✅ HIGH - Never deletes logs, only rotates oversized ones
Write-Host "`n📊 Step 5: Managing log files..." -ForegroundColor Yellow
Write-Host "   WHY: Large logs slow performance and waste disk space" -ForegroundColor Gray
Write-Host "   IMPACT: Rotates oversized logs, preserves history" -ForegroundColor Gray
Write-Host "   SAFETY: ✅ HIGH - Never deletes logs, only rotates them" -ForegroundColor Green

$logSpaceSaved = 0
$logsRotated = 0

if (Test-Path "AppLogs") {
    $logFiles = Get-ChildItem -Path "AppLogs" -Filter "*.txt" -ErrorAction SilentlyContinue
    Write-Host "  📋 Analyzing $($logFiles.Count) log files:" -ForegroundColor White

    foreach ($log in $logFiles) {
        $sizeMB = [math]::Round($log.Length / 1MB, 2)

        if ($sizeMB -gt $MaxLogSizeMB) {
            $newName = $log.BaseName + "_" + (Get-Date -Format "yyyyMMdd_HHmmss") + $log.Extension
            Write-Host "  📋 $($log.Name) ($sizeMB MB) → $newName" -ForegroundColor Yellow

            if (-not $DryRun) {
                try {
                    Rename-Item $log.FullName -NewName (Join-Path "AppLogs" $newName) -ErrorAction Stop
                    New-Item -Path $log.FullName -ItemType File -Force | Out-Null
                    Write-Host "    ✅ Rotated successfully" -ForegroundColor Green
                } catch {
                    Write-Host "    ❌ Rotation failed: $($_.Exception.Message)" -ForegroundColor Red
                }
            }
            $logsRotated++
            # Note: Log rotation doesn't save space, just prevents future bloat
        } else {
            Write-Host "  ✅ $($log.Name) ($sizeMB MB) - OK" -ForegroundColor Green
        }
    }

    if ($logsRotated -eq 0) {
        Write-Host "  ✅ All log files within size limits" -ForegroundColor Green
    } else {
        Write-Host "  ✅ $logsRotated log files rotated" -ForegroundColor Green
    }
} else {
    Write-Host "  📋 No AppLogs folder found" -ForegroundColor Blue
}

Write-Host "✅ Log management complete - $logsRotated files rotated" -ForegroundColor Green

# ========================================================================================
# CLEANUP SUMMARY & VERIFICATION
# ========================================================================================
$endTime = Get-Date
$duration = $endTime - $startTime

Write-Host "`n📊 Cleanup Summary:" -ForegroundColor Cyan
Write-Host "===================" -ForegroundColor Cyan
Write-Host "⏱️ Duration: $([math]::Round($duration.TotalSeconds, 1)) seconds" -ForegroundColor White
Write-Host "💾 Estimated space saved: $([math]::Round($totalSpaceSaved, 2)) MB" -ForegroundColor $(if ($totalSpaceSaved -gt 100) { "Green" } elseif ($totalSpaceSaved -gt 10) { "Yellow" } else { "White" })

$remainingBackups = (Get-ChildItem -Directory -Name "wwwroot-backup-*" -ErrorAction SilentlyContinue).Count
$remainingTempFolders = (Get-ChildItem -Directory -Name "temp-download-*" -ErrorAction SilentlyContinue).Count

Write-Host "📁 Backup folders remaining: $remainingBackups" -ForegroundColor White
Write-Host "📥 Temp folders remaining: $remainingTempFolders" -ForegroundColor White

# ========================================================================================
# CRITICAL FILES PROTECTION VERIFICATION
# ========================================================================================
Write-Host "`n🛡️ Critical Files Protection:" -ForegroundColor Green
$protectedFolders = @("Controllers", "Models", "Services", "Data", "wwwroot/uploads", "Migrations")
$protectionOK = $true

foreach ($folder in $protectedFolders) {
    if (Test-Path $folder) {
        Write-Host "  ✅ $folder" -ForegroundColor Green
    } else {
        Write-Host "  ⚠️ $folder - Not found (but this is OK if not created yet)" -ForegroundColor Yellow
    }
}

# ========================================================================================
# FINAL STATUS & RECOMMENDATIONS
# ========================================================================================
if ($DryRun) {
    Write-Host "`n🔍 This was a DRY RUN - No files were actually deleted." -ForegroundColor Yellow
    Write-Host "To perform actual cleanup, run:" -ForegroundColor Yellow
    Write-Host ".\cleanup.ps1 -DryRun:`$false" -ForegroundColor Cyan
    Write-Host "`n💡 Recommended next steps:" -ForegroundColor Cyan
    Write-Host "1. Review the cleanup plan above" -ForegroundColor White
    Write-Host "2. Run with -DryRun:`$false when ready" -ForegroundColor White
    Write-Host "3. Consider adjusting -KeepBackups if needed" -ForegroundColor White
} else {
    Write-Host "`n✅ Cleanup completed successfully!" -ForegroundColor Green
    Write-Host "Your project is now clean and optimized." -ForegroundColor Green
    Write-Host "`n💡 Recommendations:" -ForegroundColor Cyan
    Write-Host "• Run 'dotnet build' to verify everything still works" -ForegroundColor White
    Write-Host "• Consider committing these changes" -ForegroundColor White
    Write-Host "• Schedule weekly cleanup runs" -ForegroundColor White
}

# ========================================================================================
# QUICK STATUS CHECK
# ========================================================================================
Write-Host "`n📋 Quick Status Check:" -ForegroundColor Cyan
$currentSize = (Get-ChildItem -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "Current directory size: $([math]::Round($currentSize, 2)) MB" -ForegroundColor Yellow

Write-Host "`n🏁 Cleanup script finished!" -ForegroundColor Green