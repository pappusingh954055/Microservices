using Microsoft.EntityFrameworkCore;
using Suppliers.Application.DTOs;

public class SupplierRepository : ISupplierRepository
{
    private readonly SupplierDbContext _context;

    public SupplierRepository(SupplierDbContext context)
    {
        _context = context;
    }

    public IQueryable<Supplier> Query() => _context.Suppliers.AsNoTracking();

    public async Task<IEnumerable<Supplier>> GetAllAsync() =>
        await _context.Suppliers.AsNoTracking().ToListAsync();

    public async Task<Supplier?> GetByIdAsync(int id)
    {
        // AsNoTracking performance ke liye behtar hai read operations mein
        return await _context.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddAsync(Supplier supplier)
    {
        await _context.Suppliers.AddAsync(supplier);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Supplier supplier)
    {
        _context.Suppliers.Update(supplier);
        await _context.SaveChangesAsync();
    }
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Suppliers.AnyAsync(s => s.Id == id);
    }
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task<List<SupplierSelectDto>> GetSuppliersByIdsAsync(List<int> ids)
    {
        // 1. Safety check
        if (ids == null || !ids.Any()) return new List<SupplierSelectDto>();

        // 2. Fresh data fetch logic [cite: 2026-02-03]
        var suppliers = await _context.Suppliers
            .AsNoTracking() // Cache skip karke fresh DB query chalaye [cite: 2026-02-03]
            .Where(s => ids.Contains(s.Id)) // SQL mein 'WHERE Id IN (1)' banayega
            .OrderBy(s => s.Name)
            .Select(s => new SupplierSelectDto
            {
                Id = s.Id,
                Name = s.Name
            })
            .ToListAsync();

        return suppliers ?? new List<SupplierSelectDto>();
    }
}