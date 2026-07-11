namespace WF.MES.Models.Dtos;

public class BarcodeGeneratePrintDialogModel
{
    public int GenerateRecordId { get; init; }

    public string GenerateNo { get; init; } = string.Empty;

    public string CustomerName { get; init; } = string.Empty;

    public string MaterialNo { get; init; } = string.Empty;

    public DateTime PrintDate { get; init; }

    public string ResetKey { get; init; } = string.Empty;

    public IReadOnlyList<BarcodeRecordDto> Records { get; init; } = [];

    public string SerialRangeText
    {
        get
        {
            if (Records.Count == 0)
            {
                return string.Empty;
            }

            return $"{Records.Min(x => x.SerialValue)} ~ {Records.Max(x => x.SerialValue)}";
        }
    }
}
