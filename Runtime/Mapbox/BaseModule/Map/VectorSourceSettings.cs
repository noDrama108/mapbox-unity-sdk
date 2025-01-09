using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mapbox.BaseModule.Map
{
    [Serializable]
    public class VectorSourceSettings
    {
        [NonSerialized] public string TilesetId;
        public int CacheSize = 100;
        public Vector2 LayerRange = new Vector2(12, 16);
    }
}