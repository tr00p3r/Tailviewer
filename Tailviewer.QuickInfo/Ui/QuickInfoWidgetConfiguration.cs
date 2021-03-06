﻿using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.QuickInfo.Ui
{
	public sealed class QuickInfoWidgetConfiguration
		: IWidgetConfiguration
	{
		public QuickInfoWidgetConfiguration()
		{
			Titles = new Dictionary<Guid, QuickInfoViewConfiguration>();
		}

		/// <summary>
		///     The title of the individual quick infos.
		/// </summary>
		/// <remarks>
		///     Is part of the view configuration because the title may be changed without analyzing again...
		/// </remarks>
		public Dictionary<Guid, QuickInfoViewConfiguration> Titles;

		public object Clone()
		{
			return new QuickInfoWidgetConfiguration
			{
				Titles = Titles?.ToDictionary(x => x.Key, x=> x.Value?.Clone())
			};
		}

		public void Serialize(IWriter writer)
		{
			throw new NotImplementedException();
		}

		public void Deserialize(IReader reader)
		{
			throw new NotImplementedException();
		}
	}
}