using BE.Dtos.Android;
using BE.Model;
using Microsoft.EntityFrameworkCore;

namespace BE.Services;
public class MedicalExaminationService
{
    private readonly HospitalManagementDbContext _context;

    public MedicalExaminationService(HospitalManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<MedicalExaminationSummaryDto>> GetMyMedicalExaminationsAsync(int patientId)
    {
        return await _context.MedicalExaminations
            .Where(me => me.PatientId == patientId)
            .OrderByDescending(me => me.VisitDate)
            .Select(me => new MedicalExaminationSummaryDto
            {
                MedicalExaminationId = me.MedicalExaminationId,

                VisitDate = me.VisitDate,

                DoctorName = me.Doctor.User.User.FullName,

                Diagnosis = me.Diagnosis,

                Status = me.Status
            })
            .ToListAsync();
    }

    public async Task<MedicalExaminationDetailDto?> GetMedicalExaminationDetailAsync(
        int patientId,
        int medicalExaminationId
    )
    {
        return await _context.MedicalExaminations
            .Where(me =>
                me.PatientId == patientId &&
                me.MedicalExaminationId == medicalExaminationId
            )
            .Select(me => new MedicalExaminationDetailDto
            {
                MedicalExaminationId = me.MedicalExaminationId,

                VisitDate = me.VisitDate,

                PatientName = me.Patient.User.FullName,

                DoctorName = me.Doctor.User.User.FullName,

                Symptoms = me.Symptoms,

                Diagnosis = me.Diagnosis,

                TreatmentPlan = me.TreatmentPlan,

                Notes = me.Notes,

                BloodPressure = me.BloodPressure,

                HeartRate = me.HeartRate,

                Temperature = me.Temperature,

                Weight = me.Weight,

                Height = me.Height,

                Status = me.Status
            })
            .FirstOrDefaultAsync();
    }
}   