namespace NaSpontanie.API.Dtos.Reports
{
    public class CreateReportDto
    {
        public int ReporterId { get; set; }
        public int? ReportedUserId { get; set; }
        public int? ReportedEventId { get; set; }
        public string Reason { get; set; } = "Komentarz narusza regulamin";
    }
}
