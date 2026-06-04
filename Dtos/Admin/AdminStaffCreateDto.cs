namespace BE.Dtos.Admin;

public class AdminStaffCreateDto
{
    public string UserName { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Role { get; set; } = "Nhân viên y tế";
    public string Department { get; set; } = "";
    public string Status { get; set; } = "Hoạt động";
    public decimal BaseSalary { get; set; }
    public string Specialty { get; set; } = "";
    public string Password { get; set; } = "";
}
