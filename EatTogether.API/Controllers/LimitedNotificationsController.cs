using EatTogether.API.Models.EfModels;
using EatTogether.API.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EatTogether.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LimitedNotificationsController : ControllerBase
    {
        private readonly EatTogetherDBContext _db;

        public LimitedNotificationsController(EatTogetherDBContext db)
        {
            _db = db;
        }

        private int? GetMemberId()
        {
            var sub = User.FindFirstValue("sub")
                   ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(sub, out var id) ? id : null;
        }

        // GET api/LimitedNotifications
        // 回傳目前會員訂閱的所有 DishId
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            var dishIds = await _db.LimitedNotifications
                .Where(n => n.MemberId == memberId)
                .Select(n => n.DishId)
                .ToListAsync();

            return Ok(dishIds);
        }

        // POST api/LimitedNotifications/{dishId}
        // 新增提醒訂閱
        [HttpPost("{dishId:int}")]
        public async Task<IActionResult> AddNotification(int dishId)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            var exists = await _db.LimitedNotifications
                .AnyAsync(n => n.MemberId == memberId && n.DishId == dishId);

            if (exists)
                return Ok(new { message = "已訂閱此提醒" });

            _db.LimitedNotifications.Add(new LimitedNotification
            {
                MemberId = memberId.Value,
                DishId = dishId,
                CreatedAt = DateTime.Now,
            });

            await _db.SaveChangesAsync();
            return Ok(new { message = "已新增提醒訂閱" });
        }

        // DELETE api/LimitedNotifications/{dishId}
        // 取消提醒訂閱
        [HttpDelete("{dishId:int}")]
        public async Task<IActionResult> RemoveNotification(int dishId)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            var notification = await _db.LimitedNotifications
                .FirstOrDefaultAsync(n => n.MemberId == memberId && n.DishId == dishId);

            if (notification == null)
                return Ok(new { message = "本來就未訂閱此提醒" });

            _db.LimitedNotifications.Remove(notification);
            await _db.SaveChangesAsync();
            return Ok(new { message = "已取消提醒訂閱" });
        }

        // POST api/LimitedNotifications/SendReminders
        // 手動觸發寄信（展示用，forceAll = true）
        [HttpPost("SendReminders")]
        [AllowAnonymous]
        public async Task<IActionResult> SendReminders(
            [FromServices] LimitedReminderService reminderService)
        {
            await reminderService.SendRemindersAsync(forceAll: true);
            return Ok(new { message = "提醒信已發送" });
        }
    }
}
