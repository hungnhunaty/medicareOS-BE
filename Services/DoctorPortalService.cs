using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BE.Model;
using BE.Dtos.Doctor;
using BE.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace BE.Services;

public class DoctorPortalService
{
    private readonly HospitalManagementDbContext _dbContext;
    private readonly IHubContext<QueueHub> _hubContext;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<QueueNotificationHub> _notificationHub;
    private static readonly HttpClient _httpClient = new HttpClient();

    public DoctorPortalService(HospitalManagementDbContext dbContext, IHubContext<QueueHub> hubContext, IConfiguration configuration, IHubContext<QueueNotificationHub> notificationHub)
    {
        _dbContext = dbContext;
        _hubContext = hubContext;
        _configuration = configuration;
        _notificationHub = notificationHub;
    }

    public async Task<object> GetAssignedExaminationsAsync(int? doctorId)
    {
        var query = _dbContext.MedicalExaminations
            .Include(m => m.Patient).ThenInclude(p => p.User)
            .Include(m => m.Prescriptions).ThenInclude(pr => pr.PrescriptionDetails).ThenInclude(pd => pd.Medication)
            .AsQueryable();

        if (doctorId.HasValue && doctorId.Value > 0)
        {
            query = query.Where(m => m.DoctorId == doctorId.Value);
        }

        var exams = await query
            .OrderBy(m => m.Status == 2 ? 1 : 0) // Chưa khám/đang khám lên trước
            .ThenBy(m => m.VisitDate)
            .Select(m => new
            {
                examinationId = m.MedicalExaminationId,
                patientCode = m.Patient.PatientCode,
                patientName = m.Patient.User.FullName,
                gender = m.Patient.User.Gender ?? "Khác",
                dob = m.Patient.User.DateOfBirth.HasValue ? m.Patient.User.DateOfBirth.Value.ToString("dd/MM/yyyy") : "N/A",
                visitDate = m.VisitDate.HasValue ? m.VisitDate.Value.ToString("dd/MM/yyyy HH:mm") : "N/A",
                symptoms = m.Symptoms ?? "Không rõ triệu chứng",
                allergies = m.Patient.Allergies ?? "Không có",
                bloodType = m.Patient.BloodType ?? "Chưa rõ",
                familyMedicalHistory = m.Patient.FamilyMedicalHistory ?? "Không có bệnh lý nền",
                diagnosis = m.Diagnosis ?? "",
                treatmentPlan = m.TreatmentPlan ?? "",
                status = m.Status ?? 0, // 0: Chờ khám, 1: Đang khám, 2: Đã xong, 3: Bị lỡ
                callCount = m.CallCount ?? 0,
                medications = m.Prescriptions.SelectMany(pr => pr.PrescriptionDetails).Select(pd => new
                {
                    medicationId = pd.MedicationId,
                    name = pd.Medication.Name,
                    quantity = pd.Quantity,
                    instructions = pd.Instructions ?? ""
                }).ToList()
            })
            .ToListAsync();

        return exams;
    }

    public async Task<bool> StartExaminationAsync(int examinationId)
    {
        var exam = await _dbContext.MedicalExaminations.FindAsync(examinationId);
        if (exam == null) return false;

        exam.Status = 1; // Đang khám
        
        var queue = await _dbContext.Queues.FirstOrDefaultAsync(q => q.MedicalExaminationId == examinationId);
        if (queue != null) queue.Status = 1;

        await _dbContext.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("QueueUpdated");
        await _notificationHub.Clients.All.SendAsync("QueueUpdated");
        return true;
    }

    public async Task<object?> UpdateDiagnosisAsync(int examinationId, DoctorDiagnosisDto dto)
    {
        var exam = await _dbContext.MedicalExaminations
            .Include(m => m.Prescriptions).ThenInclude(pr => pr.PrescriptionDetails)
            .FirstOrDefaultAsync(m => m.MedicalExaminationId == examinationId);

        if (exam == null) return null;

        exam.Diagnosis = dto.Diagnosis;
        exam.TreatmentPlan = dto.TreatmentPlan;
        exam.Status = dto.Status; // Thường là 2 (Đã hoàn thành) khi lưu kết luận

        // Cập nhật trạng thái hàng chờ tương ứng nếu có
        var queue = await _dbContext.Queues.FirstOrDefaultAsync(q => q.MedicalExaminationId == exam.MedicalExaminationId);
        if (queue != null)
        {
            queue.Status = dto.Status;
        }

        // Xử lý kê đơn thuốc nếu có
        if (dto.Medications != null && dto.Medications.Any())
        {
            var prescription = exam.Prescriptions.FirstOrDefault();
            if (prescription == null)
            {
                prescription = new Prescription
                {
                    MedicalExaminationId = exam.MedicalExaminationId,
                    Date = DateTime.UtcNow,
                    Status = 1
                };
                _dbContext.Prescriptions.Add(prescription);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                // Xóa chi tiết cũ để nạp lại
                _dbContext.PrescriptionDetails.RemoveRange(prescription.PrescriptionDetails);
                await _dbContext.SaveChangesAsync();
            }

            foreach (var item in dto.Medications)
            {
                var med = await _dbContext.Medications.FindAsync(item.MedicationId);
                decimal price = med != null ? med.CurentPrice : 0;

                var detail = new PrescriptionDetail
                {
                    PrescriptionId = prescription.PrescriptionsId,
                    MedicationId = item.MedicationId,
                    Quantity = item.Quantity > 0 ? item.Quantity : 1,
                    Instructions = item.Instructions,
                    Price = price
                };
                _dbContext.PrescriptionDetails.Add(detail);

                // Trừ kho nếu cần
                if (med != null && med.Quantity.HasValue)
                {
                    med.Quantity = Math.Max(0, med.Quantity.Value - detail.Quantity);
                }
            }
        }

        await _dbContext.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("QueueUpdated");
        await _notificationHub.Clients.All.SendAsync("QueueUpdated");

        return new { success = true, message = "Cập nhật kết luận và đơn thuốc thành công." };
    }

    private static string GetUserGroupName(int userId)
    {
        return $"user-{userId}";
    }

    public async Task<object?> CallNextPatientAsync(int doctorId)
    {
        // 1. Kiểm tra xem bác sĩ có ca nào đang gọi dở (Status == 0 && CallCount > 0) hay không
        var activeCalledExam = await _dbContext.MedicalExaminations
            .Include(m => m.Patient).ThenInclude(p => p.User)
            .Include(m => m.Clinic)
            .Where(m => (doctorId <= 0 || m.DoctorId == doctorId) && m.Status == 0 && m.CallCount > 0)
            .FirstOrDefaultAsync();

        MedicalExamination? nextExam = null;

        if (activeCalledExam != null)
        {
            // Bệnh nhân đang được gọi -> Tiếp tục gọi lại (recall) bệnh nhân này (không tự động chuyển hàng chờ phụ)
            activeCalledExam.CallCount++;
            nextExam = activeCalledExam;
        }
        else
        {
            // Không có bệnh nhân nào đang được gọi -> Tìm bệnh nhân mới
            nextExam = await FindNextPatientToCallAsync(doctorId);
        }

        if (nextExam == null) return null;

        // Cập nhật trạng thái hàng chờ của bệnh nhân được gọi thành 1 (Active)
        var queue = await _dbContext.Queues.FirstOrDefaultAsync(q => q.MedicalExaminationId == nextExam.MedicalExaminationId);

        var groupName = GetUserGroupName(nextExam.PatientId);

        await _dbContext.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("PatientCalled", nextExam.PatientId, nextExam.Clinic?.Name ?? "Phòng khám", queue?.QueueNumber ?? 0);
        await _hubContext.Clients.All.SendAsync("QueueUpdated");
        await _notificationHub.Clients.All.SendAsync("QueueUpdated");
        
        var payload = new
        {
            message = "Đã đến lượt khám của bạn!",
            clinicName = nextExam.Clinic?.Name ?? "Phòng khám",
            queueNumber = queue?.QueueNumber ?? 0
        };

        Console.WriteLine($"[DoctorPortal] Sending PatientCalled to group: {groupName}");
        
        await _notificationHub.Clients.Group(groupName)
            .SendAsync("PatientCalled", payload);

        return new
        {
            examinationId = nextExam.MedicalExaminationId,
            patientCode = nextExam.Patient.PatientCode,
            patientName = nextExam.Patient.User.FullName,
            email = nextExam.Patient.User.Email,
            status = nextExam.Status ?? 0,
            callCount = nextExam.CallCount ?? 0
        };
    }

    private async Task<MedicalExamination?> FindNextPatientToCallAsync(int doctorId)
    {
        // Ưu tiên 1: Hàng chờ chính (Status == 0 và chưa được gọi lần nào hoặc CallCount == null/0)
        var nextExam = await _dbContext.MedicalExaminations
            .Include(m => m.Patient).ThenInclude(p => p.User)
            .Include(m => m.Clinic)
            .Where(m => (doctorId <= 0 || m.DoctorId == doctorId) && m.Status == 0 && (m.CallCount == null || m.CallCount == 0))
            .OrderBy(m => m.VisitDate)
            .FirstOrDefaultAsync();

        if (nextExam != null)
        {
            nextExam.CallCount = 1;
            return nextExam;
        }

        // Ưu tiên 2: Hàng chờ phụ (Status == 3)
        var secondaryExam = await _dbContext.MedicalExaminations
            .Include(m => m.Patient).ThenInclude(p => p.User)
            .Include(m => m.Clinic)
            .Where(m => (doctorId <= 0 || m.DoctorId == doctorId) && m.Status == 3)
            .OrderBy(m => m.VisitDate)
            .FirstOrDefaultAsync();

        if (secondaryExam != null)
        {
            // Trở lại hàng chờ chính
            secondaryExam.Status = 0;
            secondaryExam.CallCount = 1;
            return secondaryExam;
        }

        return null;
    }

    public async Task<bool> RecallPatientAsync(int examinationId)
    {
        var exam = await _dbContext.MedicalExaminations
            .Include(m => m.Patient).ThenInclude(p => p.User)
            .Include(m => m.Clinic)
            .FirstOrDefaultAsync(m => m.MedicalExaminationId == examinationId);

        if (exam == null) return false;

        // Reset về hàng chờ chính
        exam.Status = 0;
        exam.CallCount = 1;

        var queue = await _dbContext.Queues.FirstOrDefaultAsync(q => q.MedicalExaminationId == examinationId);
        if (queue != null)
        {
            queue.Status = 1; // Active/Gọi
        }

        await _dbContext.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("PatientCalled", exam.PatientId, exam.Clinic?.Name ?? "Phòng khám", queue?.QueueNumber ?? 0);
        await _hubContext.Clients.All.SendAsync("QueueUpdated");
        await _notificationHub.Clients.All.SendAsync("QueueUpdated");

        var groupName = GetUserGroupName(exam.PatientId);
        var payload = new
        {
            message = "Đã đến lượt khám của bạn!",
            clinicName = exam.Clinic?.Name ?? "Phòng khám",
            queueNumber = queue?.QueueNumber ?? 0
        };

        await _notificationHub.Clients.Group(groupName)
            .SendAsync("PatientCalled", payload);

        return true;
    }

    public async Task<bool> SkipPatientAsync(int examinationId)
    {
        var exam = await _dbContext.MedicalExaminations.FirstOrDefaultAsync(m => m.MedicalExaminationId == examinationId);
        if (exam == null) return false;

        // Chuyển sang hàng chờ phụ (Status = 3)
        exam.Status = 3;

        var queue = await _dbContext.Queues.FirstOrDefaultAsync(q => q.MedicalExaminationId == examinationId);
        if (queue != null)
        {
            queue.Status = 3;
        }

        await _dbContext.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("QueueUpdated");
        await _notificationHub.Clients.All.SendAsync("QueueUpdated");
        return true;
    }

    public async Task<DoctorInformationResponse?> GetDoctorInformation(int doctorId)
    {
        var today = DateTime.UtcNow.Date;
        
        var doctor = await _dbContext.Doctors
            .Include(d => d.User).ThenInclude(s => s.User)
            .Include(d => d.User).ThenInclude(s => s.Department)
            .FirstOrDefaultAsync(d => d.UserId == doctorId);

        if (doctor == null) return null;

        var latestExamClinic = await _dbContext.MedicalExaminations
            .Where(m => m.DoctorId == doctorId && m.VisitDate >= today)
            .OrderByDescending(m => m.VisitDate)
            .Select(m => m.Clinic.Name)
            .FirstOrDefaultAsync();
        
        var doctorInfo = new DoctorInformationResponse
        {
            FullName = doctor.User.User.FullName,
            Email = doctor.User.User.Email ?? "",
            Phone = doctor.User.User.Phone ?? "",
            Department = doctor.User.Department.Name,
            Specialty = doctor.Specialty ?? "",
            BaseSalary = (double)doctor.User.BaseSalary,
            ClinicName = latestExamClinic
        };
        return doctorInfo;
    }

    public async Task<bool> UpdateDoctorInformationAsync(int doctorId, DoctorInformationRequest dto)
    {
        var doctor = await _dbContext.Doctors
            .Include(d => d.User).ThenInclude(s => s.User)
            .FirstOrDefaultAsync(d => d.UserId == doctorId);
        
        if (doctor == null) return false;

        doctor.User.User.FullName = dto.FullName;
        doctor.User.User.Email = dto.Email;
        doctor.User.User.Phone = dto.Phone;
        doctor.Specialty = dto.Specialty;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<object> CheckPrescriptionSafetyAsync(CheckPrescriptionSafetyDto dto)
    {
        var exam = await _dbContext.MedicalExaminations
            .Include(m => m.Patient).ThenInclude(p => p.User)
            .FirstOrDefaultAsync(m => m.MedicalExaminationId == dto.ExaminationId);

        if (exam == null)
        {
            return new { isSafe = true, warnings = new List<string>() };
        }

        var patient = exam.Patient;
        var allergies = patient.Allergies ?? "Không có";
        var bloodType = patient.BloodType ?? "Chưa rõ";
        var medicalHistory = patient.FamilyMedicalHistory ?? "Không có bệnh lý nền";

        var medsTextList = new List<string>();
        foreach (var m in dto.Medications)
        {
            var dbMed = await _dbContext.Medications.FindAsync(m.MedicationId);
            var ingredient = dbMed?.Ingredient ?? "Không rõ hoạt chất";
            var medName = m.Name ?? dbMed?.Name ?? "Không rõ tên";
            var medInstructions = m.Instructions ?? "Không có";
            medsTextList.Add($"- Tên thuốc: {medName} (Hoạt chất: {ingredient}), Số lượng: {m.Quantity}, Hướng dẫn: {medInstructions}");
        }
        var medsText = string.Join("\n", medsTextList);

        var apiKey = _configuration["GeminiApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_GEMINI_API_KEY")
        {
            var warnings = new List<string>();

            if (allergies.Contains("penicillin", StringComparison.OrdinalIgnoreCase))
            {
                var hasAmox = dto.Medications.Any(m => m.Name != null && (m.Name.Contains("amoxicillin", StringComparison.OrdinalIgnoreCase) || m.Name.Contains("penicillin", StringComparison.OrdinalIgnoreCase)));
                if (hasAmox)
                {
                    warnings.Add("CẢNH BÁO DỊ ỨNG: Bệnh nhân có tiền sử dị ứng Penicillin. Không được kê đơn Amoxicillin hoặc các thuốc kháng sinh nhóm Penicillin.");
                }
            }

            if (medicalHistory.Contains("suy gan", StringComparison.OrdinalIgnoreCase))
            {
                var paracetamol = dto.Medications.FirstOrDefault(m => m.Name != null && (m.Name.Contains("paracetamol", StringComparison.OrdinalIgnoreCase) || m.Name.Contains("hapacol", StringComparison.OrdinalIgnoreCase)));
                if (paracetamol != null && paracetamol.Quantity > 5)
                {
                    warnings.Add($"CẢNH BÁO BỆNH LÝ: Bệnh nhân bị Suy gan. Hạn chế sử dụng Paracetamol hoặc giảm liều lượng (Hiện tại đang kê {paracetamol.Quantity} viên).");
                }
            }

            if (medicalHistory.Contains("suy thận", StringComparison.OrdinalIgnoreCase))
            {
                var ibuprofen = dto.Medications.Any(m => m.Name != null && m.Name.Contains("ibuprofen", StringComparison.OrdinalIgnoreCase));
                if (ibuprofen)
                {
                    warnings.Add("CẢNH BÁO BỆNH LÝ: Bệnh nhân bị Suy thận. Tránh sử dụng các thuốc kháng viêm không steroid (NSAID) như Ibuprofen vì có thể làm suy giảm chức năng thận.");
                }
            }

            return new
            {
                isSafe = warnings.Count == 0,
                warnings = warnings
            };
        }

        try
        {
            var prompt = $@"Bạn là một chuyên gia Dược lâm sàng thông thái. Hãy kiểm tra xem đơn thuốc đang kê có an toàn cho bệnh nhân hay không dựa trên dị ứng, bệnh lý nền và nhóm máu của họ.

                Thông tin bệnh nhân:
                - Nhóm máu: {bloodType}
                - Dị ứng: {allergies}
                - Bệnh lý nền/Tiền sử bệnh: {medicalHistory}

                Đơn thuốc đang kê:
                {medsText}

                Hãy phân tích kỹ lưỡng tương tác thuốc-thuốc, thuốc-bệnh lý, và dị ứng. Trả về kết quả phân tích theo định dạng JSON chính xác như sau:
                {{
                ""isSafe"": true hoặc false (false nếu có nguy cơ/cảnh báo trung bình đến nghiêm trọng),
                ""warnings"": [""Danh sách các câu cảnh báo ngắn gọn, rõ ràng bằng tiếng Việt. Nếu an toàn thì mảng này rỗng""]
                }}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    responseMimeType = "application/json",
                    responseSchema = new
                    {
                        type = "OBJECT",
                        properties = new
                        {
                            isSafe = new { type = "BOOLEAN" },
                            warnings = new
                            {
                                type = "ARRAY",
                                items = new { type = "STRING" }
                            }
                        },
                        required = new[] { "isSafe", "warnings" }
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";
            var response = await _httpClient.PostAsync(url, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                throw new Exception($"Gemini API error: {response.StatusCode} - {errorText}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(text))
            {
                return new { isSafe = true, warnings = new List<string>() };
            }

            var result = JsonSerializer.Deserialize<GeminiSafetyResponse>(text, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return new
            {
                isSafe = result?.IsSafe ?? true,
                warnings = result?.Warnings ?? new List<string>()
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Prescription Safety AI Error]: {ex.Message}");
            return new
            {
                isSafe = true,
                warnings = new List<string> { $"[Hệ thống AI tạm thời gián đoạn]: {ex.Message}. Bác sĩ vui lòng tự kiểm tra lâm sàng." }
            };
        }
    }

    private class GeminiSafetyResponse
    {
        public bool IsSafe { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
    }
}


