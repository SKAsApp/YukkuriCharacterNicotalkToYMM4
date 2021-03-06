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
	/// <summary>「その他」変換器</summary>
	public class OtherConverter: CharacterConverter
	{
		/// <summary>部位名</summary>
		protected string VName { get; set; } = "";
		/// <summary>「他」ディレクトリー</summary>
		protected StorageFolder OtherDirectory { get; set; } = null;
		/// <summary>部位ディレクトリー</summary>
		protected StorageFolder VDirectory { get; private set; } = null;
		/// <summary>部位ファイルたち</summary>
		protected IReadOnlyList<StorageFile> VFiles { get; private set; } = null;

		public OtherConverter( )
		{
			Log.Debug("OtherConverter生成");
		}

		/// <summary>「その他」の変換をします。</summary>
		/// <param name="inputDirectory">変換元ディレクトリー</param>
		/// <param name="outputDirectory">出力先ディレクトリー</param>
		public override async Task Convert(StorageFolder inputDirectory, StorageFolder outputDirectory)
		{
			Log.Information("「" + this.VName + "」の変換をします。");
			string characterName = inputDirectory.Name;
			try
			{
				StorageFolder characterDirectory = outputDirectory.GetFoldersAsync( ).AsTask( ).Result.Where(directory => directory.Name == characterName).First( );
				this.VDirectory = await characterDirectory.GetFolderAsync(this.VName);
				try
				{
					this.OtherDirectory = await characterDirectory.GetFolderAsync("他");
				}
				catch (Exception)
				{
					this.OtherDirectory = await characterDirectory.CreateFolderAsync("他");
				}
				Log.Debug("「" + this.VName + "」フォルダー：" + this.VDirectory.Path);
				this.VFiles = await this.VDirectory.GetFilesAsync( );
			}
			catch
			{
				Log.Debug("「" + this.VName + "」フォルダーは存在しません。");
				return;
			}
			// TODO: 各子クラスはここでSearch XXs Taskを実行
			// TODO: 部位ディレクトリー削除
		}

		/// <summary>その他の部位のひとつのxxパターンの名前を変更し，移動します。</summary>
		/// <param name="vFiles">その他の部位のファイルたち</param>
		/// <param name="numberName">その他の部位のファイルの番号（例：00a.pngの00の部分）</param>
		/// <param name="alphabet">アルファベット（例：00a.pngのaの部分），ない場合，空文字列</param>
		/// <param name="note">注釈（例：顔の00a.png⇒「（赤面）」）</param>
		protected async Task Rename(IReadOnlyList<StorageFile> vFiles, string numberName, string alphabet, string note)
		{
			StorageFile convertingVFile = null;
			try
			{
				convertingVFile = vFiles.Where(faceFile => faceFile.DisplayName == numberName + alphabet).First( );
				string extension = convertingVFile.FileType;
				Log.Debug("「" + this.VName + "」移動：" + convertingVFile.Name + " → " + this.VName + "_" + numberName + alphabet + note + extension);
				await this.MoveOther(convertingVFile, this.VName + "_" + numberName + alphabet + note + extension);
			}
			catch (Exception)
			{
				throw new ArgumentException("「" + this.VName + "」ファイルが見つかりませんでした。\r\n\r\n実行中に出力ディレクトリーのファイル操作をしないでください。");
			}
		}

		/// <summary>「他」フォルダーへ移動し，名前を変更します。</summary>
		/// <param name="file">移動元のファイル</param>
		/// <param name="newName">移動後のファイル名</param>
		private async Task MoveOther(StorageFile file, string newName)
		{
			await file.MoveAsync(this.OtherDirectory, newName, NameCollisionOption.ReplaceExisting);
		}

		/// <summary>部位ディレクトリーを完全に削除します（ゴミ箱へ送らない）。</summary>
		protected async Task RemoveVDirectory( )
		{
			Log.Debug("ディレクトリー削除：" + this.VDirectory.Path);
			await this.VDirectory.DeleteAsync(StorageDeleteOption.PermanentDelete);
		}

	}
}
