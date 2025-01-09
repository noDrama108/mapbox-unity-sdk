using System.Collections;
using System.Collections.Generic;
using Mapbox.BaseModule;
using Mapbox.BaseModule.Data.Interfaces;
using Mapbox.BaseModule.Map;
using Mapbox.BaseModule.Unity;
using Mapbox.BaseModule.Utilities;
using Mapbox.ImageModule;

namespace Mapbox.Example.Scripts.ModuleBehaviours
{
	public class StaticApiLayerModuleScript : ModuleConstructorScript
	{
		public StaticLayerModuleSettings Settings;
		private ILayerModule _layerModuleImplementation;

		private void Start()
		{
			
		}

		public override ILayerModule ConstructModule(MapService service, IMapInformation mapInformation,
			UnityContext unityContext)
		{
			if (Settings.SourceType == ImagerySourceType.None)
			{

			}
			else if (Settings.SourceType == ImagerySourceType.Custom)
			{
				Settings.DataSettings.TilesetId = Settings.CustomSourceId;
			}
			else
			{
				var imageryTileset = MapboxDefaultImagery.GetParameters(Settings.SourceType);
				Settings.DataSettings.TilesetId = imageryTileset.Id;
			}

			_layerModuleImplementation = new StaticApiLayerModule(service.GetStaticRasterSource(Settings.DataSettings), Settings);
			return _layerModuleImplementation;
		}
	}
}