using EatTogether.API.Models.EfModels;
using EatTogether.API.Models.Infra;
using EatTogether.API.Models.Repositories;
using EatTogether.API.Models.Services;
using EatTogether.Models.DTOs;
using EatTogether.Models.Repositories;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;

namespace EatTogether.Models.Services
{
    public class ReservationService
    {
        private readonly IReservationRepository _reservationRepo;
        private readonly ITableRepository       _tableRepo;
        private readonly IMemberRepository      _memberRepo;
        private readonly IEmailService          _emailService;
        private readonly IConfiguration         _config;
		private readonly INotificationService _notifyService;

		// 營業時段常數
		private static readonly TimeSpan BusinessStart = new(11, 0, 0);
        private static readonly TimeSpan BusinessEnd   = new(20, 0, 0);
        private static readonly int[]    ValidMinutes  = { 0, 15, 30, 45 };
        private const double CapacityUsageLimit = 0.7;
        private const int    WindowMinutes      = 90;
        private const int    NoShowThreshold    = 3;

        public ReservationService(
            IReservationRepository reservationRepo,
            ITableRepository       tableRepo,
            IMemberRepository      memberRepo,
            IEmailService          emailService,
            IConfiguration         config,
			INotificationService notifyService)
        {
            _reservationRepo = reservationRepo;
            _tableRepo       = tableRepo;
            _memberRepo      = memberRepo;
            _emailService    = emailService;
            _config          = config;
			_notifyService = notifyService;
		}

        // ─── 建立訂位（七道防線）──────────────────────────────────────
        public async Task<Result<string>> CreateAsync(ReservationCreateDto dto, int? memberId)
        {
            var now   = DateTime.Now;
            var resDate = dto.ReservationDate;
            var time  = resDate.TimeOfDay;
            var total = dto.AdultsCount + dto.ChildrenCount;

            // 防線① 訂位時間必須在現在 30 分鐘後
            if (resDate < now.AddMinutes(30))
                return Result<string>.Fail("訂位時間必須至少在 30 分鐘後");

            // 防線② 營業時間 11:00~19:45
            if (time < BusinessStart || time > BusinessEnd)
                return Result<string>.Fail("訂位時段須在營業時間內（11:00 ~ 19:45）");

            // 防線③ 分鐘只能選 00/15/30/45
            if (!ValidMinutes.Contains(resDate.Minute))
                return Result<string>.Fail("訂位時間的分鐘數必須為 00、15、30 或 45");

            // 防線④ 人數須在 1~10 之間
            if (total < 1 || total > 10)
                return Result<string>.Fail("訂位人數須在 1 ~ 10 人之間");

            // 取得所有桌位資訊
            var allTables = (await _tableRepo.GetAllAsync()).ToList();
            var requiredSeat = GetRequiredSeatCount(total);
            var tablesOfType = allTables.Where(t => t.SeatCount == requiredSeat).ToList();

            // 防線⑤ 同桌型在時段內組數不得超過桌數
            var windowStart = resDate.AddMinutes(-WindowMinutes);
            var windowEnd   = resDate.AddMinutes(WindowMinutes);
            var sessionReservations = await _reservationRepo.GetSessionReservationsAsync(windowStart, windowEnd);

            var sameTypeCount = sessionReservations
                .Count(r => GetRequiredSeatCount(r.Adults + r.Children) == requiredSeat);

            if (sameTypeCount >= tablesOfType.Count)
                return Result<string>.Fail($"此時段 {requiredSeat} 人座位已無空位，請選擇其他時段或調整人數");

            // 防線⑥ ±90 分鐘內總人數不得超過總容量 70%
            var totalCapacity = allTables.Sum(t => t.SeatCount);
            var bookedPeople  = sessionReservations.Sum(r => r.Adults + r.Children);
            var bookedAfter   = bookedPeople + total;

            if (bookedAfter > totalCapacity * CapacityUsageLimit)
                return Result<string>.Fail("此時段座位容量接近上限，請選擇其他時段");

            // 防線⑦ 同一會員在 ±90 分鐘內不得重複訂位
            if (memberId.HasValue)
            {
                var conflict = await _reservationRepo.IsConflictForMemberAsync(resDate, memberId.Value);
                if (conflict)
                    return Result<string>.Fail("您在此時段附近已有訂位紀錄，請查詢現有訂位");

                // 黑名單檢查
                var member = await _memberRepo.GetByEmailAsync(dto.Email ?? "");
                if (member != null && member.IsBlacklisted)
                    return Result<string>.Fail("您的帳號目前無法進行訂位，請聯絡門市");
            }

            // 產生 BookingNumber：R + yyMMdd + seq(3碼)
            var maxSeq      = await _reservationRepo.GetMaxSeqOfDayAsync(resDate);
            var bookingNumber = $"R{resDate:yyMMdd}{(maxSeq + 1):D3}";

			// 建立訂位 (並取得該筆的訂位Id)
			var reservationId = await _reservationRepo.CreateAsync(dto, bookingNumber, memberId);

            // 寄送確認信（非同步，不阻塞主流程）
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendReservationConfirmAsync(
                            dto.Email!, dto.Name, bookingNumber,
                            resDate, dto.AdultsCount, dto.ChildrenCount);
                    }
                    catch { /* 寄信失敗不影響主流程 */ }
                });
            }

			// 建立會員訂位確認通知（失敗不影響訂位結果）
			if (memberId.HasValue)
            {
				try
				{
					await _notifyService.SendToMemberAsync(
			            memberId: memberId.Value,
			            type: "RESERVATION_CONFIRM",
			            referenceType: "Reservation",
			            referenceId: reservationId,
						title: $"訂位確認｜{resDate:M/d} {resDate:HH:mm} {total} 位，我們已為您保留座位",
		                message: $"訂位單號：{bookingNumber}"
					);
				}
				catch { /* 通知失敗不影響訂位流程 */ }
			}

			return Result<string>.Success(bookingNumber);
        }

        // ─── 取消訂位 ─────────────────────────────────────────────────
        public async Task<Result> CancelByMemberAsync(int reservationId, int memberId)
        {
            var detail = await _reservationRepo.GetByIdAsync(reservationId);
            if (detail == null) return Result.Fail("找不到訂位紀錄");
            if (detail.MemberId != memberId) return Result.Fail("無權限取消此訂位");
            return await DoCancelAsync(detail);
        }

        public async Task<Result> CancelByGuestAsync(string bookingNumber, string email)
        {
            var detail = await _reservationRepo.GetByBookingNumberAndEmailAsync(bookingNumber, email);
            if (detail == null) return Result.Fail("找不到訂位紀錄，請確認訂位單號與 Email");
            return await DoCancelAsync(detail);
        }

        private async Task<Result> DoCancelAsync(ReservationDetailDto detail)
        {
            if (detail.Status != 0)
                return Result.Fail("僅限「訂位中」狀態可取消");

            if (detail.ReservationDate <= DateTime.Now.AddHours(1))
                return Result.Fail("訂位時間前 1 小時內無法自助取消，請來電聯繫門市");

            await _reservationRepo.CancelAsync(detail.Id);

            // 寄送取消確認信
            if (!string.IsNullOrWhiteSpace(detail.Email))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendReservationCancelAsync(
                            detail.Email!, detail.Name,
                            detail.BookingNumber, detail.ReservationDate);
                    }
                    catch { }
                });
            }

			// 建立會員訂位取消通知（失敗不影響取消結果）
			if (detail.MemberId.HasValue)
			{
				try
				{
					await _notifyService.SendToMemberAsync(
						memberId: detail.MemberId.Value,
						type: "RESERVATION_CANCEL",
						referenceType: "Reservation",
						referenceId: detail.Id,
						title: $"訂位已取消｜{detail.ReservationDate:M/d} {detail.ReservationDate:HH:mm} {detail.AdultsCount + detail.ChildrenCount} 位",
						message: $"訂位單號：{detail.BookingNumber}"
					);
				}
				catch { /* 通知失敗不影響取消流程 */ }
			}


			return Result.Success();
        }

        // ─── 即時可用性查詢 ────────────────────────────────────────────
        public async Task<AvailabilityDto> GetAvailabilityAsync(
            DateTime date, int adults, int children)
        {
            var total   = adults + children;
            var allTables = (await _tableRepo.GetAllAsync()).ToList();
            var windowStart = date.AddMinutes(-WindowMinutes);
            var windowEnd   = date.AddMinutes(WindowMinutes);
            var sessionReservations = await _reservationRepo.GetSessionReservationsAsync(windowStart, windowEnd);

            var totalCapacity = allTables.Sum(t => t.SeatCount);
            var bookedPeople  = sessionReservations.Sum(r => r.Adults + r.Children);
            var remainingCapacity = totalCapacity - bookedPeople;

            var requiredSeat  = GetRequiredSeatCount(total);
            var tablesOfType  = allTables.Where(t => t.SeatCount == requiredSeat).ToList();
            var sameTypeCount = sessionReservations
                .Count(r => GetRequiredSeatCount(r.Adults + r.Children) == requiredSeat);

            var isAvailable =
                sameTypeCount < tablesOfType.Count &&
                (bookedPeople + total) <= totalCapacity * CapacityUsageLimit;

            var message = isAvailable
                ? $"此時段有空位，預計可容納約 {remainingCapacity} 人"
                : "此時段座位已滿，請選擇其他時段";

            var tableTypeAvailability = allTables
                .GroupBy(t => t.SeatCount)
                .Select(g =>
                {
                    var seatCount  = g.Key;
                    var typeTotal  = g.Count();
                    var typeBooked = sessionReservations
                        .Count(r => GetRequiredSeatCount(r.Adults + r.Children) == seatCount);
                    return new TableTypeSlotDto
                    {
                        TableType  = GetTableTypeName(seatCount),
                        SeatCount  = seatCount,
                        Total      = typeTotal,
                        Available  = Math.Max(0, typeTotal - typeBooked)
                    };
                })
                .OrderBy(t => t.SeatCount)
                .ToList();

            return new AvailabilityDto
            {
                IsAvailable           = isAvailable,
                RemainingCapacity     = remainingCapacity,
                Message               = message,
                TableTypeAvailability = tableTypeAvailability
            };
        }

        // ─── 輔助方法 ──────────────────────────────────────────────────
        private static int GetRequiredSeatCount(int totalPeople) => totalPeople switch
        {
            <= 2  => 2,
            <= 4  => 4,
            <= 6  => 6,
            _     => 10
        };

        private static string GetTableTypeName(int seatCount) => seatCount switch
        {
            2  => "雙人桌",
            4  => "四人桌",
            6  => "六人桌",
            10 => "大桌",
            _  => $"{seatCount} 人桌"
        };
    }
}
