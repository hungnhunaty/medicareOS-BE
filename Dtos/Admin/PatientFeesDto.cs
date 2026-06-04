using System.Collections.Generic;

namespace BE.Dtos.Admin;

public class PatientFeeItemDto
{
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class PatientFeesDto
{
    public string PatientName { get; set; } = "";
    public string PatientCode { get; set; } = "";
    public List<PatientFeeItemDto> Items { get; set; } = new List<PatientFeeItemDto>();
    public decimal Total { get; set; }
}
