using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace YukkuriCharacterNicotalkToYMM4
{
	/// <summary>メインページ。</summary>
	public sealed partial class MainPage: Page
	{
		private ThemeController ThemeController;

		public MainPage( )
		{
			this.InitializeComponent( );
			this.ThemeController = new ThemeController(this);
		}

		/// <summary>コマンドバーのコピーを押したとき。</summary>
		private void CommandCopyClick(object sender, RoutedEventArgs e)
		{
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
				return true;
			}
			return false;
		}

		/// <summary>コマンドバーの貼り付けを押したとき。</summary>
		private async void CommandPasteClick(object sender, RoutedEventArgs e)
		{
			DataPackageView dataPackageView = Clipboard.GetContent( );
			Task<bool> textBoxInput = new Task<bool>( ( ) => false);
			if (dataPackageView.Contains(StandardDataFormats.Text) && await this.PasteFromClipbord(dataPackageView, this.TextBoxInput))
			{
				return;
			}
			if (dataPackageView.Contains(StandardDataFormats.Text))
			{
				await this.PasteFromClipbord(dataPackageView, this.TextBoxOutput);
			}
		}

		/// <summary>クリップボードから貼り付けます。</summary>
		/// <param name="dataPackageView">クリップボードのDataPackageViewインスタンス</param>
		/// <param name="textBox">どのテキストボックスに貼り付けるか</param>
		/// <returns>引数のテキストボックスに貼り付けたか（＝フォーカスがあったか）</returns>
		private async Task<bool> PasteFromClipbord(DataPackageView dataPackageView, TextBox textBox)
		{
			if (textBox.FocusState == FocusState.Pointer)
			{
				textBox.Text = await dataPackageView.GetTextAsync( );
				return true;
			}
			return false;
		}

		/// <summary>コマンドバーの配色切替えを押したとき。</summary>
		private void CommandThemeClick(object sender, RoutedEventArgs e)
		{
			this.ThemeController.ChangeTheme(this);
		}

		/// <summary>コマンドバーのについてを押したとき。</summary>
		private async void CommandAboutClick(object sender, RoutedEventArgs e)
		{
			DialogAbout dialogAbout = new DialogAbout(this.ThemeController.NowTheme);
			await dialogAbout.ShowAsync( );
		}

		/// <summary>変換元の選択ボタンを押したとき。</summary>
		private async void ButtonInputClick(object sender, RoutedEventArgs e)
		{
			string path = await this.SelectFolder( );
			if (path != "")
			{
				this.TextBoxInput.Text = path;
			}
		}

		/// <summary>出力先の選択ボタンを押したとき。</summary>
		private async void ButtonOutputClick(object sender, RoutedEventArgs e)
		{
			string path = await this.SelectFolder( );
			if (path != "")
			{
				this.TextBoxOutput.Text = path;
			}
		}

		/// <summary>フォルダーピッカーを表示します。不正な場合，メッセージダイアログを表示します。</summary>
		/// <returns>フォルダーのパス。不正な場合，空文字列。</returns>
		private async Task<string> SelectFolder( )
		{
			Windows.Storage.Pickers.FolderPicker folderPicker = new Windows.Storage.Pickers.FolderPicker( );
			folderPicker.FileTypeFilter.Add("*");
			Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync( );
			if (folder == null)
			{
				DialogError dialogError = new DialogError(this.ThemeController.NowTheme, "フォルダーが見つかりませんでした。\r\n正しいフォルダーを選択してください。");
				await dialogError.ShowAsync( );
				return "";
			}
			Windows.Storage.AccessCache.StorageApplicationPermissions.
				FutureAccessList.AddOrReplace("PickedFolderToken", folder);
			return folder.Path;
		}

		/// <summary>変換ボタンを押したとき。</summary>
		private void ButtonRunClick(object sender, RoutedEventArgs e)
		{

		}

	}
}
