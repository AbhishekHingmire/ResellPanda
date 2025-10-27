# 🚀 Azure Resource Setup Guide (Free Tier)

This guide covers creating the necessary Azure resources (App Service and SQL Database) when they don't exist yet. Both manual (portal) and scripted (PowerShell/Azure CLI) methods are provided.

## 📋 Prerequisites

1. **Azure Account**: Free Azure account with credit (https://azure.microsoft.com/free/)
2. **Azure CLI**: Install from https://aka.ms/installazurecliwindows
3. **PowerShell**: Windows PowerShell (pre-installed)

### Login to Azure CLI
```powershell
az login
```

## 🖥️ Method 1: Manual Setup via Azure Portal (Free Tier)

### Step 1: Create Resource Group
1. Go to [Azure Portal](https://portal.azure.com)
2. Click "Resource groups" in the left menu
3. Click "Create"
4. **Subscription**: Your free subscription
5. **Resource group name**: `resell-panda-rg`
6. **Region**: East US (or your preferred region)
7. Click "Review + create" → "Create"

### Step 2: Create Azure SQL Database (Free Tier)
1. In Azure Portal, search for "SQL databases"
2. Click "Create"
3. **Basics**:
   - **Subscription**: Your free subscription
   - **Resource group**: `resell-panda-rg` (select existing)
   - **Database name**: `ResellBook-db`
   - **Server**: Click "Create new"
     - **Server name**: `resellbook-server` (must be globally unique)
     - **Location**: East US
     - **Authentication method**: Use SQL authentication
     - **Server admin login**: `reselladmin`
     - **Password**: Choose a strong password (save this!)
     - **Confirm password**: Same password
   - **Want to use SQL elastic pool?**: No
   - **Workload environment**: Development
4. **Networking**:
   - **Connectivity method**: Public endpoint
   - **Allow Azure services and resources to access this server**: Yes
   - **Add current client IP address**: Yes
5. **Security**:
   - **Enable Microsoft Defender for SQL**: Later (free tier)
   - **Ledger**: Disabled
6. **Additional settings**:
   - **Use existing data**: None
   - **Collation**: SQL_Latin1_General_CP1_CI_AS
7. **Review + create** → **Create**

**Note**: Azure SQL Database free tier provides 250 GB storage and basic performance.

### Step 3: Create Azure App Service (Free Tier)
1. In Azure Portal, search for "App Services"
2. Click "Create"
3. **Basics**:
   - **Subscription**: Your free subscription
   - **Resource group**: `resell-panda-rg` (select existing)
   - **Name**: `ResellBook20250929183655` (must be globally unique)
   - **Publish**: Code
   - **Runtime stack**: .NET 8 (LTS)
   - **Operating System**: Windows
   - **Region**: East US
4. **App Service Plan**:
   - **Windows Plan**: Create new
     - **Name**: `resellbook-plan`
     - **Pricing tier**: Free F1 (1 GB RAM, 1 GB storage)
5. **Review + create** → **Create**

**Note**: Free App Service plan has limitations but sufficient for development/testing.

### Step 4: Configure Database Connection
1. Go to your SQL Database in Azure Portal
2. Under "Settings" → "Connection strings"
3. Copy the "ADO.NET" connection string
4. Replace `{your_password}` with your actual password
5. Update your `appsettings.json` with this connection string

## 💻 Method 2: Scripted Setup (PowerShell + Azure CLI)

### Resource Creation Script

The script `create-resources.ps1` has been created in your project root. Edit the password and run it:

```powershell
# Edit the password in create-resources.ps1 first
# Then run:
.\create-resources.ps1
```

### Script Details

The script will:
1. Create resource group
2. Create SQL Server and Database (Basic tier)
3. Configure firewall rules
4. Create App Service Plan (Free) and App Service
5. Output the connection string for your appsettings.json

## 🔧 Post-Setup Configuration

### Update appsettings.json

After creating resources, update your `appsettings.json` with the connection string provided by the script.

### Run Database Migrations

```powershell
# Apply Entity Framework migrations
dotnet ef database update
```

## 🚀 Next Steps

Once resources are created and configured:

1. Run your deployment script: `.\deploy.ps1`
2. Test the application at: `https://ResellBook20250929183655.azurewebsites.net`

## 🆓 Free Tier Limitations

- **App Service Free Plan**: 60 CPU minutes/day, 1 GB RAM, 1 GB storage
- **SQL Database Basic**: 2 GB storage, 5 DTUs
- **No SLA**: Not recommended for production

For production, upgrade to paid tiers (B1 for App Service, S0 for SQL).

## 🆘 Troubleshooting

### Common Issues

1. **Name already taken**: Choose different names for globally unique resources
2. **Quota exceeded**: Free tier has limits on certain resources
3. **Region not available**: Try different Azure regions

### Check Resource Status

```powershell
# List resources in resource group
az resource list --resource-group resell-panda-rg --output table
```

---

**Last Updated**: October 27, 2025