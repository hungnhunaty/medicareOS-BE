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
public class AdminPatientController : ControllerBase
{
    private readonly AdminPatientService _patientService;

    public AdminPatientController(AdminPatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAllPatients()
    {
        var data = await _patientService.GetAllPatientsAsync();
        return Ok(data);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePatient([FromBody] AdminPatientCreateDto dto)
    {
        var res = await _patientService.CreatePatientAsync(dto);
        if (res == null) return BadRequest(new { message = "Lỗi khởi tạo hồ sơ bệnh nhân." });
        return Ok(res);
    }
}
