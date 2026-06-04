using System.Text.Json.Serialization;

namespace BE.Dtos.Admin;

public class AdminInvoiceItemDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; set; }
}
