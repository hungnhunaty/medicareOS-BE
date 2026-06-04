using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BE.Model;
using BE.Dtos.Admin;
using Microsoft.EntityFrameworkCore;

namespace BE.Services;

public class AdminFinanceService
{
    private readonly HospitalManagementDbContext _dbContext;

    public AdminFinanceService(HospitalManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<object> GetAllInvoicesAsync()
    {
        var invoices = await _dbContext.Invoices
            .Include(i => i.Patient).ThenInclude(p => p.User)
            .Include(i => i.InvoiceDetails)
            .OrderByDescending(i => i.InvoiceId)
            .Select(i => new
            {
                invoiceId = "HD-2605-" + i.InvoiceId.ToString().PadLeft(3, '0'),
                realId = i.InvoiceId,
                patientName = i.Patient.User.FullName,
                patientCode = i.Patient.PatientCode,
                date = i.InvoiceDate.HasValue ? i.InvoiceDate.Value.ToString("dd/MM/yyyy HH:mm") : "N/A",
                items = i.InvoiceDetails.Select(d => new
                {
                    name = d.ItemName,
                    quantity = d.Quantity,
                    unitPrice = d.UnitPrice
                }).ToList(),
                total = i.TotalAmount,
                method = string.IsNullOrEmpty(i.PaymentMethod) ? "Chưa chọn" : i.PaymentMethod,
                status = i.Status == 1 ? "Đã thanh toán" : i.Status == 2 ? "Đã hủy" : "Chờ thanh toán"
            })
            .ToListAsync();

        // Nếu danh sách trống, chèn một vài mock data chuẩn giao diện
        if (invoices.Count == 0)
        {
            var defaultList = new List<object>
            {
                new { 
                    invoiceId = "HD-2605-001", realId = 1, patientName = "Nguyễn Văn A", patientCode = "BN-10293",
                    date = "12/05/2026 08:30", total = 650000m, method = "Chuyển khoản", status = "Đã thanh toán",
                    items = new[] { new { name = "Khám chuyên khoa Nội", quantity = 1, unitPrice = 150000m }, new { name = "Xét nghiệm máu tổng quát", quantity = 1, unitPrice = 420000m }, new { name = "Paracetamol 500mg (Hộp)", quantity = 2, unitPrice = 40000m } }
                },
                new { 
                    invoiceId = "HD-2605-002", realId = 2, patientName = "Trần Thị B", patientCode = "BN-10294",
                    date = "12/05/2026 09:15", total = 1250000m, method = "Tiền mặt", status = "Đã thanh toán",
                    items = new[] { new { name = "Khám chuyên khoa Ngoại", quantity = 1, unitPrice = 150000m }, new { name = "Siêu âm ổ bụng 4D", quantity = 1, unitPrice = 350000m }, new { name = "Nội soi thực quản", quantity = 1, unitPrice = 750000m } }
                },
                new { 
                    invoiceId = "HD-2605-003", realId = 3, patientName = "Lê Hoàng C", patientCode = "BN-10295",
                    date = "12/05/2026 10:05", total = 420000m, method = "Quẹt thẻ", status = "Chờ thanh toán",
                    items = new[] { new { name = "Khám chuyên khoa Nhi", quantity = 1, unitPrice = 120000m }, new { name = "Siêu âm kiểm tra", quantity = 1, unitPrice = 300000m } }
                }
            };
            return defaultList;
        }

        return invoices;
    }

    public async Task<object?> CreateInvoiceAsync(AdminInvoiceCreateDto dto)
    {
        Console.WriteLine($"[DIAGNOSTIC] CreateInvoiceAsync: PatientName='{dto.PatientName}', PatientCode='{dto.PatientCode}'");
        if (dto.Items != null)
        {
            foreach (var item in dto.Items)
            {
                Console.WriteLine($"[DIAGNOSTIC]   - Item: Name='{item.Name}', Quantity={item.Quantity}, UnitPrice={item.UnitPrice}");
            }
        }
        else
        {
            Console.WriteLine("[DIAGNOSTIC]   - Items list is NULL!");
        }

        // Tìm Bệnh nhân theo mã bệnh nhân trước (chính xác nhất)
        Patient? patient = null;
        if (!string.IsNullOrEmpty(dto.PatientCode))
        {
            patient = await _dbContext.Patients.Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PatientCode.Trim() == dto.PatientCode.Trim());
        }

        // Nếu không tìm thấy, thử tìm theo tên
        if (patient == null && !string.IsNullOrEmpty(dto.PatientName))
        {
            patient = await _dbContext.Patients.Include(p => p.User)
                .FirstOrDefaultAsync(p => p.User.FullName.Trim() == dto.PatientName.Trim());
        }

        if (patient == null)
        {
            var user = new User
            {
                UserName = "PAT_INV_" + DateTime.UtcNow.Ticks.ToString().Substring(10),
                FullName = dto.PatientName.Trim(),
                Password = BCrypt.Net.BCrypt.HashPassword("123"),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            patient = new Patient
            {
                UserId = user.UserId,
                PatientCode = string.IsNullOrEmpty(dto.PatientCode) ? $"BN-INV-{DateTime.UtcNow.ToString("HHmmss")}" : dto.PatientCode.Trim()
            };
            _dbContext.Patients.Add(patient);
            await _dbContext.SaveChangesAsync();
        }

        var staff = await _dbContext.Staffs.FirstOrDefaultAsync();
        int cashierId = staff?.UserId ?? 1;

        // Theo cơ chế Sổ cái, hóa đơn đại diện cho tất cả các khoản nợ của bệnh nhân
        // Nên ta chỉ cần gán nó vào cuộc khám gần nhất của bệnh nhân (để định danh đợt khám)
        var exam = await _dbContext.MedicalExaminations
            .OrderByDescending(m => m.MedicalExaminationId)
            .FirstOrDefaultAsync(m => m.PatientId == patient.UserId);

        if (exam == null)
        {
            var doc = await _dbContext.Doctors.FirstOrDefaultAsync();
            exam = new MedicalExamination
            {
                PatientId = patient.UserId,
                DoctorId = doc?.UserId ?? 1,
                VisitDate = DateTime.UtcNow,
                Status = 2
            };
            _dbContext.MedicalExaminations.Add(exam);
            await _dbContext.SaveChangesAsync();
        }

        decimal totalCalc = dto.Items != null ? dto.Items.Sum(i => i.Quantity * i.UnitPrice) : 0m;

        var invoice = new Invoice
        {
            MedicalExaminationId = exam.MedicalExaminationId,
            PatientId = patient.UserId,
            CashierId = cashierId,
            InvoiceDate = DateTime.UtcNow,
            TotalAmount = totalCalc,
            PaymentMethod = dto.Method,
            Status = dto.Status == "Đã thanh toán" ? 1 : 0
        };

        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        if (dto.Items != null)
        {
            foreach (var item in dto.Items)
            {
                _dbContext.InvoiceDetails.Add(new InvoiceDetail
                {
                    InvoiceId = invoice.InvoiceId,
                    ItemType = 1,
                    ItemName = item.Name.Trim(),
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalAmount = item.Quantity * item.UnitPrice
                });
            }
            await _dbContext.SaveChangesAsync();
        }

        return new
        {
            invoiceId = "HD-2605-" + invoice.InvoiceId.ToString().PadLeft(3, '0'),
            realId = invoice.InvoiceId,
            patientName = dto.PatientName,
            patientCode = patient.PatientCode,
            date = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm"),
            items = dto.Items,
            total = totalCalc,
            method = dto.Method,
            status = dto.Status
        };
    }

    public async Task<bool> ConfirmPaymentAsync(int invoiceId, string method)
    {
        var invoice = await _dbContext.Invoices.FindAsync(invoiceId);
        if (invoice == null) return false;

        invoice.Status = 1; // Đã thanh toán
        invoice.PaymentMethod = method;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelInvoiceAsync(int invoiceId)
    {
        var invoice = await _dbContext.Invoices.FindAsync(invoiceId);
        if (invoice == null) return false;

        invoice.Status = 2; // Đã hủy
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<PatientFeesDto?> GetPatientFeesAsync(string patientCode)
    {
        var patient = await _dbContext.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.PatientCode.Trim() == patientCode.Trim());

        if (patient == null) return null;

        // 1. Thu thập TẤT CẢ các khoản phí đã phát sinh (Incurred) từ TẤT CẢ các cuộc khám
        var allExams = await _dbContext.MedicalExaminations
            .Include(m => m.ServiceDetails).ThenInclude(sd => sd.Service)
            .Include(m => m.Prescriptions).ThenInclude(p => p.PrescriptionDetails).ThenInclude(pd => pd.Medication)
            .Where(m => m.PatientId == patient.UserId)
            .OrderBy(m => m.MedicalExaminationId) // Sắp xếp từ cũ đến mới
            .ToListAsync();

        var incurredItems = new List<PatientFeeItemDto>();
        foreach (var exam in allExams)
        {
            // Luôn có phí khám lâm sàng cho mỗi lượt đăng ký khám
            incurredItems.Add(new PatientFeeItemDto
            {
                Name = "Phí khám bệnh lâm sàng",
                Quantity = 1,
                UnitPrice = 150000m
            });

            if (exam.ServiceDetails != null)
            {
                foreach (var sd in exam.ServiceDetails)
                {
                    incurredItems.Add(new PatientFeeItemDto { Name = sd.Service.Name, Quantity = sd.Quantity ?? 1, UnitPrice = sd.UnitPrice });
                }
            }

            if (exam.Prescriptions != null)
            {
                foreach (var pres in exam.Prescriptions)
                {
                    if (pres.PrescriptionDetails != null)
                    {
                        foreach (var pd in pres.PrescriptionDetails)
                        {
                            incurredItems.Add(new PatientFeeItemDto { Name = $"{pd.Medication.Name} ({pd.Medication.Unit ?? "Viên"})", Quantity = pd.Quantity, UnitPrice = pd.Price });
                        }
                    }
                }
            }
        }

        // Gom nhóm các khoản phát sinh theo Tên và Đơn giá
        var groupedIncurred = new Dictionary<string, PatientFeeItemDto>();
        foreach (var item in incurredItems)
        {
            var key = $"{item.Name.Trim().ToLower()}_{item.UnitPrice:0.##}";
            if (groupedIncurred.ContainsKey(key))
                groupedIncurred[key].Quantity += item.Quantity;
            else
                groupedIncurred[key] = new PatientFeeItemDto { Name = item.Name.Trim(), Quantity = item.Quantity, UnitPrice = item.UnitPrice };
        }

        // 2. Thu thập TẤT CẢ các khoản đã lập hóa đơn (Billed) từ TẤT CẢ hóa đơn hoạt động của bệnh nhân
        var billedItems = new Dictionary<string, int>(); // Key: Name_Price, Value: Quantity
        var allInvoices = await _dbContext.Invoices
            .Include(i => i.InvoiceDetails)
            .Where(i => i.PatientId == patient.UserId && (i.Status == 0 || i.Status == 1))
            .ToListAsync();

        foreach (var inv in allInvoices)
        {
            if (inv.InvoiceDetails != null)
            {
                foreach (var id in inv.InvoiceDetails)
                {
                    var key = $"{id.ItemName.Trim().ToLower()}_{id.UnitPrice:0.##}";
                    if (billedItems.ContainsKey(key))
                        billedItems[key] += id.Quantity;
                    else
                        billedItems[key] = id.Quantity;
                }
            }
        }

        // 3. Tính toán Sổ cái: Unpaid = Incurred - Billed
        var feeItems = new List<PatientFeeItemDto>();
        foreach (var kvp in groupedIncurred)
        {
            var incurredItem = kvp.Value;
            int billedQty = billedItems.ContainsKey(kvp.Key) ? billedItems[kvp.Key] : 0;
            int remainingQty = incurredItem.Quantity - billedQty;

            if (remainingQty > 0)
            {
                feeItems.Add(new PatientFeeItemDto
                {
                    Name = incurredItem.Name,
                    Quantity = remainingQty,
                    UnitPrice = incurredItem.UnitPrice
                });
            }
        }

        decimal total = feeItems.Sum(i => i.Quantity * i.UnitPrice);

        return new PatientFeesDto
        {
            PatientName = patient.User.FullName,
            PatientCode = patient.PatientCode,
            Items = feeItems,
            Total = total
        };
    }
}


