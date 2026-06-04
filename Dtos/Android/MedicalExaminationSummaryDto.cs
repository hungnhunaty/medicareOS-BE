using System.Text.Json.Serialization;

namespace BE.Dtos.Android;

public class MedicalExaminationSummaryDto
{
    [JsonPropertyName("medicalExaminationId")]
    public int MedicalExaminationId { get; set; }

    [JsonPropertyName("visitDate")]
    public DateTime? VisitDate { get; set; }

    [JsonPropertyName("doctorName")]
    public string DoctorName { get; set; } = string.Empty;

    [JsonPropertyName("diagnosis")]
    public string Diagnosis { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public int? Status { get; set; }
}