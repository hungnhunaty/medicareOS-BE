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
[Authorize(Roles = "Admin, Doctor, Pharmacist")]
public class AdminMedicationController : ControllerBase
{
    private readonly AdminMedicationService _medicationService;

    public AdminMedicationController(AdminMedicationService medicationService)
    {
        _medicationService = medicationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMedications()
    {
        var data = await _medicationService.GetAllMedicationsAsync();
        return Ok(data);
    }

    [HttpPost]
    [Authorize(Roles = "Admin, Pharmacist")]
    public async Task<IActionResult> CreateMedication([FromBody] AdminMedicationCreateDto dto)
    {
        var res = await _medicationService.CreateMedicationAsync(dto);
        if (res == null) return BadRequest(new { message = "Tên dược phẩm đã tồn tại trong kho." });
        return Ok(res);
    }

    [HttpPut("add-stock/{medicationId}")]
    [Authorize(Roles = "Admin, Pharmacist")]
    public async Task<IActionResult> AddStock(int medicationId, [FromBody] AdminMedicationAddStockDto dto)
    {
        var success = await _medicationService.AddStockAsync(medicationId, dto.AddedQuantity);
        if (!success) return NotFound(new { message = "Không tìm thấy thuốc hoặc số lượng không hợp lệ." });
        return Ok(new { message = "Nhập kho thành công." });
    }

    [HttpPut("{medicationId}")]
    [Authorize(Roles = "Admin, Pharmacist")]
    public async Task<IActionResult> UpdateMedication(int medicationId, [FromBody] AdminMedicationUpdateDto dto)
    {
        var success = await _medicationService.UpdateMedicationAsync(medicationId, dto);
        if (!success) return NotFound(new { message = "Không tìm thấy dược phẩm." });
        return Ok(new { message = "Cập nhật thông tin dược phẩm thành công." });
    }

    [HttpDelete("{medicationId}")]
    [Authorize(Roles = "Admin, Pharmacist")]
    public async Task<IActionResult> DeleteMedication(int medicationId)
    {
        try
        {
            var success = await _medicationService.DeleteMedicationAsync(medicationId);
            if (!success) return NotFound(new { message = "Không tìm thấy dược phẩm." });
            return Ok(new { message = "Xóa dược phẩm thành công." });
        }
        catch (System.Exception)
        {
            return BadRequest(new { message = "Không thể xóa dược phẩm này vì đã có lịch sử kê đơn liên quan trong hệ thống." });
        }
    }
}
