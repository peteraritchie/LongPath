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
	public class PathTests
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
		public void TestGetDirectoryNameAtRoot()
		{
			string path = @"c:\";
			Assert.IsNull(Path.GetDirectoryName(path));
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void TestGetDirectoryNameWithNullPath()
		{
			Path.GetDirectoryName(null);
		}

        [TestMethod]
	    public void GetDirectoryNameOnRelativePath()
        {
            const string input = @"foo\bar\baz";
            const string expected = @"foo\bar";
            string actual = Path.GetDirectoryName(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetDirectoryNameOnRelativePathWithNoParent()
        {
            const string input = @"foo";
            const string expected = @"";
            string actual = Path.GetDirectoryName(input);
            Assert.AreEqual(expected, actual);
        }

		[TestMethod]
		public void TestGetParentAtRoot()
		{
			string path = "c:\\";
			Pri.LongPath.DirectoryInfo parent = Directory.GetParent(path);
			Assert.IsNull(parent);
		}

		[TestMethod]
		public void TestLongPathDirectoryName()
		{
			var x = Path.GetDirectoryName(@"C:\Vault Data\w\M\Access Midstream\9305 Hopeton Stabilizer Upgrades\08  COMMUNICATION\8.1  Transmittals\9305-005 Access Midstream Hopeton - Electrical Panel Wiring dwgs\TM-9305-005-Access Midstream-Hopeton Stabilizer Upgrades-Electrical Panel Wiring-IFC Revised.msg");
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void TestLongPathDirectoryNameWithInvalidChars()
		{
			Path.GetDirectoryName(longPathDirectory + '<');
		}

		[TestMethod]
		public void TestGetInvalidFileNameChars()
		{
			Assert.IsTrue(Path.GetInvalidFileNameChars().SequenceEqual(System.IO.Path.GetInvalidFileNameChars()));
		}

		[TestMethod]
		public void TestGetInvalidPathChars()
		{
			Assert.IsTrue(Path.GetInvalidPathChars().SequenceEqual(System.IO.Path.GetInvalidPathChars()));
		}

		[TestMethod]
		public void TestAltDirectorySeparatorChar()
		{
			Assert.AreEqual(System.IO.Path.AltDirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}

		[TestMethod]
		public void TestDirectorySeparatorChar()
		{
			Assert.AreEqual(System.IO.Path.DirectorySeparatorChar, Path.DirectorySeparatorChar);
		}

		[TestMethod]
		public void TestIsDirectorySeparator()
		{
			Assert.IsTrue(Path.IsDirectorySeparator(System.IO.Path.DirectorySeparatorChar));
			Assert.IsTrue(Path.IsDirectorySeparator(System.IO.Path.AltDirectorySeparatorChar));
		}

		[TestMethod]
		public void TestGetRootLength()
		{
			Assert.AreEqual(3, Path.GetRootLength(longPathFilename));
		}

		[TestMethod]
		public void TestGetRootLengthWithUnc()
		{
			Assert.AreEqual(2, Path.GetRootLength(@"\\servername\sharename\dir\filename.exe"));
		}

		[TestMethod]
		public void TestGetExtension()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Assert.AreEqual(tempLongPathFilename.Substring(tempLongPathFilename.Length - 4, 4),
				Path.GetExtension(tempLongPathFilename));
		}

		[TestMethod]
		public void TestGetPathRoot()
		{
			var root = Path.GetPathRoot(longPathDirectory);
			Assert.IsNotNull(root);
			Assert.AreEqual(3, root.Length);
		}

        [TestMethod]
        public void TestGetPathRootWithRelativePath()
        {
            var root = Path.GetPathRoot(@"foo\bar\baz");
            Assert.IsNotNull(root);
            Assert.AreEqual(0, root.Length);
        }

		[TestMethod]
		public void TestGetPathRootWithNullPath()
		{
			var root = Path.GetPathRoot(null);
			Assert.IsNull(root);
		}

		[TestMethod]
		public void TestNormalizeLongPath()
		{
			string result = Path.NormalizeLongPath(longPathDirectory);
			Assert.IsNotNull(result);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void TestNormalizeLongPathWithJustUncPrefix()
		{
			Path.NormalizeLongPath(@"\\");
		}

		[TestMethod]
		public void TestNormalizeLongPathWith()
		{
			string result = Path.NormalizeLongPath(longPathDirectory);
			Assert.IsNotNull(result);
		}


		[TestMethod]
		public void TestTryNormalizeLongPathWithJustUncPrefix()
		{
			string path;
			Assert.IsFalse(Path.TryNormalizeLongPath(@"\\", out path));
		}

		[TestMethod]
		public void TestTryNormalizeLongPat()
		{
			string path;
			Assert.IsTrue(Path.TryNormalizeLongPath(longPathDirectory, out path));
			Assert.IsNotNull(path);
		}

		[TestMethod]
		public void TestNormalizeLongPathWithEmptyPath()
		{
			string path;
			Assert.IsFalse(Path.TryNormalizeLongPath(String.Empty, out path));
		}

		[TestMethod]
		public void TestTryNormalizeLongPathWithNullPath()
		{
			string path;
			Assert.IsFalse(Path.TryNormalizeLongPath(null, out path));
		}

		[TestMethod, ExpectedException(typeof(PathTooLongException))]
		public void TestNormalizeLongPathWithHugePath()
		{
			var path = @"c:\";
			var component = Util.MakeLongComponent(path);
			component = component.Substring(3, component.Length - 3);
			while (path.Length < 32000)
			{
				path = Path.Combine(path, component);
			}
			Path.NormalizeLongPath(path);
		}

		[TestMethod]
		public void TestCombine()
		{
			const string expected = @"c:\Windows\system32";
			var actual = Path.Combine(@"c:\Windows", "system32");
			Assert.AreEqual(expected, actual);
		}

        [TestMethod]
	    public void TestCombineRelativePaths()
	    {
	        const string expected = @"foo\bar\baz\test";
            string actual = Path.Combine(@"foo\bar", @"baz\test");
            Assert.AreEqual(expected, actual);
	    }

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void TestCombineWithNull()
		{
			Path.Combine(null, null);
		}

		[TestMethod]
		public void TestCombineWithEmpthPath1()
		{
			Assert.AreEqual("test", Path.Combine("test", string.Empty));
		}

		[TestMethod]
		public void TestCombineWithEmpthPath2()
		{
			Assert.AreEqual(@"C:\test", Path.Combine(string.Empty, @"C:\test"));
		}

		[TestMethod]
		public void TestCombineWithEmpthPath1EndingInSeparator()
		{
			Assert.AreEqual(@"C:\test\test2", Path.Combine(@"C:\test\", "test2"));
		}

		[TestMethod]
		public void TestHasExtensionWithExtension()
		{
			Assert.IsTrue(Path.HasExtension(longPathFilename));
		}

		[TestMethod]
		public void TestHasExtensionWithoutExtension()
		{
			Assert.IsFalse(Path.HasExtension(longPathDirectory));
		}

		[TestMethod]
		public void TestGetTempPath()
		{
			string path = Path.GetTempPath();
			Assert.IsNotNull(path);
			Assert.IsTrue(path.Length > 0);
		}

		[TestMethod]
		public void TestGetTempFilename()
		{
			string filename = Path.GetTempFileName();
			Assert.IsNotNull(filename);
			Assert.IsTrue(filename.Length > 0);
		}

		[TestMethod]
		public void TestGetFileNameWithoutExtension()
		{
			var filename = Path.Combine(longPathDirectory, "filename.ext");

			Assert.AreEqual("filename", Path.GetFileNameWithoutExtension(filename));
		}

		[TestMethod]
		public void TestChangeExtension()
		{
			var filename = Path.Combine(longPathDirectory, "filename.ext");
			var expectedFilenameWithNewExtension = Path.Combine(longPathDirectory, "filename.txt");

			Assert.AreEqual(expectedFilenameWithNewExtension, Path.ChangeExtension(filename, ".txt"));
		}

		[TestMethod]
		public void TestCombineArray()
		{
			var strings = new[] { longPathDirectory, "subdir1", "subdir2", "filename.ext" };
			Assert.AreEqual(Path.Combine(Path.Combine(Path.Combine(longPathDirectory, "subdir1"), "subdir2"), "filename.ext"), Path.Combine(strings));
		}

		[TestMethod]
		public void TestCombineArrayOnePath()
		{
			var strings = new[] { longPathDirectory };
			Assert.AreEqual(longPathDirectory, Path.Combine(strings));
		}

		[TestMethod]
		public void TestCombineArrayTwoPaths()
		{
			var strings = new[] { longPathDirectory, "filename.ext" };
			Assert.AreEqual(Path.Combine(longPathDirectory, "filename.ext"), Path.Combine(strings));
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void TestCombineArrayNullPath()
		{
			Path.Combine((string[])null);
		}

		[TestMethod]
		public void TestCombineThreePaths()
		{
			Assert.AreEqual(Path.Combine(Path.Combine(longPathDirectory, "subdir1"), "filename.ext"),
				Path.Combine(longPathDirectory, "subdir1", "filename.ext"));
		}

		[TestMethod]
		public void TestCombineFourPaths()
		{
			Assert.AreEqual(Path.Combine(Path.Combine(Path.Combine(longPathDirectory, "subdir1"), "subdir2"), "filename.ext"),
				Path.Combine(longPathDirectory, "subdir1", "subdir2", "filename.ext"));
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void TestCombineTwoPathsOneNull()
		{
			Path.Combine(longPathDirectory, null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void TestCombineThreePathsOneNull()
		{
			Path.Combine(longPathDirectory, "subdir1", null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void TestCombineThreePathsTwoNulls()
		{
			Path.Combine(longPathDirectory, null, null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void TestCombineThreePathsThreeNulls()
		{
			Path.Combine(null, null, null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void TestCombineFourPathsOneNull()
		{
			Path.Combine(longPathDirectory, "subdir1", "subdir2", null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void TestCombineFourPathsTwoNull()
		{
			Path.Combine(longPathDirectory, "subdir1", null, null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void TestCombineFourPathsThreeNulls()
		{
			Path.Combine(longPathDirectory, null, null, null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void TestCombineFourPathsFourNulls()
		{
			Path.Combine(null, null, null, null);
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
