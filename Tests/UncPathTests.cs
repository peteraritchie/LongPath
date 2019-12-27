using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
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
    [TestFixture]
    public class UncPathTests
    {
        private static string uncDirectory;
        private static string uncFilePath;
        private static string directory;
        private static string filePath;
        private const string Filename = "filename.ext";

        [SetUp]
        public void SetUp()
        {
            directory = Path.Combine(TestContext.CurrentContext.TestDirectory, "subdir");
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
            catch (Exception ex)
            {
                Console.WriteLine("Exception " + ex.GetType().FullName + "occured\n" + ex.Message);
                if (System.IO.Directory.Exists(directory))
                    System.IO.Directory.Delete(directory, true);
                throw;
            }
        }

        [Test]
        public void TestGetDirectoryNameAtRoot()
        {
            string path = @"c:\";
            Assert.IsNull(Path.GetDirectoryName(path));
        }

        [Test]
        public void TestGetDirectoryNameWithNullPath()
        {
			Assert.Throws<ArgumentNullException>(() => Path.GetDirectoryName(null));
        }

        [Test]
        public void GetDirectoryNameOnRelativePath()
        {
            const string input = @"foo\bar\baz";
            const string expected = @"foo\bar";
            string actual = Path.GetDirectoryName(input);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetDirectoryNameOnRelativePathWithNoParent()
        {
            const string input = @"foo";
            const string expected = @"";
            string actual = Path.GetDirectoryName(input);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestGetParentAtRoot()
        {
            string path = "c:\\";
            Pri.LongPath.DirectoryInfo parent = Directory.GetParent(path);
            Assert.IsNull(parent);
        }

        [Test]
        public void TestLongPathDirectoryName()
        {
            var x = Path.GetDirectoryName(@"C:\Vault Data\w\M\Access Midstream\9305 Hopeton Stabilizer Upgrades\08  COMMUNICATION\8.1  Transmittals\9305-005 Access Midstream Hopeton - Electrical Panel Wiring dwgs\TM-9305-005-Access Midstream-Hopeton Stabilizer Upgrades-Electrical Panel Wiring-IFC Revised.msg");
        }

		[Test]
        public void TestLongPathDirectoryNameWithInvalidChars()
        {
			Assert.Throws<ArgumentException>(() => Path.GetDirectoryName(uncDirectory + '<'));
        }

        [Test]
        public void TestGetInvalidFileNameChars()
        {
            Assert.IsTrue(Path.GetInvalidFileNameChars().SequenceEqual(System.IO.Path.GetInvalidFileNameChars()));
        }

        [Test]
        public void TestGetInvalidPathChars()
        {
            Assert.IsTrue(Path.GetInvalidPathChars().SequenceEqual(System.IO.Path.GetInvalidPathChars()));
        }

        [Test]
        public void TestAltDirectorySeparatorChar()
        {
            Assert.AreEqual(System.IO.Path.AltDirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        [Test]
        public void TestDirectorySeparatorChar()
        {
            Assert.AreEqual(System.IO.Path.DirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        [Test]
        public void TestIsDirectorySeparator()
        {
            Assert.IsTrue(Path.IsDirectorySeparator(System.IO.Path.DirectorySeparatorChar));
            Assert.IsTrue(Path.IsDirectorySeparator(System.IO.Path.AltDirectorySeparatorChar));
        }

        [Test]
        public void TestGetRootLength()
        {
            Assert.AreEqual(15, Path.GetRootLength(uncFilePath));
        }

        [Test]
        public void TestGetRootLengthWithUnc()
        {
            Assert.AreEqual(23, Path.GetRootLength(@"\\servername\sharename\dir\filename.exe"));
        }

        [Test]
        public void TestGetExtension()
        {
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
            Assert.AreEqual(tempLongPathFilename.Substring(tempLongPathFilename.Length - 4, 4),
                Path.GetExtension(tempLongPathFilename));
        }

        [Test]
        public void TestGetPathRoot()
        {
            var root = Path.GetPathRoot(uncDirectory);
            Assert.IsNotNull(root);
            Assert.AreEqual(15, root.Length);
			Assert.IsTrue(@"\\localhost\C$\".Equals(root, StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void TestGetPathRootWithRelativePath()
        {
            var root = Path.GetPathRoot(@"foo\bar\baz");
            Assert.IsNotNull(root);
            Assert.AreEqual(0, root.Length);
        }

        [Test]
        public void TestGetPathRootWithNullPath()
        {
            var root = Path.GetPathRoot(null);
            Assert.IsNull(root);
        }

        [Test]
        public void TestNormalizeLongPath()
        {
            string result = Path.NormalizeLongPath(uncDirectory);
            Assert.IsNotNull(result);
        }

        [Test]
        public void TestNormalizeLongPathWithJustUncPrefix()
        {
			Assert.Throws<ArgumentException>(() => Path.NormalizeLongPath(@"\\"));
        }

        [Test]
        public void TestNormalizeLongPathWith()
        {
            string result = Path.NormalizeLongPath(uncDirectory);
            Assert.IsNotNull(result);
        }


        [Test]
        public void TestTryNormalizeLongPathWithJustUncPrefix()
        {
            string path;
            Assert.IsFalse(Path.TryNormalizeLongPath(@"\\", out path));
        }

        [Test]
        public void TestTryNormalizeLongPat()
        {
            string path;
            Assert.IsTrue(Path.TryNormalizeLongPath(uncDirectory, out path));
            Assert.IsNotNull(path);
        }

        [Test]
        public void TestNormalizeLongPathWithEmptyPath()
        {
            string path;
            Assert.IsFalse(Path.TryNormalizeLongPath(String.Empty, out path));
        }

        [Test]
        public void TestTryNormalizeLongPathWithNullPath()
        {
            string path;
            Assert.IsFalse(Path.TryNormalizeLongPath(null, out path));
        }

        [Test]
        public void TestNormalizeLongPathWithHugePath()
        {
            var path = @"c:\";
            var component = Util.MakeLongComponent(path);
            component = component.Substring(3, component.Length - 3);
            while (path.Length < 32000)
            {
                path = Path.Combine(path, component);
            }
			Assert.Throws<PathTooLongException>(() => Path.NormalizeLongPath(path));
        }

        [Test]
        public void TestCombine()
        {
            const string expected = @"c:\Windows\system32";
            var actual = Path.Combine(@"c:\Windows", "system32");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestCombineRelativePaths()
        {
            const string expected = @"foo\bar\baz\test";
            string actual = Path.Combine(@"foo\bar", @"baz\test");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestCombineWithNull()
        {
			Assert.Throws<ArgumentNullException>(() => Path.Combine(null, null));
        }

        [Test]
        public void TestCombineWithEmpthPath1()
        {
            Assert.AreEqual("test", Path.Combine("test", string.Empty));
        }

        [Test]
        public void TestCombineWithEmpthPath2()
        {
            Assert.AreEqual(@"C:\test", Path.Combine(string.Empty, @"C:\test"));
        }

        [Test]
        public void TestCombineWithEmpthPath1EndingInSeparator()
        {
            Assert.AreEqual(@"C:\test\test2", Path.Combine(@"C:\test\", "test2"));
        }

        [Test]
        public void TestHasExtensionWithExtension()
        {
            Assert.IsTrue(Path.HasExtension(uncFilePath));
        }

        [Test]
        public void TestHasExtensionWithoutExtension()
        {
            Assert.IsFalse(Path.HasExtension(uncDirectory));
        }

        [Test]
        public void TestGetTempPath()
        {
            string path = Path.GetTempPath();
            Assert.IsNotNull(path);
            Assert.IsTrue(path.Length > 0);
        }

        [Test]
        public void TestGetTempFilename()
        {
            string filename = Path.GetTempFileName();
            Assert.IsNotNull(filename);
            Assert.IsTrue(filename.Length > 0);
        }

        [Test]
        public void TestGetFileNameWithoutExtension()
        {
            var filename = Path.Combine(uncDirectory, "filename.ext");

            Assert.AreEqual("filename", Path.GetFileNameWithoutExtension(filename));
        }

        [Test]
        public void TestChangeExtension()
        {
            var filename = Path.Combine(uncDirectory, "filename.ext");
            var expectedFilenameWithNewExtension = Path.Combine(uncDirectory, "filename.txt");

            Assert.AreEqual(expectedFilenameWithNewExtension, Path.ChangeExtension(filename, ".txt"));
        }

        [Test]
        public void TestCombineArray()
        {
            var strings = new[] { uncDirectory, "subdir1", "subdir2", "filename.ext" };
            Assert.AreEqual(Path.Combine(Path.Combine(Path.Combine(uncDirectory, "subdir1"), "subdir2"), "filename.ext"), Path.Combine(strings));
        }

        [Test]
        public void TestCombineArrayOnePath()
        {
            var strings = new[] { uncDirectory };
            Assert.AreEqual(uncDirectory, Path.Combine(strings));
        }

        [Test]
        public void TestCombineArrayTwoPaths()
        {
            var strings = new[] { uncDirectory, "filename.ext" };
            Assert.AreEqual(Path.Combine(uncDirectory, "filename.ext"), Path.Combine(strings));
        }

		[Test]
        public void TestCombineArrayNullPath()
        {
			Assert.Throws<ArgumentNullException>(() => Path.Combine((string[])null));
        }

        [Test]
        public void TestCombineThreePaths()
        {
            Assert.AreEqual(Path.Combine(Path.Combine(uncDirectory, "subdir1"), "filename.ext"),
                Path.Combine(uncDirectory, "subdir1", "filename.ext"));
        }

        [Test]
        public void TestCombineFourPaths()
        {
            Assert.AreEqual(Path.Combine(Path.Combine(Path.Combine(uncDirectory, "subdir1"), "subdir2"), "filename.ext"),
                Path.Combine(uncDirectory, "subdir1", "subdir2", "filename.ext"));
        }

        [Test]
        public void TestCombineTwoPathsOneNull()
        {
			Assert.Throws<ArgumentNullException>(() => Path.Combine(uncDirectory, null));
        }

        [Test]
        public void TestCombineThreePathsOneNull()
        {
			Assert.Throws<ArgumentNullException>(() => Path.Combine(uncDirectory, "subdir1", null));
        }

        [Test]
        public void TestCombineThreePathsTwoNulls()
        {
			Assert.Throws<ArgumentNullException>(() => Path.Combine(uncDirectory, null, null));
        }

        [Test]
        public void TestCombineThreePathsThreeNulls()
        {
			Assert.Throws<ArgumentNullException>(() => Path.Combine(null, null, null));
        }

        [Test]
        public void TestCombineFourPathsOneNull()
        {
			Assert.Throws<ArgumentNullException>(() => Path.Combine(uncDirectory, "subdir1", "subdir2", null));
        }

        [Test]
        public void TestCombineFourPathsTwoNull()
        {
			Assert.Throws<ArgumentNullException>(() => Path.Combine(uncDirectory, "subdir1", null, null));
        }

        [Test]
        public void TestCombineFourPathsThreeNulls()
        {
			Assert.Throws<ArgumentNullException>(() => Path.Combine(uncDirectory, null, null, null));
        }

        [Test]
        public void TestCombineFourPathsFourNulls()
        {
			Assert.Throws<ArgumentNullException>(() => Path.Combine(null, null, null, null));
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }
            catch (Exception e)
            {
                Trace.WriteLine("Exception {0} deleting \"filePath\"", e.ToString());
                throw;
            }
            finally
            {
                if (System.IO.Directory.Exists(directory))
                    System.IO.Directory.Delete(directory, true);
            }
        }
    }
}
