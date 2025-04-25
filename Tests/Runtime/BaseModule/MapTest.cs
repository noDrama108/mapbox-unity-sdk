using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mapbox.BaseModule.Data.DataFetchers;
using Mapbox.BaseModule.Data.Interfaces;
using Mapbox.BaseModule.Data.Platform.Cache;
using Mapbox.BaseModule.Data.Platform.Cache.SQLiteCache;
using Mapbox.BaseModule.Data.Tasks;
using Mapbox.BaseModule.Data.Vector2d;
using Mapbox.BaseModule.Map;
using Mapbox.BaseModule.Unity;
using Mapbox.BaseModule.Utilities;
using Mapbox.ImageModule;
using Mapbox.ImageModule.Terrain;
using Mapbox.ImageModule.Terrain.TerrainStrategies;
using Mapbox.MapDebug.Scripts.Logging;
using Mapbox.UnityMapService;
using Mapbox.UnityMapService.DataSources;
using Mapbox.UnityMapService.TileProviders;
using Mapbox.VectorModule;
using Mapbox.VectorModule.MeshGeneration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TerrainData = Mapbox.BaseModule.Data.DataFetchers.TerrainData;

public class MapTest : MonoBehaviour
{
    private string _helsinkiLatitudeLongitudeString = "60.1734031,24.9428875";
    private string _sanFranciscoLatitudeLongitudeString = "60.1734031,24.9428875";
    private LatitudeLongitude _helsinkiLatLng;
    private LatitudeLongitude _sfLatLng;
    
    private MapboxMap _map;
    private LoggingCacheManager _cacheManager;
    private MemoryCache _memoryCache;
    private UnitySource<RasterData> _imageSource;
    private UnitySource<TerrainData> _terrainSource;
    private UnitySource<VectorData> _vectorSource;
    private StaticApiLayerModule _imageLayer;
    private TerrainLayerModule _terrainLayer;
    private VectorLayerModule _vectorLayer;
    private LoggingDataFetchingManager _dataManager;

    // [UnitySetUp]
    // public IEnumerator OneTimeSetUp()
    // {
    //     // LoadMap(_helsinkiLatitudeLongitudeString);
    //     // yield return _map.Initialize();
    // }

    private void LoadMap(string latlng)
    {
        var mapInfo = new MapInformation(latlng);
        mapInfo.SetInformation(null, 16, 45, null, 1000);
        mapInfo.Initialize();
        var mapboxContext = new MapboxContext();
        var unityContext = new UnityContext();
        unityContext.Initialize();

        var taskManager = unityContext.TaskManager;
        _dataManager = new LoggingDataFetchingManager(mapboxContext.GetAccessToken(), mapboxContext.GetSkuToken);

        var sqliteCache = new MockSqliteCache(taskManager);
        sqliteCache.ReadySqliteDatabase();

        _memoryCache = new MemoryCache();
        _cacheManager = new LoggingCacheManager(
            unityContext,
            _memoryCache,
            new MockFileCache(taskManager),
            sqliteCache);

        var mapService = new MapUnityService(
            unityContext,
            mapboxContext,
            new UnityFixedAreaTileProvider(),
            _cacheManager as MapboxCacheManager,
            _dataManager);

        _map = new MapboxMap(mapInfo, unityContext, mapService);
        var mapVisualizer = new MapboxMapVisualizer(mapInfo, unityContext,
            new TileCreator(unityContext, new FlatTerrainStrategy()));
        _map.MapVisualizer = mapVisualizer;

        var imageryTileset = MapboxDefaultImagery.GetParameters(ImagerySourceType.MapboxSatellite);
        var terrainTileset = MapboxDefaultElevation.GetParameters(ElevationSourceType.MapboxTerrain);
        var vectorTileset = MapboxDefaultVector.GetParameters(VectorSourceType.MapboxStreetsV8);

        var terrainSettings = new TerrainLayerModuleSettings();
        terrainSettings.ElevationLayerType = ElevationLayerType.TerrainWithElevation;
        terrainSettings.DataSettings = new ImageSourceSettings()
        {
            TilesetId = terrainTileset.Id,
            ClampDataLevelToMax = 14
        };
        var vectorSourceSettings = new VectorSourceSettings()
        {
            TilesetId = vectorTileset.Id,
            ClampDataLevelToMax = 15
        };

        _imageSource = (UnitySource<RasterData>)mapService.GetStaticRasterSource(new ImageSourceSettings() { TilesetId = imageryTileset.Id });
        _terrainSource = (UnitySource<TerrainData>)mapService.GetTerrainRasterSource(terrainSettings.DataSettings);
        _vectorSource = (UnitySource<VectorData>)mapService.GetVectorSource(vectorSourceSettings);
        _imageLayer = new StaticApiLayerModule(_imageSource, new StaticLayerModuleSettings());
        _terrainLayer = new TerrainLayerModule(_terrainSource, terrainSettings);
        _vectorLayer = new VectorLayerModule(
            mapInfo,
            _vectorSource,
            new MeshGenerationUnit(unityContext, new Dictionary<string, IVectorLayerVisualizer>()),
            new VectorModuleSettings() { DataSettings = vectorSourceSettings });
        mapVisualizer.LayerModules.Add(_imageLayer);
        mapVisualizer.LayerModules.Add(_terrainLayer);
        mapVisualizer.LayerModules.Add(_vectorLayer);
    }

    [UnityTest]
    public IEnumerator LoadMapView()
    {
        LoadMap(_helsinkiLatitudeLongitudeString);
        
        //expire all tiles in db
        var sqliteCache = _cacheManager.SqLiteCache as SqliteCache;
        foreach (var tile in sqliteCache.GetAllTiles())
        {
            tile.expirationDate = 0;
            sqliteCache.UpdateTile(tile);
        }
        
        yield return _map.Initialize();
        
        Runnable.EnableRunnableInEditor();
        yield return _map.LoadMapViewCoroutine();
        yield return new WaitForSeconds(2);
        
        //var tiles = _map.TileCover.Tiles.Select(x => x.Canonical);
        // Assert.IsTrue(_terrainLayer.GetDataId(tiles).All(x => _terrainSource.GetInstantData(x, out var td)));
        // Assert.IsTrue(tiles.All(x => _imageSource.GetInstantData(x, out var td)));
        // Assert.IsTrue(_vectorLayer.GetDataId(tiles).All(x => _vectorSource.GetInstantData(x, out var td)));

        var updatedTileCount = 0;
        foreach (var record in _dataManager.Records)
        {
            if (record.Key.IsUpdate)
                updatedTileCount++;
        }
        
        Assert.IsTrue(_map.Status >= InitializationStatus.Initialized);
        Assert.AreNotEqual(_map.TileCover.Tiles.Count, 0);
        Assert.AreNotEqual(updatedTileCount, 0);
    }
}