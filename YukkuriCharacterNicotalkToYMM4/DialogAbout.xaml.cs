using Serilog;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// コンテンツ ダイアログの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace YukkuriCharacterNicotalkToYMM4
{
	public sealed partial class DialogAbout: ContentDialog
	{
		private ThemeController ThemeController;

		public DialogAbout(ElementTheme theme)
		{
			Log.Information("DialogAbout生成");
			this.InitializeComponent( );
			this.ThemeController = new ThemeController(theme, this);
		}

		private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			Application.Current.Exit( );
		}

		private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{

		}

	}
}
