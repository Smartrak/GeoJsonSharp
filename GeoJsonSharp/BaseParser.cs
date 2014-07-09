using System;
using System.Linq;
using Newtonsoft.Json;

namespace GeoJsonSharp
{
	class BaseParser
	{
		protected readonly JsonTextReader Reader;
		protected readonly ParserSettings Settings;

		protected BaseParser(JsonTextReader reader, ParserSettings settings)
		{
			Reader = reader;
			Settings = settings;
		}

		protected void AssertRead(JsonToken expectedToken)
		{
			Reader.Read();

			if (Reader.TokenType != expectedToken)
				throw new Exception("Expected " + expectedToken + " but got " + Reader.TokenType);
		}


		protected void AssertRead(JsonToken[] expectedToken)
		{
			Reader.Read();

			if (!expectedToken.Contains(Reader.TokenType))
				throw new Exception("Expected one of [" + String.Join(", ", expectedToken.Select(x => x.ToString())) + "] but got " + Reader.TokenType);
		}

		protected void AssertValue(string expectedValue)
		{
			if ((string)Reader.Value != expectedValue)
				throw new Exception("Expected '" + expectedValue + "' but got '" + Reader.Value + "'");
		}

		protected void Assert(bool b)
		{
			if (!b)
				throw new Exception();
		}

	}
}
