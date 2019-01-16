using GeoAPI.Geometries;
using NetTopologySuite.Features;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace FishingMap.Domain.Data.Extensions
{
    public static class NetTopologySuiteExtensions
    {
        public static IPolygon GeoJsonFeatureToPolygon(this IGeometryFactory factory, string geoJson)
        {
            var serializer = NetTopologySuite.IO.GeoJsonSerializer.Create(factory);
            var feature = serializer.Deserialize<IFeature>(new JsonTextReader(new StringReader(geoJson)));

            var polygon = feature.Geometry as IPolygon;
            if (!polygon.Shell.IsCCW)
            {
                polygon = polygon.Reverse() as IPolygon; //new Polygon(new LinearRing(polygon.Coordinates.Reverse().ToArray()) { SRID = 4326 });
            }

            return polygon;
        }

        public static bool HasSameCoordinates(this IPolygon a, IPolygon b)
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

        public static string ToGeoJsonFeature(this IPoint point)
        {
            throw new NotImplementedException();
        }

        public static string ToGeoJsonFeature(this IPolygon polygon)
        {
            var builder = new StringBuilder();
            builder.Append(@"{""type"":""Feature"",""geometry"":{""type"":""Polygon"",""coordinates"":[[");

            foreach (var p in polygon.Coordinates)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "[{0}, {1}],", p.X, p.Y);
            }
            builder.Length--;
            builder.Append("]]}}");

            return builder.ToString();

        }

        public static IPoint CreatePoint(this IGeometryFactory factory, double x, double y)
        {
            return factory.CreatePoint(new Coordinate(x, y));
        }
    }
}
