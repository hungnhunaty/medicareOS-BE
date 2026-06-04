using System;

namespace BE.Dtos.Patient;

public class PatientBookingDto
{
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime? PreferredDate { get; set; }
    public string Symptoms { get; set; } = "";
}
