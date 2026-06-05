namespace CenterManagement.Application.DTOs.Payment
{
    public class OutstandingStudentDto
    {
        public int StudentProfileId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public string GradeLevelName { get; set; } = string.Empty;
        public decimal TotalRemaining { get; set; }
    }
}
