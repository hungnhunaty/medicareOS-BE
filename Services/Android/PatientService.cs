using BE.Dtos.Android;
using BE.Model;
using Microsoft.EntityFrameworkCore;

namespace BE.Services;

public class PatientService
{
    private readonly HospitalManagementDbContext _context;
    
    public PatientService(HospitalManagementDbContext context)
    {
        _context = context;
    }

    public async Task<PatientProfileDto?> GetMyProfileAsync(int userId)
    {
        return await _context.Users
            .Where(u => u.UserId == userId)
            .Select(u => new PatientProfileDto
            {
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                Address = u.Address,
                Gender = u.Gender,
                DateOfBirth = u.DateOfBirth,

                PatientCode = u.Patient != null ? u.Patient.PatientCode : null,
                BloodType = u.Patient != null ? u.Patient.BloodType : null,
                FamilyMedicalHistory = u.Patient != null ? u.Patient.FamilyMedicalHistory : null,
                Allergies = u.Patient != null ? u.Patient.Allergies : null
            })
            .FirstOrDefaultAsync();
    }
}