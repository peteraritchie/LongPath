using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections.Generic;
using System.Security.AccessControl;

namespace Tests
{
	using Directory = Pri.LongPath.Directory;
	using File = Pri.LongPath.File;
	using FileInfo = Pri.LongPath.FileInfo;
	using DirectoryInfo = Pri.LongPath.DirectoryInfo;
	using Path = Pri.LongPath.Path;
	using IOException = System.IO.IOException;
	using SeekOrigin = System.IO.SeekOrigin;
	using FileOptions = System.IO.FileOptions;
	using FileAttributes = System.IO.FileAttributes;
	using FileMode = System.IO.FileMode;
	using FileAccess = System.IO.FileAccess;
	using FileNotFoundException = System.IO.FileNotFoundException;
	using DirectoryNotFoundException = System.IO.DirectoryNotFoundException;

	[TestFixture]
	public class UncFileTests
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
		public void TestExists()
		{
			Assert.IsTrue(File.Exists(filePath));
		}

		[Test]
		public void TestCreateText()
		{
			var filename = new StringBuilder(uncDirectory).Append(@"\").Append("file3.ext").ToString();
			const string fileText = "test";
			using (var writer = File.CreateText(filename))
			{
				writer.WriteLine(fileText);
			}
			try
			{
				Assert.IsTrue(File.Exists(filename));

				using (var reader = File.OpenText(filename))
				{
					var text = reader.ReadLine();
					Assert.AreEqual(fileText, text);
				}
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestWriteAllText()
		{
			var filename = new StringBuilder(uncDirectory).Append(@"\").Append("file3.ext").ToString();
			const string fileText = "test";
			using (File.CreateText(filename))
			{
			}
			try
			{
				File.WriteAllText(filename, fileText);
				Assert.AreEqual(fileText, File.ReadAllText(filename));
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestReadAllTextNewFile()
		{
			var filename = new StringBuilder(uncDirectory).Append(@"\").Append("file3.ext").ToString();
			const string fileText = "test";
			using (File.CreateText(filename))
			{
			}
			try
			{
				File.WriteAllText(filename, fileText);
				Assert.AreEqual(fileText, File.ReadAllText(filename));
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestReadAllTextNullPath()
		{
			Assert.Throws<ArgumentNullException>(() => File.ReadAllText(null));
		}


		[Test]
		public void TestWriteAllTextNullPath()
		{
			Assert.Throws<ArgumentNullException>(() => File.WriteAllText(null, "test"));
		}

		[Test]
		public void TestWriteAllTextEncoding()
		{
			var filename = new StringBuilder(uncDirectory).Append(@"\").Append("file3.ext").ToString();
			const string fileText = "test";
			using (File.CreateText(filename))
			{
			}
			try
			{
				File.WriteAllText(filename, fileText, Encoding.Unicode);
				Assert.AreEqual(fileText, File.ReadAllText(filename, Encoding.Unicode));
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestReadAllTextEncoding()
		{
			var filename = new StringBuilder(uncDirectory).Append(@"\").Append("file3.ext").ToString();
			const string fileText = "test";
			using (File.CreateText(filename))
			{
			}
			try
			{
				File.WriteAllText(filename, fileText, Encoding.Unicode);
				Assert.AreEqual(fileText, File.ReadAllText(filename, Encoding.Unicode));
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestDirectoryWithRoot()
		{
			var fi = new FileInfo(@"C:\");
			Assert.IsNull(fi.Directory);
		}

		[Test]
		public void FileInfoReturnsCorrectDirectoryForLongPathFile()
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
				Assert.AreEqual(uncDirectory, fileInfo.Directory.FullName);
				Assert.AreEqual(Path.GetFileName(uncDirectory), fileInfo.Directory.Name);
			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestReadAllText()
		{
			Assert.AreEqual("test" + Environment.NewLine, File.ReadAllText(filePath));
		}

		[Test]
		public void TestCopy()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("file22.ext").ToString();
			var tempDestLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("file22-1.ext").ToString();
			Assert.IsFalse(File.Exists(tempLongPathFilename));
			File.Copy(filePath, tempLongPathFilename);
			try
			{
				Assert.IsTrue(File.Exists(tempLongPathFilename));

				File.Move(tempLongPathFilename, tempDestLongPathFilename);

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
		public void TestCopyWithoutOverwrite()
		{
			var destLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename (Copy).ext").ToString();

			File.Copy(filePath, destLongPathFilename);

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
		public void TestCopyWithoutOverwriteAndExistingFile()
		{
			var destLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename (Copy).ext").ToString();

			File.Copy(filePath, destLongPathFilename);

			try
			{
				Assert.IsTrue(File.Exists(destLongPathFilename));
				Assert.Throws<IOException>(() => File.Copy(filePath, destLongPathFilename));
			}
			finally
			{
				File.Delete(destLongPathFilename);
			}
		}

		[Test]
		public void TestCopyWithOverwrite()
		{
			var destLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename (Copy).ext").ToString();

			File.Copy(filePath, destLongPathFilename);

			try
			{
				Assert.IsTrue(File.Exists(destLongPathFilename));
				File.Copy(filePath, destLongPathFilename, true);
				Assert.AreEqual(File.ReadAllText(filePath), File.ReadAllText(destLongPathFilename));
			}
			finally
			{
				File.Delete(destLongPathFilename);
			}
		}

		[Test]
		public void TestMove()
		{
			string sourceFilename = Util.CreateNewFile(uncDirectory);
			string destFilename = Path.Combine(uncDirectory, Path.GetRandomFileName());
			File.Move(sourceFilename, destFilename);
			try
			{
				Assert.IsFalse(File.Exists(sourceFilename));
				Assert.IsTrue(File.Exists(destFilename));
				Assert.IsTrue(Util.VerifyContentsOfNewFile(destFilename));
			}
			finally
			{
				if (File.Exists(destFilename)) File.Delete(destFilename);
			}
		}

		[Test]
		public void TestReplace()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename.ext").ToString();
			using (var fileStream = File.Create(tempLongPathFilename))
			{
				fileStream.WriteByte(42);
			}
			var tempLongPathFilename2 = new StringBuilder(uncDirectory).Append(@"\").Append("filename2.ext").ToString();

			using (var fileStream = File.Create(tempLongPathFilename2))
			{
				fileStream.WriteByte(52);
			}
			try
			{
				File.Replace(tempLongPathFilename, tempLongPathFilename2, null);
				using (var fileStream = File.OpenRead(tempLongPathFilename2))
				{
					Assert.AreEqual(42, fileStream.ReadByte());
				}
				Assert.IsFalse(File.Exists(tempLongPathFilename));
			}
			finally
			{
				File.Delete(tempLongPathFilename2);
			}
		}

		[Test]
		public void TestReplaceWithNulls()
		{
			string filename = Util.CreateNewFile(uncDirectory);
			try
			{
				Assert.Throws<ArgumentNullException>(() => File.Replace(null, null, null));
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestReplaceWithNullDestination()
		{
			Assert.Throws<ArgumentNullException>(() => File.Replace(filePath, null, null));
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
				fileStream.WriteByte(42);
			}
			var tempLongPathFilename2 = new StringBuilder(uncDirectory).Append(@"\").Append("filename2.ext").ToString();

			using (var fileStream = File.Create(tempLongPathFilename2))
			{
				fileStream.WriteByte(52);
			}
			try
			{
				const bool ignoreMetadataErrors = true;
				File.Replace(tempLongPathFilename, tempLongPathFilename2, null, ignoreMetadataErrors);
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
		public void TestReplaceIgnoreMergeWithBackup()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename.ext").ToString();
			var tempBackupLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("backup").ToString();
			using (var fileStream = File.Create(tempLongPathFilename))
			{
				fileStream.WriteByte(42);
			}
			var tempLongPathFilename2 = new StringBuilder(uncDirectory).Append(@"\").Append("filename2.ext").ToString();

			using (var fileStream = File.Create(tempLongPathFilename2))
			{
				fileStream.WriteByte(52);
			}
			try
			{
				const bool ignoreMetadataErrors = true;
				File.Replace(tempLongPathFilename, tempLongPathFilename2, tempBackupLongPathFilename, ignoreMetadataErrors);
				using (var fileStream = File.OpenRead(tempLongPathFilename2))
				{
					Assert.AreEqual(42, fileStream.ReadByte());
				}
				Assert.IsFalse(File.Exists(tempLongPathFilename));
				Assert.IsTrue(File.Exists(tempBackupLongPathFilename));
			}
			finally
			{
				if (File.Exists(tempLongPathFilename))
					File.Delete(tempLongPathFilename);
				File.Delete(tempLongPathFilename2);
				File.Delete(tempBackupLongPathFilename);
			}
		}

		[Test]
		public void TestReplaceIgnoreMergeWithInvalidBackupPath()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename.ext").ToString();
			var tempBackupLongPathFilename = new StringBuilder(uncDirectory).Append(@"\gibberish\").Append("backup").ToString();
			using (var fileStream = File.Create(tempLongPathFilename))
			{
				fileStream.WriteByte(42);
			}
			var tempLongPathFilename2 = new StringBuilder(uncDirectory).Append(@"\").Append("filename2.ext").ToString();

			using (var fileStream = File.Create(tempLongPathFilename2))
			{
				fileStream.WriteByte(52);
			}
			try
			{
				const bool ignoreMetadataErrors = true;
				Assert.Throws<IOException>(() => File.Replace(tempLongPathFilename, tempLongPathFilename2, tempBackupLongPathFilename, ignoreMetadataErrors));
			}
			finally
			{
				if (File.Exists(tempLongPathFilename))
					File.Delete(tempLongPathFilename);
				File.Delete(tempLongPathFilename2);
				Assert.Throws<DirectoryNotFoundException>(() => File.Delete(tempBackupLongPathFilename));
			}
		}

		[Test]
		public void TestReplaceIgnoreMergeWithReadonlyBackupPath()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename.ext").ToString();
			var tempBackupPathName = new StringBuilder(uncDirectory).Append(@"\readonly").ToString();
			var di = new DirectoryInfo(tempBackupPathName);
			di.Create();

			var attr = di.Attributes;
			di.Attributes = attr | FileAttributes.ReadOnly;
			var tempBackupLongPathFilename = new StringBuilder(tempBackupPathName).Append(@"\").Append("backup").ToString();
			using (var fileStream = File.Create(tempLongPathFilename))
			{
				fileStream.WriteByte(42);
			}
			var tempLongPathFilename2 = new StringBuilder(uncDirectory).Append(@"\").Append("filename2.ext").ToString();

			using (var fileStream = File.Create(tempLongPathFilename2))
			{
				fileStream.WriteByte(52);
			}
			try
			{
				const bool ignoreMetadataErrors = true;
				File.Replace(tempLongPathFilename, tempLongPathFilename2, tempBackupLongPathFilename, ignoreMetadataErrors);
				using (var fileStream = File.OpenRead(tempLongPathFilename2))
				{
					Assert.AreEqual(42, fileStream.ReadByte());
				}
				Assert.IsFalse(File.Exists(tempLongPathFilename));
				Assert.IsTrue(File.Exists(tempBackupLongPathFilename));
			}
			finally
			{
				di.Attributes = attr;
				if (File.Exists(tempLongPathFilename))
					File.Delete(tempLongPathFilename);
				File.Delete(tempLongPathFilename2);
				File.Delete(tempBackupLongPathFilename);
				if (Directory.Exists(tempBackupPathName))
					Directory.Delete(tempBackupPathName);
			}
		}

		[Test]
		public void TestReplaceIgnoreMergeNulls()
		{
			const bool ignoreMetadataErrors = true;
			Assert.Throws<ArgumentNullException>(() => File.Replace(null, null, null, ignoreMetadataErrors));
		}

		[Test]
		public void TestReplaceIgnoreMergeNullDestination()
		{
			const bool ignoreMetadataErrors = true;
			string filename = Util.CreateNewFile(uncDirectory);
			try
			{
				Assert.Throws<ArgumentNullException>(() => File.Replace(filePath, null, null, ignoreMetadataErrors));
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestAppendText()
		{
			string filename = Util.CreateNewFile(uncDirectory);
			try
			{
				using (var sw = File.AppendText(filename))
				{
					sw.WriteLine("end of file");
				}
				var lines = File.ReadLines(filename);
				Assert.IsTrue(new[] { "beginning of file", "end of file" }.SequenceEqual(lines));

			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestCreateWithBuffersize()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename.ext").ToString();
			try
			{
				using (var s = File.Create(tempLongPathFilename, 200))
				{
					s.WriteByte(42);
					s.Seek(0, SeekOrigin.Begin);
					Assert.AreEqual(42, s.ReadByte());
				}

			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestEncrypt()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename.ext").ToString();
			try
			{
				using (var s = File.Create(tempLongPathFilename, 200))
				{
				}
				var preAttrib = File.GetAttributes(tempLongPathFilename);
				Assert.AreEqual((FileAttributes)0, (preAttrib & FileAttributes.Encrypted));
				File.Encrypt(tempLongPathFilename);
				var postAttrib = File.GetAttributes(tempLongPathFilename);
				Assert.AreEqual(FileAttributes.Encrypted, (postAttrib & FileAttributes.Encrypted));
			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestEncryptNonExistentFile()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append(Path.GetRandomFileName()).ToString();
			Assert.Throws<FileNotFoundException>(() => File.Encrypt(tempLongPathFilename));
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
				File.Encrypt(tempLongPathFilename);
				var postAttrib = File.GetAttributes(tempLongPathFilename);
				Assert.AreEqual(FileAttributes.Encrypted, (postAttrib & FileAttributes.Encrypted));
				File.Decrypt(tempLongPathFilename);
				postAttrib = File.GetAttributes(tempLongPathFilename);
				Assert.AreEqual((FileAttributes)0, (postAttrib & FileAttributes.Encrypted));
			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestDecryptNonExistentFile()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append(Path.GetRandomFileName()).ToString();
			Assert.Throws<FileNotFoundException>(() => File.Decrypt(tempLongPathFilename));
		}

		[Test]
		public void TestCreate()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename.ext").ToString();
			try
			{
				using (var s = File.Create(tempLongPathFilename))
				{
					s.WriteByte(42);
					s.Seek(0, SeekOrigin.Begin);
					Assert.AreEqual(42, s.ReadByte());
				}

			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestCreateWithBuffersizeFileOptions()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename.ext").ToString();
			using (var s = File.Create(tempLongPathFilename, 200, FileOptions.DeleteOnClose))
			{
				s.WriteByte(42);
				s.Seek(0, SeekOrigin.Begin);
				Assert.AreEqual(42, s.ReadByte());
			}
			Assert.IsFalse(File.Exists(tempLongPathFilename));
		}

		[Test]
		public void TestOpenExisting()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename.ext").ToString();
			try
			{
				using (var s = File.Create(tempLongPathFilename))
				{
					s.WriteByte(42);
				}
				using (var stream = File.Open(tempLongPathFilename, FileMode.Open))
				{
					Assert.IsNotNull(stream);
				}
			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestOpenNonExistent()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append(Path.GetRandomFileName()).ToString();
			Assert.Throws<FileNotFoundException>(() =>
			{
				using (File.Open(tempLongPathFilename, FileMode.Open))
				{
				}
			});
		}

		[Test]
		public void TestOpenWithAccessNonExistent()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append(Path.GetRandomFileName()).ToString();
			Assert.Throws<FileNotFoundException>(() =>
			{
				using (File.Open(tempLongPathFilename, FileMode.Open, FileAccess.Read))
				{
				}
			});
		}

		[Test]
		public void TestOpenWithAccess()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append(Path.GetRandomFileName()).ToString();
			try
			{
				using (var s = File.Create(tempLongPathFilename))
				{
					s.WriteByte(42);
				}
				using (File.Open(tempLongPathFilename, FileMode.Open, FileAccess.Read))
				{
				}
			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestOpenRead()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename.ext").ToString();
			try
			{
				using (var s = File.Create(tempLongPathFilename))
				{
					s.WriteByte(42);
				}
				using (var stream = File.OpenRead(tempLongPathFilename))
				{
					Assert.AreEqual(42, stream.ReadByte());
				}
			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestOpenWrite()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename.ext").ToString();
			try
			{
				using (File.Create(tempLongPathFilename))
				{
				}
				using (var stream = File.OpenWrite(tempLongPathFilename))
				{
					stream.WriteByte(42);
				}
				using (var stream = File.OpenRead(tempLongPathFilename))
				{
					Assert.AreEqual(42, stream.ReadByte());
				}
			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestSetCreationTime()
		{
			var filename = Util.CreateNewFile(uncDirectory);
			try
			{
				DateTime dateTime = DateTime.Now.AddDays(1);
				File.SetCreationTime(filename, dateTime);
				var fi = new FileInfo(filename);
				Assert.AreEqual(fi.CreationTime, dateTime);

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
				File.SetCreationTimeUtc(filename, dateTime);
				var fi = new FileInfo(filename);
				Assert.AreEqual(fi.CreationTimeUtc, dateTime);

			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestGetCreationTime()
		{
			var filename = Util.CreateNewFile(uncDirectory);
			try
			{
				var dateTime = File.GetCreationTime(filename);
				var fi = new FileInfo(filename);
				Assert.AreEqual(fi.CreationTime, dateTime);
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestGetCreationTimeUTc()
		{
			var filename = Util.CreateNewFile(uncDirectory);
			try
			{
				var dateTime = File.GetCreationTimeUtc(filename);
				var fi = new FileInfo(filename);
				Assert.AreEqual(fi.CreationTimeUtc, dateTime);
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
				File.SetLastWriteTime(filename, dateTime);
				var fi = new FileInfo(filename);
				Assert.AreEqual(fi.LastWriteTime, dateTime);

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
				File.SetLastWriteTimeUtc(filename, dateTime);
				var fi = new FileInfo(filename);
				Assert.AreEqual(fi.LastWriteTimeUtc, dateTime);

			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestGetLastWriteTime()
		{
			var filename = Util.CreateNewFile(uncDirectory);
			try
			{
				var dateTime = File.GetLastWriteTime(filename);
				var fi = new FileInfo(filename);
				Assert.AreEqual(fi.LastWriteTime, dateTime);
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestGetLastWriteTimeUtc()
		{
			var filename = Util.CreateNewFile(uncDirectory);
			try
			{
				var dateTime = File.GetLastWriteTimeUtc(filename);
				var fi = new FileInfo(filename);
				Assert.AreEqual(fi.LastWriteTimeUtc, dateTime);
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
				File.SetLastAccessTime(filename, dateTime);
				var fi = new FileInfo(filename);
				Assert.AreEqual(fi.LastAccessTime, dateTime);

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
				File.SetLastAccessTimeUtc(filename, dateTime);
				var fi = new FileInfo(filename);
				Assert.AreEqual(fi.LastAccessTimeUtc, dateTime);

			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestGetLastAccessTime()
		{
			var filename = Util.CreateNewFile(uncDirectory);
			try
			{
				var dateTime = File.GetLastAccessTime(filename);
				var fi = new FileInfo(filename);
				Assert.AreEqual(fi.LastAccessTime, dateTime);
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestGetLastAccessTimeUtc()
		{
			var filename = Util.CreateNewFile(uncDirectory);
			try
			{
				var dateTime = File.GetLastAccessTimeUtc(filename);
				var fi = new FileInfo(filename);
				Assert.AreEqual(fi.LastAccessTimeUtc, dateTime);
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestOpenAppend()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("file26.ext").ToString();
			var fi = new FileInfo(tempLongPathFilename);
			using (var streamWriter = fi.CreateText())
			{
				streamWriter.WriteLine("file26");
			}
			try
			{
				using (var fileStream = fi.Open(System.IO.FileMode.Append))
				{
					Assert.IsNotNull(fileStream);
					using (var streamWriter = new System.IO.StreamWriter(fileStream))
					{
						streamWriter.WriteLine("eof");
					}
				}

				Assert.AreEqual("file26" + Environment.NewLine + "eof" + Environment.NewLine, File.ReadAllText(fi.FullName));
			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestReadAllTextWithEncoding()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("file26.ext").ToString();
			var fi = new FileInfo(tempLongPathFilename);
			try
			{
				using (var streamWriter = File.CreateText(tempLongPathFilename, Encoding.Unicode))
				{
					streamWriter.WriteLine("file26");
				}

				Assert.AreEqual("file26" + Environment.NewLine, File.ReadAllText(fi.FullName, Encoding.Unicode));
			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestReadAllBytes()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename.ext").ToString();
			using (var fileStream = File.Create(tempLongPathFilename))
			{
				fileStream.WriteByte(42);
			}
			try
			{
				Assert.IsTrue(new byte[] { 42 }.SequenceEqual(File.ReadAllBytes(tempLongPathFilename)));
			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestReadAllBytesOnHugeFile()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename.ext").ToString();
			using (var fileStream = File.Create(tempLongPathFilename))
			{
				fileStream.Seek(((long)int.MaxValue) + 1, SeekOrigin.Begin);
				fileStream.WriteByte(42);
			}
			try
			{
				Assert.Throws<IOException>(() => File.ReadAllBytes(tempLongPathFilename));
			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestWriteAllBytes()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename.ext").ToString();
			var expected = new byte[] { 3, 4, 1, 5, 9, 2, 6, 5 };
			File.WriteAllBytes(tempLongPathFilename, expected);
			try
			{
				Assert.IsTrue(expected.SequenceEqual(File.ReadAllBytes(tempLongPathFilename)));
			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestReadAllLines()
		{
			string filename = Util.CreateNewFile(uncDirectory);
			try
			{
				using (var sw = File.AppendText(filename))
				{
					sw.WriteLine("end of file");
				}
				var lines = File.ReadAllLines(filename);
				Assert.IsTrue(new[] { "beginning of file", "end of file" }.SequenceEqual(lines));

			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestWriteAllLines()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("file26.ext").ToString();
			File.WriteAllLines(tempLongPathFilename, new string[] { "file26" });
			try
			{
				Assert.AreEqual("file26" + Environment.NewLine, File.ReadAllText(tempLongPathFilename));
			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestWriteAllLinesWithEncoding()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("file26.ext").ToString();
			File.WriteAllLines(tempLongPathFilename, new string[] { "file26" }, Encoding.Unicode);
			try
			{

				Assert.AreEqual("file26" + Environment.NewLine, File.ReadAllText(tempLongPathFilename, Encoding.Unicode));
			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestWriteAllLinesEnumerable()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("file26.ext").ToString();
			File.WriteAllLines(tempLongPathFilename, new List<string> { "file26" });
			try
			{
				Assert.AreEqual("file26" + Environment.NewLine, File.ReadAllText(tempLongPathFilename));
			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestWriteAllLinesWithEncodingEnumerable()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("file26.ext").ToString();
			File.WriteAllLines(tempLongPathFilename, new List<string> { "file26" }, Encoding.Unicode);
			try
			{

				Assert.AreEqual("file26" + Environment.NewLine, File.ReadAllText(tempLongPathFilename, Encoding.Unicode));
			}
			finally
			{
				File.Delete(tempLongPathFilename);
			}
		}

		[Test]
		public void TestAppendAllText()
		{
			var filename = Util.CreateNewFile(uncDirectory);
			try
			{
				File.AppendAllText(filename, "test");
				Assert.AreEqual("beginning of file" + Environment.NewLine + "test", File.ReadAllText(filename));
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestAppendAllTextEncoding()
		{
			var filename = Util.CreateNewFileUnicode(uncDirectory);
			try
			{
				File.AppendAllText(filename, "test", Encoding.Unicode);
				Assert.AreEqual("beginning of file" + Environment.NewLine + "test", File.ReadAllText(filename, Encoding.Unicode));
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestAppendAllLines()
		{
			var filename = Util.CreateNewFile(uncDirectory);
			try
			{
				File.AppendAllLines(filename, new[] { "test1", "test2" });
				Assert.AreEqual("beginning of file" + Environment.NewLine + "test1" + Environment.NewLine + "test2" + Environment.NewLine,
					File.ReadAllText(filename));
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void TestAppendAllLinesEncoding()
		{
			var filename = Util.CreateNewFileUnicode(uncDirectory);
			try
			{
				File.AppendAllLines(filename, new[] { "test1", "test2" }, Encoding.Unicode);
				Assert.AreEqual("beginning of file" + Environment.NewLine + "test1" + Environment.NewLine + "test2" + Environment.NewLine,
					File.ReadAllText(filename, Encoding.Unicode));
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test, Ignore("does not work on some server/domain systems.")]
		public void TestGetAccessControl()
		{
			var filename = Util.CreateNewFile(uncDirectory);
			try
			{
				var security = File.GetAccessControl(filename);
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
				var security = File.GetAccessControl(filename, AccessControlSections.Access);
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
				var security = new FileSecurity();
				File.SetAccessControl(filename, security);
			}
			finally
			{
				File.Delete(filename);
			}
		}

		/// <remarks>
		/// TODO: more realistic FileSecurity scenarios
		/// </remarks>
		[Test]
		public void TestCreateWithFileSecurity()
		{
			var tempLongPathFilename = new StringBuilder(uncDirectory).Append(@"\").Append("filename.ext").ToString();
			using (var s = File.Create(tempLongPathFilename, 200, FileOptions.DeleteOnClose, new FileSecurity()))
			{
				s.WriteByte(42);
				s.Seek(0, SeekOrigin.Begin);
				Assert.AreEqual(42, s.ReadByte());
			}
			Assert.IsFalse(File.Exists(tempLongPathFilename));
		}

		[Test]
		public void TestGetLastWriteTimeOnMissingFileHasNoException()
		{
			var dt = File.GetLastWriteTime("gibberish");
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