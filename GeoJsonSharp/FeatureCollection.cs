using System.Collections.Generic;

namespace GeoJsonSharp
{
	class FeatureCollection : GeoJsonObject
	{
		public readonly List<Feature> Features = new List<Feature>();
	}
}
