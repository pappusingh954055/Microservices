using Inventory.Application.Features.PurchaseOrders.Queries;
using Inventory.Application.PurchaseOrders.Commands.Delete;
using Inventory.Application.PurchaseOrders.Commands.Update;
using Inventory.Application.PurchaseOrders.DTOs;
using Inventory.Application.PurchaseOrders.Queries.GetNextPoNumber;
using Inventory.Application.PurchaseOrders.Queries.GetPurchaseOrder;
using Inventory.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PurchaseOrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("next-number")]
        [Authorize(Roles = "Admin, User, Manager")]
        public async Task<IActionResult> GetNextNumber()
        {
            // MediatR command bhej raha hai handler ko
            var result = await _mediator.Send(new GetNextPoNumberQuery());
            return Ok(new { poNumber = result });
        }

        [HttpPost("save-po")]
        [Authorize(Roles = "Manager, Admin,User")]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderDto dto)
        {
           
            var result = await _mediator.Send(new CreatePurchaseOrderCommand(dto));

            if (result)
                return Ok(new { success = true, message = "Purchase Order Draft saved successfully!" });

            return BadRequest(new { success = false, message = "Failed to save PO." });
        }

        [HttpGet]
        //[Authorize(Roles = "Manager, Admin")]
        public async Task<ActionResult> GetOrders([FromQuery] GetPurchaseOrdersQuery query)
        {
            // Ensure 'query' is not null
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("query")]
        [Authorize(Roles = "Manager, Admin,User")]
        public async Task<IActionResult> GetOrders([FromBody] GetPurchaseOrdersRequest request)
        {
            var query = new GetDateRangePurchaseOrdersQuery(request);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("get-paged-orders")]
        [Authorize(Roles = "Admin,Manager, Warehouse,User")]
        public async Task<IActionResult> GetPagedOrders([FromBody] GetPurchaseOrdersRequest request)
        {
            // Frontend se aane wale request DTO ko query mein wrap kar rahe hain
            var query = new GetDateRangePurchaseOrdersQuery(request);

            // Mediator isse sahi Handler tak pahuchayega
            var response = await _mediator.Send(query);

            return Ok(response);
        }

        [Authorize(Roles = "Admin, User,Manager")]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            // MediatR ke through Query ko Handler tak bhejna
            var query = new GetPurchaseOrderByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(new
                {
                    status = "error",
                    message = $"Purchase Order with ID {id} not found."
                });
            }

            return Ok(result);
        }

        [Authorize(Roles = "Manager, Admin,User")]
        [HttpPut("{id}")] //
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePurchaseOrderDto dto)
        {
            // 1. Validation: URL ID aur Body ID match honi chahiye
            if (id != dto.Id)
            {
                return BadRequest(new { message = "ID mismatch between URL and body." });
            }

            // 2. Command Create karna
            var command = new UpdatePurchaseOrderCommand(dto);

            // 3. Mediator ke through handler ko call karna
            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound(new { message = $"Purchase Order with ID {id} not found or update failed." });
            }

            // 4. Success Response
            return Ok(new
            {
                status = "success",
                message = "Purchase Order updated successfully"
            });
        }



        /// <summary>
        /// URL: DELETE /api/PurchaseOrders/{id}
        /// ye single record delete karega
        /// Frontend call: this.http.delete(`${this.apiUrl}/PurchaseOrders/${poId}`)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // 1. Mediator ke through Command bhej rahe hain Handler ko
                var result = await _mediator.Send(new DeletePurchaseOrderCommand(id));

                if (!result)
                {
                    return NotFound(new { success = false, message = "This PO is not found in database." });
                }

                // 2. Agar success hua toh 200 OK
                return Ok(new { success = true, message = "Purchase Order deleted successfully." });
            }
            catch (InvalidOperationException ex)
            {
                // 3. Domain Rule fail hua (e.g., Status 'Received' tha)
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                // 4. Koi aur technical error
                return StatusCode(500, new { success = false, message = "Internal server error: " + ex.Message });
            }
        }


        // --- 2. BULK PARENT DELETE ---
        // URL: POST /api/PurchaseOrders/bulk-delete
        // Frontend call: this.http.post(`${this.apiUrl}/PurchaseOrders/bulk-delete`, { ids })
        [HttpPost("bulk-delete-orders")] // Name easily identifiable
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkDeleteOrders([FromBody] BulkDeletePurchaseOrderCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return Ok(new { success = true, message = "Selected orders is deleted!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // --- 3. BULK CHILD ITEMS DELETE ---
        [HttpPost("bulk-delete-items")] // Easily identifiable name
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkDeleteItems([FromBody] BulkDeletePOItemsCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound(new { success = false, message = "Did not found PO items." });

                return Ok(new { success = true, message = "Selected items successfully removed!" });
            }
            catch (InvalidOperationException ex)
            {
                // Agar status "Received" nikla toh ye error throw karega
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Update status
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>

        [HttpPut("UpdateStatus")]
        [Authorize(Roles = "User,Manager")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusDTO dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Status))
                return BadRequest("Data sahi nahi hai");

            var command = new UpdatePOStatusCommand(dto.Id, dto.Status);
            var result = await _mediator.Send(command);

            if (result)
                return Ok(new { message = "Status Updated to " + dto.Status });

            return NotFound("PO nahi mila");
        }

        [HttpGet("pending-pos")]
        public async Task<IActionResult> GetPendingPOs()
        {
            var result = await _mediator.Send(new GetPendingPOQuery());
            return Ok(result);
        }

        [HttpGet("po-items/{poId}")]
        public async Task<IActionResult> GetPOItemsForGRN(int poId)
        {
            var result = await _mediator.Send(new GetPOItemsForGRNQuery(poId));
            return Ok(result);
        }

        /// <summary>
        /// Dashboard se lastPurchaseOrderId (int) lekar Header details fetch karta hai
        /// </summary>
        /// <param name="lastPurchaseOrderId">Integer format ID</param>
        [HttpGet("header-details/{lastPurchaseOrderId:int}")]
        public async Task<ActionResult<POHeaderDetailsDto>> GetHeaderDetails(int lastPurchaseOrderId)
        {
            // 1. Query create karein [cite: 2026-01-22]
            var query = new GetPOHeaderDetailsQuery(lastPurchaseOrderId);

            // 2. MediatR se Handler trigger karein [cite: 2026-01-22]
            var result = await _mediator.Send(query);

            // 3. Validation
            if (result == null)
            {
                return NotFound($"Previous Purchase Order with ID {lastPurchaseOrderId} not found.");
            }

            // 4. POHeaderDetailsDto return karein
            return Ok(result);
        }

    }
}


