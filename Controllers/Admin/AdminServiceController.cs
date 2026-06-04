using System.Threading.Tasks;
using BE.Services;
using BE.Dtos.Admin;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

namespace BE.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableCors("CorsPolicy")]
[Authorize]
public class AdminServiceController : ControllerBase
{
    private readonly AdminMedicalServiceService _serviceService;

    public AdminServiceController(AdminMedicalServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAllServices()
    {
        var data = await _serviceService.GetAllServicesAsync();
        return Ok(data);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateService([FromBody] AdminServiceCreateDto dto)
    {
        var res = await _serviceService.CreateServiceAsync(dto);
        if (res == null) return BadRequest(new { message = "Tên dịch vụ y tế đã tồn tại." });
        return Ok(res);
    }

    [HttpPut("{serviceId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateService(int serviceId, [FromBody] AdminServiceUpdateDto dto)
    {
        var success = await _serviceService.UpdateServiceAsync(serviceId, dto);
        if (!success) return NotFound(new { message = "Không tìm thấy dịch vụ y tế." });
        return Ok(new { message = "Cập nhật dịch vụ thành công." });
    }

    [HttpDelete("{serviceId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteService(int serviceId)
    {
        try
        {
            var success = await _serviceService.DeleteServiceAsync(serviceId);
            if (!success) return NotFound(new { message = "Không tìm thấy dịch vụ y tế." });
            return Ok(new { message = "Xóa dịch vụ thành công." });
        }
        catch (System.Exception)
        {
            return BadRequest(new { message = "Không thể xóa dịch vụ này vì đã có dữ liệu khám chữa bệnh liên quan trong hệ thống." });
        }
    }
}
