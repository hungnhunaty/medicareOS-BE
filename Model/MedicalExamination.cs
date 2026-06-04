using System;
using System.Collections.Generic;

namespace BE.Model;

public partial class MedicalExamination
{
    public int MedicalExaminationId { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public DateTime? VisitDate { get; set; }

    public string? Symptoms { get; set; }

    public string? Diagnosis { get; set; }

    public string? TreatmentPlan { get; set; }

    public string? Notes { get; set; }

    public string? BloodPressure { get; set; }

    public int? HeartRate { get; set; }

    public decimal? Temperature { get; set; }

    public decimal? Weight { get; set; }

    public decimal? Height { get; set; }

    public int? Status { get; set; }

    public int? CallCount { get; set; } = 0;

    public int? ClinicId { get; set; }

    public virtual Clinic? Clinic { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual Patient Patient { get; set; } = null!;

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual ICollection<ServiceDetail> ServiceDetails { get; set; } = new List<ServiceDetail>();
}
