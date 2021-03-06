﻿using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Archiver.Test
{
	[TestFixture]
	public class PluginPackerTest
	{
		private string _fname;
		private string _testData;

		[SetUp]
		public void Setup()
		{
			_fname = Path.GetTempFileName();
			Console.WriteLine("Plugin: {0}", _fname);
			if (File.Exists(_fname))
				File.Delete(_fname);

			_testData = Path.Combine(AssemblyDirectory, "..", "Tailviewer.Archiver.Test", "TestData");
		}

		[Test]
		public void TestAddAssembly1()
		{
			using (var packer = PluginPacker.Create(_fname))
			{
				var builder = new AbstractPluginTest.PluginBuilder("abcdefg", "Plugin");
				builder.AssemblyVersion = "4.0.3.1";
				builder.AssemblyFileVersion = "1.2.3.42";
				builder.AssemblyInformationalVersion = "4.0.0.0-beta";
				builder.Save();
				packer.AddPluginAssembly(builder.FileName);
			}

			using (var reader = PluginArchive.OpenRead(_fname))
			{
				var index = reader.Index;
				index.Should().NotBeNull();
				index.Name.Should().Be("Plugin");
				index.Assemblies.Should().NotBeNull();
				index.Assemblies.Should().HaveCount(1);
				index.Assemblies[0].EntryName.Should().Be("Plugin.dll");
				index.Assemblies[0].AssemblyName.Should().Be("Plugin");
				index.Assemblies[0].AssemblyVersion.Should().Be(new Version(4, 0, 3, 1));
				index.Assemblies[0].AssemblyFileVersion.Should().Be(new Version(1, 2, 3, 42));
				index.Assemblies[0].AssemblyInformationalVersion.Should().Be("4.0.0.0-beta");
				index.NativeImages.Should().NotBeNull();
				index.NativeImages.Should().HaveCount(0);

				var actualAssembly = reader.LoadAssembly("Plugin.dll");
				actualAssembly.Should().NotBeNull();
			}
		}

		[Test]
		[Description("Verifies that adding x64 assemblies is not supported (yet)")]
		public void TestAddAssembly2()
		{
			using (var packer = PluginPacker.Create(_fname))
			{
				var fname = Path.Combine(_testData, "Managed", "x64", "ClassLibrary1.dll");
				new Action(() => packer.AddFile("ClassLibrary1.dll", fname))
					.ShouldThrow<PackException>()
					.WithMessage("Assemblies must be compiled for x86 or AnyCPU");
			}
		}

		[Test]
		[Description("Verifies that adding x86 assemblies is allowed")]
		public void TestAddAssembly3()
		{
			using (var packer = PluginPacker.Create(_fname))
			{
				var fname = Path.Combine(_testData, "Managed", "x86", "ClassLibrary1.dll");
				packer.AddFile("ClassLibrary1.dll", fname);
			}

			using (var reader = PluginArchive.OpenRead(_fname))
			{
				var index = reader.Index;
				index.Should().NotBeNull();
				index.PluginArchiveVersion.Should().Be(PluginArchive.CurrentPluginArchiveVersion);
				index.Assemblies.Should().NotBeNull();
				index.Assemblies.Should().HaveCount(1);
				index.Assemblies[0].EntryName.Should().Be("ClassLibrary1.dll");
				index.Assemblies[0].AssemblyName.Should().Be("ClassLibrary1");
			}
		}

		[Test]
		[Description("Verifies that plugins may not target newer frameworks than 4.5.2")]
		public void TestAddAssembly4()
		{
			using (var packer = PluginPacker.Create(_fname))
			{
				var fname = Path.Combine(_testData, "Managed", "x86", "Targets.NET.4.6.dll");
				new Action(() => packer.AddFile("Foo.dll", fname))
					.ShouldThrow<PackException>()
					.WithMessage("Assemblies may only target frameworks of up to .NET 4.5.2");

				fname = Path.Combine(_testData, "Managed", "x86", "Targets.NET.4.6.1.dll");
				new Action(() => packer.AddFile("Foo.dll", fname))
					.ShouldThrow<PackException>()
					.WithMessage("Assemblies may only target frameworks of up to .NET 4.5.2");

				fname = Path.Combine(_testData, "Managed", "x86", "Targets.NET.4.6.2.dll");
				new Action(() => packer.AddFile("Foo.dll", fname))
					.ShouldThrow<PackException>()
					.WithMessage("Assemblies may only target frameworks of up to .NET 4.5.2");
			}
		}

		[Test]
		public void TestAddAssembly5()
		{
			using (var packer = PluginPacker.Create(_fname))
			{
				var builder = new AbstractPluginTest.PluginBuilder("abcdefg", "Plugin");
				builder.PluginVersion = new Version(1, 4, 12034);
				builder.ImplementInterface<IFileFormatPlugin>("Plugin.FileFormatPlugin");
				builder.Save();

				packer.AddPluginAssembly(builder.FileName);
			}

			using (var reader = PluginArchive.OpenRead(_fname))
			{
				var index = reader.Index;
				index.Version.Should().NotBeNull();
				index.Id.Should().Be("abcdefg");
				index.Name.Should().Be("Plugin");
				index.Version.Should().Be(new Version(1, 4, 12034));
			}
		}

		[Test]
		public void TestAddNativeImage1()
		{
			using (var packer = PluginPacker.Create(_fname))
			{
				var builder = new AbstractPluginTest.PluginBuilder("abcdefg", "Plugin");
				builder.Save();
				packer.AddPluginAssembly(builder.FileName);

				var fname = Path.Combine(_testData, "Native", "x86", "NativeImage.dll");
				packer.AddFile("NativeImage.dll", fname);
			}

			using (var reader = PluginArchive.OpenRead(_fname))
			{
				var index = reader.Index;
				index.Should().NotBeNull();
				index.Assemblies.Should().NotBeNull();
				index.Assemblies.Should().HaveCount(1);

				index.NativeImages.Should().NotBeNull();
				index.NativeImages.Count.Should().Be(1);
				index.NativeImages[0].EntryName.Should().Be("NativeImage.dll");
				index.NativeImages[0].ImageName.Should().Be("NativeImage");
			}
		}

		[Test]
		[Description("Verifies that adding a 64-bit native image is not supported (yet)")]
		public void TestAddNativeImage2()
		{
			using (var packer = PluginPacker.Create(_fname))
			{
				var fname = Path.Combine(_testData, "Native", "x64", "NativeImage.dll");
				new Action(() => packer.AddFile("NativeImage.dll", fname))
					.ShouldThrow<PackException>()
					.WithMessage("Native images must be compiled for x86");
			}
		}

		[Test]
		public void TestAddIcon1()
		{
			using (var packer = PluginPacker.Create(_fname))
			{
				using (var icon = File.OpenRead(Path.Combine(_testData, "cropped-uiforetwicon2.png")))
				{
					packer.SetIcon(icon);
				}
			}

			using (var reader = PluginArchive.OpenRead(_fname))
			{
				var stream = reader.ReadIcon();
				stream.Should().NotBeNull();

				using (var image = new Bitmap(stream))
				{
					image.Width.Should().Be(16);
					image.Height.Should().Be(16);
				}
			}
		}

		public static string AssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().CodeBase;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
				return path;
			}
		}
	}
}