using EatTogether.API.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EatTogether.API.Controllers
{
	[AllowAnonymous]
    [Route("api/Dishes")]
    [ApiController]
    public class DishRatingsController : ControllerBase
    {
        private readonly DishService _dishService;

        public DishRatingsController(DishService dishService)
        {
            _dishService = dishService;
        }

        [HttpPost("{id}/Rate")]
        public async Task<IActionResult> Rate(int id, [FromBody] RateRequest request)
        {
            if (request.Score < 1 || request.Score > 5)
                return BadRequest("score 必須介於 1 到 5 之間");

            var result = await _dishService.RateAsync(id, request.Score);
            if (result == null)
                return NotFound();

            return Ok(new
            {
                averageScore = result.Value.averageScore,
                ratingCount = result.Value.ratingCount
            });
        }

        [HttpGet("{id}/Rating")] // 加入Get端點
        public async Task<IActionResult> GetRating(int id)
        {
            var dish = await _dishService.GetByIdAsync(id);
            if (dish == null)
                return Ok(new { averageScore = 0, ratingCount = 0 });

            return Ok(new
            {
                averageScore = dish.AverageScore,
                ratingCount = dish.RatingCount
            });
        }
    }


    public class RateRequest
    {
        public int Score { get; set; }
    }
}
