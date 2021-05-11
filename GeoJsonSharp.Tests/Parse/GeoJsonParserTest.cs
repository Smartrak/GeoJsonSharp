using System;
using GeoJsonSharp.Parse;
using NUnit.Framework;

namespace GeoJsonSharp.Tests.Parse
{
    /// <summary>
    /// The geo json parser test.
    /// </summary>
    public class GeoJsonParserTest
    {
        /// <summary>
        /// The parse feature with date property.
        /// </summary>
        [Test]
        public void ParseFeatureWithDateProperty()
        {
            // Arrange

            // GeoJSON FeatureCollection having one feature
            // Feature has property - name:'processed' and is of type 'DateTime'
            var geoJsonString = "{\"type\": \"FeatureCollection\",\"crs\": { \"type\": \"name\", \"properties\": { \"name\": \"urn:ogc:def:crs:EPSG::4283\" } },\"features\": [{\"type\": \"Feature\",\"properties\": {\"title\": \"Feature with date property\",\"processed\": \"2015-08-10T11:10:38-00:00\"},\"geometry\": {\"type\": \"Point\",\"coordinates\": [ 152.7465, -31.1015 ]}}]}";

            // Act
            var geoParser = new GeoJsonParser(geoJsonString, new ParserSettings {SkipInvalidGeometry = false});
            var featureCollection = geoParser.Parse();
            var processed = featureCollection.Features[0].Attributes["processed"];

            // Assert
            Assert.That(processed, Is.InstanceOf<DateTime>());
        }
    }
}