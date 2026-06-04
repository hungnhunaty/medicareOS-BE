using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BE.Dtos.Queue;
using BE.Hubs;
using BE.Model;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BE.Services;

public class QueueService
{
    private readonly HospitalManagementDbContext _dbContext;
    private readonly IHubContext<QueueHub> _hubContext;
    private readonly IHubContext<QueueNotificationHub> _notificationHubContext;

    public QueueService(
        HospitalManagementDbContext dbContext, IHubContext<QueueHub> hubContext,
        IHubContext<QueueNotificationHub> notificationHubContext)
    {
        _dbContext = dbContext;
        _hubContext = hubContext;
        _notificationHubContext = notificationHubContext;
    }

    public async Task<QueueResponseDto?> AddToQueueAsync(QueueRequestDto dto)
    {

        Patient? patient = null;

        if (dto.PatientId.HasValue && dto.PatientId.Value > 0)
        {
            patient = await _dbContext.Patients.Include(p => p.User).FirstOrDefaultAsync(p => p.UserId == dto.PatientId.Value);
        }

        // Nếu không có PatientId hoặc không tìm thấy, tạo mới hoặc tìm theo tên/SĐT
        if (patient == null)
        {
            if (string.IsNullOrEmpty(dto.FullName) || string.IsNullOrEmpty(dto.Phone))
            {
                Console.WriteLine($"[QueueService] Missing patient info and no patient record. PatientId={dto.PatientId}, FullName={dto.FullName}, Phone={dto.Phone}");
                return null;
            }

            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Phone == dto.Phone || u.FullName == dto.FullName);
            
            if (existingUser != null)
            {
                patient = await _dbContext.Patients.Include(p => p.User).FirstOrDefaultAsync(p => p.UserId == existingUser.UserId);
                if (patient == null)
                {
                    patient = new Patient
                    {
                        UserId = existingUser.UserId,
                        PatientCode = "BN-" + DateTime.UtcNow.ToString("yyMMdd") + existingUser.UserId.ToString().PadLeft(4, '0')
                    };
                    _dbContext.Patients.Add(patient);
                    await _dbContext.SaveChangesAsync();
                }
            }
            else
            {
                var user = new User
                {
                    FullName = dto.FullName,
                    Phone = dto.Phone,
                    Gender = dto.Gender ?? "Khác",
                    Address = dto.Address,
                    UserName = null,
                    Password = null,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                patient = new Patient
                {
                    UserId = user.UserId,
                    PatientCode = "BN-" + DateTime.UtcNow.ToString("yyMMdd") + user.UserId.ToString().PadLeft(4, '0')
                };
                _dbContext.Patients.Add(patient);
                await _dbContext.SaveChangesAsync();
            }
        }

        // Kiểm tra xem bệnh nhân đã có trong hàng đợi nào chưa
        if (patient != null)
        {
            bool hasActiveQueue = await _dbContext.Queues.AnyAsync(q => q.PatientId == patient.UserId && (q.Status == 0 || q.Status == 1));
            if (hasActiveQueue)
            {
                throw new InvalidOperationException("Bệnh nhân này đang có trong một hàng đợi khác và chưa khám xong.");
            }
        }

        var today = DateTime.UtcNow.Date;

        // 1. Xác định Doctor
        Doctor targetDoctor = null;
        if (dto.DoctorId.HasValue && dto.DoctorId.Value > 0)
        {
            targetDoctor = await _dbContext.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == dto.DoctorId.Value);
        }
        else
        {
            // Tìm bác sĩ rảnh nhất trong Khoa
            var doctorsInDept = await _dbContext.Doctors
                .Include(d => d.User)
                .Where(d => d.User.DepartmentId == dto.DepartmentId)
                .ToListAsync();

            if (doctorsInDept.Any())
            {
                var doctorWaitCounts = await _dbContext.MedicalExaminations
                    .Where(m => doctorsInDept.Select(d => d.UserId).Contains(m.DoctorId) && (m.Status == 0 || m.Status == 1) && m.VisitDate >= today)
                    .GroupBy(m => m.DoctorId)
                    .Select(g => new { DoctorId = g.Key, Count = g.Count() })
                    .ToListAsync();

                var emptiestDoctorId = doctorsInDept
                    .Select(d => new {
                        d.UserId,
                        Count = doctorWaitCounts.FirstOrDefault(dc => dc.DoctorId == d.UserId)?.Count ?? 0
                    })
                    .OrderBy(d => d.Count)
                    .First().UserId;

                targetDoctor = doctorsInDept.First(d => d.UserId == emptiestDoctorId);
            }
        }

        if (targetDoctor == null)
        {
            Console.WriteLine($"[QueueService] No target doctor for department {dto.DepartmentId} and doctorId {dto.DoctorId}");
            return null;
        }

        // 2. Xác định Clinic (Phòng khám) cho Bác sĩ (Khóa phòng)
        var targetClinicId = 0;
        
        // Kiểm tra hôm nay bác sĩ này đã ngồi phòng nào chưa
        var latestExamToday = await _dbContext.MedicalExaminations
            .Where(m => m.DoctorId == targetDoctor.UserId && m.VisitDate >= today)
            .OrderByDescending(m => m.VisitDate)
            .FirstOrDefaultAsync();

        if (latestExamToday != null && latestExamToday.ClinicId.HasValue)
        {
            targetClinicId = latestExamToday.ClinicId.Value;
        }
        else
        {
            // Tìm phòng khám trống nhất trong khoa
            var clinics = await _dbContext.Clinics
                .Where(c => c.DepartmentId == dto.DepartmentId && c.IsActive)
                .ToListAsync();

            if (!clinics.Any())
            {
                Console.WriteLine($"[QueueService] No active clinics found for department {dto.DepartmentId}");
                return null;
            }

            var clinicWaitCounts = await _dbContext.Queues
                .Where(q => clinics.Select(c => c.ClinicId).Contains(q.ClinicId) && (q.Status == 0 || q.Status == 1) && q.CreatedAt >= today)
                .GroupBy(q => q.ClinicId)
                .Select(g => new { ClinicId = g.Key, Count = g.Count() })
                .ToListAsync();

            targetClinicId = clinics
                .Select(c => new { 
                    c.ClinicId, 
                    Count = clinicWaitCounts.FirstOrDefault(cc => cc.ClinicId == c.ClinicId)?.Count ?? 0 
                })
                .OrderBy(c => c.Count)
                .First().ClinicId;
        }

        // 3. Tính toán số thứ tự hàng đợi cho phòng khám đó
        var currentQueueCount = await _dbContext.Queues
            .Where(q => q.ClinicId == targetClinicId && q.CreatedAt >= today)
            .CountAsync();

        // Tạo bản ghi hàng đợi
        var queueEntry = new Queue
        {
            ClinicId = targetClinicId,
            PatientId = patient.UserId,
            QueueNumber = currentQueueCount + 1,
            Status = 0, // Chờ
            Priority = 0, // Thường
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Queues.Add(queueEntry);
        await _dbContext.SaveChangesAsync();
        // Tạo MedicalExamination
        var exam = new MedicalExamination
        {
            PatientId = patient.UserId,
            DoctorId = targetDoctor.UserId,
            ClinicId = targetClinicId,
            VisitDate = DateTime.UtcNow,
            Symptoms = dto.Symptoms,
            Status = 0 // Chờ khám
        };
        _dbContext.MedicalExaminations.Add(exam);
        await _dbContext.SaveChangesAsync();
        
        queueEntry.MedicalExaminationId = exam.MedicalExaminationId;
        await _dbContext.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("QueueUpdated");
        await _notificationHubContext.Clients.All.SendAsync("QueueUpdated");

        var targetClinicName = await _dbContext.Clinics
            .Where(c => c.ClinicId == targetClinicId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync() ?? "Phòng khám";

        var groupName = GetUserGroupName(patient.UserId);
        Console.WriteLine($"[QueueService] Sending QueueAssigned to group: {groupName}");

        // Gửi thông báo real-time cho bệnh nhân qua SignalR
        var notificationPayload = new
        {
            mess = "Bạn đã được thêm vào hàng đợi!",
            patientName = patient.User.FullName,
            queueNumber = queueEntry.QueueNumber
        };

        await _notificationHubContext.Clients.Group(groupName)
            .SendAsync("QueueAssigned", notificationPayload);

        return new QueueResponseDto
        {
            ExamId = queueEntry.MedicalExaminationId ?? 0,
            PatientName = patient.User.FullName,
            PatientCode = patient.PatientCode,
            ClinicName = targetClinicName,
            QueueNumber = queueEntry.QueueNumber,
            WaitingCount = currentQueueCount,
            Status = queueEntry.Status
        };
    }

    private static string GetUserGroupName(int userId)
    {
        return $"user-{userId}";
    }

    public async Task<object> GetClinicQueuesAsync(int departmentId)
    {
        var clinics = await _dbContext.Clinics
            .Where(c => c.DepartmentId == departmentId && c.IsActive)
            .Select(c => new
            {
                c.ClinicId,
                c.Name,
                waiting = _dbContext.Queues.Count(q => q.ClinicId == c.ClinicId && q.Status == 0),
                current = _dbContext.Queues
                    .Where(q => q.ClinicId == c.ClinicId && q.Status == 1)
                    .Select(q => q.Patient.User.FullName)
                    .FirstOrDefault() ?? "Trống"
            })
            .ToListAsync();

        return clinics;
    }

    public async Task<QueueResponseDto?> GetPatientQueueAsync(int patientUserId)
    {
        var queue = await _dbContext.Queues
            .Include(q => q.Clinic)
            .Include(q => q.Patient)
            .ThenInclude(p => p.User)
            .Where(q => q.PatientId == patientUserId && (q.Status == 0 || q.Status == 1))
            .OrderByDescending(q => q.CreatedAt)
            .FirstOrDefaultAsync();

        if (queue == null)
            return null;

        var today = DateTime.UtcNow.Date;
        var waitingCount = await _dbContext.Queues
            .Where(q => q.ClinicId == queue.ClinicId && q.CreatedAt >= today && q.QueueNumber < queue.QueueNumber && q.Status == 0)
            .CountAsync();

        return new QueueResponseDto
        {
            ExamId = queue.MedicalExaminationId ?? 0,
            PatientName = queue.Patient.User.FullName,
            PatientCode = queue.Patient.PatientCode,
            ClinicName = queue.Clinic.Name,
            QueueNumber = queue.QueueNumber,
            WaitingCount = waitingCount,
            Status = queue.Status
        };
    }
}
