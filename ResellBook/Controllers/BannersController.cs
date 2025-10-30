using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResellBook.Data;
using ResellBook.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
[AllowAnonymous]
[Route("api/[controller]")]
[ApiController]
public class BannersController : ControllerBase
{
    private readonly AppDbContext _context;

    public BannersController(AppDbContext context)
    {
        _context = context;
    }

    // ✅ GET: api/Banners
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Banner>>> GetAll()
    {
        return await _context.Banners.ToListAsync();
    }

    // ✅ GET: api/Banners/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Banner>> GetById(Guid id)
    {
        var banner = await _context.Banners.FindAsync(id);
        if (banner == null)
            return NotFound(new { Message = "Banner not found" });

        return banner;
    }

    // ✅ POST: api/Banners
    [HttpPost]
    public async Task<ActionResult<Banner>> Create([FromBody] Banner banner)
    {
        banner.Id = Guid.NewGuid();
        _context.Banners.Add(banner);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = banner.Id }, banner);
    }

    // ✅ PUT: api/Banners/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Banner banner)
    {
        if (id != banner.Id)
            return BadRequest(new { Message = "ID mismatch" });

        var existing = await _context.Banners.FindAsync(id);
        if (existing == null)
            return NotFound(new { Message = "Banner not found" });

        existing.Title = banner.Title;
        existing.ImageURL = banner.ImageURL;
        existing.RedirectURL = banner.RedirectURL;
        existing.Latitude = banner.Latitude;
        existing.Longitude = banner.Longitude;
        existing.Radius = banner.Radius;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    // ✅ DELETE: api/Banners/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var banner = await _context.Banners.FindAsync(id);
        if (banner == null)
            return NotFound(new { Message = "Banner not found" });

        _context.Banners.Remove(banner);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Banner deleted successfully" });
    }
}
