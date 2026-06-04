namespace CenterManagement.Application.DTOs.Common
{
    public class StudentListFilter
    {
        public string? SearchQuery { get; set; }   // matches FullName or PhoneNumber
        public int? GradeLevelId { get; set; }
        public int? SubjectId { get; set; }
        public string? PaymentStatus { get; set; } // "Paid" | "Partial" | "Unpaid"
        public bool IncludeInactive { get; set; } = false;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
