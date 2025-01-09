using System;
using System.Collections;
using System.Collections.Generic;
using Mapbox.BaseModule.Data.DataFetchers;
using Mapbox.BaseModule.Data.Interfaces;
using Mapbox.BaseModule.Data.Tiles;
using Mapbox.BaseModule.Map;
using Mapbox.BaseModule.Unity;
using UnityEngine;

namespace Mapbox.ImageModule
{
	public class StaticApiLayerModule : LayerModule, ILayerModule
	{
		protected StaticLayerModuleSettings _settings;
		protected Source<RasterData> _rasterSource;
		private HashSet<CanonicalTileId> _retainedTiles;

		public StaticApiLayerModule(Source<RasterData> source, StaticLayerModuleSettings settings) : base()
		{
			_settings = settings;
			_rasterSource = source;
		}

		public override IEnumerator Initialize()
		{
			yield return base.Initialize();
			yield return _rasterSource.Initialize();
			if (_settings.LoadBackgroundTextures)
			{
				_rasterSource.DownloadAndCacheBaseTiles();
			}
		}
		
		public override void LoadTempTile(UnityMapTile unityTile)
		{
			var parentTileId = unityTile.CanonicalTileId;
			for (int i = unityTile.CanonicalTileId.Z; i >= 2; i--)
			{
				parentTileId = parentTileId.Parent;
				if (_rasterSource.GetInstantData(parentTileId, out var instantData))
				{
					unityTile.ImageContainer.SetImageData(instantData, TileContainerState.Temporary);
					return;
				}
			}
		}

		public override bool LoadInstant(UnityMapTile unityTile)
		{
			if (_rasterSource.GetInstantData(unityTile.CanonicalTileId, out var instantData))
			{
				unityTile.ImageContainer.SetImageData(instantData);
				return true;
			}
			return false;
		}
		
		public override bool RetainTiles(HashSet<CanonicalTileId> retainedTiles,
			Dictionary<UnwrappedTileId, UnityMapTile> activeTiles)
		{
			_retainedTiles = retainedTiles;
			var isReady = _rasterSource.RetainTiles(_retainedTiles);
			return isReady;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			_rasterSource.OnDestroy();
		}


		//COROUTINE METHODS only used in initialization so far
		#region coroutines
		public override IEnumerator LoadTileData(CanonicalTileId tileId, Action<MapboxTileData> callback) => _rasterSource.LoadTileCoroutine(tileId, callback);		
		public override IEnumerator LoadTiles(IEnumerable<CanonicalTileId> tiles)
		{
			yield return _rasterSource.LoadTilesCoroutine(tiles);
		}
		#endregion
	}
}