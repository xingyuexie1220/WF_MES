using System.Windows.Media;
using WF.MES.Core.Exceptions;
using WF.MES.Core.Constants;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.WPF.Infrastructure;

namespace WF.MES.WPF.Modules.Barcode.ViewModels;

/// <summary>条码资料 QA 审核弹窗。</summary>
public class BarcodeQaReviewDialogViewModel : LocalizedViewModelBase
{
    private readonly IBarcodeQaReviewService _qaService;
    private readonly int _ruleId;
    private BarcodeQaReviewDetailDto? _detail;
    private bool _isBusy;
    private string _rejectRemark = string.Empty;
    private ImageSource? _drawingPreview;
    private ImageSource? _printSamplePreview;

    public BarcodeQaReviewDialogViewModel(
        IBarcodeQaReviewService qaService,
        int ruleId,
        ILocalizationService localization)
        : base(localization)
    {
        _qaService = qaService;
        _ruleId = ruleId;

        ApproveCommand = new DelegateCommand(async () => await ApproveAsync(), CanApprove)
            .ObservesProperty(() => IsBusy)
            .ObservesProperty(() => Detail);
        RejectCommand = new DelegateCommand(async () => await RejectAsync(), CanReject)
            .ObservesProperty(() => IsBusy)
            .ObservesProperty(() => Detail)
            .ObservesProperty(() => RejectRemark);
        CloseCommand = new DelegateCommand(() => RequestClose?.Invoke(false), () => !IsBusy)
            .ObservesProperty(() => IsBusy);
    }

    public event Action<bool>? RequestClose;

    public string WindowTitle => L("ui.barcode.qaTitle");

    public BarcodeQaReviewDetailDto? Detail
    {
        get => _detail;
        private set
        {
            if (SetProperty(ref _detail, value))
            {
                RaisePropertyChanged(nameof(TitleText));
                ApproveCommand.RaiseCanExecuteChanged();
                RejectCommand.RaiseCanExecuteChanged();
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
                ApproveCommand.RaiseCanExecuteChanged();
                RejectCommand.RaiseCanExecuteChanged();
                CloseCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string RejectRemark
    {
        get => _rejectRemark;
        set => SetProperty(ref _rejectRemark, value);
    }

    public ImageSource? DrawingPreview
    {
        get => _drawingPreview;
        private set
        {
            if (SetProperty(ref _drawingPreview, value))
            {
                ApproveCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ImageSource? PrintSamplePreview
    {
        get => _printSamplePreview;
        private set
        {
            if (SetProperty(ref _printSamplePreview, value))
            {
                ApproveCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string TitleText =>
        Detail == null ? WindowTitle : $"{Detail.CustomerName} / {Detail.MaterialNo}";

    public DelegateCommand ApproveCommand { get; }
    public DelegateCommand RejectCommand { get; }
    public DelegateCommand CloseCommand { get; }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            Detail = await _qaService.GetDetailAsync(_ruleId);
            if (Detail == null)
            {
                throw new BusinessException("err.materialRuleNotFound");
            }

            if (!BarcodeQaStatus.CanReview(Detail.QaStatus))
            {
                throw new BusinessException("err.qaPendingOnly");
            }

            DrawingPreview = BarcodeQaReviewImagePreview.FromBytes(Detail.DrawingImage);
            PrintSamplePreview = BarcodeQaReviewImagePreview.FromBytes(Detail.PrintSampleImage);
        }
        catch (Exception ex)
        {
            HandyControl.Controls.Growl.Error(EX(ex));
            RequestClose?.Invoke(false);
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected override void RefreshLocalizedProperties()
    {
        RaisePropertyChanged(nameof(WindowTitle));
        RaisePropertyChanged(nameof(TitleText));
    }

    private bool CanApproveAction =>
        !IsBusy
        && Detail != null
        && BarcodeQaStatus.CanReview(Detail.QaStatus)
        && DrawingPreview != null
        && PrintSamplePreview != null;

    private bool CanRejectAction =>
        !IsBusy && Detail != null && BarcodeQaStatus.CanReview(Detail.QaStatus);

    private bool CanApprove() => CanApproveAction;

    private bool CanReject() =>
        CanRejectAction && !string.IsNullOrWhiteSpace(RejectRemark);

    private async Task ApproveAsync()
    {
        if (Detail == null)
        {
            return;
        }

        IsBusy = true;
        try
        {
            await _qaService.ApproveAsync(Detail.RuleId);
            HandyControl.Controls.Growl.Success(L("ui.barcode.qaApproved"));
            RequestClose?.Invoke(true);
        }
        catch (Exception ex)
        {
            HandyControl.Controls.Growl.Error(EX(ex));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RejectAsync()
    {
        if (Detail == null)
        {
            return;
        }

        IsBusy = true;
        try
        {
            await _qaService.RejectAsync(new BarcodeQaReviewRejectDto
            {
                RuleId = Detail.RuleId,
                RejectRemark = RejectRemark.Trim()
            });

            HandyControl.Controls.Growl.Success(L("ui.barcode.qaRejected"));
            RequestClose?.Invoke(true);
        }
        catch (Exception ex)
        {
            HandyControl.Controls.Growl.Error(EX(ex));
        }
        finally
        {
            IsBusy = false;
        }
    }
}
