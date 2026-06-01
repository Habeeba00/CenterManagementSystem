# Phase 3 — Academic Core (Courses, Groups & Sessions)

**Developer:** Dev 3  
**HTML Sources:** `sessionDeatails.html` (header section only — QR panel and attendance table are left as a placeholder `<div>` for Phase 4)  
**Hard dependency:** Phase 1 (`_Layout.cshtml`, `IAuditLogService`, `INotificationService` interface)  
**Soft dependency:** Phase 2 seed data (`GradeLevels`, `Subjects`) must be in DB before testing

---

## Constraints (Read Before Writing Any Code)

- **No direct DB queries in controllers** — all access via service interfaces
- **Instructor authorization on `CancelSessionAsync`:** If the calling user has the `Instructor` role, the service MUST verify at runtime that the session's `Group.InstructorProfileId` matches the caller's `InstructorProfile.Id` — never trust the role claim alone
- **Overlap validation in `CreateSessionAsync`** is enforced in the service, not the controller
- **Soft delete only** — never call `DbContext.Remove()` on any entity
- **`INotificationService.SendToGroupAsync`** is called after session cancellation — never skip this call even if notifications are stubbed in Phase 1
- **Session cancellation is final** — once `IsCanceled=true`, no method in this phase reverses it
- **SOLID:** `CourseService`, `GroupService`, `SessionService`, `InstructorService` are four separate classes with single responsibilities
- All `DateTime` → UTC

---

## Files to CREATE

### DTOs

#### `CenterManagement.Application/DTOs/Course/CreateCourseDto.cs`
```csharp
public string Name { get; set; } = string.Empty;
public decimal Price { get; set; }
public int SubjectId { get; set; }
public int GradeLevelId { get; set; }
```

#### `CenterManagement.Application/DTOs/Course/UpdateCourseDto.cs`
Same fields as `CreateCourseDto`.

#### `CenterManagement.Application/DTOs/Course/CourseDto.cs`
```csharp
public int Id { get; set; }
public string Name { get; set; } = string.Empty;
public decimal Price { get; set; }
public string SubjectName { get; set; } = string.Empty;
public string GradeLevelName { get; set; } = string.Empty;
public int GroupCount { get; set; }
```

---

#### `CenterManagement.Application/DTOs/Group/CreateGroupDto.cs`
```csharp
public string Name { get; set; } = string.Empty;
public int CourseId { get; set; }
public int InstructorProfileId { get; set; }
public bool IsActive { get; set; } = true;
```

#### `CenterManagement.Application/DTOs/Group/UpdateGroupDto.cs`
```csharp
public string Name { get; set; } = string.Empty;
public bool IsActive { get; set; }
```

#### `CenterManagement.Application/DTOs/Group/GroupListItemDto.cs`
```csharp
public int Id { get; set; }
public string Name { get; set; } = string.Empty;
public string CourseName { get; set; } = string.Empty;
public string SubjectName { get; set; } = string.Empty;
public string InstructorName { get; set; } = string.Empty;
public int InstructorProfileId { get; set; }
public int EnrollmentCount { get; set; }
public bool IsActive { get; set; }
```

#### `CenterManagement.Application/DTOs/Group/GroupDetailDto.cs`
All `GroupListItemDto` fields plus:
```csharp
public List<SessionListItemDto> RecentSessions { get; set; } = new();
public List<EnrollmentDto> Enrollments { get; set; } = new(); // from Phase 2
```

---

#### `CenterManagement.Application/DTOs/Session/CreateSessionDto.cs`
```csharp
public int GroupId { get; set; }
public DateTime SessionDate { get; set; }
public TimeSpan StartTime { get; set; }
public TimeSpan EndTime { get; set; }
```

#### `CenterManagement.Application/DTOs/Session/SessionListItemDto.cs`
```csharp
public int Id { get; set; }
public DateTime SessionDate { get; set; }
public TimeSpan StartTime { get; set; }
public TimeSpan EndTime { get; set; }
public string GroupName { get; set; } = string.Empty;
public string CourseName { get; set; } = string.Empty;
public string SubjectName { get; set; } = string.Empty;
public string GradeLevelName { get; set; } = string.Empty;
public string InstructorName { get; set; } = string.Empty;
public bool IsCanceled { get; set; }
public string? CancelReason { get; set; }
public int AttendanceCount { get; set; }
```

#### `CenterManagement.Application/DTOs/Session/SessionDetailDto.cs`
All `SessionListItemDto` fields plus:
```csharp
public int SessionId { get; set; }
public int GroupId { get; set; }
public int InstructorProfileId { get; set; }
public int EnrolledCount { get; set; }
// Phase 4 appends attendance list — declare as empty list here:
public List<object> AttendanceList { get; set; } = new();
```

#### `CenterManagement.Application/DTOs/Session/SessionScheduleDto.cs`
```csharp
public int SessionId { get; set; }
public string CourseTitle { get; set; } = string.Empty;
public string GroupName { get; set; } = string.Empty;
public string InstructorName { get; set; } = string.Empty;
public TimeSpan StartTime { get; set; }
public TimeSpan EndTime { get; set; }
public bool IsCanceled { get; set; }
public int EnrolledCount { get; set; }
```

---

#### `CenterManagement.Application/DTOs/Instructor/CreateInstructorDto.cs`
```csharp
public string FullName { get; set; } = string.Empty;
public string Email { get; set; } = string.Empty;
public string Specialization { get; set; } = string.Empty;
public int? SubjectId { get; set; }
public IFormFile? Photo { get; set; }
```

#### `CenterManagement.Application/DTOs/Instructor/UpdateInstructorDto.cs`
```csharp
public string FullName { get; set; } = string.Empty;
public string Specialization { get; set; } = string.Empty;
public int? SubjectId { get; set; }
public IFormFile? Photo { get; set; }
```

#### `CenterManagement.Application/DTOs/Instructor/InstructorListItemDto.cs`
```csharp
public int InstructorProfileId { get; set; }
public string UserId { get; set; } = string.Empty;
public string FullName { get; set; } = string.Empty;
public string Specialization { get; set; } = string.Empty;
public string? SubjectName { get; set; }
public int GroupCount { get; set; }
public bool IsActive { get; set; }
public string? ImagePath { get; set; }
```

#### `CenterManagement.Application/DTOs/Instructor/InstructorProfileDto.cs`
All `InstructorListItemDto` fields plus:
```csharp
public List<GroupListItemDto> Groups { get; set; } = new();
public List<SessionListItemDto> UpcomingSessions { get; set; } = new();
```

---

### Interfaces

#### `CenterManagement.Application/Interfaces/ICourseService.cs`
```csharp
Task<int> CreateCourseAsync(CreateCourseDto dto, string adminId);
Task<List<CourseDto>> GetAllCoursesAsync(int? gradeLevelId, int? subjectId);
Task<CourseDto> GetCourseByIdAsync(int id);
Task UpdateCourseAsync(int id, UpdateCourseDto dto, string adminId);
Task SoftDeleteAsync(int id, string adminId);
```

#### `CenterManagement.Application/Interfaces/IGroupService.cs`
```csharp
Task<int> CreateGroupAsync(CreateGroupDto dto, string adminId);
Task<GroupDetailDto> GetGroupDetailAsync(int groupId);
Task<List<GroupListItemDto>> GetAllGroupsAsync();
Task<List<GroupListItemDto>> GetGroupsByInstructorAsync(int instructorProfileId);
Task UpdateGroupAsync(int id, UpdateGroupDto dto, string adminId);
Task ChangeInstructorAsync(int groupId, int newInstructorProfileId, string adminId);
Task SoftDeleteAsync(int id, string adminId);
```

#### `CenterManagement.Application/Interfaces/ISessionService.cs`
```csharp
Task<int> CreateSessionAsync(CreateSessionDto dto, string adminId);
Task<SessionDetailDto> GetSessionDetailAsync(int sessionId);
Task<List<SessionListItemDto>> GetSessionsByGroupAsync(int groupId, DateTime? from, DateTime? to);
Task<List<SessionListItemDto>> GetSessionsByDateAsync(DateTime date);
Task<List<SessionListItemDto>> GetSessionsByDateRangeAsync(DateTime from, DateTime to);
Task CancelSessionAsync(int sessionId, string cancelReason, string performedByUserId);
Task<int?> GetInstructorProfileIdByUserIdAsync(string userId);
```

#### `CenterManagement.Application/Interfaces/IInstructorService.cs`
```csharp
Task<int> CreateInstructorAsync(CreateInstructorDto dto, string adminId);
Task<InstructorProfileDto> GetInstructorProfileAsync(int id);
Task<List<InstructorListItemDto>> GetAllInstructorsAsync();
Task UpdateInstructorAsync(int id, UpdateInstructorDto dto, string adminId);
Task SoftDeleteAsync(int id, string adminId);
```

---

### Services

#### `CenterManagement.Application/Services/CourseService.cs`

**Implements:** `ICourseService`  
**Inject:** `CenterManagementDbContext`, `IAuditLogService`

**`CreateCourseAsync`:**
- Validate no existing non-deleted course with same `Name` + `GradeLevelId` + `SubjectId` → throw `InvalidOperationException` if duplicate
- Write audit `action="CourseCreated"`

**`SoftDeleteAsync`:**
- Check no active `Groups` linked to this course before deleting → throw if groups exist
- Set `IsDeleted = true`

---

#### `CenterManagement.Application/Services/GroupService.cs`

**Implements:** `IGroupService`  
**Inject:** `CenterManagementDbContext`, `IAuditLogService`

**`ChangeInstructorAsync`:**
- Load group with tracking
- Store old `InstructorProfileId` for audit
- Set `group.InstructorProfileId = newInstructorProfileId`
- Write audit with `OldValues = oldId.ToString()`, `NewValues = newId.ToString()`, `action="InstructorChanged"`

**`SoftDeleteAsync`:**
- Check no active `Enrollments` before soft-deleting a group → throw if students are enrolled

---

#### `CenterManagement.Application/Services/SessionService.cs`

**Implements:** `ISessionService`  
**Inject:** `CenterManagementDbContext`, `IAuditLogService`, `INotificationService`, `UserManager<ApplicationUser>`

**`CreateSessionAsync` — validation rules (in this order):**
1. `dto.SessionDate.Date < DateTime.UtcNow.Date` → throw `"Session date cannot be in the past"`
2. `dto.StartTime >= dto.EndTime` → throw `"Start time must be before end time"`
3. Overlap check:
```csharp
var overlap = await _db.Sessions
    .Where(s =>
        s.GroupId == dto.GroupId &&
        s.SessionDate.Date == dto.SessionDate.Date &&
        !s.IsCanceled &&
        !s.IsDeleted &&
        (
            (dto.StartTime >= s.StartTime && dto.StartTime < s.EndTime) ||
            (dto.EndTime > s.StartTime && dto.EndTime <= s.EndTime) ||
            (dto.StartTime <= s.StartTime && dto.EndTime >= s.EndTime)
        )
    )
    .AnyAsync();
if (overlap)
    throw new InvalidOperationException("An overlapping session already exists for this group on that date.");
```
4. Write audit `action="SessionCreated"`

**`CancelSessionAsync`:**
1. Load session with `Include(s => s.Group)`
2. If `session.IsCanceled` → throw `"Session is already canceled"`
3. If caller has `Instructor` role → call `GetInstructorProfileIdByUserIdAsync(performedByUserId)` → if result != `session.Group.InstructorProfileId` → throw `UnauthorizedAccessException("You do not own this session's group")`
4. Set `session.IsCanceled = true`, `session.CancelReason = cancelReason`, `session.UpdatedAt = UtcNow`
5. `await _db.SaveChangesAsync()`
6. `await _notificationService.SendToGroupAsync(session.GroupId, "Session Canceled", $"The session on {session.SessionDate:d} has been canceled. Reason: {cancelReason}")`
7. Write audit `action="SessionCanceled"`

**`GetInstructorProfileIdByUserIdAsync`:**
```csharp
return await _db.InstructorProfiles
    .Where(x => x.UserId == userId)
    .Select(x => (int?)x.Id)
    .FirstOrDefaultAsync();
```

---

#### `CenterManagement.Application/Services/InstructorService.cs`

**Implements:** `IInstructorService`  
**Inject:** `CenterManagementDbContext`, `UserManager<ApplicationUser>`, `RoleManager<IdentityRole>`, `IAuditLogService`, `IFileUploadService`

**`CreateInstructorAsync`:** Same transaction pattern as `StudentService.CreateStudentAsync` — wrap in `IDbContextTransaction`. Create `ApplicationUser`, assign `Instructor` role, create `InstructorProfile { UserId, Specialization, SubjectId }`. Temp password: `"Instructor@" + Guid.NewGuid().ToString()[..6]`. Rollback on any failure.

**`SoftDeleteAsync`:** Check no active `Groups` linked to this instructor → throw if found. Set `InstructorProfile.IsDeleted = true`.

---

## Files to MODIFY

### `CenterManagement.Application/DependencyInjection/ApplicationServiceRegistration.cs`

Add:
```csharp
services.AddScoped<ICourseService, CourseService>();
services.AddScoped<IGroupService, GroupService>();
services.AddScoped<ISessionService, SessionService>();
services.AddScoped<IInstructorService, InstructorService>();
```

---

## Web Layer

### ViewModels

#### `CenterManagement.Web/ViewModels/Course/CreateCourseViewModel.cs`
Maps `CreateCourseDto`. Adds `[Required]` on `Name`, `[Range(0.01, 99999.99)]` on `Price`. Includes `SelectList SubjectSelectList` and `SelectList GradeLevelSelectList`.

#### `CenterManagement.Web/ViewModels/Group/CreateGroupViewModel.cs`
Maps `CreateGroupDto`. Includes `SelectList CourseSelectList` and `SelectList InstructorSelectList`.

#### `CenterManagement.Web/ViewModels/Session/CreateSessionViewModel.cs`
Maps `CreateSessionDto`. Adds `[Required]` on `GroupId`, `SessionDate`. `[DataType(DataType.Time)]` on `StartTime`, `EndTime`. Includes `SelectList GroupSelectList`.

#### `CenterManagement.Web/ViewModels/Session/SessionDetailViewModel.cs`
```csharp
public SessionDetailDto SessionDetail { get; set; } = null!;
public bool CanCancel { get; set; }
// CanCancel = User.IsInRole("Admin") OR
//             (User.IsInRole("Instructor") AND instructorProfileId == SessionDetail.InstructorProfileId)
```

#### `CenterManagement.Web/ViewModels/Instructor/CreateInstructorViewModel.cs`
Maps `CreateInstructorDto`. Adds `[Required]` on `FullName`, `Email`, `Specialization`. Includes `SelectList SubjectSelectList`.

---

### Controllers

#### `CenterManagement.Web/Controllers/CourseController.cs`

`[Authorize(Roles = "Admin")]` · **Inject:** `ICourseService`

| Method | Route | Action |
|--------|-------|--------|
| `GET` | `/Course` | `Index(int? gradeLevelId, int? subjectId)` → list |
| `GET` | `/Course/Create` | `Create()` → form |
| `POST` | `/Course/Create` | `CreatePost(CreateCourseViewModel)` → redirect `Index` |
| `GET` | `/Course/Edit/{id}` | `Edit(int id)` → form pre-populated |
| `POST` | `/Course/Edit/{id}` | `EditPost(int id, UpdateCourseViewModel)` → redirect `Index` |
| `POST` | `/Course/Delete/{id}` | `Delete(int id)` → soft delete → redirect `Index` |

---

#### `CenterManagement.Web/Controllers/GroupController.cs`

`[Authorize(Roles = "Admin,Instructor")]` · **Inject:** `IGroupService`, `IInstructorService`, `ICourseService`

| Method | Route | Auth | Action |
|--------|-------|------|--------|
| `GET` | `/Group` | Both | `Index()` — Admin: all groups; Instructor: own groups via `GetGroupsByInstructorAsync` |
| `GET` | `/Group/Create` | Admin inline check | `Create()` |
| `POST` | `/Group/Create` | Admin inline check | `CreatePost(CreateGroupViewModel)` |
| `GET` | `/Group/Edit/{id}` | Admin inline check | `Edit(int id)` |
| `POST` | `/Group/Edit/{id}` | Admin inline check | `EditPost(int id, UpdateGroupViewModel)` |
| `POST` | `/Group/ChangeInstructor` | Admin inline check | `ChangeInstructor([FromBody])` → `Json({success})` |

Instructor check in actions: `if (!User.IsInRole("Admin")) return Forbid();`

---

#### `CenterManagement.Web/Controllers/SessionController.cs`

`[Authorize(Roles = "Admin,Instructor")]` · **Inject:** `ISessionService`, `IGroupService`

| Method | Route | Auth | Action |
|--------|-------|------|--------|
| `GET` | `/Session` | Both | `Index(DateTime? date, int? groupId)` |
| `GET` | `/Session/Create` | Admin inline check | `Create()` |
| `POST` | `/Session/Create` | Admin inline check | `CreatePost(CreateSessionViewModel)` |
| `GET` | `/Session/Detail/{id}` | Both | `Detail(int id)` → `SessionDetailViewModel` |
| `POST` | `/Session/Cancel/{id}` | Both (service enforces ownership) | `Cancel(int id, string cancelReason)` → `Json({success, message})` |

**Building `SessionDetailViewModel` in `Detail` action:**
```csharp
var detail = await _sessionService.GetSessionDetailAsync(id);
var instructorProfileId = await _sessionService.GetInstructorProfileIdByUserIdAsync(
    User.FindFirstValue(ClaimTypes.NameIdentifier)!);
var canCancel = User.IsInRole("Admin")
    || (User.IsInRole("Instructor") && instructorProfileId == detail.InstructorProfileId);
return View(new SessionDetailViewModel { SessionDetail = detail, CanCancel = canCancel });
```

---

#### `CenterManagement.Web/Controllers/InstructorController.cs`

`[Authorize(Roles = "Admin")]` · **Inject:** `IInstructorService`

| Method | Route | Action |
|--------|-------|--------|
| `GET` | `/Instructor` | `Index()` → list |
| `GET` | `/Instructor/Create` | `Create()` |
| `POST` | `/Instructor/Create` | `CreatePost(CreateInstructorViewModel)` |
| `GET` | `/Instructor/Profile/{id}` | `Profile(int id)` |
| `GET` | `/Instructor/Edit/{id}` | `Edit(int id)` |
| `POST` | `/Instructor/Edit/{id}` | `EditPost(int id, UpdateInstructorViewModel)` |
| `POST` | `/Instructor/Delete/{id}` | `Delete(int id)` → soft delete |

---

### Views

#### `CenterManagement.Web/Views/Course/Index.cshtml`
`@model List<CourseDto>` · Layout `"_Layout"`  
Table: Course Name, Subject, Grade Level, Price (right-aligned, `font-mono`), Group Count, Edit/Delete actions. "Add Course" button top-right.

#### `CenterManagement.Web/Views/Course/Create.cshtml`
`@model CreateCourseViewModel`  
Name input, price (`type="number" step="0.01"`), subject select, grade select. Submit + Cancel.

#### `CenterManagement.Web/Views/Course/Edit.cshtml`
`@model UpdateCourseViewModel` · Pre-populated form.

---

#### `CenterManagement.Web/Views/Group/Index.cshtml`
`@model List<GroupListItemDto>`  
Table: Group Name, Course, Subject, Instructor (with pencil icon that opens a Change Instructor modal → POST `/Group/ChangeInstructor` via fetch), Enrollment Count, Active badge, Actions.

#### `CenterManagement.Web/Views/Group/Create.cshtml`
`@model CreateGroupViewModel` · Name, course select, instructor select. Submit + Cancel.

#### `CenterManagement.Web/Views/Group/Edit.cshtml`
`@model UpdateGroupViewModel` · Name + IsActive toggle.

---

#### `CenterManagement.Web/Views/Session/Index.cshtml`
`@model List<SessionListItemDto>`  
Date picker filter + Group filter. Table: Date, Time range, Group, Course, Instructor, Status badge (Upcoming/Active/Canceled). Actions: Detail, Cancel. "New Session" button shown only to Admins: `@if (User.IsInRole("Admin"))`.

#### `CenterManagement.Web/Views/Session/Create.cshtml`
`@model CreateSessionViewModel` · Group select, date picker, start time, end time. Client-side validation: `startTime < endTime`. Submit + Cancel.

#### `CenterManagement.Web/Views/Session/Detail.cshtml`

`@model SessionDetailViewModel` · Layout `"_Layout"`

**Phase 3 owns the TOP section — scaffolded from the header of `sessionDeatails.html`:**

- Session title: `@Model.SessionDetail.CourseName`
- Chips row: Group name, Instructor name, Time range (`StartTime`–`EndTime`), Enrollment count, Grade level
- Edit button: Admin only → `asp-action="Edit"`
- Cancel button: `@if (Model.CanCancel)` → opens cancel modal with `<textarea id="cancel-reason">` → on confirm: POST `fetch("/Session/Cancel/@Model.SessionDetail.Id", { body: JSON.stringify({cancelReason}) })`
- Error alert: `<div id="scan-error-alert" class="hidden ...">` (Phase 4 uses this)

**Phase 4 placeholder — leave exactly this markup:**
```html
<!-- ============================================================ -->
<!-- PHASE 4 PLACEHOLDER — DO NOT MODIFY THIS SECTION            -->
<!-- Phase 4 replaces this div with QR scanner + attendance table -->
<!-- ============================================================ -->
<div id="phase4-attendance-section">
    <p class="text-muted text-body-sm">Attendance tracking — Phase 4</p>
</div>
```

---

#### `CenterManagement.Web/Views/Instructor/Index.cshtml`
`@model List<InstructorListItemDto>`  
Table: Photo, Name, Specialization, Subject, Group Count, Active badge, Actions (Profile/Edit/Delete).

#### `CenterManagement.Web/Views/Instructor/Create.cshtml`
`@model CreateInstructorViewModel` · Full name, email, specialization, subject select, photo upload. Submit + Cancel.

#### `CenterManagement.Web/Views/Instructor/Profile.cshtml`
`@model InstructorProfileDto` · Header: photo, name, specialization, subject. Groups table. Upcoming sessions list.

#### `CenterManagement.Web/Views/Instructor/Edit.cshtml`
`@model UpdateInstructorViewModel` · Pre-populated form for FullName, Specialization, SubjectId.

---

## Completion Checklist

- [ ] Create Course → appears in `/Course` list with correct Grade + Subject
- [ ] Duplicate course (same name+grade+subject) → service throws, form shows error
- [ ] Create Group → shows in `/Group` list with instructor name, enrollment count 0
- [ ] Create Session → appears in `/Session` list for the correct date
- [ ] Overlapping session attempt → validation error, no record created
- [ ] `POST /Session/Cancel/{id}` as Admin → `IsCanceled=true` in DB, `Notification` record created per enrolled student
- [ ] `POST /Session/Cancel/{id}` as Instructor for wrong group → `UnauthorizedAccessException` → controller returns `Json({success=false})`
- [ ] `GET /Session/Detail/{id}` → header section renders correctly; `#phase4-attendance-section` placeholder is present
- [ ] `CanCancel=false` for Instructor viewing another instructor's session → Cancel button hidden
- [ ] Create Instructor → `AspNetUsers` + `InstructorProfiles` + `AspNetUserRoles` created in one transaction
