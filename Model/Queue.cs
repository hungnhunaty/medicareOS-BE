using System;

namespace BE.Model;

public partial class Queue
{
    public int QueueId { get; set; }

    public int ClinicId { get; set; }

    public int PatientId { get; set; }

    public int? MedicalExaminationId { get; set; }

    public int QueueNumber { get; set; }

    public int Status { get; set; } // 0: Waiting, 1: Active, 2: Done, 3: Skipped

    public int Priority { get; set; } // 0: Regular, 1: Priority, 2: Emergency

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;

    public virtual MedicalExamination? MedicalExamination { get; set; }
}
