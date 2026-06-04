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
[Authorize(Roles = "Admin")]
public class AdminStaffController : ControllerBase
{
    private readonly AdminStaffService _staffService;

    public AdminStaffController(AdminStaffService staffService)
    {
        _staffService = staffService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllStaff()
    {
        var data = await _staffService.GetAllStaffAsync();
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> CreateStaff([FromBody] AdminStaffCreateDto dto)
    {
        var res = await _staffService.CreateStaffAsync(dto);
        if (res == null) return BadRequest(new { message = "Tên đăng nhập đã tồn tại trong hệ thống." });
        return Ok(res);
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateStaff(int userId, [FromBody] AdminStaffUpdateDto dto)
    {
        var success = await _staffService.UpdateStaffAsync(userId, dto);
        if (!success) return NotFound(new { message = "Không tìm thấy hồ sơ nhân viên." });
        return Ok(new { message = "Cập nhật thông tin thành công." });
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteStaff(int userId)
    {
        var success = await _staffService.DeleteStaffAsync(userId);
        if (!success) return NotFound(new { message = "Không tìm thấy hồ sơ nhân viên." });
        return Ok(new { message = "Đã khóa/hủy tài khoản nhân viên thành công." });
    }
}
