using System.Windows;
using WF.MES.Core.Interfaces;

namespace WF.MES.WPF.Auth.Views;

public partial class FactorySelectWindow : HandyControl.Controls.Window
{
    private readonly ILocalizationService _localization;

    public FactorySelectWindow(
        IEnumerable<WF.MES.Models.Dtos.FactorySummaryDto> factories,
        ILocalizationService localization)
    {
        _localization = localization;
        InitializeComponent();
        Title = _localization.T("ui.factory.selectTitle");
        FactoryList.ItemsSource = factories;
    }

    public WF.MES.Models.Dtos.FactorySummaryDto? SelectedFactory { get; private set; }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        SelectedFactory = FactoryList.SelectedItem as WF.MES.Models.Dtos.FactorySummaryDto;
        if (SelectedFactory == null)
        {
            HandyControl.Controls.Growl.Warning(_localization.T("ui.factory.selectRequired"));
            return;
        }

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
