using EatTogether.API.Models.DTOs;
using EatTogether.API.Models.Services;
using EatTogether.API.Models.ViewModels.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EatTogether.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;

		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		// POST /api/auth/login
		[HttpPost("login")]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> Login([FromBody] LoginDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await _authService.LoginAsync(dto);

			if (!result.IsSuccess)
			{
				return result.ErrorMessage switch
				{
					"account_or_password_error" => Unauthorized(new ErrorViewModel
					{
						Message = "帳號或密碼錯誤",
						ErrorCode = "account_or_password_error"
					}),
					"email_not_verified" => Unauthorized(new ErrorViewModel
					{
						Message = "請先驗證 Email",
						ErrorCode = "email_not_verified"
					}),
					"account_blacklisted" => StatusCode(403, new ErrorViewModel
					{
						Message = "帳號已停權，請聯繫客服",
						ErrorCode = "account_blacklisted"
					}),
					"account_deleted" => Unauthorized(new ErrorViewModel
					{
						Message = "此帳號已停用",
						ErrorCode = "account_deleted"
					}),
					_ => BadRequest(new ErrorViewModel { Message = "發生錯誤，請稍後再試" })
				};
			}

			return Ok(result.Value);
		}

		// POST /api/auth/register
		[HttpPost("register")]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> Register([FromBody] RegisterDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			// todo Turnstile 驗證
			// var isCaptchaValid = await _captchaService.VerifyAsync(dto.CaptchaToken);
			// if (!isCaptchaValid) return BadRequest(...);

			var result = await _authService.RegisterAsync(dto);

			if (!result.IsSuccess)
			{
				if (result.ErrorMessage == "account_taken")
					return Conflict(new ErrorViewModel
					{
						Message = "帳號已被使用，請換一個",
						ErrorCode = "account_taken"
					});

				if (result.ErrorMessage == "email_taken")
					return Conflict(new ErrorViewModel
					{
						Message = "此 Email 已有帳號，請直接登入",
						ErrorCode = "email_taken"
					});

				return BadRequest(new ErrorViewModel
				{
					Message = result.ErrorMessage ?? "發生錯誤，請稍後再試"
				});
			}

			return Ok(new SuccessViewModel
			{
				Message = "驗證信已寄出，請至信箱點擊驗證連結"
			});
		}

		// POST /api/auth/logout
		[HttpPost("logout")]
		[Authorize]
		[EnableRateLimiting("GeneralPolicy")]
		public async Task<IActionResult> Logout()
		{
			await _authService.LogoutAsync(Request);
			return Ok(new SuccessViewModel { Message = "已成功登出" });
		}

		// POST /api/auth/restore-account
		[HttpPost("restore-account")]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> RestoreAccount([FromBody] LoginDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await _authService.RestoreAccountAsync(dto);

			if (!result.IsSuccess)
			{
				return result.ErrorMessage switch
				{
					"account_or_password_error" => Unauthorized(new ErrorViewModel
					{
						Message = "帳號或密碼錯誤",
						ErrorCode = "account_or_password_error"
					}),
					"account_not_deleted" => BadRequest(new ErrorViewModel
					{
						Message = "帳號狀態正常，請直接登入",
						ErrorCode = "account_not_deleted"
					}),
					"account_blacklisted" => StatusCode(403, new ErrorViewModel
					{
						Message = "帳號已停權，請聯繫客服",
						ErrorCode = "account_blacklisted"
					}),
					_ => BadRequest(new ErrorViewModel { Message = "發生錯誤，請稍後再試" })
				};
			}

			return Ok(result.Value);
		}

		// POST /api/auth/refresh
		[HttpPost("refresh")]
		public async Task<IActionResult> Refresh()
		{
			var refreshToken = Request.Cookies["refresh_token"];
			if (string.IsNullOrEmpty(refreshToken))
				return Unauthorized();

			var result = await _authService.RefreshTokenAsync(refreshToken);
			if (!result.IsSuccess)
				return Unauthorized();

			return Ok(result.Value);
		}

		// POST /api/auth/forgot-password
		[HttpPost("forgot-password")]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			// 無論結果如何一律回傳 200（防枚舉）
			await _authService.ForgotPasswordAsync(dto.Email);
			return Ok(new SuccessViewModel { Message = "若此 Email 已註冊，重設密碼信件將寄出" });
		}

		// GET /api/auth/validate-reset-token?token=xxx
		[HttpGet("validate-reset-token")]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> ValidateResetToken([FromQuery] string token)
		{
			if (string.IsNullOrWhiteSpace(token))
				return BadRequest(new ErrorViewModel
				{
					Message = "重設連結無效或已過期",
					ErrorCode = "token_invalid"
				});

			var result = await _authService.ValidateResetTokenAsync(token);
			if (!result.IsSuccess)
				return BadRequest(new ErrorViewModel
				{
					Message = "重設連結無效或已過期",
					ErrorCode = "token_invalid"
				});

			return Ok(new SuccessViewModel { Message = "token 有效" });
		}

		// POST /api/auth/reset-password
		[HttpPost("reset-password")]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await _authService.ResetPasswordAsync(dto);
			if (!result.IsSuccess)
				return BadRequest(new ErrorViewModel
				{
					Message = "重設連結無效或已過期",
					ErrorCode = "token_invalid"
				});

			return Ok(new SuccessViewModel { Message = "密碼已成功重設，請重新登入" });
		}

		// GET /api/auth/verify-email?token=xxx
		[HttpGet("verify-email")]
		public async Task<IActionResult> VerifyEmail([FromQuery] string token)
		{
			if (string.IsNullOrWhiteSpace(token))
				return BadRequest(new ErrorViewModel
				{
					Message = "驗證連結無效或已過期",
					ErrorCode = "token_invalid"
				});

			var result = await _authService.VerifyEmailAsync(token);
			if (!result.IsSuccess)
				return BadRequest(new ErrorViewModel
				{
					Message = "驗證連結無效或已過期",
					ErrorCode = "token_invalid"
				});

			return Ok(new SuccessViewModel { Message = "Email 驗證成功" });
		}

		// POST /api/auth/resend-verify-email
		[HttpPost("resend-verify-email")]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> ResendVerifyEmail([FromBody] ResendVerifyEmailDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await _authService.ResendVerifyEmailAsync(dto.Email);

			if (!result.IsSuccess && result.ErrorMessage == "already_confirmed")
				return Ok(new ErrorViewModel
				{
					Message = "帳號已完成驗證，請直接登入",
					ErrorCode = "already_confirmed"
				});

			return Ok(new SuccessViewModel { Message = "驗證信已寄出，請至信箱點擊驗證連結" });
		}

		// POST /api/auth/google/callback
		[HttpPost("google/callback")]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> GoogleCallback([FromBody] GoogleCallbackDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await _authService.GoogleCallbackAsync(dto.Code);

			if (!result.IsSuccess)
			{
				return result.ErrorMessage switch
				{
					"account_blacklisted" => StatusCode(403, new ErrorViewModel
					{
						Message = "帳號已停權，請聯繫客服",
						ErrorCode = "account_blacklisted"
					}),
					"login_failed" => BadRequest(new ErrorViewModel
					{
						Message = "登入失敗，請聯絡客服",
						ErrorCode = "login_failed"
					}),
					"account_exists_use_password" => Ok(new
					{
						status = "account_exists_use_password",
						message = "此 Email 已有帳號，請使用帳號密碼登入，登入後可至會員中心連結 Google 帳號"
					}),
					_ => BadRequest(new ErrorViewModel
					{
						Message = "Google 登入失敗，請重試",
						ErrorCode = "google_auth_failed"
					})
				};
			}

			return Ok(result.Value);
		}
	}
}