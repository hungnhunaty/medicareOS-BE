using System;

namespace BE.Dtos.Account
{
    public class ProfileUpdateDto
    {
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public DateOnly? Dob { get; set; }
        public string? Address { get; set; }
    }
}
