﻿using Serilog;
using System.Threading.Tasks;
using Windows.Storage;

namespace YukkuriCharacterNicotalkToYMM4.Models
{
	/// <summary>「その他」一括変換器</summary>
	public class OtherConverters: CharacterConverter
	{
		public OtherConverters( )
		{
			Log.Debug("OtherConverters生成");
		}

		/// <summary>「他」を一括で変換します。</summary>
		/// <param name="inputDirectory">変換元ディレクトリー</param>
		/// <param name="outputDirectory">出力先ディレクトリー</param>
		public override async Task Convert(StorageFolder inputDirectory, StorageFolder outputDirectory)
		{
			Log.Information("「他」を一括で変換します。");
			FaceConverter faceConverter = new FaceConverter( );
			BehindConverter behindConverter = new BehindConverter( );
			await Task.WhenAll(
				faceConverter.Convert(inputDirectory, outputDirectory), 
				behindConverter.Convert(inputDirectory, outputDirectory));
		}

	}
}
