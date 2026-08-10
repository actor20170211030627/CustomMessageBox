using System.Windows;
using System.Windows.Media;

namespace CustomMessageBox.Dialog {
    
    // 内部接口，仅程序集内可见
    internal interface IMessageBoxBuilder {
        Window Owner { get; }
        string Title { get; }
        string Message { get; }
        MessageBoxButton Button { get; }
        bool HasButtons { get; }
        double ButtonMinWidth { get; }
        MessageBoxImage StandardIcon { get; }
        ImageSource CustomIcon { get; }
        MessageBoxImage WindowIcon { get; }
        ImageSource CustomWindowIcon { get; }
        MessageBoxResult DefaultResult { get; }
        string OkText { get; }
        string CancelText { get; }
        string YesText { get; }
        string NoText { get; }
    }
}