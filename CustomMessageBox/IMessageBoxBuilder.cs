using System.Windows;
using System.Windows.Media;

namespace Actor.CustomMessageBox {
    
    // 内部接口，仅程序集内可见
    internal interface IMessageBoxBuilder {
        Window Owner { get; }
        bool HasWindowIcon { get; }
        MessageBoxImage WindowIcon { get; }
        ImageSource CustomWindowIcon { get; }
        string Title { get; }
        bool EnableCloseBtn { get; }
        MessageBoxImage StandardIcon { get; }
        ImageSource CustomIcon { get; }
        string Message { get; }
        bool HasButtons { get; }
        MessageBoxButton Button { get; }
        double ButtonMinWidth { get; }
        MessageBoxResult DefaultResult { get; }
        object OkText { get; }
        bool CloseOnClickOk { get; }
        object CancelText { get; }
        bool CloseOnClickCancel { get; }
        object YesText { get; }
        bool CloseOnClickYes { get; }
        object NoText { get; }
        bool CloseOnClickNo { get; }
        bool CloseOnPressedEsc { get; }
    }
}