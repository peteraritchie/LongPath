using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Pri.LongPath
{
	using System.Security.Permissions;
	using FileAttributes = System.IO.FileAttributes;
	using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;
	using DirectoryNotFoundException = System.IO.DirectoryNotFoundException;

	public abstract class FileSystemInfo
	{
		protected string OriginalPath;
		protected string FullPath;
		protected FileInfo.State state;
		protected readonly FileAttributeData data = new FileAttributeData();
		protected int errorCode;

		// Summary:
		//     Gets or sets the attributes for the current file or directory.
		//
		// Returns:
		//     System.IO.FileAttributes of the current System.IO.FileSystemInfo.
		//
		// Exceptions:
		//   System.IO.FileNotFoundException:
		//     The specified file does not exist.
		//
		//   System.IO.DirectoryNotFoundException:
		//     The specified path is invalid; for example, it is on an unmapped drive.
		//
		//   System.Security.SecurityException:
		//     The caller does not have the required permission.
		//
		//   System.ArgumentException:
		//     The caller attempts to set an invalid file attribute. -or-The user attempts
		//     to set an attribute value but does not have write permission.
		//
		//   System.IO.IOException:
		//     System.IO.FileSystemInfo.Refresh() cannot initialize the data.
		public FileAttributes Attributes
		{
			get
			{
				return Common.GetAttributes(FullPath);
			}
			set
			{
				Common.SetAttributes(FullPath, value);
			}
		}

		//
		// Summary:
		//     Gets or sets the creation time of the current file or directory.
		//
		// Returns:
		//     The creation date and time of the current System.IO.FileSystemInfo object.
		//
		// Exceptions:
		//   System.IO.IOException:
		//     System.IO.FileSystemInfo.Refresh() cannot initialize the data.
		//
		//   System.IO.DirectoryNotFoundException:
		//     The specified path is invalid; for example, it is on an unmapped drive.
		//
		//   System.PlatformNotSupportedException:
		//     The current operating system is not Windows NT or later.
		//
		//   System.ArgumentOutOfRangeException:
		//     The caller attempts to set an invalid creation time.
		public DateTime CreationTime
		{
			get
			{
				return CreationTimeUtc.ToLocalTime();
			}

			set
			{
				CreationTimeUtc = value.ToUniversalTime();
			}
		}

		//
		// Summary:
		//     Gets or sets the creation time, in coordinated universal time (UTC), of the
		//     current file or directory.
		//
		// Returns:
		//     The creation date and time in UTC format of the current System.IO.FileSystemInfo
		//     object.
		//
		// Exceptions:
		//   System.IO.IOException:
		//     System.IO.FileSystemInfo.Refresh() cannot initialize the data.
		//
		//   System.IO.DirectoryNotFoundException:
		//     The specified path is invalid; for example, it is on an unmapped drive.
		//
		//   System.PlatformNotSupportedException:
		//     The current operating system is not Windows NT or later.
		//
		//   System.ArgumentOutOfRangeException:
		//     The caller attempts to set an invalid access time.
		public DateTime CreationTimeUtc
		{
			get
			{
				if (state == State.Uninitialized)
				{
					Refresh();
				}
				if (state == State.Error)
					Common.ThrowIOError(errorCode, FullPath);

				long fileTime = ((long)data.ftCreationTime.dwHighDateTime << 32) | (data.ftCreationTime.dwLowDateTime & 0xffffffff);
				return DateTime.FromFileTimeUtc(fileTime);
			}
			set
			{
				if (this is DirectoryInfo)
					Directory.SetCreationTimeUtc(FullPath, value);
				else
					File.SetCreationTimeUtc(FullPath, value);
				state = State.Uninitialized;
			}
		}

		public DateTime LastWriteTime
		{
			get
			{
				return LastWriteTimeUtc.ToLocalTime();
			}
			set
			{
				LastWriteTimeUtc = value.ToUniversalTime();
			}
		}

		public DateTime LastWriteTimeUtc
		{
			get
			{
				if (state == State.Uninitialized)
				{
					Refresh();
				}
				if (state == State.Error)
					Common.ThrowIOError(errorCode, FullPath);

				long fileTime = ((long)data.ftLastWriteTime.dwHighDateTime << 32) | (data.ftLastWriteTime.dwLowDateTime & 0xffffffff);
				return DateTime.FromFileTimeUtc(fileTime);
			}
			set
			{
				if (this is DirectoryInfo)
					Directory.SetLastWriteTimeUtc(FullPath, value);
				else
					File.SetLastWriteTimeUtc(FullPath, value);
				state = State.Uninitialized;
			}
		}

		public DateTime LastAccessTime
		{
			get
			{
				return LastAccessTimeUtc.ToLocalTime();
			}
			set
			{
				LastAccessTimeUtc = value.ToUniversalTime();
			}
		}

		public DateTime LastAccessTimeUtc
		{
			get
			{
				if (state == State.Uninitialized)
				{
					Refresh();
				}
				if (state == State.Error)
					Common.ThrowIOError(errorCode, FullPath);

				long fileTime = ((long)data.ftLastAccessTime.dwHighDateTime << 32) | (data.ftLastAccessTime.dwLowDateTime & 0xffffffff);
				return DateTime.FromFileTimeUtc(fileTime);
			}
			set
			{
				if (this is DirectoryInfo)
					Directory.SetLastAccessTimeUtc(FullPath, value);
				else
					File.SetLastAccessTimeUtc(FullPath, value);
				state = State.Uninitialized;
			}
		}

		public virtual string FullName
		{
			get { return FullPath; }
		}

		public string Extension
		{
			get
			{
				return Path.GetExtension(FullPath);
			}
		}

		public abstract string Name { get; }
		public abstract bool Exists { get; }
		internal string DisplayPath { get; set; }

		protected enum State
		{
			Uninitialized, Initialized, Error
		}

		protected class FileAttributeData
		{
			public System.IO.FileAttributes fileAttributes;
			public FILETIME ftCreationTime;
			public FILETIME ftLastAccessTime;
			public FILETIME ftLastWriteTime;
			public int fileSizeHigh;
			public int fileSizeLow;

			internal void From(NativeMethods.WIN32_FIND_DATA findData)
			{
				fileAttributes = findData.dwFileAttributes;
				ftCreationTime = findData.ftCreationTime;
				ftLastAccessTime = findData.ftLastAccessTime;
				ftLastWriteTime = findData.ftLastWriteTime;
				fileSizeHigh = findData.nFileSizeHigh;
				fileSizeLow = findData.nFileSizeLow;
			}
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//(new FileIOPermission(FileIOPermissionAccess.PathDiscovery, this.FullPath)).Demand();
			info.AddValue("OriginalPath", this.OriginalPath, typeof(string));
			info.AddValue("FullPath", this.FullPath, typeof(string));
		}

		public void Refresh()
		{
			try
			{
				NativeMethods.WIN32_FIND_DATA findData;
				using (var handle = Directory.BeginFind(Path.NormalizeLongPath(FullPath), out findData))
				{
					if (handle == null)
					{
						state = State.Error;
						errorCode = Marshal.GetLastWin32Error();
					}
					else
					{
						data.From(findData);
						state = State.Initialized;
					}
				}
			}
			catch (DirectoryNotFoundException)
			{
				state = State.Error;
				errorCode = NativeMethods.ERROR_PATH_NOT_FOUND;
			}
			catch (Exception)
			{
				if (state != State.Error)
					Common.ThrowIOError(Marshal.GetLastWin32Error(), string.Empty);
			}
		}

		public abstract void Delete();
	}
}