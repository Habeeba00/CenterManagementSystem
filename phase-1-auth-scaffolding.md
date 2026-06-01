# Phase 1 — Authentication & Application Scaffolding

**Developer:** Dev 1  
**HTML Source:** None — Login page is built from scratch using `DESIGN.md` tokens  
**Blocks:** Nothing — this phase must complete first  
**Unlocks:** All other phases depend on DI wiring and `_Layout.cshtml` from here

---

## Constraints (Read Before Writing Any Code)

- Do **NOT** touch `CenterManagement.Domain/` — entities are complete
- Do **NOT** touch `CenterManagement.Infrastructure/Persistence/` — DbContext and migrations are complete
- Do **NOT** duplicate the `DbContext` registration already in `Program.cs` — move it into `InfrastructureServiceRegistration.cs` instead
- Do **NOT** add repositories — all data access goes directly through `CenterManagementDbContext` injected into services
- Soft delete is handled globally by the EF Core query filter already in `DbContext` — never call `.IgnoreQueryFilters()` unless explicitly required
- Follow SOLID: one responsibility per class, depend on interfaces not implementations, register everything via DI
- All `DateTime` values stored and compared in **UTC** (`DateTime.UtcNow`)

---

## Files to CREATE

### `CenterManagement.Application/Interfaces/IAuditLogService.cs`

```csharp
Task LogAsync(
    string userId,
    string action,
    string entityName,
    int entityId,
    string? oldValues,
    string? newValues);
```

---

### `CenterManagement.Application/Interfaces/IFileUploadService.cs`

```csharp
Task<string> UploadPhotoAsync(IFormFile file, string subfolder);
void DeleteFile(string relativePath);
```

---

### `CenterManagement.Application/Interfaces/INotificationService.cs`

```csharp
Task SendToUserAsync(string userId, string title, string message);
Task SendToGroupAsync(int groupId, string title, string message);
Task<int> GetUnreadCountAsync(string userId);
Task<List<NotificationDto>> GetNotificationsAsync(string userId, int page, int pageSize);
Task MarkReadAsync(int notificationId, string userId);
Task MarkAllReadAsync(string userId);
```

> Phase 1 delivers a **stub** implementation only. Phase 6 replaces `NotificationService.cs` with the full implementation.

---

### `CenterManagement.Application/DTOs/Notification/NotificationDto.cs`

```csharp
public int Id { get; set; }
public string Title { get; set; } = string.Empty;
public string Message { get; set; } = string.Empty;
public bool IsRead { get; set; }
public DateTime SentAt { get; set; }
```

---

### `CenterManagement.Application/Services/AuditLogService.cs`

**Implements:** `IAuditLogService`  
**Inject:** `CenterManagementDbContext`

**Rules:**
- Creates a new `AuditLog` record on every call
- Sets `ActionDate = DateTime.UtcNow`, `CreatedAt = DateTime.UtcNow`
- Calls `SaveChangesAsync()`
- Never throws — wrap in try/catch and log to console on failure so audit errors never break the main flow

---

### `CenterManagement.Application/Services/FileUploadService.cs`

**Implements:** `IFileUploadService`  
**Inject:** `IWebHostEnvironment`

**Rules:**
- `UploadPhotoAsync`: validate extension is one of `.jpg`, `.jpeg`, `.png`, `.webp` — throw `InvalidOperationException` if not
- Validate `file.Length <= 5 * 1024 * 1024` — throw if exceeded
- Save to `{WebRootPath}/uploads/{subfolder}/{Guid.NewGuid()}{ext}`
- Return the relative path: `uploads/{subfolder}/{filename}`
- `DeleteFile`: check if file exists at `{WebRootPath}/{relativePath}` before deleting — never throw on missing file

---

### `CenterManagement.Application/Services/NotificationService.cs`

**Implements:** `INotificationService`  
**Inject:** `CenterManagementDbContext`  
**Phase 1 stub behavior:**
- `SendToUserAsync`: creates one `Notification` record and calls `SaveChangesAsync()`
- `SendToGroupAsync`: no-op — logs to `Console.WriteLine` only
- `GetUnreadCountAsync`: returns `0`
- `GetNotificationsAsync`: returns empty list
- `MarkReadAsync` / `MarkAllReadAsync`: no-op

---

### `CenterManagement.Application/DependencyInjection/ApplicationServiceRegistration.cs`

```csharp
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    services.AddScoped<IAuditLogService, AuditLogService>();
    services.AddScoped<IFileUploadService, FileUploadService>();
    services.AddScoped<INotificationService, NotificationService>();
    // Each subsequent phase appends their own registrations here
    return services;
}
```

---

### `CenterManagement.Infrastructure/DependencyInjection/InfrastructureServiceRegistration.cs`

```csharp
public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    IConfiguration configuration)
{
    services.AddDbContext<CenterManagementDbContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
    return services;
}
```

> After creating this file, **remove** the `AddDbContext` call from `Program.cs` and replace it with `builder.Services.AddInfrastructureServices(builder.Configuration)`.

---

### `CenterManagement.Web/ViewModels/Auth/LoginViewModel.cs`

```csharp
[Required]
[EmailAddress]
public string Email { get; set; } = string.Empty;

[Required]
[DataType(DataType.Password)]
public string Password { get; set; } = string.Empty;

public bool RememberMe { get; set; }
```

---

### `CenterManagement.Web/Controllers/AuthController.cs`

**No `[Authorize]` on the class.**  
**Inject:** `UserManager<ApplicationUser>`, `SignInManager<ApplicationUser>`, `IAuditLogService`

**Actions:**

| Method | Route | Behavior |
|--------|-------|----------|
| `GET` | `/Auth/Login` | Returns `View()` using `_LayoutEmpty` |
| `POST` | `/Auth/Login` | Validates `LoginViewModel`, calls `PasswordSignInAsync`, redirects Admin → `/Dashboard`, Instructor → `/Instructor/Dashboard` |
| `POST` | `/Auth/Logout` | Calls `SignOutAsync()`, redirects to `/Auth/Login` |
| `GET` | `/Auth/AccessDenied` | Returns `View()` |

**Post-login role check:**
```csharp
var roles = await _userManager.GetRolesAsync(user);
if (roles.Contains("Admin"))
    return RedirectToAction("Index", "Dashboard");
if (roles.Contains("Instructor"))
    return RedirectToAction("Index", "Instructor");
return RedirectToAction("Index", "Dashboard");
```

---

### `CenterManagement.Web/Controllers/HomeController.cs`

`GET /` → if authenticated redirect to `/Dashboard`, else redirect to `/Auth/Login`

---

### `CenterManagement.Web/Controllers/DashboardController.cs`

**Phase 1 stub only.**  
`[Authorize]`  
`GET Index()` → `return View()`  
Phase 6 replaces this entirely.

---

### `CenterManagement.Web/Views/Auth/Login.cshtml`

**Layout:** `"_LayoutEmpty"`

Design per `DESIGN.md`:
- Background `bg-background` (`#f8f9ff`)
- Card: `bg-white rounded-xl shadow-2xl w-full max-w-sm mx-auto`
- Logo text: **"EduManager Pro"** in `text-headline-md font-bold text-primary`
- `<form asp-action="Login" asp-controller="Auth" method="post">`
- Email field: `asp-for="Email"` + `asp-validation-for="Email"`
- Password field: `asp-for="Password"` + `asp-validation-for="Password"`
- Remember Me checkbox: `asp-for="RememberMe"`
- `<div asp-validation-summary="All" class="text-error text-body-sm"></div>`
- Submit button: `bg-primary-container text-white w-full py-3.5 rounded-lg font-label-md`
- Anti-forgery token: `@Html.AntiForgeryToken()`

---

### `CenterManagement.Web/Views/Auth/AccessDenied.cshtml`

Layout `"_Layout"`. Shows "Access Denied — 403" with a back link to `/Dashboard`.

---

### `CenterManagement.Web/Views/Shared/_Layout.cshtml`

Port the sidebar HTML from `receptioneestDash.html` exactly — preserve all Tailwind classes.

**Replace static `href="#"` with tag helpers:**

```html
<a asp-controller="Dashboard"   asp-action="Index">Dashboard</a>
<a asp-controller="Student"     asp-action="Index">Students</a>
<a asp-controller="Instructor"  asp-action="Index">Teachers</a>
<a asp-controller="Session"     asp-action="Index">Sessions</a>
<a asp-controller="Payment"     asp-action="Index">Payments</a>
<a asp-controller="Analytics"   asp-action="Index">Analytics</a>
```

**Active state logic** (add to each nav link):
```csharp
@{ var ctrl = ViewContext.RouteData.Values["controller"]?.ToString(); }
class="... @(ctrl == "Dashboard" ? "bg-white/10 border-l-4 border-primary" : "")"
```

**Topbar:** notification bell with `id="notif-bell"` and badge `id="notif-count" class="hidden"`. User name from `@User.Identity!.Name`. Logout is a `<form asp-action="Logout" asp-controller="Auth" method="post">` with `@Html.AntiForgeryToken()`.

**Sections:**
```html
@RenderBody()
@RenderSection("Scripts", required: false)
```

---

### `CenterManagement.Web/Views/Shared/_LayoutEmpty.cshtml`

Minimal layout — no sidebar. Includes Tailwind CDN, Google Fonts, `@RenderBody()`, and validation scripts partial.

---

### `CenterManagement.Web/Views/Shared/_ValidationScriptsPartial.cshtml`

```html
<script src="https://cdnjs.cloudflare.com/ajax/libs/jquery/3.7.1/jquery.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/jquery-validate/1.19.5/jquery.validate.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/jquery-validation-unobtrusive/4.0.0/jquery.validate.unobtrusive.min.js"></script>
```

---

### `CenterManagement.Web/Views/Dashboard/Index.cshtml`

**Phase 1 stub.** Layout `"_Layout"`. Paragraph: `"Dashboard — placeholder. Phase 6 replaces this view."` Phase 6 overwrites this file completely.

---

### `CenterManagement.Web/wwwroot/css/site.css`

Copy the `<style>` block from `receptioneestDash.html`:
- `.card-shadow` rule
- `::-webkit-scrollbar` rules
- `.material-symbols-outlined` `font-variation-settings`

---

### `CenterManagement.Web/wwwroot/js/site.js`

```js
// Anti-forgery token helper — used by all fetch() POST calls across phases
function getAntiForgeryToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
}

// Sidebar active state
document.addEventListener('DOMContentLoaded', () => {
    const path = window.location.pathname.toLowerCase();
    document.querySelectorAll('[data-nav-controller]').forEach(link => {
        const ctrl = link.dataset.navController.toLowerCase();
        if (path.startsWith('/' + ctrl)) {
            link.classList.add('bg-white/10', 'border-l-4', 'border-primary', 'text-white');
        }
    });
});

// Notification poll — stub wired in Phase 1, fully implemented in Phase 6
async function pollNotifications() {
    try {
        const res = await fetch('/Notification/GetUnread');
        const data = await res.json();
        const badge = document.getElementById('notif-count');
        if (!badge) return;
        if (data.count > 0) {
            badge.textContent = data.count;
            badge.classList.remove('hidden');
        } else {
            badge.classList.add('hidden');
        }
    } catch (_) { /* silent */ }
}

setInterval(pollNotifications, 60000);
```

---

### `CenterManagement.Web/wwwroot/uploads/.gitkeep`

Empty file. Add `wwwroot/uploads/` to `.gitignore` for actual uploaded files.

---

## Files to MODIFY

### `CenterManagement.Web/Program.cs`

1. **Remove** the `AddDbContext` call — it moves to `InfrastructureServiceRegistration.cs`
2. **Add** before `var app = builder.Build()`:

```csharp
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.ConfigureApplicationCookie(opts =>
{
    opts.LoginPath = "/Auth/Login";
    opts.AccessDeniedPath = "/Auth/AccessDenied";
    opts.ExpireTimeSpan = TimeSpan.FromHours(8);
    opts.SlidingExpiration = true;
});

builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    opts.AddPolicy("AdminOrInstructor", p => p.RequireRole("Admin", "Instructor"));
});
```

3. Confirm middleware order is: `UseStaticFiles → UseRouting → UseAuthentication → UseAuthorization`

---

### `CenterManagement.Web/appsettings.json`

Add the following keys if not present:

```json
{
  "FileUpload": {
    "MaxFileSizeMb": 5
  },
  "AppSettings": {
    "AttendanceLateGraceMinutes": 15,
    "NotificationPollIntervalSeconds": 60
  }
}
```

---

## Completion Checklist

- [ ] `dotnet build` → 0 errors
- [ ] Visit `/Auth/Login` → page renders with correct design tokens
- [ ] Submit empty form → client-side validation fires
- [ ] Submit wrong password → server-side error message appears below form
- [ ] Login as `admin@center.com` / `Admin@123` → redirects to `/Dashboard`
- [ ] Visit `/Dashboard` without auth → redirects to `/Auth/Login`
- [ ] Logout → redirects to `/Auth/Login`, cookie cleared
- [ ] `_Layout.cshtml` renders sidebar with all nav items
- [ ] `AuditLogService.LogAsync()` inserts record into `AuditLogs` table
- [ ] `FileUploadService.UploadPhotoAsync()` saves file to `wwwroot/uploads/`
