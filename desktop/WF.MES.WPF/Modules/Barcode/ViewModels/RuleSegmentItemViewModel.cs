using WF.MES.Core.Constants;

namespace WF.MES.WPF.Modules.Barcode.ViewModels;

/// <summary>规则段编辑行（Literal/Date/Serial 各字段绑定）。</summary>
public class RuleSegmentItemViewModel : BindableBase
{
    private string _segmentType = BarcodeSegmentTypes.Literal;
    private string _literalValue = string.Empty;
    private string _dateFormat = DatePartFormats.Default;
    private int _serialRadix = 10;
    private int _serialDigits = 4;
    private bool _includeInResetKey = true;

    public int SortOrder { get; set; }

    private string _sortOrderLabel = string.Empty;

    public string SortOrderLabel
    {
        get => _sortOrderLabel;
        set => SetProperty(ref _sortOrderLabel, value);
    }

    public string SegmentType
    {
        get => _segmentType;
        set
        {
            if (SetProperty(ref _segmentType, value))
            {
                if (IsDate && !DatePartFormats.IsValid(DateFormat))
                {
                    DateFormat = DatePartFormats.Default;
                }

                RaisePropertyChanged(nameof(IsLiteral));
                RaisePropertyChanged(nameof(IsDate));
                RaisePropertyChanged(nameof(IsSerial));
            }
        }
    }

    public bool IncludeInResetKey
    {
        get => _includeInResetKey;
        set => SetProperty(ref _includeInResetKey, value);
    }

    public string LiteralValue
    {
        get => _literalValue;
        set => SetProperty(ref _literalValue, value);
    }

    public string DateFormat
    {
        get => _dateFormat;
        set => SetProperty(ref _dateFormat, value);
    }

    public int SerialRadix
    {
        get => _serialRadix;
        set
        {
            if (SetProperty(ref _serialRadix, value))
            {
                RefreshSerialRadixDescription();
            }
        }
    }

    public int SerialDigits
    {
        get => _serialDigits;
        set => SetProperty(ref _serialDigits, value);
    }

    public bool IsLiteral => SegmentType == BarcodeSegmentTypes.Literal;
    public bool IsDate => SegmentType == BarcodeSegmentTypes.Date;
    public bool IsSerial => SegmentType == BarcodeSegmentTypes.Serial;

    public string SerialRadixDescription
    {
        get => _serialRadixDescription;
        private set => SetProperty(ref _serialRadixDescription, value);
    }

    public void RefreshLocalizedText(Func<string, string> translate)
    {
        _translate = translate;
        RefreshSerialRadixDescription();
    }

    private void RefreshSerialRadixDescription()
    {
        if (_translate == null)
        {
            return;
        }

        SerialRadixDescription = _translate($"ui.barcode.serialRadixDesc.{SerialRadix}");
    }

    private Func<string, string>? _translate;
    private string _serialRadixDescription = string.Empty;
}

public sealed record SerialRadixOption(int Value, string Display, string Description);
