namespace BH.UI.Enums
{
    /// <summary>
    /// UI层级定义（控制显示优先级）
    /// </summary>
    public enum UILayer
    {
        Bottom = 0,    // 底层（如背景、主界面）
        Middle = 10,   // 中层（如功能面板）
        Popup = 20,    // 弹窗层（如确认框）
        Top = 30       // 顶层（如加载、提示）
    }
}