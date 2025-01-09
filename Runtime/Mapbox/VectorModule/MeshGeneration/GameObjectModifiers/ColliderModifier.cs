using System;
using Mapbox.BaseModule.Map;
using Mapbox.BaseModule.Utilities;
using UnityEngine;

namespace Mapbox.VectorModule.MeshGeneration.GameObjectModifiers
{
	[Serializable]
	public class ColliderModifier : GameObjectModifier
	{
		public override void Run(VectorEntity ve, IMapInformation mapInformation)
		{
			ve.GameObject.AddComponent<MeshCollider>().sharedMesh = ve.Mesh;
		}
	}
}
