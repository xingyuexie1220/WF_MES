using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WF.MES.WPF.Infrastructure;

/// <summary>
/// DataGrid 本地化列（本文件：<see cref="LocDataGridTextColumn"/> / <see cref="LocDataGridEnumColumn"/> / <see cref="LocEnumMaps"/>）。
/// </summary>
public class LocDataGridTextColumn : DataGridTextColumn
{
    private string? _locKey;

    public string? LocKey
    {
        get => _locKey;
        set
        {
            _locKey = value;
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (Loc.IsDesignMode)
            {
                Header = Loc.ResolveDesignText(value);
                HeaderTemplate = null;
                return;
            }

            HeaderTemplate = CreateHeaderTemplate(value);
        }
    }

    private static DataTemplate CreateHeaderTemplate(string key)
    {
        var template = new DataTemplate();
        var textBlock = new FrameworkElementFactory(typeof(TextBlock));
        textBlock.SetBinding(TextBlock.TextProperty, Loc.CreateBinding(key));
        textBlock.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        template.VisualTree = textBlock;
        return template;
    }
}

/// <summary>
/// 绑定原始枚举/状态值，经 <see cref="LocEnumConverter"/> 在 UI 层翻译。
/// <c>EnumMap</c> 取值见 <see cref="LocEnumMaps"/>。
/// </summary>
public class LocDataGridEnumColumn : LocDataGridTextColumn
{
    private string? _enumMap;
    private Binding? _valueBinding;

    public string? EnumMap
    {
        get => _enumMap;
        set
        {
            _enumMap = value;
            ApplyEnumBinding();
        }
    }

    public new BindingBase? Binding
    {
        get => base.Binding;
        set
        {
            _valueBinding = value as Binding;
            ApplyEnumBinding();
        }
    }

    private void ApplyEnumBinding()
    {
        if (_valueBinding == null)
        {
            return;
        }

        base.Binding = string.IsNullOrWhiteSpace(_enumMap)
            ? _valueBinding
            : LocEnumBinding.Create(_valueBinding, _enumMap);
    }
}

/// <summary><see cref="LocDataGridEnumColumn.EnumMap"/> 标识。</summary>
public static class LocEnumMaps
{
    public const string PrintStatus = "printStatus";

    public const string QaStatus = "qaStatus";

    public const string Enable = "enable";

    public const string Attachment = "attachment";
}
