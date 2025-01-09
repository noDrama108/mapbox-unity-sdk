using System;
using System.Collections;
using System.Collections.Generic;
using Mapbox.BaseModule.Data.DataFetchers;
using Mapbox.BaseModule.Data.Tiles;
using Mapbox.BaseModule.Unity;

namespace Mapbox.BaseModule.Map
{
    public abstract class LayerModule
    {
        //public abstract void RegisterTile(UnityMapTile tile);
        //public abstract void UnregisterTile(UnityMapTile unityTile);
        public abstract bool LoadInstant(UnityMapTile unityTile);
        //public abstract void LoadDataFor(UnityMapTile unityTile);
        
        public LayerModule()
        {
        }

        public virtual IEnumerator Initialize()
        {
            return null;
        }
        
        public abstract bool RetainTiles(HashSet<CanonicalTileId> retainedTiles, Dictionary<UnwrappedTileId, UnityMapTile> activeTiles);

        public virtual void LoadTempTile(UnityMapTile tile)
        {
            
        }

        public virtual IEnumerator LoadTileData(CanonicalTileId tileId, Action<MapboxTileData> callback = null)
        {
            yield break;
        }

        public virtual IEnumerator ProcessTileData(CanonicalTileId tileId)
        {
            yield break;
        }
        
        public virtual IEnumerator LoadTiles(IEnumerable<CanonicalTileId> canonicalTileIds)
        {
            yield break;
        }

        public virtual void UnloadTile(CanonicalTileId tileId)
        {
            
        }

        public virtual void UpdatePositioning(IMapInformation mapInfo)
        {
			
        }
        
        public virtual void OnDestroy()
        {
            
        }

        
    }
}