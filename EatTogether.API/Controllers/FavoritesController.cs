using EatTogether.API.Models.EfModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EatTogether.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly EatTogetherDBContext _db;

        public FavoritesController(EatTogetherDBContext db)
        {
            _db = db;
        }

        private int? GetMemberId()
        {
            var sub = User.FindFirstValue("sub")
                   ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(sub, out var id) ? id : null;
        }

        // GET api/Favorites
        // 回傳目前會員收藏的所有 DishId
        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            var dishIds = await _db.MemberFavorites
                .Where(f => f.MemberId == memberId)
                .Join(_db.Products,
                      f => f.ProductId,
                      p => p.Id,
                      (f, p) => p.DishId)
                .Where(dishId => dishId != null)
                .ToListAsync();

            return Ok(dishIds);
        }

        // POST api/Favorites/{dishId}
        // 加入收藏
        [HttpPost("{dishId:int}")]
        public async Task<IActionResult> AddFavorite(int dishId)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.DishId == dishId);

            if (product == null)
                return NotFound(new { message = "找不到此餐點" });

            var exists = await _db.MemberFavorites
                .AnyAsync(f => f.MemberId == memberId && f.ProductId == product.Id);

            if (exists)
                return Ok(new { message = "已在收藏中" });

            _db.MemberFavorites.Add(new MemberFavorite
            {
                MemberId = memberId.Value,
                ProductId = product.Id,
                CreatedAt = DateTime.Now,
            });

            await _db.SaveChangesAsync();
            return Ok(new { message = "已加入收藏" });
        }

        // DELETE api/Favorites/{dishId}
        // 取消收藏
        [HttpDelete("{dishId:int}")]
        public async Task<IActionResult> RemoveFavorite(int dishId)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.DishId == dishId);

            if (product == null)
                return NotFound(new { message = "找不到此餐點" });

            var favorite = await _db.MemberFavorites
                .FirstOrDefaultAsync(f => f.MemberId == memberId && f.ProductId == product.Id);

            if (favorite == null)
                return Ok(new { message = "本來就不在收藏中" });

            _db.MemberFavorites.Remove(favorite);
            await _db.SaveChangesAsync();
            return Ok(new { message = "已取消收藏" });
        }

        // POST api/Favorites/Sync
        // 登入後把 localStorage 的收藏一次同步到伺服器（merge，不覆蓋）
        [HttpPost("Sync")]
        public async Task<IActionResult> SyncFavorites([FromBody] List<int> dishIds)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            if (dishIds == null || dishIds.Count == 0)
                return Ok(new { message = "無需同步" });

            var products = await _db.Products
                .Where(p => p.DishId != null && dishIds.Contains(p.DishId.Value))
                .ToListAsync();

            var existingProductIds = await _db.MemberFavorites
                .Where(f => f.MemberId == memberId)
                .Select(f => f.ProductId)
                .ToListAsync();

            var toAdd = products
                .Where(p => !existingProductIds.Contains(p.Id))
                .Select(p => new MemberFavorite
                {
                    MemberId = memberId.Value,
                    ProductId = p.Id,
                    CreatedAt = DateTime.Now,
                });

            _db.MemberFavorites.AddRange(toAdd);
            await _db.SaveChangesAsync();

            return Ok(new { message = "同步完成" });
        }
    }
}
