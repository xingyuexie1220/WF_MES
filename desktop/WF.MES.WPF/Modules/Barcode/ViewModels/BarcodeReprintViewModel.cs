using System.Collections.ObjectModel;
using WF.MES.Core.Constants;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.WPF.Infrastructure;

namespace WF.MES.WPF.Modules.Barcode.ViewModels;

/// <summary>已打印生成单的补打（每单仅允许补打一次）。</summary>
public class BarcodeReprintViewModel : LocalizedViewModelBase, INavigationAware
{
    private readonly IBarcodeGenerateRecordService _generateRecordService;
    private readonly ICustomerService _customerService;
    private readonly IMaterialBarcodeRuleService _ruleService;
    private readonly ILabelPrintService _printService;
    private readonly ISessionService _sessionService;

    private CustomerListDto? _selectedCustomer;
    private BarcodeGenerateRecordListDto? _selectedGenerateRecord;
    private string _filterMaterialNo = string.Empty;
    private string _filterGenerateNo = string.Empty;
    private DateTime _filterCreatedFrom = DateTime.Today.AddDays(-BarcodeRetentionPolicy.RetentionDays);
    private string _selectedPrinter = string.Empty;
    private string _printStatus = string.Empty;
    private bool _isLoading;
    private bool _isPrinting;

    public BarcodeReprintViewModel(
        IBarcodeGenerateRecordService generateRecordService,
        ICustomerService customerService,
        IMaterialBarcodeRuleService ruleService,
        ILabelPrintService printService,
        ISessionService sessionService,
        ILocalizationService localization,
        IDesktopUiText ui)
        : base(localization)
    {
        _generateRecordService = generateRecordService;
        _customerService = customerService;
        _ruleService = ruleService;
        _printService = printService;
        _sessionService = sessionService;
        Ui = ui;

        foreach (var printer in _printService.GetInstalledPrinters())
        {
            Printers.Add(printer);
        }

        RefreshCommand = new DelegateCommand(async () => await LoadGenerateRecordsAsync(), () => !IsLoading && !IsPrinting)
            .ObservesProperty(() => IsLoading)
            .ObservesProperty(() => IsPrinting);
        ReprintCommand = new DelegateCommand(async () => await ReprintAsync(), CanReprint)
            .ObservesProperty(() => IsLoading)
            .ObservesProperty(() => IsPrinting)
            .ObservesProperty(() => SelectedGenerateRecord)
            .ObservesProperty(() => SelectedPrinter);
    }

    public IDesktopUiText Ui { get; }

    public string PageTitle => L("desktop.barcode.reprintTitle");

    public string PageHint => L("desktop.barcode.reprintHint");

    public string ReprintInfoTitle => L("desktop.barcode.reprintInfo");

    public string CustomerLabel => Ui.Customer + "：";

    public string MaterialNoLabel => Ui.MaterialNo + "：";

    public string GenerateNoLabel => Ui.GenerateNo + "：";

    public string GenerateTimeFromLabel => Ui.GenerateTimeFrom + "：";

    public string PrinterLabel => Ui.Printer + "：";

    public string ExecuteReprintText => L("desktop.actions.executeReprint");

    public ObservableCollection<CustomerListDto> Customers { get; } = [];
    public ObservableCollection<BarcodeGenerateRecordListDto> GenerateRecords { get; } = [];
    public ObservableCollection<string> Printers { get; } = [];

    public CustomerListDto? SelectedCustomer
    {
        get => _selectedCustomer;
        set => SetProperty(ref _selectedCustomer, value);
    }

    public BarcodeGenerateRecordListDto? SelectedGenerateRecord
    {
        get => _selectedGenerateRecord;
        set
        {
            if (SetProperty(ref _selectedGenerateRecord, value))
            {
                ReprintCommand.RaiseCanExecuteChanged();
                NotifyReprintDetailLines();
            }
        }
    }

    public string FilterMaterialNo
    {
        get => _filterMaterialNo;
        set => SetProperty(ref _filterMaterialNo, value);
    }

    public string FilterGenerateNo
    {
        get => _filterGenerateNo;
        set => SetProperty(ref _filterGenerateNo, value);
    }

    public DateTime FilterCreatedFrom
    {
        get => _filterCreatedFrom;
        set => SetProperty(ref _filterCreatedFrom, value.Date);
    }

    public string SelectedPrinter
    {
        get => _selectedPrinter;
        set => SetProperty(ref _selectedPrinter, value);
    }

    public string PrintStatus
    {
        get => _printStatus;
        private set => SetProperty(ref _printStatus, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public bool IsPrinting
    {
        get => _isPrinting;
        private set
        {
            if (SetProperty(ref _isPrinting, value))
            {
                RaisePropertyChanged(nameof(ShowPrintingOverlay));
            }
        }
    }

    public bool ShowPrintingOverlay => IsPrinting;

    public string? ReprintDetailGenerateNoLine => FormatLine(Ui.GenerateNo, SelectedGenerateRecord?.GenerateNo);

    public string? ReprintDetailCustomerLine => FormatLine(Ui.Customer, SelectedGenerateRecord?.CustomerName);

    public string? ReprintDetailMaterialNoLine => FormatLine(Ui.MaterialNo, SelectedGenerateRecord?.MaterialNo);

    public string? ReprintDetailQuantityLine =>
        SelectedGenerateRecord == null
            ? null
            : string.Format(L("desktop.barcode.reprintWholeOrder"), SelectedGenerateRecord.Quantity);

    public string? ReprintDetailSerialRangeLine => FormatLine(Ui.SerialRange, SelectedGenerateRecord?.SerialRangeText);

    public string? ReprintDetailPrintStatusLine => FormatLine(Ui.PrintStatus, SelectedGenerateRecord?.PrintStatusText);

    public string? ReprintDetailReprintAtLine =>
        SelectedGenerateRecord?.IsReprinted != true
            ? null
            : $"{Ui.ReprintAt}：{SelectedGenerateRecord.LastReprintedAt:yyyy-MM-dd HH:mm}";

    public string? ReprintDetailReprintByLine =>
        SelectedGenerateRecord?.IsReprinted != true
            ? null
            : FormatLine(Ui.ReprintBy, SelectedGenerateRecord.LastReprintedBy);

    public DelegateCommand RefreshCommand { get; }
    public DelegateCommand ReprintCommand { get; }

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        await LoadCustomersAsync();
        await LoadGenerateRecordsAsync();
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    protected override void RefreshLocalizedProperties()
    {
        RaisePropertyChanged(nameof(PageTitle));
        RaisePropertyChanged(nameof(PageHint));
        RaisePropertyChanged(nameof(ReprintInfoTitle));
        RaisePropertyChanged(nameof(CustomerLabel));
        RaisePropertyChanged(nameof(MaterialNoLabel));
        RaisePropertyChanged(nameof(GenerateNoLabel));
        RaisePropertyChanged(nameof(GenerateTimeFromLabel));
        RaisePropertyChanged(nameof(PrinterLabel));
        RaisePropertyChanged(nameof(ExecuteReprintText));
        RefreshAllCustomerOption();
        NotifyReprintDetailLines();
    }

    private void RefreshAllCustomerOption()
    {
        var index = Customers.ToList().FindIndex(c => c.CustomerId == 0);
        if (index >= 0)
        {
            var selectedId = SelectedCustomer?.CustomerId;
            Customers[index] = new CustomerListDto { CustomerId = 0, CustomerName = Ui.All };
            if (selectedId == 0)
            {
                SelectedCustomer = Customers[index];
            }
        }
    }

    private static string? FormatLine(string label, string? value) =>
        string.IsNullOrEmpty(value) ? null : $"{label}：{value}";

    private void NotifyReprintDetailLines()
    {
        RaisePropertyChanged(nameof(ReprintDetailGenerateNoLine));
        RaisePropertyChanged(nameof(ReprintDetailCustomerLine));
        RaisePropertyChanged(nameof(ReprintDetailMaterialNoLine));
        RaisePropertyChanged(nameof(ReprintDetailQuantityLine));
        RaisePropertyChanged(nameof(ReprintDetailSerialRangeLine));
        RaisePropertyChanged(nameof(ReprintDetailPrintStatusLine));
        RaisePropertyChanged(nameof(ReprintDetailReprintAtLine));
        RaisePropertyChanged(nameof(ReprintDetailReprintByLine));
    }

    private bool CanReprint() =>
        !IsLoading && !IsPrinting
        && SelectedGenerateRecord != null
        && BarcodeOrderPrintStatus.CanReprint(SelectedGenerateRecord.PrintStatus)
        && !string.IsNullOrWhiteSpace(SelectedPrinter);

    private string GetCurrentOperator() =>
        _sessionService.CurrentOperatorName ?? string.Empty;

    private async Task LoadCustomersAsync()
    {
        Customers.Clear();
        Customers.Add(new CustomerListDto { CustomerId = 0, CustomerName = Ui.All });
        foreach (var customer in await _customerService.GetCustomerSelectionListAsync())
        {
            Customers.Add(customer);
        }

        SelectedCustomer ??= Customers.FirstOrDefault();
    }

    private BarcodeGenerateRecordQueryDto BuildQuery()
    {
        var customerId = SelectedCustomer?.CustomerId;
        return new BarcodeGenerateRecordQueryDto
        {
            CustomerId = customerId is > 0 ? customerId : null,
            MaterialNo = string.IsNullOrWhiteSpace(FilterMaterialNo) ? null : FilterMaterialNo,
            GenerateNo = string.IsNullOrWhiteSpace(FilterGenerateNo) ? null : FilterGenerateNo,
            CreatedFrom = FilterCreatedFrom,
            PrintStatuses =
            [
                BarcodeOrderPrintStatus.Printed,
                BarcodeOrderPrintStatus.Reprinted
            ]
        };
    }

    private async Task LoadGenerateRecordsAsync()
    {
        IsLoading = true;
        try
        {
            var selectedId = SelectedGenerateRecord?.GenerateRecordId;
            GenerateRecords.Clear();

            foreach (var item in await _generateRecordService.GetGenerateRecordsAsync(BuildQuery()))
            {
                GenerateRecords.Add(item);
            }

            SelectedGenerateRecord = selectedId is > 0
                ? GenerateRecords.FirstOrDefault(r => r.GenerateRecordId == selectedId)
                : GenerateRecords.FirstOrDefault();
        }
        catch (Exception ex)
        {
            HandyControl.Controls.Growl.Error(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ReprintAsync()
    {
        if (SelectedGenerateRecord == null
            || !BarcodeOrderPrintStatus.CanReprint(SelectedGenerateRecord.PrintStatus))
        {
            return;
        }

        IsPrinting = true;
        PrintStatus = L("desktop.barcode.loadingBarcodes");
        try
        {
            await _ruleService.EnsureRuleApprovedForPrintAsync(SelectedGenerateRecord.RuleId);

            var barcodes = await _generateRecordService.GetGenerateRecordBarcodeValuesAsync(
                SelectedGenerateRecord.GenerateRecordId);

            if (barcodes.Count == 0)
            {
                HandyControl.Controls.Growl.Warning(L("desktop.barcode.noBarcodesToReprint"));
                return;
            }

            PrintStatus = string.Format(L("desktop.barcode.sendingLabels"), barcodes.Count);
            var progress = new Progress<LabelPrintProgressDto>(item => PrintStatus = item.StatusText);
            var result = await _printService.PrintAsync(
                _printService.CreatePrintRequest(
                    SelectedGenerateRecord.MaterialNo,
                    SelectedPrinter,
                    barcodes),
                progress);

            try
            {
                await _generateRecordService.MarkReprintedAsync(
                    SelectedGenerateRecord.GenerateRecordId,
                    GetCurrentOperator());
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Warning(string.Format(L("desktop.barcode.printStatusUpdateFailed"), ex.Message));
                return;
            }

            HandyControl.Controls.Growl.Success(result.Message);
            await LoadGenerateRecordsAsync();
        }
        catch (Exception ex)
        {
            HandyControl.Controls.Growl.Error(ex.Message);
        }
        finally
        {
            PrintStatus = string.Empty;
            IsPrinting = false;
        }
    }
}
