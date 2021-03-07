using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace YukkuriCharacterNicotalkToYMM4.Models
{
	/// <summary>「目」変換器</summary>
	public class MeConverter: CharacterConverter
	{

		public MeConverter( )
		{
			Log.Debug("MeConverter生成");
		}

		/// <summary>「目」の変換をします。</summary>
		/// <param name="inputDirectory">変換元ディレクトリー</param>
		/// <param name="outputDirectory">出力先ディレクトリー</param>
		public override async Task Convert(StorageFolder inputDirectory, StorageFolder outputDirectory)
		{
			Log.Information("「目」の変換をします。");
			string characterName = inputDirectory.Name;
			IReadOnlyList<StorageFile> meFiles = null;
			try
			{
				StorageFolder characterDirectory = outputDirectory.GetFoldersAsync( ).AsTask( ).Result.Where(directory => directory.Name == characterName).First( );
				StorageFolder meDirectory = await characterDirectory.GetFolderAsync("目");
				Log.Debug("「目」フォルダー：" + meDirectory.Path);
				meFiles = await meDirectory.GetFilesAsync( );
			}
			catch
			{
				throw new ArgumentException("「目」フォルダーの取得に失敗しました。\r\n\r\n実行中に出力ディレクトリーのファイル操作をしないでください。");
			}
			await Task.WhenAll(this.SearchMeAs(meFiles), this.SearchMeKs(meFiles), this.SearchMeXs(meFiles), this.SearchMeYs(meFiles), this.SearchMeZs(meFiles));
		}

		/// <summary>「目」のxxxパターンたちを探します。</summary>
		/// <param name="meFiles">「目」のファイルたち</param>
		/// <param name="patternFirst">xxxパターンの最初の1文字（例：abcパターン→a）</param>
		/// <param name="renamesFunc">xxxパターンたちの名前変更関数</param>
		private async Task SearchMes(IReadOnlyList<StorageFile> meFiles, string patternFirst, Func<IReadOnlyList<StorageFile>, string, Task> renamesFunc)
		{
			await this.Search(meFiles, patternFirst, renamesFunc, "「目」");
		}

		/// <summary>【抽象】「目」のxxxパターンたちの名前を変更します。</summary>
		/// <param name="meFiles">「目」のファイルたち</param>
		/// <param name="numberName">「目」のファイルの番号（例：00a.pngの00の部分）</param>
		/// <param name="renameFunc">ひとつのxxxパターンを名前変更する関数</param>
		private async Task RenameMes(IReadOnlyList<StorageFile> meFiles, string numberName, Func<IReadOnlyList<StorageFile>, string, int, int, Task> renameFunc)
		{
			int animatedMeFilesCount = meFiles.Where(meFile => meFile.Name.StartsWith(numberName)).Count( );
			Log.Debug("「目」" + numberName + "フレーム数：" + animatedMeFilesCount.ToString( ));
			await renameFunc(meFiles, numberName, animatedMeFilesCount - 1, 0);
		}

		/// <summary>【抽象】「目」のひとつのxxxパターンの名前を変更します。</summary>
		/// <param name="meFiles">「目」のファイルたち</param>
		/// <param name="numberToAlphabet">数字→アルファベットの写像を返す関数</param>
		/// <param name="numberName">「目」のファイルの番号（例：00a.pngの00の部分）</param>
		/// <param name="alphabetNumber">変換前のフレームアルファベットを数字表記したもの（例：00a.pngのaの部分→1）</param>
		/// <param name="frame">変換後のフレーム番号（例：00（まばたき）.1.pngの1の部分）</param>
		/// <param name="type">アニメーション種別（例：（まばたき））</param>
		private async Task RenameMe(IReadOnlyList<StorageFile> meFiles, Func<int, string> numberToAlphabet, string numberName, int alphabetNumber, int frame, string type)
		{
			string alphabet = numberToAlphabet(alphabetNumber);
			StorageFile convertingMeFile = null;
			try
			{
				convertingMeFile = meFiles.Where(meFile => meFile.DisplayName == numberName + alphabet).First( );
				string extension = convertingMeFile.FileType;
				if (alphabetNumber == 0)
				{
					Log.Debug("「目」名前変更：" + convertingMeFile.Name + " → " + numberName + type + extension);
					await convertingMeFile.RenameAsync(numberName + type + extension, NameCollisionOption.ReplaceExisting);
					return;
				}
				Log.Debug("「目」名前変更：" + convertingMeFile.Name + " → " + numberName + type + "." + frame + extension);
				await convertingMeFile.RenameAsync(numberName + type + "." + frame + extension, NameCollisionOption.ReplaceExisting);
			}
			catch (Exception)
			{
				throw new ArgumentException("「目」ファイルが見つかりませんでした。\r\n\r\n実行中に出力ディレクトリーのファイル操作をしないでください。");
			}
			await this.RenameMe(meFiles, numberToAlphabet, numberName, alphabetNumber - 1, frame + 1, type);
		}

		/// <summary>【抽象】「目」のアニメーションでないものの名前を変更します。</summary>
		/// <param name="meFiles">「目」のファイルたち</param>
		/// <param name="numberName">「目」のファイルの番号（例：00x.pngの00の部分）</param>
		/// <param name="patternFirst">パターンアルファベット（例：00x.png→x）</param>
		/// <param name="type">アニメーション種別（例：（目・眉・口セット））</param>
		private async Task RenameMeNotAnimation(IReadOnlyList<StorageFile> meFiles, string numberName, string patternFirst, string type)
		{
			StorageFile convertingMeFile = null;
			try
			{
				convertingMeFile = meFiles.Where(meFile => meFile.DisplayName == numberName + patternFirst).First( );
				string extension = convertingMeFile.FileType;
				Log.Debug("「目」名前変更：" + convertingMeFile.Name + " → " + numberName + type + extension);
				await convertingMeFile.RenameAsync(numberName + type + extension, NameCollisionOption.ReplaceExisting);
			}
			catch (Exception)
			{
				throw new ArgumentException("「目」ファイルが見つかりませんでした。\r\n\r\n実行中に出力ディレクトリーのファイル操作をしないでください。");
			}
		}

		/// <summary>「目」のabcパターンたちを探します。</summary>
		/// <param name="meFiles">「目」のファイルたち</param>
		private async Task SearchMeAs(IReadOnlyList<StorageFile> meFiles)
		{
			await this.SearchMes(meFiles, "a", this.RenameMeAs);
		}

		/// <summary>「目」のabcパターンたちの名前を変更します。</summary>
		/// <param name="meFiles">「目」のファイルたち</param>
		/// <param name="numberName">「目」のファイルの番号（例：00a.pngの00の部分）</param>
		private async Task RenameMeAs(IReadOnlyList<StorageFile> meFiles, string numberName)
		{
			await this.RenameMes(meFiles, numberName, this.RenameMeA);
		}

		/// <summary>「目」のひとつのabcパターンの名前を変更します。</summary>
		/// <param name="meFiles">「目」のファイルたち</param>
		/// <param name="numberName">「目」のファイルの番号（例：00a.pngの00の部分）</param>
		/// <param name="alphabetNumber">変換前のフレームアルファベットを数字表記したもの（例：00a.pngのaの部分→1）</param>
		/// <param name="frame">変換後のフレーム番号（例：00（まばたき）.1.pngの1の部分）</param>
		private async Task RenameMeA(IReadOnlyList<StorageFile> meFiles, string numberName, int alphabetNumber, int frame)
		{
			await this.RenameMe(meFiles, this.NumberToAlphabetA, numberName, alphabetNumber, frame, "（まばたき）");
		}

		/// <summary>「目」のklmパターンたちを探します。</summary>
		/// <param name="meFiles">「目」のファイルたち</param>
		private async Task SearchMeKs(IReadOnlyList<StorageFile> meFiles)
		{
			await this.SearchMes(meFiles, "k", this.RenameMeKs);
		}

		/// <summary>「目」のklmパターンたちの名前を変更します。</summary>
		/// <param name="meFiles">「目」のファイルたち</param>
		/// <param name="numberName">「目」のファイルの番号（例：00k.pngの00の部分）</param>
		private async Task RenameMeKs(IReadOnlyList<StorageFile> meFiles, string numberName)
		{
			await this.RenameMes(meFiles, numberName, this.RenameMeK);
		}

		/// <summary>「目」のひとつのklmパターンの名前を変更します。</summary>
		/// <param name="meFiles">「目」のファイルたち</param>
		/// <param name="numberName">「目」のファイルの番号（例：00k.pngの00の部分）</param>
		/// <param name="alphabetNumber">変換前のフレームアルファベットを数字表記したもの（例：00k.pngのkの部分→1）</param>
		/// <param name="frame">変換後のフレーム番号（例：00【非対応】（登場退場）.1.pngの1の部分）</param>
		private async Task RenameMeK(IReadOnlyList<StorageFile> meFiles, string numberName, int alphabetNumber, int frame)
		{
			await this.RenameMe(meFiles, this.NumberToAlphabetK, numberName, alphabetNumber, frame, "【非対応】（登場退場）");
		}

		/// <summary>「目」のxたちを探します。</summary>
		/// <param name="meFiles">「目」のファイルたち</param>
		private async Task SearchMeXs(IReadOnlyList<StorageFile> meFiles)
		{
			await this.SearchMes(meFiles, "x", this.RenameMeX);
		}

		/// <summary>「目」のxの名前を変更します。</summary>
		/// <param name="meFiles">「目」のファイルたち</param>
		/// <param name="numberName">「目」のファイルの番号（00x.pngの00の部分）</param>
		private async Task RenameMeX(IReadOnlyList<StorageFile> meFiles, string numberName)
		{
			await this.RenameMeNotAnimation(meFiles, numberName, "x", "x（目・眉・口セット）");
		}

		/// <summary>「目」のyたちを探します。</summary>
		/// <param name="meFiles">「目」のファイルたち</param>
		private async Task SearchMeYs(IReadOnlyList<StorageFile> meFiles)
		{
			await this.SearchMes(meFiles, "y", this.RenameMeY);
		}

		/// <summary>「目」のyの名前を変更します。</summary>
		/// <param name="meFiles">「目」のファイルたち</param>
		/// <param name="numberName">「目」のファイルの番号（00y.pngの00の部分）</param>
		private async Task RenameMeY(IReadOnlyList<StorageFile> meFiles, string numberName)
		{
			await this.RenameMeNotAnimation(meFiles, numberName, "y", "y（目・口セット）");
		}

		/// <summary>「目」のzたちを探します。</summary>
		/// <param name="meFiles">「目」のファイルたち</param>
		private async Task SearchMeZs(IReadOnlyList<StorageFile> meFiles)
		{
			await this.SearchMes(meFiles, "z", this.RenameMeZ);
		}

		/// <summary>「目」のzの名前を変更します。</summary>
		/// <param name="meFiles">「目」のファイルたち</param>
		/// <param name="numberName">「目」のファイルの番号（00z.pngの00の部分）</param>
		private async Task RenameMeZ(IReadOnlyList<StorageFile> meFiles, string numberName)
		{
			await this.RenameMeNotAnimation(meFiles, numberName, "z", "z（目・眉セット）");
		}

	}
}
