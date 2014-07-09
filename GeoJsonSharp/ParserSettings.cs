namespace GeoJsonSharp
{
	public class ParserSettings
	{
		/// <summary>
		/// If true, any linestrings with not enough points will be skipped instead of making an exception
		/// </summary>
		public bool SkipInvalidGeometry;
	}
}
