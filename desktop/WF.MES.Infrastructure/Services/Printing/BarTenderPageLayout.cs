namespace WF.MES.Infrastructure.Services.Printing;

/// <summary>模板每页标签行列数，用于计算 PrintOut 份数。</summary>
internal readonly record struct BarTenderPageLayout(int Rows, int Columns)
{
    public int LabelsPerPage => Math.Max(1, Rows) * Math.Max(1, Columns);
}
