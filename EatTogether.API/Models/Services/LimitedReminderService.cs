using EatTogether.API.Models.EfModels;
using EatTogether.API.Models.Infra;
using Microsoft.EntityFrameworkCore;

namespace EatTogether.API.Models.Services
{
    public class LimitedReminderService
    {
        private readonly EatTogetherDBContext _db;
        private readonly IEmailService _email;
        private readonly ILogger<LimitedReminderService> _logger;

        public LimitedReminderService(
            EatTogetherDBContext db,
            IEmailService email,
            ILogger<LimitedReminderService> logger)
        {
            _db     = db;
            _email  = email;
            _logger = logger;
        }

        /// <param name="forceAll">false = 只寄明天到期；true = 全部訂閱（展示用）</param>
        public async Task SendRemindersAsync(bool forceAll = false)
        {
            var tomorrow = DateOnly.FromDateTime(DateTime.Now.AddDays(1));

            var query = _db.LimitedNotifications
                .Include(n => n.Member)
                .Include(n => n.Dish)
                .Where(n => n.Member != null
                         && n.Dish  != null
                         && !string.IsNullOrWhiteSpace(n.Member.Email));

            if (!forceAll)
                query = query.Where(n => n.Dish.EndDate == tomorrow);

            var notifications = await query.ToListAsync();

            foreach (var n in notifications)
            {
                try
                {
                    var endDate = n.Dish.EndDate ?? tomorrow;
                    await _email.SendLimitedReminderAsync(
                        n.Member.Email, n.Member.Name, n.Dish.DishName, endDate,
                        n.Dish.Description, n.Dish.Price);

                    _logger.LogInformation(
                        "限定餐點提醒已寄出：MemberId={mid}, DishId={did}",
                        n.MemberId, n.DishId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "寄送限定餐點提醒失敗：MemberId={mid}, DishId={did}",
                        n.MemberId, n.DishId);
                }
            }
        }
    }
}
