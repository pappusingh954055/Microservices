using Inventory.Application.Common.Interfaces;
using Inventory.Application.Common.Models;
using Inventory.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/expense-entries")]
public class ExpenseEntriesController : ControllerBase
{
    private readonly IInventoryDbContext _context;

    public ExpenseEntriesController(IInventoryDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse, Super Admin")]
    public async Task<IActionResult> GetList([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, [FromQuery] string? search = null)
    {
        var query = _context.ExpenseEntries
            .Include(x => x.Category)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(x => (x.Category != null && x.Category.Name.Contains(search)) || 
                                     (x.Remarks != null && x.Remarks.Contains(search)) ||
                                     (x.ReferenceNo != null && x.ReferenceNo.Contains(search)));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.ExpenseDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { items, totalCount });
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse, Super Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _context.ExpenseEntries
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);
        
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse, Super Admin")]
    public async Task<IActionResult> Create(ExpenseEntry entry)
    {
        _context.ExpenseEntries.Add(entry);
        await _context.SaveChangesAsync();
        return Ok(entry);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse, Super Admin")]
    public async Task<IActionResult> Update(int id, ExpenseEntry entry)
    {
        if (id != entry.Id) return BadRequest();

        var existing = await _context.ExpenseEntries.FindAsync(id);
        if (existing == null) return NotFound();

        existing.CategoryId = entry.CategoryId;
        existing.Amount = entry.Amount;
        existing.ExpenseDate = entry.ExpenseDate;
        existing.PaymentMode = entry.PaymentMode;
        existing.ReferenceNo = entry.ReferenceNo;
        existing.Remarks = entry.Remarks;
        existing.AttachmentPath = entry.AttachmentPath;
        existing.UpdatedDate = DateTime.Now;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse, Super Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var entry = await _context.ExpenseEntries.FindAsync(id);
        if (entry == null) return NotFound();

        _context.ExpenseEntries.Remove(entry);
        await _context.SaveChangesAsync();
        return Ok(new { success = true, message = "Expense entry deleted successfully" });
    }

    [HttpPost("chart-data")]
    [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse, Super Admin")]
    public async Task<IActionResult> GetChartData([FromBody] DashboardFilter filters)
    {
        var query = _context.ExpenseEntries
            .Include(x => x.Category)
            .AsQueryable();

        if (filters.StartDate.HasValue)
            query = query.Where(x => x.ExpenseDate >= filters.StartDate.Value);
        
        if (filters.EndDate.HasValue)
            query = query.Where(x => x.ExpenseDate <= filters.EndDate.Value);

        var data = await query
            .GroupBy(x => x.Category!.Name)
            .Select(g => new
            {
                Category = g.Key,
                Amount = g.Sum(x => x.Amount)
            })
            .ToListAsync();
        
        return Ok(data);
    }

    [HttpGet("monthly-totals")]
    [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse, Super Admin")]
    public async Task<IActionResult> GetMonthlyTotals([FromQuery] int months = 6)
    {
        var startDate = DateTime.Today.AddMonths(-(months - 1));
        startDate = new DateTime(startDate.Year, startDate.Month, 1);

        var data = await _context.ExpenseEntries
            .Where(x => x.ExpenseDate >= startDate)
            .ToListAsync(); // Load to memory for grouping by month name string if needed, or do it in SQL

        var trend = data
            .GroupBy(x => new { x.ExpenseDate.Year, x.ExpenseDate.Month })
            .Select(g => new
            {
                Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                Amount = g.Sum(x => x.Amount)
            })
            .OrderBy(t => DateTime.Parse(t.Month))
            .ToList();

        return Ok(trend);
    }
}

public class DashboardFilter
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
