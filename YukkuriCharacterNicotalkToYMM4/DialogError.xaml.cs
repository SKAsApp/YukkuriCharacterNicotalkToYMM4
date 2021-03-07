using Serilog;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// コンテンツ ダイアログの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace YukkuriCharacterNicotalkToYMM4
{
	public sealed partial class DialogError: ContentDialog
	{
		private ThemeController ThemeController;

		public DialogError(ElementTheme theme, string errorMessage)
		{
			Log.Information("DialogError生成");
			this.InitializeComponent( );
			this.ThemeController = new ThemeController(theme, this);
			this.ErrorMessage.Text = errorMessage;
		}

	}
}
