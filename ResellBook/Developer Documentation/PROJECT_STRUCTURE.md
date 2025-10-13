# 📁 ResellBook Project Architecture & File Structure
### 🎓 **Complete .NET 8 Project Learning Guide - From Beginner to Advanced**

> **🎯 Learning Philosophy:** Understand every file and folder's purpose to master .NET development  
> **⏰ Learning Investment:** 90 minutes to understand complete .NET project architecture  
> **� Skill Development:** Professional .NET project structure, build system, and deployment concepts  
> **🏗️ End Knowledge:** Deep understanding of how .NET applications are structured and deployed  

---

## 🧠 **What You'll Learn**

### **🏗️ .NET Project Architecture Fundamentals**
- **MSBuild System:** How .NET compiles and builds applications
- **Dependency Management:** How NuGet packages are managed and resolved
- **Configuration System:** How settings work in development vs production
- **Runtime Environment:** How .NET applications execute and manage memory

### **📁 File System Organization Principles**
- **Separation of Concerns:** Why different types of code go in different folders
- **Build Artifacts:** Understanding what files are generated vs what you write
- **Deployment Packaging:** How applications are prepared for production
- **Static Assets Management:** How web files and user uploads are organized

---

## 🏗️ **Complete Project Structure Analysis**

### **🎯 Source Code Folders (What You Write)**
```
ResellBook/                                    ← 🏠 Root Project Directory
├── 📁 Controllers/         [API Layer]       ← 🌐 HTTP Request Handlers
├── 📁 Data/               [Data Access]      ← 🗄️ Database Connection Layer  
├── 📁 Helpers/            [Utilities]        ← 🔧 Reusable Helper Functions
├── 📁 Models/             [Data Structure]   ← 📊 Data Models & DTOs
├── 📁 Services/           [Business Logic]   ← 💼 Core Application Logic
├── 📁 Utils/              [Static Tools]     ← 🛠️ Static Utility Classes
├── 📁 Migrations/         [Database Schema]  ← 🔄 Database Version Control
├── 📁 Properties/         [Project Config]   ← ⚙️ Project & Deployment Settings
└── 📁 Developer Documentation/ [Learning]    ← 📚 Project Documentation
```

**🎓 Learning Concept - Layered Architecture:**
This follows the **Clean Architecture** pattern where:
- **Controllers** handle HTTP requests (Presentation Layer)
- **Services** contain business logic (Application Layer)  
- **Data** manages database access (Infrastructure Layer)
- **Models** define data structures (Domain Layer)

### **🤖 Build System Folders (Auto-Generated)**
```
ResellBook/
├── 📁 bin/                [Compiled Output]   ← ⚡ Your Code Becomes Executable Files
├── 📁 obj/                [Build Cache]       ← 🏗️ Build System Working Directory
├── 📁 publish/            [Deploy Package]    ← 📦 Production-Ready Application
└── 📁 .config/            [Build Tools]       ← 🔧 Development Tool Configuration
```

**🎓 Learning Concept - Build Process:**
```
Source Code → Compilation → Testing → Packaging → Deployment
    ↓            ↓            ↓          ↓           ↓
 Your .cs    obj/Debug    Tests Run   publish/    Azure
  Files      folder       (if any)    folder      Cloud
```

### **🌐 Runtime Folders (Created During Execution)**
```
ResellBook/
├── 📁 wwwroot/            [Web Assets]        ← 🌍 Public Web Files & User Uploads
│   └── 📁 uploads/        [User Files]        ← 📸 User-Uploaded Images & Documents
└── 📁 AppLogs/            [Application Logs]  ← 📊 Runtime Information & Error Tracking
```

### **💾 Deployment Folders (Created by Deploy Scripts)**
```
ResellBook/
├── 📁 wwwroot-backup-YYYYMMDD-HHMMSS/        ← 🛡️ User Data Backups
├── 📁 temp-download-YYYYMMDD-HHMMSS/         ← 📥 Temporary Download Staging
└── 📄 Various .zip files                      ← 📦 Compressed Backup Archives
```

---

## 🔍 **Deep Dive: Critical Files Explained**

### **⚙️ Project Configuration Files**

#### **📄 `ResellBook.csproj` - The Project Blueprint**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">          <!-- 🎯 This is a web application project -->
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>   <!-- 📌 Targets .NET 8 runtime -->
    <Nullable>enable</Nullable>                 <!-- 🛡️ Enables null safety features -->
    <ImplicitUsings>enable</ImplicitUsings>     <!-- 🚀 Auto-imports common namespaces -->
  </PropertyGroup>
  
  <ItemGroup>                                   <!-- 📦 External packages (like npm for Node.js) -->
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />           <!-- 🔐 Password hashing -->
    <PackageReference Include="MailKit" Version="4.14.0" />                  <!-- 📧 Email sending -->
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.11" />  <!-- 🗄️ Database ORM -->
    <!-- More packages... -->
  </ItemGroup>
</Project>
```

**🎓 Why This Matters:**
- **`.csproj` is like `package.json` in Node.js** - defines dependencies and build settings
- **SDK Web** tells .NET this is a web application (includes MVC, API, middleware features)
- **TargetFramework** determines which .NET version features you can use
- **PackageReferences** are external libraries (like importing modules in Python)

#### **📄 `Program.cs` - Application Entry Point**
**🎯 Learning Concept:** Every .NET application starts here - this is like `main()` in C++ or `if __name__ == "__main__"` in Python

```csharp
var builder = WebApplication.CreateBuilder(args);    // 🏗️ Creates web application factory

// � Configure services (dependency injection container)
builder.Services.AddControllers();                  // 🌐 Enable API controllers
builder.Services.AddDbContext<AppDbContext>(...);   // 🗄️ Register database context
builder.Services.AddScoped<EmailService>();         // 💼 Register business services

var app = builder.Build();                           // 🚀 Build the application

// 🛤️ Configure request pipeline (middleware)
app.UseAuthentication();                             // 🔐 Handle authentication
app.UseAuthorization();                              // 🛡️ Handle authorization
app.MapControllers();                                // 🗺️ Route requests to controllers

app.Run();                                           // ▶️ Start the application
```

**🎓 Professional Concepts:**
- **Dependency Injection:** Framework manages object creation and lifetime
- **Middleware Pipeline:** Requests flow through authentication → routing → controllers
- **Service Registration:** Tell .NET how to create and manage services

#### **� `appsettings.json` vs `appsettings.Development.json`**

**`appsettings.json` - Production Configuration:**
```json
{
  "Logging": {                                    // 📊 How much detail to log
    "LogLevel": {
      "Default": "Information",                   // 🔍 Normal operations logging
      "Microsoft.AspNetCore": "Warning"           // ⚠️ Only warnings from framework
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=azure-sql..."   // 🌐 Production database (Azure)
  },
  "AllowedHosts": "*"                            // 🌍 Accept requests from any host
}
```

**`appsettings.Development.json` - Local Development:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost..."   // 💻 Local database for development
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug"                         // 🐛 Verbose logging for debugging
    }
  }
}
```

**🎓 Why Separate Files:**
- **Environment-Specific Settings:** Different databases, logging levels, API keys
- **Security:** Keep production secrets out of development environment
- **.NET automatically merges them:** Development.json overrides base appsettings.json
- **Deployment Safety:** Never commit sensitive production settings to git

---

## 🏗️ **Understanding the Build System (obj/ and bin/ folders)**

### **📁 `obj/` Folder - Build System's Working Directory**

**🎓 Learning Concept:** This is like the "scratch pad" where .NET does its work

```
obj/
├── 📁 Debug/              ← 🐛 Debug build configuration files
│   ├── 📁 net8.0/         ← 🎯 .NET 8 specific build artifacts
│   └── 📁 net9.0/         ← 🆕 .NET 9 specific build artifacts (if multi-targeting)
├── 📁 Release/            ← 🚀 Production build configuration files
├── 📄 project.assets.json ← 📦 Complete dependency tree
└── 📄 *.nuget.*          ← 🔧 NuGet package management files
```

#### **🤔 Why Debug AND Release Folders?**
```
Debug Configuration:
├── 🐛 Includes debugging symbols (.pdb files)
├── 🔍 No code optimization (easier to debug)
├── 📊 Detailed error information
├── 🏃 Slower execution but easier troubleshooting
└── 💾 Larger file sizes

Release Configuration:
├── ⚡ Optimized code (faster execution)
├── 🗜️ Smaller file sizes (removed debug info)
├── 🚀 Production-ready performance
├── 🔒 Harder to debug but more secure
└── � Ready for deployment
```

#### **🤔 Why .NET 8 AND .NET 9 Folders?**
**When you see both:**
- **Multi-targeting:** Project can run on multiple .NET versions
- **Forward Compatibility:** Testing against newer .NET versions
- **Library Projects:** Creating packages that work across .NET versions

**In Our Project:** Currently targets only .NET 8, so you'll mainly see `net8.0/`

#### **🔍 Key Files in `obj/Debug/net8.0/`:**

```
📄 ResellBook.dll                    ← 🎯 Your compiled application
📄 ResellBook.pdb                    ← 🐛 Debug symbols (maps code to line numbers)
📄 ResellBook.AssemblyInfo.cs        ← 📋 Assembly metadata (version, description)
📄 ApiEndpoints.json                 ← 🗺️ Generated API route information
📄 *.cache files                     ← ⚡ Build performance optimization
📄 staticwebassets.*.json            ← 🌐 Web assets tracking (CSS, JS, images)
```

**🎓 Professional Insight:**
- **Never commit obj/ to git** - it's regenerated every build
- **Delete obj/ if build is weird** - forces complete rebuild
- **.pdb files are crucial** - they enable debugging with line numbers

### **📁 `bin/` Folder - Final Compiled Output**

```
bin/
├── 📁 Debug/net8.0/              ← 🐛 Debug version of your application
│   ├── 📄 ResellBook.exe         ← ▶️ Windows executable
│   ├── 📄 ResellBook.dll         ← 🎯 Main application assembly
│   ├── 📄 *.dll files            ← 📦 All dependency libraries
│   ├── 📄 appsettings*.json      ← ⚙️ Configuration files (copied from root)
│   └── 📄 web.config             ← 🌐 IIS deployment configuration
└── 📁 Release/net8.0/            ← 🚀 Production-optimized version
```

**🎓 Key Learning Points:**
1. **bin/Debug** = Your app + all dependencies ready to run locally
2. **bin/Release** = Optimized version ready for production deployment
3. **All .dll files** = Every NuGet package your app needs
4. **Configuration copied** = appsettings.json files available at runtime

**🤔 Why So Many .dll Files?**
```
Your Application Dependencies:
├── 📄 ResellBook.dll              ← Your code
├── 📄 Microsoft.AspNetCore.*.dll  ← Web framework
├── 📄 EntityFrameworkCore.*.dll   ← Database ORM
├── 📄 BCrypt.dll                  ← Password hashing
├── 📄 MailKit.dll                 ← Email functionality
└── 📄 System.*.dll                ← .NET runtime libraries
```

Each .dll is a compiled library providing specific functionality.

---

## 🌐 **Runtime Environment Analysis**

### **📁 `wwwroot/` Folder - Public Web Assets**

**🎓 Learning Concept:** This is the "public" folder that browsers can directly access

```
wwwroot/
├── 📁 uploads/                    ← 📸 User-uploaded files (CRITICAL DATA)
│   └── 📁 books/                  ← 📚 Book images uploaded by users
│       ├── 📄 book123_image1.jpg  ← 🖼️ Individual book images
│       └── 📄 book456_image2.png  ← 🖼️ Various image formats
├── 📁 css/                        ← 🎨 Stylesheets (if any)
├── 📁 js/                         ← ⚡ JavaScript files (if any)
└── 📁 lib/                        ← 📚 Client-side libraries (Bootstrap, jQuery, etc.)
```

**🎓 Why This Folder Exists:**
- **Static File Serving:** Web servers can serve files directly without going through .NET
- **Performance:** Images, CSS, JS served faster than processed through controllers
- **Security Boundary:** Only files here are publicly accessible via URL
- **User Uploads:** Safe place to store user-generated content

**🔍 URL Mapping Examples:**
```
File: wwwroot/uploads/books/book123.jpg
URL:  https://yourapp.com/uploads/books/book123.jpg

File: wwwroot/css/site.css  
URL:  https://yourapp.com/css/site.css
```

**🛡️ Security Considerations:**
```
✅ Safe to store here:
├── 📸 Images (jpg, png, gif)
├── 📄 Documents (pdf)
├── 🎨 CSS files
└── ⚡ JavaScript files

❌ NEVER store here:
├── 🔐 Configuration files
├── 🗄️ Database files
├── 🔑 API keys or passwords
└── 📝 Server-side code
```

### **📁 `AppLogs/` Folder - Application Monitoring**

```
AppLogs/
├── 📄 normal.txt              ← 📊 Regular application events
│   ├── User login events
│   ├── API request logs
│   ├── Database operations
│   └── Performance metrics
└── 📄 critical.txt            ← 🚨 Critical errors and exceptions
    ├── Unhandled exceptions
    ├── Database connection failures
    ├── Email sending failures
    └── Security violations
```

**🎓 Professional Logging Strategy:**
```
Log Levels Hierarchy:
├── 🔍 Debug    ← Development only (very detailed)
├── 📊 Info     ← Normal operations (user actions)
├── ⚠️ Warning  ← Concerning but not breaking
├── 🚨 Error    ← Something failed but app continues
└── 💀 Critical ← Application-breaking failures
```

**📊 What Gets Logged in Our App:**
```csharp
// In Utils/SimpleLogger.cs
SimpleLogger.LogNormal("Authentication", "Login", $"User {userId} logged in successfully");
SimpleLogger.LogCritical("Database", "Connection", "Failed to connect to SQL Server");
```

---

## 💾 **Deployment & Backup System Analysis**

### **📁 Backup Folders (Created by Deploy Script)**

**🎓 Why Backups Matter:**
```
Production Deployment Risk:
├── 🚨 User data loss (uploaded images)
├── ⏰ Downtime during deployment  
├── 🐛 New version bugs
└── 🔄 Need to rollback quickly
```

**Our Backup Strategy:**
```
wwwroot-backup-YYYYMMDD-HHMMSS/
├── 📸 Complete copy of user uploads
├── ⏰ Timestamped for version tracking
├── 🗜️ Compressed for storage efficiency
└── 🚀 Ready for instant rollback
```

**Example Backup Structure:**
```
wwwroot-backup-20251011-235015/        ← � October 11, 2025 at 23:50:15
├── 📁 uploads/                        ← 📸 All user files preserved
│   └── �📁 books/                      ← 📚 Book images backed up
│       ├── book123_image1.jpg         ← 🖼️ Every user image safe
│       └── book456_image2.png
└── 📄 wwwroot-backup.zip              ← 🗜️ Compressed for storage
```

### **📁 Temporary Deployment Folders**

```
temp-download-YYYYMMDD-HHMMSS/         ← 📥 Staging area for deployments
├── 📦 Downloaded application files
├── 🔍 Pre-deployment validation
└── 🧹 Cleaned up after successful deployment
```

**🎓 Deployment Process Flow:**
```
1. 📥 Download new version → temp-download-*/
2. 💾 Backup current wwwroot → wwwroot-backup-*/
3. 🔍 Validate new version
4. 🚀 Deploy new version  
5. 🧹 Cleanup temporary files
6. ✅ Verify deployment success
```

---

## 🔧 **Development vs Production File Differences**

### **🏠 Local Development Environment**

```
Development Setup:
├── 🗄️ Local SQL Server Express
├── 🔍 Debug logging enabled
├── 📧 Email simulation (no real sends)
├── 🚀 Hot reload for instant code changes
├── 🐛 Detailed error pages
└── 🔧 Development tools enabled
```

**Development-Specific Files:**
```
├── 📄 appsettings.Development.json     ← 💻 Local database connection
├── 📁 obj/Debug/                       ← 🐛 Debug build artifacts
├── 📄 *.pdb files                      ← 🔍 Debug symbols included
└── 🔧 Development certificates         ← 🛡️ HTTPS for localhost
```

### **☁️ Production Environment**

```
Production Setup:
├── 🌐 Azure SQL Database
├── ⚠️ Warning-level logging only  
├── 📧 Real email sending via SMTP
├── 🚀 Optimized compiled code
├── 🛡️ Generic error pages (security)
└── 📊 Performance monitoring
```

**Production-Specific Files:**
```
├── 📄 appsettings.json only            ← 🌐 Production database connection
├── 📁 bin/Release/                     ← ⚡ Optimized build artifacts
├── 🚫 No .pdb files                    ← 🔒 No debug symbols (security)
└── 🌐 SSL certificates                 ← 🛡️ HTTPS for public access
```

---

## 🚫 **Critical: What NEVER to Commit to Git**

### **🔐 Security-Sensitive Files**
```
❌ NEVER Commit:
├── 📄 appsettings.Development.json     ← 🔑 Contains local database passwords
├── 📄 *.user files                     ← 👤 Personal Visual Studio settings
├── 📁 AppLogs/                         ← 📊 May contain sensitive user data
├── 📁 wwwroot/uploads/                 ← 👥 User's personal images
└── 📄 Any file with passwords/keys     ← 🔐 Security risk
```

### **🏗️ Build Artifacts (Auto-Generated)**
```
❌ NEVER Commit:
├── 📁 bin/                             ← ⚡ Compiled output (regenerated)
├── 📁 obj/                             ← 🔧 Build cache (regenerated)  
├── 📁 publish/                         ← 📦 Deployment packages
├── 📁 *-backup-*/                      ← 💾 Backup folders
└── 📄 *.zip files                      ← 🗜️ Compressed archives
```

### **✅ Proper .gitignore Configuration**
```gitignore
# Build outputs (regenerated every build)
bin/
obj/
publish/

# User data (privacy and size)
wwwroot/uploads/
AppLogs/
*.log

# Deployment artifacts (temporary)
wwwroot-backup-*/
temp-deploy-*/
temp-download-*/
*.zip

# Personal settings (developer-specific)
*.user
appsettings.Development.json

# IDE files (editor-specific)
.vs/
.vscode/settings.json
```

**🎓 Why These Rules Matter:**
- **Security:** Prevents accidental exposure of passwords and user data
- **Performance:** Keeps repository small and fast
- **Collaboration:** Prevents conflicts from personal settings
- **Professionalism:** Follows industry best practices

---

## 🎯 **Professional .NET Development Mastery Checklist**

### **🏗️ Build System Understanding**
After reading this guide, you should understand:

- [ ] **Why obj/ and bin/ folders exist** and their different purposes
- [ ] **Debug vs Release builds** and when to use each
- [ ] **How .NET compilation works** from source code to executable
- [ ] **NuGet package management** and dependency resolution
- [ ] **MSBuild project file structure** and configuration options

### **🌐 Web Application Architecture**
- [ ] **Why wwwroot/ is special** and what files belong there
- [ ] **Static file serving** vs server-processed content
- [ ] **Configuration system** (appsettings.json hierarchy)
- [ ] **Environment-specific settings** (Development vs Production)
- [ ] **Application entry point** (Program.cs) and service registration

### **🚀 Deployment & Operations**
- [ ] **Backup strategies** for user data and application state
- [ ] **Deployment automation** principles and safety measures
- [ ] **Logging strategy** for production applications
- [ ] **Security considerations** for public file access
- [ ] **Version control best practices** for team collaboration

### **🔧 Development Workflow**
- [ ] **Local development setup** vs production environment
- [ ] **API testing** using .http files and development tools
- [ ] **Project structure navigation** and file organization
- [ ] **Debugging capabilities** provided by .pdb files
- [ ] **Package management** and dependency updates

---

## 🧠 **Key Learning Concepts Reinforcement**

### **🏗️ The .NET Build Pipeline**
```
Source Code (.cs files) 
    ↓ [Compilation]
Intermediate Language (obj/ folder)
    ↓ [Linking] 
Executable Application (bin/ folder)
    ↓ [Publishing]
Deployment Package (publish/ folder)
    ↓ [Deployment]
Running Application (Azure/IIS)
```

### **📁 File Organization Philosophy**
```
Project Organization Principles:
├── 🎯 Source Code (Controllers, Models, Services)
│   ├── Separation of Concerns (each folder has one responsibility)
│   ├── Layered Architecture (UI → Business → Data)
│   └── Dependency Direction (UI depends on Business, not vice versa)
├── ⚙️ Configuration (appsettings, project files)  
│   ├── Environment Separation (dev vs prod settings)
│   ├── Security Boundaries (public vs private configuration)
│   └── Override Hierarchy (specific overrides general)
├── 🏗️ Build Artifacts (obj, bin folders)
│   ├── Compilation Output (generated from source)
│   ├── Dependency Resolution (NuGet package assemblies)
│   └── Deployment Packaging (ready-to-run applications)
└── 🌐 Runtime Assets (wwwroot, logs)
    ├── Public Assets (accessible via HTTP)
    ├── User Data (uploads, generated content)
    └── Operational Data (logs, monitoring)
```

### **🔐 Security Model Understanding**
```
Security Boundaries in .NET Applications:
├── 🌍 Public (wwwroot/ - accessible to anyone)
├── 🔒 Application (Controllers, Services - requires authentication)  
├── 🛡️ Configuration (appsettings - server access only)
├── 🔑 Secrets (passwords, keys - environment variables/Azure Key Vault)
└── 💻 Development (local files, debug symbols - never deployed)
```

---

## 🚀 **Next Steps for Advanced Learning**

### **📚 Recommended Learning Path**
1. **Master the Basics** (Week 1)
   - Practice creating new .NET projects
   - Experiment with different project templates
   - Understand project file modifications

2. **Explore Build System** (Week 2)  
   - Try different Target Frameworks
   - Experiment with NuGet packages
   - Learn about MSBuild properties

3. **Deployment Deep Dive** (Week 3)
   - Practice manual deployments
   - Understand publish profiles
   - Learn about environment variables

4. **Production Operations** (Week 4)
   - Set up proper logging
   - Implement health checks
   - Practice backup and restore

### **🔧 Hands-On Exercises**

#### **Exercise 1: Build System Exploration**
```powershell
# 1. Clean everything and observe what gets recreated
dotnet clean
Remove-Item -Recurse -Force bin, obj

# 2. Build and examine artifacts
dotnet build
# Explore bin/Debug/net8.0/ and obj/Debug/net8.0/

# 3. Try release build
dotnet build --configuration Release  
# Compare bin/Debug vs bin/Release
```

#### **Exercise 2: Configuration Experiments**
```json
// 1. Create appsettings.Staging.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}

// 2. Test environment-specific loading
// Set ASPNETCORE_ENVIRONMENT=Staging and observe configuration
```

#### **Exercise 3: Deployment Simulation**
```powershell
# 1. Practice publish command
dotnet publish --configuration Release --output ./publish

# 2. Examine publish folder structure
# 3. Compare with bin/Release folder
# 4. Understand the differences
```

---

## 🏆 **Professional Development Milestones**

### **🎓 Beginner Level (Achieved)**
- ✅ Understand project structure and file purposes  
- ✅ Know what to commit vs ignore in version control
- ✅ Grasp basic build and deployment concepts
- ✅ Navigate .NET project files confidently

### **💼 Intermediate Targets (Next Steps)**
- 🎯 Create custom MSBuild tasks and properties
- 🎯 Implement advanced deployment strategies  
- 🎯 Configure production monitoring and alerting
- 🎯 Design multi-environment deployment pipelines

### **🚀 Advanced Goals (Future Mastery)**
- 🏆 Design microservices architecture patterns
- 🏆 Implement container-based deployment (Docker)
- 🏆 Create custom .NET project templates
- 🏆 Contribute to open-source .NET projects

---

**🎉 Congratulations! You now have professional-level understanding of .NET project structure and the knowledge to navigate, modify, and deploy .NET applications with confidence.**

*This knowledge forms the foundation for advanced .NET development, DevOps practices, and cloud-native application development.*

---

---

## 🗑️ **File & Folder Deletion Safety Guide**
### 🎯 **What's Safe to Delete vs What to Preserve**

### **✅ SAFE TO DELETE (Can be regenerated or are temporary)**

#### **🏗️ Build Artifacts - Always Safe**
```
✅ DELETE ANYTIME:
├── 📁 bin/                    ← Regenerated on every build
├── 📁 obj/                    ← Regenerated on every build  
├── 📁 publish/                ← Regenerated on dotnet publish
├── 📄 *.pdb files             ← Debug symbols (regenerated)
└── 📄 Any .dll/.exe in root   ← Compilation outputs
```

**🔄 Regeneration Method:**
```powershell
dotnet clean    # Removes bin/ and obj/
dotnet build    # Recreates everything
```

**📊 Disk Space Impact:** These folders can be 50-200MB, safe cleanup targets.

#### **💾 Deployment Backups - Conditional Deletion**
```
🟡 CONDITIONAL DELETE (after verification):
├── 📁 wwwroot-backup-YYYYMMDD-HHMMSS/  ← User data backups
├── 📁 temp-download-YYYYMMDD-HHMMSS/   ← Deployment staging
├── 📄 *.zip backup files               ← Compressed backups
└── 📄 deployment-*.zip                 ← Deployment packages
```

**🛡️ Deletion Strategy for Backups:**
```powershell
# SAFE: Keep last 3 deployments, delete older ones
# Current situation: You have 6 backup folders

# 1. Verify current deployment works
curl https://yourapp.azurewebsites.net/health

# 2. Keep recent backups (last 2-3 deployments)
Keep: wwwroot-backup-20251011-235015  # Latest
Keep: wwwroot-backup-20251010-161008  # Previous
Keep: wwwroot-backup-20251008-115255  # Safety buffer

# 3. DELETE older backups
DELETE: wwwroot-backup-20251007-135019
DELETE: wwwroot-backup-20251007-130619  
DELETE: wwwroot-backup-20251005-222540
```

**💡 Your Current Situation Analysis:**
- **6 wwwroot-backup folders:** Keep latest 3, delete oldest 3
- **5 temp-download folders:** All appear empty - SAFE TO DELETE ALL
- **Estimated space savings:** 100-500MB depending on user uploads

#### **📊 Application Logs - Rotation Needed**
```
🟡 MANAGE WITH ROTATION:
├── 📁 AppLogs/
│   ├── 📄 normal.txt      ← Rotate when > 10MB
│   └── 📄 critical.txt    ← Keep always (archive old)
└── 📄 *.log files         ← Rotate based on age/size
```

**📊 Log Management Strategy:**
```powershell
# Check log sizes
Get-ChildItem -Path "AppLogs" -Recurse | Where-Object Length -gt 10MB

# Archive large logs
$date = Get-Date -Format "yyyyMMdd"
Rename-Item "AppLogs/normal.txt" "AppLogs/normal_$date.txt"
New-Item "AppLogs/normal.txt" -ItemType File
```

### **❌ NEVER DELETE (Critical for application)**

#### **🔐 Source Code & Configuration**
```
❌ NEVER DELETE:
├── 📁 Controllers/         ← Your API endpoints
├── 📁 Models/             ← Database structure  
├── 📁 Services/           ← Business logic
├── 📁 Data/               ← Database context
├── 📁 Helpers/            ← Utility functions
├── 📁 Utils/              ← Static utilities
├── 📁 Migrations/         ← Database version history
├── 📄 Program.cs          ← Application entry point
├── 📄 ResellBook.csproj   ← Project definition
└── 📄 appsettings.json    ← Production configuration
```

**💀 Consequences of Deletion:** Application won't compile or run.
**🔄 Recovery:** Only from git repository or backups.

#### **👥 User Data & Runtime Assets**
```
❌ NEVER DELETE:
├── 📁 wwwroot/            ← Current user uploads
│   └── 📁 uploads/        ← User's book images
├── 📄 appsettings.Development.json  ← Local dev settings
└── 📁 Properties/         ← Deployment configurations
```

**💀 Consequences of Deletion:** 
- **wwwroot/uploads:** Permanent user data loss
- **Properties:** Deployment configurations lost
- **appsettings.Development.json:** Local development broken

**🔄 Recovery:** 
- **User uploads:** From backups only (if available)
- **Configuration:** Must recreate manually or restore from git

### **🟡 DEVELOPER-SPECIFIC (Safe to delete, personal impact)**

#### **👤 Personal IDE Files**
```
🟡 PERSONAL IMPACT:
├── 📄 ResellBook.csproj.user    ← Your VS settings
├── 📁 .vs/                      ← Visual Studio cache
├── 📁 .vscode/                  ← VS Code settings
└── 📄 *.suo, *.user files       ← IDE personal files
```

**🔄 Regeneration:** IDE recreates with default settings.
**📊 Impact:** Lose personal preferences, no functional impact.

---

## 🧹 **Recommended Cleanup Script**

### **📄 `cleanup.ps1` - Safe Project Cleanup**
```powershell
<#
.SYNOPSIS
    Safe cleanup of ResellBook project files
.DESCRIPTION
    Removes build artifacts, old backups, and temporary files while preserving critical data
#>

param(
    [int]$KeepBackups = 3,              # Number of backup folders to keep
    [int]$MaxLogSizeMB = 10,            # Max log file size before rotation
    [switch]$DryRun = $false            # Show what would be deleted without deleting
)

Write-Host "🧹 ResellBook Project Cleanup" -ForegroundColor Green
Write-Host "=============================" -ForegroundColor Green

# 1. Clean build artifacts (always safe)
Write-Host "🏗️ Cleaning build artifacts..." -ForegroundColor Yellow
if (-not $DryRun) {
    dotnet clean
    Remove-Item -Path "publish" -Recurse -Force -ErrorAction SilentlyContinue
}
Write-Host "✅ Build artifacts cleaned" -ForegroundColor Green

# 2. Clean old deployment backups
Write-Host "💾 Managing deployment backups..." -ForegroundColor Yellow
$backupFolders = Get-ChildItem -Directory -Name "wwwroot-backup-*" | Sort-Object -Descending

if ($backupFolders.Count -gt $KeepBackups) {
    $toDelete = $backupFolders | Select-Object -Skip $KeepBackups
    foreach ($folder in $toDelete) {
        Write-Host "  🗑️ Would delete: $folder" -ForegroundColor Red
        if (-not $DryRun) {
            Remove-Item -Path $folder -Recurse -Force
            Write-Host "  ✅ Deleted: $folder" -ForegroundColor Green
        }
    }
}

# 3. Clean temporary download folders (usually safe)  
Write-Host "📥 Cleaning temporary download folders..." -ForegroundColor Yellow
$tempFolders = Get-ChildItem -Directory -Name "temp-download-*"
foreach ($folder in $tempFolders) {
    Write-Host "  🗑️ Would delete: $folder" -ForegroundColor Red
    if (-not $DryRun) {
        Remove-Item -Path $folder -Recurse -Force
        Write-Host "  ✅ Deleted: $folder" -ForegroundColor Green
    }
}

# 4. Rotate large log files
Write-Host "📊 Managing log files..." -ForegroundColor Yellow
if (Test-Path "AppLogs") {
    $logFiles = Get-ChildItem -Path "AppLogs" -Filter "*.txt"
    foreach ($log in $logFiles) {
        $sizeMB = [math]::Round($log.Length / 1MB, 2)
        if ($sizeMB -gt $MaxLogSizeMB) {
            $newName = $log.BaseName + "_" + (Get-Date -Format "yyyyMMdd") + $log.Extension
            Write-Host "  📋 Would rotate: $($log.Name) ($sizeMB MB) → $newName" -ForegroundColor Yellow
            if (-not $DryRun) {
                Rename-Item $log.FullName -NewName $newName
                New-Item -Path $log.FullName -ItemType File
                Write-Host "  ✅ Rotated: $($log.Name)" -ForegroundColor Green
            }
        }
    }
}

# 5. Summary
Write-Host "`n📊 Cleanup Summary:" -ForegroundColor Cyan
$totalBackups = (Get-ChildItem -Directory -Name "wwwroot-backup-*").Count
$totalTempFolders = (Get-ChildItem -Directory -Name "temp-download-*").Count

Write-Host "📁 Backup folders: $totalBackups (keeping $KeepBackups)" -ForegroundColor White
Write-Host "📥 Temp folders: $totalTempFolders" -ForegroundColor White

if ($DryRun) {
    Write-Host "`n🔍 This was a dry run. Use -DryRun:`$false to actually delete files." -ForegroundColor Yellow
}
```

### **💡 Your Immediate Recommendations**

Based on your current project state:

**✅ SAFE TO DELETE RIGHT NOW:**
```powershell
# All temp-download folders (appear empty)
Remove-Item -Recurse -Force temp-download-20251007-130619
Remove-Item -Recurse -Force temp-download-20251007-135019  
Remove-Item -Recurse -Force temp-download-20251008-115255
Remove-Item -Recurse -Force temp-download-20251010-161008
Remove-Item -Recurse -Force temp-download-20251011-235015

# Old backup folders (keep latest 3)
Remove-Item -Recurse -Force wwwroot-backup-20251005-222540
Remove-Item -Recurse -Force wwwroot-backup-20251007-130619
Remove-Item -Recurse -Force wwwroot-backup-20251007-135019
```

**💾 ESTIMATED SPACE SAVINGS:** 50-200MB depending on what was backed up.

**🛡️ SAFETY NET:**
- Before deleting backups, verify your app works: `https://yourapp.azurewebsites.net`
- Keep at least the last 2-3 backup folders as safety net
- Your current `wwwroot/uploads/` contains the live user data

---

**📅 Keep Learning:**
- **Microsoft Learn:** https://docs.microsoft.com/learn/dotnet/
- **.NET Documentation:** https://docs.microsoft.com/dotnet/
- **ASP.NET Core Guide:** https://docs.microsoft.com/aspnet/core/
- **Azure App Service:** https://docs.microsoft.com/azure/app-service/

---

## � **Individual File Analysis - Every File Explained**

### **🎯 Root Level Files (Project Foundation)**

#### **📄 `.gitignore` - Version Control Rules**
```gitignore
# What this file does: Tells Git which files to ignore
bin/                    # ← 🚫 Build output (changes every compilation)
obj/                    # ← 🚫 Build cache (regenerated automatically)  
wwwroot/uploads/        # ← 🚫 User data (privacy + large files)
*.user                  # ← 🚫 Personal Visual Studio settings
appsettings.Development.json  # ← 🚫 Local database passwords
```

**🎓 Learning Concept - Version Control Best Practices:**
- **Only commit source code** - not generated files
- **Protect sensitive data** - passwords, user files
- **Keep repo clean** - exclude temporary and personal files
- **Team collaboration** - prevent conflicts from personal settings

#### **📄 `WeatherForecast.cs` - .NET Template File**
```csharp
public class WeatherForecast    // 🌤️ Example model from .NET template
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }  
    public string? Summary { get; set; }
    // This is just a demo - can be deleted in real projects
}
```

**🎓 Why It Exists:**
- **Default .NET template** includes this as an example
- **Shows basic C# class structure** with properties
- **Safe to delete** - not used by ResellBook application
- **Learning reference** for new developers

#### **📄 `ResellBook.http` - API Testing File**
```http
### Test Authentication API
POST https://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "testpassword"  
}

### Test Books API  
GET https://localhost:5000/api/books
Authorization: Bearer {{authToken}}
```

**🎓 What This Does:**
- **Visual Studio Code integration** - can execute HTTP requests directly
- **API testing without Postman** - built into your editor
- **Documentation by example** - shows how to use your APIs
- **Development efficiency** - quick testing during development

#### **📄 `ResellBook.csproj.user` - Personal Settings**
```xml
<Project>
  <PropertyGroup>
    <!-- Personal Visual Studio settings -->
    <NameOfLastUsedPublishProfile>YourPersonalProfile</NameOfLastUsedPublishProfile>
    <LastUsedBuildConfiguration>Debug</LastUsedBuildConfiguration>
  </PropertyGroup>
</Project>
```

**🎓 Why This File Exists:**
- **Personal IDE preferences** - remembers your last settings
- **Not shared with team** - everyone has different preferences  
- **Auto-generated** - Visual Studio creates and manages this
- **Safe to ignore/delete** - will be recreated as needed

#### **📄 `deploy.ps1` - Deployment Automation**
```powershell
# Custom deployment script for ResellBook
# Handles backup, deployment, and verification
param(
    [string]$Environment = "Production"
)

# This script automates:
# 1. Backing up current user files
# 2. Downloading latest code
# 3. Building and deploying
# 4. Verifying deployment success
```

**🎓 Professional Deployment Concepts:**
- **Infrastructure as Code** - deployment steps defined in script
- **Backup Strategy** - always backup before deploying
- **Rollback Capability** - can restore if deployment fails
- **Verification Steps** - ensure deployment actually worked

#### **📄 `maintenance.ps1` - Operational Scripts**
```powershell
# Operational maintenance tasks
# - Log rotation
# - Backup cleanup  
# - Health checks
# - Performance monitoring
```

**🎓 Production Operations Learning:**
- **Preventive Maintenance** - avoid problems before they occur
- **Log Management** - prevent disk space issues
- **Health Monitoring** - detect issues early
- **Automated Operations** - reduce manual intervention

---

## 📁 **.config/ Folder - Development Tools Configuration**

```
.config/
└── 📄 dotnet-tools.json       ← 🔧 Development tool definitions
```

**What's in dotnet-tools.json:**
```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "dotnet-ef": {                    // 🗄️ Entity Framework CLI tools
      "version": "8.0.11",
      "commands": ["dotnet-ef"]
    }
  }
}
```

**🎓 Learning Concept - Development Tools:**
- **Local tool installation** - tools specific to this project
- **Version consistency** - everyone uses same tool versions
- **Command availability** - `dotnet ef` commands work in this directory
- **Team collaboration** - ensures consistent development environment

---

## 🏗️ **Properties/ Folder - Project Configuration & Deployment**

```
Properties/
├── 📄 launchSettings.json              ← 🚀 Development server settings
├── 📄 serviceDependencies.json        ← 🔧 Azure service connections  
├── 📁 PublishProfiles/                 ← 📦 Deployment configurations
│   ├── 📄 *.pubxml                     ← 🌐 Azure publish settings
│   └── 📄 *.pubxml.user                ← 👤 Personal publish settings
└── 📁 ServiceDependencies/             ← ☁️ Azure service configurations
```

#### **📄 `launchSettings.json` - Development Server Configuration**
```json
{
  "profiles": {
    "https": {                          // 🔒 HTTPS development profile
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "IIS Express": {                    // 🌐 IIS Express profile  
      "commandName": "IISExpress",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

**🎓 What Each Setting Does:**
- **applicationUrl:** Where your app runs during development (localhost:7001)
- **launchBrowser:** Automatically opens browser when you start debugging
- **ASPNETCORE_ENVIRONMENT:** Tells .NET this is development (enables debug features)
- **Multiple profiles:** Different ways to run your app (direct vs IIS)

#### **📄 `serviceDependencies.json` - Azure Integration**
```json
{
  "dependencies": {
    "apis1": {                          // 🌐 API Management connection
      "type": "apis",
      "connectionId": "AzureApiManagement"
    },
    "mssql1": {                         // 🗄️ SQL Database connection
      "type": "mssql",  
      "connectionId": "DefaultConnection"
    }
  }
}
```

**🎓 Azure Services Integration:**
- **Service Discovery:** Visual Studio knows about your Azure services
- **Connection Management:** Manages connection strings and secrets
- **Deployment Integration:** Automatically configures services during publish
- **Development Experience:** IntelliSense and debugging for cloud services

#### **📁 PublishProfiles/ - Deployment Configurations**

**📄 `*.pubxml` Files - Deployment Settings:**
```xml
<Project>
  <PropertyGroup>
    <WebPublishMethod>MSDeploy</WebPublishMethod>      <!-- 🚀 Deployment method -->
    <PublishUrl>resellbook.scm.azurewebsites.net</PublishUrl>  <!-- 🌐 Target server -->
    <UserName>$resellbook</UserName>                    <!-- 👤 Deployment credentials -->
    <ExcludeFilesFromDeployment>                        <!-- �🚫 Files not to deploy -->
      appsettings.Development.json
    </ExcludeFilesFromDeployment>
  </PropertyGroup>
</Project>
```

**🎓 Professional Deployment Concepts:**
- **MSDeploy:** Microsoft's web deployment technology
- **Target Configuration:** Where and how to deploy
- **Selective Deployment:** Don't deploy development-only files
- **Credential Management:** Secure authentication for deployment

---

## 📦 **Understanding NuGet Package Management Files**

### **📄 `obj/project.assets.json` - Complete Dependency Graph**
```json
{
  "version": 3,
  "targets": {
    "net8.0": {
      "BCrypt.Net-Next/4.0.3": {           // 🔐 Password hashing library
        "type": "package",
        "dependencies": {
          "System.Memory": "4.5.3"          // 📦 Dependent on memory management
        },
        "compile": {
          "lib/net6.0/BCrypt.Net-Next.dll": {}
        },
        "runtime": {
          "lib/net6.0/BCrypt.Net-Next.dll": {}
        }
      }
    }
  }
}
```

**🎓 Dependency Management Learning:**
- **Transitive Dependencies:** Packages your packages need
- **Version Resolution:** NuGet figures out compatible versions
- **Compile vs Runtime:** Different files needed for building vs running
- **Target Framework:** Dependencies vary by .NET version

### **📄 `obj/*.nuget.dgspec.json` - Dependency Specification**
```json
{
  "format": 1,
  "restore": {
    "C:\\Repos\\ResellPanda\\ResellBook\\ResellBook.csproj": {}
  },
  "projects": {
    "C:\\Repos\\ResellPanda\\ResellBook\\ResellBook.csproj": {
      "version": "1.0.0",
      "frameworks": {
        "net8.0": {
          "targetAlias": "net8.0",
          "dependencies": {
            "BCrypt.Net-Next": {
              "target": "Package",
              "version": "[4.0.3, )"          // 🎯 Version constraints
            }
          }
        }
      }
    }
  }
}
```

**🎓 Advanced Package Concepts:**
- **Version Constraints:** `[4.0.3, )` means "4.0.3 or higher"
- **Framework Targeting:** Different packages for different .NET versions
- **Restore Process:** How NuGet downloads and installs packages
- **Lock File Concept:** Ensures consistent package versions across environments

---

## 🚫 **Critical: What NEVER to Commit to Git**

### **.gitignore Should Include:**
```gitignore
# Build outputs
bin/
obj/
publish/

# User uploads (sensitive data)
wwwroot/uploads/

# Application logs (contain sensitive information)
AppLogs/
*.log

# Deployment backups (too large for git)
wwwroot-backup-*/
temp-deploy-*/
temp-download-*/
*.zip

# Temporary deployment files
deploy-*.zip
bulletproof-deploy.zip
app-logs.zip

# Local settings
appsettings.Development.json  # Contains local database connections
```

---

## 📋 **Folder Purposes & Maintenance**

### **🔧 Development Folders**
| Folder | Purpose | Maintenance |
|--------|---------|-------------|
| `Controllers/` | API endpoints and request handling | ✅ Commit to git |
| `Models/` | Database entities and DTOs | ✅ Commit to git |
| `Services/` | Business logic and external integrations | ✅ Commit to git |
| `Utils/` | Static utility classes (SimpleLogger) | ✅ Commit to git |
| `Data/` | DbContext and database configuration | ✅ Commit to git |
| `Migrations/` | EF Core database migrations | ✅ Commit to git |

### **🏗️ Build Folders**
| Folder | Purpose | Maintenance |
|--------|---------|-------------|
| `bin/` | Compiled application binaries | ❌ Auto-generated, ignore in git |
| `obj/` | Build intermediate files | ❌ Auto-generated, ignore in git |
| `publish/` | Deployment package output | ❌ Created by deploy script |

### **📁 Runtime Folders**
| Folder | Purpose | Maintenance |
|--------|---------|-------------|
| `wwwroot/` | Static web files and uploads | 🔒 **CRITICAL** - Never delete |
| `wwwroot/uploads/` | User uploaded book images | 🔒 **PROTECTED** by deploy script |
| `AppLogs/` | Application logging files | 📊 Monitor size, rotate if needed |

### **💾 Backup Folders**
| Folder | Purpose | Maintenance |
|--------|---------|-------------|
| `wwwroot-backup-*` | Deployment safety backups | 🗂️ Keep latest 2-3, delete old ones |

---

## 🚀 **Deployment Folder Behavior**

### **What Happens During `.\deploy.ps1`:**

#### **1. Backup Phase**
```
✅ Creates: wwwroot-backup-YYYYMMDD-HHMMSS/
✅ Downloads: Current production wwwroot as backup.zip
✅ Preserves: All user uploads and static files
```

#### **2. Build Phase**
```
✅ Creates: publish/ (temporary build output)
✅ Excludes: wwwroot/ from deployment package
✅ Packages: Application code only (no user files)
```

#### **3. Deploy Phase**
```
✅ Uploads: Application code to Azure
✅ Preserves: Existing wwwroot/ on server
✅ Maintains: All user uploads and images
```

#### **4. Cleanup Phase**
```
✅ Removes: Temporary deployment files
✅ Keeps: Latest backup for safety
✅ Cleans: Old backup folders
```

---

## 📊 **Folder Size Monitoring**

### **Expected Sizes:**
| Folder | Typical Size | Alert If Exceeds |
|--------|-------------|-----------------|
| `AppLogs/` | < 50 MB | 100 MB |
| `wwwroot/uploads/` | Varies by usage | 1 GB |
| `wwwroot-backup-*` | Same as uploads | Monitor disk space |
| `bin/` | ~50 MB | N/A (auto-managed) |

### **Maintenance Schedule:**
- **Daily**: Monitor `AppLogs/` growth
- **Weekly**: Check `wwwroot/uploads/` size
- **Monthly**: Clean old backup folders
- **After Deployment**: Verify backup creation

---

## 🔍 **Folder Health Checks**

### **Critical Folders Must Exist:**
```powershell
# Run this check after deployment
$criticalFolders = @("Controllers", "Models", "Utils", "wwwroot", "AppLogs")
foreach ($folder in $criticalFolders) {
    if (Test-Path $folder) {
        Write-Host "✅ $folder - OK" -ForegroundColor Green
    } else {
        Write-Host "❌ $folder - MISSING!" -ForegroundColor Red
    }
}
```

### **Log Files Health Check:**
```powershell
# Check logging system
if (Test-Path "AppLogs/normal.txt") {
    $normalSize = (Get-Item "AppLogs/normal.txt").Length
    Write-Host "📊 Normal logs: $([math]::Round($normalSize/1KB, 2)) KB"
}
if (Test-Path "AppLogs/critical.txt") {
    $criticalSize = (Get-Item "AppLogs/critical.txt").Length
    Write-Host "📊 Critical logs: $([math]::Round($criticalSize/1KB, 2)) KB"
}
```

---

## 🚨 **Emergency Recovery**

### **If wwwroot/ Gets Deleted:**
1. **Stop deployment immediately**
2. **Find latest backup**: `wwwroot-backup-YYYYMMDD-HHMMSS/`
3. **Extract backup.zip** to restore files
4. **Re-upload via Kudu**: https://resellbook20250929183655.scm.azurewebsites.net

### **If AppLogs/ Gets Deleted:**
1. **Restart application** (will auto-create folder)
2. **Test logging**: Call `/api/Logs/TestLogging`
3. **Verify creation**: Check `AppLogs/` folder exists

### **If Deploy Script Missing:**
1. **DO NOT DEPLOY** with any other method
2. **Contact team immediately**
3. **Restore from git** if necessary

---

## 📚 **Documentation Structure**

### **Developer Documentation/ Folder:**
```
Developer Documentation/
├── 📄 LOGS_CONTROLLER_API.md          # API documentation for logging
├── 📄 DEPLOYMENT_GUIDE_LOGGING.md     # Complete deployment guide
├── 📄 DEPLOYMENT_SAFETY_PROTOCOL.md   # Safety procedures
├── 📄 DEPLOYMENT_CHECKLIST.md         # Quick deployment steps
├── 📄 PROJECT_STRUCTURE.md            # This file
└── 📄 API_DOCUMENTATION_COMPLETE.md   # Complete API docs
```

---

## ✅ **Best Practices**

### **Do's:**
- ✅ Always use `.\deploy.ps1` for deployment
- ✅ Monitor `AppLogs/` folder size regularly
- ✅ Keep 2-3 recent backup folders
- ✅ Commit code changes to git regularly
- ✅ Test locally before deployment

### **Don'ts:**
- ❌ Never delete `wwwroot/` manually
- ❌ Never commit user uploads to git
- ❌ Never use manual deployment methods
- ❌ Never delete backup folders immediately
- ❌ Never ignore large log files

---

## 🎯 **Quick Commands**

### **Cleanup Temp Files:**
```powershell
Remove-Item "temp-*" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "*.zip" -Force -ErrorAction SilentlyContinue
```

### **Check Folder Health:**
```powershell
Get-ChildItem -Directory | ForEach-Object { 
    $size = (Get-ChildItem $_.FullName -Recurse | Measure-Object -Property Length -Sum).Sum
    "$($_.Name): $([math]::Round($size/1MB, 2)) MB"
}
```

### **Monitor Logs:**
```powershell
Get-Content "AppLogs/critical.txt" -Tail 5
```

---

**Last Updated**: October 5, 2025  
**Status**: Production Structure ✅