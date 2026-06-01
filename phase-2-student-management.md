# Phase 2 — Student Management

**Developer:** Dev 2  
**HTML Sources:** `studentmangment.html` · `studentProfile.html` · `add-newStudent.html`  
**Hard dependency:** Phase 1 must be complete (`_Layout.cshtml`, `IAuditLogService`, `IFileUploadService`, `ApplicationServiceRegistration.cs`)  
**Soft dependency:** `GradeLevels` and `Subjects` seed data must exist in DB (added to `IdentitySeeder.cs` in this phase)

---

## Constraints (Read Before Writing Any Code)

- **No direct DB queries in controllers** — all data access goes through `IStudentService`
- **Single responsibility:** `StudentService` handles business rules; `StudentController` handles HTTP only
- **Transactions:** `CreateStudentAsync` MUST wrap in `IDbContextTransaction` — an orphaned `ApplicationUser` with no `StudentProfile` is a data corruption bug
- **Soft delete:** Never call `DbContext.Remove()` on `StudentProfile` — set `IsDeleted = true` only. The global query filter in `DbContext` excludes soft-deleted records automatically
- **Transfer rule:** `TransferStudentAsync` sets the old `Enrollment.IsActive = false` — it does **not** delete it. Old attendance history must remain queryable
- **No cascade:** `DeleteBehavior.Restrict` is set globally — never assume cascading deletes will work
- **Open/Closed:** Add new query filters by extending `IStudentService` — do not modify existing method signatures
- All `DateTime` → UTC

---

## Files to CREATE

### `CenterManagement.Application/DTOs/Common/PagedResult.cs`

```csharp
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
```

> This is used by ALL phases. Create it once here.

---

### `CenterManagement.Application/DTOs/Common/StudentListFilter.cs`

```csharp
public string? SearchQuery { get; set; }   // matches FullName or PhoneNumber
public int? GradeLevelId { get; set; }
public int? SubjectId { get; set; }
public string? PaymentStatus { get; set; } // "Paid" | "Partial" | "Unpaid"
public bool IncludeInactive { get; set; } = false;
public int Page { get; set; } = 1;
public int PageSize { get; set; } = 20;
```

---

### `CenterManagement.Application/DTOs/Student/CreateStudentDto.cs`

```csharp
public string FullName { get; set; } = string.Empty;
public string Email { get; set; } = string.Empty;
public string? PhoneNumber { get; set; }
public string ParentPhone { get; set; } = string.Empty;
public int GradeLevelId { get; set; }
public List<int> GroupIds { get; set; } = new();
public IFormFile? Photo { get; set; }
```

---

### `CenterManagement.Application/DTOs/Student/UpdateStudentDto.cs`

```csharp
public string FullName { get; set; } = string.Empty;
public string? PhoneNumber { get; set; }
public string ParentPhone { get; set; } = string.Empty;
public int GradeLevelId { get; set; }
public IFormFile? Photo { get; set; }
// Email is NOT editable after account creation
```

---

### `CenterManagement.Application/DTOs/Student/StudentListItemDto.cs`

```csharp
public int StudentProfileId { get; set; }
public string UserId { get; set; } = string.Empty;
public string FullName { get; set; } = string.Empty;
public string? PhoneNumber { get; set; }
public string ParentPhone { get; set; } = string.Empty;
public string GradeLevelName { get; set; } = string.Empty;
public decimal AttendanceRatePercent { get; set; }
public string PaymentStatus { get; set; } = string.Empty; // Paid / Partial / Unpaid
public bool IsActive { get; set; }
public string? ImagePath { get; set; }
public DateTime CreatedAt { get; set; }
```

---

### `CenterManagement.Application/DTOs/Student/EnrollmentDto.cs`

```csharp
public int EnrollmentId { get; set; }
public int GroupId { get; set; }
public string GroupName { get; set; } = string.Empty;
public string CourseName { get; set; } = string.Empty;
public string SubjectName { get; set; } = string.Empty;
public string InstructorName { get; set; } = string.Empty;
public bool IsActive { get; set; }
public DateTime EnrollmentDate { get; set; }
```

---

### `CenterManagement.Application/DTOs/Student/StudentCoursePaymentSummaryDto.cs`

```csharp
public int StudentCoursePaymentId { get; set; }
public string CourseName { get; set; } = string.Empty;
public decimal RequiredAmount { get; set; }
public decimal PaidAmount { get; set; }
public decimal RemainingAmount { get; set; }
public bool IsPaid { get; set; }
```

---

### `CenterManagement.Application/DTOs/Student/StudentAttendanceDto.cs`

```csharp
public int AttendanceId { get; set; }
public DateTime SessionDate { get; set; }
public string SubjectName { get; set; } = string.Empty;
public string GroupName { get; set; } = string.Empty;
public DateTime ScanTime { get; set; }
public bool IsPresent { get; set; }
public bool IsLate { get; set; }
```

---

### `CenterManagement.Application/DTOs/Student/StudentProfileDto.cs`

All fields from `StudentListItemDto` plus:

```csharp
public string Email { get; set; } = string.Empty;
public List<EnrollmentDto> Enrollments { get; set; } = new();
public List<StudentCoursePaymentSummaryDto> CoursePayments { get; set; } = new();
public List<StudentAttendanceDto> RecentAttendances { get; set; } = new(); // last 10
public List<SessionScheduleDto> UpcomingSessions { get; set; } = new();   // Phase 3 populates; use empty list here
```

---

### `CenterManagement.Application/Interfaces/IStudentService.cs`

```csharp
Task<int> CreateStudentAsync(CreateStudentDto dto, string adminId);
Task<StudentProfileDto> GetStudentProfileAsync(int studentProfileId);
Task<PagedResult<StudentListItemDto>> GetStudentListAsync(StudentListFilter filter);
Task UpdateStudentAsync(int id, UpdateStudentDto dto, string adminId);
Task SoftDeleteAsync(int id, string adminId);
Task ToggleActiveAsync(int id, string adminId);
Task TransferStudentAsync(int studentProfileId, int fromGroupId, int toGroupId, string adminId);
Task AddToGroupAsync(int studentProfileId, int groupId, string adminId);
Task RemoveFromGroupAsync(int studentProfileId, int groupId, string adminId);
Task<List<StudentListItemDto>> SearchStudentsAsync(string query, int maxResults = 10);
```

---

### `CenterManagement.Application/Services/StudentService.cs`

**Implements:** `IStudentService`  
**Inject:** `CenterManagementDbContext`, `UserManager<ApplicationUser>`, `RoleManager<IdentityRole>`, `IAuditLogService`, `IFileUploadService`

**`CreateStudentAsync` — exact flow:**
1. Begin `IDbContextTransaction`
2. Create `ApplicationUser { FullName, Email, UserName=Email, PhoneNumber, IsActive=true }`
3. `UserManager.CreateAsync(user, "Student@" + last4ofParentPhone)` — if fails, throw with `IdentityResult.Errors` joined as message
4. `UserManager.AddToRoleAsync(user, "Student")`
5. If `dto.Photo != null` → `IFileUploadService.UploadPhotoAsync`, set `user.ImagePath`, update user
6. Create `StudentProfile { UserId=user.Id, GradeLevelId, ParentPhone }`; `_db.StudentProfiles.Add(profile)`
7. For each `GroupId` in `dto.GroupIds` → create `Enrollment { StudentProfileId, GroupId, IsActive=true, EnrollmentDate=UtcNow }`
8. `await _db.SaveChangesAsync()`
9. Commit transaction
10. `await _audit.LogAsync(adminId, "StudentCreated", "StudentProfile", profile.Id, null, ...)`
11. On any exception: rollback; if user was created, call `UserManager.DeleteAsync(user)`

**`GetStudentListAsync` — IQueryable chain (no ToList before filters):**
```csharp
var query = _db.StudentProfiles
    .Include(x => x.User)
    .Include(x => x.GradeLevel)
    .Include(x => x.Enrollments).ThenInclude(e => e.Group).ThenInclude(g => g.Course).ThenInclude(c => c.Subject)
    .Include(x => x.CoursePayments)
    .AsNoTracking();

if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
    query = query.Where(x =>
        x.User.FullName.Contains(filter.SearchQuery) ||
        x.User.PhoneNumber!.Contains(filter.SearchQuery));

if (filter.GradeLevelId.HasValue)
    query = query.Where(x => x.GradeLevelId == filter.GradeLevelId);

if (!filter.IncludeInactive)
    query = query.Where(x => x.User.IsActive);
```

**PaymentStatus logic (compute in memory after projection — small set per student):**
- All `CoursePayments` have `IsPaid=true` → `"Paid"`
- Any `RemainingAmount > 0` → `"Partial"`
- None paid at all → `"Unpaid"`

**`TransferStudentAsync`:**
1. Find active `Enrollment` where `StudentProfileId == id && GroupId == fromGroupId && IsActive == true`
2. `enrollment.IsActive = false; enrollment.UpdatedAt = UtcNow`
3. Check no existing active enrollment already exists for `toGroupId` (duplicate unique index would fail — validate first)
4. Create new `Enrollment { StudentProfileId, GroupId=toGroupId, IsActive=true, EnrollmentDate=UtcNow }`
5. `SaveChangesAsync()`
6. Audit both group IDs

**`SoftDeleteAsync`:**
- `studentProfile.IsDeleted = true; studentProfile.UpdatedAt = UtcNow`
- Do **NOT** touch `ApplicationUser.IsDeleted` — user account remains

**`ToggleActiveAsync`:**
- Toggle `applicationUser.IsActive`
- `UserManager.UpdateAsync(user)`
- Audit

---

## Files to MODIFY

### `CenterManagement.Application/DependencyInjection/ApplicationServiceRegistration.cs`

Add:
```csharp
services.AddScoped<IStudentService, StudentService>();
```

---

### `CenterManagement.Infrastructure/Seed/IdentitySeeder.cs`

After the admin user seed block, add:

```csharp
// Seed GradeLevels
if (!await context.GradeLevels.AnyAsync())
{
    context.GradeLevels.AddRange(
        new GradeLevel { Name = "Grade 1" },
        new GradeLevel { Name = "Grade 2" },
        new GradeLevel { Name = "Grade 3" },
        new GradeLevel { Name = "Grade 4" },
        new GradeLevel { Name = "Grade 5" }
    );
    await context.SaveChangesAsync();
}

// Seed Subjects
if (!await context.Subjects.AnyAsync())
{
    context.Subjects.AddRange(
        new Subject { Name = "Mathematics" },
        new Subject { Name = "Science" },
        new Subject { Name = "English" },
        new Subject { Name = "History" },
        new Subject { Name = "Art" }
    );
    await context.SaveChangesAsync();
}
```

Inject `CenterManagementDbContext` into the seeder method signature.

---

## Web Layer

### `CenterManagement.Web/ViewModels/Student/StudentListViewModel.cs`

```csharp
public List<StudentListItemDto> Students { get; set; } = new();
public int TotalCount { get; set; }
public int Page { get; set; }
public int PageSize { get; set; }
public int TotalPages { get; set; }
public string? SearchQuery { get; set; }
public int? GradeLevelId { get; set; }
public int? SubjectId { get; set; }
public string? PaymentStatusFilter { get; set; }
public SelectList GradeLevelSelectList { get; set; } = null!;
public SelectList SubjectSelectList { get; set; } = null!;
```

---

### `CenterManagement.Web/ViewModels/Student/CreateStudentViewModel.cs`

Mirrors `CreateStudentDto` with:
- `[Required]` on `FullName`, `ParentPhone`, `GradeLevelId`
- `[Required][EmailAddress]` on `Email`
- `SelectList GradeLevelSelectList`
- `SelectList GroupSelectList`

---

### `CenterManagement.Web/ViewModels/Student/UpdateStudentViewModel.cs`

```csharp
public int StudentProfileId { get; set; }
[Required] public string FullName { get; set; } = string.Empty;
public string? PhoneNumber { get; set; }
[Required] public string ParentPhone { get; set; } = string.Empty;
[Required] public int GradeLevelId { get; set; }
public IFormFile? Photo { get; set; }
public string? CurrentImagePath { get; set; }
public SelectList GradeLevelSelectList { get; set; } = null!;
```

---

### `CenterManagement.Web/ViewModels/Student/TransferStudentViewModel.cs`

```csharp
public int StudentProfileId { get; set; }
public string StudentName { get; set; } = string.Empty;
public int FromGroupId { get; set; }
public string FromGroupName { get; set; } = string.Empty;
[Required] public int ToGroupId { get; set; }
public SelectList AvailableGroupsSelectList { get; set; } = null!;
```

---

### `CenterManagement.Web/Controllers/StudentController.cs`

`[Authorize(Roles = "Admin")]`  
**Inject:** `IStudentService`

| Method | Route | Action |
|--------|-------|--------|
| `GET` | `/Student` | `Index(StudentListFilter filter)` → `StudentListViewModel` |
| `GET` | `/Student/Create` | `Create()` → populates select lists → `View` |
| `POST` | `/Student/Create` | `CreatePost(CreateStudentViewModel)` → redirect `Index` |
| `GET` | `/Student/Profile/{id}` | `Profile(int id)` → `StudentProfileDto` → `View` |
| `GET` | `/Student/Edit/{id}` | `Edit(int id)` → `UpdateStudentViewModel` → `View` |
| `POST` | `/Student/Edit/{id}` | `EditPost(int id, UpdateStudentViewModel)` → redirect `Profile` |
| `POST` | `/Student/Delete/{id}` | `Delete(int id)` → soft delete → redirect `Index` |
| `POST` | `/Student/ToggleActive/{id}` | `ToggleActive(int id)` → `Json({success, isActive})` |
| `GET` | `/Student/Transfer/{id}` | `Transfer(int id)` → `TransferStudentViewModel` → `View` |
| `POST` | `/Student/Transfer` | `TransferPost(TransferStudentViewModel)` → redirect `Profile` |
| `POST` | `/Student/AddToGroup` | `AddToGroup([FromBody])` → `Json({success})` |
| `POST` | `/Student/RemoveFromGroup` | `RemoveFromGroup([FromBody])` → `Json({success})` |
| `GET` | `/Student/Search` | `Search(string q)` → `Json(List<StudentListItemDto>)` |

**All POST actions:** validate `ModelState` first; on failure re-return `View(model)`.  
**All JSON actions:** wrap in try/catch; return `Json({success=false, error=ex.Message})` on failure.

---

### `CenterManagement.Web/Views/Student/Index.cshtml`

`@model StudentListViewModel` · Layout `"_Layout"`

Scaffolded from `studentmangment.html`. Replace all static data with `@Model` bindings:

- **Search input** → `asp-for="SearchQuery"` inside `<form method="get" asp-action="Index">`
- **Grade filter select** → `asp-for="GradeLevelId"` + `asp-items="Model.GradeLevelSelectList"`
- **"Add Student" button** → `asp-controller="Student" asp-action="Create"`
- **Table rows** → `@foreach (var s in Model.Students)` with:
  - Avatar: `<img src="@(s.ImagePath ?? "/images/avatar-default.png")">`
  - Payment badge: `Paid` = green pill, `Partial` = orange pill, `Unpaid` = red pill
  - Actions dropdown: View Profile → `/Student/Profile/{id}`, Edit → `/Student/Edit/{id}`, Transfer → `/Student/Transfer/{id}`, Toggle Active → `POST /Student/ToggleActive/{id}` via fetch
- **Pagination** → `<a asp-route-page="@p">` for each page number

---

### `CenterManagement.Web/Views/Student/Create.cshtml`

`@model CreateStudentViewModel` · Layout `"_Layout"`

Scaffolded from `add-newStudent.html`. Key wiring:

```html
<form asp-action="CreatePost" asp-controller="Student"
      method="post" enctype="multipart/form-data">
    @Html.AntiForgeryToken()
```

- Photo circle: `<input type="file" asp-for="Photo" id="photo-input" class="hidden" accept="image/png,image/jpeg,image/webp">` — JS previews in the circle on change
- `asp-for="FullName"` + `asp-validation-for="FullName"`
- `asp-for="PhoneNumber"`
- `asp-for="ParentPhone"` + `asp-validation-for="ParentPhone"`
- Grade select: `asp-for="GradeLevelId"` + `asp-items="Model.GradeLevelSelectList"` + `asp-validation-for="GradeLevelId"`
- Group select: `asp-for="GroupId"` + `asp-items="Model.GroupSelectList"`
- Cancel button: `type="button" onclick="history.back()"`
- Save button: `type="submit"`

---

### `CenterManagement.Web/Views/Student/Edit.cshtml`

`@model UpdateStudentViewModel` · Layout `"_Layout"`

Same structure as `Create.cshtml`. Pre-populates all fields. Hidden input: `asp-for="StudentProfileId"`. Shows current photo if `Model.CurrentImagePath` is not null. Posts to `POST /Student/Edit/{id}`.

---

### `CenterManagement.Web/Views/Student/Profile.cshtml`

`@model StudentProfileDto` · Layout `"_Layout"`

Scaffolded from `studentProfile.html`.

**Header:** photo, name, grade badge. Edit button → `asp-action="Edit" asp-route-id="@Model.StudentProfileId"`.

**6 tabs** (copy `switchTab()` JS from `studentProfile.html`):

| Tab | Content |
|-----|---------|
| Personal | FullName, Email, Phone, ParentPhone, GradeLevel, JoinDate (`CreatedAt`), IsActive toggle → POST `ToggleActive` |
| Subjects | `@foreach (var e in Model.Enrollments)` — group name, course, subject icon, active badge |
| Attendance | Table of `Model.RecentAttendances` (last 10): Date, Subject, Group, ScanTime, Present/Late badge |
| Payments | Financial summary card: TotalRequired / TotalPaid / Remaining. List of `Model.CoursePayments`. "Record Payment" button → POST `/Payment/CreateCourse` |
| Notes | Empty state placeholder |
| Sessions | List of `Model.UpcomingSessions` (empty list in Phase 2 — Phase 3 data populates it) |

---

### `CenterManagement.Web/wwwroot/images/avatar-default.png`

Place a 128×128 neutral gray circle avatar PNG here. Used as fallback when `ImagePath` is null.

---

## Completion Checklist

- [ ] `POST /Student/Create` with valid data → `AspNetUsers` + `StudentProfiles` + `AspNetUserRoles` records created in one transaction
- [ ] Transaction rollback test: simulate `SaveChangesAsync` failure after user creation → no orphaned user in DB
- [ ] Photo upload → file in `wwwroot/uploads/students/`, `ImagePath` stored in `ApplicationUser`
- [ ] `GET /Student?q=Ahmed` → only students matching name or phone returned
- [ ] Filter by grade → only correct grade shown
- [ ] Transfer → old `Enrollment.IsActive=false` + new `Enrollment` created; old attendance still joinable via old enrollment
- [ ] Soft delete → `StudentProfile.IsDeleted=true`; student absent from list but row present in DB
- [ ] `POST /Student/ToggleActive/{id}` → returns `{success: true, isActive: bool}` JSON
- [ ] All 6 profile tabs switch without page reload
- [ ] Duplicate email on create → model error shown, no orphaned records in DB
