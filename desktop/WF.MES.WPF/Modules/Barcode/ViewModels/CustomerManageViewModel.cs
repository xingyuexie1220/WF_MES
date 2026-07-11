using System.Collections.ObjectModel;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.WPF.Infrastructure;

namespace WF.MES.WPF.Modules.Barcode.ViewModels;

/// <summary>条码客户管理。</summary>
public class CustomerManageViewModel : LocalizedViewModelBase, INavigationAware
{
    private readonly ICustomerService _customerService;

    private CustomerListDto? _selectedCustomer;
    private CustomerEditDto _editModel = new();
    private bool _isBusy;
    private bool _isNew;

    public CustomerManageViewModel(
        ICustomerService customerService,
        ILocalizationService localization,
        IDesktopUiText ui)
        : base(localization)
    {
        _customerService = customerService;
        Ui = ui;

        AddCommand = new DelegateCommand(StartAdd);
        EditCommand = new DelegateCommand(async () => await StartEditAsync(), () => SelectedCustomer != null)
            .ObservesProperty(() => SelectedCustomer);
        SaveCommand = new DelegateCommand(async () => await SaveAsync(), CanSave)
            .ObservesProperty(() => IsBusy)
            .ObservesProperty(() => EditCustomerName);
        RefreshCommand = new DelegateCommand(async () => await LoadCustomersAsync());
    }

    public IDesktopUiText Ui { get; }

    public string PageTitle => L("desktop.barcode.customerTitle");

    public string CustomerNameLabel => Ui.CustomerName;

    public string SaveCustomerText => L("desktop.actions.saveCustomer");

    public ObservableCollection<CustomerListDto> Customers { get; } = [];

    public CustomerListDto? SelectedCustomer
    {
        get => _selectedCustomer;
        set => SetProperty(ref _selectedCustomer, value);
    }

    public CustomerEditDto EditModel
    {
        get => _editModel;
        set
        {
            if (SetProperty(ref _editModel, value))
            {
                RaisePropertyChanged(nameof(EditCustomerName));
                RaisePropertyChanged(nameof(CustomerEnabled));
            }
        }
    }

    public string EditCustomerName
    {
        get => _editModel.CustomerName;
        set
        {
            if (_editModel.CustomerName != value)
            {
                _editModel.CustomerName = value;
                RaisePropertyChanged();
            }
        }
    }

    public bool CustomerEnabled
    {
        get => EditModel.Enable == 1;
        set
        {
            EditModel.Enable = value ? 1 : 0;
            RaisePropertyChanged();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public bool IsNew
    {
        get => _isNew;
        set
        {
            if (SetProperty(ref _isNew, value))
            {
                RaisePropertyChanged(nameof(FormTitle));
            }
        }
    }

    public string FormTitle => IsNew ? L("desktop.barcode.customerFormNew") : L("desktop.barcode.customerFormEdit");

    public DelegateCommand AddCommand { get; }
    public DelegateCommand EditCommand { get; }
    public DelegateCommand SaveCommand { get; }
    public DelegateCommand RefreshCommand { get; }

    public async void OnNavigatedTo(NavigationContext navigationContext) => await LoadCustomersAsync();

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    protected override void RefreshLocalizedProperties()
    {
        RaisePropertyChanged(nameof(PageTitle));
        RaisePropertyChanged(nameof(CustomerNameLabel));
        RaisePropertyChanged(nameof(SaveCustomerText));
        RaisePropertyChanged(nameof(FormTitle));
    }

    private bool CanSave() =>
        !IsBusy && !string.IsNullOrWhiteSpace(EditCustomerName);

    private async Task LoadCustomersAsync()
    {
        IsBusy = true;
        try
        {
            Customers.Clear();
            foreach (var item in await _customerService.GetCustomersAsync())
            {
                Customers.Add(item);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void StartAdd()
    {
        IsNew = true;
        EditModel = new CustomerEditDto { Enable = 1 };
    }

    private async Task StartEditAsync()
    {
        if (SelectedCustomer == null)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var customer = await _customerService.GetCustomerAsync(SelectedCustomer.CustomerId);
            if (customer == null)
            {
                return;
            }

            IsNew = false;
            EditModel = customer;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveAsync()
    {
        IsBusy = true;
        try
        {
            var customerId = await _customerService.SaveCustomerAsync(EditModel);
            var saved = await _customerService.GetCustomerAsync(customerId);
            if (saved != null)
            {
                IsNew = false;
                EditModel = saved;
            }

            HandyControl.Controls.Growl.Success(L("desktop.barcode.customerSaveSuccess"));
            await LoadCustomersAsync();
        }
        catch (Exception ex)
        {
            HandyControl.Controls.Growl.Error(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
