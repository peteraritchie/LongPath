using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using File = Pri.LongPath.File;
using FileMode=System.IO.FileMode;
using FileAccess = System.IO.FileAccess;
using FileShare = System.IO.FileShare;
using FileOptions = System.IO.FileOptions;
using Path = Pri.LongPath.Path;

namespace Tests
{
	static internal class Util
	{
		public static string MakeLongPath(string path)
		{
			var volname = new StringBuilder(261);
			var fsname = new StringBuilder(261);
			uint sernum, maxlen;
			NativeMethods.FileSystemFeature flags;
			NativeMethods.GetVolumeInformation(System.IO.Path.GetPathRoot(path), volname, volname.Capacity, out sernum, out maxlen, out flags, fsname,
				fsname.Capacity);
			var componentText = Enumerable.Repeat("0123456789", (int) ((maxlen + 10)/10))
				.Aggregate((c, n) => c + n)
				.Substring(0, (int) maxlen);
			Debug.Assert(componentText.Length == maxlen);
			var directorySeparatorText = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
			var endsWith = path.EndsWith(directorySeparatorText);
			var resultPath = new StringBuilder(path)
				.Append(endsWith ? String.Empty : directorySeparatorText)
				.Append(componentText)
				.Append(Path.DirectorySeparatorChar)
				.Append(componentText)
				.ToString();
			Debug.Assert(resultPath.Length > 260);
			return resultPath;
		}

		public static string MakeLongComponent(string path)
		{
			var volname = new StringBuilder(261);
			var fsname = new StringBuilder(261);
			uint sernum, maxlen;
			NativeMethods.FileSystemFeature flags;
			NativeMethods.GetVolumeInformation(System.IO.Path.GetPathRoot(path), volname, volname.Capacity, out sernum, out maxlen, out flags, fsname,
				fsname.Capacity);
			var componentText = Enumerable.Repeat("0123456789", (int)((maxlen + 10) / 10))
				.Aggregate((c, n) => c + n)
				.Substring(0, (int)maxlen);
			Debug.Assert(componentText.Length == maxlen);
			var directorySeparatorText = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
			var endsWith = path.EndsWith(directorySeparatorText);
			var resultPath = new StringBuilder(path)
				.Append(endsWith ? String.Empty : directorySeparatorText)
				.Append(componentText)
				.Append(Path.DirectorySeparatorChar)
				.Append(componentText)
				.ToString();
			Debug.Assert(resultPath.Length > 260);
			return resultPath;
		}

		public static string CreateNewFile(string longPathDirectory)
		{
			var tempLongPathFilename = CreateNewEmptyFile(longPathDirectory);
			using (var streamWriter = File.AppendText(tempLongPathFilename))
			{
				streamWriter.WriteLine("beginning of file");
			}

			return tempLongPathFilename;
		}

		public static string CreateNewEmptyFile(string longPathDirectory)
		{
			var tempLongPathFilename = new StringBuilder(longPathDirectory).Append(Path.DirectorySeparatorChar).Append(Path.GetRandomFileName()).ToString();
			using (File.Create(tempLongPathFilename))
			{
			}
			return tempLongPathFilename;
		}

		public static string CreateNewEmptyFile(string longPathDirectory, string filename)
		{
			var tempLongPathFilename = new StringBuilder(longPathDirectory).Append(Path.DirectorySeparatorChar).Append(filename).ToString();
			using (File.Create(tempLongPathFilename))
			{
			}
			return tempLongPathFilename;
		}

		public static bool VerifyContentsOfNewFile(string path)
		{
			string contents = File.ReadAllText(path);
			return "beginning of file" + Environment.NewLine == contents;
		}

		public static string CreateNewFileUnicode(string longPathDirectory)
		{
			var tempLongPathFilename = CreateNewEmptyFile(longPathDirectory);
			var fileStream = File.Open(tempLongPathFilename, FileMode.Create, FileAccess.Write, FileShare.Read, 4096,
				FileOptions.SequentialScan);
			using (var streamWriter = new StreamWriter(fileStream, Encoding.Unicode, 4096, false))
			{
				streamWriter.WriteLine("beginning of file");
			}
			return tempLongPathFilename;
		}
	}
}