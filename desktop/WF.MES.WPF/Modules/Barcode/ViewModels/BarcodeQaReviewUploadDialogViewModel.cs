using System.Windows.Media;
using Microsoft.Win32;
using WF.MES.Core.Constants;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.WPF.Infrastructure;

namespace WF.MES.WPF.Modules.Barcode.ViewModels;

/// <summary>条码资料文员上传弹窗。</summary>
public class BarcodeQaReviewUploadDialogViewModel : LocalizedViewModelBase
{
    private readonly IBarcodeQaReviewService _qaService;
    private readonly int _ruleId;
    private BarcodeQaReviewDetailDto? _detail;
    private bool _isBusy;
    private string? _pendingDrawingPath;
    private string? _pendingPrintSamplePath;
    private ImageSource? _drawingPreview;
    private ImageSource? _printSamplePreview;

    public BarcodeQaReviewUploadDialogViewModel(
        IBarcodeQaReviewService qaService,
        int ruleId,
        ILocalizationService localization,
        IDesktopUiText ui)
        : base(localization)
    {
        _qaService = qaService;
        _ruleId = ruleId;
        Ui = ui;

        PickDrawingCommand = new DelegateCommand(PickDrawing, () => CanEditAttachments)
            .ObservesProperty(() => IsBusy)
            .ObservesProperty(() => Detail);
        PickPrintSampleCommand = new DelegateCommand(PickPrintSample, () => CanEditAttachments)
            .ObservesProperty(() => IsBusy)
            .ObservesProperty(() => Detail);
        SaveAttachmentsCommand = new DelegateCommand(async () => await SaveAttachmentsAsync(), () => CanSaveAttachments)
            .ObservesProperty(() => IsBusy)
            .ObservesProperty(() => Detail);
        CloseCommand = new DelegateCommand(() => RequestClose?.Invoke(false), () => !IsBusy)
            .ObservesProperty(() => IsBusy);
    }

    public IDesktopUiText Ui { get; }

    public event Action<bool>? RequestClose;

    public string WindowTitle => L("desktop.barcode.qaUploadTitle");

    public string PreviousRejectReasonLabel => L("desktop.barcode.qaLastRejectReason");

    public string UploadHint => L("desktop.barcode.qaUploadHint");

    public string DrawingPreviewTitle => L("desktop.barcode.qaDrawingPreview");

    public string SamplePreviewTitle => L("desktop.barcode.qaSamplePreview");

    public string SelectDrawingText => L("desktop.actions.selectDrawing");

    public string SelectSampleText => L("desktop.actions.selectSample");

    public string SaveAttachmentsText => L("desktop.actions.saveAttachments");

    public string BarcodeTotalLengthLine =>
        Detail == null ? string.Empty : $"{Ui.BarcodeLength}：{Detail.BarcodeLength}";

    public string QaStatusLine =>
        Detail == null ? string.Empty : $"{Ui.Status}：{Detail.QaStatusText}";

    public BarcodeQaReviewDetailDto? Detail
    {
        get => _detail;
        private set
        {
            if (SetProperty(ref _detail, value))
            {
                RaisePropertyChanged(nameof(TitleText));
                RaisePropertyChanged(nameof(HasPreviousRejectRemark));
                RaisePropertyChanged(nameof(CanEditAttachments));
                RaisePropertyChanged(nameof(BarcodeTotalLengthLine));
                RaisePropertyChanged(nameof(QaStatusLine));
                PickDrawingCommand.RaiseCanExecuteChanged();
                PickPrintSampleCommand.RaiseCanExecuteChanged();
                SaveAttachmentsCommand.RaiseCanExecuteChanged();
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
                RaisePropertyChanged(nameof(CanEditAttachments));
                RaisePropertyChanged(nameof(CanSaveAttachments));
                CloseCommand.RaiseCanExecuteChanged();
                PickDrawingCommand.RaiseCanExecuteChanged();
                PickPrintSampleCommand.RaiseCanExecuteChanged();
                SaveAttachmentsCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string? PendingDrawingPath
    {
        get => _pendingDrawingPath;
        private set => SetProperty(ref _pendingDrawingPath, value);
    }

    public string? PendingPrintSamplePath
    {
        get => _pendingPrintSamplePath;
        private set => SetProperty(ref _pendingPrintSamplePath, value);
    }

    public ImageSource? DrawingPreview
    {
        get => _drawingPreview;
        private set
        {
            if (SetProperty(ref _drawingPreview, value))
            {
                RaisePropertyChanged(nameof(HasDrawingImage));
                RaisePropertyChanged(nameof(CanSaveAttachments));
                SaveAttachmentsCommand.RaiseCanExecuteChanged();
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
                RaisePropertyChanged(nameof(HasPrintSampleImage));
                RaisePropertyChanged(nameof(CanSaveAttachments));
                SaveAttachmentsCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string TitleText =>
        Detail == null ? WindowTitle : $"{Detail.CustomerName} / {Detail.MaterialNo}";

    public bool HasPreviousRejectRemark =>
        Detail != null
        && Detail.QaStatus == BarcodeQaStatus.Rejected
        && !string.IsNullOrWhiteSpace(Detail.QaReviewRemark);

    public bool CanEditAttachments => !IsBusy && Detail != null && BarcodeQaStatus.CanUpload(Detail.QaStatus);

    public bool HasDrawingImage => DrawingPreview != null;

    public bool HasPrintSampleImage => PrintSamplePreview != null;

    public bool CanSaveAttachments => CanEditAttachments && HasDrawingImage && HasPrintSampleImage;

    public DelegateCommand PickDrawingCommand { get; }
    public DelegateCommand PickPrintSampleCommand { get; }
    public DelegateCommand SaveAttachmentsCommand { get; }
    public DelegateCommand CloseCommand { get; }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            Detail = await _qaService.GetDetailAsync(_ruleId);
            if (Detail == null)
            {
                throw new InvalidOperationException(L("desktop.barcode.ruleNotFound"));
            }

            if (!BarcodeQaStatus.CanUpload(Detail.QaStatus))
            {
                throw new InvalidOperationException(L("desktop.barcode.uploadNotAllowed"));
            }

            DrawingPreview = BarcodeQaReviewImagePreview.FromBytes(Detail.DrawingImage);
            PrintSamplePreview = BarcodeQaReviewImagePreview.FromBytes(Detail.PrintSampleImage);
        }
        catch (Exception ex)
        {
            HandyControl.Controls.Growl.Error(ex.Message);
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
        RaisePropertyChanged(nameof(PreviousRejectReasonLabel));
        RaisePropertyChanged(nameof(UploadHint));
        RaisePropertyChanged(nameof(DrawingPreviewTitle));
        RaisePropertyChanged(nameof(SamplePreviewTitle));
        RaisePropertyChanged(nameof(SelectDrawingText));
        RaisePropertyChanged(nameof(SelectSampleText));
        RaisePropertyChanged(nameof(SaveAttachmentsText));
        RaisePropertyChanged(nameof(TitleText));
        RaisePropertyChanged(nameof(BarcodeTotalLengthLine));
        RaisePropertyChanged(nameof(QaStatusLine));
    }

    private void PickDrawing()
    {
        var path = PickImageFile();
        if (path == null)
        {
            return;
        }

        PendingDrawingPath = path;
        DrawingPreview = BarcodeQaReviewImagePreview.FromFile(path);
    }

    private void PickPrintSample()
    {
        var path = PickImageFile();
        if (path == null)
        {
            return;
        }

        PendingPrintSamplePath = path;
        PrintSamplePreview = BarcodeQaReviewImagePreview.FromFile(path);
    }

    private string? PickImageFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = L("desktop.barcode.imageFilter"),
            Multiselect = false
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    private async Task SaveAttachmentsAsync()
    {
        if (Detail == null)
        {
            return;
        }

        IsBusy = true;
        try
        {
            await _qaService.SaveAttachmentsFromFilesAsync(
                Detail.RuleId,
                PendingDrawingPath,
                PendingPrintSamplePath);

            HandyControl.Controls.Growl.Success(
                Detail.QaStatus == BarcodeQaStatus.Rejected
                    ? L("desktop.barcode.qaAttachmentsSavedPending")
                    : L("desktop.barcode.qaAttachmentsSaved"));
            RequestClose?.Invoke(true);
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
