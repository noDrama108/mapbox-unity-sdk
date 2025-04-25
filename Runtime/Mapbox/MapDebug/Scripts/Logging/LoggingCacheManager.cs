using Mapbox.BaseModule.Data.Platform.Cache;
using Mapbox.BaseModule.Data.Platform.Cache.SQLiteCache;
using Mapbox.BaseModule.Unity;

namespace Mapbox.MapDebug.Scripts.Logging
{
    public class LoggingCacheManager : MapboxCacheManager
    {
        public ISqliteCache SqLiteCache => _sqLiteCache;
        
        public LoggingCacheManager(UnityContext unityContext, MemoryCache memoryCache, FileCache fileCache = null, SqliteCache cache = null) : base(unityContext, memoryCache, fileCache, cache)
        {
        }
    }
}