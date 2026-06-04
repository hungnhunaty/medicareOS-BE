using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BE.Model;
using BE.Dtos.Admin;
using Microsoft.EntityFrameworkCore;

namespace BE.Services;

public class AdminPatientService
{
    private readonly HospitalManagementDbContext _dbContext;

    public AdminPatientService(HospitalManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<object> GetAllPatientsAsync()
    {
        var patients = await _dbContext.Patients
            .Include(p => p.User)
            .Include(p => p.MedicalExaminations)
                .ThenInclude(m => m.Doctor).ThenInclude(d => d.User).ThenInclude(u => u.User)
            .Include(p => p.MedicalExaminations)
                .ThenInclude(m => m.Prescriptions).ThenInclude(pr => pr.PrescriptionDetails).ThenInclude(pd => pd.Medication)
            .OrderByDescending(p => p.User.CreatedAt)
            .Select(p => new
            {
                userId = p.UserId,
                id = p.PatientCode,
                userName = p.User.UserName,
                name = p.User.FullName,
                phone = p.User.Phone ?? "N/A",
                address = p.User.Address ?? "Chưa cập nhật",
                gender = p.User.Gender ?? "Khác",
                nfcStatus = p.PatientCode.Contains("EMG") ? "Chưa định danh" : "Đã định danh",
                bloodType = p.BloodType ?? "Chưa rõ",
                allergies = p.Allergies ?? "Không có",
                history = p.MedicalExaminations.OrderByDescending(m => m.VisitDate).Select(m => new
                {
                    date = m.VisitDate.HasValue ? m.VisitDate.Value.ToString("dd/MM/yyyy HH:mm") : "N/A",
                    department = m.Doctor.Specialty ?? "Khám Tổng Quát",
                    doctor = m.Doctor.User.User.FullName,
                    diagnosis = m.Diagnosis ?? "Đang chờ kết luận",
                    prescription = m.Prescriptions.SelectMany(pr => pr.PrescriptionDetails).Select(pd => new
                    {
                        name = pd.Medication.Name,
                        dosage = pd.Instructions ?? "Ngày uống 2 lần",
                        price = pd.Price
                    }).ToList()
                }).ToList()
            })
            .ToListAsync();

        return patients;
    }

    public async Task<object?> CreatePatientAsync(AdminPatientCreateDto dto)
    {
        var user = new User
        {
            UserName = string.IsNullOrEmpty(dto.UserName) ? null : dto.UserName.Trim(),
            FullName = dto.FullName.Trim(),
            Password = string.IsNullOrEmpty(dto.Password) ? null : BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Phone = dto.Phone?.Trim(),
            Address = dto.Address?.Trim(),
            Gender = dto.Gender?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Patient");
        if (role != null)
        {
            _dbContext.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = role.RoleId });
            await _dbContext.SaveChangesAsync();
        }

        string patCode = $"BN-{DateTime.UtcNow.ToString("yyMM")}-{user.UserId:D4}";
        var patient = new Patient
        {
            UserId = user.UserId,
            PatientCode = patCode,
            BloodType = dto.BloodType ?? "O+",
            Allergies = dto.Allergies ?? "Không có",
            FamilyMedicalHistory = "Bình thường"
        };

        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        return new
        {
            userId = user.UserId,
            id = patCode,
            name = user.FullName,
            phone = user.Phone ?? "N/A",
            address = user.Address ?? "Chưa cập nhật",
            gender = user.Gender ?? "Khác",
            nfcStatus = "Đã định danh",
            history = new List<object>()
        };
    }
}


