using Mapbox.BaseModule.Utilities;
using UnityEngine;

namespace Mapbox.VectorModule.Filters
{
	public class FilterBaseObject : ScriptableObject, ILayerFeatureFilterComparer
	{
		public virtual bool Try(VectorFeatureUnity feature)
		{
			return true;
		}
	}
	
	public class FilterBase : ILayerFeatureFilterComparer
	{
		public virtual string Key { get { return ""; } }

		public virtual bool Try(VectorFeatureUnity feature)
		{
			return true;
		}
	}
}