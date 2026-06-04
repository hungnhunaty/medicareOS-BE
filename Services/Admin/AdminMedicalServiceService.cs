using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BE.Model;
using BE.Dtos.Admin;
using Microsoft.EntityFrameworkCore;

namespace BE.Services;

public class AdminMedicalServiceService
{
    private readonly HospitalManagementDbContext _dbContext;

    public AdminMedicalServiceService(HospitalManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<object> GetAllServicesAsync()
    {
        var services = await _dbContext.Services
            .OrderByDescending(s => s.ServiceId)
            .Select(s => new
            {
                serviceId = s.ServiceId,
                id = "DV-" + s.ServiceId.ToString().PadLeft(3, '0'),
                name = s.Name,
                category = s.Name.Contains("Khám") ? "Khám bệnh" :
                           s.Name.Contains("Siêu âm") || s.Name.Contains("Chụp") ? "Chẩn đoán hình ảnh" :
                           s.Name.Contains("Xét nghiệm") ? "Xét nghiệm" : "Thủ thuật",
                price = s.CurrentPrice,
                status = "Đang áp dụng"
            })
            .ToListAsync();

        return services;
    }

    public async Task<object?> CreateServiceAsync(AdminServiceCreateDto dto)
    {
        if (await _dbContext.Services.AnyAsync(s => s.Name == dto.Name.Trim()))
        {
            return null; // Trùng tên
        }

        var service = new Service
        {
            Name = dto.Name.Trim(),
            CurrentPrice = dto.Price
        };

        _dbContext.Services.Add(service);
        await _dbContext.SaveChangesAsync();

        return new
        {
            serviceId = service.ServiceId,
            id = "DV-" + service.ServiceId.ToString().PadLeft(3, '0'),
            name = service.Name,
            category = dto.Category,
            price = service.CurrentPrice,
            status = dto.Status
        };
    }

    public async Task<bool> UpdateServiceAsync(int serviceId, AdminServiceUpdateDto dto)
    {
        var service = await _dbContext.Services.FindAsync(serviceId);
        if (service == null) return false;

        service.Name = dto.Name.Trim();
        service.CurrentPrice = dto.Price;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteServiceAsync(int serviceId)
    {
        var service = await _dbContext.Services.FindAsync(serviceId);
        if (service == null) return false;

        _dbContext.Services.Remove(service);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}


