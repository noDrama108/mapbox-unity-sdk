using Mapbox.BaseModule.Utilities;

namespace Mapbox.VectorModule.Filters
{
    public interface ILayerFeatureFilterComparer
    {
        bool Try(VectorFeatureUnity feature);
    }
}