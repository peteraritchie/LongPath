using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
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
	using FileNotFoundException = System.IO.FileNotFoundException;
	using FileAttributes = System.IO.FileAttributes;
	using IOException = System.IO.IOException;
	using SearchOption = System.IO.SearchOption;

	[TestClass]
	public class DirectoryTests
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
		public void TestExists()
		{
			Assert.IsTrue(Directory.Exists(longPathDirectory));
		}

		/// <remarks>
		/// Tests Directory.GetParent,
		/// depends on Directory.Combine, DirectoryInfo.FullName
		/// </remarks>
		[TestMethod]
		public void TestGetParent()
		{
			var actual = Directory.GetParent(Path.Combine(longPathDirectory, "system32"));
			Assert.AreEqual(longPathDirectory, actual.FullName);
		}

		/// <remarks>
		/// TODO: test some error conditions.
		/// </remarks>
		[TestMethod]
		public void TestCreateDirectory()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			var di = Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				Assert.IsNotNull(di);
				Assert.IsTrue(Directory.Exists(tempLongPathFilename));
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[TestMethod]
		public void TestCreateDirectoryThatEndsWithSlash()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName() + @"\");
			var di = Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				Assert.IsNotNull(di);
				Assert.IsTrue(Directory.Exists(tempLongPathFilename));
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[TestMethod]
		public void TestCreateDirectoryThatAlreadyExists()
		{
			var di = Directory.CreateDirectory(longPathDirectory);
			Assert.IsNotNull(di);
			Assert.IsTrue(Directory.Exists(longPathDirectory));
		}

		/// <remarks>
		/// Tests <see cref="Directory.EnumerateDirectories(string)"/>, depends on <see cref="Directory.CreateDirectory"/>
		/// </remarks>
		[TestMethod]
		public void TestEnumerateDirectories()
		{
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = Path.Combine(longPathDirectory, randomFileName);
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var dirs = Directory.EnumerateDirectories(longPathDirectory).ToArray();
				Assert.AreEqual(1, dirs.Length);
				Assert.IsTrue(dirs.Contains(tempLongPathFilename));
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[TestMethod]
		public void TestEnumerateDirectoriesWithSearch()
		{
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = Path.Combine(longPathDirectory, randomFileName);
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var dirs = Directory.EnumerateDirectories(longPathDirectory, "*").ToArray();
				Assert.AreEqual(1, dirs.Length);
				Assert.IsTrue(dirs.Contains(tempLongPathFilename));
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
				var dirs = Directory.EnumerateDirectories(longPathDirectory, "*", SearchOption.AllDirectories).ToArray();
				Assert.AreEqual(1, dirs.Length);
				Assert.IsTrue(dirs.Contains(tempLongPathFilename));
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
				var dirs = Directory.EnumerateDirectories(longPathDirectory, "gibberish", SearchOption.AllDirectories).ToArray();
				Assert.AreEqual(0, dirs.Length);
				Assert.IsFalse(dirs.Contains(tempLongPathFilename));
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
				var dirs = Directory.EnumerateDirectories(longPathDirectory, "gibberish").ToArray();
				Assert.AreEqual(0, dirs.Length);
				Assert.IsFalse(dirs.Contains(tempLongPathFilename));
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		/// <remarks>
		/// Tests <see cref="Directory.EnumerateDirectories(string)"/>, depends on <see cref="Directory.CreateDirectory"/>
		/// </remarks>
		[TestMethod]
		public void TestEnumerateFiles()
		{
			var files = Directory.EnumerateFiles(longPathDirectory).ToArray();
			Assert.AreEqual(1, files.Length);
			Assert.IsTrue(files.Contains(longPathFilename));
		}

		[TestMethod]
		public void TestEnumerateFilesWithSearch()
		{
			var files = Directory.EnumerateFiles(longPathDirectory, "*").ToArray();
			Assert.AreEqual(1, files.Length);
			Assert.IsTrue(files.Contains(longPathFilename));
		}

		[TestMethod]
		public void TestEnumerateFilesWithSearchWithNoResults()
		{
			var files = Directory.EnumerateFiles(longPathDirectory, "giberish").ToArray();
			Assert.AreEqual(0, files.Length);
			Assert.IsFalse(files.Contains(longPathFilename));
		}

		[TestMethod]
		public void TestEnumerateRecursiveFilesWithSearch()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				Assert.IsTrue(Directory.Exists(tempLongPathFilename));
				var randomFileName = Util.CreateNewEmptyFile(tempLongPathFilename);

				var files = Directory.EnumerateFiles(longPathDirectory, "*", SearchOption.AllDirectories).ToArray();
				Assert.AreEqual(2, files.Length);
				Assert.IsTrue(files.Contains(longPathFilename));
				Assert.IsTrue(files.Contains(randomFileName));
			}
			finally
			{
				const bool recursive = true;
				Directory.Delete(tempLongPathFilename, recursive);
			}
		}

		[TestMethod]
		public void TestEnumerateFilesRecursiveWithSearchWithNoResults()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				Assert.IsTrue(Directory.Exists(tempLongPathFilename));
				var randomFileName = Util.CreateNewEmptyFile(tempLongPathFilename);

				var files = Directory.EnumerateFiles(longPathDirectory, "gibberish", SearchOption.AllDirectories).ToArray();
				Assert.AreEqual(0, files.Length);
				Assert.IsFalse(files.Contains(longPathFilename));
				Assert.IsFalse(files.Contains(randomFileName));
			}
			finally
			{
				const bool recursive = true;
				Directory.Delete(tempLongPathFilename, recursive);
			}
		}

		[TestMethod]
		public void TestEnumerateFileSystemEntries()
		{
			var entries = Directory.EnumerateFileSystemEntries(longPathDirectory).ToArray();
			Assert.AreEqual(1, entries.Length);
			Assert.IsTrue(entries.Contains(longPathFilename));
		}

		[TestMethod]
		public void TestEnumerateFileSystemEntriesWithSearch()
		{
			var entries = Directory.EnumerateFileSystemEntries(longPathDirectory, "*").ToArray();
			Assert.AreEqual(1, entries.Length);
			Assert.IsTrue(entries.Contains(longPathFilename));
		}

		[TestMethod]
		public void TestEnumerateFileSystemEntriesWithSearchWithNoResults()
		{
			var entries = Directory.EnumerateFileSystemEntries(longPathDirectory, "giberish").ToArray();
			Assert.AreEqual(0, entries.Length);
			Assert.IsFalse(entries.Contains(longPathFilename));
		}

		[TestMethod]
		public void TestEnumerateRecursiveFileSystemEntriesWithSearch()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				Assert.IsTrue(Directory.Exists(tempLongPathFilename));
				var randomFileName = Util.CreateNewEmptyFile(tempLongPathFilename);

				var entries = Directory.EnumerateFileSystemEntries(longPathDirectory, "*", SearchOption.AllDirectories).ToArray();
				Assert.AreEqual(3, entries.Length);
				Assert.IsTrue(entries.Contains(longPathFilename));
				Assert.IsTrue(entries.Contains(randomFileName));
			}
			finally
			{
				const bool recursive = true;
				Directory.Delete(tempLongPathFilename, recursive);
			}
		}

		[TestMethod]
		public void TestEnumerateFileSystemEntriesRecursiveWithSearchWithNoResults()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				Assert.IsTrue(Directory.Exists(tempLongPathFilename));
				var randomFileName = Util.CreateNewEmptyFile(tempLongPathFilename);

				var files = Directory.EnumerateFileSystemEntries(longPathDirectory, "gibberish", SearchOption.AllDirectories).ToArray();
				Assert.AreEqual(0, files.Length);
				Assert.IsFalse(files.Contains(longPathFilename));
				Assert.IsFalse(files.Contains(randomFileName));
			}
			finally
			{
				const bool recursive = true;
				Directory.Delete(tempLongPathFilename, recursive);
			}
		}

		[TestMethod]
		public void TestGetFiles()
		{
			Assert.AreNotEqual(0, Directory.GetFiles(longPathDirectory).Count());
			Assert.AreEqual(1, Directory.GetFiles(longPathDirectory).Count());
			Assert.IsTrue(Directory.GetFiles(longPathDirectory).Contains(longPathFilename));
		}

		[TestMethod]
		public void TestGetDirectoriesWithSearch()
		{
			Assert.AreEqual(0, Directory.GetDirectories(longPathDirectory, "*").Count());
		}

		[TestMethod]
		public void TestGetDirectories()
		{
			Assert.AreEqual(0, Directory.GetDirectories(longPathDirectory).Count());
		}

		[TestMethod]
		public void TestGetRecursiveDirectoriesWithSearch()
		{
			Assert.AreEqual(0, Directory.GetDirectories(longPathDirectory, "*", SearchOption.AllDirectories).Count());
		}

		[TestMethod]
		public void TestGetDirectoryRoot()
		{
			Assert.AreEqual(longPathDirectory.Substring(0, 3), Directory.GetDirectoryRoot(longPathDirectory));
		}

		[TestMethod]
		public void TestCurrentDirectory()
		{
			var di = new DirectoryInfo(".");
			Assert.AreEqual(di.FullName, Directory.GetCurrentDirectory());
		}

		[TestMethod]
		public void TestDeleteDirectory()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			Assert.IsTrue(Directory.Exists(Path.GetFullPath(tempLongPathFilename)));
			Directory.Delete(tempLongPathFilename);
			Assert.IsFalse(Directory.Exists(Path.GetFullPath(tempLongPathFilename)));
		}

		[TestMethod]
		public void TestMove()
		{
			var tempLongPathFilename1 = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename1);
			Assert.IsTrue(Directory.Exists(Path.GetFullPath(tempLongPathFilename1)));
			var tempLongPathFilename2 = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename2);
			Assert.IsTrue(Directory.Exists(Path.GetFullPath(tempLongPathFilename2)));

			string destinationPath = Path.GetFullPath(Path.Combine(tempLongPathFilename1, Path.GetFileName(tempLongPathFilename2)));
			Directory.Move(tempLongPathFilename2, destinationPath);
			Assert.IsTrue(Directory.Exists(Path.GetFullPath(tempLongPathFilename1)));
			Assert.IsFalse(Directory.Exists(Path.GetFullPath(tempLongPathFilename2)));
			Assert.IsTrue(Directory.Exists(destinationPath));

			const bool recursive = true;
			Directory.Delete(tempLongPathFilename1, recursive);
		}


		[TestMethod]
		public void TestGetAccessControl()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var security = Directory.GetAccessControl(tempLongPathFilename);
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
				var security = Directory.GetAccessControl(tempLongPathFilename, AccessControlSections.Access);
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
		public void TestGetDirectoriesWithSearchWithNoResults()
		{
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = Path.Combine(longPathDirectory, randomFileName);
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var dirs = Directory.GetDirectories(longPathDirectory, "gibberish").ToArray();
				Assert.AreEqual(0, dirs.Length);
				Assert.IsFalse(dirs.Contains(tempLongPathFilename));
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[TestMethod]
		public void TestGetFilesWithSearch()
		{
			var files = Directory.GetFiles(longPathDirectory, "*").ToArray();
			Assert.AreEqual(1, files.Length);
			Assert.IsTrue(files.Contains(longPathFilename));
		}

		[TestMethod]
		public void TestGetFilesWithSearchWithNoResults()
		{
			var files = Directory.GetFiles(longPathDirectory, "giberish").ToArray();
			Assert.AreEqual(0, files.Length);
			Assert.IsFalse(files.Contains(longPathFilename));
		}

		[TestMethod]
		public void TestGetRecursiveFilesWithSearch()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				Assert.IsTrue(Directory.Exists(tempLongPathFilename));
				var randomFileName = Util.CreateNewEmptyFile(tempLongPathFilename);

				var files = Directory.GetFiles(longPathDirectory, "*", SearchOption.AllDirectories).ToArray();
				Assert.AreEqual(2, files.Length);
				Assert.IsTrue(files.Contains(longPathFilename));
				Assert.IsTrue(files.Contains(randomFileName));
			}
			finally
			{
				const bool recursive = true;
				Directory.Delete(tempLongPathFilename, recursive);
			}
		}

		[TestMethod]
		public void TestGetFilesRecursiveWithSearchWithNoResults()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				Assert.IsTrue(Directory.Exists(tempLongPathFilename));
				var randomFileName = Util.CreateNewEmptyFile(tempLongPathFilename);

				var files = Directory.GetFiles(longPathDirectory, "gibberish", SearchOption.AllDirectories).ToArray();
				Assert.AreEqual(0, files.Length);
				Assert.IsFalse(files.Contains(longPathFilename));
				Assert.IsFalse(files.Contains(randomFileName));
			}
			finally
			{
				const bool recursive = true;
				Directory.Delete(tempLongPathFilename, recursive);
			}
		}

		[TestMethod]
		public void TestGetFileSystemEntries()
		{
			var entries = Directory.GetFileSystemEntries(longPathDirectory).ToArray();
			Assert.AreEqual(1, entries.Length);
			Assert.IsTrue(entries.Contains(longPathFilename));
		}

		[TestMethod]
		public void TestGetFileSystemEntriesWithSearch()
		{
			var entries = Directory.GetFileSystemEntries(longPathDirectory, "*").ToArray();
			Assert.AreEqual(1, entries.Length);
			Assert.IsTrue(entries.Contains(longPathFilename));
		}

		[TestMethod]
		public void TestGetFileSystemEntriesWithSearchWithNoResults()
		{
			var entries = Directory.GetFileSystemEntries(longPathDirectory, "giberish").ToArray();
			Assert.AreEqual(0, entries.Length);
			Assert.IsFalse(entries.Contains(longPathFilename));
		}

		[TestMethod]
		public void TestGetRecursiveFileSystemEntriesWithSearch()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				Assert.IsTrue(Directory.Exists(tempLongPathFilename));
				var randomFileName = Util.CreateNewEmptyFile(tempLongPathFilename);

				var entries = Directory.GetFileSystemEntries(longPathDirectory, "*", SearchOption.AllDirectories).ToArray();
				Assert.AreEqual(3, entries.Length);
				Assert.IsTrue(entries.Contains(longPathFilename));
				Assert.IsTrue(entries.Contains(randomFileName));
			}
			finally
			{
				const bool recursive = true;
				Directory.Delete(tempLongPathFilename, recursive);
			}
		}

		[TestMethod]
		public void TestGetFileSystemEntriesRecursiveWithSearchWithNoResults()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				Assert.IsTrue(Directory.Exists(tempLongPathFilename));
				var randomFileName = Util.CreateNewEmptyFile(tempLongPathFilename);

				var files = Directory.GetFileSystemEntries(longPathDirectory, "gibberish", SearchOption.AllDirectories).ToArray();
				Assert.AreEqual(0, files.Length);
				Assert.IsFalse(files.Contains(longPathFilename));
				Assert.IsFalse(files.Contains(randomFileName));
			}
			finally
			{
				const bool recursive = true;
				Directory.Delete(tempLongPathFilename, recursive);
			}
		}

		[TestMethod, ExpectedException(typeof(NotSupportedException))]
		public void TestSetCurrentDirectory()
		{
			string originalDir = Directory.GetCurrentDirectory();
			try
			{
				Directory.SetCurrentDirectory(longPathDirectory);
				Assert.AreEqual(longPathDirectory, Directory.GetCurrentDirectory());
			}
			finally
			{
				Directory.SetCurrentDirectory(originalDir);
			}
		}

		[TestMethod]
		public void TestSetCreationTime()
		{
			var tempLongPathFilename = Path.Combine(rootTestDir, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			Assert.IsTrue(Directory.Exists(tempLongPathFilename));
			try
			{
				DateTime dateTime = DateTime.Now.AddDays(1);
				Directory.SetCreationTime(tempLongPathFilename, dateTime);
				var di = new DirectoryInfo(tempLongPathFilename);
				Assert.AreEqual(di.CreationTime, dateTime);

			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		/// <remarks>
		/// TODO: test some error conditions.
		/// </remarks>
		[TestMethod]
		public void TestSetCreationTimeUtc()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				DateTime dateTime = DateTime.UtcNow.AddDays(1);
				Directory.SetCreationTimeUtc(tempLongPathFilename, dateTime);
				var di = new DirectoryInfo(tempLongPathFilename);
				Assert.AreEqual(di.CreationTimeUtc, dateTime);

			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[TestMethod, ExpectedException(typeof(FileNotFoundException))]
		public void TestSetCreationTimeUtcNonExistentDir()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			DateTime dateTime = DateTime.UtcNow.AddDays(1);
			Directory.SetCreationTimeUtc(tempLongPathFilename, dateTime);
		}

		[TestMethod]
		public void TestGetCreationTime()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var dateTime = Directory.GetCreationTime(tempLongPathFilename);
				var fi = new DirectoryInfo(tempLongPathFilename);
				Assert.AreEqual(fi.CreationTime, dateTime);
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		/// <remarks>
		/// TODO: test some error conditions.
		/// </remarks>
		[TestMethod]
		public void TestGetCreationTimeUTc()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var dateTime = Directory.GetCreationTimeUtc(tempLongPathFilename);
				var fi = new DirectoryInfo(tempLongPathFilename);
				Assert.AreEqual(fi.CreationTimeUtc, dateTime);
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[TestMethod]
		public void TestSetLastWriteTime()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				DateTime dateTime = DateTime.Now.AddDays(1);
				Directory.SetLastWriteTime(tempLongPathFilename, dateTime);
				var fi = new DirectoryInfo(tempLongPathFilename);
				Assert.AreEqual(fi.LastWriteTime, dateTime);

			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		/// <remarks>
		/// TODO: test some error conditions.
		/// </remarks>
		[TestMethod]
		public void TestSetLastWriteTimeUtc()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				DateTime dateTime = DateTime.UtcNow.AddDays(1);
				Directory.SetLastWriteTimeUtc(tempLongPathFilename, dateTime);
				var fi = new DirectoryInfo(tempLongPathFilename);
				Assert.AreEqual(fi.LastWriteTimeUtc, dateTime);

			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[TestMethod, ExpectedException(typeof(FileNotFoundException))]
		public void TestSetLastWriteTimeUtcNonExistentDir()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			DateTime dateTime = DateTime.UtcNow.AddDays(1);
			Directory.SetLastWriteTimeUtc(tempLongPathFilename, dateTime);
		}

		[TestMethod]
		public void TestGetLastWriteTime()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var dateTime = Directory.GetLastWriteTime(tempLongPathFilename);
				var fi = new DirectoryInfo(tempLongPathFilename);
				Assert.AreEqual(fi.LastWriteTime, dateTime);
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[TestMethod]
		public void TestGetLastWriteTimeUtc()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var dateTime = Directory.GetLastWriteTimeUtc(tempLongPathFilename);
				var fi = new DirectoryInfo(tempLongPathFilename);
				Assert.AreEqual(fi.LastWriteTimeUtc, dateTime);
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[TestMethod]
		public void TestSetLastAccessTime()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				DateTime dateTime = DateTime.Now.AddDays(1);
				Directory.SetLastAccessTime(tempLongPathFilename, dateTime);
				var fi = new DirectoryInfo(tempLongPathFilename);
				Assert.AreEqual(fi.LastAccessTime, dateTime);

			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[TestMethod]
		public void TestSetLastAccessTimeUtc()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				DateTime dateTime = DateTime.UtcNow.AddDays(1);
				Directory.SetLastAccessTimeUtc(tempLongPathFilename, dateTime);
				var fi = new DirectoryInfo(tempLongPathFilename);
				Assert.AreEqual(fi.LastAccessTimeUtc, dateTime);

			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[TestMethod, ExpectedException(typeof(FileNotFoundException))]
		public void TestSetLastAccessTimeUtcNonExistentDir()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			DateTime dateTime = DateTime.UtcNow.AddDays(1);
			Directory.SetLastAccessTimeUtc(tempLongPathFilename, dateTime);
		}

		[TestMethod]
		public void TestGetLastAccessTime()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var dateTime = Directory.GetLastAccessTime(tempLongPathFilename);
				var fi = new DirectoryInfo(tempLongPathFilename);
				Assert.AreEqual(fi.LastAccessTime, dateTime);
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[TestMethod]
		public void TestGetLastAccessTimeUtc()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var dateTime = Directory.GetLastAccessTimeUtc(tempLongPathFilename);
				var fi = new DirectoryInfo(tempLongPathFilename);
				Assert.AreEqual(fi.LastAccessTimeUtc, dateTime);
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[TestMethod]
		public void TestGetLogicalDrives()
		{
			string[] directoryGetLogicalDrives = Directory.GetLogicalDrives();
			Assert.IsNotNull(directoryGetLogicalDrives);
			Assert.IsTrue(directoryGetLogicalDrives.Length > 0);
		}

		/// <remarks>
		/// TODO: more realistic DirectorySecurity scenarios
		/// </remarks>
		[TestMethod]
		public void TestCreateWithFileSecurity()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			try
			{
				Directory.CreateDirectory(tempLongPathFilename, new DirectorySecurity());
				Assert.IsTrue(Directory.Exists(tempLongPathFilename));
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
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
