using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BE.Dtos.Admin;

public class AdminInvoiceCreateDto
{
    [JsonPropertyName("patientName")]
    public string PatientName { get; set; } = "";

    [JsonPropertyName("patientCode")]
    public string PatientCode { get; set; } = "";

    [JsonPropertyName("method")]
    public string Method { get; set; } = "Tiền mặt";

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Đã thanh toán";

    [JsonPropertyName("items")]
    public List<AdminInvoiceItemDto> Items { get; set; } = new();
}
