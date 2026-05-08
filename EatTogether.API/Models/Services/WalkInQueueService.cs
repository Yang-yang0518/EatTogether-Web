using EatTogether.API.Models.DTOs;
using EatTogether.API.Models.Infra;
using EatTogether.API.Models.Repositories;

namespace EatTogether.API.Models.Services;

public class WalkInQueueService
{
    private readonly IWalkInQueueRepository _repo;

    public WalkInQueueService(IWalkInQueueRepository repo)
    {
        _repo = repo;
    }

    // ─── 今日整體摘要 ────────────────────────────────────────
    public async Task<WalkInTodayStatusDto> GetTodayStatusAsync()
    {
        var list = await _repo.GetTodayActiveAsync();
        return new WalkInTodayStatusDto
        {
            WaitingCount = list.Count(q => q.Status == 0),
            CalledCount  = list.Count(q => q.Status == 1)
        };
    }

    // 營業時間（候位開放：開店前 30 分鐘 ~ 打烊）
    private static readonly TimeOnly QueueOpenTime  = new(10, 30);
    private static readonly TimeOnly QueueCloseTime = new(21, 0);

    // ─── 登記候位 ────────────────────────────────────────────
    public async Task<Result<WalkInStatusDto>> RegisterAsync(WalkInCreateDto dto, int? memberId)
    {
        // 營業時間判斷
        var now = TimeOnly.FromDateTime(DateTime.Now);
        if (now < QueueOpenTime)
            return Result<WalkInStatusDto>.Fail($"候位開放時間為 {QueueOpenTime:HH:mm}，請稍後再試");
        if (now >= QueueCloseTime)
            return Result<WalkInStatusDto>.Fail("今日候位已結束，歡迎明日再來");

        // 基本驗證
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length < 2 || dto.Name.Length > 50)
            return Result<WalkInStatusDto>.Fail("請輸入正確姓名（2～50 字）");

        if (string.IsNullOrWhiteSpace(dto.Phone) || !System.Text.RegularExpressions.Regex.IsMatch(dto.Phone, @"^09\d{8}$"))
            return Result<WalkInStatusDto>.Fail("請輸入有效的手機號碼（格式：09xxxxxxxx）");

        var total = dto.AdultsCount + dto.ChildrenCount;
        if (total < 1 || total > 10)
            return Result<WalkInStatusDto>.Fail("候位人數須在 1～10 人之間");

        // 同一電話當天只能有一筆等待中/已叫號
        var existing = await _repo.GetTodayByPhoneAsync(dto.Phone);
        if (existing != null && existing.Status <= 1)
            return Result<WalkInStatusDto>.Fail("此電話今日已有候位紀錄，請查詢目前號碼狀態");

        // 產生號碼牌：A001 ~ A999
        var seq = await _repo.GetMaxSeqOfDayAsync() + 1;
        var queueNumber = $"A{seq:D3}";

        var id = await _repo.CreateAsync(dto, queueNumber, memberId);

        // 計算前面幾組
        var activeList = await _repo.GetTodayActiveAsync();
        var groupsAhead = activeList.Count(q => q.Id < id && q.Status == 0);

        return Result<WalkInStatusDto>.Success(new WalkInStatusDto
        {
            Id           = id,
            QueueNumber  = queueNumber,
            Name         = dto.Name,
            AdultsCount  = dto.AdultsCount,
            ChildrenCount = dto.ChildrenCount,
            Status       = 0,
            StatusText   = "等待中",
            RegisteredAt = DateTime.Now,
            GroupsAhead  = groupsAhead
        });
    }

    // ─── 用電話查詢候位狀態 ──────────────────────────────────
    public async Task<Result<WalkInStatusDto>> GetByPhoneAsync(string phone)
    {
        var q = await _repo.GetTodayByPhoneAsync(phone);
        if (q == null)
            return Result<WalkInStatusDto>.Fail("查無今日候位紀錄，請確認電話號碼");

        var groupsAhead = 0;
        if (q.Status == 0)
        {
            var activeList = await _repo.GetTodayActiveAsync();
            groupsAhead = activeList.Count(x => x.Id < q.Id && x.Status == 0);
        }

        return Result<WalkInStatusDto>.Success(ToDto(q, groupsAhead));
    }

    // ─── 放棄候位 ────────────────────────────────────────────
    public async Task<Result> LeaveAsync(int id)
    {
        var q = await _repo.GetByIdAsync(id);
        if (q == null) return Result.Fail("找不到候位紀錄");
        if (q.Status != 0) return Result.Fail("此候位已無法取消");

        await _repo.LeaveAsync(id);
        return Result.Success();
    }

    // ─── 輔助 ────────────────────────────────────────────────
    private static WalkInStatusDto ToDto(EatTogether.API.Models.EfModels.WalkInQueue q, int groupsAhead) => new()
    {
        Id           = q.Id,
        QueueNumber  = q.QueueNumber,
        Name         = q.Name,
        AdultsCount  = q.AdultsCount,
        ChildrenCount = q.ChildrenCount,
        Status       = q.Status,
        StatusText   = q.Status switch
        {
            0 => "等待中",
            1 => "已叫號，請至櫃台",
            2 => "已入座",
            3 => "已放棄",
            4 => "已過號",
            _ => "未知"
        },
        RegisteredAt = q.RegisteredAt,
        GroupsAhead  = groupsAhead
    };
}
