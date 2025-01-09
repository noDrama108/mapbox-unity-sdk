using System.IO;
using Mapbox.BaseModule.Utilities;
using NUnit.Framework;

namespace Mapbox.BaseModuleTests
{
    public class IosTests
    {
        [Test]
        public void MapboxCommonPath()
        {
            var dataPath = UnityEngine.Application.dataPath;
            var path = Path.Combine(dataPath, Constants.Path.IOS_MAPBOXCOMMON_XCFRAMEWORK_SOURCE_RELATIVEPATH);
            Assert.IsTrue(Directory.Exists(path));
        }
    }
}