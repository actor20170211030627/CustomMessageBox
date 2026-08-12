using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Actor.CustomMessageBox.Resources;

namespace Actor.CustomMessageBox {

    /// <summary>
    /// 自定义消息框，支持 Builder 模式设置n个属性，使用标准 MessageBoxImage, MessageBoxButton 和 MessageBoxResult
    /// </summary>
    public partial class MessageBox2 : Window {

        private const int MfByCommand = 0x0000, ScClose = 0xF060, MfGrayed = 0x0001, WmSysCommand = 0x0112;
        private MessageBoxResult _result = MessageBoxResult.None;
        private Window _owner;
        private readonly bool _enableCloseBtn, _closeOnClickOk, _closeOnClickCancel, _closeOnClickYes, _closeOnClickNo;
        private Action<MessageBoxResult> _onBtnClick;

        internal MessageBox2(Builder builder) {
            InitializeComponent();
            // 不在任务栏显示
            this.ShowInTaskbar = false;
            
            if (!(builder is IMessageBoxBuilder data)) return;
            this._owner = data.Owner;
            this._enableCloseBtn = data.EnableCloseBtn;
            this._closeOnClickOk = data.CloseOnClickOk;
            this._closeOnClickCancel = data.CloseOnClickCancel;
            this._closeOnClickYes = data.CloseOnClickYes;
            this._closeOnClickNo = data.CloseOnClickNo;

            //设置Window↖️角的icon
            if (data.HasWindowIcon) {
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                if (data.CustomWindowIcon != null) {
                    this.Icon = data.CustomWindowIcon;
                } else if (data.WindowIcon != MessageBoxImage.None) {
                    this.Icon = GetStandardIconImageSource(data.WindowIcon);
                }
            } else {
                // 使用工具窗口样式（无图标占位，更紧凑）
                this.WindowStyle = WindowStyle.ToolWindow;
            }

            //Title不能传null, 否则报错: System.ArgumentException: “”不是属性“Title”的有效值。
            this.Title = data.Title ?? "";
            this.TB_Message.Text = data.Message;

            // 设置图标
            if (data.CustomIcon != null) {
                this.Image_Icon.Source = data.CustomIcon;
                this.Image_Icon.Visibility = Visibility.Visible;
            } else if (data.StandardIcon != MessageBoxImage.None) {
                this.Image_Icon.Source = GetStandardIconImageSource(data.StandardIcon);
                this.Image_Icon.Visibility = Visibility.Visible;
            } else {
                this.Image_Icon.Visibility = Visibility.Collapsed;
                // 无图标时文本占满
                this.TB_Message.Margin = new Thickness(0);
            }

            // 创建按钮
            CreateButtons(data);

            //Esc按钮按下的时候, if给按钮设置了IsCancel, 则: Show()默认不会消失, ShowDialog()默认会消失
            //而为了不让ShowDialog()的时候被强制消失, 则按钮都没有设置IsCancel, 乺这儿需自己接管
            this.PreviewKeyDown += (s, e) => {
                if (e.Key == System.Windows.Input.Key.Escape) {
                    e.Handled = !data.CloseOnPressedEsc;
                    //赋值, 用于ShowDialog()方式的返回
                    this._result = MessageBoxResult.Cancel;
                    if (data.CloseOnPressedEsc) Close();
                    _onBtnClick?.Invoke(this._result);
                }
            };
        }

        /// <summary>确保窗口句柄有效</summary>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);
            if (!_enableCloseBtn) RemoveCloseButton();
            //捕获点击 ❌️，且不影响 Esc 等其他关闭方式
            var helper = new System.Windows.Interop.WindowInteropHelper(this);
            System.Windows.Interop.HwndSource.FromHwnd(helper.Handle)?.AddHook(hook: (IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) => {
                if (msg == WmSysCommand && (wParam.ToInt32() & 0xFFF0) == ScClose) {
                    handled = !this._enableCloseBtn;
                    this._result = MessageBoxResult.None;
                    _onBtnClick?.Invoke(this._result);
                }
                return IntPtr.Zero;
            });
        }

        /// <summary>创建Builder</summary>
        /// <param name="message">提示内容</param>
        /// <returns></returns>
        public static Builder NewBuilder(string message) => new Builder(message);
        
        /// <summary>创建Builder</summary>
        /// <param name="owner">不要传null</param>
        /// <param name="message">提示内容</param>
        /// <returns></returns>
        public static Builder NewBuilder(Window owner, string message) => new Builder(owner, message);

        private Window GetTargetOwner() {
            if (_owner != null && _owner.IsVisible) return _owner;
            var main = Application.Current.MainWindow;
            return (main != null && main.IsVisible) ? main : null;
        }

        /// <summary>显示(异步方法, 不阻塞线程)</summary>
        /// <param name="onBtnClick">当按钮点击 /Esc按下 后回调结果</param>
        /// <param name="modal">
        ///     是否模态, if modal = true阻止点击父窗口, 默认true <br />
        ///     无法让其他非父窗口被点击，也无法产生闪烁效果——只有 ShowDialog 拥有系统级模态和闪烁。<br />
        ///     而 ShowDialog() 本身无法非模态。
        /// </param>
        public void Show(Action<MessageBoxResult> onBtnClick, bool modal = true) {
            this._onBtnClick = onBtnClick;
            Window targetOwner = GetTargetOwner();
            this.Owner = targetOwner;
            this._owner = null;
            if (modal && targetOwner != null) {
                targetOwner.IsEnabled = false;
                this.Closed += (s, e) => targetOwner.IsEnabled = true;
            }
            base.Show();
        }

        /// <summary>不要调用这个方法</summary>
        [Obsolete(message: "不要调用这个方法(Don't call me)", error: true)]
        public new void Show() {
            base.Show();
        }

        /// <summary>显示(同步方法, 阻塞线程有返回值)</summary>
        /// <returns>返回被点击的按钮, 例: <br />
        ///     <see cref="F:System.Windows.MessageBoxResult.None">MessageBoxResult.None</see> <br />
        ///     <see cref="F:System.Windows.MessageBoxResult.OK">MessageBoxResult.OK</see> <br />
        ///     <see cref="F:System.Windows.MessageBoxResult.Cancel">MessageBoxResult.Cancel</see> <br />
        ///     <see cref="F:System.Windows.MessageBoxResult.Yes">MessageBoxResult.Yes</see> <br />
        ///     <see cref="F:System.Windows.MessageBoxResult.No">MessageBoxResult.No</see>
        /// </returns>
        public new MessageBoxResult ShowDialog() {
            this.Owner = GetTargetOwner();
            this._owner = null;
            //ShowDialog阻塞线程, 等CLose()后返回 _result
            bool? showDialog = base.ShowDialog();
            // return MessageBox.Win32ToMessageBoxResult(...);
            return _result;
        }

        /// <summary>MessageBoxImage 转换成 ImageSource</summary>
        private static ImageSource GetStandardIconImageSource(MessageBoxImage image) {
            System.Drawing.Icon sysIcon;
            switch (image) {
                case MessageBoxImage.Information:
                    sysIcon = System.Drawing.SystemIcons.Information;
                    break;
                case MessageBoxImage.Warning:
                    sysIcon = System.Drawing.SystemIcons.Warning;
                    break;
                case MessageBoxImage.Error:
                    sysIcon = System.Drawing.SystemIcons.Error;
                    break;
                case MessageBoxImage.Question:
                    sysIcon = System.Drawing.SystemIcons.Question;
                    break;
                case MessageBoxImage.None:
                default:
                    return null;
            }
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(sysIcon.Handle, new Int32Rect(0, 0, sysIcon.Width, sysIcon.Height), System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }

        // ---------- 动态生成按钮 ----------
        private void CreateButtons(IMessageBoxBuilder builder) {
            if (!builder.HasButtons) {
                this.StackPanel_Buttons.Visibility = Visibility.Collapsed;
                return;
            }
            this.StackPanel_Buttons.Visibility = Visibility.Visible;
            this.StackPanel_Buttons.Children.Clear();
            // 默认文本
            object okText = GetContent(builder.OkText, Strings.OK);
            object cancelText = GetContent(builder.CancelText, Strings.Cancel);
            object yesText = GetContent(builder.YesText, Strings.Yes);
            object noText = GetContent(builder.NoText, Strings.No);
            switch (builder.Button) {
                case MessageBoxButton.OK:
                    AddButton(builder, okText, MessageBoxResult.OK);
                    break;
                case MessageBoxButton.OKCancel:
                    AddButton(builder, okText, MessageBoxResult.OK);
                    AddButton(builder, cancelText, MessageBoxResult.Cancel);
                    break;
                case MessageBoxButton.YesNo:
                    AddButton(builder, yesText, MessageBoxResult.Yes);
                    AddButton(builder, noText, MessageBoxResult.No);
                    break;
                case MessageBoxButton.YesNoCancel:
                    AddButton(builder, yesText, MessageBoxResult.Yes);
                    AddButton(builder, noText, MessageBoxResult.No);
                    AddButton(builder, cancelText, MessageBoxResult.Cancel);
                    break;
                default:
                    // 默认至少一个 OK 按钮
                    AddButton(builder, okText, MessageBoxResult.OK);
                    break;
            }
        }

        private static object GetContent(object value, string defaultValue) {
            if (value == null || (value is string s && s.Length == 0)) return defaultValue;
            return value;
        }

        private void AddButton(IMessageBoxBuilder builder, object content, MessageBoxResult btnResult) {
            var btn = new Button {
                Content = content,
                MinWidth = builder.ButtonMinWidth,
                // Width = 75,
                Height = 26, // 保留固定高度（可改为统一外观）
                Padding = new Thickness(8, 4, 8, 4),// 内边距让文字不贴边
                Margin = new Thickness(8, 0, 0, 0),
                IsDefault = btnResult == builder.DefaultResult,
                // IsCancel = isCancel, //if=true: 点击这个按钮(或按下 Esc)时自动关闭窗口，即使 Click 事件里没有关闭也会自动关闭Window。
                Tag = btnResult
            };
            if (btn.IsDefault) SetElementFocus(btn);

            btn.Click += (s, e) => {
                this._result = (MessageBoxResult)((FrameworkElement)s).Tag;
                JudgeAndCloseWindow();
            };
            this.StackPanel_Buttons.Children.Add(btn);
        }

        /// <summary>
        /// 让焦点落到该按钮上，否则初始焦点可能在整个窗口上，按 Enter 虽然会触发默认按钮（因为 WPF 会处理），但用户看不到蓝色高亮
        /// 延迟设置焦点，确保窗口已加载
        /// Dispatcher.BeginInvoke 将焦点设置操作放入消息队列，在窗口布局完成、所有 Loaded 事件触发之后执行，此时视觉树已经完整，焦点能够正确设置。
        /// Background 优先级确保它不会阻塞 UI 渲染，几乎瞬间完成。
        /// </summary>
        private void SetElementFocus(FrameworkElement element) {
            // btn.Background = SystemColors.HighlightBrush;
            // btn.Foreground = SystemColors.HighlightTextBrush;

            // 确保焦点样式不为空
            // if (element.FocusVisualStyle == null) {
            //     // 从系统资源恢复默认
            //     element.FocusVisualStyle = (Style)FindResource(SystemParameters.FocusVisualStyleKey);
            // }
            // 使用 Dispatcher 延迟到布局和输入准备完成后执行（虽然虚线框可能不显示）, 按←/→ 能切换按钮的虚线
            this.Dispatcher.BeginInvoke(new Action(() => element.Focus()), System.Windows.Threading.DispatcherPriority.Input);
        }

        private void JudgeAndCloseWindow() {
            if (this._result == MessageBoxResult.OK && this._closeOnClickOk || this._result == MessageBoxResult.Cancel && this._closeOnClickCancel || this._result == MessageBoxResult.Yes && this._closeOnClickYes || this._result == MessageBoxResult.No && this._closeOnClickNo) Close();
            _onBtnClick?.Invoke(this._result);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        /// <summary>移除按钮<br />
        /// Win32 API 的原型是 BOOL RemoveMenu(...)，在 C/C++ 中 BOOL 本质是 int（通常 0 表示失败，非 0 表示成功）。<br />
        /// 在 .NET P/Invoke 中，我们可以直接映射为 int，也可以映射为 bool（配合 [MarshalAs(UnmanagedType.Bool)] 让运行时自动转换）。
        /// </summary>
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern /*int*/ bool RemoveMenu(IntPtr hMenu, int nPosition, int wFlags);

        // 备用方案：禁用而不是移除（使关闭按钮变灰）
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool EnableMenuItem(IntPtr hMenu, int uIdEnableItem, int uEnable);

        private bool RemoveCloseButton() {
            try {
                var hWnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                if (hWnd == IntPtr.Zero) return false; // 安全保护
                IntPtr hMenu = GetSystemMenu(hWnd, false);
                if (hMenu == IntPtr.Zero) return false;
                bool removeMenu = RemoveMenu(hMenu, ScClose, MfByCommand);
                if (removeMenu) return true;
                // 如果移除失败，可尝试通过 EnableMenuItem 禁用（使按钮变灰不可点）
                return EnableMenuItem(hMenu, ScClose, MfByCommand | MfGrayed);
            } catch (Exception ex) {
                // 可记录日志，但不要抛出异常以免影响界面显示
                System.Diagnostics.Debug.WriteLine($"RemoveCloseButton failed: {ex}");
                return false;
            }
        }
    }
}