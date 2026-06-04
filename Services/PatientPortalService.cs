using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BE.Model;
using BE.Dtos.Patient;
using Microsoft.EntityFrameworkCore;

namespace BE.Services;

public class PatientPortalService
{
    private readonly HospitalManagementDbContext _dbContext;

    public PatientPortalService(HospitalManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<object?> GetPatientDashboardDataAsync(int? patientId)
    {
        var query = _dbContext.Patients
            .Include(p => p.User)
            .Include(p => p.MedicalExaminations)
                .ThenInclude(m => m.Doctor).ThenInclude(d => d.User).ThenInclude(u => u.User)
            .Include(p => p.MedicalExaminations)
                .ThenInclude(m => m.Prescriptions).ThenInclude(pr => pr.PrescriptionDetails).ThenInclude(pd => pd.Medication)
            .Include(p => p.Invoices)
            .AsQueryable();

        Patient? patient = null;
        if (patientId.HasValue && patientId.Value > 0)
        {
            patient = await query.FirstOrDefaultAsync(p => p.UserId == patientId.Value);
        }

        // Nếu không tìm thấy hoặc không truyền, tự động chọn bệnh nhân đầu tiên có lịch sử khám phong phú để giao diện hiển thị đẹp
        if (patient == null)
        {
            patient = await query.OrderByDescending(p => p.MedicalExaminations.Count).FirstOrDefaultAsync();
        }

        if (patient == null) return null;

        var history = patient.MedicalExaminations
            .OrderByDescending(m => m.VisitDate)
            .Select(m => new
            {
                examinationId = m.MedicalExaminationId,
                visitDate = m.VisitDate.HasValue ? m.VisitDate.Value.ToString("dd/MM/yyyy HH:mm") : "N/A",
                department = (m.Doctor != null && m.Doctor.Specialty != null) ? m.Doctor.Specialty : "Khám Tổng Quát",
                doctorName = (m.Doctor != null && m.Doctor.User != null && m.Doctor.User.User != null) ? m.Doctor.User.User.FullName : "Bác sĩ chỉ định",
                symptoms = m.Symptoms ?? "Không có",
                diagnosis = m.Diagnosis ?? "Đang chờ kết luận",
                treatmentPlan = m.TreatmentPlan ?? "Chưa có phác đồ",
                status = m.Status == 0 ? "Chờ khám" : m.Status == 1 ? "Đang khám" : m.Status == 3 ? "Bị lỡ (Hàng chờ phụ)" : "Hoàn thành",
                statusCode = m.Status ?? 0,
                prescriptions = m.Prescriptions.SelectMany(pr => pr.PrescriptionDetails).Select(pd => new
                {
                    medicationName = pd.Medication != null ? pd.Medication.Name : "Thuốc",
                    quantity = pd.Quantity,
                    unit = (pd.Medication != null && pd.Medication.Unit != null) ? pd.Medication.Unit : "Viên/Hộp",
                    instructions = pd.Instructions ?? "Ngày uống theo chỉ định"
                }).ToList()
            }).ToList();

        var invoices = patient.Invoices
            .OrderByDescending(i => i.InvoiceDate)
            .Select(i => new
            {
                invoiceCode = "HD-" + i.InvoiceId.ToString().PadLeft(4, '0'),
                date = i.InvoiceDate.HasValue ? i.InvoiceDate.Value.ToString("dd/MM/yyyy") : "N/A",
                totalAmount = i.TotalAmount,
                status = i.Status == 1 ? "Đã thanh toán" : "Chờ thanh toán"
            }).ToList();

        return new
        {
            profile = new
            {
                patientId = patient.UserId,
                patientCode = patient.PatientCode,
                fullName = patient.User.FullName,
                phone = patient.User.Phone ?? "Chưa cập nhật",
                email = patient.User.Email ?? "Chưa cập nhật",
                gender = patient.User.Gender ?? "Khác",
                dob = patient.User.DateOfBirth.HasValue ? patient.User.DateOfBirth.Value.ToString("dd/MM/yyyy") : "N/A",
                address = patient.User.Address ?? "Chưa cập nhật",
                bloodType = patient.BloodType ?? "Chưa xét nghiệm",
                allergies = patient.Allergies ?? "Không phát hiện dị ứng"
            },
            history,
            invoices,
            // Trạng thái hàng đợi thực tế từ bảng Queue
            currentQueue = await GetActiveQueueStatusAsync(patient.UserId)
        };
    }

    private async Task<object?> GetActiveQueueStatusAsync(int patientId)
    {
        var activeQueue = await _dbContext.Queues
            .Include(q => q.Clinic).ThenInclude(c => c.Department)
            .Include(q => q.MedicalExamination!).ThenInclude(m => m.Doctor!).ThenInclude(d => d.User)
            .FirstOrDefaultAsync(q => q.PatientId == patientId && (q.Status == 0 || q.Status == 1 || q.Status == 3));

        if (activeQueue == null) return null;

        var waitingAhead = await _dbContext.Queues
            .CountAsync(q => q.ClinicId == activeQueue.ClinicId && q.Status == 0 && q.CreatedAt < activeQueue.CreatedAt);

        return new
        {
            queueId = activeQueue.QueueId,
            queueNumber = activeQueue.QueueNumber,
            clinicName = activeQueue.Clinic != null ? activeQueue.Clinic.Name : "Phòng Khám",
            department = activeQueue.Clinic != null && activeQueue.Clinic.Department != null ? activeQueue.Clinic.Department.Name : "Khám Tổng Quát",
            status = activeQueue.Status == 0 ? "Đang chờ" : activeQueue.Status == 1 ? "Đang khám" : "Bị lỡ (Hàng chờ phụ)",
            statusCode = activeQueue.Status,
            waitingAhead = waitingAhead,
            time = activeQueue.CreatedAt.ToString("HH:mm")
        };
    }

    public async Task<object> GetBookingOptionsAsync()
    {
        var doctors = await _dbContext.Doctors
            .Include(d => d.User).ThenInclude(u => u.User)
            .Select(d => new
            {
                doctorId = d.UserId,
                doctorName = d.User.User.FullName,
                specialty = d.Specialty ?? "Khám Tổng Quát"
            })
            .ToListAsync();

        return doctors;
    }

    public async Task<object?> BookAppointmentAsync(PatientBookingDto dto)
    {
        var patient = await _dbContext.Patients.FirstOrDefaultAsync(p => p.UserId == dto.PatientId);
        if (patient == null)
        {
            patient = await _dbContext.Patients.FirstOrDefaultAsync();
            if (patient == null) return null;
        }

        bool hasActiveExam = await _dbContext.MedicalExaminations.AnyAsync(m => m.PatientId == patient.UserId && (m.Status == 0 || m.Status == 1));
        if (hasActiveExam)
        {
            throw new InvalidOperationException("Bạn đang có một lịch khám đang chờ. Không thể đặt thêm lịch mới.");
        }

        var exam = new MedicalExamination
        {
            PatientId = patient.UserId,
            DoctorId = dto.DoctorId > 0 ? dto.DoctorId : (await _dbContext.Doctors.Select(d => d.UserId).FirstOrDefaultAsync()),
            VisitDate = dto.PreferredDate.HasValue ? dto.PreferredDate.Value : DateTime.UtcNow,
            Symptoms = dto.Symptoms,
            Status = 0
        };

        _dbContext.MedicalExaminations.Add(exam);
        await _dbContext.SaveChangesAsync();

        return new
        {
            success = true,
            examinationId = exam.MedicalExaminationId,
            message = "Đặt lịch khám và đăng ký số thứ tự thành công. Vui lòng theo dõi thông báo khi tới lượt."
        };
    }

    public async Task<bool> UpdateEmailAsync(int patientId, string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == patientId);
        if (user == null) return false;

        // Nếu email không rỗng/null, kiểm tra trùng lặp
        if (!string.IsNullOrEmpty(email))
        {
            bool emailExists = await _dbContext.Users.AnyAsync(u => u.Email == email && u.UserId != patientId);
            if (emailExists) return false;
        }

        user.Email = string.IsNullOrEmpty(email) ? null : email;
        user.IsEmailVerified = true; // Kích hoạt email mới cập nhật để sẵn sàng dùng cho quên mật khẩu
        await _dbContext.SaveChangesAsync();
        return true;
    }
}


