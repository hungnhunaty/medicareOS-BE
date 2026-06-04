namespace BE.Dtos.Admin;

public class AdminServiceUpdateDto
{
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public string Status { get; set; } = "Đang áp dụng";
}
