using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CustomMessageBox.Resources;

namespace CustomMessageBox.Dialog {

    /// <summary>
    /// 自定义消息框，支持 Builder 模式，使用标准 MessageBoxButton, MessageBoxImage 和 MessageBoxResult
    /// </summary>
    public partial class MessageBox2 : Window {

        private MessageBoxResult _result = MessageBoxResult.None;
        private Window _owner;
        // 状态跟踪
        private bool _defaultSet, _cancelSet;
        // 暂存当前被设为取消的按钮，用于撤销
        private Button _cancelCandidate;
        
        internal MessageBox2(Builder builder) {
            InitializeComponent();
            // 不在任务栏显示
            this.ShowInTaskbar = false;
            
            if (!(builder is IMessageBoxBuilder data)) return;
            this._owner = data.Owner;

            //设置Window↖️角的icon
            if (data.WindowIcon == MessageBoxImage.None && data.CustomWindowIcon == null) {
                // 使用工具窗口样式（无图标占位，更紧凑）
                this.WindowStyle = WindowStyle.ToolWindow;
            } else {
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                if (data.CustomWindowIcon != null) {
                    this.Icon = data.CustomWindowIcon;
                } else this.Icon = GetStandardIconImageSource(data.WindowIcon);
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


        /// <summary>显示</summary>
        /// <returns>返回被点击的按钮, 例: <br />
        ///     <see cref="F:System.Windows.MessageBoxResult.None">MessageBoxResult.None</see> <br />
        ///     <see cref="F:System.Windows.MessageBoxResult.OK">MessageBoxResult.OK</see> <br />
        ///     <see cref="F:System.Windows.MessageBoxResult.Cancel">MessageBoxResult.Cancel</see> <br />
        ///     <see cref="F:System.Windows.MessageBoxResult.Yes">MessageBoxResult.Yes</see> <br />
        ///     <see cref="F:System.Windows.MessageBoxResult.No">MessageBoxResult.No</see>
        /// </returns>
        public new MessageBoxResult Show() {
            // base.Show();
            Window targetOwner = this._owner;
            if (targetOwner == null || !targetOwner.IsVisible) {
                var main = Application.Current.MainWindow;
                if (main != null && main.IsVisible) {
                    targetOwner = main;
                } else targetOwner = null;
            }
            this.Owner = targetOwner;
            this._owner = null;
            bool? showDialog = base.ShowDialog();
            // return MessageBox.Win32ToMessageBoxResult(...);
            return _result;
        }

        /// <summary>
        /// 内置标准图标生成（纯 WPF 矢量路径，无 WinForms）
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private ImageSource GetStandardIconImageSource(MessageBoxImage image) {
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
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                sysIcon.Handle,
                new Int32Rect(0, 0, sysIcon.Width, sysIcon.Height),
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
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
            string okText = string.IsNullOrEmpty(builder.OkText) ? Strings.OK : builder.OkText;
            string cancelText = string.IsNullOrEmpty(builder.CancelText) ? Strings.Cancel : builder.CancelText;
            string yesText = string.IsNullOrEmpty(builder.YesText) ? Strings.Yes : builder.YesText;
            string noText = string.IsNullOrEmpty(builder.NoText) ? Strings.No : builder.NoText;
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
            // 如果因为某种原因 defaultSet 仍为 false（例如没有按钮或 defaultResult 不匹配），强制设置第一个为默认
            if (!_defaultSet && this.StackPanel_Buttons.Children.Count > 0) {
                Button firstElement = (Button)this.StackPanel_Buttons.Children[0];
                firstElement.IsDefault = true;
                SetElementFocus(firstElement);
            }
            _cancelCandidate = null;
        }
        
        private void AddButton(IMessageBoxBuilder builder, string content, MessageBoxResult btnResult) {
            var btn = new Button {
                Content = content,
                MinWidth = builder.ButtonMinWidth,
                // Width = 75,
                Height = 26, // 保留固定高度（可改为统一外观）
                Padding = new Thickness(8, 4, 8, 4),// 内边距让文字不贴边
                Margin = new Thickness(8, 0, 0, 0),
                // IsDefault = isDefault,
                // IsCancel = isCancel,
                Tag = btnResult
            };
            // 设置默认按钮：优先匹配 defaultResult，否则第一个按钮为默认
            if (!_defaultSet) {
                if (builder.DefaultResult != MessageBoxResult.None && btnResult == builder.DefaultResult) {
                    btn.IsDefault = true;
                    _defaultSet = true;
                    SetElementFocus(btn);
                } else if (builder.DefaultResult == MessageBoxResult.None && this.StackPanel_Buttons.Children.Count == 0) {
                    // 如果没有指定默认结果，则第一个按钮为默认
                    btn.IsDefault = true;
                    _defaultSet = true;
                    SetElementFocus(btn);
                }
            }
            // 取消按钮逻辑：优先 Cancel，其次 No，且只保留最优先的一个
            if (btnResult == MessageBoxResult.Cancel) {
                // 如果之前有候选取消按钮（可能是No），撤销它的 IsCancel
                if (_cancelCandidate != null) _cancelCandidate.IsCancel = false;
                btn.IsCancel = true;
                _cancelCandidate = btn;
                _cancelSet = true;
            } else if (btnResult == MessageBoxResult.No && !_cancelSet) {
                // 只有当尚未设置任何取消按钮时，才将 No 设为取消
                btn.IsCancel = true;
                _cancelCandidate = btn;
                _cancelSet = true;
            }
            btn.Click += (s, e) => {
                this._result = (MessageBoxResult)((Button)s).Tag;
                this.Close();
            };
            this.StackPanel_Buttons.Children.Add(btn);
        }

        /// <summary>
        /// 让焦点落到该按钮上，否则初始焦点可能在整个窗口上，按 Enter 虽然会触发默认按钮（因为 WPF 会处理），但用户看不到蓝色高亮
        /// 延迟设置焦点，确保窗口已加载
        /// Dispatcher.BeginInvoke 将焦点设置操作放入消息队列，在窗口布局完成、所有 Loaded 事件触发之后执行，此时视觉树已经完整，焦点能够正确设置。
        /// Background 优先级确保它不会阻塞 UI 渲染，几乎瞬间完成。
        /// </summary>
        /// <param name="element"></param>
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

        // 处理点右上角 X 关闭
        protected override void OnClosing(CancelEventArgs e) {
            // result 保持 None
            base.OnClosing(e);
        }
    }
}