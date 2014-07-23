using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using System.Runtime.CompilerServices;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Tests")]
namespace Pri.LongPath
{
	using PathTooLongException = System.IO.PathTooLongException;
	using FileNotFoundException = System.IO.FileNotFoundException;
	using DirectoryNotFoundException = System.IO.DirectoryNotFoundException;
	using IOException = System.IO.IOException;
	using FileAccess = System.IO.FileAccess;
	using FileMode = System.IO.FileMode;
	using FileStream = System.IO.FileStream;
	using StreamWriter = System.IO.StreamWriter;
	using StreamReader = System.IO.StreamReader;

	public static class File
	{
		public static StreamReader OpenText(string path)
		{
			var stream = Open(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, FileOptions.SequentialScan);
			return new StreamReader(stream, Encoding.UTF8, true, 1024, false);
		}

		private static StreamReader OpenText(string path, Encoding encoding)
		{
			var stream = Open(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, FileOptions.SequentialScan);
			return new StreamReader(stream, encoding, true, 1024, false);
		}

		public static StreamWriter CreateText(String path)
		{
			var fileStream = Open(path, FileMode.Create, FileAccess.Write, FileShare.Read, DefaultBufferSize, FileOptions.SequentialScan);
			return new StreamWriter(fileStream, UTF8NoBOM, DefaultBufferSize, false);
		}

		public static StreamWriter AppendText(String path)
		{
			return CreateStreamWriter(path, true);
		}

		public static void Copy(string sourceFileName, string destFileName)
		{
			Copy(sourceFileName, destFileName, false);
		}

		/// <summary>
		///     Copies the specified file to a specified new file, indicating whether to overwrite an existing file.
		/// </summary>
		/// <param name="sourcePath">
		///     A <see cref="String"/> containing the path of the file to copy.
		/// </param>
		/// <param name="destinationPath">
		///     A <see cref="String"/> containing the new path of the file.
		/// </param>
		/// <param name="overwrite">
		///     <see langword="true"/> if <paramref name="destinationPath"/> should be overwritten
		///     if it refers to an existing file, otherwise, <see langword="false"/>.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/> is
		///     <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/> is
		///     an empty string (""), contains only white space, or contains one or more
		///     invalid characters as defined in <see cref="Path.GetInvalidPathChars()"/>.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/>
		///     contains one or more components that exceed the drive-defined maximum length.
		///     For example, on Windows-based platforms, components must not exceed 255 characters.
		/// </exception>
		/// <exception cref="PathTooLongException">
		///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/>
		///     exceeds the system-defined maximum length. For example, on Windows-based platforms,
		///     paths must not exceed 32,000 characters.
		/// </exception>
		/// <exception cref="FileNotFoundException">
		///     <paramref name="sourcePath"/> could not be found.
		/// </exception>
		/// <exception cref="DirectoryNotFoundException">
		///     One or more directories in <paramref name="sourcePath"/> and/or
		///     <paramref name="destinationPath"/> could not be found.
		/// </exception>
		/// <exception cref="UnauthorizedAccessException">
		///     The caller does not have the required access permissions.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="overwrite"/> is true and <paramref name="destinationPath"/> refers to a
		///     file that is read-only.
		/// </exception>
		/// <exception cref="IOException">
		///     <paramref name="overwrite"/> is false and <paramref name="destinationPath"/> refers to
		///     a file that already exists.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/> is a
		///     directory.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="overwrite"/> is true and <paramref name="destinationPath"/> refers to
		///     a file that already exists and is in use.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="sourcePath"/> refers to a file that is in use.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/> specifies
		///     a device that is not ready.
		/// </exception>
		public static void Copy(string sourcePath, string destinationPath, bool overwrite)
		{
			string normalizedSourcePath = Path.NormalizeLongPath(sourcePath, "sourcePath");
			string normalizedDestinationPath = Path.NormalizeLongPath(destinationPath, "destinationPath");

			if (!NativeMethods.CopyFile(normalizedSourcePath, normalizedDestinationPath, !overwrite))
				throw Common.GetExceptionFromLastWin32Error();
		}

		public static FileStream Create(string path)
		{
			return Create(path, DefaultBufferSize);
		}

		public static FileStream Create(string path, int bufferSize)
		{
			return Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize, FileOptions.None);
		}

		public static FileStream Create(string path, int bufferSize, FileOptions options)
		{
			return Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize, options);
		}

		public static FileStream Create(string path, int bufferSize, FileOptions options, FileSecurity fileSecurity)
		{
			var fileStream = Create(path, bufferSize, options);
			fileStream.SetAccessControl(fileSecurity);
			return fileStream;
		}

		/// <summary>
		///     Deletes the specified file.
		/// </summary>
		/// <param name="path">
		///      A <see cref="String"/> containing the path of the file to delete.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="path"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="path"/> is an empty string (""), contains only white
		///     space, or contains one or more invalid characters as defined in
		///     <see cref="Path.GetInvalidPathChars()"/>.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="path"/> contains one or more components that exceed
		///     the drive-defined maximum length. For example, on Windows-based
		///     platforms, components must not exceed 255 characters.
		/// </exception>
		/// <exception cref="PathTooLongException">
		///     <paramref name="path"/> exceeds the system-defined maximum length.
		///     For example, on Windows-based platforms, paths must not exceed
		///     32,000 characters.
		/// </exception>
		/// <exception cref="FileNotFoundException">
		///     <paramref name="path"/> could not be found.
		/// </exception>
		/// <exception cref="DirectoryNotFoundException">
		///     One or more directories in <paramref name="path"/> could not be found.
		/// </exception>
		/// <exception cref="UnauthorizedAccessException">
		///     The caller does not have the required access permissions.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="path"/> refers to a file that is read-only.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="path"/> is a directory.
		/// </exception>
		/// <exception cref="IOException">
		///     <paramref name="path"/> refers to a file that is in use.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="path"/> specifies a device that is not ready.
		/// </exception>
		public static void Delete(string path)
		{
			string normalizedPath = Path.NormalizeLongPath(path);
			if (!NativeMethods.DeleteFile(normalizedPath))
			{
				throw Common.GetExceptionFromLastWin32Error();
			}
		}

		public static void Decrypt(string path)
		{
			String fullPath = Path.GetFullPath(path);
			string normalizedPath = Path.NormalizeLongPath(fullPath);
			if (NativeMethods.DecryptFile(normalizedPath, 0)) return;
			int errorCode = Marshal.GetLastWin32Error();
			if (errorCode == NativeMethods.ERROR_ACCESS_DENIED)
			{
				var di = new DriveInfo(Path.GetPathRoot(normalizedPath));
				if (!String.Equals("NTFS", di.DriveFormat))
					throw new NotSupportedException("NTFS drive required for file encryption");
			}
			Common.ThrowIOError(errorCode, fullPath);
		}

		public static void Encrypt(String path)
		{
			String fullPath = Path.GetFullPath(path);
			string normalizedPath = Path.NormalizeLongPath(fullPath);
			if (NativeMethods.EncryptFile(normalizedPath)) return;
			int errorCode = Marshal.GetLastWin32Error();
			if (errorCode == NativeMethods.ERROR_ACCESS_DENIED)
			{
				var di = new DriveInfo(Path.GetPathRoot(normalizedPath));
				if (!String.Equals("NTFS", di.DriveFormat))
					throw new NotSupportedException("NTFS drive required for file encryption");
			}
			Common.ThrowIOError(errorCode, fullPath);
		}

		/// <summary>
		///     Returns a value indicating whether the specified path refers to an existing file.
		/// </summary>
		/// <param name="path">
		///     A <see cref="String"/> containing the path to check.
		/// </param>
		/// <returns>
		///     <see langword="true"/> if <paramref name="path"/> refers to an existing file;
		///     otherwise, <see langword="false"/>.
		/// </returns>
		/// <remarks>
		///     Note that this method will return false if any error occurs while trying to determine
		///     if the specified file exists. This includes situations that would normally result in
		///     thrown exceptions including (but not limited to); passing in a file name with invalid
		///     or too many characters, an I/O error such as a failing or missing disk, or if the caller
		///     does not have Windows or Code Access Security (CAS) permissions to to read the file.
		/// </remarks>
		public static bool Exists(string path)
		{
			bool isDirectory;
			if (Common.Exists(path, out isDirectory))
			{
				return !isDirectory;
			}

			return false;
		}

		public static FileStream Open(string path, FileMode mode)
		{
			return File.Open(path, mode, (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite), FileShare.None);
		}

		/// <summary>
		///     Opens the specified file.
		/// </summary>
		/// <param name="path">
		///     A <see cref="String"/> containing the path of the file to open.
		/// </param>
		/// <param name="access">
		///     One of the <see cref="FileAccess"/> value that specifies the operations that can be
		///     performed on the file.
		/// </param>
		/// <param name="mode">
		///     One of the <see cref="FileMode"/> values that specifies whether a file is created
		///     if one does not exist, and determines whether the contents of existing files are
		///     retained or overwritten.
		/// </param>
		/// <returns>
		///     A <see cref="FileStream"/> that provides access to the file specified in
		///     <paramref name="path"/>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="path"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="path"/> is an empty string (""), contains only white
		///     space, or contains one or more invalid characters as defined in
		///     <see cref="Path.GetInvalidPathChars()"/>.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="path"/> contains one or more components that exceed
		///     the drive-defined maximum length. For example, on Windows-based
		///     platforms, components must not exceed 255 characters.
		/// </exception>
		/// <exception cref="PathTooLongException">
		///     <paramref name="path"/> exceeds the system-defined maximum length.
		///     For example, on Windows-based platforms, paths must not exceed
		///     32,000 characters.
		/// </exception>
		/// <exception cref="DirectoryNotFoundException">
		///     One or more directories in <paramref name="path"/> could not be found.
		/// </exception>
		/// <exception cref="UnauthorizedAccessException">
		///     The caller does not have the required access permissions.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="path"/> refers to a file that is read-only and <paramref name="access"/>
		///     is not <see cref="FileAccess.Read"/>.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="path"/> is a directory.
		/// </exception>
		/// <exception cref="IOException">
		///     <paramref name="path"/> refers to a file that is in use.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="path"/> specifies a device that is not ready.
		/// </exception>
		public static FileStream Open(string path, FileMode mode, FileAccess access)
		{
			return Open(path, mode, access, FileShare.None, 0, FileOptions.None);
		}

		/// <summary>
		///     Opens the specified file.
		/// </summary>
		/// <param name="path">
		///     A <see cref="String"/> containing the path of the file to open.
		/// </param>
		/// <param name="access">
		///     One of the <see cref="FileAccess"/> value that specifies the operations that can be
		///     performed on the file.
		/// </param>
		/// <param name="mode">
		///     One of the <see cref="FileMode"/> values that specifies whether a file is created
		///     if one does not exist, and determines whether the contents of existing files are
		///     retained or overwritten.
		/// </param>
		/// <param name="share">
		///     One of the <see cref="FileShare"/> values specifying the type of access other threads
		///     have to the file.
		/// </param>
		/// <returns>
		///     A <see cref="FileStream"/> that provides access to the file specified in
		///     <paramref name="path"/>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="path"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="path"/> is an empty string (""), contains only white
		///     space, or contains one or more invalid characters as defined in
		///     <see cref="Path.GetInvalidPathChars()"/>.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="path"/> contains one or more components that exceed
		///     the drive-defined maximum length. For example, on Windows-based
		///     platforms, components must not exceed 255 characters.
		/// </exception>
		/// <exception cref="PathTooLongException">
		///     <paramref name="path"/> exceeds the system-defined maximum length.
		///     For example, on Windows-based platforms, paths must not exceed
		///     32,000 characters.
		/// </exception>
		/// <exception cref="DirectoryNotFoundException">
		///     One or more directories in <paramref name="path"/> could not be found.
		/// </exception>
		/// <exception cref="UnauthorizedAccessException">
		///     The caller does not have the required access permissions.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="path"/> refers to a file that is read-only and <paramref name="access"/>
		///     is not <see cref="FileAccess.Read"/>.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="path"/> is a directory.
		/// </exception>
		/// <exception cref="IOException">
		///     <paramref name="path"/> refers to a file that is in use.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="path"/> specifies a device that is not ready.
		/// </exception>
		public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
		{
			return Open(path, mode, access, share, 0, FileOptions.None);
		}

		/// <summary>
		///     Opens the specified file.
		/// </summary>
		/// <param name="path">
		///     A <see cref="String"/> containing the path of the file to open.
		/// </param>
		/// <param name="access">
		///     One of the <see cref="FileAccess"/> value that specifies the operations that can be
		///     performed on the file.
		/// </param>
		/// <param name="mode">
		///     One of the <see cref="FileMode"/> values that specifies whether a file is created
		///     if one does not exist, and determines whether the contents of existing files are
		///     retained or overwritten.
		/// </param>
		/// <param name="share">
		///     One of the <see cref="FileShare"/> values specifying the type of access other threads
		///     have to the file.
		/// </param>
		/// <param name="bufferSize">
		///     An <see cref="Int32"/> containing the number of bytes to buffer for reads and writes
		///     to the file, or 0 to specified the default buffer size, 1024.
		/// </param>
		/// <param name="options">
		///     One or more of the <see cref="FileOptions"/> values that describes how to create or
		///     overwrite the file.
		/// </param>
		/// <returns>
		///     A <see cref="FileStream"/> that provides access to the file specified in
		///     <paramref name="path"/>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="path"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="path"/> is an empty string (""), contains only white
		///     space, or contains one or more invalid characters as defined in
		///     <see cref="Path.GetInvalidPathChars()"/>.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="path"/> contains one or more components that exceed
		///     the drive-defined maximum length. For example, on Windows-based
		///     platforms, components must not exceed 255 characters.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///     <paramref name="bufferSize"/> is less than 0.
		/// </exception>
		/// <exception cref="PathTooLongException">
		///     <paramref name="path"/> exceeds the system-defined maximum length.
		///     For example, on Windows-based platforms, paths must not exceed
		///     32,000 characters.
		/// </exception>
		/// <exception cref="DirectoryNotFoundException">
		///     One or more directories in <paramref name="path"/> could not be found.
		/// </exception>
		/// <exception cref="UnauthorizedAccessException">
		///     The caller does not have the required access permissions.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="path"/> refers to a file that is read-only and <paramref name="access"/>
		///     is not <see cref="FileAccess.Read"/>.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="path"/> is a directory.
		/// </exception>
		/// <exception cref="IOException">
		///     <paramref name="path"/> refers to a file that is in use.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="path"/> specifies a device that is not ready.
		/// </exception>
		internal static FileStream Open(string path, FileMode mode, FileAccess access,
			FileShare share, int bufferSize, FileOptions options)
		{
			const int defaultBufferSize = 1024;

			if (bufferSize == 0)
				bufferSize = defaultBufferSize;

			string normalizedPath = Path.NormalizeLongPath(path);

			SafeFileHandle handle = GetFileHandle(normalizedPath, mode, access, share, options);

			return new FileStream(handle, access, bufferSize, (options & FileOptions.Asynchronous) == FileOptions.Asynchronous);
		}

		public static void SetCreationTime(String path, DateTime creationTime)
		{
			SetCreationTimeUtc(path, creationTime.ToUniversalTime());
		}

		public static unsafe void SetCreationTimeUtc(String path, DateTime creationTimeUtc)
		{
			string normalizedPath = Path.NormalizeLongPath(path);
			using (SafeFileHandle handle = GetFileHandle(normalizedPath,
				FileMode.Open, FileAccess.Write, FileShare.ReadWrite, FileOptions.None))
			{
				var fileTime = new NativeMethods.FILE_TIME(creationTimeUtc.ToFileTimeUtc());
				bool r = NativeMethods.SetFileTime(handle, &fileTime, null, null);
				if (!r)
				{
					int errorCode = Marshal.GetLastWin32Error();
					Common.ThrowIOError(errorCode, path);
				}
			}
		}

		public static DateTime GetCreationTime(String path)
		{
			return GetCreationTimeUtc(path).ToLocalTime();
		}

		public static DateTime GetCreationTimeUtc(String path)
		{
			var fi = new FileInfo(path);
			return fi.CreationTimeUtc;
		}

		public static void SetLastWriteTime(String path, DateTime lastWriteTime)
		{
			SetLastWriteTimeUtc(path, lastWriteTime.ToUniversalTime());
		}

		public static unsafe void SetLastWriteTimeUtc(String path, DateTime lastWriteTimeUtc)
		{
			string normalizedPath = Path.NormalizeLongPath(path);
			using (SafeFileHandle handle = GetFileHandle(normalizedPath,
				FileMode.Open, FileAccess.Write, FileShare.ReadWrite, FileOptions.None))
			{
				var fileTime = new NativeMethods.FILE_TIME(lastWriteTimeUtc.ToFileTimeUtc());
				bool r = NativeMethods.SetFileTime(handle, null, null, &fileTime);
				if (!r)
				{
					int errorCode = Marshal.GetLastWin32Error();
					Common.ThrowIOError(errorCode, path);
				}
			}
		}

		public static DateTime GetLastWriteTime(String path)
		{
			return GetLastWriteTimeUtc(path).ToLocalTime();
		}

		public static DateTime GetLastWriteTimeUtc(String path)
		{
			var fi = new FileInfo(path);
			return fi.LastWriteTimeUtc;
		}

		public static void SetLastAccessTime(String path, DateTime lastAccessTime)
		{
			SetLastAccessTimeUtc(path, lastAccessTime.ToUniversalTime());
		}

		public static unsafe void SetLastAccessTimeUtc(String path, DateTime lastAccessTimeUtc)
		{
			string normalizedPath = Path.NormalizeLongPath(path);
			using (SafeFileHandle handle = GetFileHandle(normalizedPath,
				FileMode.Open, FileAccess.Write, FileShare.ReadWrite, FileOptions.None))
			{
				var fileTime = new NativeMethods.FILE_TIME(lastAccessTimeUtc.ToFileTimeUtc());
				bool r = NativeMethods.SetFileTime(handle, null, &fileTime, null);
				if (!r)
				{
					int errorCode = Marshal.GetLastWin32Error();
					Common.ThrowIOError(errorCode, path);
				}
			}
		}

		public static DateTime GetLastAccessTime(String path)
		{
			return GetLastAccessTimeUtc(path).ToLocalTime();
		}

		public static DateTime GetLastAccessTimeUtc(String path)
		{
			var fi = new FileInfo(path);
			return fi.LastAccessTimeUtc;
		}

		public static FileAttributes GetAttributes(string path)
		{
			return Common.GetFileAttributes(path);
		}

		public static void SetAttributes(string path, FileAttributes fileAttributes)
		{
			Common.SetAttributes(path, fileAttributes);
		}

		public static FileStream OpenRead(String path)
		{
			return Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		public static FileStream OpenWrite(String path)
		{
			return Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
		}

		public static string ReadAllText(string path)
		{
			if (path == null) throw new ArgumentNullException("path");
			return ReadAllText(path, Encoding.UTF8);
		}

		public static string ReadAllText(string path, Encoding encoding)
		{
			using (var streamReader = OpenText(path, encoding))
			{
				return streamReader.ReadToEnd();
			}
		}

		public static void WriteAllText(String path, String contents)
		{
			WriteAllText(path, contents, UTF8NoBOM);
		}

		public static void WriteAllText(String path, String contents, Encoding encoding)
		{
			const bool doNotAppend = false;
			using (var sw = CreateStreamWriter(path, doNotAppend, encoding))
			{
				sw.Write(contents);
			}
		}

		public static byte[] ReadAllBytes(String path)
		{
			using (var fileStream = Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				long length = fileStream.Length;
				if (length > Int32.MaxValue) throw new IOException("File length greater than 2GB.");
				var bytes = new byte[length];
				int offset = 0;
				while (length > 0)
				{
					int read = fileStream.Read(bytes, offset, (int)length);
					if (read == 0)
					{
						throw new EndOfStreamException("Read beyond end of file.");
					}
					offset += read;
					length -= read;
				}
				return bytes;
			}
		}

		public static void WriteAllBytes(String path, byte[] bytes)
		{
			using (var fileStream = Open(path, FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				fileStream.Write(bytes, 0, bytes.Length);
			}
		}

		public static string[] ReadAllLines(String path)
		{
			return ReadLines(path).ToArray();
		}

		public static string[] ReadAllLines(String path, Encoding encoding)
		{
			return ReadLines(path, encoding).ToArray();
		}

		public static IEnumerable<string> ReadLines(String path)
		{
			return ReadAllLines(path, Encoding.UTF8);
		}

		public static IEnumerable<string> ReadLines(String path, Encoding encoding)
		{
			var stream = Open(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, FileOptions.SequentialScan);
			using (var sr = new StreamReader(stream, encoding, true, 1024, false))
			{
				while (!sr.EndOfStream)
				{
					yield return sr.ReadLine();
				}
			}
		}

		public static void WriteAllLines(String path, String[] contents)
		{
			WriteAllLines(path, contents, Encoding.UTF8);
		}

		public static void WriteAllLines(String path, String[] contents, Encoding encoding)
		{
			using (var writer = CreateStreamWriter(path, false, encoding))
			{
				foreach (var line in contents)
				{
					writer.WriteLine(line);
				}
			}
		}

		public static void WriteAllLines(String path, IEnumerable<string> contents)
		{
			WriteAllLines(path, contents, Encoding.UTF8);
		}

		public static void WriteAllLines(String path, IEnumerable<string> contents, Encoding encoding)
		{
			const bool doNotAppend = false;
			using (var writer = CreateStreamWriter(path, doNotAppend, encoding))
			{
				foreach (var line in contents)
				{
					writer.WriteLine(line);
				}
			}
		}

		public static void AppendAllText(String path, String contents)
		{
			AppendAllText(path, contents, Encoding.UTF8);
		}

		public static void AppendAllText(String path, String contents, Encoding encoding)
		{
			const bool append = true;
			using (var writer = CreateStreamWriter(path, append, encoding))
			{
				writer.Write(contents);
			}
		}

		public static void AppendAllLines(String path, IEnumerable<string> contents)
		{
			AppendAllLines(path, contents, Encoding.UTF8);
		}

		public static void AppendAllLines(String path, IEnumerable<string> contents, Encoding encoding)
		{
			const bool append = true;
			using (var writer = CreateStreamWriter(path, append, encoding))
			{
				foreach (var line in contents)
				{
					writer.WriteLine(line);
				}
			}
		}

		/// <summary>
		///     Moves the specified file to a new location.
		/// </summary>
		/// <param name="sourcePath">
		///     A <see cref="String"/> containing the path of the file to move.
		/// </param>
		/// <param name="destinationPath">
		///     A <see cref="String"/> containing the new path of the file.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/> is
		///     <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/> is
		///     an empty string (""), contains only white space, or contains one or more
		///     invalid characters as defined in <see cref="Path.GetInvalidPathChars()"/>.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/>
		///     contains one or more components that exceed the drive-defined maximum length.
		///     For example, on Windows-based platforms, components must not exceed 255 characters.
		/// </exception>
		/// <exception cref="PathTooLongException">
		///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/>
		///     exceeds the system-defined maximum length. For example, on Windows-based platforms,
		///     paths must not exceed 32,000 characters.
		/// </exception>
		/// <exception cref="FileNotFoundException">
		///     <paramref name="sourcePath"/> could not be found.
		/// </exception>
		/// <exception cref="DirectoryNotFoundException">
		///     One or more directories in <paramref name="sourcePath"/> and/or
		///     <paramref name="destinationPath"/> could not be found.
		/// </exception>
		/// <exception cref="UnauthorizedAccessException">
		///     The caller does not have the required access permissions.
		/// </exception>
		/// <exception cref="IOException">
		///     <paramref name="destinationPath"/> refers to a file that already exists.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/> is a
		///     directory.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="sourcePath"/> refers to a file that is in use.
		///     <para>
		///         -or-
		///     </para>
		///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/> specifies
		///     a device that is not ready.
		/// </exception>
		public static void Move(string sourcePath, string destinationPath)
		{
			string normalizedSourcePath = Path.NormalizeLongPath(sourcePath, "sourcePath");
			string normalizedDestinationPath = Path.NormalizeLongPath(destinationPath, "destinationPath");

			if (!NativeMethods.MoveFile(normalizedSourcePath, normalizedDestinationPath))
				throw Common.GetExceptionFromLastWin32Error();
		}

		public static void Replace(String sourceFileName, String destinationFileName, String destinationBackupFileName)
		{
			if (sourceFileName == null) throw new ArgumentNullException("sourceFileName");
			if (destinationFileName == null) throw new ArgumentNullException("destinationFileName");

			Replace(sourceFileName, destinationFileName, destinationBackupFileName, false);
		}

		public static void Replace(String sourceFileName, String destinationFileName, String destinationBackupFileName,
			bool ignoreMetadataErrors)
		{
			if (sourceFileName == null) throw new ArgumentNullException("sourceFileName");
			if (destinationFileName == null) throw new ArgumentNullException("destinationFileName");

			String fullSrcPath = Path.NormalizeLongPath(Path.GetFullPath(sourceFileName));
			String fullDestPath = Path.NormalizeLongPath(Path.GetFullPath(destinationFileName));
			String fullBackupPath = null;
			if (destinationBackupFileName != null)
				fullBackupPath = Path.NormalizeLongPath(Path.GetFullPath(destinationBackupFileName));

			int flags = NativeMethods.REPLACEFILE_WRITE_THROUGH;
			if (ignoreMetadataErrors)
				flags |= NativeMethods.REPLACEFILE_IGNORE_MERGE_ERRORS;

			bool r = NativeMethods.ReplaceFile(fullDestPath, fullSrcPath, fullBackupPath, flags, IntPtr.Zero, IntPtr.Zero);

			if (!r)
				Common.ThrowIOError(Marshal.GetLastWin32Error(), String.Empty);
		}

		public static void SetAccessControl(string path, FileSecurity fileSecurity)
		{
			if (path == null) throw new ArgumentNullException("path");
			if (fileSecurity == null) throw new ArgumentNullException("fileSecurity");
			var name = Path.NormalizeLongPath(Path.GetFullPath(path));

			Common.SetAccessControlExtracted(fileSecurity, name);
		}

		private static void ThrowIfError(int errorCode, IntPtr ByteArray)
		{
			if (errorCode == NativeMethods.ERROR_SUCCESS && IntPtr.Zero.Equals(ByteArray))
			{
				//
				// This means that the object doesn't have a security descriptor. And thus we throw
				// a specific exception for the caller to catch and handle properly.
				//
				throw new InvalidOperationException("Object does not have security descriptor,");
			}
			else if (errorCode == NativeMethods.ERROR_NOT_ALL_ASSIGNED ||
						errorCode == NativeMethods.ERROR_PRIVILEGE_NOT_HELD)
			{
				throw new PrivilegeNotHeldException("SeSecurityPrivilege");
			}
			else if (errorCode == NativeMethods.ERROR_ACCESS_DENIED ||
				errorCode == NativeMethods.ERROR_CANT_OPEN_ANONYMOUS)
			{
				throw new UnauthorizedAccessException();
			}
			if (errorCode == NativeMethods.ERROR_NOT_ENOUGH_MEMORY)
			{
				throw new OutOfMemoryException();
			}
		}

		public static FileSecurity GetAccessControl(string path)
		{
			AccessControlSections includeSections = AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group;
			return GetAccessControl(path, includeSections);
		}

		public static FileSecurity GetAccessControl(string path, AccessControlSections includeSections)
		{
			var normalizedPath = Path.NormalizeLongPath(Path.GetFullPath(path));

			IntPtr SidOwner, SidGroup, Dacl, Sacl, ByteArray;
			SecurityInfos SecurityInfos =
				Common.ToSecurityInfos(includeSections);

			int errorCode = (int)NativeMethods.GetSecurityInfoByName(normalizedPath,
				(uint)ResourceType.FileObject,
				(uint)SecurityInfos,
				out SidOwner,
				out SidGroup,
				out Dacl,
				out Sacl,
				out ByteArray);

			ThrowIfError(errorCode, ByteArray);

			uint Length = NativeMethods.GetSecurityDescriptorLength(ByteArray);

			byte[] BinaryForm = new byte[Length];

			Marshal.Copy(ByteArray, BinaryForm, 0, (int)Length);

			NativeMethods.LocalFree(ByteArray);
			var fs = new FileSecurity();
			fs.SetSecurityDescriptorBinaryForm(BinaryForm);
			return fs;
		}

		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "handle is stored by caller")]
		internal static SafeFileHandle GetFileHandle(string normalizedPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
		{
			bool append = mode == FileMode.Append;
			if (append)
			{
				mode = FileMode.OpenOrCreate;
			}
			NativeMethods.EFileAccess underlyingAccess = GetUnderlyingAccess(access);

			SafeFileHandle handle = NativeMethods.CreateFile(normalizedPath, underlyingAccess, (uint)share, IntPtr.Zero, (uint)mode, (uint)options, IntPtr.Zero);
			if (handle.IsInvalid)
			{
				Exception ex = Common.GetExceptionFromLastWin32Error();
				Console.WriteLine("error {0} with {1}\n{2}", ex.Message, normalizedPath, ex.StackTrace);
				Console.WriteLine("{0} {1} {2} {3}", mode, access, share, options );
				throw ex;
			}

			if (append)
			{
				NativeMethods.SetFilePointer(handle, 0, SeekOrigin.End);
			}
			return handle;
		}

		private static NativeMethods.EFileAccess GetUnderlyingAccess(FileAccess access)
		{
			switch (access)
			{
				case FileAccess.Read:
					return NativeMethods.EFileAccess.GenericRead;

				case FileAccess.Write:
					return NativeMethods.EFileAccess.GenericWrite;

				case FileAccess.ReadWrite:
					return NativeMethods.EFileAccess.GenericRead | NativeMethods.EFileAccess.GenericWrite;

				default:
					throw new ArgumentOutOfRangeException("access");
			}
		}

		internal const int DefaultBufferSize = 4096;

		private static volatile Encoding _UTF8NoBOM;

		internal static Encoding UTF8NoBOM
		{
			get
			{
				if (_UTF8NoBOM == null)
				{
					// No need for double lock - we just want to avoid extra
					// allocations in the common case.
					UTF8Encoding noBOM = new UTF8Encoding(false, true);
					Thread.MemoryBarrier();
					_UTF8NoBOM = noBOM;
				}
				return _UTF8NoBOM;
			}
		}

		/// <remarks>
		/// replaces "new StreamWriter(path, true|false)"
		/// </remarks>
		internal static StreamWriter CreateStreamWriter(string path, bool append)
		{
			var fileMode = (append ? FileMode.Append : FileMode.Create);
			var fileStream = Open(path, fileMode, FileAccess.Write, FileShare.Read, 4096, FileOptions.SequentialScan);
			return new StreamWriter(fileStream, UTF8NoBOM, 1024, false);
		}

		internal static StreamWriter CreateStreamWriter(string path, bool append, Encoding encoding)
		{
			var fileMode = (append ? FileMode.Append : FileMode.Create);
			var fileStream = Open(path, fileMode, FileAccess.Write, FileShare.Read, 4096, FileOptions.SequentialScan);
			return new StreamWriter(fileStream, encoding, 1024, false);
		}

		internal static StreamWriter CreateText(string path, Encoding encoding)
		{
			return File.CreateStreamWriter(path, false, encoding);
		}

		/// <remarks>
		/// replaces "new StreamReader(path, true|false)"
		/// </remarks>
		internal static StreamReader CreateStreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
		{
			var fileStream = Open(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
			return new StreamReader(fileStream, encoding, detectEncodingFromByteOrderMarks, bufferSize, false);
		}
	}
}