using System;
using System.Collections.Generic;

namespace BE.Model;

public partial class Clinic
{
    public int ClinicId { get; set; }

    public string Name { get; set; } = null!;

    public int DepartmentId { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<MedicalExamination> MedicalExaminations { get; set; } = new List<MedicalExamination>();
}
