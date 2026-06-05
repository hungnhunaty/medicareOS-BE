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
public class AdminRoomController : ControllerBase
{
    private readonly AdminRoomService _roomService;

    public AdminRoomController(AdminRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRooms()
    {
        var data = await _roomService.GetAllRoomsAsync();
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoom([FromBody] AdminRoomCreateDto dto)
    {
        var res = await _roomService.CreateRoomAsync(dto);
        if (res == null) return BadRequest(new { message = "Khoa/phòng ban không tồn tại." });
        return Ok(res);
    }

    [HttpPut("{roomId}")]
    public async Task<IActionResult> UpdateRoom(int roomId, [FromBody] AdminRoomUpdateDto dto)
    {
        var success = await _roomService.UpdateRoomAsync(roomId, dto);
        if (!success) return NotFound(new { message = "Không tìm thấy phòng khám." });
        return Ok(new { message = "Cập nhật phòng khám thành công." });
    }

    [HttpDelete("{roomId}")]
    public async Task<IActionResult> DeleteRoom(int roomId)
    {
        try
        {
            var success = await _roomService.DeleteRoomAsync(roomId);
            if (!success) return NotFound(new { message = "Không tìm thấy phòng khám." });
            return Ok(new { message = "Xóa/vô hiệu hóa phòng khám thành công." });
        }
        catch (System.Exception)
        {
            return BadRequest(new { message = "Không thể xóa phòng này vì có dữ liệu khám bệnh liên quan." });
        }
    }
}
