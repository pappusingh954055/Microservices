using Inventory.Application.PurchaseReturn;
using Inventory.Application.PurchaseReturn.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseReturnController : ControllerBase
    {
        private readonly IPurchaseReturnRepository _repository;

        public PurchaseReturnController(IPurchaseReturnRepository repository)
        {
            _repository = repository;
        }


        // GET: api/PurchaseReturn/rejected-items/{supplierId}
        // Recommended Route
        [HttpGet("rejected-items/{supplierId}")]
        public async Task<IActionResult> GetRejectedItems(int supplierId)
        {
            try
            {
                var items = await _repository.GetRejectedItemsBySupplierAsync(supplierId);
                if (items == null || items.Count == 0)
                    return NotFound(new { message = "Is supplier ke liye koi rejected items nahi mile." });

                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error fetching items", error = ex.Message });
            }
        }

        [HttpGet("suppliers-with-rejections")]
        public async Task<IActionResult> GetSuppliersWithRejections()
        {
            try
            {
                var suppliers = await _repository.GetSuppliersWithRejectionsAsync();

                if (suppliers == null || suppliers.Count == 0)
                    return Ok(new List<SupplierSelectDto>()); // Khali list bhejein agar koi rejection nahi hai

                return Ok(suppliers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Suppliers load karne mein dikkat aayi", error = ex.Message });
            }
        }

        
        // POST: api/PurchaseReturn/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateReturn([FromBody] PurchaseReturnDto returnDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // DTO ko Entity mein map karein [cite: 2026-02-03]
                var returnEntity = new PurchaseReturn
                {
                    SupplierId = returnDto.SupplierId,
                    ReturnDate = returnDto.ReturnDate,
                    Remarks = returnDto.Remarks,
                    GrandTotal = 0, // Neeche calculate hoga
                    Status = "Confirmed", // Ya "Draft" as per your need [cite: 2026-02-03]
                    Items = new List<PurchaseReturnItem>()
                };

                foreach (var item in returnDto.Items)
                {
                    var itemTotal = item.ReturnQty * item.Rate;
                    returnEntity.GrandTotal += itemTotal;

                    returnEntity.Items.Add(new PurchaseReturnItem
                    {
                        ProductId = item.ProductId,
                        GrnRef = item.GrnRef,
                        ReturnQty = item.ReturnQty,
                        Rate = item.Rate,
                        TotalAmount = itemTotal
                    });
                }

                var result = await _repository.CreatePurchaseReturnAsync(returnEntity);

                if (result)
                    return Ok(new { message = "Purchase Return successfully created!", returnNumber = returnEntity.ReturnNumber });

                return StatusCode(500, "Data save karne mein kuch dikkat aayi.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
            }
        }
    }
}