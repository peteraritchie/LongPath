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

	/// <include path='doc/members/member[@name="T:System.IO.DirectoryInfo"]/*' file='..\ref\mscorlib.xml' />
	public class DirectoryInfo : FileSystemInfo
	{
		private readonly string _name;

		/// <include path='doc/members/member[@name="P:System.IO.FileSystemInfo.SystemInfo"]/*' file='..\ref\mscorlib.xml' />
		public override System.IO.FileSystemInfo SystemInfo { get { return SysDirectoryInfo; } }

        private System.IO.DirectoryInfo SysDirectoryInfo
	    {
	        get
	        {
	            return new System.IO.DirectoryInfo(FullPath);
	        }
	    }

		/// <include path='doc/members/member[@name="P:System.IO.DirectoryInfo.Exists"]/*' file='..\ref\mscorlib.xml' />
		public override bool Exists
		{
			get
			{
				try
				{
					if (state == State.Uninitialized)
					{
						Refresh();
					}
					return state == State.Initialized &&
						   (data.fileAttributes & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory;
				}
				catch
				{
					return false;
				}
			}
		}

		/// <include path='doc/members/member[@name="P:System.IO.DirectoryInfo.Name"]/*' file='..\ref\mscorlib.xml' />
		public override string Name
		{
			get { return _name; }
		}

		/// <include path='doc/members/member[@name="P:System.IO.DirectoryInfo.Parent"]/*' file='..\ref\mscorlib.xml' />
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

		/// <include path='doc/members/member[@name="P:System.IO.DirectoryInfo.Root"]/*' file='..\ref\mscorlib.xml' />
		public DirectoryInfo Root
		{
			get
			{
				int rootLength = Path.GetRootLength(this.FullPath);
				string str = this.FullPath.Substring(0, rootLength - (Common.IsPathUnc(FullPath) ? 1 : 0));
				return new DirectoryInfo(str);
			}
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.#ctor(System.String)"]/*' file='..\ref\mscorlib.xml' />
		public DirectoryInfo(string path)
		{
			if (path == null) throw new ArgumentNullException("path");
			OriginalPath = path;
			FullPath = Path.GetFullPath(path);
			_name = (OriginalPath.Length != 2 || OriginalPath[1] != ':' ? GetDirName(FullPath) : ".");
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.Create"]/*' file='..\ref\mscorlib.xml' />
		public void Create()
		{
			Directory.CreateDirectory(FullPath);
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.CreateSubdirectory(System.String)"]/*' file='..\ref\mscorlib.xml' />
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

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.Delete"]/*' file='..\ref\mscorlib.xml' />
		public override void Delete()
		{
			Directory.Delete(this.FullPath);
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.Delete(System.Boolean)"]/*' file='..\ref\mscorlib.xml' />
		public void Delete(bool recursive)
		{
			Directory.Delete(FullPath, recursive);
		}

#if NET_4_0 || NET_4_5
		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.EnumerateDirectories(System.String)"]/*' file='..\ref\mscorlib.xml' />
		public IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern)
		{
		    if (Common.IsRunningOnMono() && Common.IsPlatformUnix()) return SysDirectoryInfo.EnumerateDirectories(searchPattern).Select(s => new DirectoryInfo(s.FullName));

            return Directory.EnumerateFileSystemEntries(FullPath, searchPattern, true, false, System.IO.SearchOption.TopDirectoryOnly)
				.Select(directory => new DirectoryInfo(directory));
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.EnumerateDirectories(System.String,System.IO.SearchOption)"]/*' file='..\ref\mscorlib.xml' />
		public IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption)
		{
		    if (Common.IsRunningOnMono() && Common.IsPlatformUnix()) return SysDirectoryInfo.EnumerateDirectories(searchPattern, searchOption).Select(s => new DirectoryInfo(s.FullName));

            return Directory.EnumerateFileSystemEntries(FullPath, searchPattern, true, false, searchOption)
				.Select(directory => new DirectoryInfo(directory));
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.EnumerateFiles"]/*' file='..\ref\mscorlib.xml' />
		public IEnumerable<FileInfo> EnumerateFiles()
		{
			return Directory.EnumerateFiles(FullPath).Select(e => new FileInfo(e));
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.EnumerateFiles(System.String)"]/*' file='..\ref\mscorlib.xml' />
		public IEnumerable<FileInfo> EnumerateFiles(string searchPattern)
		{
		    if (Common.IsRunningOnMono() && Common.IsPlatformUnix()) return SysDirectoryInfo.EnumerateFiles(searchPattern).Select(s => new FileInfo(s.FullName));

            return Directory.EnumerateFileSystemEntries(FullPath, searchPattern, false, true, System.IO.SearchOption.TopDirectoryOnly).Select(e => new FileInfo(e));
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.EnumerateFiles(System.String,System.IO.SearchOption)"]/*' file='..\ref\mscorlib.xml' />
		public IEnumerable<FileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
		{
		    if (Common.IsRunningOnMono() && Common.IsPlatformUnix()) return SysDirectoryInfo.EnumerateFiles(searchPattern, searchOption).Select(s => new FileInfo(s.FullName));

            return Directory.EnumerateFileSystemEntries(FullPath, searchPattern, false, true, searchOption).Select(e => new FileInfo(e));
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.EnumerateFileSystemInfos"]/*' file='..\ref\mscorlib.xml' />
		public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos()
		{
            return
				Directory.EnumerateFileSystemEntries(FullPath)
					.Select(e => Directory.Exists(e) ? (FileSystemInfo)new DirectoryInfo(e) : (FileSystemInfo)new FileInfo(e));
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.EnumerateFileSystemInfos(System.String)"]/*' file='..\ref\mscorlib.xml' />
		public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string searchPattern)
		{
		    if (Common.IsRunningOnMono() && Common.IsPlatformUnix()) return SysDirectoryInfo.EnumerateFileSystemInfos(searchPattern)
                    .Select(e => System.IO.Directory.Exists(e.FullName) ? (FileSystemInfo)new DirectoryInfo(e.FullName) : (FileSystemInfo)new FileInfo(e.FullName));

            return Directory.EnumerateFileSystemEntries(FullPath, searchPattern, true, true, System.IO.SearchOption.TopDirectoryOnly)
					.Select(e => Directory.Exists(e) ? (FileSystemInfo)new DirectoryInfo(e) : (FileSystemInfo)new FileInfo(e));
		}
#if NET_4_5
		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.EnumerateFileSystemInfos(System.String,System.IO.SearchOption)"]/*' file='..\ref\mscorlib.xml' />
		public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string searchPattern, SearchOption searchOption)
		{
			return Directory.EnumerateFileSystemEntries(FullPath, searchPattern, searchOption)
					.Select(e => Directory.Exists(e) ? (FileSystemInfo)new DirectoryInfo(e) : (FileSystemInfo)new FileInfo(e));
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

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.MoveTo(System.String)"]/*' file='..\ref\mscorlib.xml' />
		public void MoveTo(string destDirName)
		{
		    if (Common.IsRunningOnMono() && Common.IsPlatformUnix())
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

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.Create(System.Security.AccessControl.DirectorySecurity)"]/*' file='..\ref\mscorlib.xml' />
		public void Create(DirectorySecurity directorySecurity)
		{
			Directory.CreateDirectory(FullPath, directorySecurity);
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.CreateSubdirectory(System.String,System.Security.AccessControl.DirectorySecurity)"]/*' file='..\ref\mscorlib.xml' />
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
		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.EnumerateDirectories"]/*' file='..\ref\mscorlib.xml' />
		public IEnumerable<DirectoryInfo> EnumerateDirectories()
		{
		    if (Common.IsRunningOnMono() && Common.IsPlatformUnix())
            {
                return SysDirectoryInfo.EnumerateDirectories().Select(s => new DirectoryInfo(s.FullName));
            }

            return Directory.EnumerateFileSystemEntries(FullPath, "*", true, false, System.IO.SearchOption.TopDirectoryOnly).Select(directory => new DirectoryInfo(directory));
		}
#endif

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.GetAccessControl"]/*' file='..\ref\mscorlib.xml' />
		public DirectorySecurity GetAccessControl()
		{
			return Directory.GetAccessControl(FullPath);
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.GetAccessControl(System.Security.AccessControl.AccessControlSections)"]/*' file='..\ref\mscorlib.xml' />
		public DirectorySecurity GetAccessControl(AccessControlSections includeSections)
		{
			return Directory.GetAccessControl(FullPath, includeSections);
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.GetDirectories"]/*' file='..\ref\mscorlib.xml' />
		public DirectoryInfo[] GetDirectories()
		{
			return Directory.GetDirectories(FullPath).Select(path => new DirectoryInfo(path)).ToArray();
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.GetDirectories(System.String)"]/*' file='..\ref\mscorlib.xml' />
		public DirectoryInfo[] GetDirectories(string searchPattern)
		{
			return Directory.GetDirectories(FullPath, searchPattern).Select(path => new DirectoryInfo(path)).ToArray();
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.GetDirectories(System.String,System.IO.SearchOption)"]/*' file='..\ref\mscorlib.xml' />
		public DirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
		{
			return Directory.GetDirectories(FullPath, searchPattern, searchOption).Select(path => new DirectoryInfo(path)).ToArray();
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.GetFiles(System.String)"]/*' file='..\ref\mscorlib.xml' />
		public FileInfo[] GetFiles(string searchPattern)
		{
			return Directory.GetFiles(FullPath, searchPattern).Select(path => new FileInfo(path)).ToArray();
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.GetFiles(System.String,System.IO.SearchOption)"]/*' file='..\ref\mscorlib.xml' />
		public FileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
		{
			return Directory.GetFiles(FullPath, searchPattern, searchOption).Select(path => new FileInfo(path)).ToArray();
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.GetFiles"]/*' file='..\ref\mscorlib.xml' />
		public FileInfo[] GetFiles()
		{
		    if (Common.IsRunningOnMono() && Common.IsPlatformUnix())
		    {
		        var files = SysDirectoryInfo.GetFiles();
                var ret = new FileInfo[files.Length];
		        for (var index = 0; index < files.Length; index++)
		            ret[index] = new FileInfo(files[index].FullName);

		        return ret;
		    }
			return Directory.EnumerateFileSystemEntries(FullPath, "*", false, true, System.IO.SearchOption.TopDirectoryOnly).Select(path => new FileInfo(path)).ToArray();
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.GetFileSystemInfos(System.String)"]/*' file='..\ref\mscorlib.xml' />
		public FileSystemInfo[] GetFileSystemInfos(string searchPattern)
		{
		    if (Common.IsRunningOnMono() && Common.IsPlatformUnix())
		    {
		        var sysInfos = SysDirectoryInfo.GetFileSystemInfos(searchPattern);
                FileSystemInfo[] fsis = new FileSystemInfo[sysInfos.Length];
                for (var i = 0; i < sysInfos.Length; i++)
                {
                    var e = sysInfos[i].FullName;
                    fsis[i] = Directory.Exists(e)
                        ? (FileSystemInfo) new DirectoryInfo(e)
                        : (FileSystemInfo) new FileInfo(e);
                }
		        return fsis;
		    }
     
            return Directory.EnumerateFileSystemEntries(FullPath, searchPattern, true, true, System.IO.SearchOption.TopDirectoryOnly)
					.Select(e => Directory.Exists(e) ? (FileSystemInfo)new DirectoryInfo(e) : (FileSystemInfo)new FileInfo(e)).ToArray();
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.GetFileSystemInfos(System.String,System.IO.SearchOption)"]/*' file='..\ref\mscorlib.xml' />
        public FileSystemInfo[] GetFileSystemInfos(string searchPattern, SearchOption searchOption)
		{
            if (Common.IsRunningOnMono() && Common.IsPlatformUnix())
            {
#if NET_4_0 || NET_4_5
                return SysDirectoryInfo.GetFileSystemInfos(searchPattern, searchOption).Select(s => s.FullName).Select(e => Directory.Exists(e) ? (FileSystemInfo)new DirectoryInfo(e) : (FileSystemInfo)new FileInfo(e)).ToArray();
#else 
                //throw new NotImplementedException("This function is not supported in ");
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
					.Select(e => Directory.Exists(e) ? (FileSystemInfo)new DirectoryInfo(e) : (FileSystemInfo)new FileInfo(e)).ToArray();
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.GetFileSystemInfos"]/*' file='..\ref\mscorlib.xml' />
		public FileSystemInfo[] GetFileSystemInfos()
		{
		    if (Common.IsRunningOnMono() && Common.IsPlatformUnix())
		    {
		        if (Common.IsRunningOnMono() && Common.IsPlatformUnix())
		        {
		            var sysInfos = SysDirectoryInfo.GetFileSystemInfos();
		            FileSystemInfo[] fsis = new FileSystemInfo[sysInfos.Length];
		            for (var i = 0; i < sysInfos.Length; i++)
		            {
		                var e = sysInfos[i].FullName;
		                fsis[i] = Directory.Exists(e)
		                    ? (FileSystemInfo)new DirectoryInfo(e)
		                    : (FileSystemInfo)new FileInfo(e);
		            }
		            return fsis;
		        }
            }

			return Directory.EnumerateFileSystemEntries(FullPath, "*", true, true, System.IO.SearchOption.TopDirectoryOnly)
					.Select(e => Directory.Exists(e) ? (FileSystemInfo)new DirectoryInfo(e) : (FileSystemInfo)new FileInfo(e)).ToArray();
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.SetAccessControl(System.Security.AccessControl.DirectorySecurity)"]/*' file='..\ref\mscorlib.xml' />
		public void SetAccessControl(DirectorySecurity directorySecurity)
		{
			Directory.SetAccessControl(FullPath, directorySecurity);
		}

		/// <include path='doc/members/member[@name="M:System.IO.DirectoryInfo.ToString"]/*' file='..\ref\mscorlib.xml' />
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
