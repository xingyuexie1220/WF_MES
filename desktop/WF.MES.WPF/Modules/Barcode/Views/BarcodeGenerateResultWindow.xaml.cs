using System.Windows;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.WPF.Infrastructure;
using WF.MES.WPF.Modules.Barcode.ViewModels;

namespace WF.MES.WPF.Modules.Barcode.Views;

public partial class BarcodeGenerateResultWindow : Window
{
    private readonly BarcodeGenerateResultViewModel _viewModel;

    public BarcodeGenerateResultWindow(
        ILabelPrintService printService,
        IBarcodeGenerateRecordService recordService,
        BarcodeGeneratePrintDialogModel model)
    {
        InitializeComponent();

        _viewModel = new BarcodeGenerateResultViewModel(
            printService,
            recordService,
            model,
            WpfLocalization.Instance,
            ResolveUiText());
        _viewModel.RequestClose += Close;
        DataContext = _viewModel;
        _viewModel.Initialize();

        Title = _viewModel.WindowTitle;
    }

    public static void ShowDialog(
        ILabelPrintService printService,
        IBarcodeGenerateRecordService recordService,
        BarcodeGeneratePrintDialogModel model)
    {
        var window = new BarcodeGenerateResultWindow(printService, recordService, model)
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();
    }

    private static IDesktopUiText ResolveUiText() =>
        ((App)Application.Current).Container.Resolve<IDesktopUiText>();
}
