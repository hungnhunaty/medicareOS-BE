namespace BE.Dtos.Admin;

public class AdminPatientCreateDto
{
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
    public string Gender { get; set; } = "Khác";
    public string BloodType { get; set; } = "O+";
    public string Allergies { get; set; } = "Không có";
    public string? UserName { get; set; }
    public string? Password { get; set; }
}
