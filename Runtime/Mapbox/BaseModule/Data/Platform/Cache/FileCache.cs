using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mapbox.BaseModule.Data.DataFetchers;
using Mapbox.BaseModule.Data.Tasks;
using Mapbox.BaseModule.Data.Tiles;
using UnityEngine;

namespace Mapbox.BaseModule.Data.Platform.Cache
{
	public interface IFileCache
	{
		bool TestAvailability();
		event Action<MapboxTileData, string> FileSaved;
		void Add(MapboxTileData textureCacheItem, bool forceInsert, Action<string> post);
		//bool GetAsync(CanonicalTileId tileId, string tilesetId, bool isTextureNonreadable, Action<CacheItem> callback);
		bool Exists(CanonicalTileId tileId, string mapId);
		void ClearAll();
		void DeleteTileFile(MapboxTileData cacheItem);
		HashSet<string> GetFileList();
		bool GetAsync<T>(CanonicalTileId tileId, string tilesetId, bool isTextureNonreadable, Action<T> callback) where T : RasterData, new();
		IEnumerator GetFileCoroutine<T>(CanonicalTileId tileId, string tilesetId, bool isTextureNonreadable, Action<T> callback) where T : RasterData, new();
	}

	public class FileCache : IFileCache
	{
		public event Action<MapboxTileData, string> FileSaved = (cacheItem, s) => { };

		protected string CacheRootFolderName = "Mapbox/FileCache";
		public static string PersistantCacheRootFolderPath;
		private static string FileExtension = "png";

		protected FileDataFetcher _fileDataFetcher;
		protected Dictionary<string, string> MapIdToFolderNameDictionary;

		private TaskManager _taskManager;
		
		public FileCache(TaskManager taskManager, string folderNamePostFix = "")
		{
			CacheRootFolderName += folderNamePostFix;
			PersistantCacheRootFolderPath = Path.Combine(Application.persistentDataPath, CacheRootFolderName);
			_taskManager = taskManager;
			_fileDataFetcher = new FileDataFetcher();
			MapIdToFolderNameDictionary = new Dictionary<string, string>();

			TestAvailability();
		}

		public bool TestAvailability()
		{
			if (!Directory.Exists(PersistantCacheRootFolderPath))
			{
				Directory.CreateDirectory(PersistantCacheRootFolderPath);
			}

			if (!Directory.Exists(PersistantCacheRootFolderPath))
				return false;

			try
			{
				string filePath = Path.Combine(PersistantCacheRootFolderPath, "MapboxTestFie.txt");
				string content = "Mapbox";
				File.WriteAllText(filePath, content);
				string actualContent = File.ReadAllText(filePath);
				if (actualContent == content)
				{
					File.Delete(filePath);
					return true;
				}
				else
				{
					return false;
				}

			}
			catch
			{
				//throw;
				return false;
			}
		}

		private string TileToFilePath(CanonicalTileId tileId, string tilesetId)
		{
			return Path.GetFullPath(string.Format("{0}/{1}/{2}{3}{4}.{5}", PersistantCacheRootFolderPath, MapIdToFolderName(tilesetId), tileId.X, tileId.Y, tileId.Z, FileExtension));
		}
		
		public virtual bool Exists(CanonicalTileId tileId, string mapId)
		{
			string filePath = TileToFilePath(tileId, mapId);
			return File.Exists(filePath);
		}

		public virtual void Add(MapboxTileData textureCacheItem, bool forceInsert, Action<string> postSave)
		{
			var filePath = TileToPath(textureCacheItem);
			var infoWrapper = new InfoWrapper(textureCacheItem, filePath, postSave);
			SaveInfo(infoWrapper);
		}

		public virtual bool GetAsync<T>(CanonicalTileId tileId, string tilesetId, bool isTextureNonreadable, Action<T> callback) where T : RasterData, new()
		{
			string fullFilePath = TileToFilePath(tileId, tilesetId);
			var fileExists = File.Exists(fullFilePath);
			if (fileExists)
			{

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
				fullFilePath = fullFilePath.Insert(0, "file://");
#endif
				fullFilePath = new Uri(fullFilePath).ToString();
				_fileDataFetcher.FetchData<T>(fullFilePath, tileId, tilesetId, isTextureNonreadable, callback);
			}

			return fileExists;
		}
		
		public virtual IEnumerator GetFileCoroutine<T>(CanonicalTileId tileId, string tilesetId, bool isTextureNonreadable, Action<T> callback) where T : RasterData, new()
		{
			string fullFilePath = TileToFilePath(tileId, tilesetId);
			var fileExists = File.Exists(fullFilePath);
			if (fileExists)
			{

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
				fullFilePath = fullFilePath.Insert(0, "file://");
#endif
				fullFilePath = new Uri(fullFilePath).ToString();
				var finished = false;
				yield return _fileDataFetcher.FetchDataCoroutine<T>(fullFilePath, tileId, tilesetId,
					isTextureNonreadable,
					(data) =>
					{
						finished = true;
						callback(data);
					});
				while (!finished) yield return null;
			}
			else
			{
				callback(null);
			}
		}

		public virtual void ClearAll()
		{
			DirectoryInfo di = new DirectoryInfo(PersistantCacheRootFolderPath);

			foreach (DirectoryInfo folder in di.GetDirectories())
			{
				ClearFolder(folder.FullName);
			}
		}

		public virtual void DeleteTileFile(MapboxTileData cacheItem)
		{
			var filePath = TileToPath(cacheItem);
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
		}

		public virtual HashSet<string> GetFileList()
		{
			var pathList = new HashSet<string>();
			if (Directory.Exists(PersistantCacheRootFolderPath))
			{
				var dir = Directory.GetDirectories(FileCache.PersistantCacheRootFolderPath);
				foreach (var rasterDirectory in dir)
				{
					var directoryInfo = new DirectoryInfo(rasterDirectory);
					var files = directoryInfo.GetFiles();
					foreach (var fileInfo in files)
					{
						pathList.Add(fileInfo.FullName);
					}
				}
			}

			return pathList;
		}

		protected virtual void SaveInfo(InfoWrapper info)
		{
			if (info.TextureCacheItem == null)
			{
				return;
			}

			string folderPath = string.Format("{0}/{1}", PersistantCacheRootFolderPath, MapIdToFolderName(info.TextureCacheItem.TilesetId));

			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}

			
			//info.TextureCacheItem.FilePath = Path.GetFullPath(string.Format("{0}/{1}/{2}.{3}", PersistantCacheRootFolderPath, MapIdToFolderName(info.TilesetId), info.TileId.GenerateKey(info.TilesetId), FileExtension));

			_taskManager.AddTask(
				new TaskWrapper(info.TextureCacheItem.TileId.GenerateKey(info.TextureCacheItem.TilesetId, "FileCache"))
				{
					TileId = info.TextureCacheItem.TileId,
					TilesetId = info.TextureCacheItem.TilesetId,
					Action = () =>
					{
						FileStream sourceStream = new FileStream(
							info.Path,
							FileMode.Create, FileAccess.Write, FileShare.Read,
							bufferSize: 4096, useAsync: false);

						sourceStream.Write(info.TextureCacheItem.Data, 0, info.TextureCacheItem.Data.Length);
						sourceStream.Close();

						info.PostSaveAction(info.Path);
						//Debug.Log(string.Format("File saved {0} - {1}", info.TextureCacheItem.TileId, info.Path));
						OnFileSaved(info.TextureCacheItem, info.Path);
						//this is not a good way to do it
						// #if UNITY_EDITOR
						// 					FileCacheDebugView.AddToLogs(string.Format("Saved {0, 20} - {1, -20}", info.TilesetId, info.TileId));
						// #endif
						// },
						// ContinueWith = (t) =>
						// {
						
					},
					ContinueWith = (t) =>
					{
						
					},
#if UNITY_EDITOR
					Info = "FileCache.SaveInfo"
#endif
				}, 4);
		}

		private string TileToPath(MapboxTileData cacheItem)
		{
			return TileToFilePath(cacheItem.TileId, cacheItem.TilesetId); //Path.GetFullPath(string.Format("{0}/{1}/{2}.{3}", PersistantCacheRootFolderPath, MapIdToFolderName(caheItem.TilesetId), caheItem.TileId.GenerateKey(caheItem.TilesetId), FileExtension));
		}

		protected virtual void OnFileSaved(MapboxTileData infoTextureCacheItem, string path)
		{
			FileSaved(infoTextureCacheItem, path);
		}

		private string MapIdToFolderName(string mapId)
		{
			if (MapIdToFolderNameDictionary.ContainsKey(mapId))
			{
				return MapIdToFolderNameDictionary[mapId];
			}
			var folderName = mapId;
			var chars = Path.GetInvalidFileNameChars();
			foreach (Char c in chars)
			{
				folderName = folderName.Replace(c, '-');
			}
			MapIdToFolderNameDictionary.Add(mapId, folderName);
			return folderName;
		}

		private void ClearFolder(string folderPath)
		{
			DirectoryInfo di = new DirectoryInfo(folderPath);

			foreach (FileInfo file in di.GetFiles())
			{
				file.Delete();
			}

			di.Delete();
		}

		protected class InfoWrapper
		{
			public MapboxTileData TextureCacheItem;
			public string Path;
			public Action<string> PostSaveAction;

			public InfoWrapper(MapboxTileData textureCacheItem, string path, Action<string> postSave)
			{
				TextureCacheItem = textureCacheItem;
				Path = path;
				PostSaveAction = postSave;
			}
		}
	}
}
