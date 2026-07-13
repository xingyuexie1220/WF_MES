using SqlSugar;
using WF.MES.Domain.Common;

namespace WF.MES.Domain.Entities;

[SugarTable("System_Notice")]
public class SystemNotice : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    /// <summary>1=通知 2=公告</summary>
    public int NoticeType { get; set; } = 1;
    /// <summary>0=草稿 1=已发布</summary>
    public int Status { get; set; }
    public DateTime? PublishTime { get; set; }
}
