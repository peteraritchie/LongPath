using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.IO;
using Pri.LongPath;
using NUnit.Framework;

namespace Tests
{
	using Path = Pri.LongPath.Path;
	using Directory = Pri.LongPath.Directory;
	using DirectoryInfo = Pri.LongPath.DirectoryInfo;
	using File = Pri.LongPath.File;

	[TestFixture]
	public class DirectoryTests
	{
		private static string rootTestDir;
		private static string longPathDirectory;
		private static string longPathFilename;
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
			using (var writer = File.CreateText(longPathFilename))
			{
				writer.WriteLine("test");
			}
			Debug.Assert(File.Exists(longPathFilename));
		}

		[Test]
		public void TestExists()
		{
			Assert.IsTrue(Directory.Exists(longPathDirectory));
		}

		[Test]
		public void TestExistsOnFile()
		{
			Assert.IsFalse(Directory.Exists(new StringBuilder(longPathDirectory).Append(@"\").Append("does-not-exist").ToString()));
		}

		/// <remarks>
		/// Tests Directory.GetParent,
		/// depends on Directory.Combine, DirectoryInfo.FullName
		/// </remarks>
		[Test]
		public void TestGetParent()
		{
			var actual = Directory.GetParent(Path.Combine(longPathDirectory, "system32"));
			Assert.AreEqual(longPathDirectory, actual.FullName);
		}

		/// <remarks>
		/// TODO: test some error conditions.
		/// </remarks>
		[Test]
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

		[Test]
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

		[Test]
		public void TestCreateDirectoryThatAlreadyExists()
		{
			var di = Directory.CreateDirectory(longPathDirectory);
			Assert.IsNotNull(di);
			Assert.IsTrue(Directory.Exists(longPathDirectory));
		}

		/// <remarks>
		/// Tests <see cref="Directory.EnumerateDirectories(string)"/>, depends on <see cref="Pri.LongPath.Directory.CreateDirectory"/>
		/// </remarks>
		[Test]
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

		[Test]
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

		[Test]
		public void TestRecursiveEnumerateDirectoriesWithAllSearch()
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

		[Test]
		public void TestRecursiveEnumerateDirectoriesWithSingleSubsetSearch()
		{
			var randomFileName = "TestRecursiveEnumerateDirectoriesWithSubsetSearch";
			var tempLongPathFilename = Path.Combine(longPathDirectory, randomFileName);
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				var dirs = Directory.EnumerateDirectories(longPathDirectory, "T*", SearchOption.AllDirectories).ToArray();
				Assert.AreEqual(1, dirs.Length);
				Assert.IsTrue(dirs.Contains(tempLongPathFilename));
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestRecursiveEnumerateDirectoriesWithMultipleSubsetSearch()
		{
			var randomFileName = "TestRecursiveEnumerateDirectoriesWithMultipleSubsetSearch";
			var tempLongPathFilename = Path.Combine(longPathDirectory, randomFileName);
			Directory.CreateDirectory(tempLongPathFilename);
			randomFileName = "ATestRecursiveEnumerateDirectoriesWithMultipleSubsetSearch";
			var tempLongPathFilename2 = Path.Combine(longPathDirectory, randomFileName);
			Directory.CreateDirectory(tempLongPathFilename2);
			try
			{
				var dirs = Directory.EnumerateDirectories(longPathDirectory, "T*", SearchOption.AllDirectories).ToArray();
				Assert.AreEqual(1, dirs.Length);
				Assert.IsTrue(dirs.Contains(tempLongPathFilename));
				Assert.IsFalse(dirs.Contains(tempLongPathFilename2));
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
				Directory.Delete(tempLongPathFilename2);
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
				var dirs = Directory.EnumerateDirectories(longPathDirectory, "gibberish", SearchOption.AllDirectories).ToArray();
				Assert.AreEqual(0, dirs.Length);
				Assert.IsFalse(dirs.Contains(tempLongPathFilename));
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
		/// Tests <see cref="Directory.EnumerateDirectories(string)"/>, depends on <see cref="Pri.LongPath.Directory.CreateDirectory"/>
		/// </remarks>
		[Test]
		public void TestEnumerateFiles()
		{
			var files = Directory.EnumerateFiles(longPathDirectory).ToArray();
			Assert.AreEqual(1, files.Length);
			Assert.IsTrue(files.Contains(longPathFilename));
		}

		[Test]
		public void TestEnumerateFilesWithSearch()
		{
			var files = Directory.EnumerateFiles(longPathDirectory, "*").ToArray();
			Assert.AreEqual(1, files.Length);
			Assert.IsTrue(files.Contains(longPathFilename));
		}

		[Test]
		public void TestEnumerateFilesWithSearchWithNoResults()
		{
			var files = Directory.EnumerateFiles(longPathDirectory, "giberish").ToArray();
			Assert.AreEqual(0, files.Length);
			Assert.IsFalse(files.Contains(longPathFilename));
		}

		[Test]
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

		[Test]
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

		[Test]
		public void TestEnumerateFileSystemEntries()
		{
			var entries = Directory.EnumerateFileSystemEntries(longPathDirectory).ToArray();
			Assert.AreEqual(1, entries.Length);
			Assert.IsTrue(entries.Contains(longPathFilename));
		}

		[Test]
		public void TestEnumerateFileSystemEntriesWithSearch()
		{
			var entries = Directory.EnumerateFileSystemEntries(longPathDirectory, "*").ToArray();
			Assert.AreEqual(1, entries.Length);
			Assert.IsTrue(entries.Contains(longPathFilename));
		}

		[Test]
		public void TestEnumerateFileSystemEntriesWithSearchWithNoResults()
		{
			var entries = Directory.EnumerateFileSystemEntries(longPathDirectory, "giberish").ToArray();
			Assert.AreEqual(0, entries.Length);
			Assert.IsFalse(entries.Contains(longPathFilename));
		}

		[Test]
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

		[Test]
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

		[Test]
		public void TestGetFiles()
		{
			Assert.AreNotEqual(0, Directory.GetFiles(longPathDirectory).Count());
			Assert.AreEqual(1, Directory.GetFiles(longPathDirectory).Count());
			Assert.IsTrue(Directory.GetFiles(longPathDirectory).Contains(longPathFilename));
		}

		[Test]
		public void TestGetDirectoriesWithAnySearch()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, "TestGetDirectoriesWithAnySearch");
			Directory.CreateDirectory(tempLongPathFilename);
			var tempLongPathFilename2 = Path.Combine(longPathDirectory, "ATestGetDirectoriesWithAnySearch");
			Directory.CreateDirectory(tempLongPathFilename2);
			try
			{
				Assert.AreEqual(2, Directory.GetDirectories(longPathDirectory, "*").Count());
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
				Directory.Delete(tempLongPathFilename2);
			}
		}

		[Test]
		public void TestGetDirectoriesWithSubsetSearch()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, "TestGetDirectoriesWithSubsetSearch");
			Directory.CreateDirectory(tempLongPathFilename);
			var tempLongPathFilename2 = Path.Combine(longPathDirectory, "ATestGetDirectoriesWithSubsetSearch");
			Directory.CreateDirectory(tempLongPathFilename2);
			try
			{
				Assert.AreEqual(1, Directory.GetDirectories(longPathDirectory, "A*").Count());
			}
			finally
			{
				Directory.Delete(tempLongPathFilename);
				Directory.Delete(tempLongPathFilename2);
			}
		}

		[Test]
		public void TestGetRecursiveDirectoriesWithSubsetSearch()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, "TestGetRecursiveDirectoriesWithSubsetSearch");
			Directory.CreateDirectory(tempLongPathFilename);
			var tempLongPathFilename2 = Path.Combine(tempLongPathFilename, "ATestGetRecursiveDirectoriesWithSubsetSearch");
			Directory.CreateDirectory(tempLongPathFilename2);
			try
			{
				Assert.AreEqual(1, Directory.GetDirectories(longPathDirectory, "A*", System.IO.SearchOption.AllDirectories).Count());
			}
			finally
			{
				Directory.Delete(tempLongPathFilename2);
				Directory.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestGetDirectories()
		{
			Assert.AreEqual(0, Directory.GetDirectories(longPathDirectory).Count());
		}

		[Test]
		public void TestGetRecursiveDirectoriesWithSearch()
		{
			Assert.AreEqual(0, Directory.GetDirectories(longPathDirectory, "*", SearchOption.AllDirectories).Count());
		}

		[Test]
		public void TestGetDirectoryRoot()
		{
			Assert.AreEqual(longPathDirectory.Substring(0, 3), Directory.GetDirectoryRoot(longPathDirectory));
		}

		[Test]
		public void TestCurrentDirectory()
		{
			var di = new DirectoryInfo(".");
			Assert.AreEqual(di.FullName, Directory.GetCurrentDirectory());
		}

		[Test]
		public void TestDeleteDirectory()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			Assert.IsTrue(Directory.Exists(Path.GetFullPath(tempLongPathFilename)));
			Directory.Delete(tempLongPathFilename);
			Assert.IsFalse(Directory.Exists(Path.GetFullPath(tempLongPathFilename)));
		}

		/// <summary> Tests the Directory.Delete where 'path' is a junction point.
		/// </summary>
		[Test]
		public void TestDeleteDirectory_JunctionPoint()
		{
			string targetFolder = Path.Combine(rootTestDir, "ADirectory");
			string junctionPoint = Path.Combine(rootTestDir, "SymLink");

			Directory.CreateDirectory(targetFolder);
			try
			{

				var targetFile = Path.Combine(targetFolder, "AFile");

				File.Create(targetFile).Close();
				try
				{
					JunctionPoint.Create(junctionPoint, targetFolder, overwrite: false);
					Assert.IsTrue(File.Exists(Path.Combine(targetFolder, "AFile")), "File should be accessible.");
					Assert.IsTrue(File.Exists(Path.Combine(junctionPoint, "AFile")), "File should be accessible via the junction point.");

					Directory.Delete(junctionPoint, false);

					Assert.IsTrue(File.Exists(Path.Combine(targetFolder, "AFile")), "File should be accessible.");
					Assert.IsFalse(JunctionPoint.Exists(junctionPoint), "Junction point should not exist now.");
					Assert.IsTrue(!File.Exists(Path.Combine(junctionPoint, "AFile")), "File should not be accessible via the junction point.");
				}
				finally
				{
					File.Delete(targetFile);
				}
			}
			finally
			{
				Directory.Delete(targetFolder);
			}
		}

		[Test]
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
			Directory.Delete(tempLongPathFilename2, recursive);
		}

		[Test]
		public void TestInUseMove()
		{
			const bool recursive = true;

#if SHORT_SOURCE
			var tempPathFilename1 = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), System.IO.Path.GetRandomFileName());
			System.IO.Directory.CreateDirectory(tempPathFilename1);
			Assert.IsTrue(System.IO.Directory.Exists(Path.GetFullPath(tempPathFilename1)));
			var tempPathFilename2 = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), System.IO.Path.GetRandomFileName());
			System.IO.Directory.CreateDirectory(tempPathFilename2);
			Assert.IsTrue(System.IO.Directory.Exists(System.IO.Path.GetFullPath(tempPathFilename2)));
			try
			{
				using (
					var writer = System.IO.File.CreateText(System.IO.Path.Combine(tempPathFilename2, "TestInUseMove")))
				{
					string destinationPath =
						System.IO.Path.GetFullPath(System.IO.Path.Combine(tempPathFilename1, System.IO.Path.GetFileName(tempPathFilename2)));
					System.IO.Directory.Move(tempPathFilename2, destinationPath);
					Assert.IsTrue(System.IO.Directory.Exists(System.IO.Path.GetFullPath(tempPathFilename1)));
					Assert.IsFalse(System.IO.Directory.Exists(System.IO.Path.GetFullPath(tempPathFilename2)));
					Assert.IsTrue(System.IO.Directory.Exists(destinationPath));
				}
			}
			catch (Exception e)
			{
				throw;
			}
			finally
			{
				Directory.Delete(tempPathFilename1, recursive);
				Directory.Delete(tempPathFilename2, recursive);
			}
#endif
			var tempLongPathFilename1 = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename1);
			Assert.IsTrue(Directory.Exists(Path.GetFullPath(tempLongPathFilename1)));
			var tempLongPathFilename2 = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename2);
			Assert.IsTrue(Directory.Exists(Path.GetFullPath(tempLongPathFilename2)));
			try
			{
				using (
					var writer = File.CreateText(Path.Combine(tempLongPathFilename2, "TestInUseMove")))
				{
					string destinationPath =
						Path.GetFullPath(Path.Combine(tempLongPathFilename1, Path.GetFileName(tempLongPathFilename2)));
					Assert.Throws<IOException>(() => Directory.Move(tempLongPathFilename2, destinationPath));
				}
			}
			finally
			{
				Directory.Delete(tempLongPathFilename1, recursive);
				Directory.Delete(tempLongPathFilename2, recursive);
			}
		}

		[Test]
		public void PathGetDirectoryNameReturnsSameResultAsBclForRelativePath()
		{
			var text = System.IO.Path.GetDirectoryName(@"foo\bar\baz");
			Assert.AreEqual(@"foo\bar", text);
		}

		[Test, Ignore("does not work on some server/domain systems.")]
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

		[Test, Ignore("does not work on some server/domain systems.")]
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

		[Test]
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

		[Test]
		public void TestGetFilesWithSearch()
		{
			var files = Directory.GetFiles(longPathDirectory, "*").ToArray();
			Assert.AreEqual(1, files.Length);
			Assert.IsTrue(files.Contains(longPathFilename));
		}

		[Test]
		public void TestGetFilesWithSearchWithNoResults()
		{
			var files = Directory.GetFiles(longPathDirectory, "giberish").ToArray();
			Assert.AreEqual(0, files.Length);
			Assert.IsFalse(files.Contains(longPathFilename));
		}

		[Test]
		public void TestGetRecursiveFilesWithAnySearch()
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

		[Test]
		public void TestGetRecursiveFilesWithSubsetSearch()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			Directory.CreateDirectory(tempLongPathFilename);
			try
			{
				Assert.IsTrue(Directory.Exists(tempLongPathFilename));
				var randomFileName = Util.CreateNewEmptyFile(tempLongPathFilename);

				var searchPattern = Path.GetFileName(randomFileName).Substring(0,3) + "*" + Path.GetExtension(randomFileName);

				var files = Directory.GetFiles(longPathDirectory, searchPattern, SearchOption.AllDirectories).ToArray();
				Assert.AreEqual(1, files.Length);
				Assert.IsFalse(files.Contains(longPathFilename));
				Assert.IsTrue(files.Contains(randomFileName));
			}
			finally
			{
				const bool recursive = true;
				Directory.Delete(tempLongPathFilename, recursive);
			}
		}

		[Test]
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

		[Test]
		public void TestGetFileSystemEntries()
		{
			var entries = Directory.GetFileSystemEntries(longPathDirectory).ToArray();
			Assert.AreEqual(1, entries.Length);
			Assert.IsTrue(entries.Contains(longPathFilename));
		}

		[Test]
		public void TestGetFileSystemEntriesWithSearch()
		{
			var entries = Directory.GetFileSystemEntries(longPathDirectory, "*").ToArray();
			Assert.AreEqual(1, entries.Length);
			Assert.IsTrue(entries.Contains(longPathFilename));
		}

		[Test]
		public void TestGetFileSystemEntriesWithSearchWithNoResults()
		{
			var entries = Directory.GetFileSystemEntries(longPathDirectory, "giberish").ToArray();
			Assert.AreEqual(0, entries.Length);
			Assert.IsFalse(entries.Contains(longPathFilename));
		}

		[Test]
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

		[Test]
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

		[Test]
		public void TestSetCurrentDirectory()
		{
			string originalDir = Directory.GetCurrentDirectory();
			try
			{
				Assert.Throws<NotSupportedException>(() => Directory.SetCurrentDirectory(longPathDirectory));
			}
			finally
			{
				Assert.Throws<NotSupportedException>(() => Directory.SetCurrentDirectory(originalDir));
			}
		}

		[Test]
		public void TestSetCreationTime()
		{
            var tempLongPathFilename = Path.Combine(longPathRoot, Path.GetRandomFileName());
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
		[Test]
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

		[Test]
		public void TestSetCreationTimeUtcNonExistentDir()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			DateTime dateTime = DateTime.UtcNow.AddDays(1);
			Assert.Throws<FileNotFoundException>(() => Directory.SetCreationTimeUtc(tempLongPathFilename, dateTime));
		}

		[Test]
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
		[Test]
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

		[Test]
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
		[Test]
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

		[Test]
		public void TestSetLastWriteTimeUtcNonExistentDir()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			DateTime dateTime = DateTime.UtcNow.AddDays(1);
			Assert.Throws<FileNotFoundException>(() => Directory.SetLastWriteTimeUtc(tempLongPathFilename, dateTime));
		}

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
		public void TestSetLastAccessTimeUtcNonExistentDir()
		{
			var tempLongPathFilename = Path.Combine(longPathDirectory, Path.GetRandomFileName());
			DateTime dateTime = DateTime.UtcNow.AddDays(1);
			Assert.Throws<FileNotFoundException>(() => Directory.SetLastAccessTimeUtc(tempLongPathFilename, dateTime));
		}

		[Test]
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

		[Test]
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

		[Test]
		public void TestGetLogicalDrives()
		{
			string[] directoryGetLogicalDrives = Directory.GetLogicalDrives();
			Assert.IsNotNull(directoryGetLogicalDrives);
			Assert.IsTrue(directoryGetLogicalDrives.Length > 0);
		}

		/// <remarks>
		/// TODO: more realistic DirectorySecurity scenarios
		/// </remarks>
		[Test]
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

		[TearDown]
		public void TearDown()
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
				if (Directory.Exists(longPathRoot))
					Directory.Delete(longPathRoot, true);
			}
		}
	}
}
