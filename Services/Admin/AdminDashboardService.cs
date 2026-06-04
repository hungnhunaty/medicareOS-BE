using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BE.Model;
using BE.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BE.Services;

public class AdminDashboardService
{
    private readonly HospitalManagementDbContext _dbContext;
    private readonly IHubContext<QueueHub> _hubContext;
    private readonly IHubContext<QueueNotificationHub> _notificationHubContext;

    public AdminDashboardService(HospitalManagementDbContext dbContext, IHubContext<QueueHub> hubContext, IHubContext<QueueNotificationHub> notificationHubContext)
    {
        _dbContext = dbContext;
        _hubContext = hubContext;
        _notificationHubContext = notificationHubContext;
    }

    public async Task<object> GetDashboardStatsAsync()
    {
        var today = DateTime.UtcNow.Date;

        var totalPatients = await _dbContext.Patients.CountAsync();
        var waitingExams = await _dbContext.Queues.CountAsync(q => q.Status == 0);
        var completedExamsToday = await _dbContext.Queues
            .CountAsync(q => q.Status == 2 && q.CreatedAt >= today);

        var totalRevenueToday = await _dbContext.Invoices
            .Where(i => i.Status == 1 && i.InvoiceDate >= today)
            .SumAsync(i => i.TotalAmount);

        // Lấy thống kê công suất thực tế theo 6 khoa (mỗi khoa có 3 phòng khám)
        var depts = await _dbContext.Departments.ToListAsync();
        var deptStats = new List<object>();

        foreach (var d in depts)
        {
            var clinics = await _dbContext.Clinics
                .Where(c => c.DepartmentId == d.DepartmentId && c.IsActive)
                .Select(c => new
                {
                    clinicId = c.ClinicId,
                    name = c.Name,
                    waiting = _dbContext.Queues.Count(q => q.ClinicId == c.ClinicId && q.Status == 0),
                    current = _dbContext.Queues
                        .Where(q => q.ClinicId == c.ClinicId && q.Status == 1)
                        .Select(q => q.Patient.User.FullName)
                        .FirstOrDefault() ?? "Trống"
                })
                .ToListAsync();

            var totalWaiting = clinics.Sum(c => c.waiting);

            deptStats.Add(new
            {
                name = d.Name,
                departmentId = d.DepartmentId,
                capacity = Math.Min(100, 15 + (totalWaiting * 25)), // Công suất thực tế tăng theo số ca chờ
                count = totalWaiting,
                clinics = clinics
            });
        }

        // Cảnh báo thuốc sắp hết
        var lowStockMeds = await _dbContext.Medications
            .Where(m => m.Quantity < 300)
            .Select(m => new { m.Name, m.Quantity, m.Unit })
            .ToListAsync();

        return new
        {
            totalPatients,
            waitingExams,
            completedExamsToday,
            totalRevenueToday,
            departmentStats = deptStats,
            lowStockAlerts = lowStockMeds
        };
    }

    public async Task<object> GetActiveQueuesAsync()
    {
        var list = await _dbContext.Queues
            .Include(q => q.Patient).ThenInclude(p => p.User)
            .Include(q => q.Clinic).ThenInclude(c => c.Department)
            .Include(q => q.MedicalExamination!).ThenInclude(m => m.Doctor!).ThenInclude(d => d.User).ThenInclude(u => u.User)
            .Where(q => q.Status == 0 || q.Status == 1)
            .OrderByDescending(q => q.CreatedAt)
            .Take(20)
            .Select(q => new
            {
                examinationId = q.MedicalExaminationId,
                patientName = q.Patient.User != null ? q.Patient.User.FullName : "Khách vãng lai",
                patientCode = q.Patient.PatientCode,
                time = q.CreatedAt.ToString("HH:mm"),
                department = q.Clinic.Department != null ? q.Clinic.Department.Name : "Khám Tổng Quát",
                doctorName = (q.MedicalExamination != null && q.MedicalExamination.Doctor != null && q.MedicalExamination.Doctor.User != null && q.MedicalExamination.Doctor.User.User != null) ? q.MedicalExamination.Doctor.User.User.FullName : "Tự động điều phối",
                status = q.Status == 0 ? "Đang chờ" : q.Status == 1 ? "Đang khám" : "Hoàn thành",
                queueNumber = q.QueueNumber
            })
            .ToListAsync();

        return list;
    }

    public async Task<bool> UpdateQueueStatusAsync(int examId, string statusStr)
    {
        var exam = await _dbContext.MedicalExaminations.FindAsync(examId);
        if (exam != null)
        {
            int newStatus = (statusStr == "Chờ khám" || statusStr == "Đang chờ") ? 0 : statusStr == "Đang khám" ? 1 : 2;
            exam.Status = newStatus;
        }

        var queue = await _dbContext.Queues.FirstOrDefaultAsync(q => q.MedicalExaminationId == examId);
        if (queue != null)
        {
            int newStatus = (statusStr == "Chờ khám" || statusStr == "Đang chờ") ? 0 : statusStr == "Đang khám" ? 1 : 2;
            queue.Status = newStatus;
        }

        await _dbContext.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("QueueUpdated");
        await _notificationHubContext.Clients.All.SendAsync("QueueUpdated");

        return true;
    }

    public async Task<object?> CreateEmergencyExamAsync(string patientName, string phone, string symptoms)
    {
        // Tìm user hoặc tạo mới
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Phone == phone || u.FullName == patientName);
        Patient? patient = null;

        if (user == null)
        {
            user = new User
            {
                FullName = patientName,
                Phone = phone,
                UserName = "EMG_" + DateTime.UtcNow.Ticks.ToString().Substring(10),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            patient = new Patient
            {
                UserId = user.UserId,
                PatientCode = "EMG-" + DateTime.UtcNow.ToString("yyMMddHHmm")
            };
            _dbContext.Patients.Add(patient);
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            patient = await _dbContext.Patients.FirstOrDefaultAsync(p => p.UserId == user.UserId);
            if (patient == null)
            {
                patient = new Patient
                {
                    UserId = user.UserId,
                    PatientCode = "EMG-" + DateTime.UtcNow.ToString("yyMMddHHmm")
                };
                _dbContext.Patients.Add(patient);
                await _dbContext.SaveChangesAsync();
            }
        }

        var doc = await _dbContext.Doctors.FirstOrDefaultAsync();
        if (doc == null) return null;

        var exam = new MedicalExamination
        {
            PatientId = patient.UserId,
            DoctorId = doc.UserId,
            VisitDate = DateTime.UtcNow,
            Symptoms = symptoms,
            Status = 0 // Chờ khám
        };

        _dbContext.MedicalExaminations.Add(exam);
        await _dbContext.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("QueueUpdated");
        await _notificationHubContext.Clients.All.SendAsync("QueueUpdated");


        return new
        {
            examId = exam.MedicalExaminationId,
            patientName = user.FullName,
            status = "Chờ khám"
        };
    }
}
