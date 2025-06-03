using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class InventoryItem
{
    [Key]
    public int ItemId { get; set; }

    [Required]
    public string? Name { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public string? Location { get; set; }

    // Foreign key to Order
    public int? OrderId { get; set; }

    [ForeignKey("OrderId")]
    [JsonIgnore]
    public Order? Order { get; set; }

    public void DisplayInfo()
    {
        Console.WriteLine($"Item: {Name} | Quantity: {Quantity} | Location: {Location}");
    }
}