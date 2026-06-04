using System;
using System.Collections.Generic;

namespace BE.Model;

public partial class ServiceDetail
{
    public int ServiceId { get; set; }

    public int MedicalExaminationId { get; set; }

    public decimal UnitPrice { get; set; }

    public int? Quantity { get; set; }

    public virtual MedicalExamination MedicalExamination { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
