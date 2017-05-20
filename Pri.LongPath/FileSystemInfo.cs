using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Pri.LongPath
{
	using FileAttributes = System.IO.FileAttributes;
	// ReSharper disable once IdentifierTypo
	using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;
	using DirectoryNotFoundException = System.IO.DirectoryNotFoundException;

	public abstract class FileSystemInfo
	{
		protected string OriginalPath;
		protected string FullPath;
		protected State InstanceState;
		protected readonly FileAttributeData Data = new FileAttributeData();
		protected int ErrorCode;

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
		public FileAttributes Attributes
		{
			get
			{
				if (Common.IsRunningOnMono()) return SystemInfo.Attributes;

				return Common.GetAttributes(FullPath);
			}
			set
			{
				if (Common.IsRunningOnMono())
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
		public DateTime CreationTime
		{
			get
			{
				if (Common.IsRunningOnMono()) return SystemInfo.CreationTime;
				return CreationTimeUtc.ToLocalTime();
			}

			set
			{
				if (Common.IsRunningOnMono())
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
		public DateTime CreationTimeUtc
		{
			get
			{
				if (Common.IsRunningOnMono()) return SystemInfo.CreationTimeUtc;

				if (InstanceState == State.Uninitialized)
				{
					Refresh();
				}
				if (InstanceState == State.Error)
					Common.ThrowIoError(ErrorCode, FullPath);

				long fileTime = ((long) Data.ftCreationTime.dwHighDateTime << 32) |
				                (Data.ftCreationTime.dwLowDateTime & 0xffffffff);
				return DateTime.FromFileTimeUtc(fileTime);
			}
			set
			{
				if (Common.IsRunningOnMono())
				{
					SystemInfo.CreationTimeUtc = value;
					return;
				}

				if (this is DirectoryInfo)
					Directory.SetCreationTimeUtc(FullPath, value);
				else
					File.SetCreationTimeUtc(FullPath, value);
				InstanceState = State.Uninitialized;
			}
		}

		public DateTime LastWriteTime
		{
			get
			{
				if (Common.IsRunningOnMono()) return SystemInfo.LastWriteTime;

				return LastWriteTimeUtc.ToLocalTime();
			}
			set
			{
				if (Common.IsRunningOnMono()) SystemInfo.LastWriteTime = value;
				else LastWriteTimeUtc = value.ToUniversalTime();
			}
		}

		private static void ThrowLastWriteTimeUtcIoError(int errorCode, string maybeFullPath)
		{
			// This doesn't have to be perfect, but is a performance optimization.
			bool isInvalidPath = errorCode == NativeMethods.ERROR_INVALID_NAME || errorCode == NativeMethods.ERROR_BAD_PATHNAME;
			string str = isInvalidPath ? Path.GetFileName(maybeFullPath) : maybeFullPath;

			switch (errorCode)
			{
				case NativeMethods.ERROR_FILE_NOT_FOUND:
				case NativeMethods.ERROR_PATH_NOT_FOUND:
					break;

				case NativeMethods.ERROR_ACCESS_DENIED:
					if (str.Length == 0)
						throw new UnauthorizedAccessException("Empty path");
					else
						throw new UnauthorizedAccessException(string.Format("Access denied accessing {0}", str));

				case NativeMethods.ERROR_ALREADY_EXISTS:
					if (str.Length == 0)
						goto default;
					throw new System.IO.IOException(string.Format("File {0}", str), NativeMethods.MakeHRFromErrorCode(errorCode));

				case NativeMethods.ERROR_FILENAME_EXCED_RANGE:
					throw new System.IO.PathTooLongException("Path too long");

				case NativeMethods.ERROR_INVALID_DRIVE:
					throw new System.IO.DriveNotFoundException(string.Format("Drive {0} not found", str));

				case NativeMethods.ERROR_SHARING_VIOLATION:
					if (str.Length == 0)
						throw new System.IO.IOException("Sharing violation with empty filename",
							NativeMethods.MakeHRFromErrorCode(errorCode));
					else
						throw new System.IO.IOException(string.Format("Sharing violation: {0}", str),
							NativeMethods.MakeHRFromErrorCode(errorCode));

				case NativeMethods.ERROR_FILE_EXISTS:
					if (str.Length == 0)
						goto default;
					throw new System.IO.IOException(string.Format("File exists {0}", str),
						NativeMethods.MakeHRFromErrorCode(errorCode));

				case NativeMethods.ERROR_OPERATION_ABORTED:
					throw new OperationCanceledException();

				// ReSharper disable once RedundantCaseLabel
				case NativeMethods.ERROR_INVALID_PARAMETER:
				default:
					throw new System.IO.IOException(NativeMethods.GetMessage(errorCode), NativeMethods.MakeHRFromErrorCode(errorCode));
			}
		}

		public DateTime LastWriteTimeUtc
		{
			get
			{
				if (Common.IsRunningOnMono()) return SystemInfo.LastWriteTimeUtc;


				if (InstanceState == State.Uninitialized)
				{
					Refresh();
				}
				if (InstanceState == State.Error)
					ThrowLastWriteTimeUtcIoError(ErrorCode, FullPath);

				long fileTime = ((long) Data.ftLastWriteTime.dwHighDateTime << 32) |
				                (Data.ftLastWriteTime.dwLowDateTime & 0xffffffff);
				return DateTime.FromFileTimeUtc(fileTime);
			}
			set
			{
				if (Common.IsRunningOnMono())
				{
					SystemInfo.LastWriteTimeUtc = value;
					return;
				}


				if (this is DirectoryInfo)
					Directory.SetLastWriteTimeUtc(FullPath, value);
				else
					File.SetLastWriteTimeUtc(FullPath, value);
				InstanceState = State.Uninitialized;
			}
		}

		public DateTime LastAccessTime
		{
			get
			{
				if (Common.IsRunningOnMono()) return SystemInfo.LastAccessTime;

				return LastAccessTimeUtc.ToLocalTime();
			}
			set
			{
				if (Common.IsRunningOnMono()) SystemInfo.LastAccessTime = value;
				else LastAccessTimeUtc = value.ToUniversalTime();
			}
		}

		public DateTime LastAccessTimeUtc
		{
			get
			{
				if (Common.IsRunningOnMono()) return SystemInfo.LastAccessTimeUtc;

				if (InstanceState == State.Uninitialized)
				{
					Refresh();
				}
				if (InstanceState == State.Error)
					Common.ThrowIoError(ErrorCode, FullPath);

				long fileTime = ((long) Data.ftLastAccessTime.dwHighDateTime << 32) |
				                (Data.ftLastAccessTime.dwLowDateTime & 0xffffffff);
				return DateTime.FromFileTimeUtc(fileTime);
			}
			set
			{
				if (Common.IsRunningOnMono())
				{
					SystemInfo.LastAccessTimeUtc = value;
					return;
				}


				if (this is DirectoryInfo)
					Directory.SetLastAccessTimeUtc(FullPath, value);
				else
					File.SetLastAccessTimeUtc(FullPath, value);
				InstanceState = State.Uninitialized;
			}
		}

		public virtual string FullName
		{
			get { return FullPath; }
		}

		public string Extension
		{
			get { return Path.GetExtension(FullPath); }
		}

		public abstract string Name { get; }
		public abstract bool Exists { get; }
		internal string DisplayPath { get; set; }

		protected enum State
		{
			Uninitialized,
			Initialized,
			Error
		}

		protected class FileAttributeData
		{
			// ReSharper disable InconsistentNaming
			public FileAttributes fileAttributes;

			public FILETIME ftCreationTime;
			public FILETIME ftLastAccessTime;
			public FILETIME ftLastWriteTime;
			public int fileSizeHigh;

			public int fileSizeLow;
			// ReSharper restore InconsistentNaming

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
			info.AddValue("OriginalPath", OriginalPath, typeof(string));
			info.AddValue("FullPath", FullPath, typeof(string));
		}

		public void Refresh()
		{
			try
			{
				NativeMethods.WIN32_FIND_DATA findData;
				// TODO: BeginFind fails on "\\?\c:\"

				string normalizedPathWithSearchPattern = Path.NormalizeLongPath(new DirectoryInfo(FullPath).Parent == null
					? Path.Combine(FullPath, "*")
					: FullPath);

				using (var handle = Directory.BeginFind(normalizedPathWithSearchPattern, out findData))
				{
					var lastWin32Error = Marshal.GetLastWin32Error();
					if (handle == null)
					{
						InstanceState = State.Error;
						ErrorCode = lastWin32Error;
					}
					else
					{
						Data.From(findData);
						InstanceState = State.Initialized;
					}
				}
			}
			catch (DirectoryNotFoundException)
			{
				InstanceState = State.Error;
				ErrorCode = NativeMethods.ERROR_PATH_NOT_FOUND;
			}
			catch (Exception)
			{
				if (InstanceState != State.Error)
					Common.ThrowIoError(Marshal.GetLastWin32Error(), FullPath);
			}
		}

		public abstract void Delete();
	}
}