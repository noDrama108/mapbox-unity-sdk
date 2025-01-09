using System;
using Mapbox.BaseModule.Data.Interfaces;
using Mapbox.BaseModule.Map;

namespace Mapbox.VectorModule
{
    [Serializable]
    public class VectorModuleSettings
    {
        public VectorSourceType SourceType;
        public string CustomSourceId;
        public bool LoadBackgroundData = false;
        public VectorSourceSettings DataSettings;
    }
}