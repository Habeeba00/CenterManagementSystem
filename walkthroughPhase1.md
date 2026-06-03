# Phase 1 — Authentication & Scaffolding — Walkthrough

## Summary
Implemented the complete Phase 1 per `phase-1-auth-scaffolding.md`. The solution builds cleanly with **0 errors, 0 warnings**.

---

## Files Created (20 new files)

### Application Layer (8 files)
| File | Purpose |
|------|---------|
| [IAuditLogService.cs](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Application/Interfaces/IAuditLogService.cs) | Audit logging interface |
| [IFileUploadService.cs](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Application/Interfaces/IFileUploadService.cs) | File upload interface (IFormFile) |
| [INotificationService.cs](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Application/Interfaces/INotificationService.cs) | Notification interface (6 methods) |
| [NotificationDto.cs](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Application/DTOs/Notification/NotificationDto.cs) | Notification data transfer object |
| [AuditLogService.cs](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Application/Services/AuditLogService.cs) | Audit log — never throws, try/catch |
| [FileUploadService.cs](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Application/Services/FileUploadService.cs) | Validates ext/size, saves to wwwroot/uploads |
| [NotificationService.cs](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Application/Services/NotificationService.cs) | Stub — only SendToUser creates record |
| [ApplicationServiceRegistration.cs](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Application/DependencyInjection/ApplicationServiceRegistration.cs) | DI registration extension method |

### Infrastructure Layer (1 file)
| File | Purpose |
|------|---------|
| [InfrastructureServiceRegistration.cs](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Infrastructure/DependencyInjection/InfrastructureServiceRegistration.cs) | Moves DbContext registration out of Program.cs |

### Web Layer (11 files)
| File | Purpose |
|------|---------|
| [LoginViewModel.cs](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Web/ViewModels/Auth/LoginViewModel.cs) | Email + Password + RememberMe with validation |
| [AuthController.cs](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Web/Controllers/AuthController.cs) | Login/Logout/AccessDenied with role-based redirect |
| [DashboardController.cs](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Web/Controllers/DashboardController.cs) | [Authorize] stub — Phase 6 replaces |
| [Login.cshtml](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Web/Views/Auth/Login.cshtml) | Login page with design tokens, _LayoutEmpty |
| [AccessDenied.cshtml](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Web/Views/Auth/AccessDenied.cshtml) | 403 page with back link |
| [_Layout.cshtml](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Web/Views/Shared/_Layout.cshtml) | Full sidebar layout from receptioneestDash.html |
| [_LayoutEmpty.cshtml](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Web/Views/Shared/_LayoutEmpty.cshtml) | Minimal layout for login (no sidebar) |
| [Dashboard/Index.cshtml](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Web/Views/Dashboard/Index.cshtml) | Placeholder stub |
| [site.css](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Web/wwwroot/css/site.css) | Material Symbols + card-shadow + scrollbar |
| [site.js](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Web/wwwroot/js/site.js) | Anti-forgery helper + sidebar + notif polling |
| [.gitkeep](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Web/wwwroot/uploads/.gitkeep) | Preserves uploads directory |

---

## Files Modified (5 files)

| File | Change |
|------|--------|
| [Application.csproj](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Application/CenterManagement.Application.csproj) | Added `FrameworkReference` for ASP.NET Core types + Infrastructure project reference |
| [Infrastructure.csproj](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Infrastructure/CenterManagement.Infrastructure.csproj) | Removed Application reference (broke circular dependency) |
| [HomeController.cs](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Web/Controllers/HomeController.cs) | Now redirects: authenticated→Dashboard, else→Login |
| [Program.cs](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Web/Program.cs) | Uses DI extension methods, cookie config, auth policies |
| [appsettings.json](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Web/appsettings.json) | Added FileUpload + AppSettings sections |
| [_ValidationScriptsPartial.cshtml](file:///d:/ITI/BACKEND/MVC/Project/CenterManagementSystem/CenterManagement.Web/Views/Shared/_ValidationScriptsPartial.cshtml) | Updated to use CDN jQuery + validation |

---

## Architecture Decision: Circular Dependency Fix

> [!IMPORTANT]
> The original project had Infrastructure referencing Application. Since Phase 1 places services that use `CenterManagementDbContext` in the Application layer (per spec), Application now references Infrastructure. To break the circular dependency, the Infrastructure→Application reference was removed. Infrastructure currently has no code that depends on Application types.

---

## Build Verification

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:02:02.20
```

## Remaining Checklist (Manual Testing)
- [ ] Visit `/Auth/Login` → page renders with correct design tokens
- [ ] Submit empty form → client-side validation fires
- [ ] Submit wrong password → server-side error message appears
- [ ] Login as `admin@center.com` / `Admin@123` → redirects to `/Dashboard`
- [ ] Visit `/Dashboard` without auth → redirects to `/Auth/Login`
- [ ] Logout → redirects to `/Auth/Login`, cookie cleared
- [ ] `_Layout.cshtml` renders sidebar with all nav items
