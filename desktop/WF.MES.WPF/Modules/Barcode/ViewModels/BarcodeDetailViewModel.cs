using System.Collections.ObjectModel;
using Microsoft.Win32;
using WF.MES.Core.Constants;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.WPF.Infrastructure;

namespace WF.MES.WPF.Modules.Barcode.ViewModels;

/// <summary>生成单明细查询与 CSV 导出（不加载全量明细到 UI）。</summary>
public class BarcodeDetailViewModel : LocalizedViewModelBase, INavigationAware
{
    private readonly IBarcodeGenerateRecordService _generateRecordService;
    private readonly ICustomerService _customerService;

    private BarcodeGenerateRecordListDto? _selectedGenerateRecord;
    private CustomerListDto? _selectedCustomer;
    private string _filterMaterialNo = string.Empty;
    private string _filterGenerateNo = string.Empty;
    private DateTime _filterCreatedFrom = DateTime.Today.AddDays(-BarcodeRetentionPolicy.RetentionDays);
    private bool _isLoadingGenerateRecords;
    private bool _isExporting;

    public BarcodeDetailViewModel(
        IBarcodeGenerateRecordService generateRecordService,
        ICustomerService customerService,
        ILocalizationService localization,
        IDesktopUiText ui)
        : base(localization)
    {
        _generateRecordService = generateRecordService;
        _customerService = customerService;
        Ui = ui;

        RefreshCommand = new DelegateCommand(async () => await LoadGenerateRecordsAsync(), CanRefresh)
            .ObservesProperty(() => IsLoadingGenerateRecords)
            .ObservesProperty(() => IsExporting);
        ExportCommand = new DelegateCommand(async () => await ExportBarcodesAsync(), CanExportExecute)
            .ObservesProperty(() => SelectedGenerateRecord)
            .ObservesProperty(() => IsLoadingGenerateRecords)
            .ObservesProperty(() => IsExporting);
    }

    public IDesktopUiText Ui { get; }

    public string PageTitle => L("desktop.barcode.detailTitle");

    public string CustomerLabel => Ui.Customer + "：";

    public string MaterialFilterLabel => Ui.MaterialFilter + "：";

    public string GenerateNoLabel => Ui.GenerateNo + "：";

    public string GenerateTimeFromLabel => Ui.GenerateTimeFrom + "：";

    public string DetailSectionTitle => L("desktop.barcode.generateDetail");

    public ObservableCollection<CustomerListDto> Customers { get; } = [];
    public ObservableCollection<BarcodeGenerateRecordListDto> GenerateRecords { get; } = [];

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
                NotifyDetailLines();
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

    public string RetentionHint => BarcodeRetentionPolicy.DetailPageHint;

    public string ExportOnlyHint => L("desktop.barcode.detailHint");

    public bool IsLoadingGenerateRecords
    {
        get => _isLoadingGenerateRecords;
        private set => SetProperty(ref _isLoadingGenerateRecords, value);
    }

    public bool IsExporting
    {
        get => _isExporting;
        private set => SetProperty(ref _isExporting, value);
    }

    public string? DetailGenerateNoLine => FormatLine(Ui.GenerateNo, SelectedGenerateRecord?.GenerateNo);

    public string? DetailCustomerLine => FormatLine(Ui.Customer, SelectedGenerateRecord?.CustomerName);

    public string? DetailMaterialNoLine => FormatLine(Ui.MaterialNo, SelectedGenerateRecord?.MaterialNo);

    public string? DetailPrintDateLine =>
        SelectedGenerateRecord == null ? null : $"{Ui.PrintDate}：{SelectedGenerateRecord.PrintDate:yyyy-MM-dd}";

    public string? DetailQuantityLine =>
        SelectedGenerateRecord == null ? null : $"{Ui.Quantity}：{SelectedGenerateRecord.Quantity:N0}";

    public string? DetailSerialRangeLine => FormatLine(Ui.SerialRange, SelectedGenerateRecord?.SerialRangeText);

    public string? DetailResetKeyLine =>
        SelectedGenerateRecord == null ? null : $"ResetKey：{SelectedGenerateRecord.ResetKey}";

    public string? DetailPrintStatusLine => FormatLine(Ui.PrintStatus, SelectedGenerateRecord?.PrintStatusText);

    public DelegateCommand RefreshCommand { get; }
    public DelegateCommand ExportCommand { get; }

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadCustomersAsync();
        await LoadGenerateRecordsAsync();
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    protected override void RefreshLocalizedProperties()
    {
        RaisePropertyChanged(nameof(PageTitle));
        RaisePropertyChanged(nameof(CustomerLabel));
        RaisePropertyChanged(nameof(MaterialFilterLabel));
        RaisePropertyChanged(nameof(GenerateNoLabel));
        RaisePropertyChanged(nameof(GenerateTimeFromLabel));
        RaisePropertyChanged(nameof(DetailSectionTitle));
        RaisePropertyChanged(nameof(ExportOnlyHint));
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
        NotifyDetailLines();
    }

    private static string? FormatLine(string label, string? value) =>
        string.IsNullOrEmpty(value) ? null : $"{label}：{value}";

    private void NotifyDetailLines()
    {
        RaisePropertyChanged(nameof(DetailGenerateNoLine));
        RaisePropertyChanged(nameof(DetailCustomerLine));
        RaisePropertyChanged(nameof(DetailMaterialNoLine));
        RaisePropertyChanged(nameof(DetailPrintDateLine));
        RaisePropertyChanged(nameof(DetailQuantityLine));
        RaisePropertyChanged(nameof(DetailSerialRangeLine));
        RaisePropertyChanged(nameof(DetailResetKeyLine));
        RaisePropertyChanged(nameof(DetailPrintStatusLine));
    }

    private bool CanRefresh() => !IsLoadingGenerateRecords && !IsExporting;

    private bool CanExportExecute() =>
        !IsLoadingGenerateRecords && !IsExporting && SelectedGenerateRecord != null;

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
        var hasCustomer = customerId is > 0;

        return new BarcodeGenerateRecordQueryDto
        {
            CustomerId = hasCustomer ? customerId : null,
            MaterialNo = string.IsNullOrWhiteSpace(FilterMaterialNo) ? null : FilterMaterialNo,
            GenerateNo = string.IsNullOrWhiteSpace(FilterGenerateNo) ? null : FilterGenerateNo,
            CreatedFrom = FilterCreatedFrom
        };
    }

    private async Task LoadGenerateRecordsAsync()
    {
        IsLoadingGenerateRecords = true;
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
            IsLoadingGenerateRecords = false;
        }
    }

    private async Task ExportBarcodesAsync()
    {
        if (SelectedGenerateRecord == null)
        {
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = L("desktop.barcode.csvFilter"),
            FileName = $"{SelectedGenerateRecord.GenerateNo}.csv",
            Title = L("desktop.barcode.exportTitle")
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        IsExporting = true;
        try
        {
            var count = await _generateRecordService.ExportGenerateRecordBarcodesAsync(
                SelectedGenerateRecord.GenerateRecordId,
                SelectedGenerateRecord,
                dialog.FileName);

            if (count == 0)
            {
                HandyControl.Controls.Growl.Warning(L("desktop.barcode.noBarcodesToExport"));
                return;
            }

            HandyControl.Controls.Growl.Success(string.Format(L("desktop.barcode.exportedCount"), count));
        }
        catch (Exception ex)
        {
            HandyControl.Controls.Growl.Error(ex.Message);
        }
        finally
        {
            IsExporting = false;
        }
    }
}
