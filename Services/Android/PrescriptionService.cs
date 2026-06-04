using BE.Dtos.Android;
using BE.Model;
using Microsoft.EntityFrameworkCore;

namespace BE.Services;

public class PrescriptionService
{
    private readonly HospitalManagementDbContext _context;

    public PrescriptionService(HospitalManagementDbContext context)
    {
        _context = context;
    }

    public async Task<PrescriptionDto?> GetByExaminationAsync(int examId, int userId)
    {
        return await _context.Prescriptions
            .Where(p =>
                p.MedicalExaminationId == examId &&
                p.MedicalExamination.PatientId == userId
            )
            .Select(p => new PrescriptionDto
            {
                PrescriptionsId = p.PrescriptionsId,
                MedicalExaminationId = p.MedicalExaminationId,
                Date = p.Date,
                Status = p.Status,

                TotalAmount = p.PrescriptionDetails
                    .Sum(d => d.Price * d.Quantity),

                Items = p.PrescriptionDetails
                    .Select(d => new PrescriptionDetailDto
                    {
                        MedicationId = d.MedicationId,
                        MedicationName = d.Medication.Name,
                        Ingredient = d.Medication.Ingredient,
                        Unit = d.Medication.Unit,
                        Quantity = d.Quantity,
                        Instructions = d.Instructions,
                        Price = d.Price,
                        LineTotal = d.Price * d.Quantity
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }
}