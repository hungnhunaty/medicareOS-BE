using System.Text.Json.Serialization;

namespace BE.Dtos.Android;

public class PatientProfileDto
{
    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("gender")]
    public string Gender { get; set; } = string.Empty;

    [JsonPropertyName("dateOfBirth")]
    public DateOnly? DateOfBirth { get; set; }

    [JsonPropertyName("patientCode")]
    public string PatientCode { get; set; } = string.Empty;

    [JsonPropertyName("bloodType")]
    public string BloodType { get; set; } = string.Empty;
    [JsonPropertyName("allergies")]
    public string Allergies { get; set; } = string.Empty;

    [JsonPropertyName("familyMedicalHistory")]
    public string FamilyMedicalHistory { get; set; } = string.Empty;
}