using System.Collections.ObjectModel;
using System.IO;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.WPF.Ui;

namespace WF.MES.WPF.Modules.Barcode.ViewModels;

/// <summary>生成结果弹窗：选打印机、首次打印、更新打印状态。</summary>
public class BarcodeGenerateResultViewModel : LocalizedViewModelBase
{
    private readonly ILabelPrintService _printService;
    private readonly IBarcodeGenerateRecordService _recordService;
    private readonly BarcodeGeneratePrintDialogModel _model;

    private string _selectedPrinter = string.Empty;
    private string _printStatus = string.Empty;
    private string? _printStatusKey;
    private object[] _printStatusArgs = [];
    private bool _isPrinting;
    private bool _hasPrinted;

    public BarcodeGenerateResultViewModel(
        ILabelPrintService printService,
        IBarcodeGenerateRecordService recordService,
        BarcodeGeneratePrintDialogModel model,
        ILocalizationService localization)
        : base(localization)
    {
        _printService = printService;
        _recordService = recordService;
        _model = model;

        foreach (var printer in _printService.GetInstalledPrinters())
        {
            Printers.Add(printer);
        }

        PrintCommand = new DelegateCommand(async () => await PrintAsync(), CanPrint)
            .ObservesProperty(() => SelectedPrinter)
            .ObservesProperty(() => HasPrinted);
        OpenTemplateFolderCommand = new DelegateCommand(OpenTemplateFolder);
        CloseCommand = new DelegateCommand(() => RequestClose?.Invoke());
    }

    public string PageTitle => L("ui.barcode.generateResultTitle");

    public string WindowTitle => $"{PageTitle} - {GenerateNo}";

    public event Action? RequestClose;

    public ObservableCollection<string> Printers { get; } = [];
    public ObservableCollection<BarcodeRecordDto> Records { get; } = [];

    public string GenerateNo => _model.GenerateNo;

    public string CustomerName => _model.CustomerName;

    public string MaterialNo => _model.MaterialNo;

    public DateTime PrintDate => _model.PrintDate;

    public string ResetKey => _model.ResetKey;

    public string SerialRangeText => _model.SerialRangeText;

    public int Quantity => _model.Records.Count;

    public string SelectedPrinter
    {
        get => _selectedPrinter;
        set => SetProperty(ref _selectedPrinter, value);
    }

    public string PrintStatus
    {
        get => _printStatus;
        set => SetProperty(ref _printStatus, value);
    }

    public bool IsPrinting
    {
        get => _isPrinting;
        private set
        {
            if (SetProperty(ref _isPrinting, value))
            {
                RaisePropertyChanged(nameof(ShowPrintingOverlay));
                RaisePropertyChanged(nameof(PrintButtonText));
            }
        }
    }

    public bool ShowPrintingOverlay => IsPrinting;

    public string PrintButtonText =>
        IsPrinting ? L("ui.barcode.printing") : HasPrinted ? L("ui.barcode.printed") : L("ui.barcode.print");

    public bool HasPrinted
    {
        get => _hasPrinted;
        private set
        {
            if (SetProperty(ref _hasPrinted, value))
            {
                RaisePropertyChanged(nameof(PrintButtonText));
            }
        }
    }

    public DelegateCommand PrintCommand { get; }
    public DelegateCommand OpenTemplateFolderCommand { get; }
    public DelegateCommand CloseCommand { get; }

    public void Initialize()
    {
        Records.Clear();
        foreach (var record in _model.Records)
        {
            Records.Add(record);
        }
    }

    private bool CanPrint() =>
        !HasPrinted && !string.IsNullOrWhiteSpace(SelectedPrinter) && Records.Count > 0;

    private void OpenTemplateFolder()
    {
        try
        {
            _printService.OpenTemplateFolder(MaterialNo);
        }
        catch (Exception ex)
        {
            HandyControl.Controls.Growl.Warning(Ex(ex));
        }
    }

    private async Task PrintAsync()
    {
        if (IsPrinting || HasPrinted || Records.Count == 0)
        {
            return;
        }

        IsPrinting = true;
        SetPrintStatus("ui.barcode.sendingLabels", Records.Count);
        try
        {
            var progress = new Progress<LabelPrintProgressDto>(item =>
                SetPrintStatus("ui.barcode.printingProgress", item.Current, item.Total));
            var result = await _printService.PrintAsync(
                _printService.CreatePrintRequest(
                    MaterialNo,
                    SelectedPrinter,
                    Records.Select(x => x.Barcode)),
                progress);

            HasPrinted = true;
            ClearPrintStatus();
            PrintCommand.RaiseCanExecuteChanged();

            try
            {
                await _recordService.MarkPrintedAsync(_model.GenerateRecordId);
                HandyControl.Controls.Growl.Success(new HandyControl.Data.GrowlInfo
                {
                    Message = Lf("ui.barcode.printedLabelsSuccess", result.PrintedCount),
                    WaitTime = 3
                });
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Warning(Lf("ui.barcode.printStatusUpdateFailed", Ex(ex)));
            }
        }
        catch (Exception ex)
        {
            ClearPrintStatus();
            HandyControl.Controls.Growl.Error(Ex(ex));
        }
        finally
        {
            IsPrinting = false;
        }
    }

    private void SetPrintStatus(string key, params object[] args)
    {
        _printStatusKey = key;
        _printStatusArgs = args;
        ApplyPrintStatus();
    }

    private void ClearPrintStatus()
    {
        _printStatusKey = null;
        _printStatusArgs = [];
        PrintStatus = string.Empty;
    }

    private void ApplyPrintStatus()
    {
        if (_printStatusKey is null)
        {
            return;
        }

        PrintStatus = _printStatusArgs.Length == 0
            ? L(_printStatusKey)
            : string.Format(L(_printStatusKey), _printStatusArgs);
    }
}
