namespace BE.Dtos.Qr;

public class QueueQrCreateDto
{
    public int DepartmentId { get; set; }
    public string? Symptoms { get; set; }
    public int? DoctorId { get; set; }
}
