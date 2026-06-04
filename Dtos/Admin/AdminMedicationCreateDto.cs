namespace BE.Dtos.Admin;

public class AdminMedicationCreateDto
{
    public string Name { get; set; } = "";
    public string ActiveIngredient { get; set; } = "";
    public string Unit { get; set; } = "Hộp";
    public decimal UnitPrice { get; set; }
    public int Stock { get; set; }
}
