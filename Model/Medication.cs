using System;
using System.Collections.Generic;

namespace BE.Model;

public partial class Medication
{
    public int MedicationId { get; set; }

    public string Name { get; set; } = null!;

    public string? Ingredient { get; set; }

    public string? Unit { get; set; }

    public decimal CurentPrice { get; set; }

    public int? Quantity { get; set; }

    public virtual ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new List<PrescriptionDetail>();
}
