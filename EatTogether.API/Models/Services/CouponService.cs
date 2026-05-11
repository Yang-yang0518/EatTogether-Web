using EatTogether.API.Models.Infra;
using EatTogether.API.Models.Repositories;
using EatTogether.API.Models.Services;
using EatTogether.Models.DTOs;
using EatTogether.Models.Repositories;

namespace EatTogether.Models.Services
{
    public class CouponService
    {
        private readonly ICouponRepository       _couponRepo;
        private readonly IMemberCouponRepository _memberCouponRepo;
        private readonly IMemberRepository       _memberRepo;
		private readonly INotificationService    _notifyService;
        private readonly IPreOrderRepository     _preOrderRepo;

		public CouponService(
            ICouponRepository       couponRepo,
            IMemberCouponRepository memberCouponRepo,
            IMemberRepository       memberRepo,
            INotificationService    notifyService,
            IPreOrderRepository     preOrderRepo)
        {
            _couponRepo       = couponRepo;
            _memberCouponRepo = memberCouponRepo;
            _memberRepo       = memberRepo;
			_notifyService    = notifyService;
            _preOrderRepo     = preOrderRepo;
		}

        // ─── 取得可領取優惠券列表（公開，含 IsClaimed 標記）─────────────
        public async Task<List<CouponDto>> GetAvailableCouponsAsync(int? memberId)
        {
            var now = DateTime.Now;
            var all = await _couponRepo.GetAllAsync();

            var active = all
                .Where(c => !c.IsDisabled && !c.IsUpcoming && !c.IsExpired && !c.IsLimitHit)
                .ToList();

            if (!memberId.HasValue)
            {
                // 未登入：IsClaimed 一律顯示 false（前端顯示「登入後領取」）
                active.ForEach(c => c.IsClaimed = false);
                return active;
            }

            var memberCoupons = (await _memberCouponRepo.GetByMemberAsync(memberId.Value)).ToList();
            var claimedIds    = memberCoupons.Select(mc => mc.CouponId).ToHashSet();
            var usedIds       = memberCoupons.Where(mc => mc.IsUsed).Select(mc => mc.CouponId).ToHashSet();

            active.ForEach(c =>
            {
                c.IsClaimed      = claimedIds.Contains(c.Id);
                c.IsUsedByMember = usedIds.Contains(c.Id);
            });

            return active;
        }

        // ─── 一鍵領取（四道防線）─────────────────────────────────────
        public async Task<Result> ClaimCouponAsync(int couponId, int memberId)
        {
            var coupon = await _couponRepo.GetByIdAsync(couponId);

            // 防線① 優惠券不存在或已停用
            if (coupon == null || coupon.IsDisabled)
                return Result.Fail("此優惠券不存在或已停用");

            // 防線② 活動尚未開始或已過期
            if (coupon.IsUpcoming) return Result.Fail("此優惠券活動尚未開始");
            if (coupon.IsExpired)  return Result.Fail("此優惠券已過期");

            // 防線③ 已達限量
            if (coupon.IsLimitHit) return Result.Fail("此優惠券已被領完，下次早點來！");

            // 防線④ 已領取過
            var existing = await _memberCouponRepo.GetByMemberAndCouponAsync(memberId, couponId);
            if (existing != null) return Result.Fail("您已領取過此優惠券");

            // 新增領取紀錄 + 遞增 ReceivedCount
            await _memberCouponRepo.AddAsync(memberId, couponId);
            await _couponRepo.IncrementReceivedCountAsync(couponId);

            // 建立領取通知
            try {
				await _notifyService.SendToMemberAsync(
				memberId: memberId,
				type: "COUPON_RECEIVED",
				referenceType: "Coupon",
				referenceId: couponId,
				title: $"優惠券已領取｜{coupon.Name}",
				message: coupon.EndDate.HasValue
								? $"有效期限至 {coupon.EndDate.Value:MM/dd}，結帳時即可使用"
								: "無使用期限，結帳時即可使用"
			    );
            }
            catch { /* 通知失敗不影響流程 */ }
			

			return Result.Success();
        }

        // ─── 以折扣碼領取（會員輸入碼直接兌換）─────────────────────────
        public async Task<Result> ClaimByCodeAsync(string code, int memberId)
        {
            var coupon = await _couponRepo.GetByCodeAsync(code.Trim().ToUpper());
            if (coupon == null) return Result.Fail("折扣碼不存在");
            return await ClaimCouponAsync(coupon.Id, memberId);
        }

        // ─── 即將上線的優惠券（公開）────────────────────────────────
        public async Task<List<CouponDto>> GetUpcomingCouponsAsync()
        {
            var all = await _couponRepo.GetAllAsync();
            return all
                .Where(c => !c.IsDisabled && c.IsUpcoming)
                .OrderBy(c => c.StartDate)
                .ToList();
        }

        // ─── 我的優惠券 ──────────────────────────────────────────────
        public async Task<IEnumerable<MemberCouponDto>> GetMyCouponsAsync(int memberId)
        {
            return await _memberCouponRepo.GetByMemberAsync(memberId);
        }

        // -----前台點餐頁用-----
        // ─── 折扣碼驗證（只驗不核銷）────────────────────────────────
        public async Task<CouponValidateDto> ValidateCouponAsync(
            string code, int? memberId, decimal orderAmount)
        {
            var fail = (string msg) => new CouponValidateDto
            {
                IsValid = false, Message = msg
            };

            var coupon = await _couponRepo.GetByCodeAsync(code.ToUpper());

            if (coupon == null)             return fail("折扣碼不存在");
            if (coupon.IsDisabled)          return fail("此折扣碼已停用");
            if (coupon.IsUpcoming)          return fail("此折扣碼活動尚未開始");
            if (coupon.IsExpired)           return fail("此折扣碼已過期");
            if (coupon.IsLimitHit)          return fail("此折扣碼已達使用上限");
            if (orderAmount < coupon.MinSpend)
                return fail($"消費金額未達最低門檻 NT${coupon.MinSpend}");

            // 必須已登入會員才能使用優惠券
            if (!memberId.HasValue)
                return fail("請先登入會員以使用優惠券");

            var mc = await _memberCouponRepo.GetByMemberAndCouponAsync(memberId.Value, coupon.Id);

            // 必須已領取
            if (mc == null)
                return fail("您尚未領取此優惠券");

            // 必須未使用
            if (mc.IsUsed)
                return fail("此優惠券已使用過");

            // 已套用至當日進行中訂單（含製作中）→ 同樣不能再用
            if (await _preOrderRepo.IsCouponInActiveTodayOrderAsync(coupon.Id, memberId.Value))
                return fail("此優惠券已套用於進行中的訂單，如需重新使用請先取消該筆訂單");

            var discount = coupon.DiscountType == 0
                ? coupon.DiscountValue
                : (int)Math.Floor(orderAmount * coupon.DiscountValue / 100m);

            return new CouponValidateDto
            {
                IsValid    = true,
                CouponId   = coupon.Id,
                CouponName = coupon.Name,
                Discount   = discount,
                Message    = $"折扣碼有效！折抵 NT${discount}"
            };
        }

        // ─── 生日優惠券檢查 ───────────────────────────────────────────
        public async Task<BirthdayCheckDto> GetBirthdayCheckAsync(int memberId)
        {
            var member = await _memberRepo.GetByEmailAsync("");  // placeholder
            // 直接查詢 MemberCoupons 取生日券
            var now = DateTime.Now;
            var bdayPrefix = $"BDAY{now:yy}{now:MM}";

            var coupon = await _couponRepo.GetByCodeAsync(bdayPrefix);
            if (coupon == null) return new BirthdayCheckDto { HasBirthdayCoupon = false };

            var claimed = await _memberCouponRepo.GetByMemberAndCouponAsync(memberId, coupon.Id);
            if (claimed == null) return new BirthdayCheckDto { HasBirthdayCoupon = false };

            var daysLeft = coupon.EndDate.HasValue
                ? (int)(coupon.EndDate.Value - now).TotalDays
                : (int?)null;

            return new BirthdayCheckDto
            {
                HasBirthdayCoupon  = true,
                CouponName         = coupon.Name,
                Code               = coupon.Code,
                DiscountDescription = coupon.DiscountDescription,
                EndDate            = coupon.EndDate,
                DaysUntilExpiry    = daysLeft
            };
        }

        // ─── 優惠券使用明細 ───────────────────────────────────────────
        public async Task<IEnumerable<MemberCouponDto>> GetUsageHistoryAsync(int memberId)
        {
            // 直接從 Orders 抓，不依賴 MemberCoupons.IsUsed
            // 可正確反映所有實際使用過券的訂單（包含 seed data）
            return await _memberCouponRepo.GetUsageHistoryFromOrdersAsync(memberId);
        }
    }
}
