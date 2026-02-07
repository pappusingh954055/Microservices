using Inventory.Application.Common.Interfaces;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

public class DashboardRepository : IDashboardRepository
{
    private readonly InventoryDbContext _context;

    public DashboardRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        // AsNoTracking performance ke liye zaroori hai
        var purchaseOrders = _context.PurchaseOrders.AsNoTracking();
        var products = _context.Products.AsNoTracking();
        var saleOrders = _context.SaleOrders.AsNoTracking();

        return new DashboardSummaryDto
        {
            // 1. Total Sales: SaleOrders table ke GrandTotal column ka sum
            TotalSales = await saleOrders.SumAsync(x => x.GrandTotal),

            // 2. Pending Purchase Orders: Status "Pending" wale orders ka count
            PendingPurchaseOrders = await purchaseOrders.CountAsync(x => x.Status == "Pending"),

            // 3. Total Stock Units: Products table ke CurrentStock column ka sum
            TotalStockItems = (int)await products.SumAsync(x => x.CurrentStock),

            // 4. Low Stock Alert: CurrentStock jab MinStock se kam ya barabar ho
            LowStockAlertCount = await products.CountAsync(x => x.CurrentStock <= x.MinStock)
        };
    }

    public async Task<DashboardChartDto> GetDashboardChartsAsync()
    {
        var currentYear = DateTime.Now.Year;

        // Sales Trends: SODate ke basis par month-wise group karke GrandTotal sum karna
        var salesTrends = await _context.SaleOrders
            .AsNoTracking()
            .Where(x => x.SODate.Year == currentYear)
            .GroupBy(x => x.SODate.Month)
            .Select(g => new { Month = g.Key, Total = g.Sum(x => x.GrandTotal) })
            .ToListAsync();

        // Purchase Trends: PoDate ke basis par month-wise group karke GrandTotal sum karna
        var purchaseTrends = await _context.PurchaseOrders
            .AsNoTracking()
            .Where(x => x.PoDate.Year == currentYear)
            .GroupBy(x => x.PoDate.Month)
            .Select(g => new { Month = g.Key, Total = g.Sum(x => x.GrandTotal) })
            .ToListAsync();

        var chart = new DashboardChartDto();

        // Jan se July tak ke labels aur real data mapping
        for (int i = 1; i <= 7; i++)
        {
            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i);
            chart.Labels.Add(monthName);

            chart.SalesData.Add(salesTrends.FirstOrDefault(x => x.Month == i)?.Total ?? 0);
            chart.PurchaseData.Add(purchaseTrends.FirstOrDefault(x => x.Month == i)?.Total ?? 0);
        }

        // Stock Distribution (Donut Chart)
        // Example: IsActive products ko Finished Goods maan rahe hain
        chart.FinishedGoods = await _context.Products.CountAsync(x => x.IsActive);
        chart.RawMaterials = await _context.Products.CountAsync(x => !x.IsActive);
        chart.DamagedItems = 0; // Agar damaged flag nahi hai toh default 0

        return chart;
    }

    public async Task<List<RecentActivityDto>> GetRecentActivitiesAsync()
    {
        // 1. Sales Transactions (Using SaleOrders + SaleOrderItems)
        var sales = await _context.SaleOrderItems
            .AsNoTracking()
            .OrderByDescending(x => x.SaleOrder.SODate)
            .Take(5)
            .Select(x => new RecentActivityDto
            {
                Product = x.ProductName, // SaleOrderItems mein ProductName direct hai
                Type = "Sale",
                Qty = x.Qty,
                Date = x.SaleOrder.SODate,
                Status = x.SaleOrder.Status
            }).ToListAsync();

        // 2. Purchase Transactions (Using PurchaseOrders + PurchaseOrderItems + Products)
        var purchases = await _context.PurchaseOrderItems
            .AsNoTracking()
            .OrderByDescending(x => x.PurchaseOrder.PoDate) // Ab ye navigation property kaam karegi
            .Take(5)
            .Select(x => new RecentActivityDto
            {
                // Product table se Name lene ke liye direct navigation use karein
                Product = x.Product != null ? x.Product.Name : "Unknown",
                Type = "Purchase",
                Qty = x.Qty,
                Date = x.PurchaseOrder.PoDate,
                Status = x.PurchaseOrder.Status
            }).ToListAsync();

        // 3. Dono ko combine karke dashboard ke liye final Top 5 transactions nikalna
        return sales.Concat(purchases)
            .OrderByDescending(x => x.Date)
            .Take(5)
            .ToList();
    }
}