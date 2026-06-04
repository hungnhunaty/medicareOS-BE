using System;
using System.Collections.Generic;

namespace BE.Model;

public partial class InvoiceDetail
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }

    public int ItemType { get; set; }

    public string ItemName { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalAmount { get; set; }

    public int? ReferenceId { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;
}
