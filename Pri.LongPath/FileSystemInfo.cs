using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Pri.LongPath
{
	using System.Security.Permissions;
	using FileAttributes = System.IO.FileAttributes;
	using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;
	using DirectoryNotFoundException = System.IO.DirectoryNotFoundException;

	/// <include path='doc/members/member[@name="T:System.IO.FileSystemInfo"]/*' file='..\ref\mscorlib.xml' />
	public abstract class FileSystemInfo
	{
		protected string OriginalPath;
		protected string FullPath;
		protected FileInfo.State state;
		protected readonly FileAttributeData data = new FileAttributeData();
		protected int errorCode;

		/// <include path='doc/members/member[@name="P:System.IO.FileSystemInfo.SystemInfo"]/*' file='..\ref\mscorlib.xml' />
		public abstract System.IO.FileSystemInfo SystemInfo { get; }

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
		/// <include path='doc/members/member[@name="P:System.IO.FileSystemInfo.Attributes"]/*' file='..\ref\mscorlib.xml' />
        public FileAttributes Attributes
		{
			get
			{
			    if (Common.IsRunningOnMono() && Common.IsPlatformUnix()) return SystemInfo.Attributes;

				return Common.GetAttributes(FullPath);
			}
			set
			{
			    if (Common.IsRunningOnMono() && Common.IsPlatformUnix())
			        SystemInfo.Attributes = value;
			    else
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
		/// <include path='doc/members/member[@name="P:System.IO.FileSystemInfo.CreationTime"]/*' file='..\ref\mscorlib.xml' />
		public DateTime CreationTime
		{
			get
			{
			    if(Common.IsRunningOnMono() && Common.IsPlatformUnix()) return SystemInfo.CreationTime;
                return CreationTimeUtc.ToLocalTime();
			}

			set
			{
			    if (Common.IsRunningOnMono() && Common.IsPlatformUnix())
			        SystemInfo.CreationTime = value;
			    else
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
		/// <include path='doc/members/member[@name="P:System.IO.FileSystemInfo.CreationTimeUtc"]/*' file='..\ref\mscorlib.xml' />
		public DateTime CreationTimeUtc
		{
			get
			{
			    if (Common.IsRunningOnMono() && Common.IsPlatformUnix()) return SystemInfo.CreationTimeUtc;

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
			    if (Common.IsRunningOnMono() && Common.IsPlatformUnix())
			    {
			        SystemInfo.CreationTimeUtc = value;
			        return;
			    }

                if (this is DirectoryInfo)
					Directory.SetCreationTimeUtc(FullPath, value);
				else
					File.SetCreationTimeUtc(FullPath, value);
				state = State.Uninitialized;
			}
		}

		/// <include path='doc/members/member[@name="P:System.IO.FileSystemInfo.LastWriteTime"]/*' file='..\ref\mscorlib.xml' />
		public DateTime LastWriteTime
		{
			get
			{
			    if (Common.IsRunningOnMono() && Common.IsPlatformUnix()) return SystemInfo.LastWriteTime;

                return LastWriteTimeUtc.ToLocalTime();
			}
			set
			{
			    if (Common.IsRunningOnMono() && Common.IsPlatformUnix())SystemInfo.LastWriteTime = value;
                else LastWriteTimeUtc = value.ToUniversalTime();
			}
		}

		private static void ThrowLastWriteTimeUtcIOError(int errorCode, String maybeFullPath)
		{
			// This doesn't have to be perfect, but is a perf optimization.
			bool isInvalidPath = errorCode == NativeMethods.ERROR_INVALID_NAME || errorCode == NativeMethods.ERROR_BAD_PATHNAME;
			String str = isInvalidPath ? Path.GetFileName(maybeFullPath) : maybeFullPath;

			switch (errorCode)
			{
				case NativeMethods.ERROR_FILE_NOT_FOUND:
					break;

				case NativeMethods.ERROR_PATH_NOT_FOUND:
					break;

				case NativeMethods.ERROR_ACCESS_DENIED:
					if (str.Length == 0)
						throw new UnauthorizedAccessException("Empty path");
					else
						throw new UnauthorizedAccessException(String.Format("Access denied accessing {0}", str));

				case NativeMethods.ERROR_ALREADY_EXISTS:
					if (str.Length == 0)
						goto default;
					throw new System.IO.IOException(String.Format("File {0}", str), NativeMethods.MakeHRFromErrorCode(errorCode));

				case NativeMethods.ERROR_FILENAME_EXCED_RANGE:
					throw new System.IO.PathTooLongException("Path too long");

				case NativeMethods.ERROR_INVALID_DRIVE:
					throw new System.IO.DriveNotFoundException(String.Format("Drive {0} not found", str));

				case NativeMethods.ERROR_INVALID_PARAMETER:
					throw new System.IO.IOException(NativeMethods.GetMessage(errorCode), NativeMethods.MakeHRFromErrorCode(errorCode));

				case NativeMethods.ERROR_SHARING_VIOLATION:
					if (str.Length == 0)
						throw new System.IO.IOException("Sharing violation with empty filename", NativeMethods.MakeHRFromErrorCode(errorCode));
					else
						throw new System.IO.IOException(String.Format("Sharing violation: {0}", str), NativeMethods.MakeHRFromErrorCode(errorCode));

				case NativeMethods.ERROR_FILE_EXISTS:
					if (str.Length == 0)
						goto default;
					throw new System.IO.IOException(String.Format("File exists {0}", str), NativeMethods.MakeHRFromErrorCode(errorCode));

				case NativeMethods.ERROR_OPERATION_ABORTED:
					throw new OperationCanceledException();

				default:
					throw new System.IO.IOException(NativeMethods.GetMessage(errorCode), NativeMethods.MakeHRFromErrorCode(errorCode));
			}
		}
		public DateTime LastWriteTimeUtc
		{
			get
			{
			    if (Common.IsRunningOnMono() && Common.IsPlatformUnix()) return SystemInfo.LastWriteTimeUtc;


                if (state == State.Uninitialized)
				{
					Refresh();
				}
				if (state == State.Error)
					ThrowLastWriteTimeUtcIOError(errorCode, FullPath);

				long fileTime = ((long)data.ftLastWriteTime.dwHighDateTime << 32) | (data.ftLastWriteTime.dwLowDateTime & 0xffffffff);
				return DateTime.FromFileTimeUtc(fileTime);
			}
			set
			{
			    if (Common.IsRunningOnMono() && Common.IsPlatformUnix())
			    {
			        SystemInfo.LastWriteTimeUtc = value;
			        return;
			    }


                if (this is DirectoryInfo)
					Directory.SetLastWriteTimeUtc(FullPath, value);
				else
					File.SetLastWriteTimeUtc(FullPath, value);
				state = State.Uninitialized;
			}
		}

		/// <include path='doc/members/member[@name="P:System.IO.FileSystemInfo.LastAccessTime"]/*' file='..\ref\mscorlib.xml' />
		public DateTime LastAccessTime
		{
			get
			{
			    if (Common.IsRunningOnMono() && Common.IsPlatformUnix()) return SystemInfo.LastAccessTime;

                return LastAccessTimeUtc.ToLocalTime();
			}
			set
			{
			    if (Common.IsRunningOnMono() && Common.IsPlatformUnix())SystemInfo.LastAccessTime = value;
			    else LastAccessTimeUtc = value.ToUniversalTime();
			}
		}

		/// <include path='doc/members/member[@name="P:System.IO.FileSystemInfo.LastAccessTimeUtc"]/*' file='..\ref\mscorlib.xml' />
		public DateTime LastAccessTimeUtc
		{
			get
			{
			    if (Common.IsRunningOnMono() && Common.IsPlatformUnix()) return SystemInfo.LastAccessTimeUtc;

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
			    if (Common.IsRunningOnMono() && Common.IsPlatformUnix())
			    {
			        SystemInfo.LastAccessTimeUtc = value;
			        return;
			    }


                if (this is DirectoryInfo)
					Directory.SetLastAccessTimeUtc(FullPath, value);
				else
					File.SetLastAccessTimeUtc(FullPath, value);
				state = State.Uninitialized;
			}
		}

		/// <include path='doc/members/member[@name="P:System.IO.FileSystemInfo.FullName"]/*' file='..\ref\mscorlib.xml' />
		public virtual string FullName
		{
			get { return FullPath; }
		}

		/// <include path='doc/members/member[@name="P:System.IO.FileSystemInfo.Extension"]/*' file='..\ref\mscorlib.xml' />
		public string Extension
		{
			get
			{
				return Path.GetExtension(FullPath);
			}
		}

		/// <include path='doc/members/member[@name="P:System.IO.FileSystemInfo.Name"]/*' file='..\ref\mscorlib.xml' />
		public abstract string Name { get; }
		/// <include path='doc/members/member[@name="P:System.IO.FileSystemInfo.Exists"]/*' file='..\ref\mscorlib.xml' />
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

		/// <include path='doc/members/member[@name="M:System.IO.FileSystemInfo.GetObjectData(System.Runtime.Serialization.SerializationInfo,System.Runtime.Serialization.StreamingContext)"]/*' file='..\ref\mscorlib.xml' />
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//(new FileIOPermission(FileIOPermissionAccess.PathDiscovery, this.FullPath)).Demand();
			info.AddValue("OriginalPath", this.OriginalPath, typeof(string));
			info.AddValue("FullPath", this.FullPath, typeof(string));
		}

        internal virtual string GetNormalizedPathWithSearchPattern()
        {
            // https://docs.microsoft.com/en-us/windows/desktop/api/fileapi/nf-fileapi-findfirstfilew
            // "If the string ends with a wildcard, period (.), or directory name, the user must have access permissions to the root and all subdirectories on the path"
            // This is a problem if the executing principal has no access to the parent folder;
            // appending "\*" fixes this while still allowing retrieval of attributes
            if (this is DirectoryInfo)
            {
                return Path.NormalizeLongPath(Path.Combine(FullPath, "*"));
            }

            return Path.NormalizeLongPath(FullPath);
        }


		/// <include path='doc/members/member[@name="M:System.IO.FileSystemInfo.Refresh"]/*' file='..\ref\mscorlib.xml' />
        public void Refresh()
		{
			try
			{
                NativeMethods.WIN32_FIND_DATA findData;

                // TODO: BeginFind fails on "\\?\c:\"
                using (var handle = Directory.BeginFind(GetNormalizedPathWithSearchPattern(), out findData))
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
					Common.ThrowIOError(Marshal.GetLastWin32Error(), FullPath);
			}
		}

		/// <include path='doc/members/member[@name="M:System.IO.FileSystemInfo.Delete"]/*' file='..\ref\mscorlib.xml' />
		public abstract void Delete();
	}
}