using System.Text.Json.Serialization;

namespace BE.Dtos.Android;

public class MedicalExaminationDetailDto
{
    [JsonPropertyName("medicalExaminationId")]
    public int MedicalExaminationId { get; set; }

    [JsonPropertyName("visitDate")]
    public DateTime? VisitDate { get; set; }

    [JsonPropertyName("patientName")]
    public string PatientName { get; set; } = string.Empty;

    [JsonPropertyName("doctorName")]
    public string DoctorName { get; set; } = string.Empty;

    [JsonPropertyName("symptoms")]
    public string Symptoms { get; set; } = string.Empty;

    [JsonPropertyName("diagnosis")]
    public string Diagnosis { get; set; } = string.Empty;

    [JsonPropertyName("treatmentPlan")]
    public string TreatmentPlan { get; set; } = string.Empty;

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    [JsonPropertyName("bloodPressure")]
    public string BloodPressure { get; set; } = string.Empty;

    [JsonPropertyName("heartRate")]
    public int? HeartRate { get; set; }

    [JsonPropertyName("temperature")]
    public decimal? Temperature { get; set; }

    [JsonPropertyName("weight")]
    public decimal? Weight { get; set; }

    [JsonPropertyName("height")]
    public decimal? Height { get; set; }

    [JsonPropertyName("status")]
    public int? Status { get; set; }
}