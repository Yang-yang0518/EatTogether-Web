using EatTogether.Models.DTOs;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EatTogether.Models.Infra
{
    public static class OrderNoteHelper
    {
        private static readonly JsonSerializerOptions _opts = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        /// <summary>
        /// 安全解析 Note 欄位。
        /// 舊格式純字串 → 當作 Order 備註，Items 為空。
        /// 新格式 JSON → 正常反序列化。
        /// </summary>
        public static OrderNoteDto Parse(string? note)
        {
            if (string.IsNullOrWhiteSpace(note))
                return new OrderNoteDto();

            var trimmed = note.Trim();
            if (!trimmed.StartsWith("{"))
                return new OrderNoteDto { Order = note };   // 舊格式相容

            try
            {
                return JsonSerializer.Deserialize<OrderNoteDto>(trimmed, _opts)
                       ?? new OrderNoteDto();
            }
            catch
            {
                return new OrderNoteDto { Order = note };   // 解析失敗也不爆錯
            }
        }

        /// <summary>
        /// 將整筆備註＋個別餐點備註＋外帶顧客資訊組成 JSON 字串。
        /// </summary>
        public static string Build(
            string?                     orderNote,
            Dictionary<string, string>? itemNotes,
            string?                     customerName  = null,
            string?                     customerPhone = null,
            string?                     pickupTime    = null)
        {
            var dto = new OrderNoteDto
            {
                Order         = string.IsNullOrWhiteSpace(orderNote) ? null : orderNote.Trim(),
                Items         = itemNotes ?? new(),
                CustomerName  = string.IsNullOrWhiteSpace(customerName)  ? null : customerName.Trim(),
                CustomerPhone = string.IsNullOrWhiteSpace(customerPhone) ? null : customerPhone.Trim(),
                PickupTime    = string.IsNullOrWhiteSpace(pickupTime)    ? null : pickupTime.Trim(),
            };

            return JsonSerializer.Serialize(dto, _opts);
        }
    }
}
