﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Tailviewer.BusinessLogic
{
	/// <summary>
	/// Combines multiple <see cref="ILogEntryFilter"/>s into one.
	/// A <see cref="LogLine"/> passes if it passes *all* individual filters.
	/// </summary>
	internal sealed class FilterChain
		: ILogEntryFilter
	{
		private readonly ILogEntryFilter[] _filters;

		public FilterChain(IEnumerable<ILogEntryFilter> filters)
		{
			if (filters == null) throw new ArgumentNullException("filters");

			_filters = filters.ToArray();
			if (_filters.Any(x => x == null)) throw new ArgumentNullException("filters");
		}

		public bool PassesFilter(IEnumerable<LogLine> logEntry)
		{
			var passes = new bool[_filters.Length];
			foreach(var logLine in logEntry)
			{
				for (int i = 0; i < _filters.Length; ++i)
				{
					var filter = _filters[i];
					if (!passes[i])
					{
						passes[i] = filter.PassesFilter(logLine);
					}
				}
			}

// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable ForCanBeConvertedToForeach
			for (int i = 0; i < passes.Length; ++i)
// ReSharper restore ForCanBeConvertedToForeach
// ReSharper restore LoopCanBeConvertedToQuery
			{
				if (!passes[i])
					return false;
			}

			return true;
		}

		public bool PassesFilter(LogLine logLine)
		{
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable ForCanBeConvertedToForeach
			for (int i = 0; i < _filters.Length; ++i)
// ReSharper restore ForCanBeConvertedToForeach
// ReSharper restore LoopCanBeConvertedToQuery
			{
				if (!_filters[i].PassesFilter(logLine))
					return false;
			}

			return true;
		}
	}
}