using System;
using Mapbox.BaseModule.Utilities;
using UnityEngine;

namespace Mapbox.VectorModule.Filters
{
	[CreateAssetMenu(menuName = "Mapbox/Modifiers/Type Filter")]
	public class TypeFilterObject : FilterBaseObject
	{
		public TypeFilter TypeFilter;
		
		public override bool Try(VectorFeatureUnity feature)
		{
			return TypeFilter.Try(feature);
		}
	}
	
	[Serializable]
	public class TypeFilter : FilterBase
	{
		public override string Key { get { return "type"; } }
		[SerializeField]
		private string[] _types;
		[SerializeField]
		private TypeFilterType _behaviour;

		public override bool Try(VectorFeatureUnity feature)
		{
			var check = false;
			for (int i = 0; i < _types.Length; i++)
			{
				if (_types[i].ToLowerInvariant() == feature.Properties["type"].ToString().ToLowerInvariant())
				{
					check = true;
				}
			}
			return _behaviour == TypeFilterType.Include ? check : !check;
		}

		public enum TypeFilterType
		{
			Include,
			Exclude
		}
	}
}
