using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace YukkuriCharacterNicotalkToYMM4.Models
{
	/// <summary>「後」変換器</summary>
	public class BehindConverter: OtherConverter
	{
		public BehindConverter( )
		{
			Log.Debug("BehindConverter生成");
			this.VName = "後";
		}

		/// <summary>「後」の変換をします。</summary>
		/// <param name="inputDirectory">変換元ディレクトリー</param>
		/// <param name="outputDirectory">出力先ディレクトリー</param>
		public override async Task Convert(StorageFolder inputDirectory, StorageFolder outputDirectory)
		{
			await base.Convert(inputDirectory, outputDirectory);
			if (!this.HasVDirectory)
			{
				return;
			}
			await Task.WhenAll(this.SearchBehindUs(this.VFiles), this.SearchBehindMs(this.VFiles));
			await this.RenameAll(this.VFiles, "【非対応】");
			await this.RemoveVDirectory( );
		}

		/// <summary>「後」のuパターンたちを探します。</summary>
		/// <param name="behindFiles">「後」のファイルたち</param>
		private async Task SearchBehindUs(IReadOnlyList<StorageFile> behindFiles)
		{
			await Task.WhenAll(
				this.Search(behindFiles, "u", this.RenameBehindU, "「" + this.VName + "」"),
				this.Search(behindFiles, "u1", this.RenameBehindU1, "「" + this.VName + "」"),
				this.Search(behindFiles, "u2", this.RenameBehindU2, "「" + this.VName + "」"));
		}

		/// <summary>「後」のひとつのuパターンの名前を変更し，移動します。</summary>
		/// <param name="behindFiles">「後」のファイルたち</param>
		/// <param name="numberName">「後」のファイルの番号（例：00u1.pngの00の部分）</param>
		private async Task RenameBehindU(IReadOnlyList<StorageFile> behindFiles, string numberName)
		{
			await this.Rename(behindFiles, numberName, "u", "【非対応】（最背面）");
		}

		private async Task RenameBehindU1(IReadOnlyList<StorageFile> behindFiles, string numberName)
		{
			await this.Rename(behindFiles, numberName, "u1", "【非対応】（最背面）");
		}

		private async Task RenameBehindU2(IReadOnlyList<StorageFile> behindFiles, string numberName)
		{
			await this.Rename(behindFiles, numberName, "u2", "【非対応】（最背面）");
		}

		/// <summary>「後」のmパターンたちを探します。</summary>
		/// <param name="behindFiles">「後」のファイルたち</param>
		private async Task SearchBehindMs(IReadOnlyList<StorageFile> behindFiles)
		{
			await Task.WhenAll(
				this.Search(behindFiles, "m", this.RenameBehindM, "「" + this.VName + "」"),
				this.Search(behindFiles, "m1", this.RenameBehindM1, "「" + this.VName + "」"),
				this.Search(behindFiles, "m2", this.RenameBehindM2, "「" + this.VName + "」"));
		}

		/// <summary>「後」のひとつのmパターンの名前を変更し，移動します。</summary>
		/// <param name="behindFiles">「後」のファイルたち</param>
		/// <param name="numberName">「後」のファイルの番号（例：00m1.pngの00の部分）</param>
		private async Task RenameBehindM(IReadOnlyList<StorageFile> behindFiles, string numberName)
		{
			await this.Rename(behindFiles, numberName, "m", "（最前面）");
		}

		private async Task RenameBehindM1(IReadOnlyList<StorageFile> behindFiles, string numberName)
		{
			await this.Rename(behindFiles, numberName, "m1", "（最前面）");
		}

		private async Task RenameBehindM2(IReadOnlyList<StorageFile> behindFiles, string numberName)
		{
			await this.Rename(behindFiles, numberName, "m2", "【非対応】（最前面）");
		}

	}
}
