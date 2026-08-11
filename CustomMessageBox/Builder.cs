using System.Windows;
using System.Windows.Media;

namespace Actor.CustomMessageBox {

    /// <summary>Builder 模式 </summary>
    public class Builder: IMessageBoxBuilder {
            private readonly Window _owner;
            private readonly string _message;
            private string _title;
            private MessageBoxButton _button = MessageBoxButton.OK;
            private bool _hasButtons = true, _hasWindowIcon = true;
            private double _buttonMinWidth = 75;
            private MessageBoxImage _standardIcon = MessageBoxImage.None;
            private MessageBoxImage _windowIcon = MessageBoxImage.None;
            private ImageSource _customIcon, _customWindowIcon;
            private MessageBoxResult _defaultResult = MessageBoxResult.None;
            private string _okText, _cancelText, _yesText, _noText;

            /// <summary>创建Builder</summary>
            /// <param name="message">显示内容</param>
            public Builder(string message) {
                this._message = message;
            }
            /// <summary>创建Builder</summary>
            /// <param name="owner">当前Window, 没有就不要传</param>
            /// <param name="message">显示内容</param>
            public Builder(Window owner, string message) {
                this._owner = owner;
                this._message = message;
            }

            /// <summary>设置标题</summary>
            /// <param name="caption">弹框顶部标题</param>
            /// <returns></returns>
            public Builder SetCaption(string caption) => SetTitle(caption);

            /// <summary>
            /// 设置标题
            /// </summary>
            /// <param name="title">弹框顶部标题</param>
            /// <returns></returns>
            public Builder SetTitle(string title) { this._title = title; return this; }
            
            /// <summary>设置按钮</summary>
            /// <param name="button"><see cref="T:System.Windows.MessageBoxButton" /> 按钮类型, 例: <see cref="F:System.Windows.MessageBoxButton.YesNo">MessageBoxButton.YesNo</see></param>
            /// <returns></returns>
            public Builder SetButton(MessageBoxButton button) { this._button = button; return this; }
            
            /// <summary>设置是否有按钮, 默认True</summary>
            /// <returns></returns>
            public Builder SetHasButtons(bool hasButtons) { this._hasButtons = hasButtons; return this; }

            /// <summary>
            /// 设置Button最小宽度
            /// </summary>
            /// <param name="minWidth">Button最小宽度</param>
            /// <returns></returns>
            public Builder SetButtonMinWidth(double minWidth) {
                this._buttonMinWidth = minWidth;
                return this;
            }

            /// <summary>设置标准图标</summary>
            /// <param name="icon"><see cref="T:System.Windows.MessageBoxImage" /> 弹框图标, 例: <br />
            ///     <see cref="F:System.Windows.MessageBoxImage.None">MessageBoxImage.None</see> (默认) <br />
            ///     <see cref="F:System.Windows.MessageBoxImage.Error">MessageBoxImage.Error</see> <br />
            ///     <see cref="F:System.Windows.MessageBoxImage.Hand">MessageBoxImage.Hand</see> <br />
            ///     <see cref="F:System.Windows.MessageBoxImage.Stop">MessageBoxImage.Stop</see> <br />
            ///     <see cref="F:System.Windows.MessageBoxImage.Question">MessageBoxImage.Question</see> <br />
            ///     <see cref="F:System.Windows.MessageBoxImage.Exclamation">MessageBoxImage.Exclamation</see> <br />
            ///     <see cref="F:System.Windows.MessageBoxImage.Warning">MessageBoxImage.Warning</see> <br />
            ///     <see cref="F:System.Windows.MessageBoxImage.Asterisk">MessageBoxImage.Asterisk</see> <br />
            ///     <see cref="F:System.Windows.MessageBoxImage.Information">MessageBoxImage.Information</see> <br />
            /// </param>
            /// <returns></returns>
            public Builder SetIcon(MessageBoxImage icon) { 
                this._standardIcon = icon; 
                this._customIcon = null;
                return this;
            }

            /// <summary>设置自定义图标</summary>
            /// <param name="icon">
            ///     System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();<br />
            ///     string AssemblyName = assembly.GetName().Name;<br />
            ///     Uri uri = new Uri($"pack://application:,,,/{AssemblyName};component/Resources/Images/xxx.png");<br />
            ///     var icon = new System.Windows.Media.Imaging.BitmapImage(uri);
            /// </param>
            /// <returns></returns>
            public Builder SetIcon(ImageSource icon) {
                this._customIcon = icon; 
                this._standardIcon = MessageBoxImage.None;
                return this; 
            }

            /// <summary>
            /// 设置默认按钮，它的作用是：<br />
            /// 1. 初始焦点：对话框打开时，这个按钮会获得键盘焦点（通常显示为蓝色高亮或虚线框）。<br />
            /// 2. Enter 键触发：用户按下 Enter 键时，会触发这个按钮的点击事件。<br />
            /// 3. 允许切换：用户仍可以通过 Tab 键或方向键（←/→）在按钮间切换焦点，此时 Enter 键会触发当前焦点所在的按钮，而不再是默认按钮。<br />
            /// 4.系统的MessageBox.Show(...) 经测试: if只有1个按钮, 就算调用本方法设置了, 按Enter or 点击↗角 ❌️ 还是会触发MessageBoxResult.OK, 无语...<br />
            /// 它定义了当用户直接按 Enter 时返回的结果，但并不强制用户只能点击这个按钮。
            /// </summary>
            /// <param name="defaultResult"><see cref="T:System.Windows.MessageBoxResult" /> 默认按钮, 例: <br />
            ///     <see cref="F:System.Windows.MessageBoxResult.None">MessageBoxResult.None</see> (默认) <br />
            ///     <see cref="F:System.Windows.MessageBoxResult.OK">MessageBoxResult.OK</see> <br />
            ///     <see cref="F:System.Windows.MessageBoxResult.Cancel">MessageBoxResult.Cancel</see> <br />
            ///     <see cref="F:System.Windows.MessageBoxResult.Yes">MessageBoxResult.Yes</see> <br />
            ///     <see cref="F:System.Windows.MessageBoxResult.No">MessageBoxResult.No</see>
            /// </param>
            /// <returns></returns>
            public Builder SetDefaultResult(MessageBoxResult defaultResult) {
                this._defaultResult = defaultResult;
                return this;
            }

            /// <summary>设置是否显示窗口↖️角的Icon, 默认true, 并显示App的icon</summary>
            /// <param name="hasWindowIcon"></param>
            /// <returns></returns>
            public Builder SetHasWindowIcon(bool hasWindowIcon) {
                this._hasWindowIcon = hasWindowIcon;
                return this;
            }

            /// <summary>设置窗口↖️角的Icon</summary>
            /// <param name="windowIcon"></param>
            /// <returns></returns>
            public Builder SetWindowIcon(MessageBoxImage windowIcon) {
                this._windowIcon = windowIcon;
                this._customWindowIcon = null;
                return this;
            }

            /// <summary>设置窗口↖️角的Icon</summary>
            /// <param name="windowIcon">
            ///     System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();<br />
            ///     string AssemblyName = assembly.GetName().Name;<br />
            ///     Uri uri = new Uri($"pack://application:,,,/{AssemblyName};component/Resources/Images/xxx.png");<br />
            ///     var icon = new System.Windows.Media.Imaging.BitmapImage(uri);
            /// </param>
            /// <returns></returns>
            public Builder SetWindowIcon(ImageSource windowIcon) {
                this._windowIcon = MessageBoxImage.None;
                this._customWindowIcon = windowIcon;
                return this;
            }

            /// <summary>设置消息选项(这个方法未实现, 设置了也无用!)</summary>
            /// <param name="options"><see cref="T:System.Windows.MessageBoxOptions" /> 消息选项, 例: <br />
            ///     <see cref="F:System.Windows.MessageBoxOptions.None">MessageBoxOptions.None</see> (默认) <br />
            ///     <see cref="F:System.Windows.MessageBoxOptions.ServiceNotification">MessageBoxOptions.ServiceNotification</see> <br />
            ///     <see cref="F:System.Windows.MessageBoxOptions.DefaultDesktopOnly">MessageBoxOptions.DefaultDesktopOnly</see> <br />
            ///     <see cref="F:System.Windows.MessageBoxOptions.RightAlign">MessageBoxOptions.RightAlign</see> <br />
            ///     <see cref="F:System.Windows.MessageBoxOptions.RtlReading">MessageBoxOptions.RtlReading</see>
            /// </param>
            /// <returns></returns>
            [System.Obsolete(message: "方法未实现(Method not implemented)", error: false)]
            public Builder SetOptions(MessageBoxOptions options) {
                // this._options = options;
                return this;
            }

            /// <summary>给"Ok"按钮设置自定义文本</summary>
            /// <param name="text"></param>
            /// <returns></returns>
            public Builder SetOkText(string text) { this._okText = text; return this; }

            /// <summary>给"Cancel"按钮设置自定义文本</summary>
            /// <param name="text"></param>
            /// <returns></returns>
            public Builder SetCancelText(string text) { this._cancelText = text; return this; }

            /// <summary>给"Yes"按钮设置自定义文本</summary>
            /// <param name="text"></param>
            /// <returns></returns>
            public Builder SetYesText(string text) { this._yesText = text; return this; }

            /// <summary>给"No"按钮设置自定义文本</summary>
            /// <param name="text"></param>
            /// <returns></returns>
            public Builder SetNoText(string text) { this._noText = text; return this; }

            /// <summary>创建MessageBox2对象</summary>
            /// <returns></returns>
            public MessageBox2 Build() {
                return new MessageBox2(this);
            }


            // ---------- 显式实现接口（全部私有，外部不可见） ----------
            Window IMessageBoxBuilder.Owner => _owner;
            string IMessageBoxBuilder.Title => _title;
            string IMessageBoxBuilder.Message => _message;
            MessageBoxButton IMessageBoxBuilder.Button => _button;
            bool IMessageBoxBuilder.HasButtons => _hasButtons;
            double IMessageBoxBuilder.ButtonMinWidth => _buttonMinWidth;
            MessageBoxImage IMessageBoxBuilder.StandardIcon => _standardIcon;
            ImageSource IMessageBoxBuilder.CustomIcon => _customIcon;
            bool IMessageBoxBuilder.HasWindowIcon => _hasWindowIcon;
            MessageBoxImage IMessageBoxBuilder.WindowIcon => _windowIcon;
            ImageSource IMessageBoxBuilder.CustomWindowIcon => _customWindowIcon;
            MessageBoxResult IMessageBoxBuilder.DefaultResult => _defaultResult;
            string IMessageBoxBuilder.OkText => _okText;
            string IMessageBoxBuilder.CancelText => _cancelText;
            string IMessageBoxBuilder.YesText => _yesText;
            string IMessageBoxBuilder.NoText => _noText;
    }
}