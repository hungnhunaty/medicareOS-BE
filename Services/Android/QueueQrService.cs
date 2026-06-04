using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using BE.Dtos.Qr;
using BE.Dtos.Queue;
using BE.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BE.Services;

public class QueueQrService
{
    private static readonly ConcurrentDictionary<string, QueueQrSessionItem> _sessions = new();

    private readonly QueueService _queueService;
    private readonly IHubContext<QueueHub> _hubContext;

    public QueueQrService(
        QueueService queueService,
        IHubContext<QueueHub> hubContext)
    {
        _queueService = queueService;
        _hubContext = hubContext;
    }

    public Task<QueueQrCreateResponseDto> CreateQueueSessionAsync(
        QueueQrCreateDto dto,
        int staffUserId)
    {
        var sessionId = Guid.NewGuid().ToString("N");
        var expiresAt = DateTime.UtcNow.AddMinutes(30);
        Console.WriteLine("DEBUG DEPARTMENT ID: " + dto.DepartmentId);
        var session = new QueueQrSessionItem
        {
            SessionId = sessionId,
            DepartmentId = dto.DepartmentId,
            Symptoms = dto.Symptoms,
            DoctorId = dto.DoctorId,
            CreatedByUserId = staffUserId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsUsed = false
        };

        _sessions[sessionId] = session;

        return Task.FromResult(new QueueQrCreateResponseDto
        {
            SessionId = sessionId,
            QrPayload = sessionId,
            ExpiresAt = expiresAt
        });
    }

    public async Task<QueueQrCheckinResponseDto> CheckinByQrAsync(
        QueueQrRequestDto dto,
        int patientUserId)
    {
        if (!_sessions.TryGetValue(dto.SessionId, out var session))
            throw new Exception("QR session invalid or expired");

        if (session.ExpiresAt < DateTime.UtcNow)
        {
            _sessions.TryRemove(dto.SessionId, out _);
            throw new Exception("QR session invalid or expired");
        }

        if (session.IsUsed)
            throw new Exception("QR đã được sử dụng");

        // Mark session as used
        session.IsUsed = true;
        session.UsedAt = DateTime.UtcNow;

        // Create queue entry from session data
        var request = new QueueRequestDto
        {
            DepartmentId = session.DepartmentId,
            Symptoms = session.Symptoms,
            DoctorId = session.DoctorId,
            PatientId = patientUserId
        };

        var doctorText = request.DoctorId.HasValue ? request.DoctorId.Value.ToString() : "null";
        Console.WriteLine($"[-----------------------------------------QueueQrService] Can. session={dto.SessionId}, patientUserId={patientUserId}, departmentId={request.DepartmentId}, doctorId={doctorText}");

        var queueResult = await _queueService.AddToQueueAsync(request);
        if (queueResult == null)
        {
            Console.WriteLine($"[QueueQrService] Cannot create queue. session={dto.SessionId}, patientUserId={patientUserId}, departmentId={request.DepartmentId}, doctorId={doctorText}");
            throw new Exception("Cannot create queue");
        }

        // Notify staff group about successful check-in
        var notificationPayload = new
        {
            SessionId = session.SessionId,
            Status = "Scanned",
            ExamId = queueResult.ExamId,
            PatientName = queueResult.PatientName,
            PatientCode = queueResult.PatientCode,
            ClinicName = queueResult.ClinicName,
            QueueNumber = queueResult.QueueNumber,
            WaitingCount = queueResult.WaitingCount
        };

        await _hubContext.Clients.Group(GetGroupName(session.SessionId))
            .SendAsync("QueueCreated", notificationPayload);

        // Remove session after processing
        _sessions.TryRemove(dto.SessionId, out _);

        // Only return sessionId, let client fetch queue details separately
        return new QueueQrCheckinResponseDto
        {
            SessionId = session.SessionId
        };
    }

    private static string GetGroupName(string sessionId)
        => $"session-{sessionId}";

    private sealed class QueueQrSessionItem
    {
        public string SessionId { get; set; } = null!;
        public int DepartmentId { get; set; }
        public string? Symptoms { get; set; }
        public int? DoctorId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
    }
}
