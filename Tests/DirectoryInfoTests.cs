using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.AccessControl;

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

	[TestClass]
	public class DirectoryInfoTests
	{
		private static string rootTestDir;
		private static string longPathDirectory;
		private static string longPathFilename;
		private const string Filename = "filename.ext";

		[ClassInitialize]
		public static void ClassInitialize(TestContext context)
		{
			rootTestDir = context.TestDir;
			longPathDirectory = Util.MakeLongPath(rootTestDir);
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
		public void TestExistsNonexistentDirectory()
		{
			var di = new DirectoryInfo("gibberish");
			Assert.IsFalse(di.Exists);
		}
		[TestMethod]
		public void TestExistsNonexistentParentDirectory()
		{
			var fi = new FileInfo(@"C:\.w\.y");

			Assert.IsFalse(fi.Directory.Exists);
		}

		[TestMethod]
		public void TestExistsOnExistantDirectory()
		{
			Assert.IsTrue(new DirectoryInfo(longPathDirectory).Exists);
		}

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
		public void TestParent()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var parent = di.Parent;
			Assert.IsNotNull(parent);
			Assert.AreEqual(Path.GetDirectoryName(longPathDirectory), parent.FullName);
		}

		[TestMethod]
		public void TestParentOnRoot()
		{
			var di = new DirectoryInfo(@"C:\");
			var parent = di.Parent;
			Assert.IsNull(parent);
		}

		[TestMethod]
		public void TestParentPathEndingWithSlash()
		{
			var di = new DirectoryInfo(longPathDirectory + @"\");
			var parent = di.Parent;
			Assert.IsNotNull(parent);
			Assert.AreEqual(Path.GetDirectoryName(longPathDirectory), parent.FullName);
		}

		[TestMethod]
		public void TestRoot()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var root = di.Root;
			Assert.IsNotNull(root);
			Assert.AreEqual(new System.IO.DirectoryInfo(rootTestDir).Root.FullName, root.FullName);
		}

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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


		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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
					Assert.AreEqual(2, di.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly).Count());
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

		[TestMethod]
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
					Assert.AreEqual(0, di.EnumerateFileSystemInfos("gibberish", SearchOption.TopDirectoryOnly).Count());
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
		public void TestToString()
		{
			var fi = new DirectoryInfo(longPathDirectory);

			Assert.AreEqual(fi.DisplayPath, fi.ToString());
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorWithNullPath()
		{
			new DirectoryInfo(null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void TestMoveToNullPath()
		{
			var randomDirectoryName = Path.GetRandomFileName();
			var tempLongPathDirectory = Path.Combine(longPathDirectory, randomDirectoryName);
			var di = new DirectoryInfo(tempLongPathDirectory);

			di.MoveTo(null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void TestMoveToEmptyPath()
		{
			var randomDirectoryName = Path.GetRandomFileName();
			var tempLongPathDirectory = Path.Combine(longPathDirectory, randomDirectoryName);
			var di = new DirectoryInfo(tempLongPathDirectory);

			di.MoveTo(string.Empty);
		}

		[TestMethod, ExpectedException(typeof(IOException))]
		public void TestMoveToSamePathWithSlash()
		{
			var randomDirectoryName = Path.GetRandomFileName();
			var tempLongPathDirectory = Path.Combine(longPathDirectory, randomDirectoryName) + @"\";
			var di = new DirectoryInfo(tempLongPathDirectory);

			di.MoveTo(tempLongPathDirectory);
		}

		[TestMethod, ExpectedException(typeof(IOException))]
		public void TestMoveToSamePath()
		{
			var randomDirectoryName = Path.GetRandomFileName();
			var tempLongPathDirectory = Path.Combine(longPathDirectory, randomDirectoryName);
			var di = new DirectoryInfo(tempLongPathDirectory);

			di.MoveTo(tempLongPathDirectory);
		}


		[TestMethod, ExpectedException(typeof(IOException))]
		public void TestMoveToDifferentRoot()
		{
			var randomDirectoryName = Path.GetRandomFileName();
			var tempLongPathDirectory = Path.Combine(longPathDirectory, randomDirectoryName);
			var di = new DirectoryInfo(tempLongPathDirectory);

			di.MoveTo(@"D:\");
		}
		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod, ExpectedException(typeof(FileNotFoundException))]
		public void TestSetCreationTimeMissingFile()
		{
			var filename = Path.Combine(longPathDirectory, "gibberish.ext");
			DateTime dateTime = DateTime.Now.AddDays(1);
			var di = new DirectoryInfo(filename);
			di.CreationTime = dateTime;
		}

		[TestMethod, ExpectedException(typeof(FileNotFoundException))]
		public void TestSetCreationTimeUtcMissingFile()
		{
			var filename = Path.Combine(longPathDirectory, "gibberish.ext");
			DateTime dateTime = DateTime.UtcNow.AddDays(1);
			var di = new DirectoryInfo(filename);
			di.CreationTimeUtc = dateTime;
		}

		[TestMethod, ExpectedException(typeof(FileNotFoundException))]
		public void TestSetLastWriteTimeMissingFile()
		{
			var filename = Path.Combine(longPathDirectory, "gibberish.ext");
			DateTime dateTime = DateTime.Now.AddDays(1);
			var di = new DirectoryInfo(filename);
			di.LastWriteTime = dateTime;
		}

		[TestMethod, ExpectedException(typeof(FileNotFoundException))]
		public void TestSetLastWriteTimeUtcMissingFile()
		{
			var filename = Path.Combine(longPathDirectory, "gibberish.ext");
			DateTime dateTime = DateTime.UtcNow.AddDays(1);
			var di = new DirectoryInfo(filename);
			di.LastWriteTimeUtc = dateTime;
		}

		[TestMethod, ExpectedException(typeof(FileNotFoundException))]
		public void TestSetLastAccessTimeMissingFile()
		{
			var filename = Path.Combine(longPathDirectory, "gibberish.ext");
			DateTime dateTime = DateTime.Now.AddDays(1);
			var di = new DirectoryInfo(filename);
			di.LastAccessTime = dateTime;
		}

		[TestMethod, ExpectedException(typeof(FileNotFoundException))]
		public void TestSetLastAccessTimeUtcMissingFile()
		{
			var filename = Path.Combine(longPathDirectory, "gibberish.ext");
			DateTime dateTime = DateTime.UtcNow.AddDays(1);
			var di = new DirectoryInfo(filename);
			di.LastAccessTimeUtc = dateTime;
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void TestCreateInvalidSubdirectory()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var newDi = di.CreateSubdirectory(@"\");
		}
		/// <remarks>
		/// TODO: more realistic DirectorySecurity scenarios
		/// </remarks>
		[TestMethod]
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

		[TestMethod]
		public void TestInstantiateWithDrive()
		{
			var di = new DirectoryInfo(@"C:");
			Assert.AreEqual(".", di.Name);
		}

		/// <remarks>
		/// TODO: more realistic DirectorySecurity scenarios
		/// </remarks>
		[TestMethod,]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
		public void TestGetFiles()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var files = di.GetFiles().ToArray();
			Assert.AreEqual(1, files.Length);
			Assert.IsTrue(files.Any(f => f.Name == Filename));
		}

		[TestMethod]
		public void TestGetFilesWithSearch()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var files = di.GetFiles("*").ToArray();
			Assert.AreEqual(1, files.Length);
		}

		[TestMethod]
		public void TestGetFilesWithSearchWithNoResults()
		{
			var di = new DirectoryInfo(longPathDirectory);
			var files = di.GetFiles("giberish").ToArray();
			Assert.AreEqual(0, files.Length);
		}

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[ClassCleanup]
		public static void ClassCleanup()
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
				Directory.Delete(longPathDirectory, true);
			}
		}
	}
}
