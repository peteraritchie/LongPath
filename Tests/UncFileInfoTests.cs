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
using System.Security.AccessControl;

namespace Tests
{
	using FileNotFoundException = System.IO.FileNotFoundException;

	[TestFixture]
	public class UncFileInfoTests
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
			catch (Exception)
			{
				if (System.IO.Directory.Exists(directory))
					System.IO.Directory.Delete(directory, true);
				throw;
			}
		}

		[Test]
		public void CanCreateFileInfoWithLongPathFile()
		{
			string tempLongPathFilename;
			do
			{
				tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
			} while (File.Exists(tempLongPathFilename));
			Assert.IsFalse(File.Exists(tempLongPathFilename));

			using (var writer = File.CreateText(tempLongPathFilename))
			{
				writer.WriteLine("test");
			}
			try
			{
				Assert.IsTrue(File.Exists(tempLongPathFilename));
				var fileInfo = new FileInfo(tempLongPathFilename);
				Assert.IsNotNull(fileInfo); // just to use fileInfo variable
			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void FileInfoReturnsCorrectDirectoryNameForLongPathFile()
		{
			Assert.IsTrue(Directory.Exists(uncDirectory));
			string tempLongPathFilename;
			do
			{
				tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
			} while (File.Exists(tempLongPathFilename));
			Assert.IsFalse(File.Exists(tempLongPathFilename));

			using (var writer = File.CreateText(tempLongPathFilename))
			{
				writer.WriteLine("test");
			}
			try
			{
				Assert.IsTrue(File.Exists(tempLongPathFilename));
				var fileInfo = new FileInfo(tempLongPathFilename);
				Assert.AreEqual(uncDirectory, fileInfo.DirectoryName);
			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestLengthWithBadPath()
		{
			var filename = Util.CreateNewFile(uncDirectory);
			Pri.LongPath.FileInfo fi;
			try
			{
				Assert.Throws<FileNotFoundException>(() => fi = new FileInfo(filename));
			}
			catch
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestCreateTextAndWrite()
		{
			Assert.IsTrue(Directory.Exists(uncDirectory));
			string tempLongPathFilename;
			do
			{
				tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
			} while (File.Exists(tempLongPathFilename));
			Assert.IsFalse(File.Exists(tempLongPathFilename));

			const string fileText = "test";
			using (var writer = File.CreateText(tempLongPathFilename))
			{
				writer.WriteLine(fileText);
			}
			try
			{
				Assert.IsTrue(File.Exists(tempLongPathFilename));
				var fileInfo = new FileInfo(tempLongPathFilename);
				Assert.AreEqual(fileText.Length + Environment.NewLine.Length, fileInfo.Length);
			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestExistsNonExistent()
		{
			Assert.IsFalse(new FileInfo("giberish").Exists);
		}

		[Test]
		public void TestExists()
		{
			Assert.IsTrue(new FileInfo(filePath).Exists);
		}

		[Test]
		public void TestAppendText()
		{
			var filename = new StringBuilder(uncDirectory).Append(@"\").Append("file16.ext").ToString();
			using (var writer = File.CreateText(filename))
			{
				writer.Write("start");
			}
			Assert.IsTrue(File.Exists(filename));

			try
			{
				using (var writer = new FileInfo(filename).AppendText())
				{
					writer.WriteLine("end");
				}

				using (var reader = File.OpenText(filename))
				{
					var text = reader.ReadLine();
					Assert.AreEqual("startend", text);
				}
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestCopyToWithoutOverwrite()
		{
			var fi = new FileInfo(filePath);
			var destLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename (Copy).ext").ToString();

			fi.CopyTo(destLongPathFilename);

			try
			{
				Assert.IsTrue(File.Exists(destLongPathFilename));

				Assert.AreEqual(File.ReadAllText(filePath), File.ReadAllText(destLongPathFilename));
			}
			finally
			{
				File.Delete(destLongPathFilename);
			}
		}

		[Test]
		public void TestCopyToWithoutOverwriteAndExistingFile()
		{
			var fi = new FileInfo(filePath);
			var destLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename (Copy).ext").ToString();

			fi.CopyTo(destLongPathFilename);

			try
			{
				Assert.IsTrue(File.Exists(destLongPathFilename));
				Assert.Throws<IOException>(() => fi.CopyTo(destLongPathFilename));
			}
			finally
			{
				File.Delete(destLongPathFilename);
			}
		}

		[Test]
		public void TestCopyToWithOverwrite()
		{
			var fi = new FileInfo(filePath);
			var destLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename (Copy).ext").ToString();

			fi.CopyTo(destLongPathFilename);

			try
			{
				Assert.IsTrue(File.Exists(destLongPathFilename));
				fi.CopyTo(destLongPathFilename, true);
				Assert.AreEqual(File.ReadAllText(filePath), File.ReadAllText(destLongPathFilename));
			}
			finally
			{
				File.Delete(destLongPathFilename);
			}
		}

		[Test]
		public void TestCreate()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("file19.ext").ToString();
			var fi = new FileInfo(tempLongPathFilename);
			Assert.IsFalse(fi.Exists);

			using (fi.Create())
			{
			}

			try
			{
				Assert.IsTrue(File.Exists(fi.FullName)); // don't use FileInfo.Exists, it caches existance
			}
			finally
			{
				fi.Delete();
			}
		}

		[Test]
		public void TestCreateText()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("file20.ext").ToString();
			var fi = new FileInfo(tempLongPathFilename);
			Assert.IsFalse(fi.Exists);

			using (fi.CreateText())
			{
			}

			try
			{
				Assert.IsTrue(File.Exists(fi.FullName)); // don't use FileInfo.Exists, it caches existance
			}
			finally
			{
				fi.Delete();
			}
		}

		[Test]
		public void TestMoveTo()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("file21.ext").ToString();
			var tempDestLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("file21-1.ext").ToString();
			Assert.IsFalse(File.Exists(tempLongPathFilename));
			File.Copy(filePath, tempLongPathFilename);
			try
			{
				Assert.IsTrue(File.Exists(tempLongPathFilename));

				var fi = new FileInfo(tempLongPathFilename);
				fi.MoveTo(tempDestLongPathFilename);

				try
				{
					Assert.IsFalse(File.Exists(tempLongPathFilename));
					Assert.IsTrue(File.Exists(tempDestLongPathFilename));
				}
				finally
				{
					File.Delete(tempDestLongPathFilename);
				}
			}
			finally
			{
				if (File.Exists(tempLongPathFilename))
					File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestOpenOpen()
		{
			var fi = new FileInfo(filePath);
			using (var fileStream = fi.Open(FileMode.Open))
			{
				Assert.IsNotNull(fileStream);
			}
		}

		[Test]
		public void TestOpenCreateNew()
		{
			var fi = new FileInfo(filePath);
			Assert.Throws<IOException>(() =>
			{
				using (var fileStream = fi.Open(FileMode.CreateNew))
				{
					Assert.IsNotNull(fileStream);
				}
			});
		}

		[Test]
		public void TestOpenHidden()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("file25.ext").ToString();
			var fi = new FileInfo(tempLongPathFilename);

			using (fi.Create())
			{
			}
			try
			{
				File.SetAttributes(fi.FullName, File.GetAttributes(fi.FullName) | FileAttributes.Hidden);

				Assert.Throws<UnauthorizedAccessException>(() =>
				{
					using (var fileStream = fi.Open(FileMode.Create))
					{
						Assert.IsNotNull(fileStream);
					}
				});

			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestOpenReadWithWrite()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("file31.ext").ToString();
			var fi = new FileInfo(tempLongPathFilename);
			try
			{
				Assert.Throws<NotSupportedException>(() =>
				{
					using (var fileStream = fi.Open(FileMode.Append, FileAccess.Read))
					{
						fileStream.WriteByte(43);
					}
				});
			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestOpenCreatesEmpty()
		{
			var tempLongPathFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
			try
			{
				using (var writer = File.CreateText(tempLongPathFilename))
				{
					writer.WriteLine("test");
				}

				var fi = new FileInfo(tempLongPathFilename);
				using (var fileStream = fi.Open(FileMode.Append, FileAccess.Read, FileShare.Read))
				{
					Assert.AreEqual(-1, fileStream.ReadByte());
				}

			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestOpenReadReadsExistingData()
		{
			var fi = new FileInfo(filePath);
			using (var fileStream = fi.OpenRead())
			{
				Assert.AreEqual('t', fileStream.ReadByte());
			}
		}

		[Test]
		public void TestOpenTextReadsExistingData()
		{
			var fi = new FileInfo(filePath);
			using (var streamReader = fi.OpenText())
			{
				Assert.AreEqual("test", streamReader.ReadLine());
			}
		}

		[Test]
		public void TestOpenWriteWritesCorrectly()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("file31a.ext").ToString();
			var fi = new FileInfo(tempLongPathFilename);
			try
			{
				using (var fileStream = fi.OpenWrite())
				{
					fileStream.WriteByte(42);
				}
				using (var fileStream = fi.OpenRead())
				{
					Assert.AreEqual(42, fileStream.ReadByte());
				}
			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestLastWriteTime()
		{
			var filename = Util.CreateNewFile(uncDirectory);
			try
			{
				var dateTime = DateTime.Now.AddDays(1);
				{
					var fiTemp = new FileInfo(filename) { LastWriteTime = dateTime };
				}
				var fi = new FileInfo(filename);
				Assert.AreEqual(dateTime, fi.LastWriteTime);
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestDecrypt()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename.ext").ToString();
			try
			{
				using (var s = File.Create(tempLongPathFilename, 200))
				{
				}
				var preAttrib = File.GetAttributes(tempLongPathFilename);
				Assert.AreEqual((FileAttributes)0, (preAttrib & FileAttributes.Encrypted));

				var fi = new FileInfo(tempLongPathFilename);
				fi.Encrypt();

				var postAttrib = File.GetAttributes(tempLongPathFilename);
				Assert.AreEqual(FileAttributes.Encrypted, (postAttrib & FileAttributes.Encrypted));

				fi.Decrypt();

				postAttrib = File.GetAttributes(tempLongPathFilename);
				Assert.AreEqual((FileAttributes)0, (postAttrib & FileAttributes.Encrypted));
			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestGetIsReadOnly()
		{
			var filename = Util.CreateNewFile(uncDirectory);
			try
			{
				var fi = new FileInfo(filename);
				Assert.IsTrue(fi.Exists);
				Assert.IsFalse(fi.IsReadOnly);
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestSetIsReadOnly()
		{
			var filename = Util.CreateNewFile(uncDirectory);
			var fi = new FileInfo(filename);
			try
			{
				fi.IsReadOnly = true;
				Assert.IsTrue(fi.IsReadOnly);
			}
			finally
			{
				fi.IsReadOnly = false;
				fi.Delete();
			}
		}

		[Test]
		public void TestReplace()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename.ext").ToString();
			using (var fileStream = File.Create(tempLongPathFilename))
			{
				try
				{
					fileStream.WriteByte(42);
				}
				catch (Exception)
				{
					File.Delete(tempLongPathFilename);
					throw;
				}
			}
			var tempLongPathFilename2 = new StringBuilder(uncDirectory).Append(@"\").Append("filename2.ext").ToString();

			using (var fileStream = File.Create(tempLongPathFilename2))
			{
				try
				{
					fileStream.WriteByte(52);
				}
				catch (Exception)
				{
					File.Delete(tempLongPathFilename2);
					throw;
				}
			}

			var fi = new FileInfo(tempLongPathFilename);
			try
			{
				var fi2 = fi.Replace(tempLongPathFilename2, null);
				Assert.IsNotNull(fi2);
				Assert.AreEqual(tempLongPathFilename2, fi2.FullName);
				using (var fileStream = File.OpenRead(tempLongPathFilename2))
				{
					Assert.AreEqual(42, fileStream.ReadByte());
				}
				Assert.IsFalse(File.Exists(tempLongPathFilename));
			}
			finally
			{
				if (File.Exists(tempLongPathFilename))
					File.Delete(tempLongPathFilename);
				File.Delete(tempLongPathFilename2);
			}
		}

		/// <remarks>
		/// TODO: create a scenario where ignoreMetadataErrors actually makes a difference
		/// </remarks>
		[Test]
		public void TestReplaceIgnoreMerge()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename.ext").ToString();
			using (var fileStream = File.Create(tempLongPathFilename))
			{
				try
				{
					fileStream.WriteByte(42);
				}
				catch (Exception)
				{
					File.Delete(tempLongPathFilename);
					throw;
				}
			}
			var tempLongPathFilename2 = new StringBuilder(uncDirectory).Append(@"\").Append("filename2.ext").ToString();

			using (var fileStream = File.Create(tempLongPathFilename2))
			{
				try
				{
					fileStream.WriteByte(52);
				}
				catch (Exception)
				{
					File.Delete(tempLongPathFilename2);
					throw;
				}
			}
			var fi = new FileInfo(tempLongPathFilename);
			try
			{
				const bool ignoreMetadataErrors = true;
				var fi2 = fi.Replace(tempLongPathFilename2, null, ignoreMetadataErrors);
				Assert.IsNotNull(fi2);
				Assert.AreEqual(tempLongPathFilename2, fi2.FullName);
				using (var fileStream = File.OpenRead(tempLongPathFilename2))
				{
					Assert.AreEqual(42, fileStream.ReadByte());
				}
				Assert.IsFalse(File.Exists(tempLongPathFilename));
			}
			finally
			{
				if (File.Exists(tempLongPathFilename))
					File.Delete(tempLongPathFilename);
				File.Delete(tempLongPathFilename2);
			}
		}

		[Test]
		public void TestToString()
		{
			var fi = new FileInfo(filePath);

			Assert.AreEqual(fi.DisplayPath, fi.ToString());
		}

		[Test]
		public void TestConstructorWithNullPath()
		{
			Assert.Throws<ArgumentNullException>(() => new FileInfo(null));
		}

		[Test, Ignore("does not work on some server/domain systems.")]
		public void TestGetAccessControl()
		{
			var filename = Util.CreateNewFile(uncDirectory);
			try
			{
				var fi = new FileInfo(filename);
				var security = fi.GetAccessControl();
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
				File.Delete(filename);
			}
		}

		[Test, Ignore("does not work on some server/domain systems.")]
		public void TestGetAccessControlSections()
		{
			var filename = Util.CreateNewFile(uncDirectory);
			try
			{
				var fi = new FileInfo(filename);
				FileSecurity security = fi.GetAccessControl(AccessControlSections.Access);
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
				File.Delete(filename);
			}
		}

		[Test]
		public void TestSetAccessControl()
		{
			var filename = Util.CreateNewFile(uncDirectory);
			try
			{
				var fi = new FileInfo(filename);
				var security = new FileSecurity();
				fi.SetAccessControl(security);
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestSetCreationTime()
		{
			var filename = Util.CreateNewFile(uncDirectory);
			try
			{
				DateTime dateTime = DateTime.Now.AddDays(1);
				var fi = new FileInfo(filename) {CreationTime = dateTime};
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
			var filename = Util.CreateNewFile(uncDirectory);
			try
			{
				DateTime dateTime = DateTime.UtcNow.AddDays(1);
				var fi = new FileInfo(filename) {CreationTimeUtc = dateTime};
				Assert.AreEqual(dateTime, File.GetCreationTimeUtc(filename));
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestSetLastWriteTime()
		{
			var filename = Util.CreateNewFile(uncDirectory);
			try
			{
				DateTime dateTime = DateTime.Now.AddDays(1);
				var fi = new FileInfo(filename) {LastWriteTime = dateTime};
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
			var filename = Util.CreateNewFile(uncDirectory);
			try
			{
				DateTime dateTime = DateTime.UtcNow.AddDays(1);
				var fi = new FileInfo(filename) {LastWriteTimeUtc = dateTime};
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
			var filename = Util.CreateNewFile(uncDirectory);
			try
			{
				DateTime dateTime = DateTime.Now.AddDays(1);
				var fi = new FileInfo(filename) {LastAccessTime = dateTime};
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
			var filename = Util.CreateNewFile(uncDirectory);
			try
			{
				DateTime dateTime = DateTime.UtcNow.AddDays(1);
				var fi = new FileInfo(filename) {LastAccessTimeUtc = dateTime};
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
			var filename = Path.Combine(uncDirectory, "gibberish.ext");
			DateTime dateTime = DateTime.Now.AddDays(1);
			var fi = new FileInfo(filename);
			Assert.Throws<FileNotFoundException>(() => fi.CreationTime = dateTime);
		}

		[Test]
		public void TestSetCreationTimeUtcMissingFile()
		{
			var filename = Path.Combine(uncDirectory, "gibberish.ext");
			DateTime dateTime = DateTime.UtcNow.AddDays(1);
			var fi = new FileInfo(filename);
			Assert.Throws<FileNotFoundException>(() => fi.CreationTimeUtc = dateTime);
		}

		[Test]
		public void TestSetLastWriteTimeMissingFile()
		{
			var filename = Path.Combine(uncDirectory, "gibberish.ext");
			DateTime dateTime = DateTime.Now.AddDays(1);
			var fi = new FileInfo(filename);
			Assert.Throws<FileNotFoundException>(() => fi.LastWriteTime = dateTime);
		}

		[Test]
		public void TestSetLastWriteTimeUtcMissingFile()
		{
			var filename = Path.Combine(uncDirectory, "gibberish.ext");
			DateTime dateTime = DateTime.UtcNow.AddDays(1);
			var fi = new FileInfo(filename);
			Assert.Throws<FileNotFoundException>(() => fi.LastWriteTimeUtc = dateTime);
		}

		[Test]
		public void TestSetLastAccessTimeMissingFile()
		{
			var filename = Path.Combine(uncDirectory, "gibberish.ext");
			DateTime dateTime = DateTime.Now.AddDays(1);
			var fi = new FileInfo(filename);
			Assert.Throws<FileNotFoundException>(() => fi.LastAccessTime = dateTime);
		}

		[Test]
		public void TestSetLastAccessTimeUtcMissingFile()
		{
			var filename = Path.Combine(uncDirectory, "gibberish.ext");
			DateTime dateTime = DateTime.UtcNow.AddDays(1);
			var fi = new FileInfo(filename);
			Assert.Throws<FileNotFoundException>(() => fi.LastAccessTimeUtc = dateTime);
		}

		[Test]
		public void TestDisplayPath()
		{
			var sfi = new System.IO.FileInfo(@"c:\Windows\notepad.exe");
			var fi = new FileInfo(@"c:\Windows\notepad.exe");

			Assert.AreEqual(sfi.ToString(), fi.DisplayPath);

		}


		[TearDown]
		public void TearDown()
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