using System.Collections.ObjectModel;
using System.Windows;
using WF.MES.Core.Constants;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.WPF.Infrastructure;
using WF.MES.WPF.Modules.Barcode.Views;

namespace WF.MES.WPF.Modules.Barcode.ViewModels;

/// <summary>条码生成与首次打印。</summary>
public class BarcodePrintViewModel : LocalizedViewModelBase, INavigationAware
{
    private readonly IMaterialBarcodeRuleService _ruleService;
    private readonly ICustomerService _customerService;
    private readonly IBarcodeGenerateService _generateService;
    private readonly ISessionService _sessionService;
    private readonly ILabelPrintService _printService;
    private readonly IBarcodeGenerateRecordService _recordService;

    private List<MaterialRuleListDto> _allRules = [];
    private CustomerListDto? _selectedCustomer;
    private MaterialRuleListDto? _selectedRule;
    private DateTime _printDate = DateTime.Today;
    private int _quantity = 1;
    private bool _isGenerating;
    private bool _isPreviewLoading;
    private string _statusMessage = string.Empty;
    private string? _statusMessageKey;
    private object[] _statusMessageArgs = [];
    private string _serialConfigPreview = string.Empty;
    private string _resetKeyPreview = string.Empty;
    private string _serialRangePreview = string.Empty;
    private string _samplePreview = string.Empty;
    private int _sampleLengthPreview;
    private string _generateNo = string.Empty;

    public BarcodePrintViewModel(
        IMaterialBarcodeRuleService ruleService,
        ICustomerService customerService,
        IBarcodeGenerateService generateService,
        ISessionService sessionService,
        ILabelPrintService printService,
        IBarcodeGenerateRecordService recordService,
        ILocalizationService localization)
        : base(localization)
    {
        _ruleService = ruleService;
        _customerService = customerService;
        _generateService = generateService;
        _sessionService = sessionService;
        _printService = printService;
        _recordService = recordService;

        GenerateCommand = new DelegateCommand(async () => await GenerateAsync(), CanGenerate)
            .ObservesProperty(() => SelectedCustomer)
            .ObservesProperty(() => SelectedRule)
            .ObservesProperty(() => Quantity);
    }

    public string PageTitle => L("ui.barcode.printTitle");

    public string PreviewUpdatingText => L("ui.barcode.previewUpdating");

    public string MaxQuantityHint => TF("ui.barcode.maxQuantityHint", BarcodeGenerateLimits.MaxQuantityPerBatch);

    public ObservableCollection<CustomerListDto> Customers { get; } = [];
    public ObservableCollection<MaterialRuleListDto> Rules { get; } = [];

    public CustomerListDto? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            if (SetProperty(ref _selectedCustomer, value))
            {
                ApplyCustomerRules();
            }
        }
    }

    public MaterialRuleListDto? SelectedRule
    {
        get => _selectedRule;
        set
        {
            if (SetProperty(ref _selectedRule, value))
            {
                _ = RefreshTextPreviewAsync();
            }
        }
    }

    public DateTime PrintDate
    {
        get => _printDate;
        set
        {
            if (SetProperty(ref _printDate, value))
            {
                _ = RefreshTextPreviewAsync();
            }
        }
    }

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (SetProperty(ref _quantity, value))
            {
                _ = RefreshTextPreviewAsync();
            }
        }
    }

    public bool IsGenerating
    {
        get => _isGenerating;
        private set
        {
            if (SetProperty(ref _isGenerating, value))
            {
                RaisePropertyChanged(nameof(ShowGeneratingOverlay));
                RaisePropertyChanged(nameof(GenerateButtonText));
            }
        }
    }

    public bool ShowGeneratingOverlay => IsGenerating;

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string GenerateButtonText => IsGenerating ? L("ui.actions.generating") : L("ui.actions.generateBarcode");

    public bool IsPreviewLoading
    {
        get => _isPreviewLoading;
        private set
        {
            if (SetProperty(ref _isPreviewLoading, value))
            {
                RaisePropertyChanged(nameof(ShowPreviewLoading));
            }
        }
    }

    public bool ShowPreviewLoading => IsPreviewLoading && !IsGenerating;

    public string SerialConfigPreview
    {
        get => _serialConfigPreview;
        set => SetProperty(ref _serialConfigPreview, value);
    }

    public string ResetKeyPreview
    {
        get => _resetKeyPreview;
        set => SetProperty(ref _resetKeyPreview, value);
    }

    public string SerialRangePreview
    {
        get => _serialRangePreview;
        set => SetProperty(ref _serialRangePreview, value);
    }

    public string SamplePreview
    {
        get => _samplePreview;
        set => SetProperty(ref _samplePreview, value);
    }

    public int SampleLengthPreview
    {
        get => _sampleLengthPreview;
        private set
        {
            if (SetProperty(ref _sampleLengthPreview, value))
            {
                RaisePropertyChanged(nameof(ShowSampleLengthPreview));
            }
        }
    }

    public bool ShowSampleLengthPreview => SampleLengthPreview > 0;

    public string GenerateNo
    {
        get => _generateNo;
        set => SetProperty(ref _generateNo, value);
    }

    public DelegateCommand GenerateCommand { get; }

    public async void OnNavigatedTo(NavigationContext navigationContext) => await LoadAsync();

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    protected override void RefreshLocalizedProperties()
    {
        RaisePropertyChanged(nameof(PageTitle));
        RaisePropertyChanged(nameof(PreviewUpdatingText));
        RaisePropertyChanged(nameof(MaxQuantityHint));
        RaisePropertyChanged(nameof(GenerateButtonText));
        ApplyStatusMessage();

        if (!IsGenerating && SelectedRule != null && Quantity > 0)
        {
            _ = UpdateTextPreviewCoreAsync();
        }
    }

    private bool CanGenerate() =>
        SelectedCustomer != null && SelectedRule != null && Quantity > 0;

    private async Task LoadAsync()
    {
        try
        {
            var customerId = SelectedCustomer?.CustomerId;
            var ruleId = SelectedRule?.RuleId;

            await LoadCustomersAsync(customerId);
            _allRules = (await _ruleService.GetApprovedRulesAsync()).ToList();

            _selectedCustomer = customerId is > 0 ? Customers.FirstOrDefault(c => c.CustomerId == customerId) : Customers.FirstOrDefault();
            RaisePropertyChanged(nameof(SelectedCustomer));

            if (_selectedCustomer == null)
            {
                Rules.Clear();
                SelectedRule = null;
                ResetPreview();
                return;
            }

            ApplyCustomerRules(ruleId);
        }
        catch (Exception ex)
        {
            HandyControl.Controls.Growl.Error(EX(ex));
        }
    }

    private async Task LoadCustomersAsync(int? ensureCustomerId = null)
    {
        Customers.Clear();
        foreach (var customer in await _customerService.GetCustomerSelectionListAsync(ensureCustomerId))
        {
            Customers.Add(customer);
        }
    }

    private void ApplyCustomerRules(int? preferredRuleId = null)
    {
        var previousRuleId = preferredRuleId ?? SelectedRule?.RuleId;
        Rules.Clear();

        if (SelectedCustomer == null)
        {
            SelectedRule = null;
            ResetPreview();
            return;
        }

        foreach (var rule in _allRules.Where(r => r.CustomerId == SelectedCustomer.CustomerId))
        {
            Rules.Add(rule);
        }

        SelectedRule = previousRuleId is > 0 ? Rules.FirstOrDefault(r => r.RuleId == previousRuleId) : Rules.FirstOrDefault();

        if (SelectedRule == null)
        {
            ResetPreview();
        }
    }

    private async Task RefreshTextPreviewAsync()
    {
        if (IsGenerating)
        {
            return;
        }

        IsPreviewLoading = true;
        try
        {
            await UpdateTextPreviewCoreAsync();
        }
        finally
        {
            IsPreviewLoading = false;
        }
    }

    private async Task UpdateTextPreviewCoreAsync()
    {
        if (SelectedRule == null || Quantity <= 0)
        {
            ResetPreview();
            return;
        }

        try
        {
            var preview = await _generateService.PreviewAsync(new BarcodeGenerateRequestDto
            {
                RuleId = SelectedRule.RuleId,
                PrintDate = PrintDate,
                Quantity = Quantity
            });

            ResetKeyPreview = preview.ResetKey;
            SerialConfigPreview = string.Format(
                L("ui.barcode.serialConfigShort"),
                preview.SerialRadix,
                preview.SerialDigits);
            SerialRangePreview = string.Format(
                L("ui.barcode.serialRangeValue"),
                preview.FirstSerialFormatted,
                preview.LastSerialFormatted,
                preview.NextSerialStart,
                preview.NextSerialEnd);
            SamplePreview = string.Format(L("ui.barcode.firstSample"), preview.FirstBarcodeSample)
                + Environment.NewLine
                + string.Format(L("ui.barcode.lastSample"), preview.LastBarcodeSample);
            SampleLengthPreview = preview.FirstBarcodeSample.Length;
        }
        catch (Exception ex)
        {
            ResetPreview();
            SamplePreview = EX(ex);
            SampleLengthPreview = 0;
        }
    }

    private async Task GenerateAsync()
    {
        if (IsGenerating || SelectedRule == null || SelectedCustomer == null)
        {
            return;
        }

        IsGenerating = true;
        SetStatusMessage("ui.barcode.generatingStatus", Quantity);
        try
        {
            var result = await _generateService.GenerateAsync(new BarcodeGenerateRequestDto
            {
                RuleId = SelectedRule.RuleId,
                PrintDate = PrintDate,
                Quantity = Quantity,
                CreatedBy = _sessionService.CurrentOperatorName ?? _sessionService.CurrentUser?.UserName
            });

            GenerateNo = result.GenerateNo;
            await UpdateTextPreviewCoreAsync();
            ClearStatusMessage();
            HandyControl.Controls.Growl.Success(new HandyControl.Data.GrowlInfo
            {
                Message = TF("ui.barcode.generatedCount", result.Records.Count),
                WaitTime = 3
            });

            var dialogModel = new BarcodeGeneratePrintDialogModel
            {
                GenerateRecordId = result.GenerateRecordId,
                GenerateNo = result.GenerateNo,
                CustomerName = SelectedCustomer.CustomerName,
                MaterialNo = SelectedRule.MaterialNo,
                PrintDate = PrintDate,
                ResetKey = ResetKeyPreview,
                Records = result.Records.ToList()
            };

            Application.Current.Dispatcher.Invoke(() =>
                BarcodeGenerateResultWindow.ShowDialog(_printService, _recordService, dialogModel));
        }
        catch (Exception ex)
        {
            ClearStatusMessage();
            HandyControl.Controls.Growl.Error(EX(ex));
        }
        finally
        {
            IsGenerating = false;
        }
    }

    private void SetStatusMessage(string key, params object[] args)
    {
        _statusMessageKey = key;
        _statusMessageArgs = args;
        ApplyStatusMessage();
    }

    private void ClearStatusMessage()
    {
        _statusMessageKey = null;
        _statusMessageArgs = [];
        StatusMessage = string.Empty;
    }

    private void ApplyStatusMessage()
    {
        if (_statusMessageKey is null)
        {
            return;
        }

        StatusMessage = _statusMessageArgs.Length == 0
            ? L(_statusMessageKey)
            : string.Format(L(_statusMessageKey), _statusMessageArgs);
    }

    private void ResetPreview()
    {
        ResetKeyPreview = string.Empty;
        SerialConfigPreview = string.Empty;
        SerialRangePreview = string.Empty;
        SamplePreview = string.Empty;
        SampleLengthPreview = 0;
    }
}
