using Newtonsoft.Json;

namespace GeoJsonSharp.Serialize
{
	public class BaseSerializer
	{
		protected readonly JsonTextWriter Writer;
		protected readonly ParserSettings Settings;

		protected BaseSerializer(JsonTextWriter writer, ParserSettings settings)
		{
			Writer = writer;
			Settings = settings;
		}
	}
}
