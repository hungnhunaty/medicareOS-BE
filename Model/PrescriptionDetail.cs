using System;
using System.Collections.Generic;

namespace BE.Model;

public partial class PrescriptionDetail
{
    public int PrescriptionId { get; set; }

    public int MedicationId { get; set; }

    public int Quantity { get; set; }

    public string? Instructions { get; set; }

    public decimal Price { get; set; }

    public virtual Medication Medication { get; set; } = null!;

    public virtual Prescription Prescription { get; set; } = null!;
}
