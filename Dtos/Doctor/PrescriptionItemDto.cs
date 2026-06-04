namespace BE.Dtos.Doctor;

public class PrescriptionItemDto
{
    public int MedicationId { get; set; }
    public int Quantity { get; set; }
    public string Instructions { get; set; } = "";
}
