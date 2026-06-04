using System.Text.Json.Serialization;

namespace BE.Dtos.Android;

public class InvoiceDto
{
    [JsonPropertyName("invoiceId")]
    public int InvoiceId { get; set; }

    [JsonPropertyName("medicalExaminationId")]
    public int MedicalExaminationId { get; set; }

    [JsonPropertyName("invoiceDate")]
    public DateTime? InvoiceDate { get; set; }

    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("paymentMethod")]
    public string PaymentMethod { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("details")]
    public List<InvoiceDetailDto> Details { get; set; } = new();
}