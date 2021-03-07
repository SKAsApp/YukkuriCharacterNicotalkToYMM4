using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace YukkuriCharacterNicotalkToYMM4.Models
{
	/// <summary>「顔」変換器</summary>
	public class FaceConverter: OtherConverter
	{
		public FaceConverter( )
		{
			Log.Debug("FaceConverter生成");
			this.VName = "顔";
		}

		/// <summary>「顔」の変換をします。</summary>
		/// <param name="inputDirectory">変換元ディレクトリー</param>
		/// <param name="outputDirectory">出力先ディレクトリー</param>
		public override async Task Convert(StorageFolder inputDirectory, StorageFolder outputDirectory)
		{
			await base.Convert(inputDirectory, outputDirectory);
			if (!this.HasVDirectory)
			{
				return;
			}
			await Task.WhenAll(this.SearchFaceAs(this.VFiles), this.SearchFaceBs(this.VFiles));
			await this.RenameAll(this.VFiles, "【非対応】");
			await this.RemoveVDirectory( );
		}

		/// <summary>「顔」のaパターンたちを探します。</summary>
		/// <param name="faceFiles">「顔」のファイルたち</param>
		private async Task SearchFaceAs(IReadOnlyList<StorageFile> faceFiles)
		{
			await this.Search(faceFiles, "a", this.RenameFaceA, "「" + this.VName + "」");
		}

		/// <summary>「顔」のひとつのaパターンの名前を変更し，移動します。</summary>
		/// <param name="faceFiles">「顔」のファイルたち</param>
		/// <param name="numberName">「顔」のファイルの番号（例：00a.pngの00の部分）</param>
		private async Task RenameFaceA(IReadOnlyList<StorageFile> faceFiles, string numberName)
		{
			await this.Rename(faceFiles, numberName, "a", "【非対応】（赤面）");
		}

		/// <summary>「顔」のbパターンたちを探します。</summary>
		/// <param name="faceFiles">「顔」のファイルたち</param>
		private async Task SearchFaceBs(IReadOnlyList<StorageFile> faceFiles)
		{
			await this.Search(faceFiles, "b", this.RenameFaceB, "「" + this.VName + "」");
		}

		/// <summary>「顔」のひとつのbパターンの名前を変更し，移動します。</summary>
		/// <param name="faceFiles">「顔」のファイルたち</param>
		/// <param name="numberName">「顔」のファイルの番号（例：00b.pngの00の部分）</param>
		private async Task RenameFaceB(IReadOnlyList<StorageFile> faceFiles, string numberName)
		{
			await this.Rename(faceFiles, numberName, "b", "【非対応】（蒼白）");
		}

	}
}
