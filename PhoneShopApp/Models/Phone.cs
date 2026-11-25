using System.ComponentModel.DataAnnotations;

namespace PhoneShopApp.Models;

public class Phone
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A modell neve kötelező!")]
    [Display(Name = "Modell neve")]
    public string ModelName { get; set; } = "";

    [Range(1, 5000000, ErrorMessage = "Az ár 1 és 5 000 000 Ft között legyen!")]
    [Display(Name = "Ár (Ft)")]
    public decimal Price { get; set; }

    [Range(0, 10000, ErrorMessage = "A készlet nem lehet negatív!")]
    [Display(Name = "Készlet (db)")]
    public int StockQuantity { get; set; }

    [Display(Name = "Leírás")]
    public string? Description { get; set; }

    [Display(Name = "Márka")]
    public int BrandId { get; set; }
    public Brand? Brand { get; set; }
}
