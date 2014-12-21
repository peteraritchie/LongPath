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
	public class FileSystemInfoTests
	{
		private static string rootTestDir;
		private static string longPathDirectory;
		private static string longPathFilename;
		private static string longPathRoot;
		private const string Filename = "filename.ext";

		[ClassInitialize]
		public static void ClassInitialize(TestContext context)
		{
			rootTestDir = context.TestDir;
			longPathDirectory = Util.MakeLongPath(rootTestDir);
			longPathRoot = longPathDirectory.Substring(0, context.TestDir.Length + 1 + longPathDirectory.Substring(rootTestDir.Length + 1).IndexOf('\\'));
			Directory.CreateDirectory(longPathDirectory);
			Debug.Assert(Directory.Exists(longPathDirectory));
			longPathFilename = new StringBuilder(longPathDirectory).Append(@"\").Append(Filename).ToString();
			using (var writer = File.CreateText(longPathFilename))
			{
				writer.WriteLine("test");
			}
			Debug.Assert(File.Exists(longPathFilename));
		}

		[TestMethod]
		public void TestExtension()
		{
			var fi = new FileInfo(longPathFilename);
			Assert.AreEqual(".ext", fi.Extension);
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
			try
			{
				if (File.Exists(longPathFilename))
					File.Delete(longPathFilename);
			}
			catch (Exception e)
			{
				Trace.WriteLine("Exception {0} deleting \"longPathFilename\"", e.ToString());
				throw;
			}
			finally
			{
				if(Directory.Exists(longPathRoot))
					Directory.Delete(longPathRoot, true);
			}
		}
	}
}
