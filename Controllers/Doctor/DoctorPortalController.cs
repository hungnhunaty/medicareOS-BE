using System.Threading.Tasks;
using BE.Services;
using BE.Dtos.Doctor;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

namespace BE.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableCors("CorsPolicy")]
[Authorize(Roles = "Doctor")]
public class DoctorPortalController : ControllerBase
{
    private readonly DoctorPortalService _doctorService;

    public DoctorPortalController(DoctorPortalService doctorService)
    {
        _doctorService = doctorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetExaminations([FromQuery] int? doctorId)
    {
        var data = await _doctorService.GetAssignedExaminationsAsync(doctorId);
        return Ok(data);
    }

    [HttpPut("{examinationId}/start")]
    public async Task<IActionResult> StartExamination(int examinationId)
    {
        var success = await _doctorService.StartExaminationAsync(examinationId);
        if (!success) return NotFound(new { message = "Không tìm thấy ca khám." });
        return Ok(new { success = true });
    }

    [HttpPut("{examinationId}/diagnosis")]
    public async Task<IActionResult> UpdateDiagnosis(int examinationId, [FromBody] DoctorDiagnosisDto dto)
    {
        var res = await _doctorService.UpdateDiagnosisAsync(examinationId, dto);
        if (res == null) return NotFound(new { message = "Không tìm thấy thông tin ca khám." });
        return Ok(res);
    }

    [HttpPost("call-next")]
    public async Task<IActionResult> CallNextPatient([FromQuery] int doctorId)
    {
        var res = await _doctorService.CallNextPatientAsync(doctorId);
        if (res == null) return Ok(new { message = "Không còn bệnh nhân nào đang chờ khám trong hàng đợi." });
        return Ok(res);
    }

    [HttpPost("recall/{examinationId}")]
    public async Task<IActionResult> RecallPatient(int examinationId)
    {
        var success = await _doctorService.RecallPatientAsync(examinationId);
        if (!success) return NotFound(new { message = "Không tìm thấy ca khám." });
        return Ok(new { success = true });
    }

    [HttpPost("skip/{examinationId}")]
    public async Task<IActionResult> SkipPatient(int examinationId)
    {
        var success = await _doctorService.SkipPatientAsync(examinationId);
        if (!success) return NotFound(new { message = "Không tìm thấy ca khám." });
        return Ok(new { success = true });
    }

    [HttpGet("{doctorId}/information")]
    public async Task<IActionResult> GetDoctorInformation(int doctorId)
    {
        var data = await _doctorService.GetDoctorInformation(doctorId);
        if (data == null) return NotFound(new { message = "Không tìm thấy thông tin bác sĩ." });
        return Ok(data);
    }

    [HttpPut("{doctorId}/information")]
    public async Task<IActionResult> UpdateDoctorInformation(int doctorId, [FromBody] DoctorInformationRequest dto)
    {
        var success = await _doctorService.UpdateDoctorInformationAsync(doctorId, dto);
        if (!success) return BadRequest(new { message = "Không thể cập nhật thông tin." });
        return Ok(new { success = true, message = "Cập nhật hồ sơ thành công." });
    }

    [HttpPost("check-prescription-safety")]
    public async Task<IActionResult> CheckPrescriptionSafety([FromBody] CheckPrescriptionSafetyDto dto)
    {
        var res = await _doctorService.CheckPrescriptionSafetyAsync(dto);
        return Ok(res);
    }
}
