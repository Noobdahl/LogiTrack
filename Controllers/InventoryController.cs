using LogiTrack.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Protect all endpoints by default
    public class InventoryController : ControllerBase
    {
        private readonly LogiTrackContext _context;
        private readonly IMemoryCache _cache;
        public InventoryController(LogiTrackContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // Return a list of all inventory items
        [HttpGet]
        public async Task<IActionResult> GetAllInventoryItems()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            const string cacheKey = "all_inventory_items";
            if (!_cache.TryGetValue(cacheKey, out List<InventoryItem>? items))
            {
                items = await _context.InventoryItems.AsNoTracking().ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                _cache.Set(cacheKey, items, cacheEntryOptions);
            }

            stopwatch.Stop();
            Response.Headers["X-Elapsed-Milliseconds"] = stopwatch.ElapsedMilliseconds.ToString();
            return Ok(items);
        }

        // Get an item by ID
        [HttpGet("{id}")]
        public IActionResult GetInventoryItemById(int id)
        {
            var item = _context.InventoryItems.FirstOrDefault(i => i.ItemId == id);
            if (item == null)
            {
                return NotFound($"Inventory item with ID {id} not found.");
            }
            return Ok(item);
        }

        // Add a new item to the inventory
        [HttpPost]
        [Authorize(Roles = "Manager")] // Only Manager can add items
        public async Task<IActionResult> AddInventoryItem([FromBody] InventoryItem newItem)
        {
            if (newItem == null)
                return BadRequest("Invalid inventory item.");

            await _context.InventoryItems.AddAsync(newItem);
            await _context.SaveChangesAsync();
            _cache.Remove("all_inventory_items");
            return CreatedAtAction(nameof(GetAllInventoryItems), new { id = newItem.ItemId }, newItem);
        }

        // Remove an item by ID
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")] // Only Manager can delete items
        public async Task<IActionResult> RemoveInventoryItem(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null)
                return NotFound($"Inventory item with ID {id} not found.");

            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();
            _cache.Remove("all_inventory_items");
            return NoContent();
        }
    }
}