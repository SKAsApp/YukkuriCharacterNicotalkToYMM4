using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace YukkuriCharacterNicotalkToYMM4.Models
{
	public interface ICharacterConverter
	{
		bool CheckDirectory(StorageFolder storageFolder);
		Task Convert(StorageFolder inputDirectory, StorageFolder outputDirectory);
	}

}
