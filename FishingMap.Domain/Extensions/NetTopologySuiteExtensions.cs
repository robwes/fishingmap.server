using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
            var builder = new StringBuilder();
            builder.Append(@"{""type"":""Feature"",""geometry"":{""type"":""Polygon"",""coordinates"":[[");
            
            foreach (var c in polygon.Coordinates)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "[{0}, {1}],", c.X, c.Y);
            }
            builder.Length--;
            builder.Append("]]}}");

            return builder.ToString();
        }

        public static string ToGeoJsonFeature(this MultiPolygon multiPolygon)
        {
            var builder = new StringBuilder();
            builder.Append(@"{""type"":""Feature"",""geometry"":{""type"":""MultiPolygon"",""coordinates"":[");

            foreach (var p in multiPolygon.Geometries)
            {
                builder.Append("[[");
                foreach (var c in p.Coordinates)
                {
                    builder.AppendFormat(CultureInfo.InvariantCulture, "[{0}, {1}],", c.X, c.Y);
                }
                builder.Length--;
                builder.Append("]],");
            }
            builder.Length--;
            builder.Append("]}}");

            return builder.ToString();
        }

        public static Point CreatePoint(this GeometryFactory factory, double x, double y)
        {
            return factory.CreatePoint(new Coordinate(x, y));
        }
    }
}
