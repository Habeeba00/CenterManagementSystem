using CenterManagement.Application.DTOs.Student;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CenterManagement.Web.ViewModels.Student
{
    public class StudentListViewModel
    {
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
    }
}
