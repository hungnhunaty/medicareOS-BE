using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BE.Model;
using BE.Dtos.Admin;
using Microsoft.EntityFrameworkCore;

namespace BE.Services;

public class AdminMedicationService
{
    private readonly HospitalManagementDbContext _dbContext;

    public AdminMedicationService(HospitalManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<object> GetAllMedicationsAsync()
    {
        var meds = await _dbContext.Medications
            .OrderByDescending(m => m.MedicationId)
            .Select(m => new
            {
                medicationId = m.MedicationId,
                id = "MED-" + m.MedicationId.ToString().PadLeft(3, '0'),
                name = m.Name,
                activeIngredient = m.Ingredient ?? "N/A",
                unit = m.Unit ?? "Hộp",
                unitPrice = m.CurentPrice,
                stock = m.Quantity ?? 0
            })
            .ToListAsync();

        return meds;
    }

    public async Task<object?> CreateMedicationAsync(AdminMedicationCreateDto dto)
    {
        if (await _dbContext.Medications.AnyAsync(m => m.Name == dto.Name.Trim()))
        {
            return null; // Trùng tên thuốc
        }

        var med = new Medication
        {
            Name = dto.Name.Trim(),
            Ingredient = dto.ActiveIngredient?.Trim(),
            Unit = dto.Unit?.Trim() ?? "Hộp",
            CurentPrice = dto.UnitPrice,
            Quantity = dto.Stock >= 0 ? dto.Stock : 0
        };

        _dbContext.Medications.Add(med);
        await _dbContext.SaveChangesAsync();

        return new
        {
            medicationId = med.MedicationId,
            id = "MED-" + med.MedicationId.ToString().PadLeft(3, '0'),
            name = med.Name,
            activeIngredient = med.Ingredient,
            unit = med.Unit,
            unitPrice = med.CurentPrice,
            stock = med.Quantity
        };
    }

    public async Task<bool> AddStockAsync(int medicationId, int addedQty)
    {
        var med = await _dbContext.Medications.FindAsync(medicationId);
        if (med == null || addedQty <= 0) return false;

        med.Quantity = (med.Quantity ?? 0) + addedQty;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateMedicationAsync(int medicationId, AdminMedicationUpdateDto dto)
    {
        var med = await _dbContext.Medications.FindAsync(medicationId);
        if (med == null) return false;

        med.Name = dto.Name.Trim();
        med.Ingredient = dto.ActiveIngredient?.Trim();
        med.Unit = dto.Unit?.Trim() ?? "Hộp";
        med.CurentPrice = dto.UnitPrice;
        med.Quantity = dto.Stock >= 0 ? dto.Stock : 0;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteMedicationAsync(int medicationId)
    {
        var med = await _dbContext.Medications.FindAsync(medicationId);
        if (med == null) return false;

        _dbContext.Medications.Remove(med);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}


