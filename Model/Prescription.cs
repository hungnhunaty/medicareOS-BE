using System;
using System.Collections.Generic;

namespace BE.Model;

public partial class Prescription
{
    public int PrescriptionsId { get; set; }

    public int MedicalExaminationId { get; set; }

    public DateTime? Date { get; set; }

    public int? Status { get; set; }

    public virtual MedicalExamination MedicalExamination { get; set; } = null!;

    public virtual ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new List<PrescriptionDetail>();
}
