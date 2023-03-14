using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FishingMap.Domain.Extensions
{
    public static class NetTopologySuiteExtensions
    {
        public static Polygon GeoJsonFeatureToPolygon(this GeometryFactory factory, string geoJson)
        {
            var serializer = NetTopologySuite.IO.GeoJsonSerializer.Create(factory);
            var feature = serializer.Deserialize<IFeature>(new JsonTextReader(new StringReader(geoJson)));

            var polygon = feature.Geometry as Polygon;
            if (!polygon.Shell.IsCCW)
            {
                polygon = polygon.Reverse() as Polygon;
            }

            return polygon;
        }

        public static MultiPolygon GeoJsonFeatureToMultiPolygon(this GeometryFactory factory, string geoJson)
        {
            var serializer = NetTopologySuite.IO.GeoJsonSerializer.Create(factory);
            var feature = serializer.Deserialize<IFeature>(new JsonTextReader(new StringReader(geoJson)));

            var polygons = new List<Polygon>();

            var multiPolygon = feature.Geometry as MultiPolygon;
            foreach (var geometry in multiPolygon.Geometries)
            {
                var polygon = geometry as Polygon;
                if (!polygon.Shell.IsCCW)
                {
                    polygon = polygon.Reverse() as Polygon;
                }
                polygons.Add(polygon);
            }

            return new MultiPolygon(polygons.ToArray(), factory);
        }

        public static MultiPolygon GeoJsonFeatureCollectionToMultiPolygon(this GeometryFactory factory, string geoJson)
        {
            var serializer = NetTopologySuite.IO.GeoJsonSerializer.Create(factory);
            var features = serializer.Deserialize<FeatureCollection>(new JsonTextReader(new StringReader(geoJson)));
            
            var feature = features.FirstOrDefault();

            if (feature == null)
            {
                return null;
            }

            var polygons = new List<Polygon>();

            var multiPolygon = feature.Geometry as MultiPolygon;
            foreach (var geometry in multiPolygon.Geometries)
            {
                var polygon = geometry as Polygon;
                if (!polygon.Shell.IsCCW)
                {
                    polygon = polygon.Reverse() as Polygon;
                }
                polygons.Add(polygon);
            }

            return new MultiPolygon(polygons.ToArray(), factory);
        }

        public static bool HasSameCoordinates(this Geometry a, Geometry b)
        {
            if (a.Coordinates.Length != b.Coordinates.Length)
            {
                return false;
            }
            
            for (int i = 0; i < a.Coordinates.Length; i++)
            {
                var aCoord = a.Coordinates[i];
                var bCoord = b.Coordinates[i];

                if (aCoord.X != bCoord.X || aCoord.Y != bCoord.Y)
                {
                    return false;
                }
            }

            return true;
        }

        public static string ToGeoJsonFeature(this Point point)
        {
            throw new NotImplementedException();
        }

        public static string ToGeoJsonFeature(this Polygon polygon)
        {
            Feature feature = new Feature();
            feature.Geometry = polygon;

            var writer = new GeoJsonWriter();
            var geoJson = writer.Write(feature);

            return geoJson;
        }

        public static string ToGeoJsonFeature(this MultiPolygon multiPolygon)
        {
            Feature feature = new Feature();           
            feature.Geometry = multiPolygon;

            var writer = new GeoJsonWriter();
            var geoJson = writer.Write(feature);

            return geoJson;
        }

        public static IFeature ToFeature(this MultiPolygon multiPolygon)
        {
            Feature feature = new Feature() { Geometry = multiPolygon };
            return feature;
        }

        public static string ToGeoJsonFeatureCollection(this MultiPolygon multiPolygon)
        {
            FeatureCollection features= new FeatureCollection();
            Feature feature= new Feature() { Geometry = multiPolygon };
            features.Add(feature);

            var writer = new GeoJsonWriter();
            var geoJson = writer.Write(features);

            return geoJson;
        }

        public static Point CreatePoint(this GeometryFactory factory, double x, double y)
        {
            return factory.CreatePoint(new Coordinate(x, y));
        }
    }
}
