using EatTogether.API.Models.EfModels;
using EatTogether.Models.Infra;
using Microsoft.EntityFrameworkCore;

namespace EatTogether.Models.Repositories
{
    public interface IPreOrderRepository
    {
        // Create
        Task<List<PreOrder>> GetByStatusAsync(int doneOrCancel); // string → int
        /// <summary>前台訂單查詢用：當日外帶未完成 + 結帳後 30 分鐘內已完成</summary>
        Task<List<PreOrder>> GetTodayTakeoutForLookupAsync();
        Task AddAsync(PreOrder preOrder);
        Task<int> CountTodayAsync(DateTime date);
        Task<List<PreOrder>> GetActiveByTableIdAsync(int tableId);
        Task CancelAllByTableIdAsync(int tableId);
        Task SaveChangesAsync();

        // List
        Task UpdateDetailStatusAsync(int detailId, int status);
        Task<List<PreOrder>> GetAllAsync();
        Task<int> GetPreOrderIdByDetailIdAsync(int detailId);
        Task UpdateStatusAsync(int id, int doneOrCancel);

        // Payment
        Task CancelUnservedDetailsAsync(int preOrderId, ISet<int>? excludeDetailIds = null);
        Task<PreOrder?> GetByIdAsync(int id);
        Task CancelEntireOrderAsync(int preOrderId);
        Task UpdateDetailBilledAsync(int detailId);
        Task<bool> HasUnbilledDetailsForTableAsync(int tableId);
        Task<bool> AllNonCancelledDetailsBilledAsync(int preOrderId);
        Task UpdateTableAsync(int preOrderId, int? tableId, bool inOrOut);

        // 前台重複使用防呆
        /// <summary>取得當日會員已套用（未取消）的活動 ID 集合</summary>
        Task<HashSet<int>> GetUsedEventIdsTodayByMemberAsync(int memberId);
        /// <summary>依訂單編號查詢當日外帶訂單狀態（前台成功頁輪詢用）</summary>
        Task<int?> GetTakeoutStatusByOrderNumberAsync(string orderNumber);
        /// <summary>當日會員是否已在未取消的訂單中套用此優惠券</summary>
        Task<bool> IsCouponInActiveTodayOrderAsync(int couponId, int memberId);
        /// <summary>依訂單編號取得當日外帶訂單實體（修改取餐資訊用）</summary>
        Task<PreOrder?> GetTodayTakeoutByOrderNumberAsync(string orderNumber);
    }

    public class PreOrderRepository : IPreOrderRepository
    {
        private readonly EatTogetherDBContext _context;
        public PreOrderRepository(EatTogetherDBContext db) => _context = db;

        // Create -----------------------------------------------------------------------------------------
        public async Task<List<PreOrder>> GetByStatusAsync(int doneOrCancel) =>
            await _context.PreOrders
                     .Include(p => p.PreOrderDetails)
                     .Include(p => p.Table)
                     .Include(p => p.Event)
                     .Include(p => p.Coupon)
                     .Where(p => p.DoneOrCancel == doneOrCancel)
                     .ToListAsync();

        public async Task<List<PreOrder>> GetTodayTakeoutForLookupAsync()
        {
            var today    = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return await _context.PreOrders
                .Include(p => p.PreOrderDetails)
                .Include(p => p.Event)
                .Include(p => p.Coupon)
                .Include(p => p.Payments)
                .Where(p => p.OrderAt >= today && p.OrderAt < tomorrow
                         && !p.InOrOut)   // 不過濾 DoneOrCancel，包含未完成、已完成、已取消
                .ToListAsync();
        }
        public async Task AddAsync(PreOrder preOrder)
        {
            _context.PreOrders.Add(preOrder);
            await _context.SaveChangesAsync();
        }
        public async Task<int> CountTodayAsync(DateTime date)
        {
            return await _context.PreOrders
                .Where(p => p.OrderAt.Date == date.Date)
                .CountAsync();
        }

        public async Task<List<PreOrder>> GetActiveByTableIdAsync(int tableId)
        {
            var today = DateTime.Today;
            return await _context.PreOrders
                .Include(p => p.PreOrderDetails)
                .Include(p => p.Member)
                .Include(p => p.Coupon)
                .Include(p => p.Event)
                .Where(p => p.TableId == tableId
                         && p.DoneOrCancel == 0
                         && p.OrderAt.Date == today)
                .ToListAsync();
        }

        public async Task CancelAllByTableIdAsync(int tableId)
        {
            var orders = await GetActiveByTableIdAsync(tableId);
            foreach (var order in orders)
            {
                order.DoneOrCancel = 2;
                order.CancelledAt = DateTime.Now;
                // 改成（用 OrderNoteHelper 正確處理）
                var parsed = OrderNoteHelper.Parse(order.Note);
                order.Note = OrderNoteHelper.Build(
                    parsed.Order,
                    parsed.Items,
                    parsed.CustomerName,
                    parsed.CustomerPhone,
                    parsed.PickupTime);
                foreach (var detail in order.PreOrderDetails)
                    detail.DoneOrCancel = 2;
            }
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

        // List -----------------------------------------------------------------------------------------
        public async Task UpdateDetailStatusAsync(int detailId, int status)
        {
            var detail = await _context.PreOrderDetails.FindAsync(detailId);
            if (detail is null) return;

            detail.DoneOrCancel = status;
            await _context.SaveChangesAsync();
        }
        public async Task<List<PreOrder>> GetAllAsync() =>
            await _context.PreOrders
                     .Include(p => p.PreOrderDetails)
                     .Include(p => p.Table)
                     .Include(p => p.Member)
                     .Include(p => p.Coupon)
                     .Include(p => p.Event)
                     .OrderByDescending(p => p.OrderAt)
                     .ToListAsync();
        public async Task<int> GetPreOrderIdByDetailIdAsync(int detailId)
        {
            var detail = await _context.PreOrderDetails.FindAsync(detailId);
            return detail?.PreOrderId ?? 0;
        }
        public async Task UpdateStatusAsync(int id, int doneOrCancel)
        {
            var entity = await _context.PreOrders.FindAsync(id);
            if (entity is null) return;
            entity.DoneOrCancel = doneOrCancel;
            await _context.SaveChangesAsync();
        }

        // Payment -----------------------------------------------------------------------------------------
        public async Task CancelUnservedDetailsAsync(int preOrderId, ISet<int>? excludeDetailIds = null)
        {
            var query = _context.PreOrderDetails
                .Where(d => d.PreOrderId == preOrderId && d.DoneOrCancel == 0);

            if (excludeDetailIds?.Count > 0)
                query = query.Where(d => !excludeDetailIds.Contains(d.Id));

            var details = await query.ToListAsync();

            foreach (var d in details)
                d.DoneOrCancel = 2;

            await _context.SaveChangesAsync();
        }
        public async Task<PreOrder?> GetByIdAsync(int id) =>
            await _context.PreOrders
                     .Include(p => p.PreOrderDetails)
                     .Include(p => p.Table)
                     .Include(p => p.User)
                     .Include(p => p.Member)
                     .Include(p => p.Coupon)
                     .Include(p => p.Event)
                     .Include(p => p.Payments)
                     .FirstOrDefaultAsync(p => p.Id == id);

        public async Task CancelEntireOrderAsync(int preOrderId)
        {
            var order = await _context.PreOrders
                .Include(p => p.PreOrderDetails)
                .FirstOrDefaultAsync(p => p.Id == preOrderId);

            if (order == null) return;

            order.DoneOrCancel = 2; // Cancelled
            order.CancelledAt = DateTime.Now;
            // 改成（用 OrderNoteHelper 正確處理）
            var parsed = OrderNoteHelper.Parse(order.Note);
            order.Note = OrderNoteHelper.Build(
                parsed.Order,
                parsed.Items,
                parsed.CustomerName,
                parsed.CustomerPhone,
                parsed.PickupTime);

            foreach (var detail in order.PreOrderDetails)
            {
                detail.DoneOrCancel = 2; // Cancelled
            }

            await _context.SaveChangesAsync();
        }
        public async Task UpdateDetailBilledAsync(int detailId)
        {
            var detail = await _context.PreOrderDetails.FindAsync(detailId);
            if (detail is null) return;
            detail.IsBilled = true;
            await _context.SaveChangesAsync();
        }
        public async Task<bool> HasUnbilledDetailsForTableAsync(int tableId)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            return await _context.PreOrderDetails
                .AnyAsync(d => d.PreOrder.TableId == tableId
                            && d.PreOrder.DoneOrCancel == 0
                            && d.PreOrder.OrderAt >= today
                            && d.PreOrder.OrderAt < tomorrow
                            && !d.IsBilled
                            && d.DoneOrCancel != 2);
        }

        public async Task<bool> AllNonCancelledDetailsBilledAsync(int preOrderId)
        {
            var details = await _context.PreOrderDetails
                .Where(d => d.PreOrderId == preOrderId && d.DoneOrCancel != 2)
                .ToListAsync();

            return details.Any() && details.All(d => d.IsBilled);
        }

        public async Task UpdateTableAsync(int preOrderId, int? tableId, bool inOrOut)
        {
            var order = await _context.PreOrders.FindAsync(preOrderId);
            if (order == null) return;
            order.TableId = tableId;
            order.InOrOut = inOrOut;
            await _context.SaveChangesAsync();
        }

        // ── 前台重複使用防呆 ──────────────────────────────────────────────

        public async Task<HashSet<int>> GetUsedEventIdsTodayByMemberAsync(int memberId)
        {
            var today    = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var ids = await _context.PreOrders
                .Where(p => p.MemberId == memberId
                         && p.EventId != null
                         && p.DoneOrCancel != 2          // 未取消
                         && p.OrderAt >= today
                         && p.OrderAt < tomorrow)
                .Select(p => p.EventId!.Value)
                .Distinct()
                .ToListAsync();
            return ids.ToHashSet();
        }

        public async Task<bool> IsCouponInActiveTodayOrderAsync(int couponId, int memberId)
        {
            var today    = DateTime.Today;
            var tomorrow = today.AddDays(1);
            return await _context.PreOrders
                .AnyAsync(p => p.MemberId  == memberId
                            && p.CouponId  == couponId
                            && p.DoneOrCancel != 2       // 未取消（已完整取消的訂單不佔用優惠券）
                            && p.OrderAt >= today
                            && p.OrderAt < tomorrow
                            // 至少有一筆未取消的明細；若所有明細已取消（CancelUnservedDetails 路徑），
                            // 則視同訂單無效，釋放優惠券讓會員重新使用
                            && p.PreOrderDetails.Any(d => d.DoneOrCancel != 2));
        }

        public async Task<int?> GetTakeoutStatusByOrderNumberAsync(string orderNumber)
        {
            var today    = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var order = await _context.PreOrders
                .Include(p => p.PreOrderDetails)
                .Where(p => p.OrderNumber == orderNumber
                         && !p.InOrOut
                         && p.OrderAt >= today
                         && p.OrderAt < tomorrow)
                .FirstOrDefaultAsync();
            if (order == null) return null;
            return ComputeTakeoutStatus(order.DoneOrCancel, order.PreOrderDetails);
        }

        public async Task<PreOrder?> GetTodayTakeoutByOrderNumberAsync(string orderNumber)
        {
            var today    = DateTime.Today;
            var tomorrow = today.AddDays(1);
            return await _context.PreOrders
                .Include(p => p.PreOrderDetails)
                .Where(p => p.OrderNumber == orderNumber
                         && !p.InOrOut
                         && p.OrderAt >= today
                         && p.OrderAt < tomorrow)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 計算外帶訂單的前台顯示狀態：
        /// 0=製作中　1=餐點已完成（待取餐）　2=已取消　3=已結帳
        /// </summary>
        public static int ComputeTakeoutStatus(int doneOrCancel, ICollection<PreOrderDetail> details)
        {
            if (doneOrCancel == 2) return 2; // 已取消
            if (doneOrCancel == 1) return 3; // 已結帳

            // DoneOrCancel == 0：檢查明細是否全部完成
            if (details == null || !details.Any() || details.Any(d => d.DoneOrCancel == 0))
                return 0; // 仍有未完成明細 → 製作中

            // 全部明細為 1（完成）或 2（取消），且至少一筆已完成
            return details.Any(d => d.DoneOrCancel == 1) ? 1 : 2;
        }
    }
}
