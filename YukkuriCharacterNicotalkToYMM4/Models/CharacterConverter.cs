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
		/// <param name="outputDirectory">出力先ディレクトリー</param>
		public async Task ConvertMe(StorageFolder inputDirectory, StorageFolder outputDirectory)
		{
			Log.Information("「目」の変換をします。");
			string characterName = inputDirectory.Name;
			StorageFolder characterDirectory = outputDirectory.GetFoldersAsync( ).AsTask( ).Result.Where(directory => directory.Name == characterName).First( );
			StorageFolder meDirectory = await characterDirectory.GetFolderAsync("目");
			Log.Debug("「目」フォルダー：" + meDirectory.Path);
			IReadOnlyList<StorageFile> meFiles = await meDirectory.GetFilesAsync( );
			// aのみ（00a.pngのようなもの）を抽出
			Regex animationRegex = new Regex("[0-9]{2}a.(png|PNG|gif|GIF|bmp|BMP)");
			IEnumerable<string> animationNames = meFiles.Select(meFile => meFile.Name).AsParallel( ).Where(meFileName => animationRegex.IsMatch(meFileName));
			Log.Debug("「目」アニメーション一覧：" + string.Join(", ", animationNames.ToList( )));
		}

	}
}
