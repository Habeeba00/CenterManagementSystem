# Phase 6 — Dashboards, Notifications & Analytics

**Developer:** Dev 6  
**HTML Sources:** `receptioneestDash.html` · `insights.html`  
**Hard dependencies:** All Phases 1–5 must have at least one data-producing endpoint each

---

## Constraints (Read Before Writing Any Code)

- **No `.ToList()` before aggregation** — all KPI and analytics queries use EF Core async methods (`.SumAsync`, `.CountAsync`, `.GroupBy` on `IQueryable`) — never load records into memory to aggregate
- **`GetSmartInsightsAsync` must use server-side aggregation** — never load all `StudentAttendance` records to compute per-student rates in C#
- **`DashboardService` and `AnalyticsService` inject `CenterManagementDbContext` directly** — they perform read-only aggregation queries and do not write data; this is an acceptable Clean Architecture exception for reporting services
- **`NotificationService`** replaces the Phase 1 stub entirely — `SendToGroupAsync` must create one `Notification` record per enrolled student in a single `SaveChangesAsync()` call (one batch, not N round trips)
- **All `DateTime` comparisons use UTC**
- **Soft delete:** the global query filter on `DbContext` already excludes `IsDeleted=true` records — no need to add `.Where(!IsDeleted)` manually in queries
- **SOLID:** `DashboardService` serves the receptionist dashboard; `AnalyticsService` serves the analytics page — two separate classes with single responsibilities

---

## Files to CREATE

### DTOs

#### `CenterManagement.Application/DTOs/Dashboard/DashboardKpiDto.cs`
```csharp
public int TotalStudents { get; set; }
public int ActiveSessionsNow { get; set; }
public decimal AttendanceRateLast7Days { get; set; }
public decimal RevenueTodayAmount { get; set; }
public int NewStudentsThisMonth { get; set; }
```

#### `CenterManagement.Application/DTOs/Dashboard/ActiveSessionDto.cs`
```csharp
public int SessionId { get; set; }
public string CourseTitle { get; set; } = string.Empty;
public string GroupName { get; set; } = string.Empty;
public string GradeLevelName { get; set; } = string.Empty;
public string InstructorName { get; set; } = string.Empty;
public TimeSpan StartTime { get; set; }
public TimeSpan EndTime { get; set; }
public int TotalEnrolled { get; set; }
public int PresentCount { get; set; }
```

#### `CenterManagement.Application/DTOs/Analytics/AnalyticsKpiDto.cs`
```csharp
public int TotalStudents { get; set; }
public int NewEnrollmentsThisMonth { get; set; }
public decimal MonthlyRevenue { get; set; }
public decimal AttendanceRatePercent { get; set; }
public int TotalInstructors { get; set; }
public int TotalGroups { get; set; }
```

#### `CenterManagement.Application/DTOs/Analytics/AttendanceTrendPointDto.cs`
```csharp
public DateTime Date { get; set; }
public decimal AttendanceRatePercent { get; set; }
public int PresentCount { get; set; }
public int TotalCount { get; set; }
```

#### `CenterManagement.Application/DTOs/Analytics/GradeDistributionDto.cs`
```csharp
public string GradeLevelName { get; set; } = string.Empty;
public int StudentCount { get; set; }
public decimal Percentage { get; set; }
```

#### `CenterManagement.Application/DTOs/Analytics/SubjectRevenueDto.cs`
```csharp
public string SubjectName { get; set; } = string.Empty;
public decimal TotalRevenue { get; set; }
public decimal Percentage { get; set; }
```

#### `CenterManagement.Application/DTOs/Analytics/TopTeacherDto.cs`
```csharp
public int InstructorProfileId { get; set; }
public string InstructorName { get; set; } = string.Empty;
public string? ImagePath { get; set; }
public string SubjectName { get; set; } = string.Empty;
public decimal AttendanceRatePercent { get; set; }
public int TotalSessionsConducted { get; set; }
public int GroupCount { get; set; }
```

#### `CenterManagement.Application/DTOs/Analytics/SmartInsightDto.cs`
```csharp
public string Category { get; set; } = string.Empty;   // "Low Attendance" | "High Cancellation Rate"
public string Description { get; set; } = string.Empty;
public string Severity { get; set; } = string.Empty;   // "Warning" | "Critical"
public int AffectedCount { get; set; }
```

---

### Interfaces

#### `CenterManagement.Application/Interfaces/IDashboardService.cs`
```csharp
Task<DashboardKpiDto> GetReceptionistKpisAsync();
Task<List<ActiveSessionDto>> GetActiveSessionsAsync(DateTime now);
Task<List<SessionScheduleDto>> GetTodayScheduleAsync(DateTime today);
```

#### `CenterManagement.Application/Interfaces/IAnalyticsService.cs`
```csharp
Task<AnalyticsKpiDto> GetAnalyticsKpisAsync();
Task<List<AttendanceTrendPointDto>> GetAttendanceTrend30DaysAsync();
Task<List<GradeDistributionDto>> GetStudentDistributionByGradeAsync();
Task<List<SubjectRevenueDto>> GetRevenueBySubjectAsync();
Task<List<TopTeacherDto>> GetTopTeachersAsync(int topN = 5);
Task<List<SmartInsightDto>> GetSmartInsightsAsync();
```

---

### Services

#### `CenterManagement.Application/Services/DashboardService.cs`

**Implements:** `IDashboardService`  
**Inject:** `CenterManagementDbContext`

**`GetReceptionistKpisAsync`:**
```csharp
var today = DateTime.UtcNow.Date;
var nowTime = DateTime.UtcNow.TimeOfDay;
var monthStart = new DateTime(today.Year, today.Month, 1);

var totalStudents = await _db.StudentProfiles.CountAsync();

var activeSessionsNow = await _db.Sessions
    .CountAsync(s =>
        s.SessionDate.Date == today &&
        !s.IsCanceled &&
        s.StartTime <= nowTime &&
        s.EndTime >= nowTime);

var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
var totalRecent = await _db.StudentAttendances
    .CountAsync(a => a.CreatedAt >= sevenDaysAgo);
var presentRecent = await _db.StudentAttendances
    .CountAsync(a => a.CreatedAt >= sevenDaysAgo && a.IsPresent);
var attendanceRate = totalRecent > 0
    ? (decimal)presentRecent / totalRecent * 100 : 0;

var revenueToday = await _db.PaymentTransactions
    .Where(t => t.PaymentDate.Date == today)
    .SumAsync(t => (decimal?)t.Amount) ?? 0;

var newStudentsThisMonth = await _db.StudentProfiles
    .CountAsync(s => s.CreatedAt >= monthStart);
```

**`GetActiveSessionsAsync`:**
```csharp
var nowTime = now.TimeOfDay;
var today = now.Date;

return await _db.Sessions
    .Where(s =>
        s.SessionDate.Date == today &&
        !s.IsCanceled &&
        s.StartTime <= nowTime &&
        s.EndTime >= nowTime)
    .Select(s => new ActiveSessionDto
    {
        SessionId = s.Id,
        CourseTitle = s.Group.Course.Name,
        GroupName = s.Group.Name,
        GradeLevelName = s.Group.Course.GradeLevel.Name,
        InstructorName = s.Group.InstructorProfile.User.FullName,
        StartTime = s.StartTime,
        EndTime = s.EndTime,
        TotalEnrolled = s.Group.Enrollments.Count(e => e.IsActive),
        PresentCount = s.Attendances.Count(a => a.IsPresent)
    })
    .ToListAsync();
```

---

#### `CenterManagement.Application/Services/AnalyticsService.cs`

**Implements:** `IAnalyticsService`  
**Inject:** `CenterManagementDbContext`

**`GetAttendanceTrend30DaysAsync` — single grouped query:**
```csharp
var from = DateTime.UtcNow.Date.AddDays(-29);

var raw = await _db.StudentAttendances
    .Where(a => a.Session.SessionDate >= from)
    .GroupBy(a => a.Session.SessionDate.Date)
    .Select(g => new
    {
        Date = g.Key,
        PresentCount = g.Count(a => a.IsPresent),
        TotalCount = g.Count()
    })
    .OrderBy(x => x.Date)
    .ToListAsync();

return raw.Select(x => new AttendanceTrendPointDto
{
    Date = x.Date,
    PresentCount = x.PresentCount,
    TotalCount = x.TotalCount,
    AttendanceRatePercent = x.TotalCount > 0
        ? (decimal)x.PresentCount / x.TotalCount * 100 : 0
}).ToList();
```

**`GetSmartInsightsAsync` — two separate queries, never load all records:**

**Query 1 — Low attendance students:**
```csharp
var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

var studentsWithLowAttendance = await _db.StudentProfiles
    .Where(sp => _db.StudentAttendances
        .Where(a => a.StudentProfileId == sp.Id && a.CreatedAt >= thirtyDaysAgo)
        .Any()) // only students who have sessions
    .Select(sp => new
    {
        sp.Id,
        Total = _db.StudentAttendances.Count(a => a.StudentProfileId == sp.Id && a.CreatedAt >= thirtyDaysAgo),
        Present = _db.StudentAttendances.Count(a => a.StudentProfileId == sp.Id && a.IsPresent && a.CreatedAt >= thirtyDaysAgo)
    })
    .Where(x => x.Total > 0 && (decimal)x.Present / x.Total < 0.8m)
    .CountAsync();

if (studentsWithLowAttendance > 0)
    insights.Add(new SmartInsightDto
    {
        Category = "Low Attendance",
        Description = $"{studentsWithLowAttendance} student(s) have attended less than 80% of sessions in the last 30 days.",
        Severity = studentsWithLowAttendance > 10 ? "Critical" : "Warning",
        AffectedCount = studentsWithLowAttendance
    });
```

**Query 2 — High cancellation groups:**
```csharp
var highCancellationGroups = await _db.Sessions
    .Where(s => s.IsCanceled && s.SessionDate >= thirtyDaysAgo)
    .GroupBy(s => s.GroupId)
    .Where(g => g.Count() > 2)
    .CountAsync();

if (highCancellationGroups > 0)
    insights.Add(new SmartInsightDto
    {
        Category = "High Cancellation Rate",
        Description = $"{highCancellationGroups} group(s) have had more than 2 canceled sessions in the last 30 days.",
        Severity = "Warning",
        AffectedCount = highCancellationGroups
    });
```

**`GetTopTeachersAsync`:**
```csharp
return await _db.InstructorProfiles
    .Select(ip => new TopTeacherDto
    {
        InstructorProfileId = ip.Id,
        InstructorName = ip.User.FullName,
        ImagePath = ip.User.ImagePath,
        SubjectName = ip.Subject != null ? ip.Subject.Name : "—",
        GroupCount = ip.Groups.Count(g => !g.IsDeleted),
        TotalSessionsConducted = ip.Groups
            .SelectMany(g => g.Sessions)
            .Count(s => !s.IsCanceled),
        AttendanceRatePercent = ip.Groups
            .SelectMany(g => g.Sessions)
            .SelectMany(s => s.Attendances)
            .Any()
            ? ip.Groups.SelectMany(g => g.Sessions)
                .SelectMany(s => s.Attendances)
                .Average(a => a.IsPresent ? 1.0 : 0.0) * 100
            : 0
    })
    .OrderByDescending(x => x.AttendanceRatePercent)
    .Take(topN)
    .ToListAsync();
```

---

#### `CenterManagement.Application/Services/NotificationService.cs`

**REPLACE the Phase 1 stub entirely.**

**Implements:** `INotificationService`  
**Inject:** `CenterManagementDbContext`

```csharp
public async Task SendToUserAsync(string userId, string title, string message)
{
    _db.Notifications.Add(new Notification
    {
        UserId = userId,
        Title = title,
        Message = message,
        SentAt = DateTime.UtcNow,
        IsRead = false,
        CreatedAt = DateTime.UtcNow
    });
    await _db.SaveChangesAsync();
}

public async Task SendToGroupAsync(int groupId, string title, string message)
{
    var userIds = await _db.Enrollments
        .Where(e => e.GroupId == groupId && e.IsActive)
        .Select(e => e.StudentProfile.UserId)
        .Distinct()
        .ToListAsync();

    // Single batch — do NOT call SaveChangesAsync in a loop
    var notifications = userIds.Select(uid => new Notification
    {
        UserId = uid,
        Title = title,
        Message = message,
        SentAt = DateTime.UtcNow,
        IsRead = false,
        CreatedAt = DateTime.UtcNow
    }).ToList();

    await _db.Notifications.AddRangeAsync(notifications);
    await _db.SaveChangesAsync();
}

public async Task<int> GetUnreadCountAsync(string userId)
    => await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

public async Task<List<NotificationDto>> GetNotificationsAsync(
    string userId, int page, int pageSize)
    => await _db.Notifications
        .Where(n => n.UserId == userId)
        .OrderByDescending(n => n.SentAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(n => new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            IsRead = n.IsRead,
            SentAt = n.SentAt
        })
        .ToListAsync();

public async Task MarkReadAsync(int notificationId, string userId)
{
    var n = await _db.Notifications
        .FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId);
    if (n is null) return;
    n.IsRead = true;
    n.UpdatedAt = DateTime.UtcNow;
    await _db.SaveChangesAsync();
}

public async Task MarkAllReadAsync(string userId)
{
    await _db.Notifications
        .Where(n => n.UserId == userId && !n.IsRead)
        .ExecuteUpdateAsync(s => s
            .SetProperty(n => n.IsRead, true)
            .SetProperty(n => n.UpdatedAt, DateTime.UtcNow));
}
```

> `ExecuteUpdateAsync` requires EF Core 7+. If on EF Core 6, replace with: load all unread → set `IsRead=true` in a loop → single `SaveChangesAsync()`.

---

## Files to MODIFY

### `CenterManagement.Application/DependencyInjection/ApplicationServiceRegistration.cs`

Add:
```csharp
services.AddScoped<IDashboardService, DashboardService>();
services.AddScoped<IAnalyticsService, AnalyticsService>();
services.AddMemoryCache(); // optional — for KPI result caching
// NotificationService is already registered from Phase 1 — the implementation class file is replaced in-place
```

---

## Web Layer

### `CenterManagement.Web/Controllers/DashboardController.cs`

**REPLACE the Phase 1 stub entirely.**

`[Authorize(Roles = "Admin,Instructor")]`  
**Inject:** `IDashboardService`

| Method | Route | Returns |
|--------|-------|---------|
| `GET` | `/Dashboard` | `View()` |
| `GET` | `/Dashboard/Kpis` | `Json(await GetReceptionistKpisAsync())` |
| `GET` | `/Dashboard/ActiveSessions` | `Json(await GetActiveSessionsAsync(DateTime.UtcNow))` |
| `GET` | `/Dashboard/TodaySchedule` | `Json(await GetTodayScheduleAsync(DateTime.Today))` |

---

### `CenterManagement.Web/Controllers/AnalyticsController.cs`

`[Authorize(Roles = "Admin")]`  
**Inject:** `IAnalyticsService`

| Method | Route | Returns |
|--------|-------|---------|
| `GET` | `/Analytics` | `View()` |
| `GET` | `/Analytics/Kpis` | `Json(await GetAnalyticsKpisAsync())` |
| `GET` | `/Analytics/AttendanceTrend` | `Json(await GetAttendanceTrend30DaysAsync())` |
| `GET` | `/Analytics/StudentDistribution` | `Json(await GetStudentDistributionByGradeAsync())` |
| `GET` | `/Analytics/RevenueBySubject` | `Json(await GetRevenueBySubjectAsync())` |
| `GET` | `/Analytics/TopTeachers` | `Json(await GetTopTeachersAsync(5))` |
| `GET` | `/Analytics/SmartInsights` | `Json(await GetSmartInsightsAsync())` |

---

### `CenterManagement.Web/Controllers/NotificationController.cs`

`[Authorize]`  
**Inject:** `INotificationService`

```csharp
private string CurrentUserId =>
    User.FindFirstValue(ClaimTypes.NameIdentifier)!;

[HttpGet] public async Task<IActionResult> GetUnread()
    => Json(new { count = await _notificationService.GetUnreadCountAsync(CurrentUserId) });

[HttpGet] public async Task<IActionResult> List(int page = 1)
    => Json(await _notificationService.GetNotificationsAsync(CurrentUserId, page, 10));

[HttpPost] public async Task<IActionResult> MarkRead(int id)
{
    await _notificationService.MarkReadAsync(id, CurrentUserId);
    return Json(new { success = true });
}

[HttpPost] public async Task<IActionResult> MarkAllRead()
{
    await _notificationService.MarkAllReadAsync(CurrentUserId);
    return Json(new { success = true });
}
```

---

### `CenterManagement.Web/Controllers/AuditLogController.cs`

`[Authorize(Roles = "Admin")]`  
**Inject:** `CenterManagementDbContext` directly (read-only; acceptable for simple paginated display)

```csharp
public async Task<IActionResult> Index(
    int page = 1,
    string? userId = null,
    string? entityName = null,
    DateTime? from = null,
    DateTime? to = null)
{
    var query = _db.AuditLogs
        .Include(a => a.User)
        .AsNoTracking();

    if (!string.IsNullOrWhiteSpace(userId))
        query = query.Where(a => a.UserId == userId);
    if (!string.IsNullOrWhiteSpace(entityName))
        query = query.Where(a => a.EntityName.Contains(entityName));
    if (from.HasValue)
        query = query.Where(a => a.ActionDate >= from.Value);
    if (to.HasValue)
        query = query.Where(a => a.ActionDate <= to.Value);

    var total = await query.CountAsync();
    var items = await query
        .OrderByDescending(a => a.ActionDate)
        .Skip((page - 1) * 20)
        .Take(20)
        .ToListAsync();

    return View(new PagedResult<AuditLog>
    {
        Items = items, TotalCount = total, Page = page, PageSize = 20
    });
}
```

---

### Views

#### `CenterManagement.Web/Views/Dashboard/Index.cshtml`

**REPLACE Phase 1 stub entirely.**  
`@model object` · Layout `"_Layout"`

Scaffolded from `receptioneestDash.html`. All data loaded via fetch on `DOMContentLoaded`.

**KPI cards** (copy exact Tailwind classes from `receptioneestDash.html`):
```html
<div id="kpi-total-students">—</div>  <!-- filled by JS -->
<div id="kpi-active-sessions">—</div>
<div id="kpi-attendance-rate">—</div>
<div id="kpi-revenue-today">—</div>
```

**Quick Actions (exact buttons from `receptioneestDash.html`):**
```html
<a asp-controller="Session" asp-action="Index">
    <span class="material-symbols-outlined">qr_code_scanner</span> Quick QR Scan
</a>
<a asp-controller="Student" asp-action="Create">
    <span class="material-symbols-outlined">person_add</span> New Student
</a>
<a asp-controller="Payment" asp-action="Index">
    <span class="material-symbols-outlined">payments</span> Record Payment
</a>
```

**Active Sessions grid:** `<div id="active-sessions-grid">` populated via fetch.  
Each card rendered by JS with: course name, `ACTIVE` badge, instructor, group/grade, time, "Manage Attendance" link → `/Session/Detail/{sessionId}`.

**JavaScript:**
```js
async function loadKpis() {
    const res = await fetch('/Dashboard/Kpis');
    const d = await res.json();
    document.getElementById('kpi-total-students').textContent = d.totalStudents;
    document.getElementById('kpi-active-sessions').textContent = d.activeSessionsNow;
    document.getElementById('kpi-attendance-rate').textContent = d.attendanceRateLast7Days.toFixed(1) + '%';
    document.getElementById('kpi-revenue-today').textContent = d.revenueTodayAmount.toFixed(2);
}

async function loadActiveSessions() {
    const res = await fetch('/Dashboard/ActiveSessions');
    const sessions = await res.json();
    const grid = document.getElementById('active-sessions-grid');
    grid.innerHTML = sessions.length === 0
        ? '<p class="text-muted">No active sessions right now.</p>'
        : sessions.map(s => `
            <div class="bg-white border border-outline-variant p-5 rounded-xl card-shadow">
                <h4 class="font-headline-sm text-headline-sm">${s.courseTitle}</h4>
                <span class="badge-green text-xs">ACTIVE</span>
                <p class="text-body-sm text-muted">${s.instructorName} · ${s.groupName} · ${s.gradeLevelName}</p>
                <p class="text-body-sm">${formatTime(s.startTime)} – ${formatTime(s.endTime)}</p>
                <p class="text-body-sm">${s.presentCount} / ${s.totalEnrolled} present</p>
                <a href="/Session/Detail/${s.sessionId}"
                   class="block mt-3 py-2 bg-primary-container text-white text-center rounded-lg text-label-sm">
                    Manage Attendance
                </a>
            </div>`
        ).join('');
}

function formatTime(timespan) {
    // TimeSpan comes as "HH:MM:SS" string from JSON
    const parts = timespan.split(':');
    const h = parseInt(parts[0]);
    const m = parts[1];
    return `${h % 12 || 12}:${m} ${h < 12 ? 'AM' : 'PM'}`;
}

document.addEventListener('DOMContentLoaded', () => {
    loadKpis();
    loadActiveSessions();
    setInterval(loadActiveSessions, 60000);
});
```

---

#### `CenterManagement.Web/Views/Analytics/Index.cshtml`

`@model object` · Layout `"_Layout"`  
Scaffolded from `insights.html`.

**Add Chart.js CDN** (in `@section Scripts`):
```html
<script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/4.4.0/chart.umd.min.js"></script>
```

**Canvas elements** (replace SVG placeholders from `insights.html`):
```html
<canvas id="trend-chart" height="120"></canvas>
<canvas id="distribution-chart" height="200"></canvas>
<canvas id="revenue-chart" height="120"></canvas>
```

**Top Teachers table:**
```html
<tbody id="top-teachers-tbody"></tbody>
```

**Smart Insights container:**
```html
<div id="insights-container"></div>
```

**JavaScript — chart rendering:**
```js
let trendChart, distChart, revChart;

async function loadAnalytics() {
    // KPIs
    const kpiRes = await fetch('/Analytics/Kpis');
    const kpi = await kpiRes.json();
    document.getElementById('anl-students').textContent = kpi.totalStudents;
    document.getElementById('anl-revenue').textContent = kpi.monthlyRevenue.toFixed(2);
    document.getElementById('anl-attendance').textContent = kpi.attendanceRatePercent.toFixed(1) + '%';

    // Attendance Trend
    const trendRes = await fetch('/Analytics/AttendanceTrend');
    const trend = await trendRes.json();
    if (trendChart) trendChart.destroy();
    trendChart = new Chart(document.getElementById('trend-chart'), {
        type: 'line',
        data: {
            labels: trend.map(t => new Date(t.date).toLocaleDateString()),
            datasets: [{ label: 'Attendance %', data: trend.map(t => t.attendanceRatePercent),
                borderColor: '#2563eb', backgroundColor: 'rgba(37,99,235,0.1)', fill: true, tension: 0.4 }]
        },
        options: { responsive: true, plugins: { legend: { display: false } },
                   scales: { y: { min: 0, max: 100 } } }
    });

    // Grade Distribution
    const distRes = await fetch('/Analytics/StudentDistribution');
    const dist = await distRes.json();
    if (distChart) distChart.destroy();
    distChart = new Chart(document.getElementById('distribution-chart'), {
        type: 'doughnut',
        data: {
            labels: dist.map(d => d.gradeLevelName),
            datasets: [{ data: dist.map(d => d.studentCount),
                backgroundColor: ['#2563eb','#0891b2','#7c3aed','#ea580c','#16a34a'] }]
        }
    });

    // Revenue by Subject
    const revRes = await fetch('/Analytics/RevenueBySubject');
    const rev = await revRes.json();
    if (revChart) revChart.destroy();
    revChart = new Chart(document.getElementById('revenue-chart'), {
        type: 'bar',
        data: {
            labels: rev.map(r => r.subjectName),
            datasets: [{ label: 'Revenue', data: rev.map(r => r.totalRevenue),
                backgroundColor: '#2563eb' }]
        },
        options: { responsive: true, plugins: { legend: { display: false } } }
    });

    // Top Teachers
    const teachRes = await fetch('/Analytics/TopTeachers');
    const teachers = await teachRes.json();
    document.getElementById('top-teachers-tbody').innerHTML = teachers.map((t, i) => `
        <tr>
            <td>${i + 1}</td>
            <td>${t.instructorName}</td>
            <td>${t.subjectName}</td>
            <td>${t.groupCount}</td>
            <td>${t.totalSessionsConducted}</td>
            <td>${t.attendanceRatePercent.toFixed(1)}%</td>
        </tr>`
    ).join('');

    // Smart Insights
    const insRes = await fetch('/Analytics/SmartInsights');
    const insights = await insRes.json();
    const container = document.getElementById('insights-container');
    if (insights.length === 0) {
        container.innerHTML = '<p class="text-muted text-body-sm">No insights at this time. All metrics look healthy.</p>';
    } else {
        container.innerHTML = insights.map(ins => `
            <div class="p-4 rounded-xl border ${ins.severity === 'Critical' ? 'border-error bg-error-container text-on-error-container' : 'border-tertiary-container bg-tertiary-container text-on-tertiary-container'} mb-3">
                <p class="font-label-md">${ins.category}</p>
                <p class="text-body-sm mt-1">${ins.description}</p>
            </div>`
        ).join('');
    }
}

document.addEventListener('DOMContentLoaded', loadAnalytics);
```

---

#### `CenterManagement.Web/Views/AuditLog/Index.cshtml`

`@model PagedResult<AuditLog>` · Layout `"_Layout"`

Filter form (`method="get"`): User ID input, Entity Name input, Date From/To pickers, Apply button.

Table columns: ActionDate (formatted), User FullName, Action, EntityName, EntityId, OldValues (`.Substring(0,50)` if long), NewValues (same).

Pagination: `<a asp-route-page="@p">` for each page.

---

## Files to MODIFY

### `CenterManagement.Web/Views/Shared/_Layout.cshtml`

Add notification dropdown below the bell button:

```html
<!-- Notification bell + dropdown -->
<div class="relative">
    <button id="notif-bell" onclick="toggleNotifDropdown()"
            class="p-2 text-on-surface-variant hover:bg-surface-container-low rounded-full transition-colors relative">
        <span class="material-symbols-outlined">notifications</span>
        <span id="notif-count"
              class="hidden absolute -top-1 -right-1 bg-error text-white text-xs rounded-full w-5 h-5 flex items-center justify-center">
        </span>
    </button>
    <div id="notif-dropdown"
         class="hidden absolute right-0 top-full mt-2 w-80 bg-white rounded-xl card-shadow border border-outline-variant z-50 max-h-96 overflow-y-auto">
        <div class="flex items-center justify-between p-3 border-b border-outline-variant">
            <span class="font-label-md text-on-surface">Notifications</span>
            <button onclick="markAllRead()" class="text-primary text-label-sm hover:underline">Mark all read</button>
        </div>
        <ul id="notif-list" class="divide-y divide-outline-variant"></ul>
    </div>
</div>
```

Add JavaScript (in the shared layout `<script>` block):
```js
async function toggleNotifDropdown() {
    const dropdown = document.getElementById('notif-dropdown');
    const isHidden = dropdown.classList.contains('hidden');
    dropdown.classList.toggle('hidden');
    if (isHidden) await fetchNotificationList();
}

async function fetchNotificationList() {
    const res = await fetch('/Notification/List?page=1');
    const items = await res.json();
    const list = document.getElementById('notif-list');
    list.innerHTML = items.length === 0
        ? '<li class="p-4 text-body-sm text-muted">No notifications</li>'
        : items.map(n => `
            <li class="p-3 ${n.isRead ? '' : 'bg-surface-container-low'} hover:bg-surface-container transition-colors cursor-pointer"
                onclick="markRead(${n.id}, this)">
                <p class="font-label-sm text-on-surface">${n.title}</p>
                <p class="text-body-sm text-on-surface-variant line-clamp-2">${n.message}</p>
                <p class="text-label-sm text-muted mt-1">${new Date(n.sentAt).toLocaleString()}</p>
            </li>`
        ).join('');
}

async function markRead(id, el) {
    await fetch(`/Notification/MarkRead/${id}`, {
        method: 'POST',
        headers: { 'X-CSRF-TOKEN': getAntiForgeryToken() }
    });
    el.classList.remove('bg-surface-container-low');
    pollNotifications();
}

async function markAllRead() {
    await fetch('/Notification/MarkAllRead', {
        method: 'POST',
        headers: { 'X-CSRF-TOKEN': getAntiForgeryToken() }
    });
    document.getElementById('notif-dropdown').classList.add('hidden');
    pollNotifications();
}
```

---

### `CenterManagement.Web/wwwroot/js/site.js`

Replace the notification poll stub from Phase 1 with the full implementation (already shown in `_Layout.cshtml` modifications above — `pollNotifications()` is already defined in Phase 1; it only needs the full `fetchNotificationList`, `markRead`, and `markAllRead` functions which are added to `_Layout.cshtml` directly).

---

## Completion Checklist

- [ ] `GET /Dashboard` → KPI cards populate with real DB values within 2 seconds
- [ ] Active Sessions grid reflects sessions happening right now (verify with a live test session)
- [ ] Grid refreshes every 60 seconds automatically without page reload
- [ ] Session cancellation (Phase 3) → `Notification` record created per enrolled student; bell badge increments within 60 seconds
- [ ] Clicking bell → dropdown opens with notification list
- [ ] Clicking a notification → removes unread style, badge decrements
- [ ] "Mark all read" → all notifications marked read, badge hidden
- [ ] `GET /Analytics` → all 3 Chart.js charts render with real data points (verify with non-empty DB)
- [ ] Smart Insights shows alert card when a student's attendance rate is below 80%
- [ ] Smart Insights shows "No insights" message when all metrics are healthy
- [ ] Top Teachers table ranks instructors by attendance rate descending
- [ ] `GET /AuditLog` → paginated table showing all system actions from all phases
- [ ] `NotificationService.SendToGroupAsync` creates N records (one per enrolled student) in a **single** `SaveChangesAsync()` — verify with SQL profiler or EF Core logging
