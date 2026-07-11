using System.Collections.ObjectModel;
using System.IO;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.WPF.Infrastructure;

namespace WF.MES.WPF.Modules.Barcode.ViewModels;

/// <summary>生成结果弹窗：选打印机、首次打印、更新打印状态。</summary>
public class BarcodeGenerateResultViewModel : LocalizedViewModelBase
{
    private readonly ILabelPrintService _printService;
    private readonly IBarcodeGenerateRecordService _recordService;
    private readonly BarcodeGeneratePrintDialogModel _model;

    private string _selectedPrinter = string.Empty;
    private string _printStatus = string.Empty;
    private bool _isPrinting;
    private bool _hasPrinted;

    public BarcodeGenerateResultViewModel(
        ILabelPrintService printService,
        IBarcodeGenerateRecordService recordService,
        BarcodeGeneratePrintDialogModel model,
        ILocalizationService localization,
        IDesktopUiText ui)
        : base(localization)
    {
        _printService = printService;
        _recordService = recordService;
        _model = model;
        Ui = ui;

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

    public IDesktopUiText Ui { get; }

    public event Action? RequestClose;

    public string PageTitle => L("desktop.barcode.generateResultTitle");

    public string WindowTitle => $"{PageTitle} - {GenerateNo}";

    public string PrinterLabel => Ui.Printer + "：";

    public string LabelFolderText => L("desktop.actions.labelFolder");

    public ObservableCollection<string> Printers { get; } = [];
    public ObservableCollection<BarcodeRecordDto> Records { get; } = [];

    public string GenerateNo => _model.GenerateNo;

    public string CustomerName => _model.CustomerName;

    public string MaterialNo => _model.MaterialNo;

    public DateTime PrintDate => _model.PrintDate;

    public string ResetKey => _model.ResetKey;

    public string SerialRangeText => _model.SerialRangeText;

    public int Quantity => _model.Records.Count;

    public string GenerateNoLine => $"{Ui.GenerateNo}：{GenerateNo}";

    public string CustomerLine => $"{Ui.Customer}：{CustomerName}";

    public string MaterialNoLine => $"{Ui.MaterialNo}：{MaterialNo}";

    public string PrintDateLine => $"{Ui.PrintDate}：{PrintDate:yyyy-MM-dd}";

    public string QuantityLine => $"{Ui.Quantity}：{Quantity}";

    public string SerialRangeLine => $"{Ui.SerialRange}：{SerialRangeText}";

    public string ResetKeyLine => $"ResetKey：{ResetKey}";

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
        IsPrinting ? L("desktop.barcode.printing") : HasPrinted ? L("desktop.barcode.printed") : L("desktop.barcode.print");

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

    protected override void RefreshLocalizedProperties()
    {
        RaisePropertyChanged(nameof(PageTitle));
        RaisePropertyChanged(nameof(WindowTitle));
        RaisePropertyChanged(nameof(PrinterLabel));
        RaisePropertyChanged(nameof(LabelFolderText));
        RaisePropertyChanged(nameof(GenerateNoLine));
        RaisePropertyChanged(nameof(CustomerLine));
        RaisePropertyChanged(nameof(MaterialNoLine));
        RaisePropertyChanged(nameof(PrintDateLine));
        RaisePropertyChanged(nameof(QuantityLine));
        RaisePropertyChanged(nameof(SerialRangeLine));
        RaisePropertyChanged(nameof(PrintButtonText));
    }

    private bool CanPrint() =>
        !HasPrinted && !string.IsNullOrWhiteSpace(SelectedPrinter) && Records.Count > 0;

    private void OpenTemplateFolder()
    {
        try
        {
            _printService.OpenTemplateFolder(MaterialNo);
        }
        catch (FileNotFoundException ex)
        {
            HandyControl.Controls.Growl.Warning(ex.Message);
        }
        catch (Exception ex)
        {
            HandyControl.Controls.Growl.Error(ex.Message);
        }
    }

    private async Task PrintAsync()
    {
        if (IsPrinting || HasPrinted || Records.Count == 0)
        {
            return;
        }

        IsPrinting = true;
        PrintStatus = string.Format(L("desktop.barcode.sendingLabels"), Records.Count);
        try
        {
            var progress = new Progress<LabelPrintProgressDto>(item => PrintStatus = item.StatusText);
            var result = await _printService.PrintAsync(
                _printService.CreatePrintRequest(
                    MaterialNo,
                    SelectedPrinter,
                    Records.Select(x => x.Barcode)),
                progress);

            HasPrinted = true;
            PrintStatus = string.Empty;
            PrintCommand.RaiseCanExecuteChanged();

            try
            {
                await _recordService.MarkPrintedAsync(_model.GenerateRecordId);
                HandyControl.Controls.Growl.Success(new HandyControl.Data.GrowlInfo
                {
                    Message = result.Message,
                    WaitTime = 3
                });
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Warning(string.Format(L("desktop.barcode.printStatusUpdateFailed"), ex.Message));
            }
        }
        catch (Exception ex)
        {
            PrintStatus = string.Empty;
            HandyControl.Controls.Growl.Error(ex.Message);
        }
        finally
        {
            IsPrinting = false;
        }
    }
}
