using System;
using Mapbox.BaseModule.Map;
using UnityEngine;

namespace Mapbox.BaseModule.Utilities
{
    public class MapBehaviourCore : MonoBehaviour
    {
        [NonSerialized] public MapboxMap MapboxMap = null;
        public InitializationStatus InitializationStatus => MapboxMap != null
            ? MapboxMap.Status
            : InitializationStatus.WaitingForInitialization;
        public Action<MapboxMap> Initialized = (m) => { };
    }
}
