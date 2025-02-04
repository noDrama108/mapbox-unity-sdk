using System;
using System.Collections;
using Mapbox.BaseModule.Data.Interfaces;
using Mapbox.BaseModule.Data.Vector2d;
using Mapbox.BaseModule.Unity;
using Mapbox.BaseModule.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mapbox.BaseModule.Map
{
    [Serializable]
    public sealed class MapboxMap
    {
        [NonSerialized] public IMapInformation mapInformation;
        [NonSerialized] public UnityContext UnityContext;
        [NonSerialized] public IMapVisualizer MapVisualizer;
        [NonSerialized] public TileCover TileCover;
        [NonSerialized] public InitializationStatus Status = InitializationStatus.WaitingForInitialization;
        
        private MapService _mapService;
        
        public MapboxMap(IMapInformation information, UnityContext unityContext, MapService mapService)
        {
            mapInformation = information;
            UnityContext = unityContext;
            TileCover = new TileCover();
            _mapService = mapService;
        }

        public IEnumerator Initialize()
        {
            if (Status != InitializationStatus.WaitingForInitialization)
                yield break;

            Status = InitializationStatus.Initializing;
            yield return MapVisualizer.Initialize();
            Status = InitializationStatus.Initialized;
            Initialized();
            
        }

        public void MapUpdated()
        {
            _mapService.TileCover(mapInformation, TileCover);
            MapVisualizer.Load(TileCover);
        }

        public void LoadMapView(Action callback)
        {
            Runnable.Instance.StartCoroutine(LoadMapViewCoroutine(callback));
        }

        public IEnumerator LoadMapViewCoroutine(Action callback)
        {
            var tileCover = new TileCover();
            _mapService.TileCover(mapInformation, tileCover);
            yield return MapVisualizer.LoadTileCoverToMemory(tileCover);
            if (Status == InitializationStatus.Initialized)
            {
                Status = InitializationStatus.ViewLoaded;
                OnFirstViewCompleted();
            }

            callback();
            Status = InitializationStatus.ReadyForUpdates;
        }

        public void ChangeView(LatitudeLongitude? latlng = null, float? zoom = null, float? pitch = null, float? bearing = null)
        {
            mapInformation.SetInformation(latlng, zoom, pitch, bearing);
        }
        
        public Action Initialized = () => {};
        public Action OnFirstViewCompleted = () => { };
        
        public void OnDestroy()
        {
            MapVisualizer?.OnDestroy();
            _mapService.OnDestroy();
        }

        public void UpdateTileCover() => _mapService.TileCover(mapInformation, TileCover);
        public MapService GetMapService() => _mapService;
    }
}

