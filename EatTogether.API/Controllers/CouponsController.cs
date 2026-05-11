using EatTogether.Models.DTOs;
using EatTogether.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EatTogether.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CouponsController : ControllerBase
    {
        private readonly CouponService _service;

        public CouponsController(CouponService service)
        {
            _service = service;
        }

        // ─── 取得 MemberId（未登入回 null）────────────────────────────
        private int? GetMemberId()
        {
            var sub = User.FindFirstValue("sub")
                   ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(sub, out var id) ? id : null;
        }

        // GET api/Coupons?discountType=0&birthdayOnly=false
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? discountType  = null,
            [FromQuery] bool birthdayOnly  = false)
        {
            var memberId = GetMemberId();
            var coupons  = await _service.GetAvailableCouponsAsync(memberId);

            if (discountType.HasValue)
                coupons = coupons.Where(c => c.DiscountType == discountType.Value).ToList();

            if (birthdayOnly)
                coupons = coupons.Where(c => c.Code.StartsWith("BDAY")).ToList();

            return Ok(coupons);
        }

        // GET api/Coupons/Upcoming
        [HttpGet("Upcoming")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUpcoming()
        {
            var coupons = await _service.GetUpcomingCouponsAsync();
            return Ok(coupons);
        }

        // POST api/Coupons/{id}/Claim  [需登入]
        [HttpPost("{id:int}/Claim")]
        [Authorize]
        public async Task<IActionResult> Claim(int id)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            var result = await _service.ClaimCouponAsync(id, memberId.Value);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { message = "領取成功！已加入您的優惠券" });
        }

        // POST api/Coupons/ClaimByCode  [需登入]
        [HttpPost("ClaimByCode")]
        [Authorize]
        public async Task<IActionResult> ClaimByCode([FromBody] ClaimByCodeRequest req)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            var result = await _service.ClaimByCodeAsync(req.Code, memberId.Value);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { message = "領取成功！已加入您的優惠券" });
        }

        // GET api/Coupons/My  [需登入]
        [HttpGet("My")]
        [Authorize]
        public async Task<IActionResult> GetMyCoupons()
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            var list = await _service.GetMyCouponsAsync(memberId.Value);
            return Ok(list);
        }

        // POST api/Coupons/Validate
        [HttpPost("Validate")]
        [AllowAnonymous]
        public async Task<IActionResult> Validate([FromBody] ValidateCouponRequest req)
        {
            var memberId = GetMemberId();
            var result   = await _service.ValidateCouponAsync(req.Code, memberId, req.OrderAmount);
            return Ok(result);
        }

        // GET api/Coupons/BirthdayCheck  [需登入]
        [HttpGet("BirthdayCheck")]
        [Authorize]
        public async Task<IActionResult> BirthdayCheck()
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            var result = await _service.GetBirthdayCheckAsync(memberId.Value);
            return Ok(result);
        }

        // GET api/Coupons/My/History  [需登入]
        [HttpGet("My/History")]
        [Authorize]
        public async Task<IActionResult> GetUsageHistory()
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            var list = await _service.GetUsageHistoryAsync(memberId.Value);
            return Ok(list);
        }
    }

    public class ClaimByCodeRequest
    {
        public string Code { get; set; } = string.Empty;
    }

    public class ValidateCouponRequest
    {
        public string Code { get; set; } = string.Empty;
        public decimal OrderAmount { get; set; }
    }
}
