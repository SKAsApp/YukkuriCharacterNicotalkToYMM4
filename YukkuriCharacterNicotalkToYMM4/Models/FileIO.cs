using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.AccessCache;
using System.Diagnostics;
using Serilog;
using System.Text.RegularExpressions;

namespace YukkuriCharacterNicotalkToYMM4.Models
{
	/// <summary>ファイル入出力の管理者</summary>
	public class FileIO
	{
		public FileIO( )
		{
			Log.Debug("FileIO生成");
		}

		/// <summary>フォルダーピッカーを表示します。不正な場合，メッセージダイアログを表示します。</summary>
		/// <returns>フォルダーのパス。不正な場合，空文字列。</returns>
		public async Task<StorageFolder> SelectFolder(EDirectoryType directoryType)
		{
			FolderPicker folderPicker = new FolderPicker( );
			folderPicker.FileTypeFilter.Add("*");
			Log.Information("FolderPicker生成");
			StorageFolder folder = await folderPicker.PickSingleFolderAsync( );
			if (folder != null)
			{
				this.AddDirectoryPermission(directoryType, folder);
			}
			return folder;
		}

		/// <summary>選択したフォルダーをアクセス許可アイテムに追加します。</summary>
		/// <param name="directoryType">ディレクトリータイプ</param>
		/// <param name="storageItem">ファイルまたはフォルダー</param>
		private void AddDirectoryPermission(EDirectoryType directoryType, IStorageItem storageItem)
		{
			Log.Debug("ファイルアクセス許可：" + storageItem.Path + " を " + directoryType.ToString( ) + " として");
			switch (directoryType)
			{
				case EDirectoryType.Internal:
					StorageApplicationPermissions.FutureAccessList.AddOrReplace("Internal", storageItem);
					break;
				case EDirectoryType.Input:
					StorageApplicationPermissions.FutureAccessList.AddOrReplace("Input", storageItem);
					break;
				case EDirectoryType.Output:
					StorageApplicationPermissions.FutureAccessList.AddOrReplace("Output", storageItem);
					break;
				default:
					StorageApplicationPermissions.FutureAccessList.AddOrReplace("Other", storageItem);
					break;
			}
		}

		/// <summary>ディレクトリーをコピーします。</summary>
		/// <param name="fromDirectory">コピー元ディレクトリー</param>
		/// <param name="toDirectory">コピー先ディレクトリー</param>
		public async Task CopyDirectory(StorageFolder fromDirectory, StorageFolder toDirectory)
		{
			Log.Information("ディレクトリーをコピーします。");
			string fromParentPath = new DirectoryInfo(fromDirectory.Path).Parent.FullName;
			if (fromParentPath == toDirectory.Path)
			{
				throw new ArgumentException("変換元ディレクトリーと出力先ディレクトリーが被っています。\r\n\r\n出力先ディレクトリーを変更してください。");
			}
			await this.CopyDirectoryMain(fromDirectory, toDirectory);
		}

		/// <summary>ディレクトリーコピーのメイン処理です。</summary>
		/// <param name="fromDirectory">コピー元ディレクトリー</param>
		/// <param name="toDirectory">コピー先ディレクトリー</param>
		private async Task CopyDirectoryMain(StorageFolder fromDirectory, StorageFolder toDirectory)
		{
			StorageFolder workingDirectory = await toDirectory.CreateFolderAsync(fromDirectory.Name, CreationCollisionOption.ReplaceExisting);
			Log.Debug("WorkingDirectory：" + workingDirectory.Path);
			IReadOnlyList<StorageFile> fromFiles = await fromDirectory.GetFilesAsync( );
			Log.Debug("「" + fromDirectory.Name + "」fromFiles：" + string.Join(", ", fromFiles.Select(file => file.Name)));
			await Task.WhenAll(fromFiles.Select(async file => await file.CopyAsync(workingDirectory, file.Name, NameCollisionOption.ReplaceExisting)));
			IReadOnlyList<StorageFolder> fromChildDirectories = await fromDirectory.GetFoldersAsync( );
			Log.Debug("「" + fromDirectory.Name + "」fromChildDirectories：" + string.Join(", ", fromChildDirectories.Select(directory => directory.Name)));
			if (fromChildDirectories.Any( ))
			{
				await Task.WhenAll(fromChildDirectories.Select(async directory => await this.CopyDirectoryMain(directory, workingDirectory)));
			}
		}

	}
}
