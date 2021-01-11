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
	/// <summary>
	/// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
	/// </summary>
	public sealed partial class MainPage: Page
	{
		public MainPage( )
		{
			this.InitializeComponent( );
		}

		private void CommandCopyClick(object sender, RoutedEventArgs e)
		{
			if (this.CopyToClipbord(TextBoxInput))
			{
				return;
			}
			this.CopyToClipbord(TextBoxOutput);
		}

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

		private async void CommandPasteClick(object sender, RoutedEventArgs e)
		{
			DataPackageView dataPackageView = Clipboard.GetContent( );
			Task<bool> textBoxInput = new Task<bool>( ( ) => false);
			if (dataPackageView.Contains(StandardDataFormats.Text) && this.PasteFromClipbord(dataPackageView, TextBoxInput).Result)
			{
				return;
			}
			if (dataPackageView.Contains(StandardDataFormats.Text))
			{
				await this.PasteFromClipbord(dataPackageView, TextBoxOutput);
			}
		}

		private async Task<bool> PasteFromClipbord(DataPackageView dataPackageView, TextBox textBox)
		{
			if (textBox.FocusState == FocusState.Pointer)
			{
				textBox.Text = await dataPackageView.GetTextAsync( );
				return true;
			}
			return false;
		}

		private async void CommandAboutClick(object sender, RoutedEventArgs e)
		{
			DialogAbout dialogAbout = new DialogAbout( );
			await dialogAbout.ShowAsync( );
		}

		private void ButtonInputClick(object sender, RoutedEventArgs e)
		{

		}

		private void ButtonOutputClick(object sender, RoutedEventArgs e)
		{

		}

		private void ButtonRunClick(object sender, RoutedEventArgs e)
		{

		}

		
	}
}
