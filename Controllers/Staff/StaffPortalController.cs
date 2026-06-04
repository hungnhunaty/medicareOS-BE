using System.Threading.Tasks;
using BE.Services;
using BE.Dtos.Staff;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

namespace BE.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableCors("CorsPolicy")]
[Authorize(Roles = "Staff")]
public class StaffPortalController : ControllerBase
{
    private readonly StaffPortalService _staffService;

    public StaffPortalController(StaffPortalService staffService)
    {
        _staffService = staffService;
    }

    [HttpGet("queues")]
    public async Task<IActionResult> GetQueues()
    {
        var data = await _staffService.GetReceptionQueuesAsync();
        return Ok(data);
    }

    [HttpGet("routing-options")]
    public async Task<IActionResult> GetRoutingOptions()
    {
        var data = await _staffService.GetRoutingOptionsAsync();
        return Ok(data);
    }

    [HttpPost("register-queue")]
    public async Task<IActionResult> RegisterQueue([FromBody] StaffRegisterQueueDto dto)
    {
        try
        {
            var res = await _staffService.RegisterPatientQueueAsync(dto);
            if (res == null) return BadRequest(new { message = "Lỗi khi đăng ký tiếp đón vào hàng đợi." });
            return Ok(res);
        }
        catch (System.InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("patient/{userId}")]
    public async Task<IActionResult> GetPatient(int userId)
    {
        var patient = await _staffService.GetPatientByIdAsync(userId);
        if (patient == null) return NotFound(new { message = "Không tìm thấy tài khoản Bệnh nhân có ID này." });
        return Ok(patient);
    }

    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices()
    {
        var data = await _staffService.GetPendingInvoicesAsync();
        return Ok(data);
    }

    [HttpPut("invoices/{invoiceId}/pay")]
    public async Task<IActionResult> ProcessPayment(int invoiceId, [FromBody] StaffProcessPaymentDto dto)
    {
        var success = await _staffService.ProcessPaymentAsync(invoiceId, dto.PaymentMethod);
        if (!success) return NotFound(new { message = "Không tìm thấy hóa đơn cần thanh toán." });
        return Ok(new { success = true, message = "Thanh toán viện phí thành công." });
    }
}
