using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace YukkuriCharacterNicotalkToYMM4.Models
{
	/// <summary>キャラクター素材変換器</summary>
	public class CharacterConverter
	{
		/// <summary>5系以降か</summary>
		private bool IsFive = false;

		public CharacterConverter( )
		{
			Log.Debug("CharacterConverter生成");
		}

		/// <summary>変換可能なディレクトリーが入力されたか確認します。また，5系かどうかの判別もします。</summary>
		/// <param name="inputDirectory">変換元ディレクトリー</param>
		public bool CheckDirectory(StorageFolder inputDirectory)
		{
			Log.Information("変換可能なディレクトリーか確認します。");
			bool isCharacterDirectory = this.CheckCharacterDirectory(inputDirectory);
			Log.Debug("キャラ素材ディレクトリーか：" + isCharacterDirectory.ToString( ));
			if (!isCharacterDirectory)
			{
				throw new ArgumentException("変換元がキャラ素材ディレクトリーではありません。\r\n\r\n「目」「口」「体」などのフォルダーのひとつ上の，キャラクター名のフォルダーを選択してください。");
			}
			this.IsFive = this.CheckCharacter5(inputDirectory);
			Log.Debug("5系キャラ素材か：" + this.IsFive.ToString( ));
			if (this.IsFive)
			{
				throw new ArgumentException("キャラ素材スクリプトVer5a以降対応のキャラ素材のようです。\r\n\r\n大変申し訳ありませんが，このアプリでこのキャラ素材は変換できません。\r\nまた，画像の改変なしにゆっくりMovieMakerで使うこともできません。最新のキャラ素材スクリプトに対応した「nicotalk」の使用をご検討ください。");
			}
			return isCharacterDirectory && !this.IsFive;
		}

		/// <summary>キャラ素材ディレクトリーであるか確認します。</summary>
		/// <param name="inputDirectory">変換元ディレクトリー</param>
		/// <returns>「目」「口」「体」すべてあるフォルダーであればtrue</returns>
		private bool CheckCharacterDirectory(StorageFolder inputDirectory)
		{
			IEnumerable<string> characterFoldersName = inputDirectory.GetFoldersAsync( ).AsTask( ).Result.Select(folder => folder.Name);
			return characterFoldersName.Where(name => name == "目").Any( ) && characterFoldersName.Where(name => name == "口").Any( ) && characterFoldersName.Where(name => name == "体").Any( );
		}

		/// <summary>キャラ素材が5系であるか確認します。</summary>
		/// <param name="inputDirectory">変換元ディレクトリー</param>
		/// <returns>5系（「服上」「服下」がある，または「目」の中に子フォルダーがある）であればtrue</returns>
		private bool CheckCharacter5(StorageFolder inputDirectory)
		{
			IReadOnlyList<StorageFolder> characterFolders = inputDirectory.GetFoldersAsync( ).AsTask( ).Result;
			IEnumerable<string> characterFoldersName = characterFolders.Select(folder => folder.Name);
			bool hasMeChildFolder = characterFolders.Where(characterFolder => characterFolder.Name == "目").First( ).GetFoldersAsync( ).AsTask( ).Result.Any( );
			return characterFoldersName.Where(name => name == "服上").Any( ) || characterFoldersName.Where(name => name == "服下").Any( ) || hasMeChildFolder;
		}

		/// <summary>「目」の変換をします。</summary>
		/// <param name="inputDirectory">変換元ディレクトリー</param>
		/// <param name="outputDirectory">出力先ディレクトリー</param>
		public async Task ConvertMe(StorageFolder inputDirectory, StorageFolder outputDirectory)
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

		/// <summary>【抽象】「目」または「口」のxxxパターンたちを探します。</summary>
		/// <param name="files">「目」または「口」のファイルたち</param>
		/// <param name="patternFirst">xxxパターンの最初の1文字（例：abcパターン→a）</param>
		/// <param name="renamesFunc">xxxパターンたちの名前変更関数</param>
		/// <param name="part">部位（「「目」」または「「口」」）</param>
		private async Task Search(IReadOnlyList<StorageFile> files, string patternFirst, Func<IReadOnlyList<StorageFile>, string, Task> renamesFunc, string part)
		{
			Regex patternRegex = new Regex("([0-9]{2})" + patternFirst + ".(png|PNG|gif|GIF|bmp|BMP)");
			IEnumerable<string> patternNames = files.Select(meFile => meFile.Name).AsParallel( ).Where(meFileName => patternRegex.IsMatch(meFileName));
			Log.Debug(part + patternFirst + "一覧：" + string.Join(", ", patternNames));
			IEnumerable<string> numberNames = patternNames.Select(fileName => patternRegex.Match(fileName).Groups[1].ToString( ));
			Log.Debug(part + patternFirst + " numberNames：" + string.Join(", ", numberNames));
			await Task.WhenAll(numberNames.Select(async numberName => await renamesFunc(files, numberName)));
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
					Log.Debug("名前変更：" + convertingMeFile.Name + " → " + numberName + type + extension);
					await convertingMeFile.RenameAsync(numberName + type + extension, NameCollisionOption.ReplaceExisting);
					return;
				}
				Log.Debug("名前変更：" + convertingMeFile.Name + " → " + numberName + type + "." + frame + extension);
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
				Log.Debug("名前変更：" + convertingMeFile.Name + " → " + numberName + type + extension);
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

		private string NumberToAlphabetA(int number)
		{
			switch (number)
			{
				case 1:
					return "a";
				case 2:
					return "b";
				case 3:
					return "c";
				case 4:
					return "d";
				case 5:
					return "e";
				case 6:
					return "f";
				case 7:
					return "g";
				case 8:
					return "h";
				case 9:
					return "i";
				case 10:
					return "j";
				default:
					return "";
			}
		}

		private string NumberToAlphabetK(int number)
		{
			switch (number)
			{
				case 1:
					return "k";
				case 2:
					return "l";
				case 3:
					return "m";
				case 4:
					return "n";
				case 5:
					return "o";
				case 6:
					return "p";
				case 7:
					return "q";
				case 8:
					return "r";
				case 9:
					return "s";
				case 10:
					return "t";
				default:
					return "";
			}
		}

		/// <summary>「口」の変換をします。</summary>
		/// <param name="inputDirectory">変換元ディレクトリー</param>
		/// <param name="outputDirectory">出力先ディレクトリー</param>
		public async Task ConvertMouth(StorageFolder inputDirectory, StorageFolder outputDirectory)
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
					Log.Debug("名前変更：" + convertingMouthFile.Name + " → " + numberName + "（口パク）" + extension);
					await convertingMouthFile.RenameAsync(numberName + "（口パク）" + extension, NameCollisionOption.ReplaceExisting);
					return;
				}
				Log.Debug("名前変更：" + convertingMouthFile.Name + " → " + numberName + "（口パク）." + frame + extension);
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
