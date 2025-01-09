using System;
using Mapbox.BaseModule.Data.Interfaces;
using Mapbox.BaseModule.Map;

namespace Mapbox.ImageModule
{
    [Serializable]
    public class StaticLayerModuleSettings
    {
        public ImagerySourceType SourceType;
        public string CustomSourceId;
        public bool LoadBackgroundTextures = false;
        public ImageSourceSettings DataSettings;
    }
}