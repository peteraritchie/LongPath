using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Directory = Pri.LongPath.Directory;
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
using Path = Pri.LongPath.Path;
using SearchOption = System.IO.SearchOption;

namespace Tests
{
	[TestClass]
	public class UncDirectoryTests
	{
		[TestMethod]
		public void BaselineDirectoryExists()
		{
            Assert.IsTrue(System.IO.Directory.Exists(UncHelper.GetUncFromPath(".")));
		}

        [TestMethod]
        public void BaselineTestCreateDirectory()
        {
            var tempPath = System.IO.Path.Combine(uncDirectory, Path.GetRandomFileName());
            var di = System.IO.Directory.CreateDirectory(tempPath);
            try
            {
                Assert.IsNotNull(di);
                Assert.IsTrue(System.IO.Directory.Exists(tempPath));
            }
            finally
            {
                System.IO.Directory.Delete(tempPath);
            }
        }

		[TestMethod]
		public void BaselineGetParent()
		{
			var actual = System.IO.Directory.GetParent(System.IO.Path.Combine(uncDirectory, "system32"));
			Assert.AreEqual(uncDirectory, actual.FullName);
		}

		[TestMethod]
        public void BaselineTestCreateMultipleDirectories()
        {
	        string tempSubDir = System.IO.Path.Combine(uncDirectory, Path.GetRandomFileName());
	        var tempSubSubDir = System.IO.Path.Combine(tempSubDir, Path.GetRandomFileName());
            var di = System.IO.Directory.CreateDirectory(tempSubSubDir);
            try
            {
                Assert.IsNotNull(di);
                Assert.IsTrue(System.IO.Directory.Exists(tempSubSubDir));
            }
            finally
            {
                System.IO.Directory.Delete(tempSubDir, true);
            }
        }

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
        public void TestExists()
        {
            Assert.IsTrue(Directory.Exists(uncDirectory));
        }

        [TestMethod]
        public void TestExistsOnFile()
        {
            Assert.IsFalse(Directory.Exists(uncFilePath));
        }

        [TestMethod]
        public void TestExistsOnNonexistentFile()
        {
            Assert.IsFalse(Directory.Exists(new StringBuilder(uncDirectory).Append(@"\").Append("does-not-exist").ToString()));
        }

        /// <remarks>
        /// Tests Directory.GetParent,
        /// depends on Directory.Combine, DirectoryInfo.FullName
        /// </remarks>
        [TestMethod]
        public void TestGetParent()
        {
            var actual = Directory.GetParent(Path.Combine(uncDirectory, "system32"));
            Assert.AreEqual(uncDirectory, actual.FullName);
        }

        /// <remarks>
        /// TODO: test some error conditions.
        /// </remarks>
        [TestMethod]
        public void TestCreateDirectory()
        {
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
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
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName() + @"\");
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
            var di = Directory.CreateDirectory(uncDirectory);
            Assert.IsNotNull(di);
            Assert.IsTrue(Directory.Exists(uncDirectory));
        }

        /// <remarks>
        /// Tests <see cref="Directory.EnumerateDirectories(string)"/>, depends on <see cref="Directory.CreateDirectory"/>
        /// </remarks>
        [TestMethod]
        public void TestEnumerateDirectories()
        {
            var randomFileName = Path.GetRandomFileName();
            var tempPath = Path.Combine(uncDirectory, randomFileName);
            Directory.CreateDirectory(tempPath);
            try
            {
                var dirs = Directory.EnumerateDirectories(uncDirectory).ToArray();
                Assert.AreEqual(1, dirs.Length);
                Assert.IsTrue(dirs.Contains(tempPath));
            }
            finally
            {
                Directory.Delete(tempPath);
            }
        }

        [TestMethod]
        public void TestEnumerateDirectoriesWithSearch()
        {
            var randomFileName = Path.GetRandomFileName();
            var tempLongPathFilename = Path.Combine(uncDirectory, randomFileName);
            Directory.CreateDirectory(tempLongPathFilename);
            try
            {
                var dirs = Directory.EnumerateDirectories(uncDirectory, "*").ToArray();
                Assert.AreEqual(1, dirs.Length);
                Assert.IsTrue(dirs.Contains(tempLongPathFilename));
            }
            finally
            {
                Directory.Delete(tempLongPathFilename);
            }
        }

        [TestMethod]
        public void TestRecursiveEnumerateDirectoriesWithAllSearch()
        {
            var randomFileName = Path.GetRandomFileName();
            var tempLongPathFilename = Path.Combine(uncDirectory, randomFileName);
            Directory.CreateDirectory(tempLongPathFilename);
            try
            {
                var dirs = Directory.EnumerateDirectories(uncDirectory, "*", SearchOption.AllDirectories).ToArray();
                Assert.AreEqual(1, dirs.Length);
                Assert.IsTrue(dirs.Contains(tempLongPathFilename));
            }
            finally
            {
                Directory.Delete(tempLongPathFilename);
            }
        }

        [TestMethod]
        public void TestRecursiveEnumerateDirectoriesWithSingleSubsetSearch()
        {
            var randomFileName = "TestRecursiveEnumerateDirectoriesWithSubsetSearch";
            var tempLongPathFilename = Path.Combine(uncDirectory, randomFileName);
            Directory.CreateDirectory(tempLongPathFilename);
            try
            {
                var dirs = Directory.EnumerateDirectories(uncDirectory, "T*", SearchOption.AllDirectories).ToArray();
                Assert.AreEqual(1, dirs.Length);
                Assert.IsTrue(dirs.Contains(tempLongPathFilename));
            }
            finally
            {
                Directory.Delete(tempLongPathFilename);
            }
        }

        [TestMethod]
        public void TestRecursiveEnumerateDirectoriesWithMultipleSubsetSearch()
        {
            var randomFileName = "TestRecursiveEnumerateDirectoriesWithMultipleSubsetSearch";
            var tempLongPathFilename = Path.Combine(uncDirectory, randomFileName);
            Directory.CreateDirectory(tempLongPathFilename);
            randomFileName = "ATestRecursiveEnumerateDirectoriesWithMultipleSubsetSearch";
            var tempLongPathFilename2 = Path.Combine(uncDirectory, randomFileName);
            Directory.CreateDirectory(tempLongPathFilename2);
            try
            {
                var dirs = Directory.EnumerateDirectories(uncDirectory, "T*", SearchOption.AllDirectories).ToArray();
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

        [TestMethod]
        public void TestRecursiveEnumerateDirectoriesWithSearchNoResults()
        {
            var randomFileName = Path.GetRandomFileName();
            var tempLongPathFilename = Path.Combine(uncDirectory, randomFileName);
            Directory.CreateDirectory(tempLongPathFilename);
            try
            {
                var dirs = Directory.EnumerateDirectories(uncDirectory, "gibberish", SearchOption.AllDirectories).ToArray();
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
            var tempLongPathFilename = Path.Combine(uncDirectory, randomFileName);
            Directory.CreateDirectory(tempLongPathFilename);
            try
            {
                var dirs = Directory.EnumerateDirectories(uncDirectory, "gibberish").ToArray();
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
            var files = Directory.EnumerateFiles(uncDirectory).ToArray();
            Assert.AreEqual(1, files.Length);
            Assert.IsTrue(files.Contains(uncFilePath));
        }

        [TestMethod]
        public void TestEnumerateFilesWithSearch()
        {
            var files = Directory.EnumerateFiles(uncDirectory, "*").ToArray();
            Assert.AreEqual(1, files.Length);
            Assert.IsTrue(files.Contains(uncFilePath));
        }

        [TestMethod]
        public void TestEnumerateFilesWithSearchWithNoResults()
        {
            var files = Directory.EnumerateFiles(uncDirectory, "giberish").ToArray();
            Assert.AreEqual(0, files.Length);
            Assert.IsFalse(files.Contains(uncFilePath));
        }

        [TestMethod]
        public void TestEnumerateRecursiveFilesWithSearch()
        {
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
            Directory.CreateDirectory(tempLongPathFilename);
            try
            {
                Assert.IsTrue(Directory.Exists(tempLongPathFilename));
                var randomFileName = Util.CreateNewEmptyFile(tempLongPathFilename);

                var files = Directory.EnumerateFiles(uncDirectory, "*", SearchOption.AllDirectories).ToArray();
                Assert.AreEqual(2, files.Length);
                Assert.IsTrue(files.Contains(uncFilePath));
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
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
            Directory.CreateDirectory(tempLongPathFilename);
            try
            {
                Assert.IsTrue(Directory.Exists(tempLongPathFilename));
                var randomFileName = Util.CreateNewEmptyFile(tempLongPathFilename);

                var files = Directory.EnumerateFiles(uncDirectory, "gibberish", SearchOption.AllDirectories).ToArray();
                Assert.AreEqual(0, files.Length);
                Assert.IsFalse(files.Contains(uncFilePath));
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
            var entries = Directory.EnumerateFileSystemEntries(uncDirectory).ToArray();
            Assert.AreEqual(1, entries.Length);
            Assert.IsTrue(entries.Contains(uncFilePath));
        }

        [TestMethod]
        public void TestEnumerateFileSystemEntriesWithSearch()
        {
            var entries = Directory.EnumerateFileSystemEntries(uncDirectory, "*").ToArray();
            Assert.AreEqual(1, entries.Length);
            Assert.IsTrue(entries.Contains(uncFilePath));
        }

        [TestMethod]
        public void TestEnumerateFileSystemEntriesWithSearchWithNoResults()
        {
            var entries = Directory.EnumerateFileSystemEntries(uncDirectory, "giberish").ToArray();
            Assert.AreEqual(0, entries.Length);
            Assert.IsFalse(entries.Contains(uncFilePath));
        }

        [TestMethod]
        public void TestEnumerateRecursiveFileSystemEntriesWithSearch()
        {
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
            Directory.CreateDirectory(tempLongPathFilename);
            try
            {
                Assert.IsTrue(Directory.Exists(tempLongPathFilename));
                var randomFileName = Util.CreateNewEmptyFile(tempLongPathFilename);

                var entries = Directory.EnumerateFileSystemEntries(uncDirectory, "*", SearchOption.AllDirectories).ToArray();
                Assert.AreEqual(3, entries.Length);
                Assert.IsTrue(entries.Contains(uncFilePath));
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
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
            Directory.CreateDirectory(tempLongPathFilename);
            try
            {
                Assert.IsTrue(Directory.Exists(tempLongPathFilename));
                var randomFileName = Util.CreateNewEmptyFile(tempLongPathFilename);

                var files = Directory.EnumerateFileSystemEntries(uncDirectory, "gibberish", SearchOption.AllDirectories).ToArray();
                Assert.AreEqual(0, files.Length);
                Assert.IsFalse(files.Contains(uncFilePath));
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
            Assert.AreNotEqual(0, Directory.GetFiles(uncDirectory).Count());
            Assert.AreEqual(1, Directory.GetFiles(uncDirectory).Count());
            Assert.IsTrue(Directory.GetFiles(uncDirectory).Contains(uncFilePath));
        }

        [TestMethod]
        public void TestGetDirectoriesWithAnySearch()
        {
            var tempLongPathFilename = Path.Combine(uncDirectory, "TestGetDirectoriesWithAnySearch");
            Directory.CreateDirectory(tempLongPathFilename);
            var tempLongPathFilename2 = Path.Combine(uncDirectory, "ATestGetDirectoriesWithAnySearch");
            Directory.CreateDirectory(tempLongPathFilename2);
            try
            {
                Assert.AreEqual(2, Directory.GetDirectories(uncDirectory, "*").Count());
            }
            finally
            {
                Directory.Delete(tempLongPathFilename);
                Directory.Delete(tempLongPathFilename2);
            }
        }

        [TestMethod]
        public void TestGetDirectoriesWithSubsetSearch()
        {
            var tempLongPathFilename = Path.Combine(uncDirectory, "TestGetDirectoriesWithSubsetSearch");
            Directory.CreateDirectory(tempLongPathFilename);
            var tempLongPathFilename2 = Path.Combine(uncDirectory, "ATestGetDirectoriesWithSubsetSearch");
            Directory.CreateDirectory(tempLongPathFilename2);
            try
            {
                Assert.AreEqual(1, Directory.GetDirectories(uncDirectory, "A*").Count());
            }
            finally
            {
                Directory.Delete(tempLongPathFilename);
                Directory.Delete(tempLongPathFilename2);
            }
        }

        [TestMethod]
        public void TestGetRecursiveDirectoriesWithSubsetSearch()
        {
            var tempLongPathFilename = Path.Combine(uncDirectory, "TestGetRecursiveDirectoriesWithSubsetSearch");
            Directory.CreateDirectory(tempLongPathFilename);
            var tempLongPathFilename2 = Path.Combine(tempLongPathFilename, "ATestGetRecursiveDirectoriesWithSubsetSearch");
            Directory.CreateDirectory(tempLongPathFilename2);
            try
            {
                Assert.AreEqual(1, Directory.GetDirectories(uncDirectory, "A*", System.IO.SearchOption.AllDirectories).Count());
            }
            finally
            {
                Directory.Delete(tempLongPathFilename2);
                Directory.Delete(tempLongPathFilename);
            }
        }

        [TestMethod]
        public void TestGetDirectories()
        {
            Assert.AreEqual(0, Directory.GetDirectories(uncDirectory).Count());
        }

        [TestMethod]
        public void TestGetRecursiveDirectoriesWithSearch()
        {
            Assert.AreEqual(0, Directory.GetDirectories(uncDirectory, "*", SearchOption.AllDirectories).Count());
        }

        [TestMethod]
        public void TestGetDirectoryRoot()
        {
			Assert.IsTrue(@"\\localhost\C$\".Equals(Directory.GetDirectoryRoot(uncDirectory), StringComparison.InvariantCultureIgnoreCase));
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
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
            Directory.CreateDirectory(tempLongPathFilename);
            Assert.IsTrue(Directory.Exists(Path.GetFullPath(tempLongPathFilename)));
            Directory.Delete(tempLongPathFilename);
            Assert.IsFalse(Directory.Exists(Path.GetFullPath(tempLongPathFilename)));
        }

        [TestMethod]
        public void TestMove()
        {
            var tempLongPathFilename1 = Path.Combine(uncDirectory, Path.GetRandomFileName());
            Directory.CreateDirectory(tempLongPathFilename1);
            Assert.IsTrue(Directory.Exists(Path.GetFullPath(tempLongPathFilename1)));
            var tempLongPathFilename2 = Path.Combine(uncDirectory, Path.GetRandomFileName());
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

        [TestMethod, ExpectedException(typeof(IOException))]
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
            var tempLongPathFilename1 = Path.Combine(uncDirectory, Path.GetRandomFileName());
            Directory.CreateDirectory(tempLongPathFilename1);
            Assert.IsTrue(Directory.Exists(Path.GetFullPath(tempLongPathFilename1)));
            var tempLongPathFilename2 = Path.Combine(uncDirectory, Path.GetRandomFileName());
            Directory.CreateDirectory(tempLongPathFilename2);
            Assert.IsTrue(Directory.Exists(Path.GetFullPath(tempLongPathFilename2)));
            try
            {
                using (
                    var writer = File.CreateText(Path.Combine(tempLongPathFilename2, "TestInUseMove")))
                {
                    string destinationPath =
                        Path.GetFullPath(Path.Combine(tempLongPathFilename1, Path.GetFileName(tempLongPathFilename2)));
                    Directory.Move(tempLongPathFilename2, destinationPath);
                    Assert.IsTrue(Directory.Exists(Path.GetFullPath(tempLongPathFilename1)));
                    Assert.IsFalse(Directory.Exists(Path.GetFullPath(tempLongPathFilename2)));
                    Assert.IsTrue(Directory.Exists(destinationPath));
                }
            }
            finally
            {
                Directory.Delete(tempLongPathFilename1, recursive);
                Directory.Delete(tempLongPathFilename2, recursive);
            }
        }

        [TestMethod]
        public void PathGetDirectoryNameReturnsSameResultAsBclForRelativePath()
        {
            var text = System.IO.Path.GetDirectoryName(@"foo\bar\baz");
            Assert.AreEqual(@"foo\bar", text);
        }

		[TestMethod, Ignore] //("does not work on some server/domain systems.")
		public void TestGetAccessControl()
        {
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
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

		[TestMethod, Ignore] //("does not work on some server/domain systems.")
		public void TestGetAccessControlSections()
        {
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
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
            var tempLongPathFilename = Path.Combine(uncDirectory, randomFileName);
            Directory.CreateDirectory(tempLongPathFilename);
            try
            {
                var dirs = Directory.GetDirectories(uncDirectory, "gibberish").ToArray();
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
            var files = Directory.GetFiles(uncDirectory, "*").ToArray();
            Assert.AreEqual(1, files.Length);
            Assert.IsTrue(files.Contains(uncFilePath));
        }

        [TestMethod]
        public void TestGetFilesWithSearchWithNoResults()
        {
            var files = Directory.GetFiles(uncDirectory, "giberish").ToArray();
            Assert.AreEqual(0, files.Length);
            Assert.IsFalse(files.Contains(uncFilePath));
        }

        [TestMethod]
        public void TestGetRecursiveFilesWithAnySearch()
        {
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
            Directory.CreateDirectory(tempLongPathFilename);
            try
            {
                Assert.IsTrue(Directory.Exists(tempLongPathFilename));
                var randomFileName = Util.CreateNewEmptyFile(tempLongPathFilename);

                var files = Directory.GetFiles(uncDirectory, "*", SearchOption.AllDirectories).ToArray();
                Assert.AreEqual(2, files.Length);
                Assert.IsTrue(files.Contains(uncFilePath));
                Assert.IsTrue(files.Contains(randomFileName));
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
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
            Directory.CreateDirectory(tempLongPathFilename);
            try
            {
                Assert.IsTrue(Directory.Exists(tempLongPathFilename));
                var randomFileName = Util.CreateNewEmptyFile(tempLongPathFilename);

                var searchPattern = Path.GetFileName(randomFileName).Substring(0, 3) + "*" + Path.GetExtension(randomFileName);

                var files = Directory.GetFiles(uncDirectory, searchPattern, SearchOption.AllDirectories).ToArray();
                Assert.AreEqual(1, files.Length);
                Assert.IsFalse(files.Contains(uncFilePath));
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
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
            Directory.CreateDirectory(tempLongPathFilename);
            try
            {
                Assert.IsTrue(Directory.Exists(tempLongPathFilename));
                var randomFileName = Util.CreateNewEmptyFile(tempLongPathFilename);

                var files = Directory.GetFiles(uncDirectory, "gibberish", SearchOption.AllDirectories).ToArray();
                Assert.AreEqual(0, files.Length);
                Assert.IsFalse(files.Contains(uncFilePath));
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
            var entries = Directory.GetFileSystemEntries(uncDirectory).ToArray();
            Assert.AreEqual(1, entries.Length);
            Assert.IsTrue(entries.Contains(uncFilePath));
        }

        [TestMethod]
        public void TestGetFileSystemEntriesWithSearch()
        {
            var entries = Directory.GetFileSystemEntries(uncDirectory, "*").ToArray();
            Assert.AreEqual(1, entries.Length);
            Assert.IsTrue(entries.Contains(uncFilePath));
        }

        [TestMethod]
        public void TestGetFileSystemEntriesWithSearchWithNoResults()
        {
            var entries = Directory.GetFileSystemEntries(uncDirectory, "giberish").ToArray();
            Assert.AreEqual(0, entries.Length);
            Assert.IsFalse(entries.Contains(uncFilePath));
        }

        [TestMethod]
        public void TestGetRecursiveFileSystemEntriesWithSearch()
        {
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
            Directory.CreateDirectory(tempLongPathFilename);
            try
            {
                Assert.IsTrue(Directory.Exists(tempLongPathFilename));
                var randomFileName = Util.CreateNewEmptyFile(tempLongPathFilename);

                var entries = Directory.GetFileSystemEntries(uncDirectory, "*", SearchOption.AllDirectories).ToArray();
                Assert.AreEqual(3, entries.Length);
                Assert.IsTrue(entries.Contains(uncFilePath));
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
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
            Directory.CreateDirectory(tempLongPathFilename);
            try
            {
                Assert.IsTrue(Directory.Exists(tempLongPathFilename));
                var randomFileName = Util.CreateNewEmptyFile(tempLongPathFilename);

                var files = Directory.GetFileSystemEntries(uncDirectory, "gibberish", SearchOption.AllDirectories).ToArray();
                Assert.AreEqual(0, files.Length);
                Assert.IsFalse(files.Contains(uncFilePath));
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
                Directory.SetCurrentDirectory(uncDirectory);
                Assert.AreEqual(uncDirectory, Directory.GetCurrentDirectory());
            }
            finally
            {
                Directory.SetCurrentDirectory(originalDir);
            }
        }

        [TestMethod]
        public void TestSetCreationTime()
        {
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
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
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
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
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
            DateTime dateTime = DateTime.UtcNow.AddDays(1);
            Directory.SetCreationTimeUtc(tempLongPathFilename, dateTime);
        }

        [TestMethod]
        public void TestGetCreationTime()
        {
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
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
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
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
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
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
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
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
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
            DateTime dateTime = DateTime.UtcNow.AddDays(1);
            Directory.SetLastWriteTimeUtc(tempLongPathFilename, dateTime);
        }

        [TestMethod]
        public void TestGetLastWriteTime()
        {
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
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
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
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
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
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
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
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
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
            DateTime dateTime = DateTime.UtcNow.AddDays(1);
            Directory.SetLastAccessTimeUtc(tempLongPathFilename, dateTime);
        }

        [TestMethod]
        public void TestGetLastAccessTime()
        {
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
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
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
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
            var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
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