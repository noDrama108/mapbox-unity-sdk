using System;
using System.Collections.Generic;
using Mapbox.BaseModule.Data.DataFetchers;

namespace Mapbox.BaseModule.Data.Platform.Cache
{
	public interface IMemoryCache
	{
		void OnDestroy();
		TypeMemoryCache<T> RegisterType<T>(int cacheSize = 100) where T : MapboxTileData;
	}

	public class MemoryCache : IMemoryCache
	{
		private Dictionary<Type, ITypeCache> _subCaches;

		public MemoryCache()
		{
			_subCaches = new Dictionary<Type, ITypeCache>();
		}

		public TypeMemoryCache<T> RegisterType<T>(int cacheSize = 100) where T : MapboxTileData
		{
			var dataType = typeof(T);
			if (_subCaches.ContainsKey(dataType))
			{
				return (TypeMemoryCache<T>) _subCaches[dataType];
			}
			else
			{
				var subcache = new TypeMemoryCache<T>(cacheSize);
				_subCaches.Add(typeof(T), subcache);
				return subcache;
			}
		}

		public void OnDestroy()
		{
			foreach (var subCache in _subCaches)
			{
				subCache.Value.OnDestroy();
			}
		}
	}
}