using System;
using System.Collections.Generic;

namespace BE.Model;

public partial class Staff
{
    public int UserId { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public int DepartmentId { get; set; }

    public decimal BaseSalary { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual Doctor? Doctor { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();

    public virtual User User { get; set; } = null!;
}
