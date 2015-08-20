// 
// Source: http://www.codeproject.com/Articles/15633/Manipulating-NTFS-Junction-Points-in-NET
// rewritten for MSTest (by github.com/SchreinerK)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pri.LongPath;

namespace Tests {

	using Path = Pri.LongPath.Path;
	using Directory = Pri.LongPath.Directory;
	using DirectoryInfo = Pri.LongPath.DirectoryInfo;
	using File = Pri.LongPath.File;
	using FileSystemInfo = Pri.LongPath.FileSystemInfo;

    [TestClass]
    public class JunctionPointTest
    {
        private string tempFolder;

        [TestInitialize]
        public void CreateTempFolder()
        {
            tempFolder = Path.GetTempFileName();
            File.Delete(tempFolder);
            Directory.CreateDirectory(tempFolder);
        }

        [TestCleanup]
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

        [TestMethod]
        public void Exists_NoSuchFile()
        {
            Assert.IsFalse(JunctionPoint.Exists(Path.Combine(tempFolder, "$$$NoSuchFolder$$$")));
        }

        [TestMethod]
        public void Exists_IsADirectory()
        {
            File.Create(Path.Combine(tempFolder, "AFile")).Close();

            Assert.IsFalse(JunctionPoint.Exists(Path.Combine(tempFolder, "AFile")));
        }

        [TestMethod]
        public void Create_VerifyExists_GetTarget_Delete()
        {
            string targetFolder = Path.Combine(tempFolder, "ADirectory");
            string junctionPoint = Path.Combine(tempFolder, "SymLink");

            Directory.CreateDirectory(targetFolder);
            File.Create(Path.Combine(targetFolder, "AFile")).Close();

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

            // Cleanup
            File.Delete(Path.Combine(targetFolder, "AFile"));
        }

        [TestMethod]
        [ExpectedException(typeof(IOException), "Directory already exists and overwrite parameter is false.")]
        public void Create_ThrowsIfOverwriteNotSpecifiedAndDirectoryExists()
        {
            string targetFolder = Path.Combine(tempFolder, "ADirectory");
            string junctionPoint = Path.Combine(tempFolder, "SymLink");

            Directory.CreateDirectory(junctionPoint);

            JunctionPoint.Create(junctionPoint, targetFolder, false);
        }

        [TestMethod]
        public void Create_OverwritesIfSpecifiedAndDirectoryExists()
        {
            string targetFolder = Path.Combine(tempFolder, "ADirectory");
            string junctionPoint = Path.Combine(tempFolder, "SymLink");

            Directory.CreateDirectory(junctionPoint);
            Directory.CreateDirectory(targetFolder);

            JunctionPoint.Create(junctionPoint, targetFolder, true);

            Assert.AreEqual(targetFolder, JunctionPoint.GetTarget(junctionPoint));
        }

        [TestMethod]
        [ExpectedException(typeof(IOException), "Target path does not exist or is not a directory.")]
        public void Create_ThrowsIfTargetDirectoryDoesNotExist()
        {
            string targetFolder = Path.Combine(tempFolder, "ADirectory");
            string junctionPoint = Path.Combine(tempFolder, "SymLink");

            JunctionPoint.Create(junctionPoint, targetFolder, false);
        }

        [TestMethod]
        [ExpectedException(typeof(IOException), "Unable to open reparse point.")]
        public void GetTarget_NonExistentJunctionPoint()
        {
            JunctionPoint.GetTarget(Path.Combine(tempFolder, "SymLink"));
        }

        [TestMethod]
        [ExpectedException(typeof(IOException), "Path is not a junction point.")]
        public void GetTarget_CalledOnADirectoryThatIsNotAJunctionPoint()
        {
            JunctionPoint.GetTarget(tempFolder);
        }

        [TestMethod]
        [ExpectedException(typeof(IOException), "Path is not a junction point.")]
        public void GetTarget_CalledOnAFile()
        {
            File.Create(Path.Combine(tempFolder, "AFile")).Close();

            JunctionPoint.GetTarget(Path.Combine(tempFolder, "AFile"));
        }

        [TestMethod]
        public void Delete_NonExistentJunctionPoint()
        {
            // Should do nothing.
            JunctionPoint.Delete(Path.Combine(tempFolder, "SymLink"));
        }

        [TestMethod]
        [ExpectedException(typeof(IOException), "Unable to delete junction point.")]
        public void Delete_CalledOnADirectoryThatIsNotAJunctionPoint()
        {
            JunctionPoint.Delete(tempFolder);
        }

        [TestMethod]
        [ExpectedException(typeof(IOException), "Path is not a junction point.")]
        public void Delete_CalledOnAFile()
        {
            File.Create(Path.Combine(tempFolder, "AFile")).Close();

            JunctionPoint.Delete(Path.Combine(tempFolder, "AFile"));
        }
    }
}
