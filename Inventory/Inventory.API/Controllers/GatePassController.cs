using Inventory.API.Common;
using Inventory.Application.GatePasses.Commands.CreateGatePass;
using Inventory.Application.GatePasses.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Inventory.API.Controllers
{
    [ApiController]
    [Route("api/inventory/gate-passes")]
    public class GatePassController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GatePassController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Save")]
        [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
        public async Task<IActionResult> Create(CreateGatePassCommand command)
        {
            var result = await _mediator.Send(command);
            
            return Ok(ApiResponse<GatePassDto>.Ok(
                result,
                "Gate Pass generated successfully"
            ));
        }

        // Add Get/List later if requested
    }
}
