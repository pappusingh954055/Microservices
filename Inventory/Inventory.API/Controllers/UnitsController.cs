using Inventory.Application.Unit.Command;
using Inventory.Application.Unit.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public UnitsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk([FromBody] CreateBulkUnitsCommand command)
        {
            var result = await _mediator.Send(command);
            return result ? Ok() : BadRequest("Could not save units");
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetAll()
            => Ok(await _mediator.Send(new GetAllUnitsQuery()));
    }
}
