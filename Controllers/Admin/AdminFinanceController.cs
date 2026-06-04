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
[Authorize(Roles = "Admin,Staff")]
public class AdminFinanceController : ControllerBase
{
    private readonly AdminFinanceService _financeService;

    public AdminFinanceController(AdminFinanceService financeService)
    {
        _financeService = financeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllInvoices()
    {
        var data = await _financeService.GetAllInvoicesAsync();
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> CreateInvoice([FromBody] AdminInvoiceCreateDto dto)
    {
        var res = await _financeService.CreateInvoiceAsync(dto);
        if (res == null) return BadRequest(new { message = "Lỗi xuất hóa đơn viện phí." });
        return Ok(res);
    }

    [HttpPut("confirm/{invoiceId}")]
    public async Task<IActionResult> ConfirmPayment(int invoiceId, [FromBody] AdminPaymentConfirmDto dto)
    {
        var success = await _financeService.ConfirmPaymentAsync(invoiceId, dto.Method);
        if (!success) return NotFound(new { message = "Không tìm thấy hóa đơn." });
        return Ok(new { message = "Xác nhận thanh toán thành công." });
    }

    [HttpPut("cancel/{invoiceId}")]
    public async Task<IActionResult> CancelInvoice(int invoiceId)
    {
        var success = await _financeService.CancelInvoiceAsync(invoiceId);
        if (!success) return NotFound(new { message = "Không tìm thấy hóa đơn." });
        return Ok(new { message = "Đã hủy hóa đơn thành công." });
    }

    [HttpGet("patient-fees/{patientCode}")]
    public async Task<IActionResult> GetPatientFees(string patientCode)
    {
        var res = await _financeService.GetPatientFeesAsync(patientCode);
        if (res == null) return NotFound(new { message = "Không tìm thấy thông tin bệnh nhân hoặc mã bệnh nhân không hợp lệ." });
        return Ok(res);
    }
}

public class AdminPaymentConfirmDto
{
    public string Method { get; set; } = "Tiền mặt";
}
