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
			// aのみ（00a.pngのようなもの）を抽出
			await this.SearchMeAs(meFiles);
		}

		/// <summary>「目」のabcパターンたちを探します。</summary>
		/// <param name="meFiles">「目」のファイルたち</param>
		private async Task SearchMeAs(IReadOnlyList<StorageFile> meFiles)
		{
			Regex animationRegex = new Regex("[0-9]{2}a.(png|PNG|gif|GIF|bmp|BMP)");
			IEnumerable<string> animationNames = meFiles.Select(meFile => meFile.Name).AsParallel( ).Where(meFileName => animationRegex.IsMatch(meFileName));
			Log.Debug("「目」アニメーション一覧：" + string.Join(", ", animationNames));
			Regex numberRegex = new Regex("([0-9]{2})a.(png|PNG|gif|GIF|bmp|BMP)");
			IEnumerable<string> numberNames = animationNames.Select(fileName => numberRegex.Match(fileName).Groups[1].ToString( ));
			Log.Debug("numberNames：" + string.Join(", ", numberNames));
			await Task.WhenAll(numberNames.Select(async numberName => await this.RenameMeAs(meFiles, numberName)));
		}

		/// <summary>「目」のabcパターンたちの名前変更します。</summary>
		/// <param name="meFiles">「目」のファイルたち</param>
		/// <param name="numberName">「目」のファイルの番号（00a.pngの00の部分）</param>
		private async Task RenameMeAs(IReadOnlyList<StorageFile> meFiles, string numberName)
		{
			int animatedMeFilesCount = meFiles.Where(meFile => meFile.Name.StartsWith(numberName)).Count( );
			Log.Debug("目" + numberName + "フレーム数：" + animatedMeFilesCount.ToString( ));
			await this.RenameMeA(meFiles, numberName, animatedMeFilesCount - 1, 0);
		}

		/// <summary>「目」のabcパターンの名前変更します。</summary>
		/// <param name="meFiles">「目」のファイルたち</param>
		/// <param name="numberName">「目」のファイルの番号（00a.pngの00の部分）</param>
		/// <param name="alphabetNumber">変換前のフレームアルファベットを数字表記したもの（00a.pngのaの部分→1）</param>
		/// <param name="frame">変換後のフレーム番号（00（まばたき）.1.pngの1の部分）</param>
		private async Task RenameMeA(IReadOnlyList<StorageFile> meFiles, string numberName, int alphabetNumber, int frame)
		{
			string alphabet = this.NumberToAlphabetA(alphabetNumber);
			StorageFile convertingMeFile = null;
			try
			{
				convertingMeFile = meFiles.Where(meFile => meFile.DisplayName == numberName + alphabet).First( );
				string extension = convertingMeFile.FileType;
				if (alphabetNumber == 0)
				{
					Log.Debug("名前変更：" + convertingMeFile.Name + " → " + numberName + "（まばたき）" + extension);
					await convertingMeFile.RenameAsync(numberName + "（まばたき）" + extension, NameCollisionOption.ReplaceExisting);
					return;
				}
				Log.Debug("名前変更：" + convertingMeFile.Name + " → " + numberName + "（まばたき）." + frame + extension);
				await convertingMeFile.RenameAsync(numberName + "（まばたき）." + frame + extension, NameCollisionOption.ReplaceExisting);
			}
			catch (Exception)
			{
				throw new ArgumentException("「目」ファイルが見つかりませんでした。\r\n\r\n実行中に出力ディレクトリーのファイル操作をしないでください。");
			}
			await this.RenameMeA(meFiles, numberName, alphabetNumber - 1, frame + 1);
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

	}
}
