# Azure Cost Optimization - Data Cleanup Script
# Run this script periodically to clean up old data and reduce database costs

param(
    [int]$DaysToKeepLogs = 30,
    [int]$DaysToKeepSearchHistory = 90,
    [int]$DaysToKeepOldLocations = 180
)

Write-Host "🧹 Starting Azure Cost Optimization Data Cleanup..." -ForegroundColor Green
Write-Host "Keeping logs for: $DaysToKeepLogs days" -ForegroundColor Yellow
Write-Host "Keeping search history for: $DaysToKeepSearchHistory days" -ForegroundColor Yellow
Write-Host "Keeping locations for: $DaysToKeepOldLocations days" -ForegroundColor Yellow

$cutoffDate = Get-Date
$logCutoff = $cutoffDate.AddDays(-$DaysToKeepLogs)
$searchCutoff = $cutoffDate.AddDays(-$DaysToKeepSearchHistory)
$locationCutoff = $cutoffDate.AddDays(-$DaysToKeepOldLocations)

# Note: This is a template script. In production, you would:
# 1. Connect to your Azure SQL Database
# 2. Run cleanup queries
# 3. Archive data to cheaper storage if needed
# 4. Update this script with actual database connection logic

Write-Host "📅 Cutoff dates:" -ForegroundColor Cyan
Write-Host "  Logs: $logCutoff" -ForegroundColor White
Write-Host "  Search History: $searchCutoff" -ForegroundColor White
Write-Host "  Old Locations: $locationCutoff" -ForegroundColor White

Write-Host "`n🗑️  Sample cleanup queries (run these in your database):" -ForegroundColor Yellow
Write-Host "-- Clean up old logs" -ForegroundColor Gray
Write-Host "DELETE FROM Logs WHERE CreatedAt < '$logCutoff';" -ForegroundColor White

Write-Host "`n-- Archive old search history" -ForegroundColor Gray
Write-Host "-- (Consider moving to archive table instead of deleting)" -ForegroundColor White
Write-Host "SELECT * INTO SearchHistory_Archive FROM UserSearchLogs WHERE SearchDate < '$searchCutoff';" -ForegroundColor White
Write-Host "DELETE FROM UserSearchLogs WHERE SearchDate < '$searchCutoff';" -ForegroundColor White

Write-Host "`n-- Clean up old location history (keep only latest per user)" -ForegroundColor Gray
Write-Host "DELETE FROM UserLocations WHERE CreateDate < '$locationCutoff' AND UserId IN (SELECT UserId FROM UserLocations GROUP BY UserId HAVING COUNT(*) > 1);" -ForegroundColor White

Write-Host "`n-- Rebuild indexes after cleanup" -ForegroundColor Gray
Write-Host "ALTER INDEX ALL ON Logs REBUILD;" -ForegroundColor White
Write-Host "ALTER INDEX ALL ON UserSearchLogs REBUILD;" -ForegroundColor White
Write-Host "ALTER INDEX ALL ON UserLocations REBUILD;" -ForegroundColor White

Write-Host "`n✅ Data cleanup script generated!" -ForegroundColor Green
Write-Host "💡 Run these queries during low-traffic hours to minimize impact." -ForegroundColor Cyan
Write-Host "💰 Expected savings: 10-30% reduction in database storage costs." -ForegroundColor Green