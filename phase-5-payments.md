# Phase 5 — Payments & Financial Module

**Developer:** Dev 5  
**HTML Sources:** `payments-financial.html`  
**Hard dependencies:** Phase 2 (StudentProfiles must exist). Phase 3 (Courses must exist with a `Price`).

---

## Constraints (Read Before Writing Any Code)

- **Never allow overpayment:** `RecordPaymentAsync` must throw if `dto.Amount > scp.RemainingAmount`
- **Balance must always recompute from scratch:** `RemainingAmount = RequiredAmount - PaidAmount` — never increment/decrement in place. This prevents balance drift from concurrent writes
- **Atomic payment recording:** The `StudentCoursePayment` update AND the `PaymentTransaction` creation MUST happen in the same `SaveChangesAsync()` call — never save them separately
- **No EF Core `.ToList()` before aggregation:** All KPI calculations (`Sum`, `Count`) must use EF Core async aggregation methods that translate to SQL — never load records into memory to aggregate
- **Soft delete:** `StudentCoursePayment` is never hard-deleted. Setting `IsDeleted=true` is the only removal path, and the global query filter excludes it automatically
- **No direct DB queries in `PaymentController`** — all logic goes through `IPaymentService`
- **SOLID:** `PaymentService` has one responsibility — payment business logic. CSV export is one method on that service, not a separate class for this scale
- All `DateTime` → UTC

---

## Files to CREATE

### DTOs

#### `CenterManagement.Application/DTOs/Payment/RecordPaymentDto.cs`
```csharp
public int StudentCoursePaymentId { get; set; }
[Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
public decimal Amount { get; set; }
[Required]
public string PaymentMethod { get; set; } = string.Empty; // "Cash" | "Card" | "Transfer"
public string? Notes { get; set; }
```

#### `CenterManagement.Application/DTOs/Payment/CreateSessionPaymentDto.cs`
```csharp
public int StudentProfileId { get; set; }
public int SessionId { get; set; }
[Range(0.01, double.MaxValue)]
public decimal Amount { get; set; }
[Required]
public string PaymentMethod { get; set; } = string.Empty;
public string? Notes { get; set; }
```

#### `CenterManagement.Application/DTOs/Payment/PaymentKpiDto.cs`
```csharp
public decimal TotalDueToday { get; set; }
public decimal RevenueThisMonth { get; set; }
public decimal TotalRevenueAllTime { get; set; }
public int OutstandingStudentsCount { get; set; }
public decimal CollectionRatePercent { get; set; }
public int PaidStudentsCount { get; set; }
```

#### `CenterManagement.Application/DTOs/Payment/TransactionListItemDto.cs`
```csharp
public int TransactionId { get; set; }
public DateTime PaymentDate { get; set; }
public string StudentName { get; set; } = string.Empty;
public string? StudentImagePath { get; set; }
public string CourseName { get; set; } = string.Empty;
public decimal Amount { get; set; }
public string PaymentMethod { get; set; } = string.Empty;
public bool IsPaid { get; set; }
public decimal RemainingAfterPayment { get; set; }
```

#### `CenterManagement.Application/DTOs/Payment/StudentFinancialSummaryDto.cs`
```csharp
public int StudentProfileId { get; set; }
public string StudentName { get; set; } = string.Empty;
public decimal TotalRequired { get; set; }
public decimal TotalPaid { get; set; }
public decimal TotalRemaining { get; set; }
public bool IsFullyPaid { get; set; }
public List<StudentCoursePaymentSummaryDto> CoursePayments { get; set; } = new();
public List<SessionPaymentDto> SessionPayments { get; set; } = new();
```

#### `CenterManagement.Application/DTOs/Payment/SessionPaymentDto.cs`
```csharp
public int SessionPaymentId { get; set; }
public string SessionTitle { get; set; } = string.Empty;
public decimal Amount { get; set; }
public DateTime PaymentDate { get; set; }
public string PaymentMethod { get; set; } = string.Empty;
```

#### `CenterManagement.Application/DTOs/Payment/OutstandingStudentDto.cs`
```csharp
public int StudentProfileId { get; set; }
public string StudentName { get; set; } = string.Empty;
public string? ImagePath { get; set; }
public string GradeLevelName { get; set; } = string.Empty;
public decimal TotalRemaining { get; set; }
```

#### `CenterManagement.Application/DTOs/Payment/TransactionFilter.cs`
```csharp
public string? StudentName { get; set; }
public string? PaymentStatus { get; set; } // "Paid" | "Pending" | null = all
public DateTime? DateFrom { get; set; }
public DateTime? DateTo { get; set; }
public int Page { get; set; } = 1;
public int PageSize { get; set; } = 20;
```

---

### Interface

#### `CenterManagement.Application/Interfaces/IPaymentService.cs`
```csharp
Task<StudentCoursePayment> CreateCoursePaymentAsync(
    int studentProfileId, int courseId, string adminId);

Task<PaymentTransaction> RecordPaymentAsync(
    RecordPaymentDto dto, string adminId);

Task<SessionPayment> CreateSessionPaymentAsync(
    CreateSessionPaymentDto dto, string adminId);

Task<StudentFinancialSummaryDto> GetStudentFinancialSummaryAsync(int studentProfileId);

Task<PagedResult<TransactionListItemDto>> GetTransactionListAsync(TransactionFilter filter);

Task<PaymentKpiDto> GetPaymentKpisAsync();

Task<List<OutstandingStudentDto>> GetOutstandingStudentsAsync();

Task<byte[]> ExportTransactionsCsvAsync(TransactionFilter filter);
```

---

### Service

#### `CenterManagement.Application/Services/PaymentService.cs`

**Implements:** `IPaymentService`  
**Inject:** `CenterManagementDbContext`, `IAuditLogService`

---

**`CreateCoursePaymentAsync`:**
1. Check no existing non-deleted `StudentCoursePayment` for same `studentProfileId + courseId` → throw `InvalidOperationException("Course payment already exists for this student")` if found
2. Load `course` by `courseId`
3. Create:
```csharp
var scp = new StudentCoursePayment
{
    StudentProfileId = studentProfileId,
    CourseId = courseId,
    RequiredAmount = course.Price,
    PaidAmount = 0,
    RemainingAmount = course.Price,
    IsPaid = false,
    CreatedAt = DateTime.UtcNow
};
_db.StudentCoursePayments.Add(scp);
await _db.SaveChangesAsync();
```
4. Write audit `action="CoursePaymentCreated"`
5. Return `scp`

---

**`RecordPaymentAsync`:**
1. Load `StudentCoursePayment` with **tracking** (no `.AsNoTracking()`)
2. If `scp.IsPaid` → throw `InvalidOperationException("This course is already fully paid")`
3. If `dto.Amount <= 0` → throw `InvalidOperationException("Amount must be greater than zero")`
4. If `dto.Amount > scp.RemainingAmount` → throw `InvalidOperationException($"Amount exceeds remaining balance of {scp.RemainingAmount}")`
5. `scp.PaidAmount += dto.Amount`
6. `scp.RemainingAmount = scp.RequiredAmount - scp.PaidAmount` ← always recompute
7. `if (scp.RemainingAmount <= 0) { scp.IsPaid = true; scp.RemainingAmount = 0; }`
8. `scp.UpdatedAt = DateTime.UtcNow`
9. Create `PaymentTransaction`:
```csharp
var tx = new PaymentTransaction
{
    StudentCoursePaymentId = scp.Id,
    Amount = dto.Amount,
    PaymentDate = DateTime.UtcNow,
    AdminId = adminId,
    CreatedAt = DateTime.UtcNow
};
_db.PaymentTransactions.Add(tx);
```
10. `await _db.SaveChangesAsync()` ← scp update + tx creation in ONE call
11. Write audit `action="PaymentRecorded"` with `NewValues = $"Amount:{dto.Amount},Remaining:{scp.RemainingAmount}"`
12. Return `tx`

---

**`GetPaymentKpisAsync` — server-side aggregation only:**
```csharp
var today = DateTime.UtcNow.Date;
var monthStart = new DateTime(today.Year, today.Month, 1);

var totalDueToday = await _db.PaymentTransactions
    .Where(t => t.PaymentDate.Date == today && !t.IsDeleted)
    .SumAsync(t => t.Amount);

var revenueThisMonth = await _db.PaymentTransactions
    .Where(t => t.PaymentDate >= monthStart && !t.IsDeleted)
    .SumAsync(t => t.Amount);

var totalRevenue = await _db.PaymentTransactions
    .Where(t => !t.IsDeleted)
    .SumAsync(t => (decimal?)t.Amount) ?? 0;

var outstanding = await _db.StudentCoursePayments
    .CountAsync(s => !s.IsPaid && !s.IsDeleted);

var paid = await _db.StudentCoursePayments
    .CountAsync(s => s.IsPaid && !s.IsDeleted);

var totalRequired = await _db.StudentCoursePayments
    .Where(s => !s.IsDeleted)
    .SumAsync(s => (decimal?)s.RequiredAmount) ?? 0;

var totalPaid = await _db.StudentCoursePayments
    .Where(s => !s.IsDeleted)
    .SumAsync(s => (decimal?)s.PaidAmount) ?? 0;

var collectionRate = totalRequired > 0 ? totalPaid / totalRequired * 100 : 0;
```

---

**`GetTransactionListAsync`:**
```csharp
var query = _db.PaymentTransactions
    .Include(t => t.StudentCoursePayment)
        .ThenInclude(scp => scp.StudentProfile)
            .ThenInclude(sp => sp.User)
    .Include(t => t.StudentCoursePayment)
        .ThenInclude(scp => scp.Course)
    .AsNoTracking();

// Filter by student name
if (!string.IsNullOrWhiteSpace(filter.StudentName))
    query = query.Where(t =>
        t.StudentCoursePayment.StudentProfile.User.FullName.Contains(filter.StudentName));

// Filter by status
if (filter.PaymentStatus == "Paid")
    query = query.Where(t => t.StudentCoursePayment.IsPaid);
else if (filter.PaymentStatus == "Pending")
    query = query.Where(t => !t.StudentCoursePayment.IsPaid);

// Filter by date range
if (filter.DateFrom.HasValue)
    query = query.Where(t => t.PaymentDate >= filter.DateFrom.Value);
if (filter.DateTo.HasValue)
    query = query.Where(t => t.PaymentDate <= filter.DateTo.Value);

var total = await query.CountAsync();
var items = await query
    .OrderByDescending(t => t.PaymentDate)
    .Skip((filter.Page - 1) * filter.PageSize)
    .Take(filter.PageSize)
    .Select(t => new TransactionListItemDto { ... })
    .ToListAsync();
```

---

**`ExportTransactionsCsvAsync`:**
```csharp
var transactions = await GetTransactionListAsync(new TransactionFilter
    { Page = 1, PageSize = int.MaxValue });

var sb = new StringBuilder();
sb.AppendLine("Date,Student,Course,Amount,Method,Status,Remaining");
foreach (var t in transactions.Items)
{
    sb.AppendLine($"{t.PaymentDate:yyyy-MM-dd},{t.StudentName}," +
                  $"{t.CourseName},{t.Amount},{t.PaymentMethod}," +
                  $"{(t.IsPaid ? "Paid" : "Pending")},{t.RemainingAfterPayment}");
}
return Encoding.UTF8.GetBytes(sb.ToString());
```

---

## Files to MODIFY

### `CenterManagement.Application/DependencyInjection/ApplicationServiceRegistration.cs`

Add:
```csharp
services.AddScoped<IPaymentService, PaymentService>();
```

---

## Web Layer

### `CenterManagement.Web/ViewModels/Payment/PaymentIndexViewModel.cs`
```csharp
public PaymentKpiDto Kpis { get; set; } = new();
public TransactionFilter CurrentFilter { get; set; } = new();
```

---

### `CenterManagement.Web/Controllers/PaymentController.cs`

`[Authorize(Roles = "Admin")]`  
**Inject:** `IPaymentService`

| Method | Route | Returns |
|--------|-------|---------|
| `GET` | `/Payment` | `View(new PaymentIndexViewModel { Kpis = await GetPaymentKpisAsync() })` |
| `GET` | `/Payment/KPIs` | `Json(await GetPaymentKpisAsync())` |
| `GET` | `/Payment/Transactions` | `Json(await GetTransactionListAsync(filter))` |
| `POST` | `/Payment/RecordCourse` | `[FromBody] RecordPaymentDto` → `Json({success, newPaidAmount, newRemainingAmount, isPaid})` |
| `POST` | `/Payment/CreateCourse` | `[FromBody] {studentProfileId, courseId}` → `Json(scp)` |
| `POST` | `/Payment/RecordSession` | `[FromBody] CreateSessionPaymentDto` → `Json(sessionPayment)` |
| `GET` | `/Payment/StudentSummary/{id}` | `Json(await GetStudentFinancialSummaryAsync(id))` |
| `GET` | `/Payment/Outstanding` | `Json(await GetOutstandingStudentsAsync())` |
| `GET` | `/Payment/Export` | `File(bytes, "text/csv; charset=utf-8", "transactions.csv")` |

**`RecordCourse` action — wrap `InvalidOperationException` in friendly response:**
```csharp
[HttpPost]
public async Task<IActionResult> RecordCourse([FromBody] RecordPaymentDto dto)
{
    try
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var tx = await _paymentService.RecordPaymentAsync(dto, adminId);
        var scp = await _db.StudentCoursePayments.FindAsync(dto.StudentCoursePaymentId);
        return Json(new { success = true, newPaidAmount = scp!.PaidAmount,
                          newRemainingAmount = scp.RemainingAmount, isPaid = scp.IsPaid });
    }
    catch (InvalidOperationException ex)
    {
        return Json(new { success = false, error = ex.Message });
    }
}
```

Note: `PaymentController` may inject `CenterManagementDbContext` **only** for the above reload-after-save pattern (reading `scp` post-update). All business logic stays in `PaymentService`.

---

### `CenterManagement.Web/Views/Payment/Index.cshtml`

`@model PaymentIndexViewModel` · Layout `"_Layout"`

Scaffolded from `payments-financial.html`. Server-side pre-renders KPI cards from `@Model.Kpis` for fast initial paint. JS refreshes them after every payment.

**KPI cards** (id each for JS targeting):
- `id="kpi-due-today"` → `@Model.Kpis.TotalDueToday`
- `id="kpi-revenue-month"` → `@Model.Kpis.RevenueThisMonth`
- `id="kpi-outstanding"` → `@Model.Kpis.OutstandingStudentsCount`
- `id="kpi-collection-rate"` → `@Model.Kpis.CollectionRatePercent:F1`%

**Transaction table:** empty `<tbody id="tx-tbody">` on load; populated via `fetchTransactions(1)`.

**Filter select:**
```html
<select id="status-filter" onchange="fetchTransactions(1)">
    <option value="">All Transactions</option>
    <option value="Paid">Paid</option>
    <option value="Pending">Pending</option>
</select>
```

**"Record Payment" button** → opens `<div id="paymentModal" class="hidden ...">` modal.

**"Export" button** → `window.location.href = '/Payment/Export'`

**Modal — Record New Payment:**
- Student search input with debounced fetch to `/Student/Search?q=` — renders dropdown suggestions
- On student select: fetch `/Payment/StudentSummary/{id}` → populate course payment select
- Amount input: `type="number" step="0.01" min="0.01"`
- Method select: Cash / Card / Transfer
- Confirm button → `POST /Payment/RecordCourse`

**JavaScript:**
```js
async function loadKpis() {
    const res = await fetch('/Payment/KPIs');
    const data = await res.json();
    document.getElementById('kpi-due-today').textContent = data.totalDueToday.toFixed(2);
    document.getElementById('kpi-revenue-month').textContent = data.revenueThisMonth.toFixed(2);
    document.getElementById('kpi-outstanding').textContent = data.outstandingStudentsCount;
    document.getElementById('kpi-collection-rate').textContent = data.collectionRatePercent.toFixed(1) + '%';
}

async function fetchTransactions(page) {
    const status = document.getElementById('status-filter').value;
    const res = await fetch(`/Payment/Transactions?page=${page}&paymentStatus=${status}`);
    const data = await res.json();
    renderTransactionTable(data.items);
    renderPagination(data.totalPages, page);
}

function renderTransactionTable(items) {
    const tbody = document.getElementById('tx-tbody');
    tbody.innerHTML = '';
    items.forEach(t => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${new Date(t.paymentDate).toLocaleDateString()}</td>
            <td>${t.studentName}</td>
            <td>${t.courseName}</td>
            <td class="text-right font-mono">${t.amount.toFixed(2)}</td>
            <td>${t.paymentMethod}</td>
            <td><span class="${t.isPaid ? 'badge-green' : 'badge-orange'}">${t.isPaid ? 'Paid' : 'Pending'}</span></td>
        `;
        tbody.appendChild(row);
    });
}

async function submitPayment() {
    const payload = {
        studentCoursePaymentId: parseInt(document.getElementById('payment-scp-id').value),
        amount: parseFloat(document.getElementById('payment-amount').value),
        paymentMethod: document.getElementById('payment-method').value,
        notes: document.getElementById('payment-notes').value
    };
    const res = await fetch('/Payment/RecordCourse', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-CSRF-TOKEN': getAntiForgeryToken()
        },
        body: JSON.stringify(payload)
    });
    const data = await res.json();
    if (data.success) {
        closeModal();
        loadKpis();
        fetchTransactions(1);
    } else {
        document.getElementById('payment-error').textContent = data.error;
    }
}

document.addEventListener('DOMContentLoaded', () => {
    loadKpis();
    fetchTransactions(1);
});
```

---

## Completion Checklist

- [ ] `POST /Payment/CreateCourse` → `StudentCoursePayment` created with `RequiredAmount = Course.Price`, `PaidAmount=0`, `IsPaid=false`
- [ ] Duplicate course payment creation → service throws, controller returns `{success=false, error=...}`
- [ ] `POST /Payment/RecordCourse` partial amount → `PaidAmount` updated, `RemainingAmount = RequiredAmount - PaidAmount`, `IsPaid=false`
- [ ] `POST /Payment/RecordCourse` exact remaining amount → `IsPaid=true`, `RemainingAmount=0`
- [ ] `POST /Payment/RecordCourse` amount > remaining → `{success=false, error="Amount exceeds remaining balance"}`
- [ ] `POST /Payment/RecordCourse` on already-paid course → `{success=false, error="This course is already fully paid"}`
- [ ] `scp.PaidAmount` update AND `PaymentTransaction` creation are in the same `SaveChangesAsync()` — verify by simulating failure between the two; both should rollback together
- [ ] `GET /Payment/KPIs` → all 4 values are aggregated on the SQL server (verify with EF Core query logging: no `SELECT *` before aggregation)
- [ ] `GET /Payment/Export` → downloads CSV with correct headers and data rows
- [ ] Payment modal: submit payment → modal closes, KPI cards refresh, table refreshes — all without page reload
