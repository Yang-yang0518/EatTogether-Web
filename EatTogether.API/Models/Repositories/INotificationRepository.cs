using EatTogether.API.Models.EfModels;

namespace EatTogether.API.Models.Repositories
{
	public interface INotificationRepository
	{
		Task<IEnumerable<UserNotification>> GetByMemberIdAsync(int memberId);
		Task MarkAsReadAsync(int notificationId, int memberId);
		Task MarkAllAsReadAsync(int memberId);
		Task CreateAsync(UserNotification notification);
	}
}
