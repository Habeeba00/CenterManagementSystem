using CenterManagement.Application.DTOs.Attendance;
using CenterManagement.Application.DTOs.Student;
using CenterManagement.Application.DTOs.Common;
using CenterManagement.Domain.Entities;
using CenterManagement.Domain.Common;

namespace CenterManagement.Application.Interfaces;

public interface IAttendanceService
{
    Task<ScanResultDto> ProcessScanAsync(string qrCode, DateTime scanTime);
    Task MarkManuallyAsync(ManualMarkDto dto, string adminId);
    Task<AttendanceSessionSummaryDto> GetSessionSummaryAsync(int sessionId);
    Task<PagedResult<AttendanceListItemDto>> GetSessionAttendanceListAsync(int sessionId, int page, int pageSize, string? search);
    Task<List<StudentAttendanceDto>> GetStudentAttendanceHistoryAsync(int studentProfileId, int? groupId, DateTime? from, DateTime? to);
    Task<decimal> GetStudentAttendanceRateAsync(int studentProfileId);
    Task<Session?> GetSessionForStudentAtTimeAsync(int studentProfileId, DateTime scanTime);
    Task<Session?> GetSessionForInstructorAtTimeAsync(int instructorProfileId, DateTime scanTime);
}
