using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace GeoJsonSharp.Parse
{
	internal class GeometryParser : BaseParser
	{
		public GeometryParser(JsonTextReader reader, ParserSettings settings)
			: base(reader, settings)
		{
		}

		public IGeometry ParseGeometry()
		{
			AssertRead(JsonToken.StartObject);

			AssertRead(JsonToken.PropertyName);
			AssertValue("type");

			AssertRead(JsonToken.String);

			IGeometry geometry;
			switch (((string)Reader.Value).ToLower())
			{
				case "point":
					geometry = ParsePoint();
					break;
				case "linestring":
					geometry = ParseLineString(false);
					break;
				case "multilinestring":
					geometry = ParseMultiLineString(false);
					break;
				case "polygon":
					geometry = ParsePolygon(false);
					break;
				case "multipolygon":
					geometry = ParseMultiPolygon();
					break;
				default:
					throw new Exception("Don't know how to parse " + Reader.Value);
			}

			AssertRead(JsonToken.EndObject);

			return geometry;
		}

		private Coordinate ParseCoord(bool alreadyConsumedStartArray)
		{
			if (!alreadyConsumedStartArray)
				AssertRead(JsonToken.StartArray);

			double x;
			double y;
			double z = double.NaN;

			x = (double)Reader.ReadAsDecimal().Value;
			y = (double)Reader.ReadAsDecimal().Value;

			var zMaybe = Reader.ReadAsDecimal();
			if (zMaybe.HasValue)
			{
				z = (double)zMaybe.Value;
				AssertRead(JsonToken.EndArray);
			}
			else
			{
				//Otherwise we just read something, check it is EndArray
				Assert(Reader.TokenType == JsonToken.EndArray);
			}

			return new Coordinate(x, y, z);
		}

		private Point ParsePoint()
		{
			AssertCoordinatesProperty();

			var coord = ParseCoord(false);
			return new Point(coord);
		}

		private LineString ParseLineString(bool alreadyInsideArray)
		{
			if (!alreadyInsideArray)
			{
				AssertCoordinatesProperty();
				AssertRead(JsonToken.StartArray);
			}

			List<Coordinate> coords = new List<Coordinate>();

			while (true)
			{
				Reader.Read();

				if (Reader.TokenType == JsonToken.EndArray)
					break;

				Assert(Reader.TokenType == JsonToken.StartArray);

				coords.Add(ParseCoord(true));
			}

			if (Settings.SkipInvalidGeometry && coords.Count <= 1)
				return null;

			return new LineString(coords.ToArray());
		}

		private MultiLineString ParseMultiLineString(bool alreadyInsideArray)
		{
			if (!alreadyInsideArray)
			{
				AssertCoordinatesProperty();
				AssertRead(JsonToken.StartArray);
			}

			var lineStrings = new List<ILineString>();

			while (true)
			{
				Reader.Read();

				if (Reader.TokenType == JsonToken.EndArray)
					break;

				Assert(Reader.TokenType == JsonToken.StartArray);

				var lineString = ParseLineString(true);
				if (lineString != null)
					lineStrings.Add(lineString);
			}

			return new MultiLineString(lineStrings.ToArray());
		}

		private Polygon ParsePolygon(bool alreadyInsideArray)
		{
			//TODO: Do this not hax
			var mls = ParseMultiLineString(alreadyInsideArray);

			if (mls.Count == 1)
				return new Polygon(new LinearRing(mls[0].Coordinates));

			var holes = new List<ILinearRing>();
			for (var i = 1; i < mls.Count; i++)
				holes.Add(new LinearRing(mls[i].Coordinates));
			return new Polygon(new LinearRing(mls[0].Coordinates), holes.ToArray());
		}

		private MultiPolygon ParseMultiPolygon()
		{
			AssertCoordinatesProperty();
			AssertRead(JsonToken.StartArray);

			var polygons = new List<IPolygon>();

			while (true)
			{
				Reader.Read();

				if (Reader.TokenType == JsonToken.EndArray)
					break;

				Assert(Reader.TokenType == JsonToken.StartArray);

				var polygon = ParsePolygon(true);
				if (polygon != null)
					polygons.Add(polygon);
			}

			return new MultiPolygon(polygons.ToArray());
		}

		private void AssertCoordinatesProperty()
		{
			AssertRead(JsonToken.PropertyName);
			AssertValue("coordinates");
		}

	}
}