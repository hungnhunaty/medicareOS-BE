using System;
using System.Collections.Generic;

namespace BE.Model;

public partial class User
{
    public int UserId { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsEmailVerified { get; set; }

    public string? VerificationToken { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTime? ResetTokenExpiry { get; set; }

    public virtual Patient? Patient { get; set; }

    public virtual Staff? Staff { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
