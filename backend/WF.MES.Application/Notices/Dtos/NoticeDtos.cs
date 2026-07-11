namespace WF.MES.Application.Notices.Dtos;

public class NoticeDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int NoticeType { get; set; }
    public int Status { get; set; }
    public DateTime? PublishTime { get; set; }
    public long? CreateBy { get; set; }
    public string? CreateByName { get; set; }
    public DateTime CreateTime { get; set; }
    public long? UpdateBy { get; set; }
    public string? UpdateByName { get; set; }
    public DateTime? UpdateTime { get; set; }
}

public class NoticeQueryRequest
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Title { get; set; }
    public int? NoticeType { get; set; }
    public int? Status { get; set; }
}

public class CreateNoticeRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int NoticeType { get; set; } = 1;
    public int Status { get; set; }
}

public class UpdateNoticeRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int NoticeType { get; set; } = 1;
    public int Status { get; set; }
}

public class NoticePushDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int NoticeType { get; set; }
    public DateTime? PublishTime { get; set; }
}
