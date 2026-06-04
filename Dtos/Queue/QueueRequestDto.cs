namespace BE.Dtos.Queue;

public class QueueRequestDto
{
    public int DepartmentId { get; set; }
    
    // Nếu có PatientId thì dùng, nếu không thì dùng thông tin bên dưới để tạo hồ sơ
    public int? PatientId { get; set; }
    
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public string? Symptoms { get; set; }
    public string? Address { get; set; }
    public int? DoctorId { get; set; }
}
