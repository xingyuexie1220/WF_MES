using System.Windows;
using WF.MES.Core.Interfaces;
using WF.MES.WPF.Ui;
using WF.MES.WPF.Modules.Barcode.ViewModels;

namespace WF.MES.WPF.Modules.Barcode.Views;

public partial class BarcodeQaReviewDialogWindow : HandyControl.Controls.Window
{
    private readonly BarcodeQaReviewDialogViewModel _viewModel;
    private bool _dialogResult;

    private BarcodeQaReviewDialogWindow(IBarcodeQaReviewService qaService, int ruleId)
    {
        InitializeComponent();

        _viewModel = new BarcodeQaReviewDialogViewModel(
            qaService,
            ruleId,
            WpfLocalization.Instance);
        _viewModel.RequestClose += OnRequestClose;
        DataContext = _viewModel;
    }

    public static bool ShowDialog(IBarcodeQaReviewService qaService, int ruleId)
    {
        var window = new BarcodeQaReviewDialogWindow(qaService, ruleId)
        {
            Owner = Application.Current.MainWindow
        };

        window.ShowDialog();
        return window._dialogResult;
    }

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        Title = _viewModel.TitleText;
        await _viewModel.InitializeAsync();
        Title = _viewModel.TitleText;
    }

    private void OnRequestClose(bool changed)
    {
        _dialogResult = changed;
        DialogResult = true;
        Close();
    }
}
