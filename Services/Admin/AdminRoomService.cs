using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BE.Model;
using BE.Dtos.Admin;
using Microsoft.EntityFrameworkCore;

namespace BE.Services;

public class AdminRoomService
{
    private readonly HospitalManagementDbContext _dbContext;

    public AdminRoomService(HospitalManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<object> GetAllRoomsAsync()
    {
        // Lấy danh sách doctor đang khám (status = 1: Đang khám) để biết phòng nào đang sử dụng
        var activeExams = await _dbContext.MedicalExaminations
            .Where(e => e.Status == 1 && e.ClinicId != null)
            .Include(e => e.Doctor)
                .ThenInclude(d => d.User)
                    .ThenInclude(u => u.User)
            .Include(e => e.Doctor)
                .ThenInclude(d => d.User)
                    .ThenInclude(s => s.Department)
            .ToListAsync();

        var activeByClinic = activeExams
            .GroupBy(e => e.ClinicId)
            .ToDictionary(
                g => g.Key!.Value,
                g => g.First()
            );

        var rooms = await _dbContext.Clinics
            .Include(c => c.Department)
            .OrderBy(c => c.ClinicId)
            .Select(c => new
            {
                roomId = c.ClinicId,
                roomName = c.Name,
                departmentId = c.DepartmentId,
                departmentName = c.Department.Name,
                isActive = c.IsActive
            })
            .ToListAsync();

        var result = rooms.Select(r =>
        {
            var hasActiveExam = activeByClinic.ContainsKey(r.roomId);
            string? doctorName = null;
            string? doctorDept = null;
            int? doctorId = null;

            if (hasActiveExam)
            {
                var exam = activeByClinic[r.roomId];
                doctorName = exam.Doctor?.User?.User?.FullName;
                doctorDept = exam.Doctor?.User?.Department?.Name;
                doctorId = exam.DoctorId;
            }

            return new
            {
                r.roomId,
                r.roomName,
                r.departmentId,
                r.departmentName,
                r.isActive,
                status = !r.isActive ? "Ngưng hoạt động" : (hasActiveExam ? "Đang sử dụng" : "Trống"),
                assignedDoctorId = doctorId,
                assignedDoctorName = doctorName ?? "",
                assignedDoctorDepartment = doctorDept ?? ""
            };
        }).ToList();

        return result;
    }

    public async Task<object?> CreateRoomAsync(AdminRoomCreateDto dto)
    {
        var dept = await _dbContext.Departments.FindAsync(dto.DepartmentId);
        if (dept == null) return null;

        var clinic = new Clinic
        {
            Name = dto.RoomName.Trim(),
            DepartmentId = dto.DepartmentId,
            IsActive = true
        };

        _dbContext.Clinics.Add(clinic);
        await _dbContext.SaveChangesAsync();

        return new
        {
            roomId = clinic.ClinicId,
            roomName = clinic.Name,
            departmentId = clinic.DepartmentId,
            departmentName = dept.Name,
            isActive = clinic.IsActive,
            status = "Trống",
            assignedDoctorId = (int?)null,
            assignedDoctorName = "",
            assignedDoctorDepartment = ""
        };
    }

    public async Task<bool> UpdateRoomAsync(int roomId, AdminRoomUpdateDto dto)
    {
        var clinic = await _dbContext.Clinics.FindAsync(roomId);
        if (clinic == null) return false;

        clinic.Name = dto.RoomName.Trim();
        clinic.DepartmentId = dto.DepartmentId;
        clinic.IsActive = dto.IsActive;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteRoomAsync(int roomId)
    {
        var clinic = await _dbContext.Clinics.FindAsync(roomId);
        if (clinic == null) return false;

        // Kiểm tra có ca khám liên quan không
        var hasExams = await _dbContext.MedicalExaminations.AnyAsync(e => e.ClinicId == roomId);
        if (hasExams)
        {
            // Không xóa vật lý, chỉ vô hiệu hóa
            clinic.IsActive = false;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        _dbContext.Clinics.Remove(clinic);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
