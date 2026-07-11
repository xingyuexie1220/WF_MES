using System.Collections.ObjectModel;
using System.Windows;
using WF.MES.Core.Interfaces;

namespace WF.MES.WPF.Views.Login;

public partial class FactorySelectWindow : HandyControl.Controls.Window
{
    private readonly ILocalizationService _localization;

    public FactorySelectWindow(
        IEnumerable<WF.MES.Models.Dtos.FactorySummaryDto> factories,
        ILocalizationService localization)
    {
        _localization = localization;
        InitializeComponent();
        FactoryList.ItemsSource = factories;
        ApplyTexts();
    }

    public WF.MES.Models.Dtos.FactorySummaryDto? SelectedFactory { get; private set; }

    private void ApplyTexts()
    {
        Title = _localization.T("desktop.factory.selectTitle");
        TitleText.Text = _localization.T("desktop.factory.selectTitle");
        HintText.Text = _localization.T("desktop.factory.selectHint");
        CancelButton.Content = _localization.T("common.cancel");
        ConfirmButton.Content = _localization.T("common.confirm");
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        SelectedFactory = FactoryList.SelectedItem as WF.MES.Models.Dtos.FactorySummaryDto;
        if (SelectedFactory == null)
        {
            HandyControl.Controls.Growl.Warning(_localization.T("desktop.factory.selectRequired"));
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

public sealed class LocaleDisplayItem
{
    public LocaleDisplayItem(string value, string label)
    {
        Value = value;
        Label = label;
    }

    public string Value { get; }
    public string Label { get; }
}
