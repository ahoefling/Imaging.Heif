﻿using System.IO;
using FileOnQ.Imaging.Heif.Tests.Utilities;
using NUnit.Framework;

namespace FileOnQ.Imaging.Heif.Tests.Integration
{
	[TestFixture(TestData.Image5)]
	[Category(Constants.Category.Integration)]
	[Category(Constants.Category.MemoryBuffer)]
	public class Thumbnail_ToArray_Tests
	{
		readonly string input;
		readonly string hash;
		byte[] output;

		public Thumbnail_ToArray_Tests(string path)
		{
			hash = TestData.Integration.ThuumbnailSave.HashCodes[path];

			var assemblyDirectory = Path.GetDirectoryName(typeof(NoThumbnailTests).Assembly.Location) ?? string.Empty;
			input = Path.Combine(assemblyDirectory, path);
		}

		[OneTimeSetUp]
		public void Execute()
		{
			using (var image = new HeifImage(input))
			using (var thumbnail = image.Thumbnail())
			{
				output = thumbnail.ToArray();
			}
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			output = null;
		}

		[Test]
		public void Thumbnail_ToArray_NotNull_Test()
		{
			Assert.IsNotNull(output);
		}

		[Test]
		public void Thumbnail_ToArray_Match_Test()
		{
			Assert.IsTrue(output.Length > 0);
			AssertUtilities.IsHashEqual(hash, output);
		}
    }
}
