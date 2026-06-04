namespace BE.Dtos.Doctor;

public class DoctorInformationRequest
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Specialty { get; set; } = "";
}
