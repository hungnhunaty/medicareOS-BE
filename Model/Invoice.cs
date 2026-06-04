using System;
using System.Collections.Generic;

namespace BE.Model;

public partial class Invoice
{
    public int InvoiceId { get; set; }

    public int MedicalExaminationId { get; set; }

    public int PatientId { get; set; }

    public int CashierId { get; set; }

    public DateTime? InvoiceDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string? PaymentMethod { get; set; }

    public int? Status { get; set; }

    public virtual Staff Cashier { get; set; } = null!;

    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();

    public virtual MedicalExamination MedicalExamination { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;
}
