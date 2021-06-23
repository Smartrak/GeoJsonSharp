using System;
using NetTopologySuite.CoordinateSystems;
using NetTopologySuite.Features;
using Newtonsoft.Json;

namespace GeoJsonSharp.Serialize
{
	public class GeoJsonSerializer : BaseSerializer
	{
		private readonly GeometrySerializer _geometrySerializer;

		public GeoJsonSerializer(JsonTextWriter writer, ParserSettings settings)
			: base(writer, settings)
		{
			_geometrySerializer = new GeometrySerializer(writer, settings);
		}

		public void Serialize(FeatureCollection featureCollection)
		{
			Writer.WriteStartObject();
			Writer.WritePropertyName("type");
			Writer.WriteValue("FeatureCollection");

			if (featureCollection.CRS != null)
			{
				Writer.WritePropertyName("crs");
				Writer.WriteStartObject();

				switch (featureCollection.CRS.Type)
				{
					case CRSTypes.Name:
						SerializeNamedCRS(featureCollection.CRS);
						break;
					case CRSTypes.Link:
						SerializeLinkedCRS(featureCollection.CRS);
						break;
					default:
						throw new Exception($"Don't know how to serialize {featureCollection.CRS} CRS type");
				}

				Writer.WriteEndObject();
			}

			Writer.WritePropertyName("features");
			Writer.WriteStartArray();

			foreach (var feature in featureCollection.Features)
			{
				SerializeFeature(feature);
			}

			Writer.WriteEndArray();
			Writer.WriteEndObject();
		}

		private void SerializeFeature(IFeature feature)
		{
			Writer.WriteStartObject();
			Writer.WritePropertyName("type");
			Writer.WriteValue("Feature");

			Writer.WritePropertyName("properties");
			Writer.WriteStartObject();

			var attributeNames = feature.Attributes.GetNames();

			foreach (var attrName in attributeNames)
			{
				Writer.WritePropertyName(attrName);
				Writer.WriteValue(feature.Attributes[attrName]);
			}

			Writer.WriteEndObject();

			Writer.WritePropertyName("geometry");
			_geometrySerializer.SerializeGeometry(feature.Geometry);

			Writer.WriteEndObject();
		}

		private void SerializeNamedCRS(ICRSObject crs)
		{
			var namedCrs = (NamedCRS) crs;

			Writer.WritePropertyName("type");
			Writer.WriteValue("name");
			
			Writer.WritePropertyName("properties");
			Writer.WriteStartObject();

			Writer.WritePropertyName("name");
			Writer.WriteValue((string) namedCrs.Properties["name"]); //object boxed string

			Writer.WriteEndObject();
		}
		
		private void SerializeLinkedCRS(ICRSObject crs)
		{
			var linkedCrs = (LinkedCRS) crs;

			Writer.WritePropertyName("type");
			Writer.WriteValue("link");

			Writer.WritePropertyName("properties");
			Writer.WriteStartObject();

			Writer.WritePropertyName("href");
			Writer.WriteValue((string) linkedCrs.Properties["href"]); //object boxed string

			if (linkedCrs.Properties.TryGetValue("type", out var typeProperty))
			{
				Writer.WritePropertyName("type");
				Writer.WriteValue((string) typeProperty); //object boxed string
			}

			Writer.WriteEndObject();
		}
	}
}
