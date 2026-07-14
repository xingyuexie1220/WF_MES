using System.Collections.ObjectModel;
using System.Windows;
using WF.MES.Core.Constants;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.WPF.Ui;

namespace WF.MES.WPF.Modules.Barcode.ViewModels;

/// <summary>料号条码规则与规则段编辑。</summary>
public class MaterialRuleViewModel : LocalizedViewModelBase, INavigationAware
{
    private readonly IMaterialBarcodeRuleService _ruleService;
    private readonly ICustomerService _customerService;
    private readonly IBarcodeBuilder _barcodeBuilder;

    private MaterialRuleListDto? _selectedRule;
    private MaterialRuleEditDto _editModel = new();
    private bool _isBusy;
    private bool _isNew;
    private CustomerListDto? _selectedCustomer;
    private string _previewText = string.Empty;
    private int _previewLength;
    private DateTime _previewDate = DateTime.Today;
    private List<RuleSegmentEditDto>? _savedSegmentSnapshot;
    private readonly Dictionary<int, string> _segmentSummaryRaw = new();

    public MaterialRuleViewModel(
        IMaterialBarcodeRuleService ruleService,
        ICustomerService customerService,
        IBarcodeBuilder barcodeBuilder,
        ILocalizationService localization)
        : base(localization)
    {
        _ruleService = ruleService;
        _customerService = customerService;
        _barcodeBuilder = barcodeBuilder;

        RebuildSelectOptions();
        AddCommand = new DelegateCommand(StartAdd);
        EditCommand = new DelegateCommand(async () => await StartEditAsync(), () => SelectedRule != null)
            .ObservesProperty(() => SelectedRule);
        SaveCommand = new DelegateCommand(async () => await SaveAsync(), CanSave)
            .ObservesProperty(() => IsBusy)
            .ObservesProperty(() => EditMaterialNo)
            .ObservesProperty(() => SelectedCustomer);
        RefreshCommand = new DelegateCommand(async () => await LoadRulesAsync());
        AddSegmentCommand = new DelegateCommand(AddSegment);
        RemoveSegmentCommand = new DelegateCommand<RuleSegmentItemViewModel?>(RemoveSegment);
        PreviewCommand = new DelegateCommand(UpdatePreview);
    }

    public string FormTitle => IsNew ? L("ui.barcode.materialRuleFormNew") : L("ui.barcode.materialRuleFormEdit");

    public string PageTitle => L("ui.barcode.materialRuleTitle");

    public string SegmentTitle => L("ui.barcode.segmentTitle");

    public string TotalLengthText => string.Format(L("ui.barcode.totalLength"), PreviewLength);

    public IReadOnlyList<BarcodeSegmentTypeOption> SegmentTypeOptions { get; private set; } = [];

    public IReadOnlyList<DateFormatOption> DateFormatOptions { get; private set; } = [];

    public IReadOnlyList<SerialRadixOption> SerialRadixOptions { get; private set; } = [];

    public ObservableCollection<MaterialRuleListDto> Rules { get; } = [];
    public ObservableCollection<CustomerListDto> Customers { get; } = [];
    public ObservableCollection<RuleSegmentItemViewModel> Segments { get; } = [];

    public MaterialRuleListDto? SelectedRule
    {
        get => _selectedRule;
        set => SetProperty(ref _selectedRule, value);
    }

    public MaterialRuleEditDto EditModel
    {
        get => _editModel;
        set
        {
            if (SetProperty(ref _editModel, value))
            {
                RaisePropertyChanged(nameof(EditMaterialNo));
                RaisePropertyChanged(nameof(EditBarcodeLength));
            }
        }
    }

    public CustomerListDto? SelectedCustomer
    {
        get => _selectedCustomer;
        set => SetProperty(ref _selectedCustomer, value);
    }

    public string EditMaterialNo
    {
        get => _editModel.MaterialNo;
        set
        {
            if (_editModel.MaterialNo != value)
            {
                _editModel.MaterialNo = value;
                RaisePropertyChanged();
            }
        }
    }

    public int EditBarcodeLength
    {
        get => _editModel.BarcodeLength;
        set
        {
            if (_editModel.BarcodeLength != value)
            {
                _editModel.BarcodeLength = value;
                RaisePropertyChanged();
            }
        }
    }

    public DateTime PreviewDate
    {
        get => _previewDate;
        set
        {
            if (SetProperty(ref _previewDate, value))
            {
                UpdatePreview();
            }
        }
    }

    public string PreviewText
    {
        get => _previewText;
        set => SetProperty(ref _previewText, value);
    }

    public int PreviewLength
    {
        get => _previewLength;
        private set
        {
            if (SetProperty(ref _previewLength, value))
            {
                RaisePropertyChanged(nameof(ShowPreviewLength));
                RaisePropertyChanged(nameof(TotalLengthText));
            }
        }
    }

    public bool ShowPreviewLength => PreviewLength > 0;

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

    public DelegateCommand AddCommand { get; }
    public DelegateCommand EditCommand { get; }
    public DelegateCommand SaveCommand { get; }
    public DelegateCommand RefreshCommand { get; }
    public DelegateCommand AddSegmentCommand { get; }
    public DelegateCommand<RuleSegmentItemViewModel?> RemoveSegmentCommand { get; }
    public DelegateCommand PreviewCommand { get; }

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        var customerId = EditModel.CustomerId > 0 ? EditModel.CustomerId : SelectedCustomer?.CustomerId;
        await LoadCustomersAsync(customerId);
        await LoadRulesAsync();
        RestoreSelectedCustomer(customerId);
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    private void RebuildSelectOptions()
    {
        SegmentTypeOptions = LocalizedOptions.SegmentTypes(key => L(key));
        DateFormatOptions = LocalizedOptions.DateFormats(key => L(key));
        SerialRadixOptions = LocalizedOptions.SerialRadices(key => L(key));

        RaisePropertyChanged(nameof(SegmentTypeOptions));
        RaisePropertyChanged(nameof(DateFormatOptions));
        RaisePropertyChanged(nameof(SerialRadixOptions));
    }

    private bool CanSave() =>
        !IsBusy && SelectedCustomer != null && !string.IsNullOrWhiteSpace(EditMaterialNo);

    private async Task LoadCustomersAsync(int? ensureCustomerId = null)
    {
        Customers.Clear();
        foreach (var customer in await _customerService.GetCustomerSelectionListAsync(ensureCustomerId))
        {
            Customers.Add(customer);
        }
    }

    private void RestoreSelectedCustomer(int? customerId)
    {
        if (customerId is > 0)
        {
            SelectedCustomer = Customers.FirstOrDefault(c => c.CustomerId == customerId);
            return;
        }

        if (IsNew && SelectedCustomer == null)
        {
            SelectedCustomer = Customers.FirstOrDefault();
        }
    }

    private async Task LoadRulesAsync()
    {
        IsBusy = true;
        try
        {
            Rules.Clear();
            _segmentSummaryRaw.Clear();
            foreach (var rule in await _ruleService.GetRulesAsync())
            {
                _segmentSummaryRaw[rule.RuleId] = rule.SegmentSummary;
                rule.SegmentSummary = LocalizedOptions.TranslateSegmentSummary(rule.SegmentSummary, key => L(key));
                Rules.Add(rule);
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
        _savedSegmentSnapshot = null;
        EditModel = new MaterialRuleEditDto();
        SelectedCustomer = Customers.FirstOrDefault();
        Segments.Clear();
        AddSegment();
        UpdatePreview();
    }

    private async Task StartEditAsync()
    {
        if (SelectedRule == null)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var rule = await _ruleService.GetRuleAsync(SelectedRule.RuleId);
            if (rule == null)
            {
                return;
            }

            await LoadCustomersAsync(rule.CustomerId);

            IsNew = false;
            EditModel = rule;
            _savedSegmentSnapshot = CloneSegments(rule.Segments);
            SelectedCustomer = Customers.FirstOrDefault(c => c.CustomerId == rule.CustomerId);
            LoadSegments(rule.Segments);
            UpdatePreview();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void LoadSegments(IEnumerable<RuleSegmentEditDto> segments)
    {
        Segments.Clear();
        foreach (var s in segments.OrderBy(x => x.SortOrder))
        {
            Segments.Add(CreateSegmentItem(s));
        }

        RefreshSegmentOrderLabels();
    }

    private RuleSegmentItemViewModel CreateSegmentItem(RuleSegmentEditDto s)
    {
        var item = new RuleSegmentItemViewModel
        {
            SortOrder = s.SortOrder,
            SegmentType = s.SegmentType,
            IncludeInResetKey = s.IncludeInResetKey,
            LiteralValue = s.LiteralValue,
            DateFormat = s.DateFormat,
            SerialRadix = s.SerialRadix,
            SerialDigits = s.SerialDigits
        };
        item.RefreshLocalizedText(key => L(key));
        return item;
    }

    private void AddSegment()
    {
        var item = new RuleSegmentItemViewModel { SegmentType = BarcodeSegmentTypes.Literal };
        item.RefreshLocalizedText(key => L(key));
        Segments.Add(item);
        ReorderSegments();
        UpdatePreview();
    }

    private void RemoveSegment(RuleSegmentItemViewModel? item)
    {
        if (item != null)
        {
            Segments.Remove(item);
            ReorderSegments();
            UpdatePreview();
        }
    }

    private void ReorderSegments()
    {
        for (var i = 0; i < Segments.Count; i++)
        {
            Segments[i].SortOrder = i + 1;
        }

        RefreshSegmentOrderLabels();
    }

    private void RefreshSegmentOrderLabels()
    {
        foreach (var segment in Segments)
        {
            segment.SortOrderLabel = string.Format(L("ui.barcode.segmentOrder"), segment.SortOrder);
        }
    }

    private void UpdatePreview()
    {
        try
        {
            var entities = ToSegmentEntities();
            _barcodeBuilder.ValidateSegments(entities);
            var sample = _barcodeBuilder.PreviewSample(entities, new BarcodeBuildContext
            {
                PrintDate = PreviewDate
            });
            PreviewText = sample;
            PreviewLength = sample.Length;
        }
        catch (Exception ex)
        {
            PreviewText = Lf("ui.barcode.previewFailed", Ex(ex));
            PreviewLength = 0;
        }
    }

    private async Task SaveAsync()
    {
        EditModel.CustomerId = SelectedCustomer?.CustomerId ?? 0;
        EditModel.Segments = Segments.Select(s => new RuleSegmentEditDto
        {
            SortOrder = s.SortOrder,
            SegmentType = s.SegmentType,
            IncludeInResetKey = s.IncludeInResetKey,
            LiteralValue = s.LiteralValue,
            DateFormat = s.DateFormat,
            SerialRadix = s.SerialRadix,
            SerialDigits = s.SerialDigits
        }).ToList();

        UpdatePreview();

        IsBusy = true;
        var isCreate = EditModel.RuleId == 0;
        try
        {
            if (!await ConfirmResetKeyChangeIfNeededAsync())
            {
                return;
            }

            var ruleId = await _ruleService.SaveRuleAsync(EditModel);
            await LoadRulesAsync();

            var saved = await _ruleService.GetRuleAsync(ruleId);
            if (saved != null)
            {
                IsNew = false;
                EditModel = saved;
                SelectedRule = Rules.FirstOrDefault(r => r.RuleId == ruleId);
                await LoadCustomersAsync(saved.CustomerId);
                SelectedCustomer = Customers.FirstOrDefault(c => c.CustomerId == saved.CustomerId);
                LoadSegments(saved.Segments);
                _savedSegmentSnapshot = CloneSegments(saved.Segments);
                UpdatePreview();
            }

            HandyControl.Controls.Growl.Success(
                isCreate
                    ? L("ui.barcode.materialRuleSaveSuccess")
                    : L("ui.barcode.materialRuleSaveSuccessReset"));
        }
        catch (Exception ex)
        {
            HandyControl.Controls.Growl.Error(Ex(ex));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private List<Models.Entities.BarcodeRuleSegment> ToSegmentEntities() =>
        ToSegmentEntities(Segments.Select(s => new RuleSegmentEditDto
        {
            SortOrder = s.SortOrder,
            SegmentType = s.SegmentType,
            IncludeInResetKey = s.IncludeInResetKey,
            LiteralValue = s.LiteralValue,
            DateFormat = s.DateFormat,
            SerialRadix = s.SerialRadix,
            SerialDigits = s.SerialDigits
        }));

    private static List<Models.Entities.BarcodeRuleSegment> ToSegmentEntities(IEnumerable<RuleSegmentEditDto> segments)
    {
        return segments.Select(s => new Models.Entities.BarcodeRuleSegment
        {
            SortOrder = s.SortOrder,
            SegmentType = s.SegmentType,
            ConfigJson = RuleSegmentConfigHelper.ToConfigJson(new RuleSegmentEditDto
            {
                SegmentType = s.SegmentType,
                LiteralValue = s.LiteralValue,
                DateFormat = s.DateFormat,
                SerialRadix = s.SerialRadix,
                SerialDigits = s.SerialDigits
            }),
            IncludeInResetKey = s.SegmentType == BarcodeSegmentTypes.Serial ? 0 : s.IncludeInResetKey ? 1 : 0
        }).ToList();
    }

    private async Task<bool> ConfirmResetKeyChangeIfNeededAsync()
    {
        if (IsNew || EditModel.RuleId <= 0 || _savedSegmentSnapshot == null)
        {
            return true;
        }

        if (!await _ruleService.HasGenerationHistoryAsync(EditModel.RuleId))
        {
            return true;
        }

        var context = new BarcodeBuildContext { PrintDate = PreviewDate.Date };
        var oldKey = _barcodeBuilder.BuildResetKey(ToSegmentEntities(_savedSegmentSnapshot), context);
        var newKey = _barcodeBuilder.BuildResetKey(ToSegmentEntities(), context);

        if (string.Equals(oldKey, newKey, StringComparison.Ordinal))
        {
            return true;
        }

        var emptyKey = L("ui.barcode.emptyKey");
        var message = string.Format(
            L("ui.barcode.resetKeyConfirmBody"),
            PreviewDate,
            string.IsNullOrEmpty(oldKey) ? emptyKey : oldKey,
            string.IsNullOrEmpty(newKey) ? emptyKey : newKey);

        return MessageBox.Show(
                   message,
                   L("ui.barcode.resetKeyConfirmTitle"),
                   MessageBoxButton.YesNo,
                   MessageBoxImage.Warning,
                   MessageBoxResult.No)
               == MessageBoxResult.Yes;
    }

    private static List<RuleSegmentEditDto> CloneSegments(IEnumerable<RuleSegmentEditDto> segments)
    {
        return segments.Select(s => new RuleSegmentEditDto
        {
            SegmentId = s.SegmentId,
            SortOrder = s.SortOrder,
            SegmentType = s.SegmentType,
            IncludeInResetKey = s.IncludeInResetKey,
            LiteralValue = s.LiteralValue,
            DateFormat = s.DateFormat,
            SerialRadix = s.SerialRadix,
            SerialDigits = s.SerialDigits
        }).ToList();
    }
}
