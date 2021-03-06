﻿using System;
using System.IO;
using Metrolib;

namespace Tailviewer
{
	public static class Constants
	{
		public static readonly string ApplicationTitle;
		public static readonly string MainWindowTitle;
		public static readonly Version ApplicationVersion;
		public static readonly DateTime BuildDate;
		public static readonly Uri ProjectPage;
		public static readonly Uri GithubPage;
		public static readonly Uri ReportBugPage;
		public static readonly string ApplicationFolder;
		public static readonly string AppDataLocalFolder;
		public static readonly string ApplicationLogFile;
		public static readonly string MyDocumentsFolder;
		public static readonly string ExportDirectory;
		public static readonly string DownloadFolder;
		public static readonly string PluginPath;

		public static readonly string SnapshotDirectory;
		public static readonly string SnapshotExtension;

		public static readonly string AnalysisDirectory;
		public static readonly string AnalysisExtension;

		public static readonly string AnalysisTemplateDirectory;
		public static readonly string AnalysisTemplateExtension;

		public static string ApplicationLicense => Resource.ReadResourceToEnd("Licenses/Tailviewer/LICENSE");

		static Constants()
		{
			ApplicationTitle = "Tailviewer";
			ApplicationVersion = Core.Constants.ApplicationVersion;
			BuildDate = Core.Constants.BuildDate;
			MainWindowTitle = string.Format("Tailviewer, v{0}", ApplicationVersion.Format());
			ProjectPage = new Uri("https://kittyfisto.github.io/Tailviewer/");
			GithubPage = new Uri("https://github.com/Kittyfisto/Tailviewer");
			ReportBugPage = new Uri("https://github.com/Kittyfisto/Tailviewer/issues/new");
			ApplicationFolder = Core.Constants.ApplicationFolder;
			PluginPath = Path.Combine(ApplicationFolder, "Plugins");
			AppDataLocalFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationTitle);
			ApplicationLogFile = Path.Combine(AppDataLocalFolder, "Tailviewer.log");
			DownloadFolder = Path.Combine(AppDataLocalFolder, "Downloads");
			MyDocumentsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ApplicationTitle);
			ExportDirectory = Path.Combine(MyDocumentsFolder, "Export");

			SnapshotDirectory = Path.Combine(MyDocumentsFolder, "Snapshots");
			SnapshotExtension = "tvas";

			AnalysisDirectory = Path.Combine(MyDocumentsFolder, "Analyses");
			AnalysisExtension = "tva";

			AnalysisTemplateDirectory = Path.Combine(MyDocumentsFolder, "Templates");
			AnalysisTemplateExtension = "tvat";
		}
	}
}