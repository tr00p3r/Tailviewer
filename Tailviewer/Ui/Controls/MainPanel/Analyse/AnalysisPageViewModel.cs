﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using log4net;
using Metrolib;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core.Analysis;
using Tailviewer.Ui.Analysis;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Layouts;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse
{
	/// <summary>
	///     Represents a single page of an analysis.
	///     A page consists of zero or more <see cref="IWidgetViewModel" />s which are
	///     presented using a <see cref="IWidgetLayoutViewModel" />.
	///     The layout of a page can be changed through <see cref="PageLayout" />.
	/// </summary>
	public sealed class AnalysisPageViewModel
		: IAnalysisPageViewModel
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly PageTemplate _template;
		private readonly IAnalysis _analyser;
		private readonly DelegateCommand _deletePageCommand;
		private readonly List<IWidgetViewModel> _widgets;
		private readonly Dictionary<IWidgetViewModel, IDataSourceAnalyser> _analysersPerWidget;
		private bool _canBeDeleted;
		private IWidgetLayoutViewModel _layout;
		private string _name;
		private PageLayout _pageLayout;
		private bool _hasWidgets;

		public AnalysisPageViewModel(PageTemplate template, IAnalysis analyser)
		{
			if (template == null)
				throw new ArgumentNullException(nameof(template));
			if (analyser == null)
				throw new ArgumentNullException(nameof(analyser));

			_template = template;
			_analyser = analyser;
			_name = "New Page";
			_deletePageCommand = new DelegateCommand(DeletePage, () => _canBeDeleted);
			_widgets = new List<IWidgetViewModel>();
			_analysersPerWidget = new Dictionary<IWidgetViewModel, IDataSourceAnalyser>();

			PageLayout = PageLayout.WrapHorizontal;
		}

		public PageLayout PageLayout
		{
			get { return _pageLayout; }
			set
			{
				if (value == _pageLayout)
					return;

				_pageLayout = value;
				EmitPropertyChanged();

				switch (value)
				{
					case PageLayout.None:
						Layout = null;
						break;

					case PageLayout.WrapHorizontal:
						Layout = new HorizontalWidgetLayoutViewModel();
						break;
				}
			}
		}

		public bool HasWidgets
		{
			get { return _hasWidgets; }
			private set
			{
				if (value == _hasWidgets)
					return;

				_hasWidgets = value;
				EmitPropertyChanged();
			}
		}

		public IWidgetLayoutViewModel Layout
		{
			get { return _layout; }
			private set
			{
				if (value == _layout)
					return;

				if (_layout != null)
				{
					_layout.RequestAdd -= LayoutOnRequestAddWidget;
				}

				_layout = value;

				if (_layout != null)
				{
					_layout.RequestAdd += LayoutOnRequestAddWidget;
				}

				EmitPropertyChanged();

				if (_layout != null)
					foreach (var widget in _widgets)
						_layout.Add(widget);

				HasWidgets = _widgets.Any();
			}
		}

		private void LayoutOnRequestAddWidget(IWidgetPlugin plugin)
		{
			var viewConfiguration = CreateViewConfiguration(plugin);

			var configuration = plugin.DefaultAnalyserConfiguration?.Clone() as ILogAnalyserConfiguration;
			var analyser = CreateAnalyser(plugin.AnalyserId, configuration);

			var widgetTemplate = new WidgetTemplate
			{
				Id = WidgetId.CreateNew(),
				AnalyserId = analyser.Id,
				Configuration = viewConfiguration
			};
			var widget = plugin.CreateViewModel(widgetTemplate, analyser);

			_analysersPerWidget.Add(widget, analyser);
			_template.Add(widgetTemplate);

			Add(widget);
		}

		private IWidgetConfiguration CreateViewConfiguration(IWidgetPlugin plugin)
		{
			try
			{
				return plugin.DefaultViewConfiguration?.Clone() as IWidgetConfiguration;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while creating view configuration for widget '{0}': {1}", plugin, e);
				return null;
			}
		}

		private IDataSourceAnalyser CreateAnalyser(LogAnalyserFactoryId factoryId, ILogAnalyserConfiguration configuration)
		{
			try
			{
				if (factoryId != LogAnalyserFactoryId.Empty)
				{
					return _analyser.Add(factoryId, configuration);
				}

				Log.DebugFormat("Widget '{0}' doesn't specify a log analyser, none will created", factoryId);
				return new NoAnalyser();
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while creating log analyser for widget '{0}': {1}", factoryId, e);
				return new NoAnalyser(factoryId);
			}
		}

		private void Add(IWidgetViewModel widget)
		{
			_widgets.Add(widget);
			_layout?.Add(widget);
			widget.OnDelete += WidgetOnDelete;
			HasWidgets = _widgets.Any();
		}

		private void WidgetOnDelete(IWidgetViewModel widget)
		{
			Remove(widget);
		}

		public void Remove(IWidgetViewModel widget)
		{
			_widgets.Remove(widget);
			_layout?.Remove(widget);
			_template.Remove(widget.Template);

			IDataSourceAnalyser analyser;
			if (_analysersPerWidget.TryGetValue(widget, out analyser))
			{
				_analysersPerWidget.Remove(widget);
				_analyser.Remove(analyser);
			}

			widget.OnDelete -= WidgetOnDelete;
			HasWidgets = _widgets.Any();
		}

		public string Name
		{
			get { return _name; }
			set
			{
				if (value == _name)
					return;
				_name = value;
				EmitPropertyChanged();
			}
		}

		public ICommand DeletePageCommand => _deletePageCommand;

		public bool CanBeDeleted
		{
			get { return _canBeDeleted; }
			set
			{
				_canBeDeleted = value;
				_deletePageCommand.RaiseCanExecuteChanged();
			}
		}

		public PageTemplate Template => _template;

		public event PropertyChangedEventHandler PropertyChanged;

		private void DeletePage()
		{
			OnDelete?.Invoke(this);
		}

		public event Action<AnalysisPageViewModel> OnDelete;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void Update()
		{
			foreach(var widget in _widgets)
			{
				widget.Update();
			}
		}
	}
}