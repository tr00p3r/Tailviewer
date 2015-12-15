﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class QuickFilterViewModelTest
	{
		private QuickFilter _quickFilter;
		private DataSource _dataSource;
		private QuickFilterViewModel _model;
		private Tailviewer.Settings.DataSource _dataSourceSettings;
		private List<string> _changes;

		[SetUp]
		public void SetUp()
		{
			_quickFilter = new QuickFilter(new Tailviewer.Settings.QuickFilter());
			_dataSource = new DataSource(_dataSourceSettings = new Tailviewer.Settings.DataSource("nothing"));
			_model = new QuickFilterViewModel(_quickFilter, x => { })
				{
					CurrentDataSource = _dataSource
				};
			_changes = new List<string>();
			_model.PropertyChanged += (sender, args) => _changes.Add(args.PropertyName);
		}

		[Test]
		public void TestCtor()
		{
			_model.CurrentDataSource.Should().NotBeNull();
			_model.IsActive.Should().BeFalse("Because the quickfilter's guid is not the data source's list");
			_model.CanBeActivated.Should().BeTrue();
			_model.RemoveCommand.Should().NotBeNull();
			_model.RemoveCommand.CanExecute(null).Should().BeTrue();
		}

		[Test]
		public void TestDataSource1()
		{
			_model.CurrentDataSource.Should().NotBeNull();
			_model.CanBeActivated.Should().BeTrue();
			_model.CurrentDataSource = null;
			_model.CanBeActivated.Should().BeFalse();
			_model.IsActive.Should().BeFalse();
			new Action(() => _model.IsActive = true).ShouldThrow<InvalidOperationException>();
		}

		[Test]
		public void TestDataSource2()
		{
			_model.CurrentDataSource.Should().NotBeNull();
			_model.CanBeActivated.Should().BeTrue();
			_model.CurrentDataSource = null;
			_changes.Should().Equal(new[] {"CanBeActivated"});
			_model.CanBeActivated.Should().BeFalse();
			_model.IsActive.Should().BeFalse();

			_model.CurrentDataSource = _dataSource;
			_changes.Should().Equal(new[] { "CanBeActivated", "CanBeActivated" });

			_model.CurrentDataSource = _dataSource;
			_changes.Should().Equal(new[] { "CanBeActivated", "CanBeActivated" });
		}

		[Test]
		public void TestIsActive()
		{
			_model.IsActive = true;
			_dataSourceSettings.ActivatedQuickFilters.Should().Equal(new[] { _quickFilter.Id });

			_model.IsActive = false;
			_dataSourceSettings.ActivatedQuickFilters.Should().BeEmpty();
		}
	}
}