using System;
using System.Collections.Generic;

namespace BE.Model;

public partial class Patient
{
    public int UserId { get; set; }

    public string PatientCode { get; set; } = null!;

    public string? BloodType { get; set; }

    public string? Allergies { get; set; }

    public string? FamilyMedicalHistory { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<MedicalExamination> MedicalExaminations { get; set; } = new List<MedicalExamination>();

    public virtual User User { get; set; } = null!;
}
