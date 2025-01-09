using Mapbox.BaseModule.Data.DataFetchers;
using Mapbox.BaseModule.Data.Platform.Cache;
using Mapbox.BaseModule.Data.Tiles;
using Mapbox.BaseModule.Map;
using UnityEngine;
using TerrainData = Mapbox.BaseModule.Data.DataFetchers.TerrainData;

namespace Mapbox.UnityMapService.DataSources
{
    public class TerrainSource : ImageSource<TerrainData>
    {
        protected ImageSourceSettings _settings;
        private IElevationDataExtractionStrategy _elevationDataExtractionStrategy;
        
        public TerrainSource(DataFetchingManager dataFetchingManager, MapboxCacheManager mapboxCacheManager, ImageSourceSettings settings) 
            : base(dataFetchingManager, mapboxCacheManager, settings)
        {
            _settings = settings;
            _elevationDataExtractionStrategy = SystemInfo.supportsAsyncGPUReadback
                ? (IElevationDataExtractionStrategy) new AsyncExtractElevationArray()
                : (IElevationDataExtractionStrategy) new SyncExtractElevationArray();
        }
        
        protected override RasterTile CreateTile(CanonicalTileId tileId, string tilesetId)
        {
            RasterTile rasterTile;

            // //TODO fix this obviously
            if (tilesetId == "mapbox.mapbox-terrain-dem-v1")
            {
                if (SystemInfo.supportsAsyncGPUReadback)
                {
                    rasterTile = new DemRasterTile(tileId, tilesetId, true);
                }
                else
                {
                    rasterTile = new DemRasterTile(tileId, tilesetId, false);
                }

            }
            else
            {
                if (SystemInfo.supportsAsyncGPUReadback)
                {
                    rasterTile = new RawPngRasterTile(tileId, tilesetId, true);
                }
                else
                {
                    rasterTile = new RawPngRasterTile(tileId, tilesetId, false);
                }
            }

            return rasterTile;
        }

        protected override TerrainData CreateRasterDataWrapper(RasterTile tile)
        {
            TerrainData rasterData = new TerrainData()
            {
                TileId = tile.Id,
                TilesetId = tile.TilesetId,
                Texture = tile.Texture2D,
                CacheType = tile.FromCache,
                Data = tile.Data,
                ETag = tile.ETag,
                ExpirationDate = tile.ExpirationDate
            };

            return rasterData;
        }

        protected override void TextureReceivedFromFile(TerrainData cacheItem)
        {
            base.TextureReceivedFromFile(cacheItem);
            _elevationDataExtractionStrategy.ExtractHeightData(cacheItem.Texture, (elevationArray) =>
            {
                cacheItem.SetElevationValues(elevationArray);
            });
        }

        protected override TerrainData TextureReceivedFromWeb(RasterTile tile)
        {
            var cacheItem = base.TextureReceivedFromWeb(tile);
            
            _elevationDataExtractionStrategy.ExtractHeightData(cacheItem.Texture, (elevationArray) =>
            {
                cacheItem.SetElevationValues(elevationArray);
            });

            return cacheItem;
        }
    }
}