using System;
using System.Windows;
using System.Windows.Controls;
using Actor.CustomMessageBox;

namespace CustomMessageBoxDemo {
    
    public partial class MainWindow {

        private readonly string _assemblyName;

        public MainWindow() {
            InitializeComponent();            
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            _assemblyName = assembly.GetName().Name;

            //自定义图标
            this.ComboBox_Custom_Icon.Visibility = Visibility.Hidden;
            this.ComboBox_Custom_WindowIcon.Visibility = Visibility.Hidden;

            this.ComboBox_Icon.SelectionChanged += (sender, args) => {
                var selectedIndex = this.ComboBox_Icon.SelectedIndex;
                //自定义图标
                this.ComboBox_Custom_Icon.Visibility = selectedIndex == 5 ? Visibility.Visible : Visibility.Hidden;
            };
            this.ComboBox_WindowIcon.SelectionChanged += (sender, args) => {
                var selectedIndex = this.ComboBox_WindowIcon.SelectedIndex;
                //自定义图标
                this.ComboBox_Custom_WindowIcon.Visibility = selectedIndex == 5 ? Visibility.Visible : Visibility.Hidden;
            };
        }
        
        private void OnBtnClick(object sender, RoutedEventArgs routedEventArgs) {
            if (!(sender is Button button)) return;
            string name = button.Name;
            
            if (name == this.Btn_Show.Name) {
                Builder builder = MessageBox2.NewBuilder(this.TB_Message.Text)
                    .SetCaption(this.TB_Title.Text)
                    // .SetTitle(this.TB_Title.Text)    //和上面一样的
                    ;

                //设置图标
                int imageIndex = this.ComboBox_Icon.SelectedIndex;
                if (imageIndex <= 4) {
                    builder.SetIcon(GetIcon(this.ComboBox_Icon.SelectedIndex));
                } else if (imageIndex == 5) {
                    Uri uri;
                    if (this.ComboBox_Custom_Icon.SelectedIndex == 0) {
                        uri = new Uri($"pack://application:,,,/{_assemblyName};component/Resources/Images/icon_switch_green2.png");
                    } else {
                        uri = new Uri($"pack://application:,,,/{_assemblyName};component/Resources/Images/icon_switch_lightyellow.png");
                    }
                    var icon = new System.Windows.Media.Imaging.BitmapImage(uri);
                    builder.SetIcon(icon);
                }
                
                //设置Window图标
                int imageIndexWindow = this.ComboBox_WindowIcon.SelectedIndex;
                if (imageIndexWindow <= 4) {
                    builder.SetWindowIcon(GetIcon(this.ComboBox_WindowIcon.SelectedIndex));
                } else if (imageIndexWindow == 5) {
                    Uri uri;
                    if (this.ComboBox_Custom_WindowIcon.SelectedIndex == 0) {
                        uri = new Uri($"pack://application:,,,/{_assemblyName};component/Resources/Images/icon_switch_green2.png");
                    } else {
                        uri = new Uri($"pack://application:,,,/{_assemblyName};component/Resources/Images/icon_switch_lightyellow.png");
                    }
                    var icon = new System.Windows.Media.Imaging.BitmapImage(uri);
                    builder.SetWindowIcon(icon);
                }
                
                //设置按钮
                MessageBoxResult result = builder
                    .SetHasButtons(this.ComboBox_Button.SelectedIndex < 4)
                    .SetButton(GetButton(this.ComboBox_Button.SelectedIndex))
                    .SetButtonMinWidth(GetButtonMinWidth(this.ComboBox_ButtonMinWidth.SelectedIndex))
                    //
                    .SetOptions(MessageBoxOptions.DefaultDesktopOnly)
                    //设置按钮文字
                    .SetOkText(this.TB_OkText.Text)
                    .SetCancelText(this.TB_CancelText.Text)
                    .SetYesText(this.TB_YesText.Text)
                    .SetNoText(this.TB_NoText.Text)
                    .SetDefaultResult(GetDefaultResult(this.ComboBox_DefaultResult.SelectedIndex))
                    .Build()
                    .Show();

                this.TB_Result.Text = $"result = {result}";
            }
        }

        private MessageBoxImage GetIcon(int index) {
            switch (index) {
                case 0: return MessageBoxImage.None;
                case 1: return MessageBoxImage.Error;
                case 2: return MessageBoxImage.Question;
                case 3: return MessageBoxImage.Warning;
                case 4: return MessageBoxImage.Information;
                default: return MessageBoxImage.None;
            }
        }

        private MessageBoxButton GetButton(int index) {
            switch (index) {
                case 0: return MessageBoxButton.OK;
                case 1: return MessageBoxButton.OKCancel;
                case 2: return MessageBoxButton.YesNo;
                case 3: return MessageBoxButton.YesNoCancel;
                default: return MessageBoxButton.OK;
            }
        }

        private double GetButtonMinWidth(int index) {
            switch (index) {
                case 0: return 75D;
                case 1: return 85D;
                case 2: return 95D;
                case 3: return 105D;
                default: return 75D;
            }
        }

        private MessageBoxResult GetDefaultResult(int index) {
            switch (index) {
                case 0: return MessageBoxResult.OK;
                case 1: return MessageBoxResult.None;
                case 2: return MessageBoxResult.Cancel;
                case 3: return MessageBoxResult.Yes;
                case 4: return MessageBoxResult.No;
                default: return MessageBoxResult.OK;
            }
        }
    }
}