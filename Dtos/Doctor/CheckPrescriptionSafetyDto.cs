using System.Collections.Generic;

namespace BE.Dtos.Doctor;

public class CheckPrescriptionSafetyDto
{
    public int ExaminationId { get; set; }
    public List<CheckPrescriptionItemDto> Medications { get; set; } = new List<CheckPrescriptionItemDto>();
}

public class CheckPrescriptionItemDto
{
    public int MedicationId { get; set; }
    public string? Name { get; set; }
    public int Quantity { get; set; }
    public string? Instructions { get; set; }
}
