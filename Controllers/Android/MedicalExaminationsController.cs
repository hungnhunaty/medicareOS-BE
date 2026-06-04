using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using BE.Services;
using BE.Dtos.Android;

namespace BE.Controllers;

[ApiController]
[Route("api/me/medical-examinations")]
[Authorize]
public class MedicalExaminationsController : ControllerBase
{
    private readonly MedicalExaminationService _medicalExaminationService;

    public MedicalExaminationsController(
        MedicalExaminationService medicalExaminationService
    )
    {
        _medicalExaminationService = medicalExaminationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyMedicalExaminations()
    {
        var patientId = GetCurrentUserId();

        var result = await _medicalExaminationService
            .GetMyMedicalExaminationsAsync(patientId);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMedicalExaminationDetail(int id)
    {
        var patientId = GetCurrentUserId();

        var result = await _medicalExaminationService
            .GetMedicalExaminationDetailAsync(patientId, id);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User ID not found");
        return int.Parse(userIdClaim);
    }
}