using System.Net;
using System.Net.Mail;

namespace EatTogether.API.Models.Infra
{
	public interface IEmailService
	{
		Task SendVerifyEmailAsync(string toEmail, string verifyUrl);
		Task SendEmailChangeVerifyAsync(string toEmail, string verifyUrl);
		Task SendPasswordResetEmailAsync(string toEmail, string resetUrl);
		Task SendSecurityNoticeAsync(string toEmail, string action);

		// 發送刪除帳號確認信
		Task SendDeleteAccountConfirmAsync(string toEmail, string memberName, string confirmUrl);

		// 發送帳號刪除完成通知信
		Task SendAccountDeletedNoticeAsync(string toEmail, string memberName);

		// ── 訂位相關 ──
		Task SendReservationConfirmAsync(string toEmail, string name, string bookingNumber,
			DateTime reservationDate, int adults, int children);
		Task SendReservationCancelAsync(string toEmail, string name,
			string bookingNumber, DateTime reservationDate);
		Task SendReservationReminderAsync(string toEmail, string name,
			string bookingNumber, DateTime reservationDate);

		// ── 優惠券相關 ──
		Task SendCouponExpiryNotifyAsync(string toEmail, string name,
			string couponName, string code, DateTime endDate);

		// ── 限定餐點提醒 ──
		Task SendLimitedReminderAsync(string toEmail, string memberName,
			string dishName, DateOnly endDate,
			string description = null, decimal? price = null);
	}

	public class EmailService : IEmailService
	{
		private readonly string _host;
		private readonly int _port;
		private readonly bool _enableSsl;
		private readonly string _userName;
		private readonly string _password;
		private readonly string _fromName;
		private readonly string _fromAddress;
		private readonly string _frontendBaseUrl;

		public EmailService(IConfiguration configuration)
		{
			_host = configuration["Smtp:Host"]!;
			_port = int.Parse(configuration["Smtp:Port"]!);
			_enableSsl = bool.Parse(configuration["Smtp:EnableSsl"]!);
			_userName = configuration["Smtp:UserName"]!;
			_password = configuration["Smtp:Password"]!;
			_fromName = configuration["Smtp:FromName"]!;
			_fromAddress = configuration["Smtp:FromAddress"]!;
			_frontendBaseUrl = configuration["FrontendBaseUrl"]!;
		}

		// 建立 SMTP 連線物件的私有方法
		private SmtpClient BuildSmtpClient()
		{
			return new SmtpClient(_host, _port)
			{
				EnableSsl = _enableSsl,
				Credentials = new NetworkCredential(_userName, _password)
			};
		}

		// 建立郵件物件的私有方法，設定寄件人、主旨、HTML 內文、收件人
		private MailMessage BuildMailMessage(string toEmail, string subject, string htmlBody)
		{
			var message = new MailMessage
			{
				From = new MailAddress(_fromAddress, _fromName),
				Subject = subject,
				Body = htmlBody,
				IsBodyHtml = true // 設定郵件內容為 HTML 格式
			};
			message.To.Add(toEmail);
			return message;
		}

		// 寄出 Email 驗證信
		public async Task SendVerifyEmailAsync(string toEmail, string verifyUrl)
		{
			const string subject = "義起吃 - 請驗證您的 Email";
			var body = $@"
<div style=""font-family:sans-serif;max-width:600px;margin:auto;"">
    <h2 style=""color:#c0392b;"">義起吃 | 義式料理</h2>
    <p>您好，感謝您註冊義起吃！</p>
    <p>請點擊下方按鈕完成 Email 驗證，連結有效期限為 15 分鐘：</p>
    <a href=""{verifyUrl}""
       style=""display:inline-block;padding:12px 24px;background:#c0392b;color:#fff;
              text-decoration:none;border-radius:4px;margin:16px 0;"">
        驗證我的 Email
    </a>
    <p style=""color:#888;font-size:13px;"">若您未曾註冊，請忽略此封信件。</p>
</div>";

			using var smtp = BuildSmtpClient();
			using var message = BuildMailMessage(toEmail, subject, body);
			await smtp.SendMailAsync(message);
		}

		// 寄出 Email 變更驗證信
		public async Task SendEmailChangeVerifyAsync(string toEmail, string verifyUrl)
		{
			const string subject = "義起吃 - Email 變更驗證";
			var body = $@"
<div style=""font-family:sans-serif;max-width:600px;margin:auto;"">
    <h2 style=""color:#c0392b;"">義起吃 | 義式料理</h2>
    <p>您好，您申請變更 Email，請點擊下方連結完成驗證，連結有效期限為 15 分鐘。</p>
    <a href=""{verifyUrl}""
       style=""display:inline-block;padding:12px 24px;background:#c0392b;color:#fff;
              text-decoration:none;border-radius:4px;margin:16px 0;"">
        驗證新 Email
    </a>
    <p style=""color:#888;font-size:13px;"">若您未申請此操作，請忽略此信件，您的帳號不會有任何變更。</p>
</div>";

			using var smtp = BuildSmtpClient();
			using var message = BuildMailMessage(toEmail, subject, body);
			await smtp.SendMailAsync(message);
		}

		// 寄出重設密碼通知信
		public async Task SendPasswordResetEmailAsync(string toEmail, string resetUrl)
		{
			const string subject = "義起吃 - 重設密碼通知";
			var body = $@"
<div style=""font-family:sans-serif;max-width:600px;margin:auto;"">
    <h2 style=""color:#c0392b;"">義起吃 | 義式料理</h2>
    <p>您好，我們收到了您的密碼重設請求。</p>
    <p>請點擊下方按鈕重設密碼，連結有效期限為 30 分鐘：</p>
    <a href=""{resetUrl}""
       style=""display:inline-block;padding:12px 24px;background:#c0392b;color:#fff;
              text-decoration:none;border-radius:4px;margin:16px 0;"">
        重設我的密碼
    </a>
    <p style=""color:#888;font-size:13px;"">若您並未提出此請求，請忽略此封信件，您的帳號密碼不會有任何變更。</p>
</div>";

			using var smtp = BuildSmtpClient();
			using var message = BuildMailMessage(toEmail, subject, body);
			await smtp.SendMailAsync(message);
		}

		// 寄出帳號安全通知信
		public async Task SendSecurityNoticeAsync(string toEmail, string action)
		{
			const string subject = "義起吃 - 帳號安全通知";

			var frontendBaseUrl = _frontendBaseUrl;

			var body = $@"
<div style=""font-family:sans-serif;max-width:600px;margin:auto;"">
    <h2 style=""color:#c0392b;"">義起吃 | 義式料理</h2>
    <p>您好，您的帳號發生了以下安全事件：</p>
    <p style=""padding:12px;background:#f9f9f9;border-left:4px solid #c0392b;"">
        {action}
    </p>
    <p style=""color:#888;font-size:13px;"">若此操作並非您本人執行，請<a href=""{frontendBaseUrl}"" style=""color:#c0392b;"">登入網站</a>並修改密碼，或聯繫客服。</p>
</div>";

			using var smtp = BuildSmtpClient();
			using var message = BuildMailMessage(toEmail, subject, body);
			await smtp.SendMailAsync(message);
		}


		/// <summary>
		/// 發送刪除帳號確認信
		/// </summary>
		public async Task SendDeleteAccountConfirmAsync(string toEmail, string memberName, string confirmUrl)
		{
			var subject = "確認刪除帳號 - EatTogether";
			var body = $@"
			<!DOCTYPE html>
			<html>
			<head>
				<meta charset=""UTF-8"">
				<style>
					body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
					.container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
					.warning {{ background-color: #fff3cd; border: 1px solid #ffc107; padding: 15px; border-radius: 5px; margin: 20px 0; }}
					.footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }}
				</style>
			</head>
			<body>
				<div class=""container"">
					<h2>確認刪除帳號</h2>
					<p>親愛的 {memberName}：</p>
					<p>您申請刪除 EatTogether 帳號。如果這是您的操作，請點擊以下按鈕確認：</p>
					<a href=""{confirmUrl}"" style=""display:inline-block;padding:12px 24px;background:#c0392b;color:#fff;
              text-decoration:none;border-radius:4px;margin:16px 0;"">確認刪除帳號</a>
					<div class=""warning"">
						<strong>⚠️ 重要警告</strong>
						<p>此連結將在 <strong>15 分鐘</strong>後失效</p>
					</div>
					<p>如果這<strong>不是您的操作</strong>，請：</p>
					<ol>
						<li>忽略此信</li>
						<li>立即更改密碼</li>
						<li>聯繫客服：support@eattogether.com</li>
					</ol>
					<div class=""footer"">
						<p>此為系統自動發送的郵件，請勿直接回覆。</p>
						<p>&copy; 2024 EatTogether. All rights reserved.</p>
					</div>
				</div>
			</body>
			</html>
		";

			using var smtp = BuildSmtpClient();
			using var message = BuildMailMessage(toEmail, subject, body);
			await smtp.SendMailAsync(message);
		}

		/// <summary>
		/// 發送帳號刪除完成通知信
		/// </summary>
		public async Task SendAccountDeletedNoticeAsync(string toEmail, string memberName)
		{
			var subject = "您的帳號已被刪除 - EatTogether";
			var body = $@"
			<!DOCTYPE html>
			<html>
			<head>
				<meta charset=""UTF-8"">
				<style>
					body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
					.container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
					.alert {{ background-color: #f8d7da; border: 1px solid #f5c6cb; padding: 15px; border-radius: 5px; margin: 20px 0; }}
					.footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }}
				</style>
			</head>
			<body>
				<div class=""container"">
					<h2>帳號已刪除</h2>
					<p>親愛的 {memberName}：</p>
					<p>您的 EatTogether 帳號已於 <strong>{DateTime.Now:yyyy-MM-dd HH:mm}</strong> 被刪除。</p>
					<div class=""alert"">
						<strong>🔒 安全提醒</strong>
						<p>如果這<strong>不是您的操作</strong>，表示您的帳號可能已被盜用。</p>
						<p>請立即聯繫客服：<strong>support@eattogether.com</strong></p>
						<p>客服電話：<strong>(02) 1234-5678</strong></p>
					</div>
					<p>感謝您使用 EatTogether 的服務。</p>
					<p>期待未來有機會再次為您服務！</p>
					<div class=""footer"">
						<p>此為系統自動發送的郵件，請勿直接回覆。</p>
						<p>&copy; 2024 EatTogether. All rights reserved.</p>
					</div>
				</div>
			</body>
			</html>
		";

			using var smtp = BuildSmtpClient();
			using var message = BuildMailMessage(toEmail, subject, body);
			await smtp.SendMailAsync(message);
		}

		// 寄出訂位確認信
		public async Task SendReservationConfirmAsync(string toEmail, string name,
			string bookingNumber, DateTime reservationDate, int adults, int children)
		{
			const string subject = "義起吃 - 訂位確認通知";
			var body = $@"
<div style=""font-family:sans-serif;max-width:600px;margin:auto;"">
    <h2 style=""color:#c0392b;"">義起吃 | 義式料理</h2>
    <p>{name} 您好，感謝您的訂位，以下是您的訂位資訊：</p>
    <table style=""width:100%;border-collapse:collapse;margin:16px 0;"">
        <tr style=""background:#f9f9f9;"">
            <td style=""padding:10px;border:1px solid #ddd;font-weight:bold;"">訂位單號</td>
            <td style=""padding:10px;border:1px solid #ddd;"">{bookingNumber}</td>
        </tr>
        <tr>
            <td style=""padding:10px;border:1px solid #ddd;font-weight:bold;"">訂位日期時段</td>
            <td style=""padding:10px;border:1px solid #ddd;"">{reservationDate:yyyy/MM/dd HH:mm}</td>
        </tr>
        <tr style=""background:#f9f9f9;"">
            <td style=""padding:10px;border:1px solid #ddd;font-weight:bold;"">人數</td>
            <td style=""padding:10px;border:1px solid #ddd;"">大人 {adults} 位 / 小孩 {children} 位</td>
        </tr>
    </table>
    <p>如需取消訂位，請於訂位時間前 1 小時至官網自助取消，或來電聯繫門市。</p>
    <p style=""color:#888;font-size:13px;"">義起吃 | 義式料理</p>
</div>";

			using var smtp = BuildSmtpClient();
			using var message = BuildMailMessage(toEmail, subject, body);
			await smtp.SendMailAsync(message);
		}

		// 寄出訂位取消確認信
		public async Task SendReservationCancelAsync(string toEmail, string name,
			string bookingNumber, DateTime reservationDate)
		{
			const string subject = "義起吃 - 訂位取消通知";
			var body = $@"
<div style=""font-family:sans-serif;max-width:600px;margin:auto;"">
    <h2 style=""color:#c0392b;"">義起吃 | 義式料理</h2>
    <p>{name} 您好，您的訂位已成功取消：</p>
    <table style=""width:100%;border-collapse:collapse;margin:16px 0;"">
        <tr style=""background:#f9f9f9;"">
            <td style=""padding:10px;border:1px solid #ddd;font-weight:bold;"">訂位單號</td>
            <td style=""padding:10px;border:1px solid #ddd;"">{bookingNumber}</td>
        </tr>
        <tr>
            <td style=""padding:10px;border:1px solid #ddd;font-weight:bold;"">原訂日期時段</td>
            <td style=""padding:10px;border:1px solid #ddd;"">{reservationDate:yyyy/MM/dd HH:mm}</td>
        </tr>
    </table>
    <p>期待下次再為您服務！</p>
    <p style=""color:#888;font-size:13px;"">義起吃 | 義式料理</p>
</div>";

			using var smtp = BuildSmtpClient();
			using var message = BuildMailMessage(toEmail, subject, body);
			await smtp.SendMailAsync(message);
		}

		// 寄出訂位 24hr 提醒信
		public async Task SendReservationReminderAsync(string toEmail, string name,
			string bookingNumber, DateTime reservationDate)
		{
			const string subject = "義起吃 - 訂位提醒";
			var body = $@"
<div style=""font-family:sans-serif;max-width:600px;margin:auto;"">
    <h2 style=""color:#c0392b;"">義起吃 | 義式料理</h2>
    <p>{name} 您好，提醒您明天有一筆訂位：</p>
    <table style=""width:100%;border-collapse:collapse;margin:16px 0;"">
        <tr style=""background:#f9f9f9;"">
            <td style=""padding:10px;border:1px solid #ddd;font-weight:bold;"">訂位單號</td>
            <td style=""padding:10px;border:1px solid #ddd;"">{bookingNumber}</td>
        </tr>
        <tr>
            <td style=""padding:10px;border:1px solid #ddd;font-weight:bold;"">訂位日期時段</td>
            <td style=""padding:10px;border:1px solid #ddd;"">{reservationDate:yyyy/MM/dd HH:mm}</td>
        </tr>
    </table>
    <p>期待您的光臨，義起吃全體員工敬上。</p>
    <p style=""color:#888;font-size:13px;"">義起吃 | 義式料理</p>
</div>";

			using var smtp = BuildSmtpClient();
			using var message = BuildMailMessage(toEmail, subject, body);
			await smtp.SendMailAsync(message);
		}

		// 寄出限定餐點到期提醒信
		public async Task SendLimitedReminderAsync(string toEmail, string memberName,
			string dishName, DateOnly endDate,
			string description = null, decimal? price = null)
		{
			const string subject = "【義起吃】您關注的限定餐點即將到期";

			var descSection = !string.IsNullOrWhiteSpace(description)
				? $@"<p style=""color:#555;font-size:14px;line-height:1.6;margin:8px 0;"">{description}</p>"
				: string.Empty;

			var priceSection = price.HasValue
				? $@"<p style=""font-size:18px;font-weight:bold;color:#c0392b;margin:8px 0;"">NT$ {price.Value:N0}</p>"
				: string.Empty;

			var body = $@"
<div style=""font-family:sans-serif;max-width:600px;margin:auto;"">
    <h2 style=""color:#c0392b;"">義起吃 | 義式料理</h2>
    <p>親愛的 {memberName}，</p>
    <p>您訂閱提醒的限定餐點「<strong>{dishName}</strong>」<br>
       將於明天（<strong>{endDate:yyyy/MM/dd}</strong>）到期，把握最後機會！</p>
    {descSection}
    {priceSection}
    <p style=""color:#888;font-size:13px;"">義起吃 | 義式料理</p>
</div>";

			using var smtp = BuildSmtpClient();
			using var message = BuildMailMessage(toEmail, subject, body);
			await smtp.SendMailAsync(message);
		}

		// 寄出優惠券到期提醒信
		public async Task SendCouponExpiryNotifyAsync(string toEmail, string name,
			string couponName, string code, DateTime endDate)
		{
			const string subject = "義起吃 - 優惠券即將到期提醒";
			var body = $@"
<div style=""font-family:sans-serif;max-width:600px;margin:auto;"">
    <h2 style=""color:#c0392b;"">義起吃 | 義式料理</h2>
    <p>{name} 您好，您有一張優惠券即將於 <strong>{endDate:yyyy/MM/dd}</strong> 到期，請把握機會使用！</p>
    <table style=""width:100%;border-collapse:collapse;margin:16px 0;"">
        <tr style=""background:#f9f9f9;"">
            <td style=""padding:10px;border:1px solid #ddd;font-weight:bold;"">優惠券名稱</td>
            <td style=""padding:10px;border:1px solid #ddd;"">{couponName}</td>
        </tr>
        <tr>
            <td style=""padding:10px;border:1px solid #ddd;font-weight:bold;"">折扣碼</td>
            <td style=""padding:10px;border:1px solid #ddd;"">
                <strong style=""font-size:18px;letter-spacing:2px;color:#c0392b;"">{code}</strong>
            </td>
        </tr>
        <tr style=""background:#f9f9f9;"">
            <td style=""padding:10px;border:1px solid #ddd;font-weight:bold;"">到期日</td>
            <td style=""padding:10px;border:1px solid #ddd;"">{endDate:yyyy/MM/dd}</td>
        </tr>
    </table>
    <p>立即前往官網點餐，享受您的優惠！</p>
    <p style=""color:#888;font-size:13px;"">義起吃 | 義式料理</p>
</div>";

			using var smtp = BuildSmtpClient();
			using var message = BuildMailMessage(toEmail, subject, body);
			await smtp.SendMailAsync(message);
		}
	}
}
