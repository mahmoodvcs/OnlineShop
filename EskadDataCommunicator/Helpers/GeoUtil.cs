using MahtaKala.Models;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace MahtaKala.Helpers
{
    public class GeoUtil
    {
        private static readonly GeometryFactory geometryFactory;

        static GeoUtil()
        {
            geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        }

        public static Point CreatePoint(ILocationModel location)
        {
            return geometryFactory.CreatePoint(new Coordinate(location.Lng, location.Lat));
        }
    }
}
