using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace YukkuriCharacterNicotalkToYMM4.Models
{
	/// <summary>キャラクター素材変換器</summary>
	public abstract class CharacterConverter: ICharacterConverter
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

		public abstract Task Convert(StorageFolder inputDirectory, StorageFolder outputDirectory);

		/// <summary>【抽象】「目」または「口」のxxxパターンたちを探します。</summary>
		/// <param name="files">「目」または「口」のファイルたち</param>
		/// <param name="patternFirst">xxxパターンの最初の1文字（例：abcパターン→a）</param>
		/// <param name="renamesFunc">xxxパターンたちの名前変更関数</param>
		/// <param name="part">部位（「「目」」または「「口」」）</param>
		protected async Task Search(IReadOnlyList<StorageFile> files, string patternFirst, Func<IReadOnlyList<StorageFile>, string, Task> renamesFunc, string part)
		{
			Regex patternRegex = new Regex("([0-9]{2})" + patternFirst + ".(png|PNG|gif|GIF|bmp|BMP)");
			IEnumerable<string> patternNames = files.Select(meFile => meFile.Name).AsParallel( ).Where(meFileName => patternRegex.IsMatch(meFileName));
			Log.Debug(part + patternFirst + "一覧：" + string.Join(", ", patternNames));
			IEnumerable<string> numberNames = patternNames.Select(fileName => patternRegex.Match(fileName).Groups[1].ToString( ));
			Log.Debug(part + patternFirst + " numberNames：" + string.Join(", ", numberNames));
			await Task.WhenAll(numberNames.Select(async numberName => await renamesFunc(files, numberName)));
		}

		protected string NumberToAlphabetA(int number)
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
		
		protected string NumberToAlphabetK(int number)
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
