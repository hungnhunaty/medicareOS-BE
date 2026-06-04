using System;
using System.Collections.Generic;

namespace BE.Model;

public partial class Payroll
{
    public int PayrollId { get; set; }

    public int StaffId { get; set; }

    public string PayPeriod { get; set; } = null!;

    public decimal? Allowances { get; set; }

    public decimal? Bonuses { get; set; }

    public decimal? Deductions { get; set; }

    public decimal NetPay { get; set; }

    public DateTime? PaymentDate { get; set; }

    public int? Status { get; set; }

    public virtual Staff Staff { get; set; } = null!;
}
