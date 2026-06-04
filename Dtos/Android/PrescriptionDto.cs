using System.Text.Json.Serialization;

namespace BE.Dtos.Android;

public class PrescriptionDto
{
    [JsonPropertyName("prescriptionsId")]
    public int PrescriptionsId { get; set; }

    [JsonPropertyName("medicalExaminationId")]
    public int MedicalExaminationId { get; set; }

    [JsonPropertyName("date")]
    public DateTime? Date { get; set; }

    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("items")]
    public List<PrescriptionDetailDto> Items { get; set; } = new();
}