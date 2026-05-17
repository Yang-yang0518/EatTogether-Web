using EatTogether.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EatTogether.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class SetMealsController : ControllerBase
{
    private readonly ISetMealRepository _setMealRepository;
    private readonly IConfiguration _config;

    public SetMealsController(ISetMealRepository setMealRepository, IConfiguration config)
    {
        _setMealRepository = setMealRepository;
        _config = config;
    }

    private string ResolveImageUrl(string dbImageUrl, string name)
    {
        if (!string.IsNullOrEmpty(dbImageUrl)) return dbImageUrl;

        var staticRoot = _config["StaticFilesRoot"];
        var imagesFolder = !string.IsNullOrEmpty(staticRoot) && Directory.Exists(Path.Combine(staticRoot, "images"))
            ? Path.Combine(staticRoot, "images")
            : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

        var safeName = name ?? "";
        foreach (var c in Path.GetInvalidFileNameChars())
            safeName = safeName.Replace(c, '_');

        if (System.IO.File.Exists(Path.Combine(imagesFolder, safeName + ".jpg")))
            return "/images/" + safeName + ".jpg";
        if (System.IO.File.Exists(Path.Combine(imagesFolder, safeName + ".png")))
            return "/images/" + safeName + ".png";

        return "";
    }

    // GET /api/SetMeals/active
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var meals = await _setMealRepository.GetAllActiveAsync();
        var result = meals.Select(s => new
        {
            id            = s.Id,
            setMealName   = s.SetMealName,
            description   = s.Description,
            setPrice      = s.SetPrice,
            imageUrl      = ResolveImageUrl(s.ImageUrl, s.SetMealName),
            isRecommended = s.IsRecommended,
            isPopular     = s.IsPopular,
            startTime     = s.StartTime,
            endTime       = s.EndTime,
            items = s.Items
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new
                {
                    dishId        = i.DishId,
                    dishName      = i.DishName,
                    dishPrice     = i.DishPrice,
                    quantity      = i.Quantity,
                    isOptional    = i.IsOptional,
                    optionGroupNo = i.OptionGroupNo,
                    pickLimit     = i.PickLimit,
                    categoryName  = i.CategoryName
                })
        }).ToList();
        return Ok(result);
    }

    // GET /api/SetMeals/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var s = await _setMealRepository.GetByIdAsync(id);
        if (s == null) return NotFound();
        return Ok(new
        {
            id            = s.Id,
            setMealName   = s.SetMealName,
            description   = s.Description,
            setPrice      = s.SetPrice,
            imageUrl      = ResolveImageUrl(s.ImageUrl, s.SetMealName),
            isRecommended = s.IsRecommended,
            isPopular     = s.IsPopular,
            startTime     = s.StartTime,
            endTime       = s.EndTime,
            items = s.Items
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new
                {
                    dishId        = i.DishId,
                    dishName      = i.DishName,
                    dishPrice     = i.DishPrice,
                    quantity      = i.Quantity,
                    isOptional    = i.IsOptional,
                    optionGroupNo = i.OptionGroupNo,
                    pickLimit     = i.PickLimit,
                    categoryName  = i.CategoryName
                })
        });
    }
}
