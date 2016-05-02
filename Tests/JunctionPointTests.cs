// 
// Source: http://www.codeproject.com/Articles/15633/Manipulating-NTFS-Junction-Points-in-NET
// rewritten for MSTest (by github.com/SchreinerK)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pri.LongPath;
using NUnit.Framework;

namespace Tests {

	using Path = Pri.LongPath.Path;
	using Directory = Pri.LongPath.Directory;
	using DirectoryInfo = Pri.LongPath.DirectoryInfo;
	using File = Pri.LongPath.File;
	using FileSystemInfo = Pri.LongPath.FileSystemInfo;

    [TestFixture]
    public class JunctionPointTest
    {
        private string tempFolder;

        [SetUp]
        public void CreateTempFolder()
        {
            tempFolder = Path.GetTempFileName();
            File.Delete(tempFolder);
            Directory.CreateDirectory(tempFolder);
        }

        [TearDown]
        public void DeleteTempFolder()
        {
            if (tempFolder != null)
            {
                foreach (FileSystemInfo file in new DirectoryInfo(tempFolder).GetFileSystemInfos())
                {
                    file.Delete();
                }

                Directory.Delete(tempFolder);
                tempFolder = null;
            }
        }

        [Test]
        public void Exists_NoSuchFile()
        {
            Assert.IsFalse(JunctionPoint.Exists(Path.Combine(tempFolder, "$$$NoSuchFolder$$$")));
        }

        [Test]
        public void Exists_IsADirectory()
        {
            File.Create(Path.Combine(tempFolder, "AFile")).Close();

            Assert.IsFalse(JunctionPoint.Exists(Path.Combine(tempFolder, "AFile")));
        }

		[Test]
		public void Create_VerifyExists_GetTarget_Delete()
		{
			string targetFolder = Path.Combine(tempFolder, "ADirectory");
			string junctionPoint = Path.Combine(tempFolder, "SymLink");

			Directory.CreateDirectory(targetFolder);
			try
			{
				File.Create(Path.Combine(targetFolder, "AFile")).Close();
				try
				{
					// Verify behavior before junction point created.
					Assert.IsFalse(File.Exists(Path.Combine(junctionPoint, "AFile")),
						"File should not be located until junction point created.");

					Assert.IsFalse(JunctionPoint.Exists(junctionPoint), "Junction point not created yet.");

					// Create junction point and confirm its properties.
					JunctionPoint.Create(junctionPoint, targetFolder, false /*don't overwrite*/);

					Assert.IsTrue(JunctionPoint.Exists(junctionPoint), "Junction point exists now.");

					Assert.AreEqual(targetFolder, JunctionPoint.GetTarget(junctionPoint));

					Assert.IsTrue(File.Exists(Path.Combine(junctionPoint, "AFile")),
						"File should be accessible via the junction point.");

					// Delete junction point.
					JunctionPoint.Delete(junctionPoint);

					Assert.IsFalse(JunctionPoint.Exists(junctionPoint), "Junction point should not exist now.");

					Assert.IsFalse(File.Exists(Path.Combine(junctionPoint, "AFile")),
						"File should not be located after junction point deleted.");

					Assert.IsFalse(Directory.Exists(junctionPoint), "Ensure directory was deleted too.");
				}
				finally
				{
					File.Delete(Path.Combine(targetFolder, "AFile"));
				}
			}
			finally
			{
				Directory.Delete(targetFolder);
			}
		}

        [Test]
        public void Create_ThrowsIfOverwriteNotSpecifiedAndDirectoryExists()
        {
            string targetFolder = Path.Combine(tempFolder, "ADirectory");
            string junctionPoint = Path.Combine(tempFolder, "SymLink");

            Directory.CreateDirectory(junctionPoint);

			Assert.Throws<IOException>(() => JunctionPoint.Create(junctionPoint, targetFolder, false),
				"Directory already exists and overwrite parameter is false.");
        }

        [Test]
        public void Create_OverwritesIfSpecifiedAndDirectoryExists()
        {
            string targetFolder = Path.Combine(tempFolder, "ADirectory");
            string junctionPoint = Path.Combine(tempFolder, "SymLink");

            Directory.CreateDirectory(junctionPoint);
            Directory.CreateDirectory(targetFolder);

            JunctionPoint.Create(junctionPoint, targetFolder, true);

            Assert.AreEqual(targetFolder, JunctionPoint.GetTarget(junctionPoint));
        }

        [Test]
        public void Create_ThrowsIfTargetDirectoryDoesNotExist()
        {
            string targetFolder = Path.Combine(tempFolder, "ADirectory");
            string junctionPoint = Path.Combine(tempFolder, "SymLink");

			Assert.Throws<IOException>(() => JunctionPoint.Create(junctionPoint, targetFolder, false),
				"Target path does not exist or is not a directory.");
        }

        [Test]
        public void GetTarget_NonExistentJunctionPoint()
        {
			Assert.Throws<IOException>(() => JunctionPoint.GetTarget(Path.Combine(tempFolder, "SymLink")),
				"Unable to open reparse point.");
        }

        [Test]
        public void GetTarget_CalledOnADirectoryThatIsNotAJunctionPoint()
        {
			Assert.Throws<IOException>(() => JunctionPoint.GetTarget(tempFolder),
				"Path is not a junction point.");
        }

        [Test]
        public void GetTarget_CalledOnAFile()
        {
            File.Create(Path.Combine(tempFolder, "AFile")).Close();

			Assert.Throws<IOException>(() => JunctionPoint.GetTarget(Path.Combine(tempFolder, "AFile")),
				"Path is not a junction point.");
        }

        [Test]
        public void Delete_NonExistentJunctionPoint()
        {
            // Should do nothing.
            JunctionPoint.Delete(Path.Combine(tempFolder, "SymLink"));
        }

        [Test]
        public void Delete_CalledOnADirectoryThatIsNotAJunctionPoint()
        {
			Assert.Throws<IOException>(() => JunctionPoint.Delete(tempFolder),
				"Unable to delete junction point.");
        }

        [Test]
        public void Delete_CalledOnAFile()
        {
            File.Create(Path.Combine(tempFolder, "AFile")).Close();

			Assert.Throws<IOException>(() => JunctionPoint.Delete(Path.Combine(tempFolder, "AFile")),
				"Path is not a junction point.");
        }
    }
}
