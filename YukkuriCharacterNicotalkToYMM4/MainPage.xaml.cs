using Serilog;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace YukkuriCharacterNicotalkToYMM4
{
	/// <summary>メインページ。</summary>
	public sealed partial class MainPage: Page
	{
		private CoreCursor CursorDefault = Window.Current.CoreWindow.PointerCursor;
		private CoreCursor CursorRunning = new CoreCursor(CoreCursorType.Wait, 1);
		private ThemeController ThemeController;
		private Models.FileIO FileIO = new Models.FileIO( );
		private StorageFolder DirectoryInput;
		private StorageFolder DirectoryOutput;

		public MainPage( )
		{
			Log.Information("MainPage生成");
			this.InitializeComponent( );
			this.ThemeController = new ThemeController(this);
		}

		/// <summary>コマンドバーのコピーを押したとき。</summary>
		private void CommandCopyClick(object sender, RoutedEventArgs e)
		{
			Log.Information("CommandCopyClick");
			if (this.CopyToClipbord(this.TextBoxInput))
			{
				return;
			}
			this.CopyToClipbord(this.TextBoxOutput);
		}

		/// <summary>クリップボードにコピーします。</summary>
		/// <param name="textBox">どのテキストボックスの内容をコピーするか。</param>
		/// <returns>引数のテキストボックスからコピーできたか（＝フォーカスがあったか）</returns>
		private bool CopyToClipbord(TextBox textBox)
		{
			if (textBox.FocusState == FocusState.Pointer)
			{
				DataPackage dataPackage = new DataPackage( );
				dataPackage.SetText(textBox.Text);
				Clipboard.SetContent(dataPackage);
				Log.Debug("クリップボードコピー：" + textBox.Text);
				return true;
			}
			return false;
		}

		///// <summary>コマンドバーの貼り付けを押したとき。</summary>
		//private async void CommandPasteClick(object sender, RoutedEventArgs e)
		//{
		//	DataPackageView dataPackageView = Clipboard.GetContent( );
		//	Task<bool> textBoxInput = new Task<bool>( ( ) => false);
		//	if (dataPackageView.Contains(StandardDataFormats.Text) && await this.PasteFromClipbord(dataPackageView, this.TextBoxInput))
		//	{
		//		return;
		//	}
		//	if (dataPackageView.Contains(StandardDataFormats.Text))
		//	{
		//		await this.PasteFromClipbord(dataPackageView, this.TextBoxOutput);
		//	}
		//}

		///// <summary>クリップボードから貼り付けます。</summary>
		///// <param name="dataPackageView">クリップボードのDataPackageViewインスタンス</param>
		///// <param name="textBox">どのテキストボックスに貼り付けるか</param>
		///// <returns>引数のテキストボックスに貼り付けたか（＝フォーカスがあったか）</returns>
		//private async Task<bool> PasteFromClipbord(DataPackageView dataPackageView, TextBox textBox)
		//{
		//	if (textBox.FocusState == FocusState.Pointer)
		//	{
		//		textBox.Text = await dataPackageView.GetTextAsync( );
		//		return true;
		//	}
		//	return false;
		//}

		/// <summary>コマンドバーの配色切替えを押したとき。</summary>
		private void CommandThemeClick(object sender, RoutedEventArgs e)
		{
			Log.Information("CommandThemeClick");
			this.ThemeController.ChangeTheme(this);
		}

		/// <summary>コマンドバーのについてを押したとき。</summary>
		private async void CommandAboutClick(object sender, RoutedEventArgs e)
		{
			Log.Information("CommandAboutClick");
			DialogAbout dialogAbout = new DialogAbout(this.ThemeController.NowTheme);
			await dialogAbout.ShowAsync( );
		}

		/// <summary>変換元の選択ボタンを押したとき。</summary>
		private async void ButtonInputClick(object sender, RoutedEventArgs e)
		{
			this.ButtonInput.IsEnabled = false;
			Log.Information("ButtonInputClick");
			StorageFolder tempDirectory = await this.SelectFolder(Models.EDirectoryType.Input);
			if (tempDirectory != null)
			{
				this.DirectoryInput = tempDirectory;
				this.TextBoxInput.Text = this.DirectoryInput.Path;
			}
			this.ButtonInput.IsEnabled = true;
		}

		/// <summary>出力先の選択ボタンを押したとき。</summary>
		private async void ButtonOutputClick(object sender, RoutedEventArgs e)
		{
			this.ButtonOutput.IsEnabled = false;
			Log.Information("ButtonOutputClick");
			StorageFolder tempDirectory = await this.SelectFolder(Models.EDirectoryType.Output);
			if (tempDirectory != null)
			{
				this.DirectoryOutput = tempDirectory;
				this.TextBoxOutput.Text = this.DirectoryOutput.Path;
			}
			this.ButtonOutput.IsEnabled = true;
		}

		private async Task<StorageFolder> SelectFolder(Models.EDirectoryType directoryType)
		{
			StorageFolder folder;
			try
			{
				folder = await this.FileIO.SelectFolder(directoryType);
			}
			catch (ArgumentException)
			{
				folder = null;
			}
			if (folder == null)
			{
				Log.Warning("フォルダーが見つかりません。正しいフォルダーを選択してください。");
				DialogError dialogError = new DialogError(this.ThemeController.NowTheme, "フォルダーが見つかりません。\r\n\r\n正しいフォルダーを選択してください。");
				await dialogError.ShowAsync( );
			}
			return folder;
		}

		/// <summary>変換ボタンを押したとき。</summary>
		private async void ButtonRunClick(object sender, RoutedEventArgs e)
		{
			this.ButtonRun.IsEnabled = false;
			Log.Information("ButtonRunClick");
			Window.Current.CoreWindow.PointerCursor = this.CursorRunning;
			Models.MeConverter meConverter = new Models.MeConverter( );
			Models.MouthConverter mouthConverter = new Models.MouthConverter( );
			Models.OtherConverters otherConverters = new Models.OtherConverters( );
			try
			{
				this.CheckDirectory( );
				meConverter.CheckDirectory(this.DirectoryInput);
				await this.FileIO.CopyDirectory(this.DirectoryInput, this.DirectoryOutput);
				await Task.WhenAll(meConverter.Convert(this.DirectoryInput, this.DirectoryOutput), mouthConverter.Convert(this.DirectoryInput, this.DirectoryOutput), otherConverters.Convert(this.DirectoryInput, this.DirectoryOutput));
			}
			catch (Exception exception) when (exception is ArgumentException || exception is NullReferenceException)
			{
				Log.Warning(exception.Message.Replace("\r\n", " "));
				Window.Current.CoreWindow.PointerCursor = this.CursorDefault;
				DialogError dialogError = new DialogError(this.ThemeController.NowTheme, exception.Message);
				await dialogError.ShowAsync( );
				this.ButtonRun.IsEnabled = true;
				return;
			}
			catch (Exception exception)
			{
				Log.Warning("想定外のエラー：" + exception.ToString( ));
				Window.Current.CoreWindow.PointerCursor = this.CursorDefault;
				DialogError dialogError = new DialogError(this.ThemeController.NowTheme, "想定外のエラーが発生しました。開発者に↓を伝えると修正してくれるかもしれません。\r\n\r\n" + exception.ToString( ));
				await dialogError.ShowAsync( );
				this.ButtonRun.IsEnabled = true;
				return;
			}

			this.Finish( );
			this.ButtonRun.IsEnabled = true;
		}

		private void CheckDirectory( )
		{
			if (this.DirectoryInput == null)
			{
				throw new NullReferenceException("変換元ディレクトリーがありません。\r\n\r\nフォルダーを選択してください。");
			}
			if (this.DirectoryOutput == null)
			{
				throw new NullReferenceException("出力先ディレクトリーがありません。\r\n\r\nフォルダーを選択してください。");
			}
		}

		private async void Finish( )
		{
			Window.Current.CoreWindow.PointerCursor = this.CursorDefault;
			Log.Information("変換完了しました。");
			DialogFinish dialogFinish = new DialogFinish(this.ThemeController.NowTheme);
			await dialogFinish.ShowAsync( );
		}

	}
}
