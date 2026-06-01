# Phase 4 — Attendance & QR Scanning

**Developer:** Dev 4  
**HTML Sources:** `sessionDeatails.html` — QR scanner panel + attendance table sections only  
**Hard dependency:** Phase 3 (Sessions and Groups must exist). Phase 2 (StudentProfiles must exist).

---

## Constraints (Read Before Writing Any Code)

- The **Session Resolution Algorithm** is the most critical logic in the system — read and implement it exactly as specified below
- **Every scan attempt** (valid or invalid) MUST write a `QrCodeLog` record — this is non-negotiable for audit purposes
- **Duplicate scans** are rejected by the unique index on `(StudentProfileId, SessionId)` in `StudentAttendances` — catch `DbUpdateException` and return a friendly message; never let the constraint exception bubble to the user
- **Lateness** is computed by comparing `ScanTime.TimeOfDay` to `session.StartTime + grace period (15 minutes from appsettings)` — the grace period value comes from `appsettings.json` key `AppSettings:AttendanceLateGraceMinutes`
- **No direct DB queries in `AttendanceController`** — all logic goes through `IAttendanceService`
- **`GetSessionForStudentAtTimeAsync`** must be a **server-side EF Core query** — never load all sessions into memory and filter in C#
- **SOLID:** `QrService` handles encoding/decoding only; `AttendanceService` handles all business logic
- All `DateTime` → UTC

---

## Files to CREATE

### Install NuGet Package

In `CenterManagement.Web/CenterManagement.Web.csproj`, add:
```xml
<PackageReference Include="QRCoder" Version="1.4.3" />
```
Run `dotnet restore`.

---

### DTOs

#### `CenterManagement.Application/DTOs/Attendance/ScanResultDto.cs`
```csharp
public bool Success { get; set; }
public string? ErrorMessage { get; set; }
public string? StudentName { get; set; }
public string? StudentImagePath { get; set; }
public string? SessionTitle { get; set; }
public string? GroupName { get; set; }
public string? InstructorName { get; set; }
public string? GradeLevelName { get; set; }
public bool IsLate { get; set; }
public DateTime? ScanTime { get; set; }
public int? AttendanceId { get; set; }
```

#### `CenterManagement.Application/DTOs/Attendance/AttendanceListItemDto.cs`
```csharp
public int AttendanceId { get; set; }
public int StudentProfileId { get; set; }
public string StudentName { get; set; } = string.Empty;
public string? StudentImagePath { get; set; }
public bool IsPresent { get; set; }
public bool IsLate { get; set; }
public DateTime ScanTime { get; set; }
```

#### `CenterManagement.Application/DTOs/Attendance/AttendanceSessionSummaryDto.cs`
```csharp
public int SessionId { get; set; }
public int TotalEnrolled { get; set; }
public int PresentCount { get; set; }
public int AbsentCount { get; set; }
public int LateCount { get; set; }
public decimal AttendanceRatePercent { get; set; }
```

#### `CenterManagement.Application/DTOs/Attendance/ManualMarkDto.cs`
```csharp
public int StudentProfileId { get; set; }
public int SessionId { get; set; }
public bool IsPresent { get; set; }
public bool IsLate { get; set; }
```

---

### Interfaces

#### `CenterManagement.Application/Interfaces/IAttendanceService.cs`
```csharp
Task<ScanResultDto> ProcessScanAsync(string qrCode, DateTime scanTime);
Task MarkManuallyAsync(ManualMarkDto dto, string adminId);
Task<AttendanceSessionSummaryDto> GetSessionSummaryAsync(int sessionId);
Task<PagedResult<AttendanceListItemDto>> GetSessionAttendanceListAsync(
    int sessionId, int page, int pageSize, string? search);
Task<List<StudentAttendanceDto>> GetStudentAttendanceHistoryAsync(
    int studentProfileId, int? groupId, DateTime? from, DateTime? to);
Task<decimal> GetStudentAttendanceRateAsync(int studentProfileId);
Task<Session?> GetSessionForStudentAtTimeAsync(int studentProfileId, DateTime scanTime);
```

#### `CenterManagement.Application/Interfaces/IQrService.cs`
```csharp
string GenerateStudentQrCode(string userId);
string? DecodeQrCode(string qrCode);
byte[] GenerateQrCodeImage(string content);
```

---

### Services

#### `CenterManagement.Application/Services/QrService.cs`

**Implements:** `IQrService`

```csharp
public string GenerateStudentQrCode(string userId)
    => Convert.ToBase64String(Encoding.UTF8.GetBytes(userId));

public string? DecodeQrCode(string qrCode)
{
    try { return Encoding.UTF8.GetString(Convert.FromBase64String(qrCode)); }
    catch { return null; }
}

public byte[] GenerateQrCodeImage(string content)
{
    using var qrGenerator = new QRCodeGenerator();
    var qrData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
    var qrCode = new PngByteQRCode(qrData);
    return qrCode.GetGraphic(10);
}
```

---

#### `CenterManagement.Application/Services/AttendanceService.cs`

**Implements:** `IAttendanceService`  
**Inject:** `CenterManagementDbContext`, `IQrService`, `IAuditLogService`, `IConfiguration`

**`GetSessionForStudentAtTimeAsync` — CRITICAL. Must be server-side EF Core:**

```csharp
public async Task<Session?> GetSessionForStudentAtTimeAsync(int studentProfileId, DateTime scanTime)
{
    var scanTimeOfDay = scanTime.TimeOfDay;
    var scanDate = scanTime.Date;
    var graceWindow = TimeSpan.FromMinutes(30); // scan-out grace

    return await _db.Enrollments
        .Where(e =>
            e.StudentProfileId == studentProfileId &&
            e.IsActive &&
            !e.IsDeleted)
        .SelectMany(e => e.Group.Sessions)
        .Where(s =>
            !s.IsDeleted &&
            !s.IsCanceled &&
            s.SessionDate.Date == scanDate &&
            s.StartTime <= scanTimeOfDay &&
            s.EndTime.Add(graceWindow) >= scanTimeOfDay)
        .OrderByDescending(s => s.StartTime)
        .FirstOrDefaultAsync();
}
```

> The `OrderByDescending(StartTime)` tiebreaker handles Scenario 1: if a student has two sessions on the same day and scans at 3 PM, the session with the latest `StartTime <= 3 PM` wins.

---

**`ProcessScanAsync` — 9-step flow (implement exactly in this order):**

```
Step 1: Decode QR → userId via IQrService.DecodeQrCode
        If null → log QrCodeLog(QrCode=raw, ScanTime, UserId="INVALID") → return { Success=false, ErrorMessage="Invalid QR code" }

Step 2: Find StudentProfile by UserId
        If null → log → return { Success=false, ErrorMessage="Student not found" }

Step 3: Call GetSessionForStudentAtTimeAsync(studentProfile.Id, scanTime)
        If null → log → return { Success=false, ErrorMessage="No active session found at this time" }

Step 4: Load session with Group, Course.Subject, GradeLevel, InstructorProfile.User (needed for response)

Step 5: Check existing StudentAttendance for same StudentProfileId + SessionId
        If exists → log → return { Success=true, StudentName=..., IsLate=existing.IsLate, ErrorMessage="Already scanned" }
        (return Success=true here so the UI still shows the student info)

Step 6: Compute IsLate:
        var graceMinutes = _config.GetValue<int>("AppSettings:AttendanceLateGraceMinutes");
        var isLate = scanTime.TimeOfDay > session.StartTime.Add(TimeSpan.FromMinutes(graceMinutes));

Step 7: Create StudentAttendance { StudentProfileId, SessionId, IsPresent=true, IsLate, ScanTime }
        _db.StudentAttendances.Add(attendance);
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true)
        { return { Success=true, ErrorMessage="Already scanned" }; }

Step 8: Write QrCodeLog { QrCode=rawQrCode, ScanTime, UserId=studentProfile.UserId }
        await _db.SaveChangesAsync();

Step 9: Return ScanResultDto { Success=true, StudentName, SessionTitle, GroupName, InstructorName, GradeLevelName, IsLate, ScanTime, AttendanceId=attendance.Id }
```

Wrap entire method in `try/catch(Exception ex)` → write to `IAuditLogService` → return `{ Success=false, ErrorMessage="System error" }`.

---

**`GetSessionSummaryAsync`:**
```csharp
var session = await _db.Sessions
    .Include(s => s.Group).ThenInclude(g => g.Enrollments)
    .Include(s => s.Attendances)
    .AsNoTracking()
    .FirstOrDefaultAsync(s => s.Id == sessionId);

var totalEnrolled = session!.Group.Enrollments.Count(e => e.IsActive && !e.IsDeleted);
var present = session.Attendances.Count(a => a.IsPresent && !a.IsDeleted);
var late = session.Attendances.Count(a => a.IsLate && !a.IsDeleted);
var absent = totalEnrolled - present;
var rate = totalEnrolled > 0 ? (decimal)present / totalEnrolled * 100 : 0;
```

---

**`MarkManuallyAsync`:**
1. Check if `StudentAttendance` already exists for this `StudentProfileId + SessionId`
2. If exists: update `IsPresent`, `IsLate`, `UpdatedAt`
3. If not: create new `StudentAttendance`
4. `SaveChangesAsync()`
5. Write audit `action="ManualAttendanceMark"`

---

**`GetStudentAttendanceRateAsync`:**
```csharp
var total = await _db.StudentAttendances
    .CountAsync(a => a.StudentProfileId == studentProfileId && !a.IsDeleted);
if (total == 0) return 0;
var present = await _db.StudentAttendances
    .CountAsync(a => a.StudentProfileId == studentProfileId && a.IsPresent && !a.IsDeleted);
return (decimal)present / total * 100;
```

---

## Files to MODIFY

### `CenterManagement.Application/DependencyInjection/ApplicationServiceRegistration.cs`

Add:
```csharp
services.AddScoped<IAttendanceService, AttendanceService>();
services.AddScoped<IQrService, QrService>();
```

---

## Web Layer

### `CenterManagement.Web/Controllers/AttendanceController.cs`

`[Authorize(Roles = "Admin,Instructor")]`  
**Inject:** `IAttendanceService`, `IQrService`

| Method | Route | Action |
|--------|-------|--------|
| `POST` | `/Attendance/Scan` | `Scan([FromBody] ScanRequest req)` → `Json(await ProcessScanAsync(req.QrCode, DateTime.UtcNow))` |
| `POST` | `/Attendance/MarkManual` | `MarkManual([FromBody] ManualMarkDto dto)` → Admin only inline check → `Json({success})` |
| `GET` | `/Attendance/SessionSummary/{sessionId}` | `SessionSummary(int sessionId)` → `Json(summary)` |
| `GET` | `/Attendance/SessionList/{sessionId}` | `SessionList(int sessionId, int page=1, string? q=null)` → `Json(pagedList)` |
| `GET` | `/Attendance/StudentHistory/{studentProfileId}` | `[Authorize(Roles="Admin")]` → `Json(list)` |
| `GET` | `/Attendance/GetQr/{userId}` | `[Authorize(Roles="Admin")]` → `File(bytes, "image/png", $"qr_{userId}.png")` |

**`ScanRequest` inline model (inside controller file):**
```csharp
public record ScanRequest(string QrCode);
```

All JSON endpoints wrap in try/catch → return `Json({success=false, error=ex.Message})` on failure.

---

### `CenterManagement.Web/Views/Session/Detail.cshtml`

**MODIFY** — replace `<div id="phase4-attendance-section">` placeholder with the full QR + attendance HTML from `sessionDeatails.html`.

**QR Panel section:**
- `qr_code_scanner` Material Symbol icon
- Heading "Ready to Scan"
- `<input id="qr-input" type="text" class="..." placeholder="Type or scan QR code...">` — on `Enter` keydown OR button click calls `processScan()`
- `<button id="scan-btn" onclick="processScan()">Process Scan</button>`
- Error alert: `<div id="scan-error-alert" class="hidden bg-error-container text-on-error-container ...">` with `<span id="scan-error-msg"></span>`

**Attendance Summary row:**
```html
<div id="attend-summary" class="flex gap-4">
    <span class="pill-green">Present: <span id="sum-present">0</span></span>
    <span class="pill-orange">Late: <span id="sum-late">0</span></span>
    <span class="pill-red">Absent: <span id="sum-absent">0</span></span>
</div>
```

**Attendance Table:**
```html
<table>
  <thead>
    <tr><th>Student</th><th>Status</th><th>Check-In</th><th>Actions</th></tr>
  </thead>
  <tbody id="attend-tbody"></tbody>
</table>
```

**JavaScript block at bottom of the view:**
```js
const SESSION_ID = @Model.SessionDetail.SessionId;

async function processScan() {
    const qrCode = document.getElementById('qr-input').value.trim();
    if (!qrCode) return;
    const res = await fetch('/Attendance/Scan', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-CSRF-TOKEN': getAntiForgeryToken()
        },
        body: JSON.stringify({ qrCode })
    });
    const data = await res.json();
    document.getElementById('qr-input').value = '';
    if (!data.success && data.errorMessage && data.errorMessage !== 'Already scanned') {
        showError(data.errorMessage);
        return;
    }
    prependAttendanceRow(data);
    fetchSummary();
}

function showError(msg) {
    const alert = document.getElementById('scan-error-alert');
    document.getElementById('scan-error-msg').textContent = msg;
    alert.classList.remove('hidden');
    setTimeout(() => alert.classList.add('hidden'), 4000);
}

function prependAttendanceRow(data) {
    const tbody = document.getElementById('attend-tbody');
    const row = document.createElement('tr');
    row.innerHTML = `
        <td>${data.studentName ?? '—'}</td>
        <td><span class="${data.isLate ? 'badge-orange' : 'badge-green'}">${data.isLate ? 'Late' : 'Present'}</span></td>
        <td>${data.scanTime ? new Date(data.scanTime).toLocaleTimeString() : '—'}</td>
        <td><!-- manual mark buttons --></td>
    `;
    tbody.prepend(row);
}

async function fetchSummary() {
    const res = await fetch(`/Attendance/SessionSummary/${SESSION_ID}`);
    const data = await res.json();
    document.getElementById('sum-present').textContent = data.presentCount;
    document.getElementById('sum-late').textContent = data.lateCount;
    document.getElementById('sum-absent').textContent = data.absentCount;
}

async function fetchAttendanceList() {
    const res = await fetch(`/Attendance/SessionList/${SESSION_ID}?page=1`);
    const data = await res.json();
    const tbody = document.getElementById('attend-tbody');
    tbody.innerHTML = '';
    (data.items ?? []).forEach(a => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${a.studentName}</td>
            <td><span class="${a.isLate ? 'badge-orange' : (a.isPresent ? 'badge-green' : 'badge-red')}">${a.isLate ? 'Late' : (a.isPresent ? 'Present' : 'Absent')}</span></td>
            <td>${new Date(a.scanTime).toLocaleTimeString()}</td>
            <td>
                <button onclick="markManual(${a.studentProfileId}, ${SESSION_ID}, true, false)">✓</button>
                <button onclick="markManual(${a.studentProfileId}, ${SESSION_ID}, false, false)">✗</button>
            </td>
        `;
        tbody.appendChild(row);
    });
}

async function markManual(studentProfileId, sessionId, isPresent, isLate) {
    await fetch('/Attendance/MarkManual', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': getAntiForgeryToken() },
        body: JSON.stringify({ studentProfileId, sessionId, isPresent, isLate })
    });
    fetchAttendanceList();
    fetchSummary();
}

document.addEventListener('DOMContentLoaded', () => {
    fetchAttendanceList();
    fetchSummary();
    setInterval(fetchSummary, 30000);
    // Auto-focus QR input for hands-free scanning
    document.getElementById('qr-input')?.focus();
});
```

---

## Completion Checklist

- [ ] `POST /Attendance/Scan` with valid base64-encoded student userId → `Success=true`, `StudentName` populated, `StudentAttendance` created in DB
- [ ] Second scan with same QR for same session → `Success=true`, `ErrorMessage="Already scanned"`, NO duplicate row in `StudentAttendances`
- [ ] DB unique constraint test: attempt direct INSERT of duplicate `(StudentProfileId, SessionId)` → caught as `DbUpdateException`, not 500 error
- [ ] Scan with invalid base64 → `Success=false`, `ErrorMessage="Invalid QR code"`, `QrCodeLog` written
- [ ] Scan outside any session time window → `Success=false`, `ErrorMessage="No active session found at this time"`
- [ ] Scan 20 minutes after `StartTime` → `IsLate=true` in `StudentAttendance`
- [ ] Scan within 15 minutes of `StartTime` → `IsLate=false`
- [ ] `GET /Attendance/GetQr/{userId}` → returns PNG image
- [ ] Session Detail page: QR panel renders, table loads on page load, summary chips update after scan
- [ ] Auto-focus on `#qr-input` on page load — receptionist can scan without clicking
- [ ] Every scan (valid or invalid) writes a `QrCodeLog` record
