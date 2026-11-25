namespace PhoneShopApp.Models;

public class Brand
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    
    // 1:N kapcsolat: egy márkához több telefon tartozik
    public List<Phone> Phones { get; set; } = new();
}
