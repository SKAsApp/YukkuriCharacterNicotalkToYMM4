using Serilog;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace YukkuriCharacterNicotalkToYMM4
{
	/// <summary>ライトテーマ・ダークテーマの切替えの管理者。</summary>
	public class ThemeController
	{
		private ApplicationTheme OSTheme = App.Current.RequestedTheme;
		public ElementTheme NowTheme { get; private set; }

		/// <summary>テーマ管理者を初期化し，controlをthemeにします。</summary>
		/// <param name="theme">初期テーマ</param>
		/// <param name="control">初期テーマを適用するUIコントロール</param>
		public ThemeController(ElementTheme theme, Control control)
		{
			Log.Debug("ThemeController生成：theme：" + theme.ToString( ) + "，control：" + control.ToString( ) + "，OS：" + this.OSTheme.ToString( ));
			this.SetInitialNowTheme(theme);
			control.RequestedTheme = this.NowTheme;
		}

		/// <summary>テーマ管理者を初期化し，初期テーマをthemeにします。</summary>
		/// <param name="theme">初期テーマ</param>
		public ThemeController(ElementTheme theme)
		{
			Log.Debug("ThemeController生成：theme：" + theme.ToString( ) + "，OS：" + this.OSTheme.ToString( ));
			this.SetInitialNowTheme(theme);
		}

		/// <summary>テーマ管理者を初期化し，controlをOSテーマにします。</summary>
		/// <param name="control">初期テーマを適用するUIコントロール</param>
		public ThemeController(Control control)
		{
			Log.Debug("ThemeController生成：control：" + control.ToString( ) + "，OS：" + this.OSTheme.ToString( ));
			this.SetInitialNowTheme(ElementTheme.Default);
			control.RequestedTheme = this.NowTheme;
		}

		/// <summary>テーマ管理者をOSテーマで初期化します。</summary>
		public ThemeController( )
		{
			Log.Debug("ThemeController生成：OS：" + this.OSTheme.ToString( ));
			this.SetInitialNowTheme(ElementTheme.Default);
		}

		/// <summary>初期のNowThemeをセットします。themeが送られてきた場合→そのtheme，そうでない場合→OS設定</summary>
		/// <param name="theme">初期化時に送られてきたtheme</param>
		private void SetInitialNowTheme(ElementTheme theme)
		{
			if (theme == ElementTheme.Light || theme == ElementTheme.Dark)
			{
				this.NowTheme = theme;
				return;
			}
			if (this.OSTheme == ApplicationTheme.Light)
			{
				this.NowTheme = ElementTheme.Light;
				return;
			}
			this.NowTheme = ElementTheme.Dark;
		}

		/// <summary>テーマを変更します。ダークテーマ→ライトテーマ，ライトテーマ→ダークテーマ</summary>
		/// <param name="control">変更テーマを適用するUIコントロール</param>
		public void ChangeTheme(Control control)
		{
			this.ChangeNowTheme( );
			control.RequestedTheme = this.NowTheme;
			Log.Debug("配色変更：" + control.ToString( ) + " を " + this.NowTheme + " へ");
		}

		/// <summary>NowThemeを反転させます。</summary>
		private void ChangeNowTheme( )
		{
			if (this.NowTheme == ElementTheme.Light)
			{
				this.NowTheme = ElementTheme.Dark;
				return;
			}
			this.NowTheme = ElementTheme.Light;
		}

	}
}
