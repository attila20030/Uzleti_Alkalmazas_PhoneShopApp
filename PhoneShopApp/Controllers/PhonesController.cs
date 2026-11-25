using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PhoneShopApp.Data;
using PhoneShopApp.Models;

namespace PhoneShopApp.Controllers;

public class PhonesController : Controller
{
    private readonly AppDbContext _db;

    public PhonesController(AppDbContext db) => _db = db;

    // GET: /Phones (Lista + szűrés márkára és keresőszóra)
    public async Task<IActionResult> Index(int? brandId, string? search)
    {
        var query = _db.Phones
            .AsNoTracking()
            .Include(p => p.Brand)
            .AsQueryable();

        if (brandId.HasValue)
            query = query.Where(p => p.BrandId == brandId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.ModelName.Contains(search));

        ViewBag.Brands = new SelectList(await _db.Brands.ToListAsync(), "Id", "Name", brandId);
        ViewBag.Search = search;

        var phones = await query.OrderBy(p => p.Brand!.Name).ThenBy(p => p.ModelName).ToListAsync();
        return View(phones);
    }

    // GET: /Phones/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Brands = new SelectList(await _db.Brands.ToListAsync(), "Id", "Name");
        return View();
    }

    // POST: /Phones/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Phone phone)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Brands = new SelectList(await _db.Brands.ToListAsync(), "Id", "Name", phone.BrandId);
            return View(phone);
        }

        _db.Phones.Add(phone);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Phones/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var phone = await _db.Phones.FindAsync(id);
        if (phone is null) return NotFound();

        ViewBag.Brands = new SelectList(await _db.Brands.ToListAsync(), "Id", "Name", phone.BrandId);
        return View(phone);
    }

    // POST: /Phones/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Phone input)
    {
        var phone = await _db.Phones.FindAsync(id);
        if (phone is null) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.Brands = new SelectList(await _db.Brands.ToListAsync(), "Id", "Name", input.BrandId);
            return View(input);
        }

        phone.ModelName = input.ModelName;
        phone.Price = input.Price;
        phone.StockQuantity = input.StockQuantity;
        phone.Description = input.Description;
        phone.BrandId = input.BrandId;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
