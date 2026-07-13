using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WF.MES.WPF.Infrastructure;

/// <summary>
/// 【高级】信息面板「字段标签 + 值」行。日常列表页用 <see cref="LocDataGridTextColumn"/>；
/// 仅明细/侧栏需要「标签：值」两段式时使用。
/// </summary>
public class LocInfoField : UserControl
{
    private TextBlock? _labelText;
    private TextBlock? _valueText;

    public static readonly DependencyProperty LabelKeyProperty =
        DependencyProperty.Register(
            nameof(LabelKey),
            typeof(string),
            typeof(LocInfoField),
            new PropertyMetadata(null, OnDisplayPropertyChanged));

    public static readonly DependencyProperty ValuePathProperty =
        DependencyProperty.Register(
            nameof(ValuePath),
            typeof(string),
            typeof(LocInfoField),
            new PropertyMetadata(null, OnDisplayPropertyChanged));

    public static readonly DependencyProperty EnumMapProperty =
        DependencyProperty.Register(
            nameof(EnumMap),
            typeof(string),
            typeof(LocInfoField),
            new PropertyMetadata(null, OnDisplayPropertyChanged));

    public static readonly DependencyProperty ValueStringFormatProperty =
        DependencyProperty.Register(
            nameof(ValueStringFormat),
            typeof(string),
            typeof(LocInfoField),
            new PropertyMetadata(null, OnDisplayPropertyChanged));

    public static readonly DependencyProperty ValueTextWrappingProperty =
        DependencyProperty.Register(
            nameof(ValueTextWrapping),
            typeof(TextWrapping),
            typeof(LocInfoField),
            new PropertyMetadata(TextWrapping.NoWrap, OnDisplayPropertyChanged));

    public string? LabelKey
    {
        get => (string?)GetValue(LabelKeyProperty);
        set => SetValue(LabelKeyProperty, value);
    }

    public string? ValuePath
    {
        get => (string?)GetValue(ValuePathProperty);
        set => SetValue(ValuePathProperty, value);
    }

    public string? EnumMap
    {
        get => (string?)GetValue(EnumMapProperty);
        set => SetValue(EnumMapProperty, value);
    }

    public string? ValueStringFormat
    {
        get => (string?)GetValue(ValueStringFormatProperty);
        set => SetValue(ValueStringFormatProperty, value);
    }

    public TextWrapping ValueTextWrapping
    {
        get => (TextWrapping)GetValue(ValueTextWrappingProperty);
        set => SetValue(ValueTextWrappingProperty, value);
    }

    public LocInfoField()
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        _labelText = new TextBlock();
        _valueText = new TextBlock();
        panel.Children.Add(_labelText);
        panel.Children.Add(_valueText);
        Content = panel;
        Loaded += (_, _) => ApplyBindings();
    }

    private static void OnDisplayPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((LocInfoField)d).ApplyBindings();

    private void ApplyBindings()
    {
        if (_labelText == null || _valueText == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(LabelKey))
        {
            Loc.SetFieldLabelKey(_labelText, LabelKey);
        }

        _valueText.TextWrapping = ValueTextWrapping;

        BindingOperations.ClearBinding(_valueText, TextBlock.TextProperty);
        if (string.IsNullOrWhiteSpace(ValuePath))
        {
            _valueText.Text = string.Empty;
            return;
        }

        var valueBinding = new Binding(ValuePath)
        {
            StringFormat = ValueStringFormat,
            TargetNullValue = string.Empty
        };

        _valueText.SetBinding(
            TextBlock.TextProperty,
            string.IsNullOrWhiteSpace(EnumMap)
                ? valueBinding
                : LocEnumBinding.Create(valueBinding, EnumMap));
    }
}
