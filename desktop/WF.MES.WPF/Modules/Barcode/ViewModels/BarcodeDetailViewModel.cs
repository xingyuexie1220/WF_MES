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
        ILocalizationService localization)
        : base(localization)
    {
        _generateRecordService = generateRecordService;
        _customerService = customerService;

        RefreshCommand = new DelegateCommand(async () => await LoadGenerateRecordsAsync(), CanRefresh)
            .ObservesProperty(() => IsLoadingGenerateRecords)
            .ObservesProperty(() => IsExporting);
        ExportCommand = new DelegateCommand(async () => await ExportBarcodesAsync(), CanExportExecute)
            .ObservesProperty(() => SelectedGenerateRecord)
            .ObservesProperty(() => IsLoadingGenerateRecords)
            .ObservesProperty(() => IsExporting);
    }

    public string PageTitle => L("ui.barcode.detailTitle");

    public string DetailSectionTitle => L("ui.barcode.generateDetail");

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
        set => SetProperty(ref _selectedGenerateRecord, value);
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

    public string RetentionHint => TF("ui.barcode.retentionHint", BarcodeRetentionPolicy.RetentionDays);

    public string ExportOnlyHint => L("ui.barcode.detailHint");

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
        RaisePropertyChanged(nameof(DetailSectionTitle));
        RaisePropertyChanged(nameof(RetentionHint));
        RaisePropertyChanged(nameof(ExportOnlyHint));
        var index = Customers.ToList().FindIndex(c => c.CustomerId == 0);
        if (index >= 0)
        {
            var selectedId = SelectedCustomer?.CustomerId;
            Customers[index] = new CustomerListDto { CustomerId = 0, CustomerName = L("ui.actions.all") };
            if (selectedId == 0)
            {
                SelectedCustomer = Customers[index];
            }
        }

    }

    private bool CanRefresh() => !IsLoadingGenerateRecords && !IsExporting;

    private bool CanExportExecute() =>
        !IsLoadingGenerateRecords && !IsExporting && SelectedGenerateRecord != null;

    private async Task LoadCustomersAsync()
    {
        Customers.Clear();
        Customers.Add(new CustomerListDto { CustomerId = 0, CustomerName = L("ui.actions.all") });
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
            HandyControl.Controls.Growl.Error(EX(ex));
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
            Filter = L("ui.barcode.csvFilter"),
            FileName = $"{SelectedGenerateRecord.GenerateNo}.csv",
            Title = L("ui.barcode.exportTitle")
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
                HandyControl.Controls.Growl.Warning(L("ui.barcode.noBarcodesToExport"));
                return;
            }

            HandyControl.Controls.Growl.Success(TF("ui.barcode.exportedCount", count));
        }
        catch (Exception ex)
        {
            HandyControl.Controls.Growl.Error(EX(ex));
        }
        finally
        {
            IsExporting = false;
        }
    }
}
