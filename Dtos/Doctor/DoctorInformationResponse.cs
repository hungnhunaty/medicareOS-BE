namespace BE.Dtos.Doctor;

public class DoctorInformationResponse
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Department { get; set; } = "";
    public string Specialty { get; set; } = "";
    public double BaseSalary { get; set; } = 0;
    public string? ClinicName { get; set; }
}
