using System;
using System.Collections.Generic;

namespace BE.Model;

public partial class Doctor
{
    public int UserId { get; set; }

    public string? Specialty { get; set; }

    public string? LicenseNumber { get; set; }

    public decimal? CommissionRate { get; set; }

    public virtual ICollection<MedicalExamination> MedicalExaminations { get; set; } = new List<MedicalExamination>();

    public virtual Staff User { get; set; } = null!;
}
