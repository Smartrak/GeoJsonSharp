using System.Collections.Generic;

namespace GeoJsonSharp
{
	public class FeatureCollection : GeoJsonObject
	{
		public readonly List<Feature> Features = new List<Feature>();
	}
}
