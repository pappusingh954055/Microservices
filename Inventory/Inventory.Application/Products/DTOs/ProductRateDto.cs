using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Products.DTOs
{
    // Products.Application/Dtos/ProductRateDto.cs
    public record ProductRateDto(
        Guid ProductId,
        Guid PriceListId,
        decimal Rate
    );
}
