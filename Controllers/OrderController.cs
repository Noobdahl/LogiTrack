using LogiTrack.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly LogiTrackContext _context;

        public OrderController(LogiTrackContext context)
        {
            _context = context;
        }

        // Return a list of all orders
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders.ToListAsync();
            return Ok(orders);
        }

        // Return an order with its items
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null)
            {
                return NotFound($"Order with ID {id} not found.");
            }

            return Ok(order);
        }

        // Create a new order
        [HttpPost]
        public async Task<IActionResult> AddOrder([FromBody] Order newOrder)
        {
            if (newOrder == null)
            {
                return BadRequest("Invalid order.");
            }

            // Ensure each InventoryItem is tracked as a new entity and linked to the order
            if (newOrder.Items != null && newOrder.Items.Count > 0)
            {
                foreach (var item in newOrder.Items)
                {
                    // Remove ItemId if present to avoid conflicts with existing items
                    item.ItemId = 0;
                }
            }

            await _context.Orders.AddAsync(newOrder);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAllOrders), new { id = newOrder.OrderId }, newOrder);
        }

        // Delete an order by ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveOrder(int id)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null)
            {
                return NotFound($"Order with ID {id} not found.");
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}