using System;
using System.Collections.Generic;

namespace BE.Model;

public partial class Service
{
    public int ServiceId { get; set; }

    public string Name { get; set; } = null!;

    public decimal CurrentPrice { get; set; }

    public virtual ICollection<ServiceDetail> ServiceDetails { get; set; } = new List<ServiceDetail>();
}
