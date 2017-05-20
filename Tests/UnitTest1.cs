using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Directory = Pri.LongPath.Directory;
using Path = Pri.LongPath.Path;
using FileInfo = Pri.LongPath.FileInfo;
using DirectoryInfo = Pri.LongPath.DirectoryInfo;
using FileSystemInfo = Pri.LongPath.FileSystemInfo;
using File = Pri.LongPath.File;
using FileMode = System.IO.FileMode;
using FileAccess = System.IO.FileAccess;
using FileShare = System.IO.FileShare;
using BinaryWriter = System.IO.BinaryWriter;
using PathTooLongException = System.IO.PathTooLongException;
using FileAttributes = System.IO.FileAttributes;
using IOException = System.IO.IOException;
using SearchOption = System.IO.SearchOption;
using System.Reflection;
using System.Collections.Generic;

namespace Tests
{
	[TestFixture]
	public class UnitTest1
	{
		private static string longPathDirectory;
		private static string longPathRoot;

		[SetUp]
		public void SetUp()
		{
			longPathDirectory = Util.MakeLongPath(TestContext.CurrentContext.TestDirectory);
			longPathRoot = longPathDirectory.Substring(0,
				TestContext.CurrentContext.TestDirectory.Length + 1 + longPathDirectory
					.Substring(TestContext.CurrentContext.TestDirectory.Length + 1).IndexOf('\\'));
			Directory.CreateDirectory(longPathDirectory);
			Debug.Assert(Directory.Exists(longPathDirectory));
		}

		[Test]
		public void TestProblemWithSystemIoExists()
		{
			Assert.Throws<PathTooLongException>(() =>
			{
				var filename = new StringBuilder(longPathDirectory).Append(@"\").Append("file4.ext").ToString();
				using (var writer = File.CreateText(filename))
				{
					writer.WriteLine("test");
				}
				Assert.IsTrue(File.Exists(filename));

				try
				{
					using (var fileStream = new System.IO.FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.None))
					using (var bw = new BinaryWriter(fileStream))
					{
						bw.Write(10u);
					}
				}
				finally
				{
					File.Delete(filename);
				}
			});
		}

		[Test]
		public void WhatHappensWithBclPathGetDiretoryNameAndRelatiePath()
		{
			var text = System.IO.Path.GetDirectoryName(@"foo\bar\baz");
			Assert.AreEqual(@"foo\bar", text);
		}

		private string MemberToMethodString(MemberInfo member)
		{
			var method = member as MethodInfo;
			if (method == null) return member.Name;
			ParameterInfo[] parameters = method.GetParameters();
			return string.Format("{0} {1}({2})", method.ReturnType.Name, method.Name,
				!parameters.Any() ? "" : (parameters.Select(e => e.ParameterType.Name).Aggregate((c, n) => c + ", " + n)));
		}

		[Test]
		public void FileClassIsComplete()
		{
			MemberInfo[] systemIoFileMembers =
				typeof(System.IO.File).GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance |
				                                  BindingFlags.Static);
			MemberInfo[] fileMembers = typeof(File).GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public |
			                                                   BindingFlags.Instance | BindingFlags.Static);
			string missing = "";
			if (systemIoFileMembers.Length != fileMembers.Length)
			{
				IEnumerable<string> systemIoFileMemberNames =
					systemIoFileMembers.OrderBy(e => e.Name).Select(e => MemberToMethodString(e));
				missing = systemIoFileMemberNames.Aggregate((c, n) => c + ", " + n);
				IEnumerable<string> fileMemberNames = fileMembers.OrderBy(e => e.Name).Select(e => MemberToMethodString(e));
				missing = fileMemberNames.Aggregate((c, n) => c + ", " + n);
				IEnumerable<string> missingCollection = fileMemberNames.Except(systemIoFileMemberNames);
				IEnumerable<string> missingCollection2 = systemIoFileMemberNames.Except(fileMemberNames);
				missing = (!missingCollection2.Any()
					          ? ""
					          : ("missing: " + missingCollection2.Aggregate((c, n) => c + ", " + n) + Environment.NewLine)) +
				          (!missingCollection.Any() ? "" : ("extra: " + missingCollection.Aggregate((c, n) => c + ", " + n)));
			}
			Assert.AreEqual(systemIoFileMembers.Length, fileMembers.Length, missing);
		}

		[Test]
		public void DirectoryClassIsComplete()
		{
			MemberInfo[] systemIoDirectoryMembers =
				typeof(System.IO.Directory).GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance |
				                                       BindingFlags.Static);
			MemberInfo[] directoryMembers =
				typeof(Directory).GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance |
				                             BindingFlags.Static);
			string missing = "";
			if (systemIoDirectoryMembers.Length != directoryMembers.Length)
			{
				IOrderedEnumerable<MemberInfo> systemIoDirectoryMembersOrdered = systemIoDirectoryMembers.OrderBy(e => e.Name);
				IEnumerable<string> systemIoDirectoryMemberNames =
					systemIoDirectoryMembersOrdered.Select(e => MemberToMethodString(e));
				IOrderedEnumerable<MemberInfo> directoryMembersOrdered = directoryMembers.OrderBy(e => e.Name);
				IEnumerable<string> directoryMemberNames = directoryMembersOrdered.Select(e => MemberToMethodString(e));
				IEnumerable<string> missingCollection = directoryMemberNames.Except(systemIoDirectoryMemberNames);
				IEnumerable<string> missingCollection2 = systemIoDirectoryMemberNames.Except(directoryMemberNames);
				missing = (!missingCollection2.Any()
					          ? ""
					          : ("missing: " + missingCollection2.Aggregate((c, n) => c + ", " + n) + Environment.NewLine)) +
				          (!missingCollection.Any() ? "" : ("extra: " + missingCollection.Aggregate((c, n) => c + ", " + n)));
			}
			Assert.AreEqual(systemIoDirectoryMembers.Length, directoryMembers.Length, missing);
		}

		[Test]
		public void FileInfoClassIsComplete()
		{
			MemberInfo[] systemIoFileInfoMembers =
				typeof(System.IO.FileInfo).GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance |
				                                      BindingFlags.Static);
			MemberInfo[] FileInfoMembers =
				typeof(FileInfo).GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance |
				                            BindingFlags.Static);
			string missing = "";
			if (systemIoFileInfoMembers.Length != FileInfoMembers.Length)
			{
				IOrderedEnumerable<MemberInfo> systemIoFileInfoMembersOrdered = systemIoFileInfoMembers.OrderBy(e => e.Name);
				IEnumerable<string> systemIoFileInfoMemberNames =
					systemIoFileInfoMembersOrdered.Select(e => MemberToMethodString(e));
				IOrderedEnumerable<MemberInfo> FileInfoMembersOrdered = FileInfoMembers.OrderBy(e => e.Name);
				IEnumerable<string> FileInfoMemberNames = FileInfoMembersOrdered.Select(e => MemberToMethodString(e));
				IEnumerable<string> missingCollection = FileInfoMemberNames.Except(systemIoFileInfoMemberNames);
				IEnumerable<string> missingCollection2 = systemIoFileInfoMemberNames.Except(FileInfoMemberNames);
				missing = (!missingCollection2.Any()
					          ? ""
					          : ("missing: " + missingCollection2.Aggregate((c, n) => c + ", " + n) + Environment.NewLine)) +
				          (!missingCollection.Any() ? "" : ("extra: " + missingCollection.Aggregate((c, n) => c + ", " + n)));
			}
			Assert.LessOrEqual(systemIoFileInfoMembers.Length, FileInfoMembers.Length, missing);
		}

		[Test]
		public void DirectoryInfoClassIsComplete()
		{
			MemberInfo[] systemIoDirectoryInfoMembers =
				typeof(System.IO.DirectoryInfo).GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance |
				                                           BindingFlags.Static);
			MemberInfo[] DirectoryInfoMembers =
				typeof(DirectoryInfo).GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance |
				                                 BindingFlags.Static);
			string missing = "";
			if (systemIoDirectoryInfoMembers.Length != DirectoryInfoMembers.Length)
			{
				IOrderedEnumerable<MemberInfo> systemIoDirectoryInfoMembersOrdered =
					systemIoDirectoryInfoMembers.OrderBy(e => e.Name);
				IEnumerable<string> systemIoDirectoryInfoMemberNames =
					systemIoDirectoryInfoMembersOrdered.Select(e => MemberToMethodString(e));
				IOrderedEnumerable<MemberInfo> DirectoryInfoMembersOrdered = DirectoryInfoMembers.OrderBy(e => e.Name);
				IEnumerable<string> DirectoryInfoMemberNames = DirectoryInfoMembersOrdered.Select(e => MemberToMethodString(e));
				IEnumerable<string> missingCollection = DirectoryInfoMemberNames.Except(systemIoDirectoryInfoMemberNames);
				IEnumerable<string> missingCollection2 = systemIoDirectoryInfoMemberNames.Except(DirectoryInfoMemberNames);
				missing = (!missingCollection2.Any()
					          ? ""
					          : ("missing: " + missingCollection2.Aggregate((c, n) => c + ", " + n) + Environment.NewLine)) +
				          (!missingCollection.Any() ? "" : ("extra: " + missingCollection.Aggregate((c, n) => c + ", " + n)));
			}
			Assert.LessOrEqual(systemIoDirectoryInfoMembers.Length, DirectoryInfoMembers.Length, missing);
		}

		[Test]
		public void PathClassIsComplete()
		{
			MemberInfo[] systemIoPathMembers =
				typeof(System.IO.Path).GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance |
				                                  BindingFlags.Static);
			MemberInfo[] PathMembers = typeof(Path).GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public |
			                                                   BindingFlags.Instance | BindingFlags.Static);
			string missing = "";
			if (systemIoPathMembers.Length != PathMembers.Length)
			{
				IOrderedEnumerable<MemberInfo> systemIoPathMembersOrdered = systemIoPathMembers.OrderBy(e => e.Name);
				IEnumerable<string> systemIoPathMemberNames = systemIoPathMembersOrdered.Select(e => MemberToMethodString(e));
				IOrderedEnumerable<MemberInfo> PathMembersOrdered = PathMembers.OrderBy(e => e.Name);
				IEnumerable<string> PathMemberNames = PathMembersOrdered.Select(e => MemberToMethodString(e));
				IEnumerable<string> missingCollection = PathMemberNames.Except(systemIoPathMemberNames);
				IEnumerable<string> missingCollection2 = systemIoPathMemberNames.Except(PathMemberNames);
				missing = (!missingCollection2.Any()
					          ? ""
					          : ("missing: " + missingCollection2.Aggregate((c, n) => c + ", " + n) + Environment.NewLine)) +
				          (!missingCollection.Any() ? "" : ("extra: " + missingCollection.Aggregate((c, n) => c + ", " + n)));
			}
			Assert.AreEqual(systemIoPathMembers.Length, PathMembers.Length, missing);
		}

		[Test]
		public void FileSystemInfoClassIsComplete()
		{
			MemberInfo[] systemIoFileSystemInfoMembers =
				typeof(System.IO.FileSystemInfo).GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public |
				                                            BindingFlags.Instance | BindingFlags.Static);
			MemberInfo[] FileSystemInfoMembers =
				typeof(FileSystemInfo).GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance |
				                                  BindingFlags.Static);
			string missing = "";
			if (systemIoFileSystemInfoMembers.Length != FileSystemInfoMembers.Length)
			{
				IOrderedEnumerable<MemberInfo> systemIoFileSystemInfoMembersOrdered =
					systemIoFileSystemInfoMembers.OrderBy(e => e.Name);
				IEnumerable<string> systemIoFileSystemInfoMemberNames =
					systemIoFileSystemInfoMembersOrdered.Select(e => MemberToMethodString(e));
				IOrderedEnumerable<MemberInfo> FileSystemInfoMembersOrdered = FileSystemInfoMembers.OrderBy(e => e.Name);
				IEnumerable<string> FileSystemInfoMemberNames = FileSystemInfoMembersOrdered.Select(e => MemberToMethodString(e));
				IEnumerable<string> missingCollection = FileSystemInfoMemberNames.Except(systemIoFileSystemInfoMemberNames);
				IEnumerable<string> missingCollection2 = systemIoFileSystemInfoMemberNames.Except(FileSystemInfoMemberNames);
				missing = (!missingCollection2.Any()
					          ? ""
					          : ("missing: " + missingCollection2.Aggregate((c, n) => c + ", " + n) + Environment.NewLine)) +
				          (!missingCollection.Any() ? "" : ("extra: " + missingCollection.Aggregate((c, n) => c + ", " + n)));
			}
			Assert.LessOrEqual(systemIoFileSystemInfoMembers.Length, FileSystemInfoMembers.Length, missing);
		}

		[TearDown]
		public void TearDown()
		{
			Directory.Delete(longPathRoot, true);
			Debug.Assert(!Directory.Exists(longPathDirectory));
		}
	}
}