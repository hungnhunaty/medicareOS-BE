namespace BE.Dtos.Staff;

public class StaffRegisterQueueDto
{
    public int? PatientId { get; set; }
    public string PatientName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Gender { get; set; } = "Nam";
    public int DoctorId { get; set; }
    public string Department { get; set; } = "Khoa Khám bệnh";
    public string Symptoms { get; set; } = "";
}
