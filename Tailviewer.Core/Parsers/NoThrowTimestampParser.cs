﻿using System;
using System.Reflection;
using log4net;

namespace Tailviewer.Core.Parsers
{
	/// <summary>
	///     This class is responsible for ensuring that if a given <see cref="ITimestampParser" />
	///     throws, then its exception is caught and logged, but not thrown to a client.
	/// </summary>
	public sealed class NoThrowTimestampParser
		: ITimestampParser
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ITimestampParser _parser;

		/// <summary>
		///     Initializes this parser.
		/// </summary>
		/// <param name="parser"></param>
		public NoThrowTimestampParser(ITimestampParser parser)
		{
			if (parser == null)
				throw new ArgumentNullException(nameof(parser));

			_parser = parser;
		}

		/// <inheritdoc />
		public bool TryParse(string content, out DateTime timestamp)
		{
			try
			{
				return _parser.TryParse(content, out timestamp);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				timestamp = DateTime.MinValue;
				return false;
			}
		}
	}
}