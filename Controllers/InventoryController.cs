using LogiTrack.Context;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly LogiTrackContext _context;

        public InventoryController(LogiTrackContext context)
        {
            _context = context;
        }

        // Return a list of all inventory items
        [HttpGet]
        public IActionResult GetAllInventoryItems()
        {
            var items = _context.InventoryItems.ToList();
            return Ok(items);
        }

        // Add a new item to the inventory
        [HttpPost]
        public IActionResult AddInventoryItem([FromBody] InventoryItem newItem)
        {
            if (newItem == null)
            {
                return BadRequest("Invalid inventory item.");
            }

            _context.InventoryItems.Add(newItem);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetAllInventoryItems), new { id = newItem.ItemId }, newItem);
        }

        // Remove an item by ID
        [HttpDelete("{id}")]
        public IActionResult RemoveInventoryItem(int id)
        {
            var item = _context.InventoryItems.FirstOrDefault(i => i.ItemId == id);
            if (item == null)
            {
                return NotFound($"Inventory item with ID {id} not found.");
            }

            _context.InventoryItems.Remove(item);
            _context.SaveChanges();
            return NoContent();
        }
    }
}