using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BE.Model;
using BE.Dtos.Admin;
using Microsoft.EntityFrameworkCore;

namespace BE.Services;

public class AdminStaffService
{
    private readonly HospitalManagementDbContext _dbContext;

    public AdminStaffService(HospitalManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<object> GetAllStaffAsync()
    {
        var staffs = await _dbContext.Staffs
            .Include(s => s.User)
            .Include(s => s.Department)
            .Include(s => s.Doctor)
            .OrderByDescending(s => s.User.CreatedAt)
            .Select(s => new
            {
                userId = s.UserId,
                id = s.EmployeeCode,
                userName = s.User.UserName,
                name = s.User.FullName,
                role = s.Doctor != null ? "Bác sĩ " + (s.Doctor.Specialty ?? "") : "Nhân viên y tế",
                department = s.Department.Name,
                departmentId = s.DepartmentId,
                phone = s.User.Phone ?? "N/A",
                email = s.User.Email ?? "N/A",
                status = s.User.IsActive == true ? "Hoạt động" : (s.User.IsActive == false ? "Đã khóa" : "Nghỉ phép"),
                baseSalary = s.BaseSalary,
                specialty = s.Doctor != null ? s.Doctor.Specialty : ""
            })
            .ToListAsync();

        return staffs;
    }

    public async Task<object?> CreateStaffAsync(AdminStaffCreateDto dto)
    {
        // Kiểm tra username
        if (await _dbContext.Users.AnyAsync(u => u.UserName == dto.UserName))
        {
            return null; // Trùng lặp
        }

        var dept = await _dbContext.Departments.FirstOrDefaultAsync(d => d.Name == dto.Department || d.DepartmentId.ToString() == dto.Department);
        int deptId = dept?.DepartmentId ?? (await _dbContext.Departments.FirstOrDefaultAsync())?.DepartmentId ?? 1;

        var user = new User
        {
            UserName = dto.UserName.Trim(),
            FullName = dto.FullName.Trim(),
            Password = BCrypt.Net.BCrypt.HashPassword(string.IsNullOrEmpty(dto.Password) ? "staff123" : dto.Password),
            Email = dto.Email?.Trim(),
            Phone = dto.Phone?.Trim(),
            IsActive = dto.Status == "Hoạt động" ? true : (dto.Status == "Đã khóa" ? false : null),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Gán role
        string targetRoleName = dto.Role.Contains("Bác sĩ") ? "Doctor" : "Staff";
        var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == targetRoleName);
        if (role != null)
        {
            _dbContext.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = role.RoleId });
            await _dbContext.SaveChangesAsync();
        }

        // Tạo bản ghi Staff
        string empCode = $"EMP-{DateTime.UtcNow.ToString("yyMM")}-{user.UserId:D3}";
        var staff = new Staff
        {
            UserId = user.UserId,
            EmployeeCode = empCode,
            DepartmentId = deptId,
            BaseSalary = dto.BaseSalary > 0 ? dto.BaseSalary : 12000000m
        };

        _dbContext.Staffs.Add(staff);
        await _dbContext.SaveChangesAsync();

        if (targetRoleName == "Doctor")
        {
            var doctor = new Doctor
            {
                UserId = user.UserId,
                Specialty = dto.Specialty ?? "Đa khoa",
                LicenseNumber = "CCHN-" + DateTime.UtcNow.Ticks.ToString().Substring(10),
                CommissionRate = 5.0m
            };
            _dbContext.Doctors.Add(doctor);
            await _dbContext.SaveChangesAsync();
        }

        return new
        {
            userId = user.UserId,
            id = empCode,
            name = user.FullName,
            role = dto.Role,
            department = dept?.Name ?? "Khoa Khám Bệnh",
            status = dto.Status
        };
    }

    public async Task<bool> UpdateStaffAsync(int userId, AdminStaffUpdateDto dto)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        var staff = await _dbContext.Staffs.FindAsync(userId);

        if (user == null || staff == null) return false;

        user.FullName = dto.FullName.Trim();
        user.Email = dto.Email?.Trim();
        user.Phone = dto.Phone?.Trim();
        
        if (!string.IsNullOrEmpty(dto.Password))
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }
        user.IsActive = dto.Status == "Hoạt động" ? true : (dto.Status == "Đã khóa" ? false : null);

        if (dto.BaseSalary > 0)
        {
            staff.BaseSalary = dto.BaseSalary;
        }

        var dept = await _dbContext.Departments.FirstOrDefaultAsync(d => d.Name == dto.Department);
        if (dept != null)
        {
            staff.DepartmentId = dept.DepartmentId;
        }

        var doctor = await _dbContext.Doctors.FindAsync(userId);
        if (doctor != null && !string.IsNullOrEmpty(dto.Specialty))
        {
            doctor.Specialty = dto.Specialty.Trim();
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteStaffAsync(int userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) return false;

        // An toàn: Chuyển sang vô hiệu hóa (Đã khóa) thay vì xóa vật lý gây lỗi khóa ngoại
        user.IsActive = false;
        await _dbContext.SaveChangesAsync();
        return true;
    }
}


