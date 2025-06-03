using LogiTrack.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly LogiTrackContext _context;

        public TestController(LogiTrackContext context)
        {
            _context = context;
        }

        #region Test Methods


        [HttpGet("test-inventory")]
        public IActionResult TestInventory()
        {
            var item = new InventoryItem
            {
                ItemId = 1,
                Name = "Pallet Jack",
                Quantity = 12,
                Location = "Warehouse A"
            };
            item.DisplayInfo();
            return Ok($"Tested InventoryItem: {item.Name}");
        }

        [HttpGet("test-order")]
        public IActionResult TestOrder()
        {
            var item1 = new InventoryItem { ItemId = 1, Name = "Pallet Jack", Quantity = 12, Location = "Warehouse A" };
            var order = new Order { OrderId = 1001, CustomerName = "Samir", DatePlaced = DateTime.Now };
            order.AddItem(item1);
            order.AddItem(new InventoryItem { ItemId = 2, Name = "Forklift", Quantity = 1, Location = "Warehouse B" });
            order.RemoveItem(2);

            return Ok(order.GetOrderSummary());
        }

        #endregion

        [HttpGet("order-summaries")]
        public IActionResult GetOrderSummaries()
        {
            var summaries = _context.Orders
                .Include(o => o.Items) // Ensure related items are loaded
                .Select(o => o.GetOrderSummary())
                .ToList();

            return Ok(summaries);
        }

        [HttpGet("seed-db")]
        public IActionResult SeedDb()
        {
            if (!_context.Orders.Any())
            {
                var order = new Order
                {
                    CustomerName = "Samir",
                    DatePlaced = DateTime.Now,
                    Items = new List<InventoryItem>
                    {
                        new InventoryItem { Name = "Pallet Jack", Quantity = 12, Location = "Warehouse A" },
                        new InventoryItem { Name = "Forklift", Quantity = 1, Location = "Warehouse B" }
                    }
                };

                _context.Orders.Add(order);
                _context.SaveChanges();
            }

            return Ok("Database seeded successfully.");
        }
    }
}