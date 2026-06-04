using System.Text.Json.Serialization;

namespace BE.Dtos.Android;

public class PrescriptionDetailDto
{
    [JsonPropertyName("medicationId")]
    public int MedicationId { get; set; }

    [JsonPropertyName("medicationName")]
    public string MedicationName { get; set; } = string.Empty;

    [JsonPropertyName("ingredient")]
    public string Ingredient { get; set; } = string.Empty;

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("instructions")]
    public string Instructions { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("lineTotal")]
    public decimal LineTotal { get; set; }
}