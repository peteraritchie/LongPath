using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Directory = Pri.LongPath.Directory;
using Path = Pri.LongPath.Path;
using FileInfo = Pri.LongPath.FileInfo;
using DirectoryInfo = Pri.LongPath.DirectoryInfo;
using File = Pri.LongPath.File;
using FileMode = System.IO.FileMode;
using FileAccess = System.IO.FileAccess;
using FileShare = System.IO.FileShare;
using BinaryWriter = System.IO.BinaryWriter;
using PathTooLongException = System.IO.PathTooLongException;
using FileAttributes = System.IO.FileAttributes;
using IOException = System.IO.IOException;
using SearchOption = System.IO.SearchOption;

namespace Tests
{
	[TestClass]
	public class UncFileSystemInfoTests
	{
		private static string uncDirectory;
		private static string uncFilePath;
		private static string directory;
		private static string filePath;
		private const string Filename = "filename.ext";

		[ClassInitialize]
		public static void ClassInitialize(TestContext context)
		{
			directory = Path.Combine(context.TestDir, "subdir");
			System.IO.Directory.CreateDirectory(directory);
			try
			{
				uncDirectory = UncHelper.GetUncFromPath(directory);
				filePath = new StringBuilder(directory).Append(@"\").Append(Filename).ToString();
				uncFilePath = UncHelper.GetUncFromPath(filePath);
				using (var writer = System.IO.File.CreateText(filePath))
				{
					writer.WriteLine("test");
				}
				Debug.Assert(File.Exists(uncFilePath));
			}
			catch (Exception)
			{
				if (System.IO.Directory.Exists(directory))
					System.IO.Directory.Delete(directory, true);
				throw;
			}
		}

		[TestMethod]
		public void TestExtension()
		{
			var fi = new FileInfo(filePath);
			Assert.AreEqual(".ext", fi.Extension);
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
			try
			{
				if (File.Exists(filePath))
					File.Delete(filePath);
			}
			catch (Exception e)
			{
				Trace.WriteLine("Exception {0} deleting \"filePath\"", e.ToString());
				throw;
			}
			finally
			{
				if (Directory.Exists(directory))
					Directory.Delete(directory, true);
			}
		}
	}
}