using BE.Dtos.Android;
using BE.Model;
using Microsoft.EntityFrameworkCore;

namespace BE.Services;

public class InvoiceService
{
    private readonly HospitalManagementDbContext _context;

    public InvoiceService(HospitalManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<InvoiceDto>> GetAllByExaminationAsync(int examId, int userId)
    {
        return await _context.Invoices
            .Where(i =>
                i.MedicalExaminationId == examId &&
                i.Patient.UserId == userId // đảm bảo đúng user login
            )
            .OrderByDescending(i => i.InvoiceDate)
            .Select(i => new InvoiceDto
            {
                InvoiceId = i.InvoiceId,
                MedicalExaminationId = i.MedicalExaminationId,
                InvoiceDate = i.InvoiceDate,
                TotalAmount = i.TotalAmount,
                PaymentMethod = i.PaymentMethod,
                Status = i.Status,
                FullName = i.Patient.User.FullName,

                Details = i.InvoiceDetails.Select(d => new InvoiceDetailDto
                {
                    Id = d.Id,
                    ItemType = d.ItemType,
                    ItemName = d.ItemName,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    TotalAmount = d.TotalAmount,
                    ReferenceId = d.ReferenceId
                }).ToList()
            })
            .ToListAsync();
    }
}