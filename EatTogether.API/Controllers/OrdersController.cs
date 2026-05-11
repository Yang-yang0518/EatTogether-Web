using EatTogether.Models.DTOs;
using EatTogether.Models.Repositories;
using EatTogether.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EatTogether.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : Controller
    {
        private readonly IOrderService        _service;
        private readonly CouponService        _couponService;
        private readonly IEventRepository     _eventRepo;
        private readonly IPreOrderRepository  _preOrderRepo;

        public OrdersController(IOrderService service, CouponService couponService, IEventRepository eventRepo, IPreOrderRepository preOrderRepo)
        {
            _service       = service;
            _couponService = couponService;
            _eventRepo     = eventRepo;
            _preOrderRepo  = preOrderRepo;
        }

        [HttpGet("Tables")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTables()
        {
            try
            {
                var tables = await _service.GetTablesAsync();
                return Ok(tables);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("MenuItems")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMenuItems()
        {
            try
            {
                var items = await _service.GetMenuItemsAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CreatePreOrder")]
        [AllowAnonymous]
        public async Task<IActionResult> CreatePreOrder([FromBody] CreatePreOrderDto dto)
        {
            try
            {
                var orderNumber = await _service.CreatePreOrderAsync(dto);
                return Ok(new { orderNumber });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Favorites")]
        [AllowAnonymous] // 暫時改成 AllowAnonymous
        public async Task<IActionResult> GetFavorites([FromQuery] int? memberId)
        {
            // 先嘗試從 JWT 取，沒有就用 query parameter（測試用）
            var memberIdClaim = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            int id;
            if (!int.TryParse(memberIdClaim, out id))
            {
                if (memberId == null) return Unauthorized();
                id = memberId.Value;
            }

            var favorites = await _service.GetFavoritesAsync(id);
            return Ok(favorites);
        }

        // GET /api/Orders/ActiveEvents?amount={amount} — 點餐頁活動查詢
        [HttpGet("ActiveEvents")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveEvents([FromQuery] int amount = 0)
        {
            var autoEvents      = await _eventRepo.GetApplicableEventsAsync(amount);
            var notifyEvents    = await _eventRepo.GetNotifyEventsAsync(amount);
            var nearAutoEvents  = await _eventRepo.GetNearThresholdAutoEventsAsync(amount);

            // 已登入：過濾掉同會員當日已套用（未取消）的活動
            var memberIdStr = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(memberIdStr, out var memberId))
            {
                var usedIds = await _preOrderRepo.GetUsedEventIdsTodayByMemberAsync(memberId);
                if (usedIds.Count > 0)
                {
                    autoEvents     = autoEvents    .Where(e => !usedIds.Contains(e.Id)).ToList();
                    notifyEvents   = notifyEvents  .Where(e => !usedIds.Contains(e.Id)).ToList();
                    nearAutoEvents = nearAutoEvents.Where(e => !usedIds.Contains(e.Id)).ToList();
                }
            }

            return Ok(new { autoEvents, notifyEvents, nearAutoEvents });
        }

        // POST /api/Orders/ValidateCoupon — 點餐頁優惠券驗證
        [HttpPost("ValidateCoupon")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponRequest req)
        {
            // 從 JWT 取得登入會員 ID；未登入則為 null（CouponService 會回傳「請先登入」）
            var memberIdStr = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? memberId = int.TryParse(memberIdStr, out var mid) ? mid : null;

            var result = await _couponService.ValidateCouponAsync(req.Code, memberId, req.OrderAmount);
            return Ok(result);
        }

        // 前台外帶訂單查詢（擇一：orderNumber / name / phone）
        [HttpGet("Lookup")]
        [AllowAnonymous]
        public async Task<IActionResult> LookupOrder([FromQuery] string type, [FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q) || string.IsNullOrWhiteSpace(type))
                return BadRequest("請提供查詢條件");

            var allowed = new[] { "orderNumber", "name", "phone" };
            if (!allowed.Contains(type))
                return BadRequest("type 必須為 orderNumber / name / phone");

            var result = await _service.QueryTodayPendingTakeoutAsync(type, q.Trim());
            return Ok(result);
        }

        // 前台成功頁輪詢：依訂單編號查詢當日外帶訂單狀態
        [HttpGet("TakeoutStatus")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTakeoutStatus([FromQuery] string orderNumber)
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
                return BadRequest("請提供訂單編號");

            var status = await _preOrderRepo.GetTakeoutStatusByOrderNumberAsync(orderNumber.Trim());
            if (status == null)
                return NotFound();

            return Ok(new { orderNumber, status });
        }

        // 前台修改取餐資訊（僅限製作中 / 餐點完成，未付款前可改）
        [HttpPut("UpdatePickupInfo")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdatePickupInfo([FromBody] UpdatePickupInfoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.OrderNumber))
                return BadRequest("請提供訂單編號");

            var ok = await _service.UpdatePickupInfoAsync(dto);
            if (!ok) return BadRequest("訂單不存在或已無法修改");
            return Ok();
        }

        // 會員歷史訂單
        [HttpGet("MemberOrderHistory")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMemberOrderHistory([FromQuery] int? memberId)
        {
            var memberIdClaim = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            int id;
            if (!int.TryParse(memberIdClaim, out id))
            {
                if (memberId == null) return Unauthorized();
                id = memberId.Value;
            }
            var history = await _service.GetMemberOrderHistoryAsync(id);
            return Ok(history);
        }

		// ── 前台會員中心訂單紀錄頁專用 ────────────────
		[Authorize]
		[HttpGet("~/api/members/me/orders")]
		public async Task<IActionResult> GetMyOrders(
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 10,
			[FromQuery] DateTime? dateFrom = null,
			[FromQuery] DateTime? dateTo = null)
		{
			var memberIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
							 ?? User.FindFirstValue("sub");

			if (!int.TryParse(memberIdClaim, out int memberId))
				return Unauthorized();

			var result = await _service.GetPagedOrdersAsync(memberId, page, pageSize, dateFrom, dateTo);
			return Ok(result);
		}
	}
}