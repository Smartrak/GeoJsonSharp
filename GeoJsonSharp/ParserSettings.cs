using System;
using NetTopologySuite.Features;

namespace GeoJsonSharp
{
	public class ParserSettings
	{
		/// <summary>
		/// If true, any linestrings with not enough points will be skipped instead of making an exception
		/// </summary>
		public bool SkipInvalidGeometry;
		
		/// <summary>
		/// Provides the IAttributesTable implementation to use, defaults to AttributesTable
		/// </summary>
		public Func<IAttributesTable> AttributesTableFactory = () => new AttributesTable();
	}
}
