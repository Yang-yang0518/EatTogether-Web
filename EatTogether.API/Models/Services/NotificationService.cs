using EatTogether.API.Models.DTOs;
using EatTogether.API.Models.EfModels;
using EatTogether.API.Models.Repositories;

namespace EatTogether.API.Models.Services
{
	public interface INotificationService
	{
		Task<IEnumerable<NotificationDto>> GetNotificationsAsync(int memberId);
		Task MarkAsReadAsync(int notificationId, int memberId);
		Task MarkAllAsReadAsync(int memberId);
		Task SendToMemberAsync(int memberId, string type, string? referenceType, int? referenceId, string title, string? message = null);
	}


	/// <summary>
	/// 通知中心服務
	/// 統一管理所有會員通知的建立，各業務模組只需呼叫此 Service，不需自行寫入資料庫。
	///
	/// ══════════════════════════════════════════════════════
	///  使用方式
	/// ══════════════════════════════════════════════════════
	///
	/// 1. 全員通知（文章發布、系統公告等）
	///    呼叫 SendToAllMembersAsync()，系統自動對所有正常會員批次建立通知。
	///
	///    範例：
	///    await _notificationService.SendToAllMembersAsync(
	///        type          : "NEWS",           // 通知類型（見下方類型列表）
	///        referenceType : "Article",        // 關聯資料類型
	///        referenceId   : articleId,        // 關聯資料的 Id
	///        title         : $"親愛的會員，{title}",
	///        scheduledAt   : dto.PublishDate   // 指定顯示時間，null 則為當下
	///    );
	///
	/// ──────────────────────────────────────────────────────
	///
	/// 2. 個人通知（訂位、外帶、優惠券等）
	///    呼叫 SendToMemberAsync()，只建立該會員的通知。
	///
	///    範例：
	///	   await _notifyService.SendToMemberAsync(
	///		  memberId: memberId.Value,				//抓取目前登入會員Id
	///		  type: "RESERVATION_CONFIRM",			//自行定義，以此為相同類型訊息的規則
	///		  referenceType: "Reservation",         //資料表名稱
	///		  referenceId: reservationId,			//相聯Id
	///		  title: $"訂位確認｜{resDate:M/d} {resDate:HH:mm} {total} 位，我們已為您保留座位",  //主要顯示訊息
	///		  message: $"訂位單號：{bookingNumber}"    //可不寫，需補充資訊再寫
	///	   );
	///
	/// ══════════════════════════════════════════════════════
	///  通知類型（Type）一覽，由各模組定義並統一使用，前台依此類型決定跳轉頁面
	/// ══════════════════════════════════════════════════════
	///
	///  類型字串                    說明                  觸發位置
	///  ─────────────────────────────────────────────────────────
	///  NEWS                       文章 / 最新消息        ArticleService
	///  RESERVATION_CONFIRM        訂位確認               ReservationService
	///  RESERVATION_CANCEL         訂位取消               ReservationService
	///  TAKEOUT_CREATED            外帶訂單成立           OrderService
	///  COUPON_RECEIVED            優惠券領取             CouponService
	///  TAKEOUT_READY              外帶備餐完成           OrderService（待接）
	///  COUPON_EXPIRING            優惠券即將到期         排程背景服務（待接）
	///
	/// ══════════════════════════════════════════════════════
	///  前台顯示規則（Vue BellNotification）
	/// ══════════════════════════════════════════════════════
	///
	///  - 只顯示近三個月的通知
	///  - NEWS 類型：文章 PublishDate 未到則不顯示（避免提前曝光）
	///  - 點擊通知後依 Type 跳轉對應會員頁面，未讀自動標為已讀
	///
	/// </summary>

	public class NotificationService : INotificationService
	{
		private readonly INotificationRepository _repo;

		public NotificationService(INotificationRepository repo)
		{
			_repo = repo;
		}

		/// <summary>
		/// 取得通知列表
		/// </summary>
		public async Task<IEnumerable<NotificationDto>> GetNotificationsAsync(int memberId)
		{
			var notifications = await _repo.GetByMemberIdAsync(memberId);

			return notifications.Select(n => new NotificationDto
			{
				Id = n.Id,
				Type = n.Type,
				ReferenceType = n.ReferenceType,
				ReferenceId = n.ReferenceId,
				Title = n.Title,
				Message = n.Message,
				IsRead = n.IsRead,
				CreatedAt = n.CreatedAt
			});
		}
		
		/// <summary>
		/// 單筆已讀
		/// </summary>
		public async Task MarkAsReadAsync(int notificationId, int memberId)
		{
			await _repo.MarkAsReadAsync(notificationId, memberId);
		}
		/// <summary>
		/// 全部已讀
		/// </summary>
		public async Task MarkAllAsReadAsync(int memberId)
		{
			await _repo.MarkAllAsReadAsync(memberId);
		}

		/// <summary>
		/// 建立個別會員通知
		/// </summary>
		public async Task SendToMemberAsync(int memberId, string type, string? referenceType, int? referenceId, string title, string? message = null)
		{
			var notification = new UserNotification
			{
				MemberId = memberId,
				Type = type,
				ReferenceType = referenceType,
				ReferenceId = referenceId,
				Title = title,
				Message = message,
				IsRead = false,
				CreatedAt = DateTime.Now
			};

			await _repo.CreateAsync(notification); 
		}
	}
}