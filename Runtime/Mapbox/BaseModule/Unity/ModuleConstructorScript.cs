using Mapbox.BaseModule.Data.Interfaces;
using Mapbox.BaseModule.Map;
using UnityEngine;

namespace Mapbox.BaseModule.Unity
{
	public abstract class ModuleConstructorScript : MonoBehaviour
	{
		public abstract ILayerModule ConstructModule(MapService service, IMapInformation mapInformation,
			UnityContext unityContext);

		public virtual void OnDestroy()
		{
			
		}
	}
}