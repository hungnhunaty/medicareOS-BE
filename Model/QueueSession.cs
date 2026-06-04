using System;

namespace BE.Model;

public partial class QueueSession
{
    public string SessionId { get; set; } = null!;
    public int DepartmentId { get; set; }
    public string? Symptoms { get; set; }
    public int? DoctorId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(30);
    public DateTime? UsedAt { get; set; }
    public int CreatedByUserId { get; set; }
}
