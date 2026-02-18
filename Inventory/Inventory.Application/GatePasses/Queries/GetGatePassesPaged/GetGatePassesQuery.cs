using Inventory.Application.Common.Models;
using Inventory.Application.GatePasses.DTOs;
using MediatR;
using System;

namespace Inventory.Application.GatePasses.Queries.GetGatePassesPaged
{
    public record GetGatePassesQuery(
        int PageIndex = 0,
        int PageSize = 10,
        string? SortField = null,
        string? SortOrder = null,
        string? Filter = null,
        DateTime? FromDate = null,
        DateTime? ToDate = null
    ) : IRequest<PagedResponse<GatePassDto>>;
}
