namespace BE.Dtos.Admin;

public class AdminServiceCreateDto
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "Khám bệnh";
    public decimal Price { get; set; }
    public string Status { get; set; } = "Đang áp dụng";
}
