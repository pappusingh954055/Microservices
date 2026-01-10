using System;
using System.Collections.Generic;

namespace Inventory.Domain.PriceLists;

public class PriceList
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public DateTime ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public bool IsActive { get; private set; }

    // 🔴 IMPORTANT: NO backing field, NO private list
    public ICollection<PriceListItem> Items { get; private set; } = new List<PriceListItem>();

    private PriceList() { } // EF Core

    public PriceList(
       
        string name,
        string code,
        DateTime validFrom,
        DateTime? validTo,
        bool isActive)
    {
        Id = Guid.NewGuid();
        Name = name;
        Code = code;
        ValidFrom = validFrom;
        ValidTo = validTo;
        IsActive = isActive;
    }

    public void AddItem(Guid productId, decimal price, int minqty, int maxqty, bool isactive)
    {
        Items.Add(new PriceListItem(Guid.NewGuid(), Id, productId, price, minqty,maxqty, isactive));
    }

    public void Update(
    string name,
    string code,
    DateTime validFrom,
    DateTime? validTo,
    bool isActive)
    {
        Name = name;
        Code = code;
        ValidFrom = validFrom;
        ValidTo = validTo;
        IsActive = isActive;
    }

}
