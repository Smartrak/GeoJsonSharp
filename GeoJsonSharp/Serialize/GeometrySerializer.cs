using System;
using System.Linq;
using GeoAPI.Geometries;
using Newtonsoft.Json;

namespace GeoJsonSharp.Serialize
{
	internal class GeometrySerializer : BaseSerializer
	{
		public GeometrySerializer(JsonTextWriter writer, ParserSettings settings)
			: base(writer, settings)
		{
		}

		public void SerializeGeometry(IGeometry geometry)
		{
			Writer.WriteStartObject();
			Writer.WritePropertyName("type");
			Writer.WriteValue(geometry.GeometryType);

			switch (geometry.OgcGeometryType)
			{
				case OgcGeometryType.Point:
					SerializePoint(geometry);
					break;
				case OgcGeometryType.LineString:
					SerializeLineString(geometry, false);
					break;
				case OgcGeometryType.MultiLineString:
					SerializeMultiLineString(geometry);
					break;
				case OgcGeometryType.Polygon:
					SerializePolygon(geometry, false);
					break;
				case OgcGeometryType.MultiPolygon:
					SerializeMultiPolygon(geometry);
					break;
				default:
					throw new Exception("Don't know how to serialize " + geometry.OgcGeometryType);
			}

			Writer.WriteEndObject();
		}

		/// <remarks>Serializes 2D coordinates only, 3D coordinates not supported</remarks>
		private void SerializeCoord(Coordinate coordinate, bool hasThreeDimensions = false)
		{
			Writer.WriteStartArray();

			Writer.WriteValue(coordinate.X);
			Writer.WriteValue(coordinate.Y);

			if (hasThreeDimensions)
				throw new Exception("Don't know how to write 3D coordinates");

			Writer.WriteEndArray();
		}
		
		private void SerializePoint(IGeometry geometry)
		{
			Writer.WritePropertyName("coordinates");
			SerializeCoord(geometry.Coordinates.Single());
		}

		private void SerializeLineString(IGeometry geometry, bool alreadyWrittenCoordinates)
		{
			if (!alreadyWrittenCoordinates)
				Writer.WritePropertyName("coordinates");

			Writer.WriteStartArray();

			if (!(geometry.Coordinates.Length <= 1 && Settings.SkipInvalidGeometry))
			{
				foreach (var coord in geometry.Coordinates)
				{
					SerializeCoord(coord);
				}
			}

			Writer.WriteEndArray();
		}

		private void SerializeMultiLineString(IGeometry geometry)
		{
			Writer.WritePropertyName("coordinates");
			Writer.WriteStartArray();

			for (int i = 0; i < geometry.NumGeometries; i++)
			{
				SerializeLineString(geometry.GetGeometryN(i), true);
			}

			Writer.WriteEndArray();
		}

		private void SerializePolygon(IGeometry geometry, bool alreadyWrittenCoordinates)
		{
			if (!alreadyWrittenCoordinates)
				Writer.WritePropertyName("coordinates");

			Writer.WriteStartArray();

			//Outer shell serialized first, then inner holes if we have any, as per GeoJSON spec
			var polygon = ((IPolygon) geometry);

			SerializeLineString(polygon.ExteriorRing, true);

			for (int i = 0; i < polygon.NumInteriorRings; i++)
			{
				SerializeLineString(polygon.GetInteriorRingN(i), true);
			}

			Writer.WriteEndArray();
		}

		private void SerializeMultiPolygon(IGeometry geometry)
		{
			Writer.WritePropertyName("coordinates");
			Writer.WriteStartArray();

			var multiPolygon = ((IMultiPolygon) geometry);

			for (int i = 0; i < multiPolygon.NumGeometries; i++)
			{
				SerializePolygon(multiPolygon.GetGeometryN(i), true);
			}

			Writer.WriteEndArray();
		}
	}
}
