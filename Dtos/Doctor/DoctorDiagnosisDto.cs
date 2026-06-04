using System.Collections.Generic;

namespace BE.Dtos.Doctor;

public class DoctorDiagnosisDto
{
    public string Diagnosis { get; set; } = "";
    public string TreatmentPlan { get; set; } = "";
    public int Status { get; set; } = 2; // 2: Hoàn thành
    public List<PrescriptionItemDto> Medications { get; set; } = new List<PrescriptionItemDto>();
}
