namespace WF.MES.Core.Constants;

/// <summary>
/// 受控操作权限码（与 System_Menu.Permission 一致，MenuType=3）。
/// </summary>
public static class MenuActions
{
    public static class BarcodeQaReview
    {
        public const string SaveAttachments = "barcode:qareview:saveattachments";
        public const string Review = "barcode:qareview:review";
    }
}
