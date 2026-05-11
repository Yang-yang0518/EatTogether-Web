using EatTogether.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace EatTogether.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TablesController : ControllerBase
    {
        private readonly ITableRepository _tableRepo;
        private readonly IConfiguration   _config;

        public TablesController(ITableRepository tableRepo, IConfiguration config)
        {
            _tableRepo = tableRepo;
            _config    = config;
        }

        // GET api/Tables
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var tables = await _tableRepo.GetAllAsync();
            return Ok(tables);
        }

        // GET api/Tables/{id}/QRCode — 回傳 QR Code PNG 圖片
        [HttpGet("{id:int}/QRCode")]
        [AllowAnonymous]
        public async Task<IActionResult> GetQRCode(int id)
        {
            var table = await _tableRepo.GetByIdAsync(id);
            if (table == null) return NotFound();

            // QR Code 內容：前台內用點餐頁 URL + table 桌名
            var frontendBase = _config["FrontendBaseUrl"] ?? "http://localhost:5173";
            var qrContent    = $"{frontendBase}/dinein?table={Uri.EscapeDataString(table.TableName)}";

            using var qrGenerator = new QRCodeGenerator();
            using var qrData      = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
            using var qrCode      = new PngByteQRCode(qrData);
            var pngBytes          = qrCode.GetGraphic(10);

            return File(pngBytes, "image/png",
                fileDownloadName: $"table-{table.TableName}-qrcode.png");
        }
    }
}
