using System;
using System.IO;
using NetTopologySuite.CoordinateSystems;
using NetTopologySuite.Features;
using Newtonsoft.Json;

namespace GeoJsonSharp.Parse
{
	public class GeoJsonParser : BaseParser
	{
		private readonly GeometryParser _geometryParser;

		public GeoJsonParser(string json, ParserSettings settings)
			: base(new JsonTextReader(new StringReader(json)), settings)
		{
			_geometryParser = new GeometryParser(Reader, settings);
		}

		public GeoJsonParser(TextReader reader, ParserSettings settings)
			: base(new JsonTextReader(reader), settings)
		{
			_geometryParser = new GeometryParser(Reader, settings);

		}


		public FeatureCollection Parse()
		{
			return (FeatureCollection)ParseUnknown(false);
		}

		private object ParseUnknown(bool startObjectAlreadyConsumed)
		{
			if (!startObjectAlreadyConsumed)
			{
				Reader.Read();
				Assert(Reader.TokenType == JsonToken.StartObject);
			}

			AssertRead(JsonToken.PropertyName);
			AssertValue("type");

			AssertRead(JsonToken.String);
			var type = (string?)Reader.Value;

			switch (type?.ToLower())
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

			var attributes = Settings.AttributesTableFactory.Invoke();

			AssertRead(JsonToken.StartObject);

			// Now we read key/value pairs for the properties until we hit end object
			while (true)
			{
				// Should be a propertyname, or endobject (meaning we are done with properties)
				Reader.Read();
				if (Reader.TokenType == JsonToken.EndObject)
					break;
				Assert(Reader.TokenType == JsonToken.PropertyName);

				var property = (string?)Reader.Value;

				AssertRead(new[] {JsonToken.Float, JsonToken.Integer, JsonToken.Null, JsonToken.String, JsonToken.Date});

				var value = Reader.Value;

				attributes.AddAttribute(property, value);
			}

			// Now hopefully we hit the geometry
			AssertRead(JsonToken.PropertyName);
			AssertValue("geometry");

			var geometry = _geometryParser.ParseGeometry();

			AssertRead(JsonToken.EndObject);

			return new Feature(geometry, attributes);
		}

		private FeatureCollection ParseFeatureCollection()
		{
			var res = new FeatureCollection();

			AssertRead(JsonToken.PropertyName);

			if ((string?)Reader.Value == "crs")
			{
				CRSBase crs;

				AssertRead(JsonToken.StartObject);

				AssertRead(JsonToken.PropertyName);
				AssertValue("type");

				AssertRead(JsonToken.String);
				var type = (string)Reader.Value;

				switch (type)
				{
					case "name":
						crs = ParseNamedCRS();
						break;
					case "link":
						crs = ParseLinkedCRS();
						break;
					default:
						throw new Exception($"Don't know how to parse CRS type {type}");
				}

				AssertRead(JsonToken.EndObject);

				res.CRS = crs;

				AssertRead(JsonToken.PropertyName);
			}

			Assert((string?)Reader.Value == "features");

			AssertRead(JsonToken.StartArray);


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

			// TODO: Read endobject?
			return res;
		}

		private NamedCRS ParseNamedCRS()
		{
			AssertRead(JsonToken.PropertyName);
			AssertValue("properties");

			AssertRead(JsonToken.StartObject);

			AssertRead(JsonToken.PropertyName);
			AssertValue("name");

			AssertRead(JsonToken.String);
			var name = (string?)Reader.Value;

			AssertRead(JsonToken.EndObject);
			
			var namedCrs = new NamedCRS(name);
			return namedCrs;
		}

		private LinkedCRS ParseLinkedCRS()
		{
			AssertRead(JsonToken.PropertyName);
			AssertValue("properties");

			AssertRead(JsonToken.StartObject);

			AssertRead(JsonToken.PropertyName);
			AssertValue("href");

			AssertRead(JsonToken.String);
			var href = (string?)Reader.Value;

			AssertRead(JsonToken.PropertyName);
			AssertValue("type");

			AssertRead(JsonToken.String);
			var type = (string?)Reader.Value;

			AssertRead(JsonToken.EndObject);

			var linkedCrs = new LinkedCRS(href, type);
			return linkedCrs;
		}
	}
}
