using System.ComponentModel.DataAnnotations;

public class Order
{
    [Key]
    public int OrderId { get; set; }

    [Required]
    public string? CustomerName { get; set; }

    [Required]
    public DateTime DatePlaced { get; set; }

    // Navigation property for related InventoryItems
    public List<InventoryItem> Items { get; set; } = new List<InventoryItem>();

    public Order()
    {
        Items = new List<InventoryItem>();
    }

    public void AddItem(InventoryItem item)
    {
        if (!Items.Any(i => i.ItemId == item.ItemId))
    {
        Items.Add(item);
    }
    }

    public void RemoveItem(int itemId)
    {
        var itemToRemove = Items.FirstOrDefault(i => i.ItemId == itemId);
        if (itemToRemove != null)
        {
            Items.Remove(itemToRemove);
        }
    }

    public string GetOrderSummary()
    {
        return $"Order #{OrderId} for {CustomerName} | Items: {Items.Count} | Placed: {DatePlaced.ToShortDateString()}";
    }
}