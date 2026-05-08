using EatTogether.Models.DTOs;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

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
                // JSON 解析失敗（例如 CancelEntireOrderAsync 在 JSON 後附加了「／取消」）
                // 嘗試以 regex 從原始字串逐欄位提取，避免整串被當作 Order 純文字
                var dto2 = new OrderNoteDto();

                var orderM = Regex.Match(trimmed, @"""order""\s*:\s*""((?:[^""\\]|\\.)*)""",
                                         RegexOptions.IgnoreCase);
                if (orderM.Success)
                    dto2.Order = Regex.Unescape(orderM.Groups[1].Value);

                var nameM = Regex.Match(trimmed, @"""customerName""\s*:\s*""([^""]*)""",
                                         RegexOptions.IgnoreCase);
                if (nameM.Success) dto2.CustomerName = nameM.Groups[1].Value;

                var phoneM = Regex.Match(trimmed, @"""customerPhone""\s*:\s*""([^""]*)""",
                                          RegexOptions.IgnoreCase);
                if (phoneM.Success) dto2.CustomerPhone = phoneM.Groups[1].Value;

                var timeM = Regex.Match(trimmed, @"""pickupTime""\s*:\s*""([^""]*)""",
                                         RegexOptions.IgnoreCase);
                if (timeM.Success) dto2.PickupTime = timeM.Groups[1].Value;

                // regex 完全沒命中（真的不是 JSON），才退回舊行為
                if (dto2.Order == null && dto2.CustomerName == null &&
                    dto2.CustomerPhone == null && dto2.PickupTime == null)
                    dto2.Order = note;

                return dto2;
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
