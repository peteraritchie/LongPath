using System;
using System.Collections.Generic;
using System.Security.AccessControl;
#if NET_2_0
using System.Runtime.CompilerServices;
#else
using System.Linq;

#endif

namespace Pri.LongPath
{
	using SearchOption = System.IO.SearchOption;
	using IOException = System.IO.IOException;

	public class DirectoryInfo : FileSystemInfo
	{
		public override System.IO.FileSystemInfo SystemInfo
		{
			get { return SysDirectoryInfo; }
		}

		private System.IO.DirectoryInfo SysDirectoryInfo
		{
			get { return new System.IO.DirectoryInfo(FullPath); }
		}

		public override bool Exists
		{
			get
			{
				if (InstanceState == State.Uninitialized)
				{
					Refresh();
				}
				return InstanceState == State.Initialized &&
				       (Data.fileAttributes & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory;
			}
		}

		public override string Name { get; }

		public DirectoryInfo Parent
		{
			get
			{
				string fullPath = FullPath;
				if (fullPath.Length > 3 && fullPath.EndsWith(Path.DirectorySeparatorChar))
				{
					fullPath = FullPath.Substring(0, FullPath.Length - 1);
				}
				string directoryName = Path.GetDirectoryName(fullPath);
				return directoryName == null ? null : new DirectoryInfo(directoryName);
			}
		}

		public DirectoryInfo Root
		{
			get
			{
				int rootLength = Path.GetRootLength(FullPath);
				string str = FullPath.Substring(0, rootLength - (Common.IsPathUnc(FullPath) ? 1 : 0));
				return new DirectoryInfo(str);
			}
		}

		public DirectoryInfo(string path)
		{
			if (path == null) throw new ArgumentNullException("path");
			OriginalPath = path;
			FullPath = Path.GetFullPath(path);
			Name = (OriginalPath.Length != 2 || OriginalPath[1] != ':' ? GetDirName(FullPath) : ".");
		}

		public void Create()
		{
			Directory.CreateDirectory(FullPath);
		}

		public DirectoryInfo CreateSubdirectory(string path)
		{
			var newDir = Path.Combine(FullPath, path);
			var newFullPath = Path.GetFullPath(newDir);
			if (string.Compare(FullPath, 0, newFullPath, 0, FullPath.Length, StringComparison.OrdinalIgnoreCase) != 0)
			{
				throw new ArgumentException("Invalid subpath", path);
			}
			Directory.CreateDirectory(newDir);
			return new DirectoryInfo(newDir);
		}

		public override void Delete()
		{
			Directory.Delete(FullPath);
		}

		public void Delete(bool recursive)
		{
			Directory.Delete(FullPath, recursive);
		}

#if NET_4_0 || NET_4_5
		public IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern)
		{
			if (Common.IsRunningOnMono())
				return SysDirectoryInfo.EnumerateDirectories(searchPattern).Select(s => new DirectoryInfo(s.FullName));

			return Directory.EnumerateFileSystemEntries(FullPath, searchPattern, true, false, SearchOption.TopDirectoryOnly)
				.Select(directory => new DirectoryInfo(directory));
		}

		public IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption)
		{
			if (Common.IsRunningOnMono())
				return SysDirectoryInfo.EnumerateDirectories(searchPattern, searchOption)
					.Select(s => new DirectoryInfo(s.FullName));

			return Directory.EnumerateFileSystemEntries(FullPath, searchPattern, true, false, searchOption)
				.Select(directory => new DirectoryInfo(directory));
		}

		public IEnumerable<FileInfo> EnumerateFiles()
		{
			return Directory.EnumerateFiles(FullPath).Select(e => new FileInfo(e));
		}

		public IEnumerable<FileInfo> EnumerateFiles(string searchPattern)
		{
			if (Common.IsRunningOnMono())
				return SysDirectoryInfo.EnumerateFiles(searchPattern).Select(s => new FileInfo(s.FullName));

			return Directory.EnumerateFileSystemEntries(FullPath, searchPattern, false, true, SearchOption.TopDirectoryOnly)
				.Select(e => new FileInfo(e));
		}

		public IEnumerable<FileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
		{
			if (Common.IsRunningOnMono())
				return SysDirectoryInfo.EnumerateFiles(searchPattern, searchOption).Select(s => new FileInfo(s.FullName));

			return Directory.EnumerateFileSystemEntries(FullPath, searchPattern, false, true, searchOption)
				.Select(e => new FileInfo(e));
		}

		public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos()
		{
			return
				Directory.EnumerateFileSystemEntries(FullPath)
					.Select(e => Directory.Exists(e) ? (FileSystemInfo) new DirectoryInfo(e) : (FileSystemInfo) new FileInfo(e));
		}

		public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string searchPattern)
		{
			if (Common.IsRunningOnMono())
				return SysDirectoryInfo.EnumerateFileSystemInfos(searchPattern)
					.Select(e => System.IO.Directory.Exists(e.FullName)
						? (FileSystemInfo) new DirectoryInfo(e.FullName)
						: (FileSystemInfo) new FileInfo(e.FullName));

			return Directory.EnumerateFileSystemEntries(FullPath, searchPattern, true, true, SearchOption.TopDirectoryOnly)
				.Select(e => Directory.Exists(e) ? (FileSystemInfo) new DirectoryInfo(e) : (FileSystemInfo) new FileInfo(e));
		}
#if NET_4_5
		public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string searchPattern, SearchOption searchOption)
		{
			return Directory.EnumerateFileSystemEntries(FullPath, searchPattern, searchOption)
				.Select(e => Directory.Exists(e) ? (FileSystemInfo) new DirectoryInfo(e) : (FileSystemInfo) new FileInfo(e));
		}
#endif
#endif //NET_4_0 || NET_4_5

		private string GetDirName(string fullPath)
		{
			if (fullPath.Length <= 3) return fullPath;
			var s = fullPath;
			if (s.EndsWith(Path.DirectorySeparatorChar))
			{
				s = s.Substring(0, s.Length - 1);
			}
			return Path.GetFileName(s);
		}

		public void MoveTo(string destDirName)
		{
			if (Common.IsRunningOnMono())
			{
				SysDirectoryInfo.MoveTo(destDirName);
				return;
			}

			if (destDirName == null) throw new ArgumentNullException("destDirName");
#if NET_2_0
			if (string.IsNullOrEmpty(destDirName))
#else
			if (string.IsNullOrWhiteSpace(destDirName))
#endif
				throw new ArgumentException("Empty filename", "destDirName");

			string fullDestDirName = Path.GetFullPath(destDirName);
			if (!fullDestDirName.EndsWith(Path.DirectorySeparatorChar))
				fullDestDirName = fullDestDirName + Path.DirectorySeparatorChar;
			String fullSourcePath;
			if (FullPath.EndsWith(Path.DirectorySeparatorChar))
				fullSourcePath = FullPath;
			else
				fullSourcePath = FullPath + Path.DirectorySeparatorChar;

			if (String.Compare(fullSourcePath, fullDestDirName, StringComparison.OrdinalIgnoreCase) == 0)
				throw new IOException("source and destination directories must be different");

			String sourceRoot = Path.GetPathRoot(fullSourcePath);
			String destinationRoot = Path.GetPathRoot(fullDestDirName);

			if (String.Compare(sourceRoot, destinationRoot, StringComparison.OrdinalIgnoreCase) != 0)
				throw new IOException("Source and destination directories must have same root");

			File.Move(fullSourcePath, fullDestDirName);
		}

		public void Create(DirectorySecurity directorySecurity)
		{
			Directory.CreateDirectory(FullPath, directorySecurity);
		}

		public DirectoryInfo CreateSubdirectory(string path, DirectorySecurity directorySecurity)
		{
			var newDir = Path.Combine(FullPath, path);
			var newFullPath = Path.GetFullPath(newDir);
			if (string.Compare(FullPath, 0, newFullPath, 0, FullPath.Length, StringComparison.OrdinalIgnoreCase) != 0)
			{
				throw new ArgumentException("Invalid subpath", path);
			}
			Directory.CreateDirectory(newDir, directorySecurity);
			return new DirectoryInfo(newDir);
		}

#if NET_4_0 || NET_4_5
		public IEnumerable<DirectoryInfo> EnumerateDirectories()
		{
			if (Common.IsRunningOnMono())
			{
				return SysDirectoryInfo.EnumerateDirectories().Select(s => new DirectoryInfo(s.FullName));
			}

			return Directory.EnumerateFileSystemEntries(FullPath, "*", true, false, SearchOption.TopDirectoryOnly)
				.Select(directory => new DirectoryInfo(directory));
		}
#endif

		public DirectorySecurity GetAccessControl()
		{
			return Directory.GetAccessControl(FullPath);
		}

		public DirectorySecurity GetAccessControl(AccessControlSections includeSections)
		{
			return Directory.GetAccessControl(FullPath, includeSections);
		}

		public DirectoryInfo[] GetDirectories()
		{
			return Directory.GetDirectories(FullPath).Select(path => new DirectoryInfo(path)).ToArray();
		}

		public DirectoryInfo[] GetDirectories(string searchPattern)
		{
			return Directory.GetDirectories(FullPath, searchPattern).Select(path => new DirectoryInfo(path)).ToArray();
		}

		public DirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
		{
			return Directory.GetDirectories(FullPath, searchPattern, searchOption).Select(path => new DirectoryInfo(path))
				.ToArray();
		}

		public FileInfo[] GetFiles(string searchPattern)
		{
			return Directory.GetFiles(FullPath, searchPattern).Select(path => new FileInfo(path)).ToArray();
		}

		public FileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
		{
			return Directory.GetFiles(FullPath, searchPattern, searchOption).Select(path => new FileInfo(path)).ToArray();
		}

		public FileInfo[] GetFiles()
		{
			if (Common.IsRunningOnMono())
			{
				var files = SysDirectoryInfo.GetFiles();
				var ret = new FileInfo[files.Length];
				for (var index = 0; index < files.Length; index++)
					ret[index] = new FileInfo(files[index].FullName);

				return ret;
			}
			return Directory.EnumerateFileSystemEntries(FullPath, "*", false, true, SearchOption.TopDirectoryOnly)
				.Select(path => new FileInfo(path)).ToArray();
		}

		public FileSystemInfo[] GetFileSystemInfos(string searchPattern)
		{
			if (Common.IsRunningOnMono())
			{
				var sysInfos = SysDirectoryInfo.GetFileSystemInfos(searchPattern);
				FileSystemInfo[] info = new FileSystemInfo[sysInfos.Length];
				for (var i = 0; i < sysInfos.Length; i++)
				{
					var e = sysInfos[i].FullName;
					info[i] = Directory.Exists(e)
						? (FileSystemInfo) new DirectoryInfo(e)
						: new FileInfo(e);
				}
				return info;
			}

			return Directory.EnumerateFileSystemEntries(FullPath, searchPattern, true, true, SearchOption.TopDirectoryOnly)
				.Select(e => Directory.Exists(e) ? new DirectoryInfo(e) as FileSystemInfo : (FileSystemInfo) new FileInfo(e))
				.ToArray();
		}

		public FileSystemInfo[] GetFileSystemInfos(string searchPattern, SearchOption searchOption)
		{
			if (Common.IsRunningOnMono())
			{
#if NET_4_0 || NET_4_5
				return SysDirectoryInfo.GetFileSystemInfos(searchPattern, searchOption).Select(s => s.FullName)
					.Select(e => Directory.Exists(e) ? (FileSystemInfo) new DirectoryInfo(e) : (FileSystemInfo) new FileInfo(e))
					.ToArray();
#else //throw new NotImplementedException("This function is not supported in ");
				var fileInfos = SysDirectoryInfo.GetFiles(searchPattern);
				var directories = SysDirectoryInfo.GetDirectories(searchPattern);
				List<FileSystemInfo> fileSystemInfos = new List<FileSystemInfo>();
				foreach (System.IO.FileInfo fsi in fileInfos)
					fileSystemInfos.Add(new FileInfo(fsi.FullName));

				foreach (System.IO.DirectoryInfo fsi in directories)
					fileSystemInfos.Add(new DirectoryInfo(fsi.FullName));

				if (searchOption != SearchOption.AllDirectories)
					return fileSystemInfos.ToArray();

				foreach (var di in SysDirectoryInfo.GetDirectories())
					fileSystemInfos.AddRange(new DirectoryInfo(di.FullName).GetFileSystemInfos(searchPattern, searchOption));

				return fileSystemInfos.ToArray();
#endif
			}

			return Directory.EnumerateFileSystemEntries(FullPath, searchPattern, true, true, searchOption)
				.Select(e => Directory.Exists(e) ? (FileSystemInfo) new DirectoryInfo(e) : (FileSystemInfo) new FileInfo(e))
				.ToArray();
		}

		public FileSystemInfo[] GetFileSystemInfos()
		{
			if (Common.IsRunningOnMono())
			{
				var sysInfos = SysDirectoryInfo.GetFileSystemInfos();
				FileSystemInfo[] info = new FileSystemInfo[sysInfos.Length];
				for (var i = 0; i < sysInfos.Length; i++)
				{
					var e = sysInfos[i].FullName;
					info[i] = Directory.Exists(e)
						? (FileSystemInfo) new DirectoryInfo(e)
						: new FileInfo(e);
				}
				return info;
			}

			return Directory.EnumerateFileSystemEntries(FullPath, "*", true, true, SearchOption.TopDirectoryOnly)
				.Select(e => Directory.Exists(e) ? (FileSystemInfo) new DirectoryInfo(e) : (FileSystemInfo) new FileInfo(e))
				.ToArray();
		}

		public void SetAccessControl(DirectorySecurity directorySecurity)
		{
			Directory.SetAccessControl(FullPath, directorySecurity);
		}

		public override string ToString()
		{
			return DisplayPath;
		}
	}

	public static class StringExtensions
	{
		public static bool EndsWith(this string text, char value)
		{
			if (string.IsNullOrEmpty(text)) return false;

			return text[text.Length - 1] == value;
		}
	}
}
#if NET_2_0
namespace System.Runtime.CompilerServices
{
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class
		 | AttributeTargets.Method)]
	public sealed class ExtensionAttribute : Attribute { }
}
#endif