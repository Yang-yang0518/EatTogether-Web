using EatTogether.API.Models.EfModels;
using Microsoft.EntityFrameworkCore;

namespace EatTogether.Models.Repositories
{
    public interface IOrderRepository 
    {
        Task AddWithPaymentAsync(Order order, Payment payment);
        Task<List<Order>> GetRecentByMemberIdAsync(int memberId, int take = 5);

		// ── 前台會員中心訂單紀錄頁專用 ────────────────
		Task<List<Order>> GetPagedByMemberIdAsync(int memberId, int page, int pageSize, DateTime? dateFrom, DateTime? dateTo);
		Task<int> GetTotalCountByMemberIdAsync(int memberId, DateTime? dateFrom, DateTime? dateTo);
	}
    public class OrderRepository : IOrderRepository 
    {
        private readonly EatTogetherDBContext _context;
        public OrderRepository(EatTogetherDBContext db) => _context = db;
        public async Task AddWithPaymentAsync(Order order, Payment payment)
        {
            // 先存 Payment 取得 Id
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // 再存 Order，帶入 PaymentId
            order.PaymentId = payment.Id;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 回填 Payment.OrderId
            payment.OrderId = order.Id;
            await _context.SaveChangesAsync();
        }
        public async Task<List<Order>> GetRecentByMemberIdAsync(int memberId, int take = 5)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.PreOrder)
                    .ThenInclude(po => po.PreOrderDetails)  // 備援：OrderDetails 空時改用 PreOrderDetails
                .Where(o => o.MemberId == memberId)
                .OrderByDescending(o => o.OrderAt)
                .Take(take)
                .ToListAsync();
        }

		// ── 前台會員中心訂單紀錄頁專用 ────────────────
		public async Task<List<Order>> GetPagedByMemberIdAsync(int memberId, int page, int pageSize, DateTime? dateFrom, DateTime? dateTo)
		{
			var query = _context.Orders
				.Include(o => o.PreOrder)
					.ThenInclude(po => po.PreOrderDetails)
				.Include(o => o.PreOrder.Event)
				.Include(o => o.Coupon)
				.Where(o => o.MemberId == memberId);

			if (dateFrom.HasValue)
				query = query.Where(o => o.OrderAt >= dateFrom.Value);

			if (dateTo.HasValue)
				query = query.Where(o => o.OrderAt < dateTo.Value.AddDays(1));

			return await query
				.OrderByDescending(o => o.OrderAt)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();
		}

		public async Task<int> GetTotalCountByMemberIdAsync(int memberId, DateTime? dateFrom, DateTime? dateTo)
		{
			var query = _context.Orders
				.Where(o => o.MemberId == memberId);

			if (dateFrom.HasValue)
				query = query.Where(o => o.OrderAt >= dateFrom.Value);

			if (dateTo.HasValue)
				query = query.Where(o => o.OrderAt < dateTo.Value.AddDays(1));

			return await query.CountAsync();
		}
	}
}
