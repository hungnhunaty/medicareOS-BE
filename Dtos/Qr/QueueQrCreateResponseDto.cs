using System;

namespace BE.Dtos.Qr;

public class QueueQrCreateResponseDto
{
    public string SessionId { get; set; } = null!;
    public string QrPayload { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}
