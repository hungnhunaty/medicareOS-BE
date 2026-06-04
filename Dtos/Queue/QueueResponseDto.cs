namespace BE.Dtos.Queue;

public class QueueResponseDto
{
    public int ExamId { get; set; }
    public string PatientName { get; set; } = "";
    public string PatientCode { get; set; } = "";
    public string ClinicName { get; set; } = "";
    public int QueueNumber { get; set; }
    public int WaitingCount { get; set; }
    public int Status { get; set; }
}
