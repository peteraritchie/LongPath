using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Security.AccessControl;
using NUnit.Framework;
using System.IO;

namespace Tests
{
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
	using FileNotFoundException = System.IO.FileNotFoundException;

	[TestFixture]
	public class DirectoryInfoTests
	{
		private static string rootTestDir;
		private static string longPathDirectory;
		private static string longPathFilename;
		private string shortPathFilename;
		private static string longPathRoot;
		private const string Filename = "filename.ext";

		[SetUp]
		public void SetUp()
		{
			rootTestDir = TestContext.CurrentContext.TestDirectory;
			longPathDirectory = Util.MakeLongPath(rootTestDir);
			longPathRoot = longPathDirectory.Substring(0, TestContext.CurrentContext.TestDirectory.Length + 1 + longPathDirectory.Substring(rootTestDir.Length + 1).IndexOf('\\'));
			Directory.CreateDirectory(longPathDirectory);
			Debug.Assert(Directory.Exists(longPathDirectory));
			longPathFilename = new StringBuilder(longPathDirectory).Append(@"\").Append(Filename).ToString();
			shortPathFilename = @".\" + Filename;
			using (var writer = File.CreateText(shortPathFilename))
			{
				writer.WriteLine("test");
			}
			using (var writer = File.CreateText(longPathFilename))
			{
				writer.WriteLine("test");
			}
			Debug.Assert(File.Exists(longPathFilename));
		}

		[Test]
		public void TestGetPathRoot()
		{
			var path = Path.GetPathRoot("\\\\root\\base");
		}

		[Test]
		public void TextExistenceOfFileWithDirectoryInfo()
		{
			var di = new DirectoryInfo(longPathFilename);
			Assert.IsFalse(di.Exists);
		}

		[Test]
		public void TestExistsNonexistentDirectory()
		{
			var di = new DirectoryInfo("gibberish");
			Assert.IsFalse(di.Exists);
		}
		[Test]
		public void TestExistsNonexistentParentDirectory()
		{
			var fi = new FileInfo(@"C:\.w\.y");

			Assert.IsFalse(fi.Directory.Exists);
		}

		[Test]
		public void TestExistsOnExistantDirectory()
		{
			Assert.IsTrue(new DirectoryInfo(longPathDirectory).Exists);
		}

		[Test]
		public void TestEnumerateDirectoriesWithSearch()
		{
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = Path.Combine(longPathDirectory, randomFileName);
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var di = new DirectoryInfo(longPathDirectory);
				var dirs = di.EnumerateDirectories("*").ToArray();
				Assert.AreEqual(1, dirs.Length);
				Assert.IsTrue(dirs.Any(f => f.Name == randomFileName));
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestEnumerateDirectoriesSearchWithNoResults()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var di = new DirectoryInfo(longPathDirectory);
				Assert.AreEqual(0, di.EnumerateDirectories("gibberish*").Count());
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestEnumerateFilesSearchWithResults()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var di = new DirectoryInfo(longPathDirectory);
				var files = di.EnumerateFiles("*").ToArray();
				Assert.AreEqual(1, files.Length);
				Assert.IsTrue(files.Any(f => f.Name == Filename));
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestEnumerateFilesSearchWithNoResults()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var di = new DirectoryInfo(longPathDirectory);
				Assert.AreEqual(0, di.EnumerateFiles("gibberish*").Count());
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestParent()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var parent = di.Parent;
			Assert.IsNotNull(parent);
			Assert.AreEqual(Path.GetDirectoryName(longPathDirectory), parent.FullName);
		}

		[Test]
		public void TestParentOnRoot()
		{
			var di = new DirectoryInfo(@"C:\");
			var parent = di.Parent;
			Assert.IsNull(parent);
		}

		[Test]
		public void TestParentPathEndingWithSlash()
		{
			var di = new DirectoryInfo(longPathDirectory + @"\");
			var parent = di.Parent;
			Assert.IsNotNull(parent);
			Assert.AreEqual(Path.GetDirectoryName(longPathDirectory), parent.FullName);
		}

		[Test]
		public void TestRoot()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var root = di.Root;
			Assert.IsNotNull(root);
			Assert.AreEqual(new System.IO.DirectoryInfo(rootTestDir).Root.FullName, root.FullName);
		}

		[Test]
		public void TestCreate()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			var di = new DirectoryInfo(tempLongPathFilename);
			di.Create();
			try
			{
				Assert.IsTrue(di.Exists);
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestCreateSubdirectory()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory(randomFileName);
			try
			{
				Assert.IsNotNull(newDi);
				Assert.IsTrue(di.Exists);
			}
			finally
			{
				newDi.Delete();
			}
		}

		[Test]
		public void TestEnumerateFileSystemInfos()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory(randomFileName);
			try
			{
				var fi = new FileInfo(Path.Combine(newDi.FullName, "filename"));
				using (fi.Create())
				{
				}
				try
				{
					Assert.AreEqual(1, newDi.EnumerateFileSystemInfos().Count());
				}
				finally
				{
					fi.Delete();
				}
			}
			finally
			{
				newDi.Delete(true);
			}
		}

		[Test]
		public void TestEnumerateFiles()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory(randomFileName);
			try
			{
				var fi = new FileInfo(Path.Combine(newDi.FullName, "filename"));
				using (fi.Create())
				{
				}
				try
				{
					Assert.AreEqual(1, di.EnumerateFiles().Count());
				}
				finally
				{
					fi.Delete();
				}
			}
			finally
			{
				newDi.Delete(true);
			}
		}

		[Test]
		public void TestEnumerateFilesRecursiveWithSearch()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory(randomFileName);
			try
			{
				var fi = new FileInfo(Path.Combine(newDi.FullName, "filename"));
				using (fi.Create())
				{
				}
				try
				{
					Assert.AreEqual(2, di.EnumerateFiles("*", SearchOption.AllDirectories).Count());
				}
				finally
				{
					fi.Delete();
				}
			}
			finally
			{
				newDi.Delete(true);
			}
		}

		[Test]
		public void TestEnumerateFilesWithSearch()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory(randomFileName);
			try
			{
				var fi = new FileInfo(Path.Combine(newDi.FullName, "filename"));
				using (fi.Create())
				{
				}
				try
				{
					Assert.AreEqual(1, newDi.EnumerateFiles("*").Count());
				}
				finally
				{
					fi.Delete();
				}
			}
			finally
			{
				newDi.Delete(true);
			}
		}

		[Test]
		public void TestEnumerateFilesWithSearchNoResults()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory(randomFileName);
			try
			{
				var fi = new FileInfo(Path.Combine(newDi.FullName, "filename"));
				using (fi.Create())
				{
				}
				try
				{
					Assert.AreEqual(0, newDi.EnumerateFiles("gibberish").Count());

				}
				finally
				{
					fi.Delete();
				}
			}
			finally
			{
				newDi.Delete(true);
			}
		}


		[Test]
		public void TestEnumerateFilesWithSearchAndOption()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory(randomFileName);
			try
			{
				var fi = new FileInfo(Path.Combine(newDi.FullName, "filename"));
				using (fi.Create())
				{
				}
				try
				{
					Assert.AreEqual(1, di.EnumerateFiles("*", SearchOption.TopDirectoryOnly).Count());
				}
				finally
				{
					fi.Delete();
				}
			}
			finally
			{
				newDi.Delete(true);
			}
		}

		[Test]
		public void TestEnumerateFilesWithSearchAndOptionNoResults()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory(randomFileName);
			try
			{
				var fi = new FileInfo(Path.Combine(newDi.FullName, "filename"));
				using (fi.Create())
				{
				}
				try
				{
					Assert.AreEqual(0, di.EnumerateFiles("gibberish", SearchOption.TopDirectoryOnly).Count());
				}
				finally
				{
					fi.Delete();
				}
			}
			finally
			{
				newDi.Delete(true);
			}
		}

		[Test]
		public void TestEnumerateFilesWithSearchRecursiveAndOption()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory(randomFileName);
			try
			{
				var fi = new FileInfo(Path.Combine(newDi.FullName, "filename"));
				using (fi.Create())
				{
				}
				try
				{
					Assert.AreEqual(0, di.EnumerateFiles("gibberish", SearchOption.AllDirectories).Count());
				}
				finally
				{
					fi.Delete();
				}
			}
			finally
			{
				newDi.Delete(true);
			}
		}

		[Test]
		public void TestEnumerateFileSystemInfosWithSearch()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory(randomFileName);
			try
			{
				var fi = new FileInfo(Path.Combine(newDi.FullName, "filename"));
				using (fi.Create())
				{
				}
				try
				{
					Assert.AreEqual(2, di.EnumerateFileSystemInfos("*").Count());
				}
				finally
				{
					fi.Delete();
				}
			}
			finally
			{
				newDi.Delete(true);
			}
		}

		[Test]
		public void TestEnumerateFileSystemInfosWithSearchNoResults()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory(randomFileName);
			try
			{
				var fi = new FileInfo(Path.Combine(newDi.FullName, "filename"));
				using (fi.Create())
				{
				}
				try
				{
					Assert.AreEqual(0, di.EnumerateFileSystemInfos("gibberish").Count());
				}
				finally
				{
					fi.Delete();
				}
			}
			finally
			{
				newDi.Delete(true);
			}
		}

		[Test]
		public void TestEnumerateFileSystemInfosWithSearchAndOptionMultipleResults()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory(randomFileName);
			try
			{
				var fi = new FileInfo(Path.Combine(newDi.FullName, "filename"));
				using (fi.Create())
				{
				}
				try
				{
					Assert.AreEqual(2, di.EnumerateFileSystemInfos("*").Count());
				}
				finally
				{
					fi.Delete();
				}
			}
			finally
			{
				newDi.Delete(true);
			}
		}

		[Test]
		public void TestEnumerateFileSystemInfosWithSearchAndOptionNoResults()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory(randomFileName);
			try
			{
				var fi = new FileInfo(Path.Combine(newDi.FullName, "filename"));
				using (fi.Create())
				{
				}
				try
				{
					Assert.AreEqual(0, di.EnumerateFileSystemInfos("gibberish").Count());
				}
				finally
				{
					fi.Delete();
				}
			}
			finally
			{
				newDi.Delete(true);
			}
		}

		[Test]
		public void TestEnumerateFileSystemInfosWithSearchRecursiveNoResults()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory(randomFileName);
			try
			{
				var fi = new FileInfo(Path.Combine(newDi.FullName, "filename"));
				using (fi.Create())
				{
				}
				try
				{
					Assert.AreEqual(0, di.EnumerateFileSystemInfos("gibberish", SearchOption.AllDirectories).Count());
				}
				finally
				{
					fi.Delete();
				}
			}
			finally
			{
				newDi.Delete(true);
			}
		}

		[Test]
		public void TestEnumerateDirectoriesWithSearchAndOption()
		{
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = Path.Combine(longPathDirectory, randomFileName);
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var di = new DirectoryInfo(longPathDirectory);
				var dirs = di.EnumerateDirectories("*", SearchOption.TopDirectoryOnly).ToArray();
				Assert.AreEqual(1, dirs.Length);
				Assert.IsTrue(dirs.Any(f => f.Name == randomFileName));
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestEnumerateDirectoriesWithWildcardSearchAndOptionNoResults()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var di = new DirectoryInfo(longPathDirectory);
				Assert.AreEqual(0, di.EnumerateDirectories("gibberish*", SearchOption.TopDirectoryOnly).Count());
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestEnumerateDirectoriesWithSearchRecursive()
		{
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = Path.Combine(longPathDirectory, randomFileName);
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var di = new DirectoryInfo(longPathDirectory);
				var dirs = di.EnumerateDirectories("*", SearchOption.AllDirectories).ToArray();
				Assert.AreEqual(1, dirs.Length);
				Assert.IsTrue(dirs.Any(f => f.Name == randomFileName));
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestEnumerateDirectoriesWithSearchRecursiveNoResults()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var di = new DirectoryInfo(longPathDirectory);
				Assert.AreEqual(0, di.EnumerateDirectories("gibberish*", SearchOption.AllDirectories).Count());
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestMoveTo()
		{
			var randomFileName = Path.GetRandomFileName();
			var randomFileName2 = Path.GetRandomFileName();
			var tempLongPathFilename = Path.Combine(longPathDirectory, randomFileName);
			var tempLongPathFilename2 = Path.Combine(longPathDirectory, randomFileName2);
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var di = new DirectoryInfo(tempLongPathFilename);
				di.MoveTo(tempLongPathFilename2);
				di = new DirectoryInfo(longPathDirectory);
				var dirs = di.EnumerateDirectories("*", SearchOption.TopDirectoryOnly).ToArray();
				Assert.AreEqual(1, dirs.Length);
				Assert.IsTrue(dirs.Any(f => f.Name == randomFileName2));
				Assert.IsFalse(dirs.Any(f => f.Name == randomFileName));
			}
			finally
			{
				Directory.Delete(tempLongPathFilename2);
			}
			Assert.IsFalse(Directory.Exists(tempLongPathFilename));
		}

		[Test]
		public void TestToString()
		{
			var fi = new DirectoryInfo(longPathDirectory);

			Assert.AreEqual(fi.DisplayPath, fi.ToString());
		}

		[Test]
		public void TestConstructorWithNullPath()
		{
			Assert.Throws<ArgumentNullException>(() => new DirectoryInfo(null));
		}

		[Test]
		public void TestMoveToNullPath()
		{
			var randomDirectoryName = Path.GetRandomFileName();
			var tempLongPathDirectory = Path.Combine(longPathDirectory, randomDirectoryName);
			var di = new DirectoryInfo(tempLongPathDirectory);

			Assert.Throws<ArgumentNullException>(() => di.MoveTo(null));
		}

		[Test]
		public void TestMoveToEmptyPath()
		{
			var randomDirectoryName = Path.GetRandomFileName();
			var tempLongPathDirectory = Path.Combine(longPathDirectory, randomDirectoryName);
			var di = new DirectoryInfo(tempLongPathDirectory);

			Assert.Throws<ArgumentException>(() => di.MoveTo(string.Empty));
		}

		[Test]
		public void TestMoveToSamePathWithSlash()
		{
			var randomDirectoryName = Path.GetRandomFileName();
			var tempLongPathDirectory = Path.Combine(longPathDirectory, randomDirectoryName) + @"\";
			var di = new DirectoryInfo(tempLongPathDirectory);

			Assert.Throws<IOException>(() => di.MoveTo(tempLongPathDirectory));
		}

		[Test]
		public void TestMoveToSamePath()
		{
			var randomDirectoryName = Path.GetRandomFileName();
			var tempLongPathDirectory = Path.Combine(longPathDirectory, randomDirectoryName);
			var di = new DirectoryInfo(tempLongPathDirectory);

			Assert.Throws<IOException>(() => di.MoveTo(tempLongPathDirectory));
		}


		[Test]
		public void TestMoveToDifferentRoot()
		{
			var randomDirectoryName = Path.GetRandomFileName();
			var tempLongPathDirectory = Path.Combine(longPathDirectory, randomDirectoryName);
			var di = new DirectoryInfo(tempLongPathDirectory);

			Assert.Throws<IOException>(() => di.MoveTo(@"D:\"));
		}

		[Test]
		public void TestSetCreationTime()
		{
			var filename = Util.CreateNewFile(longPathDirectory);
			try
			{
				DateTime dateTime = DateTime.Now.AddDays(1);
				var di = new DirectoryInfo(filename);
				di.CreationTime = dateTime;
				Assert.AreEqual(dateTime, File.GetCreationTime(filename));
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestSetCreationTimeUtc()
		{
			var filename = Util.CreateNewFile(longPathDirectory);
			try
			{
				DateTime dateTime = DateTime.UtcNow.AddDays(1);
				var di = new DirectoryInfo(filename);
				di.CreationTimeUtc = dateTime;
				Assert.AreEqual(dateTime, File.GetCreationTimeUtc(filename));
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestLastWriteTimeOnRoot()
		{
			var di = new DirectoryInfo(@"c:\");
			var ignore = di.LastWriteTime;
		}

		[Test]
		public void TestSetLastWriteTime()
		{
			var filename = Util.CreateNewFile(longPathDirectory);
			try
			{
				DateTime dateTime = DateTime.Now.AddDays(1);
				var di = new DirectoryInfo(filename);
				di.LastWriteTime = dateTime;
				Assert.AreEqual(dateTime, File.GetLastWriteTime(filename));
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestSetLastWriteTimeUtc()
		{
			var filename = Util.CreateNewFile(longPathDirectory);
			try
			{
				DateTime dateTime = DateTime.UtcNow.AddDays(1);
				var di = new DirectoryInfo(filename);
				di.LastWriteTimeUtc = dateTime;
				Assert.AreEqual(dateTime, File.GetLastWriteTimeUtc(filename));
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestSetLastAccessTime()
		{
			var filename = Util.CreateNewFile(longPathDirectory);
			try
			{
				DateTime dateTime = DateTime.Now.AddDays(1);
				var di = new DirectoryInfo(filename);
				di.LastAccessTime = dateTime;
				Assert.AreEqual(dateTime, File.GetLastAccessTime(filename));
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestSetLastAccessTimeUtc()
		{
			var filename = Util.CreateNewFile(longPathDirectory);
			try
			{
				DateTime dateTime = DateTime.UtcNow.AddDays(1);
				var di = new DirectoryInfo(filename);
				di.LastAccessTimeUtc = dateTime;
				Assert.AreEqual(dateTime, File.GetLastAccessTimeUtc(filename));
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestSetCreationTimeMissingFile()
		{
			var filename = Path.Combine(longPathDirectory, "gibberish.ext");
			DateTime dateTime = DateTime.Now.AddDays(1);
			var di = new DirectoryInfo(filename);
			Assert.Throws<FileNotFoundException>(() => di.CreationTime = dateTime);
		}

		[Test]
		public void TestSetCreationTimeUtcMissingFile()
		{
			var filename = Path.Combine(longPathDirectory, "gibberish.ext");
			DateTime dateTime = DateTime.UtcNow.AddDays(1);
			var di = new DirectoryInfo(filename);
			Assert.Throws<FileNotFoundException>(() => di.CreationTimeUtc = dateTime);
		}

		[Test]
		public void TestSetLastWriteTimeMissingFile()
		{
			var filename = Path.Combine(longPathDirectory, "gibberish.ext");
			DateTime dateTime = DateTime.Now.AddDays(1);
			var di = new DirectoryInfo(filename);
			Assert.Throws<FileNotFoundException>(() => di.LastWriteTime = dateTime);
		}

		[Test]
		public void TestSetLastWriteTimeUtcMissingFile()
		{
			var filename = Path.Combine(longPathDirectory, "gibberish.ext");
			DateTime dateTime = DateTime.UtcNow.AddDays(1);
			var di = new DirectoryInfo(filename);
			Assert.Throws<FileNotFoundException>(() => di.LastWriteTimeUtc = dateTime);
		}

		[Test]
		public void TestSetLastAccessTimeMissingFile()
		{
			var filename = Path.Combine(longPathDirectory, "gibberish.ext");
			DateTime dateTime = DateTime.Now.AddDays(1);
			var di = new DirectoryInfo(filename);
			Assert.Throws<FileNotFoundException>(() => di.LastAccessTime = dateTime);
		}

		[Test]
		public void TestSetLastAccessTimeUtcMissingFile()
		{
			var filename = Path.Combine(longPathDirectory, "gibberish.ext");
			DateTime dateTime = DateTime.UtcNow.AddDays(1);
			var di = new DirectoryInfo(filename);
			Assert.Throws<FileNotFoundException>(() => di.LastAccessTimeUtc = dateTime);
		}

		[Test]
		public void TestCreateInvalidSubdirectory()
		{
			var di = new DirectoryInfo(longPathDirectory);
			Assert.Throws<ArgumentException>(() => { var newDi = di.CreateSubdirectory(@"\"); });
		}

		/// <remarks>
		/// TODO: more realistic DirectorySecurity scenarios
		/// </remarks>
		[Test]
		public void TestCreateSubdirectoryWithFileSecurity()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var randomFileName = Path.GetRandomFileName();
			Pri.LongPath.DirectoryInfo newDi = null;
			try
			{
				newDi = di.CreateSubdirectory(randomFileName, new DirectorySecurity());
				Assert.IsNotNull(newDi);
				Assert.IsTrue(di.Exists);
			}
			finally
			{
				if(newDi != null) newDi.Delete();
			}
		}

		[Test]
		public void TestInstantiateWithDrive()
		{
			var di = new DirectoryInfo(@"C:");
			Assert.AreEqual(".", di.Name);
		}

		/// <remarks>
		/// TODO: more realistic DirectorySecurity scenarios
		/// </remarks>
		[Test,]
		public void TestCreateWithFileSecurity()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			var di = new DirectoryInfo(tempLongPathFilename);
			try
			{
				di.Create(new DirectorySecurity());
				Assert.IsTrue(Directory.Exists(tempLongPathFilename));
			}
			finally
			{
				di.Delete();
			}
		}

		[Test]
		public void TestEnumerateDirectories()
		{
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = Path.Combine(longPathDirectory, randomFileName);
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var di = new DirectoryInfo(longPathDirectory);
				var dirs = di.EnumerateDirectories().ToArray();
				Assert.AreEqual(1, dirs.Length);
				Assert.IsTrue(dirs.Any(f => f.Name == randomFileName));
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestRecursiveEnumerateDirectoriesWithSearch()
		{
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = Path.Combine(longPathDirectory, randomFileName);
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var di = new DirectoryInfo(longPathDirectory);
				var dirs = di.EnumerateDirectories("*", SearchOption.AllDirectories).ToArray();
				Assert.AreEqual(1, dirs.Length);
				Assert.IsTrue(dirs.Any(f => f.Name == randomFileName));
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestRecursiveEnumerateDirectoriesWithSearchNoResults()
		{
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = Path.Combine(longPathDirectory, randomFileName);
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var di = new DirectoryInfo(longPathDirectory);
				var dirs = di.EnumerateDirectories("gibberish", SearchOption.AllDirectories).ToArray();
				Assert.AreEqual(0, dirs.Length);
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestEnumerateDirectoriesWithSearchWithNoResults()
		{
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = Path.Combine(longPathDirectory, randomFileName);
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var di = new DirectoryInfo(longPathDirectory);
				var dirs = di.EnumerateDirectories("gibberish").ToArray();
				Assert.AreEqual(0, dirs.Length);
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[Test, Ignore("does not work on some server/domain systems.")]
		public void TestGetAccessControl()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var di = new DirectoryInfo(tempLongPathFilename);
				var security = di.GetAccessControl();
				Assert.IsNotNull(security);
				Assert.AreEqual(typeof(FileSystemRights), security.AccessRightType);
				Assert.AreEqual(typeof(FileSystemAccessRule), security.AccessRuleType);
				Assert.AreEqual(typeof(FileSystemAuditRule), security.AuditRuleType);
				Assert.IsTrue(security.AreAccessRulesCanonical);
				Assert.IsTrue(security.AreAuditRulesCanonical);
				Assert.IsFalse(security.AreAccessRulesProtected);
				Assert.IsFalse(security.AreAuditRulesProtected);
				AuthorizationRuleCollection perm = security.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
				var ntAccount = new System.Security.Principal.NTAccount(System.Security.Principal.WindowsIdentity.GetCurrent().Name);
				FileSystemAccessRule rule = perm.Cast<FileSystemAccessRule>().SingleOrDefault(e => ntAccount == e.IdentityReference);
				Assert.IsNotNull(rule);
				Assert.IsTrue((rule.FileSystemRights & FileSystemRights.FullControl) != 0);
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[Test, Ignore("does not work on some server/domain systems.")]
		public void TestGetAccessControlSections()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var di = new DirectoryInfo(tempLongPathFilename);
				var security = di.GetAccessControl(AccessControlSections.Access);
				Assert.IsNotNull(security);
				Assert.AreEqual(typeof(FileSystemRights), security.AccessRightType);
				Assert.AreEqual(typeof(FileSystemAccessRule), security.AccessRuleType);
				Assert.AreEqual(typeof(FileSystemAuditRule), security.AuditRuleType);
				Assert.IsTrue(security.AreAccessRulesCanonical);
				Assert.IsTrue(security.AreAuditRulesCanonical);
				Assert.IsFalse(security.AreAccessRulesProtected);
				Assert.IsFalse(security.AreAuditRulesProtected);
				var securityGetAccessRules = security.GetAuditRules(true, true, typeof(System.Security.Principal.NTAccount)).Cast<FileSystemAccessRule>();
				Assert.AreEqual(0, securityGetAccessRules.Count());
				AuthorizationRuleCollection perm = security.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
				var ntAccount = new System.Security.Principal.NTAccount(System.Security.Principal.WindowsIdentity.GetCurrent().Name);
				FileSystemAccessRule rule = perm.Cast<FileSystemAccessRule>().SingleOrDefault(e => ntAccount == e.IdentityReference);
				Assert.IsNotNull(rule);
				Assert.IsTrue((rule.FileSystemRights & FileSystemRights.FullControl) != 0);
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestGetDirectories()
		{
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = Path.Combine(longPathDirectory, randomFileName);
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var di = new DirectoryInfo(longPathDirectory);
				var dirs = di.GetDirectories();
				Assert.AreEqual(1, dirs.Length);
				Assert.IsTrue(dirs.Any(f => f.Name == randomFileName));
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestGetDirectoriesWithAllSearch()
		{
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = Path.Combine(longPathDirectory, randomFileName);
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var di = new DirectoryInfo(longPathDirectory);
				var dirs = di.GetDirectories("*").ToArray();
				Assert.AreEqual(1, dirs.Length);
				Assert.IsTrue(dirs.Any(f => f.Name == randomFileName));
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestGetDirectoriesWithSingleResultSubsetSearch()
		{
			var randomFileName = "TestGetDirectoriesWithSubsetSearch";
			var tempLongPathFilename = Path.Combine(longPathDirectory, randomFileName);
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var di = new DirectoryInfo(longPathDirectory);
				var dirs = di.GetDirectories("A*").ToArray();
				Assert.AreEqual(0, dirs.Length);
				Assert.IsFalse(dirs.Any(f => f.Name == randomFileName));
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestGetDirectoriesWithMultipleResultSubsetSearch()
		{
			var randomFileName = "TestGetDirectoriesWithMultipleResultSubsetSearch";
			var randomFileName2 = "ATestGetDirectoriesWithMultipleResultSubsetSearch";
			var tempLongPathFilename = Path.Combine(longPathDirectory, randomFileName);
			var tempLongPathFilename2 = Path.Combine(longPathDirectory, randomFileName2);
			Directory.CreateDirectory(tempLongPathFilename);
			Directory.CreateDirectory(tempLongPathFilename2);
			try
			{
				var di = new DirectoryInfo(longPathDirectory);
				var dirs = di.GetDirectories("A*").ToArray();
				Assert.AreEqual(1, dirs.Length);
				Assert.IsTrue(dirs.Any(f => f.Name == randomFileName2));
				Assert.IsFalse(dirs.Any(f => f.Name == randomFileName));
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
				Directory.Delete(tempLongPathFilename2);
			}
		}

		[Test]
		public void TestRecursiveGetDirectoriesWithSearch()
		{
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = Path.Combine(longPathDirectory, randomFileName);
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var di = new DirectoryInfo(longPathDirectory);
				var dirs = di.GetDirectories("*", SearchOption.AllDirectories).ToArray();
				Assert.AreEqual(1, dirs.Length);
				Assert.IsTrue(dirs.Any(f => f.Name == randomFileName));
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestGetFiles()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var files = di.GetFiles().ToArray();
			Assert.AreEqual(1, files.Length);
			Assert.IsTrue(files.Any(f => f.Name == Filename));
		}

		[Test]
		public void TestGetFilesWithSearch()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var files = di.GetFiles("*").ToArray();
			Assert.AreEqual(1, files.Length);
		}

		[Test]
		public void TestGetFilesWithSearchWithNoResults()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var files = di.GetFiles("giberish").ToArray();
			Assert.AreEqual(0, files.Length);
		}

		[Test]
		public void TestGetRecursiveFilesWithAllSearch()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				Assert.IsTrue(Directory.Exists(tempLongPathFilename));
				string newEmptyFile = Util.CreateNewEmptyFile(tempLongPathFilename);
				try
				{
					var randomFileName = Path.GetFileName(newEmptyFile);

					var di = new DirectoryInfo(longPathDirectory);
					var files = di.GetFiles("*", SearchOption.AllDirectories).ToArray();
					Assert.AreEqual(2, files.Length);
					Assert.IsTrue(files.Any(f => f.Name == Filename));
					Assert.IsTrue(files.Any(f => f.Name == randomFileName));

				}
				finally
				{
					File.Delete(newEmptyFile);
				}
			}
			finally
			{
				const bool recursive = true;
				Directory.Delete(tempLongPathFilename, recursive);
			}
		}

		[Test]
		public void TestGetRecursiveFilesWithSubsetSearch()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				Assert.IsTrue(Directory.Exists(tempLongPathFilename));
				string newEmptyFile1 = Util.CreateNewEmptyFile(tempLongPathFilename, "A-file");
				string newEmptyFile2 = Util.CreateNewEmptyFile(tempLongPathFilename, "B-file");
				try
				{
					var randomFileName = Path.GetFileName(newEmptyFile1);

					var di = new DirectoryInfo(longPathDirectory);
					var files = di.GetFiles("A*", SearchOption.AllDirectories).ToArray();
					Assert.AreEqual(1, files.Length);
					Assert.IsTrue(files.Any(f => f.Name == Path.GetFileName(newEmptyFile1) && f.DirectoryName == Path.GetDirectoryName(newEmptyFile1)));
					Assert.IsFalse(files.Any(f => f.Name == Path.GetFileName(newEmptyFile2) && f.DirectoryName == Path.GetDirectoryName(newEmptyFile2)));
					Assert.IsFalse(files.Any(f => f.Name == Path.GetFileName(Filename) && f.DirectoryName == Path.GetDirectoryName(Filename)));
				}
				finally
				{
					File.Delete(newEmptyFile1);
					File.Delete(newEmptyFile2);
				}
			}
			finally
			{
				const bool recursive = true;
				Directory.Delete(tempLongPathFilename, recursive);
			}
		}

		[Test]
		public void TestGetFileSystemInfos()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory(randomFileName);
			try
			{
				var fi = new FileInfo(Path.Combine(newDi.FullName, "filename"));
				using (fi.Create())
				{
				}
				try
				{
					Assert.AreEqual(1, newDi.GetFileSystemInfos().Count());
				}
				finally
				{
					fi.Delete();
				}
			}
			finally
			{
				newDi.Delete(true);
			} 
		}

		[Test]
		public void TestGetFileSystemInfosWithSearch()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory(randomFileName);
			try
			{
				var fi = new FileInfo(Path.Combine(newDi.FullName, "filename"));
				using (fi.Create())
				{
				}
				try
				{
					Assert.AreEqual(2, di.GetFileSystemInfos("*").Count());
				}
				finally
				{
					fi.Delete();
				}
			}
			finally
			{
				newDi.Delete(true);
			}
		}

		[Test]
		public void TestGetFileSystemInfosWithSearchNoResults()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory(randomFileName);
			try
			{
				var fi = new FileInfo(Path.Combine(newDi.FullName, "filename"));
				using (fi.Create())
				{
				}
				try
				{
					Assert.AreEqual(0, di.GetFileSystemInfos("gibberish").Count());
				}
				finally
				{
					fi.Delete();
				}
			}
			finally
			{
				newDi.Delete(true);
			}
		}

		[Test]
		public void TestGetFileSystemInfosWithSearchAndOptionMultipleResults()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory(randomFileName);
			try
			{
				var fi = new FileInfo(Path.Combine(newDi.FullName, "filename"));
				using (fi.Create())
				{
				}
				try
				{
					Assert.AreEqual(2, di.GetFileSystemInfos("*", SearchOption.TopDirectoryOnly).Count());
				}
				finally
				{
					fi.Delete();
				}
			}
			finally
			{
				newDi.Delete(true);
			}
		}

		[Test]
		public void TestGetFileSystemInfosWithSearchAndOptionNoResults()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory(randomFileName);
			try
			{
				var fi = new FileInfo(Path.Combine(newDi.FullName, "filename"));
				using (fi.Create())
				{
				}
				try
				{
					Assert.AreEqual(0, di.GetFileSystemInfos("gibberish", SearchOption.TopDirectoryOnly).Count());
				}
				finally
				{
					fi.Delete();
				}
			}
			finally
			{
				newDi.Delete(true);
			}
		}

		[Test]
		public void TestGetFileSystemInfosWithSearchRecursiveNoResults()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory(randomFileName);
			try
			{
				var fi = new FileInfo(Path.Combine(newDi.FullName, "filename"));
				using (fi.Create())
				{
				}
				try
				{
					Assert.AreEqual(0, di.GetFileSystemInfos("gibberish", SearchOption.AllDirectories).Count());
				}
				finally
				{
					fi.Delete();
				}
			}
			finally
			{
				newDi.Delete(true);
			}
		}

		[Test]
		public void TestGetLastWriteTimeUtcWithTrailingBackslashDoesNotThrow()
		{
			var di = new DirectoryInfo(longPathDirectory + Path.DirectorySeparatorChar);
			var date = di.LastWriteTimeUtc;
		}

		[TearDown]
		public void TearDown()
		{
			try
			{
				File.Delete(longPathFilename);
			}
			catch (Exception e)
			{
				Trace.WriteLine("Exception {0} deleting \"longPathFilename\"", e.ToString());
				throw;
			}
			finally
			{
				Directory.Delete(longPathRoot, true);
			}
		}
	}
}
