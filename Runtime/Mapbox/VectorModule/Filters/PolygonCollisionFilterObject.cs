using System;
using System.Collections.Generic;
using Mapbox.BaseModule.Data.Tiles;
using Mapbox.BaseModule.Map;
using Mapbox.BaseModule.Utilities;
using UnityEngine;

namespace Mapbox.VectorModule.Filters
{
    [CreateAssetMenu(menuName = "Mapbox/Modifiers/Mesh Collider Filter")]
    public class PolygonCollisionFilterObject : FilterBaseObject
    {
        public PolygonCollisionFilter PolygonCollisionFilter;
        
        public override bool Try(VectorFeatureUnity feature)
        {
            return PolygonCollisionFilter.Try(feature);
        }
    }
    
    //TODO this probably should work based on latlng values instead of tileID and vertex positions 
    [Serializable]
    public class PolygonCollisionFilter : FilterBase
    {
        [NonSerialized] public Dictionary<CanonicalTileId, List<List<Vector3>>> PolygonsPerTile = new Dictionary<CanonicalTileId, List<List<Vector3>>>();
        
        public override bool Try(VectorFeatureUnity feature)
        {
            if (PolygonsPerTile.TryGetValue(feature.TileId, out var colliders))
            {
                foreach (var collider in colliders)
                {
                    foreach (var submesh in feature.Points)
                    {
                        if (PolygonIntersection2D.ArePolygonsIntersecting(collider, submesh))
                            return false;
                    }
                }
            }
            return true;
        }

        public void AddMeshCollider(Transform tr, Mesh mesh, List<CanonicalTileId> tileList, IMapInformation mapInfo)
        {
            PolygonsPerTile.Clear();
            foreach (var tileId in tileList)
            {
                var vertices = new List<Vector3>();
                foreach (var vertex in mesh.vertices)
                {
                    var pos = tr.TransformPoint(vertex);
                    var latlng = mapInfo.ConvertPositionToLatLng(pos);
                    var zeroOnePosition = Conversions.LatitudeLongitudeToInTile01(latlng, tileId);
                    var localPosition = ZeroOneToLocal(zeroOnePosition);
                    vertices.Add(new Vector3(localPosition.x, 0, localPosition.y));
                }

                if(!PolygonsPerTile.ContainsKey(tileId))
                    PolygonsPerTile.Add(tileId, new List<List<Vector3>>());
                PolygonsPerTile[tileId].Add(vertices);
            }
        }

        private Vector2 ZeroOneToLocal(Vector2 pos)
        {
            return new Vector2(pos.x, -1 * (1 - pos.y));
        }
    }
    
    public class PolygonIntersection2D
    {
        public static bool ArePolygonsIntersecting(List<Vector3> polygon1, List<Vector3> polygon2)
        {
            return IsSeparatingAxisFound(polygon1, polygon2) == false && IsSeparatingAxisFound(polygon2, polygon1) == false;
        }

        private static bool IsSeparatingAxisFound(List<Vector3> polygonA, List<Vector3> polygonB)
        {
            // Iterate through each edge of polygonA
            for (int i = 0; i < polygonA.Count; i++)
            {
                // Get the current edge in the XZ plane
                Vector2 edge = new Vector2(
                    polygonA[(i + 1) % polygonA.Count].x - polygonA[i].x,
                    polygonA[(i + 1) % polygonA.Count].z - polygonA[i].z
                );

                // Find the axis perpendicular to the edge
                Vector2 axis = new Vector2(-edge.y, edge.x);

                // Project both polygons onto this axis
                (float minA, float maxA) = ProjectPolygonOnAxis(axis, polygonA);
                (float minB, float maxB) = ProjectPolygonOnAxis(axis, polygonB);

                // Check for gap
                if (maxA < minB || maxB < minA)
                {
                    // If there's a gap, then there's a separating axis
                    return true;
                }
            }
            return false;
        }

        private static (float min, float max) ProjectPolygonOnAxis(Vector2 axis, List<Vector3> polygon)
        {
            // Project the first point of the polygon onto the axis
            float min = Vector2.Dot(axis, new Vector2(polygon[0].x, polygon[0].z));
            float max = min;

            // Project the rest of the points
            for (int i = 1; i < polygon.Count; i++)
            {
                float projection = Vector2.Dot(axis, new Vector2(polygon[i].x, polygon[i].z));
                if (projection < min) min = projection;
                if (projection > max) max = projection;
            }

            return (min, max);
        }
    }
}