using System.Collections.Generic;
using GeoAPI.Geometries;

namespace GeoJsonSharp
{
	class Feature : GeoJsonObject
	{
		public readonly Dictionary<string, object> Properties = new Dictionary<string, object>();

		public IGeometry Geometry;
	}
}
