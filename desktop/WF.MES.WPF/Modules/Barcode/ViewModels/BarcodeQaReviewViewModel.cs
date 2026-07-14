using System.Collections.ObjectModel;
using WF.MES.Core.Constants;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.WPF.Ui;
using WF.MES.WPF.Modules.Barcode.Views;

namespace WF.MES.WPF.Modules.Barcode.ViewModels;

/// <summary>条码资料审核列表（文员「上传资料」、QA「审核」均通过弹窗处理）。</summary>
public class BarcodeQaReviewViewModel : LocalizedViewModelBase, INavigationAware
{
    private readonly IBarcodeQaReviewService _qaService;
    private readonly ICustomerService _customerService;
    private readonly IMenuActionAuthorization _auth;
    private BarcodeQaReviewListDto? _selectedItem;
    private CustomerListDto? _selectedCustomer;
    private QaStatusFilterItem? _selectedStatusFilter;
    private string _filterMaterialNo = string.Empty;
    private bool _isBusy;
    private int _loadListVersion;

    public BarcodeQaReviewViewModel(
        IBarcodeQaReviewService qaService,
        ICustomerService customerService,
        IMenuActionAuthorization auth,
        ILocalizationService localization)
        : base(localization)
    {
        _qaService = qaService;
        _customerService = customerService;
        _auth = auth;

        RebuildStatusFilters();

        OpenUploadCommand = new DelegateCommand(async () => await OpenUploadAsync(), () => CanOpenUpload)
            .ObservesProperty(() => IsBusy)
            .ObservesProperty(() => SelectedItem);
        OpenReviewCommand = new DelegateCommand(async () => await OpenReviewAsync(), () => CanOpenReview)
            .ObservesProperty(() => IsBusy)
            .ObservesProperty(() => SelectedItem);
        RefreshCommand = new DelegateCommand(async () => await LoadListAsync(), () => !IsBusy)
            .ObservesProperty(() => IsBusy);

        _selectedStatusFilter = StatusFilters[0];
    }

    public string PageTitle => L("ui.barcode.qaTitle");

    public ObservableCollection<BarcodeQaReviewListDto> Items { get; } = [];
    public ObservableCollection<CustomerListDto> Customers { get; } = [];
    public ObservableCollection<QaStatusFilterItem> StatusFilters { get; } = [];

    public BarcodeQaReviewListDto? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                RaisePropertyChanged(nameof(CanOpenUpload));
                RaisePropertyChanged(nameof(CanOpenReview));
                OpenUploadCommand?.RaiseCanExecuteChanged();
                OpenReviewCommand?.RaiseCanExecuteChanged();
                RefreshCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    public CustomerListDto? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            if (SetProperty(ref _selectedCustomer, value))
            {
                _ = LoadListAsync();
            }
        }
    }

    public QaStatusFilterItem? SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set
        {
            if (SetProperty(ref _selectedStatusFilter, value))
            {
                _ = LoadListAsync();
            }
        }
    }

    public string FilterMaterialNo
    {
        get => _filterMaterialNo;
        set
        {
            if (SetProperty(ref _filterMaterialNo, value))
            {
                _ = LoadListAsync();
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RaisePropertyChanged(nameof(CanOpenUpload));
                RaisePropertyChanged(nameof(CanOpenReview));
                OpenUploadCommand?.RaiseCanExecuteChanged();
                OpenReviewCommand?.RaiseCanExecuteChanged();
                RefreshCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasUploadPermission => _auth.HasAction(MenuActions.BarcodeQaReview.SaveAttachments);

    public bool HasReviewPermission => _auth.HasAction(MenuActions.BarcodeQaReview.Review);

    public bool CanOpenUpload =>
        HasUploadPermission
        && !IsBusy
        && SelectedItem != null
        && BarcodeQaStatus.CanUpload(SelectedItem.QaStatus);

    public bool CanOpenReview =>
        HasReviewPermission
        && !IsBusy
        && SelectedItem != null
        && BarcodeQaStatus.CanReview(SelectedItem.QaStatus);

    public DelegateCommand RefreshCommand { get; }
    public DelegateCommand OpenUploadCommand { get; }
    public DelegateCommand OpenReviewCommand { get; }

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        await LoadCustomersAsync();
        await LoadListAsync();
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    private void RebuildStatusFilters()
    {
        StatusFilters.Clear();
        StatusFilters.Add(new QaStatusFilterItem(null, L("ui.actions.all")));
        StatusFilters.Add(new QaStatusFilterItem(BarcodeQaStatus.PendingUpload, LocalizedOptions.TranslateQaStatus(BarcodeQaStatus.PendingUpload, key => L(key))));
        StatusFilters.Add(new QaStatusFilterItem(BarcodeQaStatus.PendingReview, LocalizedOptions.TranslateQaStatus(BarcodeQaStatus.PendingReview, key => L(key))));
        StatusFilters.Add(new QaStatusFilterItem(BarcodeQaStatus.Approved, LocalizedOptions.TranslateQaStatus(BarcodeQaStatus.Approved, key => L(key))));
        StatusFilters.Add(new QaStatusFilterItem(BarcodeQaStatus.Rejected, LocalizedOptions.TranslateQaStatus(BarcodeQaStatus.Rejected, key => L(key))));
    }

    private async Task LoadCustomersAsync()
    {
        Customers.Clear();
        Customers.Add(new CustomerListDto { CustomerId = 0, CustomerName = L("ui.actions.all") });
        foreach (var customer in await _customerService.GetCustomerSelectionListAsync())
        {
            Customers.Add(customer);
        }

        if (_selectedCustomer == null)
        {
            _selectedCustomer = Customers.FirstOrDefault();
            RaisePropertyChanged(nameof(SelectedCustomer));
        }
    }

    private async Task LoadListAsync()
    {
        var loadVersion = Interlocked.Increment(ref _loadListVersion);
        IsBusy = true;
        try
        {
            var selectedId = SelectedItem?.RuleId;
            var customerId = SelectedCustomer?.CustomerId;
            var items = await _qaService.GetListAsync(
                SelectedStatusFilter?.StatusValue,
                customerId is > 0 ? customerId : null,
                string.IsNullOrWhiteSpace(FilterMaterialNo) ? null : FilterMaterialNo.Trim());

            if (loadVersion != _loadListVersion)
            {
                return;
            }

            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }

            SelectedItem = selectedId is > 0
                ? Items.FirstOrDefault(i => i.RuleId == selectedId)
                : Items.FirstOrDefault();
        }
        catch (Exception ex)
        {
            if (loadVersion == _loadListVersion)
            {
                HandyControl.Controls.Growl.Error(Ex(ex));
            }
        }
        finally
        {
            if (loadVersion == _loadListVersion)
            {
                IsBusy = false;
            }
        }
    }

    private async Task OpenUploadAsync()
    {
        if (SelectedItem == null)
        {
            return;
        }

        var ruleId = SelectedItem.RuleId;
        var changed = BarcodeQaReviewUploadDialogWindow.ShowDialog(_qaService, ruleId);
        if (!changed)
        {
            return;
        }

        await LoadListAsync();
        SelectedItem = Items.FirstOrDefault(i => i.RuleId == ruleId);
    }

    private async Task OpenReviewAsync()
    {
        if (SelectedItem == null)
        {
            return;
        }

        var ruleId = SelectedItem.RuleId;
        var changed = BarcodeQaReviewDialogWindow.ShowDialog(_qaService, ruleId);
        if (!changed)
        {
            return;
        }

        await LoadListAsync();
        SelectedItem = Items.FirstOrDefault(i => i.RuleId == ruleId);
    }

    public sealed record QaStatusFilterItem(int? StatusValue, string DisplayText);
}
