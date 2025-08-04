using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace L10NSharp.Windows.Forms.Tests
{

	/// <summary>
	/// This is useful a temporary file is needed. When it is disposed, it will delete the file.
	///
	/// Sometimes it is useful to make a temp file and NOT have the TempFile class delete it.
	/// In such cases, simply do not Dispose() the TempFile. To make this possible and reliable,
	/// this class deliberately does NOT implement a destructor or do anything to ensure
	/// the file is deleted if the TempFile is not disposed. Please don't change this.
	/// </summary>
	/// <example>using(f = new TempFile())</example>
	public class TempFile : IDisposable
	{
		protected string _path;
		private string _folderToDelete; // if not null, delete this as well on dispose

		public TempFile()
		{
			_path = System.IO.Path.GetTempFileName();
		}

		public TempFile(bool dontMakeMeAFileAndDontSetPath)
		{
			if(!dontMakeMeAFileAndDontSetPath)
			{
				_path = System.IO.Path.GetTempFileName();
			}
		}

		public TempFile(string contents)
			: this()
		{
			File.WriteAllText(_path, contents);
		}

		public TempFile(string contents, Encoding encoding)
			: this()
		{
			File.WriteAllText(_path, contents, encoding);
		}

		public TempFile(string[] contentLines)
			: this()
		{
			File.WriteAllLines(_path, contentLines);
		}

		public string Path
		{
			get { return _path; }
		}

		// See comment on class above regarding Dispose
		public void Dispose()
		{
			File.Delete(_path);
			if(_folderToDelete != null)
				DeleteDirectoryRobust(_folderToDelete);
		}

		public static TempFile CopyOf(string pathToExistingFile)
		{
			TempFile t = new TempFile();
			File.Copy(pathToExistingFile, t.Path, true);
			return t;
		}

		public TempFile(string existingPath, bool dummy)
		{
			_path = existingPath;
		}

		/// <summary>
		/// Create a TempFile based on a pre-existing file, which will be deleted when this is disposed.
		/// </summary>
		public static TempFile TrackExisting(string path)
		{
			return new TempFile(path, false);
		}

		public static TempFile CreateAndGetPathButDontMakeTheFile()
		{
			TempFile t = new TempFile();
			File.Delete(t.Path);
			return t;
		}

		/// <summary>
		/// Use this one when it's important to have a certain file extension
		/// </summary>
		/// <param name="extension">with or with out '.', will work the same</param>
		public static TempFile WithExtension(string extension)
		{
			extension = extension.TrimStart('.');
			var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName() + "." + extension);
			File.Create(path).Close();
			return TrackExisting(path);
		}

		/// <summary>
		/// Use this one when it's important to have a certain file name (with, or without extension).
		/// </summary>
		/// <param name="filename">with or with out an extension, will work the same</param>
		public static TempFile WithFilename(string filename)
		{
			if(filename == null)
				throw new ArgumentNullException("filename");
			if(filename == string.Empty)
				throw new ArgumentException("Filename has no content", "filename");
			filename = filename.Trim();
			if(filename == string.Empty)
				throw new ArgumentException("Filename has only whitespace", "filename");

			var pathname = System.IO.Path.Combine(System.IO.Path.GetTempPath(), filename);
			File.Create(pathname).Close();
			return TrackExisting(pathname);
		}

		/// <summary>
		/// Creates a file with the specified name in a new, randomly named folder.
		/// Dispose will dispose of the folder (and any subsequently added content) as well as the temp file.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static TempFile WithFilenameInTempFolder(string fileName)
		{
			var tempFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
			Directory.CreateDirectory(tempFolder);
			var path = System.IO.Path.Combine(tempFolder, fileName);
			var result = TempFile.TrackExisting(path);
			result._folderToDelete = tempFolder;
			return result;
		}

		/// <summary>
		/// Used to make a real file out of a resource for the purpose of testing
		/// </summary>
		/// <param name="resource">e.g., an audio resource</param>
		/// <param name="extension">with or with out '.', will work the same</param>
		public static TempFile FromResource(Stream resource, string extension)
		{
			var f = WithExtension(extension);
			byte[] buffer = new byte[resource.Length + 1];
			resource.Read(buffer, 0, (int)resource.Length);
			File.WriteAllBytes(f.Path, buffer);
			return f;
		}

		/// <summary>
		/// Used to make a real file out of a resource for the purpose of testing
		/// </summary>
		/// <param name="resource">e.g., a video resource</param>
		/// <param name="extension">with or with out '.', will work the same</param>
		public static TempFile FromResource(byte[] resource, string extension)
		{
			var f = WithExtension(extension);
			File.WriteAllBytes(f.Path, resource);
			return f;
		}

		/// <summary>
		/// Used to move a file to a new path
		/// </summary>
		public void MoveTo(string path)
		{
			File.Move(Path, path);
			_path = path;
		}

		/// <summary>
		/// There are various things which can prevent a simple directory deletion, mostly timing related things which are hard to debug.
		/// This method uses all the tricks to do its best.
		/// </summary>
		/// <returns>returns true if the directory is fully deleted</returns>
		public static bool DeleteDirectoryRobust(string path)
		{
			// ReSharper disable EmptyGeneralCatchClause

			for(int i = 0; i < 40; i++) // each time, we sleep a little. This will try for up to 2 seconds (40*50ms)
			{
				if(!Directory.Exists(path))
					break;

				try
				{
					Directory.Delete(path, true);
				}
				catch(Exception)
				{
				}

				if(!Directory.Exists(path))
					break;

				try
				{
					//try to clear it out a bit
					string[] dirs = Directory.GetDirectories(path);
					string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
					foreach(string filePath in files)
					{
						try
						{
							/* we could do this too, but it's dangerous
							 *  File.SetAttributes(filePath, FileAttributes.Normal);
							 */
							File.Delete(filePath);
						}
						catch(Exception)
						{
						}
					}
					foreach(var dir in dirs)
					{
						DeleteDirectoryRobust(dir);
					}

				}
				catch(Exception)//yes, even these simple queries can throw exceptions, as stuff suddenly is deleted base on our prior request
				{
				}
				//sleep and let some OS things catch up
				Thread.Sleep(50);
			}

			return !Directory.Exists(path);
			// ReSharper restore EmptyGeneralCatchClause
		}
	}

	public class TempFolder : IDisposable
	{
		private static string _basePath;
		private readonly string _path;

		public TempFolder(): this(TestContext.CurrentContext.Test.Name)
		{
		}

		public TempFolder(string testName)
		{
			testName = System.IO.Path.GetInvalidPathChars().Aggregate(testName,
				(current, c) => current.Replace(c, '_')).Replace('`', '_').Replace('"', '_');
			_path = System.IO.Path.Combine(BasePath, testName);
			if(Directory.Exists(_path))
			{
				TestUtilities.DeleteFolderThatMayBeInUse(_path);
			}
			Directory.CreateDirectory(_path);
		}

		private static string BasePath =>
			_basePath ?? (_basePath = System.IO.Path.Combine(
				System.IO.Path.GetTempPath(),
				System.IO.Path.GetRandomFileName()));

		public string Path => _path;

		public void Dispose()
		{
			TestUtilities.DeleteFolderThatMayBeInUse(_path);
		}

		public TempFile GetPathForNewTempFile(bool doCreateTheFile)
		{
			var s = System.IO.Path.GetRandomFileName();
			s = System.IO.Path.Combine(_path, s);
			if(doCreateTheFile)
			{
				File.Create(s).Close();
			}
			return TempFile.TrackExisting(s);
		}

		public string Combine(string innerFileName)
		{
			return System.IO.Path.Combine(_path, innerFileName);
		}
	}

	public class TestUtilities
	{
		public static void DeleteFolderThatMayBeInUse(string folder)
		{
			if (!Directory.Exists(folder))
				return;

			for(int i = 0; i < 50; i++) //wait up to five seconds
			{
				try
				{
					Directory.Delete(folder, true);
					return;
				}
				catch(Exception)
				{
				}
				Thread.Sleep(100);
			}
			//maybe we can at least clear it out a bit
			try
			{
				Debug.WriteLine("TestUtilities.DeleteFolderThatMayBeInUse(): gave up trying to delete the whole folder. Some files may be abandoned in your temp folder.");

				string[] files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
				foreach(string s in files)
				{
					File.Delete(s);
				}
				//sleep and try again
				Thread.Sleep(1000);
				Directory.Delete(folder, true);
			}
			catch(Exception)
			{
			}
		}
	}
}
