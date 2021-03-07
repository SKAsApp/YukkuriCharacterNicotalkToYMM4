using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace YukkuriCharacterNicotalkToYMM4.Models
{
	/// <summary>「口」変換器</summary>
	public class MouthConverter: CharacterConverter
	{

		public MouthConverter( )
		{
			Log.Debug("MouthConverter生成");
		}

		/// <summary>「口」の変換をします。</summary>
		/// <param name="inputDirectory">変換元ディレクトリー</param>
		/// <param name="outputDirectory">出力先ディレクトリー</param>
		public override async Task Convert(StorageFolder inputDirectory, StorageFolder outputDirectory)
		{
			Log.Information("「口」の変換をします。");
			string characterName = inputDirectory.Name;
			IReadOnlyList<StorageFile> mouthFiles = null;
			try
			{
				StorageFolder characterDirectory = outputDirectory.GetFoldersAsync( ).AsTask( ).Result.Where(directory => directory.Name == characterName).First( );
				StorageFolder mouthDirectory = await characterDirectory.GetFolderAsync("口");
				Log.Debug("「口」フォルダー：" + mouthDirectory.Path);
				mouthFiles = await mouthDirectory.GetFilesAsync( );
			}
			catch
			{
				throw new ArgumentException("「口」フォルダーの取得に失敗しました。\r\n\r\n実行中に出力ディレクトリーのファイル操作をしないでください。");
			}
			await this.SearchMouths(mouthFiles);
		}

		/// <summary>「口」のabcパターンたちを探します。</summary>
		/// <param name="mouthFiles">「口」のファイルたち</param>
		private async Task SearchMouths(IReadOnlyList<StorageFile> mouthFiles)
		{
			await this.Search(mouthFiles, "a", this.RenameMouths, "「口」");
		}

		/// <summary>「口」のabcパターンたちの名前を変更します。</summary>
		/// <param name="mouthFiles">「口」のファイルたち</param>
		/// <param name="numberName">「口」のファイルの番号（例：00a.pngの00の部分）</param>
		private async Task RenameMouths(IReadOnlyList<StorageFile> mouthFiles, string numberName)
		{
			int animatedMouthFilesCount = mouthFiles.Where(mouthFile => mouthFile.Name.StartsWith(numberName)).Count( );
			Log.Debug("「口」" + numberName + "フレーム数：" + animatedMouthFilesCount.ToString( ));
			await this.RenameMouth(mouthFiles, numberName, animatedMouthFilesCount, 0, 0);
		}

		/// <summary>「口」のひとつのabcパターンの名前を変更します。</summary>
		/// <param name="mouthFiles">「口」のファイルたち</param>
		/// <param name="numberName">「口」のファイルの番号（例：00a.pngの00の部分）</param>
		/// <param name="frameLength">フレーム総数</param>
		/// <param name="alphabetNumber">変換前のフレームアルファベットを数字表記したもの（例：00a.pngのaの部分→1）</param>
		/// <param name="frame">変換後のフレーム番号（例：00（口パク）.1.pngの1の部分）</param>
		private async Task RenameMouth(IReadOnlyList<StorageFile> mouthFiles, string numberName, int frameLength, int alphabetNumber, int frame)
		{
			string alphabet = this.NumberToAlphabetA(alphabetNumber);
			StorageFile convertingMouthFile = null;
			try
			{
				convertingMouthFile = mouthFiles.Where(mouthFile => mouthFile.DisplayName == numberName + alphabet).First( );
				string extension = convertingMouthFile.FileType;
				if (alphabetNumber == frameLength - 1)
				{
					Log.Debug("「口」名前変更：" + convertingMouthFile.Name + " → " + numberName + "（口パク）" + extension);
					await convertingMouthFile.RenameAsync(numberName + "（口パク）" + extension, NameCollisionOption.ReplaceExisting);
					return;
				}
				Log.Debug("「口」名前変更：" + convertingMouthFile.Name + " → " + numberName + "（口パク）." + frame + extension);
				await convertingMouthFile.RenameAsync(numberName + "（口パク）." + frame + extension, NameCollisionOption.ReplaceExisting);
			}
			catch (Exception)
			{
				throw new ArgumentException("「口」ファイルが見つかりませんでした。\r\n\r\n実行中に出力ディレクトリーのファイル操作をしないでください。");
			}
			await this.RenameMouth(mouthFiles, numberName, frameLength, alphabetNumber + 1, frame + 1);
		}

	}
}
