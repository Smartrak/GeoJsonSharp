using System;
using System.IO;
using Newtonsoft.Json;

namespace GeoJsonSharp
{
	public class GeoJsonParser : BaseParser
	{
		private readonly GeometryParser _geometryParser;

		public GeoJsonParser(string json, ParserSettings settings)
			: base(new JsonTextReader(new StringReader(json)), settings)
		{
			_geometryParser = new GeometryParser(Reader, settings);
		}

		public FeatureCollection Parse()
		{
			return (FeatureCollection)ParseUnknown(false);
		}

		private GeoJsonObject ParseUnknown(bool startObjectAlreadyConsumed)
		{
			if (!startObjectAlreadyConsumed)
			{
				Reader.Read();
				Assert(Reader.TokenType == JsonToken.StartObject);
			}

			AssertRead(JsonToken.PropertyName);
			AssertValue("type");

			AssertRead(JsonToken.String);
			var type = (string)Reader.Value;

			switch (type.ToLower())
			{
				case "featurecollection":
					return ParseFeatureCollection();
				case "feature":
					return ParseFeature();
				default:
					throw new Exception("Don't know how to parse a " + type);
			}
		}

		private Feature ParseFeature()
		{
			AssertRead(JsonToken.PropertyName);
			AssertValue("properties");

			var res = new Feature();
			AssertRead(JsonToken.StartObject);

			//Now we read key/value pairs for the properties until we hit end object
			while (true)
			{
				//Should be a propertyname, or endobject (meaning we are done with properties)
				Reader.Read();
				if (Reader.TokenType == JsonToken.EndObject)
					break;
				Assert(Reader.TokenType == JsonToken.PropertyName);

				string property = (string)Reader.Value;

				AssertRead(new[] { JsonToken.Float, JsonToken.Integer, JsonToken.Null, JsonToken.String });

				var value = Reader.Value;

				res.Properties[property] = value;
			}

			//Now hopefully we hit the geometry
			AssertRead(JsonToken.PropertyName);
			AssertValue("geometry");

			res.Geometry = _geometryParser.ParseGeometry();

			AssertRead(JsonToken.EndObject);

			return res;
		}

		private FeatureCollection ParseFeatureCollection()
		{
			AssertRead(JsonToken.PropertyName);

			if ((string)Reader.Value == "crs")
			{
				//TODO: Would be nice to remember the crs, for now, toss it
				int stackSize = 0;
				while (Reader.Read())
				{
					if (Reader.TokenType == JsonToken.StartObject)
						stackSize++;
					else if (Reader.TokenType == JsonToken.EndObject)
						stackSize--;

					if (stackSize == 0)
						break;
				}

				AssertRead(JsonToken.PropertyName);
			}

			Assert((string)Reader.Value == "features");

			AssertRead(JsonToken.StartArray);

			var res = new FeatureCollection();

			while (Reader.Read())
			{
				if (Reader.TokenType == JsonToken.StartObject)
				{
					res.Features.Add((Feature)ParseUnknown(true));
				}
				else if (Reader.TokenType == JsonToken.EndArray)
				{
					break;
				}
				else
				{
					throw new Exception();
				}
			}

			//TODO: Read endobject?

			return res;
		}
	}
}
