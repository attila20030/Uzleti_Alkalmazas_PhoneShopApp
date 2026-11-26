using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneShopApp.Data;
using PhoneShopApp.Models;

namespace PhoneShopApp.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var query = _db.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(i => i.Phone)
                    .ThenInclude(p => p!.Brand)
            .AsQueryable();

        if (!User.IsInRole("Admin"))
            query = query.Where(o => o.UserId == userId);

        var orders = await query.OrderByDescending(o => o.OrderDate).ToArrayAsync();
        return View(orders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int phoneId, int quantity = 1)
    {
        if (quantity < 1)
        {
            TempData["Error"] = "A mennyiségnek legalább 1-nek kell lennie!";
            return RedirectToAction("Index", "Phones");
        }

        var phone = await _db.Phones.FindAsync(phoneId);
        if (phone is null) return NotFound();

        if (phone.StockQuantity < quantity)
        {
            TempData["Error"] = $"Nincs elegendõ készlet a(z) {phone.ModelName} modellbõl (elérhetõ: {phone.StockQuantity} db).";
            return RedirectToAction("Index", "Phones");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            Status = "Feldolgozás alatt",
            TotalAmount = phone.Price * quantity,
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    PhoneId = phone.Id,
                    Quantity = quantity,
                    UnitPrice = phone.Price
                }
            }
        };

        phone.StockQuantity -= quantity;
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Rendelésedet sikeresen rögzítettük!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Phone)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return NotFound();

        if (status == "Törölve" && order.Status != "Törölve")
        {
            foreach (var item in order.Items)
            {
                if (item.Phone is not null)
                    item.Phone.StockQuantity += item.Quantity;
            }
        }

        order.Status = status;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
