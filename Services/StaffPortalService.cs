using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BE.Model;
using BE.Dtos.Staff;
using Microsoft.EntityFrameworkCore;

namespace BE.Services;

public class StaffPortalService
{
    private readonly HospitalManagementDbContext _dbContext;
    private readonly QueueService _queueService;

    public StaffPortalService(HospitalManagementDbContext dbContext, QueueService queueService)
    {
        _dbContext = dbContext;
        _queueService = queueService;
    }

    public async Task<object> GetReceptionQueuesAsync()
    {
        var list = await _dbContext.Queues
            .Include(q => q.Patient).ThenInclude(p => p.User)
            .Include(q => q.Clinic).ThenInclude(c => c.Department)
            .Where(q => q.Status == 0 || q.Status == 1)
            .OrderByDescending(q => q.CreatedAt)
            .Take(50)
            .Select(q => new
            {
                queueId = q.QueueId,
                patientCode = q.Patient.PatientCode,
                patientName = q.Patient.User.FullName,
                phone = q.Patient.User.Phone ?? "N/A",
                gender = q.Patient.User.Gender ?? "Nam",
                time = q.CreatedAt.ToString("HH:mm dd/MM/yyyy"),
                department = q.Clinic.Department.Name,
                clinicName = q.Clinic.Name,
                queueNumber = q.QueueNumber,
                status = q.Status == 0 ? "Chờ khám" : q.Status == 1 ? "Đang khám" : "Hoàn thành"
            })
            .ToListAsync();

        return list;
    }

    public async Task<object> GetRoutingOptionsAsync()
    {
        var today = DateTime.UtcNow.Date;
        
        var doctors = await _dbContext.Doctors
            .Include(d => d.User).ThenInclude(u => u.User)
            .Include(d => d.User).ThenInclude(u => u.Department)
            .Select(d => new
            {
                doctorId = d.UserId,
                doctorName = d.User.User.FullName,
                department = d.User.Department.Name,
                latestExamClinic = d.MedicalExaminations
                    .Where(m => m.VisitDate >= today)
                    .OrderByDescending(m => m.VisitDate)
                    .Select(m => m.Clinic.Name)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return doctors.Select(d => new
        {
            doctorId = d.doctorId,
            doctorName = d.doctorName,
            department = d.department,
            clinicName = d.latestExamClinic
        });
    }

    public async Task<object?> RegisterPatientQueueAsync(StaffRegisterQueueDto dto)
    {
        // 1. Tìm DepartmentId từ tên khoa
        var department = await _dbContext.Departments
            .FirstOrDefaultAsync(d => d.Name == dto.Department);
        
        if (department == null) return null;

        // 2. Gọi QueueService để thực hiện logic hàng đợi thông minh
        var queueRequest = new BE.Dtos.Queue.QueueRequestDto
        {
            DepartmentId = department.DepartmentId,
            PatientId = dto.PatientId,
            FullName = dto.PatientName,
            Phone = dto.Phone,
            Gender = dto.Gender,
            Symptoms = dto.Symptoms,
            DoctorId = dto.DoctorId > 0 ? dto.DoctorId : null
        };

        var result = await _queueService.AddToQueueAsync(queueRequest);

        if (result == null) return null;

        return new
        {
            success = true,
            examinationId = result.ExamId,
            patientName = result.PatientName,
            patientCode = result.PatientCode,
            clinicName = result.ClinicName,
            queueNumber = result.QueueNumber,
            status = "Chờ khám"
        };
    }

    public async Task<object?> GetPatientByIdAsync(int userId)
    {
        var patient = await _dbContext.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (patient == null) return null;

        return new
        {
            patientId = patient.UserId,
            patientCode = patient.PatientCode,
            fullName = patient.User.FullName,
            phone = patient.User.Phone ?? "N/A",
            gender = patient.User.Gender ?? "Nam"
        };
    }

    public async Task<object> GetPendingInvoicesAsync()
    {
        var invoices = await _dbContext.Invoices
            .Include(i => i.Patient).ThenInclude(p => p.User)
            .Include(i => i.InvoiceDetails)
            .OrderByDescending(i => i.InvoiceDate)
            .Select(i => new
            {
                invoiceId = i.InvoiceId,
                invoiceCode = "HD-" + i.InvoiceId.ToString().PadLeft(4, '0'),
                patientName = i.Patient.User.FullName,
                patientCode = i.Patient.PatientCode,
                date = i.InvoiceDate.HasValue ? i.InvoiceDate.Value.ToString("dd/MM/yyyy HH:mm") : "N/A",
                totalAmount = i.TotalAmount,
                paymentMethod = i.PaymentMethod ?? "Tiền mặt",
                status = i.Status == 1 ? "Đã thanh toán" : "Chờ thanh toán",
                items = i.InvoiceDetails.Select(d => new
                {
                    itemName = d.ItemName,
                    quantity = d.Quantity,
                    unitPrice = d.UnitPrice,
                    total = d.TotalAmount
                }).ToList()
            })
            .ToListAsync();

        return invoices;
    }

    public async Task<bool> ProcessPaymentAsync(int invoiceId, string method)
    {
        var invoice = await _dbContext.Invoices.FindAsync(invoiceId);
        if (invoice == null) return false;

        invoice.Status = 1; // Đã thanh toán
        invoice.PaymentMethod = method;
        await _dbContext.SaveChangesAsync();
        return true;
    }
}


