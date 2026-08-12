# CustomMessageBox
一个可定制的 WPF MessageBox 控件。(A custom MessageBox)
> [Github](https://github.com/actor20170211030627/CustomMessageBox)

[//]: # (由于NuGet找不到Github的相对图片, 所以这儿写死图片地址)
## 1.Screenshot
![Loading...](https://raw.githubusercontent.com/actor20170211030627/CustomMessageBox/main/captures/Snipaste_2026-08-13_02-23-59.png)
![Loading...](https://raw.githubusercontent.com/actor20170211030627/CustomMessageBox/main/captures/Snipaste_2026-08-10_14-50-55.png)

## 2.Sample
[Download Demo](https://github.com/actor20170211030627/CustomMessageBox/releases/latest/download/CustomMessageBoxDemo.exe)

## 3.How to
To get a Git project into your project:

在 NuGet 中搜索 `CustomMessageBox.Actor.WPF` 安装即可。(Search `CustomMessageBox.Actor.WPF` in NuGet)

## 4.Usage
**Step 1.** NewBuilder

    Builder builder = MessageBox2.NewBuilder(Window, message);
	Builder builder = MessageBox2.NewBuilder(message);	//or like this


**Step 2.** 设置属性, 非必须 (Set attribute, optional)

	//设置弹窗↖角图标 (Set window'↖ icon)
	builder.SetHasWindowIcon(bool)
	    .SetWindowIcon(MessageBoxImage)
	    .SetWindowIcon(ImageSource)

	    //设置标题 (Set title/caption)
	    .SetCaption(title)
        .SetTitle(title)	//和上面一样 (the same with ↑ line)

	    //设置关闭按钮❌️ (Set close button)
	    .SetEnableCloseBtn(bool)

	    //设置按下Esc时是否关闭弹窗 (Set closeable on pressed Esc)
	    .SetCloseOnPressedEsc(bool)

	    //设置Icon (Set icon)
        .SetIcon(MessageBoxImage)
        .SetIcon(ImageSource)

	    //设置Button (Set button)
	    .SetHasButtons(bool)
	    .SetButton(MessageBoxButton)
	    .SetButtonMinWidth(double)

	    //设置按钮文字 & 按下后是否关闭弹窗 (Set button text & window closeable when btn pressed)
	    .SetOkText(content)
	    .SetCloseOnClickOk(bool)
	    .SetCancelText(content)
	    .SetCloseOnClickCancel(bool)
	    .SetYesText(content)
	    .SetCloseOnClickYes(bool)
	    .SetNoText(content)
	    .SetCloseOnClickNo(bool)

	    //设置按下Enter时的默认值 (Set default result when pressed Enter and window close)
	    .SetDefaultResult(MessageBoxResult)

		//别调用, 还没实现 (Haven't support)
		.SetOptions(MessageBoxOptions)
	    ;

**Step 3.** 显示: Show() or ShowDialog()

	MessageBox2 messageBox2 = builder.Build();

	//同步方法, 弹框关闭后这句代码后面的代码才开始执行, 直接返回结果 (asynchronous method, direct return value)‌
	MessageBoxResult result = messageBox2.ShowDialog();
	Console.WriteLine($"result = {result}");
	following code...								//后面的代码


	//异步方法, 调用完Show()后, 后面的代码接着执行, 异步返回结果 (asynchronous method, callback to return value)
	messageBox2.Show(onBtnClick: result => {
	    Console.WriteLine($"result = {result}");
	}, modal: true);
	following code...								//后面的代码
