using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;

namespace Pri.LongPath
{
	using SearchOption = System.IO.SearchOption;
	using IOException = System.IO.IOException;

	public class DirectoryInfo : FileSystemInfo
	{
		private readonly string name;

		public override bool Exists
		{
			get
			{
				if (state == State.Uninitialized)
				{
					Refresh();
				}
				return state == State.Initialized &&
					   (data.fileAttributes & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory;
			}
		}

		public override string Name { get { return name; } }

		public DirectoryInfo Parent
		{
			get
			{
				string fullPath = this.FullPath;
				if (fullPath.Length > 3 && fullPath.EndsWith(Path.DirectorySeparatorChar))
				{
					fullPath = this.FullPath.Substring(0, this.FullPath.Length - 1);
				}
				string directoryName = Path.GetDirectoryName(fullPath);
				return directoryName == null ? null : new DirectoryInfo(directoryName);
			}
		}

		public DirectoryInfo Root
		{
			get
			{
				int rootLength = Path.GetRootLength(this.FullPath);
				string str = this.FullPath.Substring(0, rootLength);
				return new DirectoryInfo(str);
			}
		}

		public DirectoryInfo(string path)
		{
			if (path == null) throw new ArgumentNullException("path");
			OriginalPath = path;
			FullPath = Path.GetFullPath(path);
			name = (OriginalPath.Length != 2 || OriginalPath[1] != ':' ? GetDirName(FullPath) : ".");
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
			Directory.Delete(this.FullPath);
		}

		public void Delete(bool recursive)
		{
			Directory.Delete(FullPath, recursive);
		}

		public IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern)
		{
			return Directory.EnumerateDirectories(FullPath, searchPattern).Select(directory => new DirectoryInfo(directory));
		}

		public IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption)
		{
			return Directory.EnumerateDirectories(FullPath, searchPattern, searchOption)
				.Select(directory => new DirectoryInfo(directory));
		}

		public IEnumerable<FileInfo> EnumerateFiles()
		{
			return Directory.EnumerateFiles(FullPath).Select(e => new FileInfo(e));
		}

		public IEnumerable<FileInfo> EnumerateFiles(string searchPattern)
		{
			return Directory.EnumerateFiles(FullPath, searchPattern).Select(e => new FileInfo(e));
		}

		public IEnumerable<FileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
		{
			return Directory.EnumerateFiles(FullPath, searchPattern, searchOption).Select(e => new FileInfo(e));
		}

		public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos()
		{
			return
				Directory.EnumerateFileSystemEntries(FullPath)
					.Select(e => Directory.Exists(e) ? (FileSystemInfo)new DirectoryInfo(e) : (FileSystemInfo)new FileInfo(e));
		}

		public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string searchPattern)
		{
			return
				Directory.EnumerateFileSystemEntries(FullPath, searchPattern)
					.Select(e => Directory.Exists(e) ? (FileSystemInfo)new DirectoryInfo(e) : (FileSystemInfo)new FileInfo(e));
		}

		public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string searchPattern, SearchOption searchOption)
		{
			return Directory.EnumerateFileSystemEntries(FullPath, searchPattern, searchOption)
					.Select(e => Directory.Exists(e) ? (FileSystemInfo)new DirectoryInfo(e) : (FileSystemInfo)new FileInfo(e));
		}

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
			if (destDirName == null) throw new ArgumentNullException("destDirName");
			if (string.IsNullOrWhiteSpace(destDirName)) throw new ArgumentException("Empty filename", "destDirName");

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

		public IEnumerable<DirectoryInfo> EnumerateDirectories()
		{
			return Directory.EnumerateDirectories(FullPath).Select(directory => new DirectoryInfo(directory));
		}

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
			return Directory.EnumerateDirectories(FullPath).Select(directory => new DirectoryInfo(directory)).ToArray();
		}

		public DirectoryInfo[] GetDirectories(string searchPattern)
		{
			return Directory.EnumerateDirectories(FullPath, searchPattern).Select(directory => new DirectoryInfo(directory)).ToArray();
		}

		public DirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
		{
			return Directory.EnumerateDirectories(FullPath, searchPattern, searchOption).Select(directory => new DirectoryInfo(directory)).ToArray();
		}

		public FileInfo[] GetFiles(string searchPattern)
		{
			return Directory.EnumerateFiles(FullPath, searchPattern).Select(path => new FileInfo(path)).ToArray();
		}

		public FileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
		{
			return Directory.EnumerateFiles(FullPath, searchPattern, searchOption).Select(path => new FileInfo(path)).ToArray();
			throw new NotImplementedException();
		}

		public FileInfo[] GetFiles()
		{
			return Directory.EnumerateFiles(FullPath).Select(path => new FileInfo(path)).ToArray();
		}

		public FileSystemInfo[] GetFileSystemInfos(string searchPattern)
		{
			return EnumerateFileSystemInfos(searchPattern).ToArray();
		}

		public FileSystemInfo[] GetFileSystemInfos(string searchPattern, SearchOption searchOption)
		{
			return EnumerateFileSystemInfos(searchPattern, searchOption).ToArray();
		}

		public FileSystemInfo[] GetFileSystemInfos()
		{
			return EnumerateFileSystemInfos().ToArray();
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