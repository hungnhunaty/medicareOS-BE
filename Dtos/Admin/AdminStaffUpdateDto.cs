namespace BE.Dtos.Admin;

public class AdminStaffUpdateDto
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Department { get; set; } = "";
    public string Status { get; set; } = "Hoạt động";
    public decimal BaseSalary { get; set; }
    public string Specialty { get; set; } = "";
    public string? Password { get; set; }
}
