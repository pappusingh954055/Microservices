using MediatR;

public record CreatePriceListCommand(
    string name,
    string priceType,
    string code,
    DateTime validFrom,
    DateTime? validTo,
    bool isActive,
    List<PriceListItemDto> priceListItems // Angular ke array name se match karein
) : IRequest<Guid>;