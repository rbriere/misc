/*
The MIT License (MIT)

Copyright (c) 2014 RBriere

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace GmailizeFolder
{
    class Program
    {
		//To compile:
		//"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"  /out:C:\utils\GmailizeFolder.exe  C:\Programming\cs\GmailizeFolder.cs 
		//
		//To Run"
		//GmailizeFolder "c:\somewhere\soon-to-be-zipped-and-or-mailed-folder-name"
        [STAThread]
		static int Main(string[] args)
        {
			var showhelp = args.Where(x => x.Trim().ToLower().Equals("/?") || x.Trim().ToLower().Equals("-") || x.Trim().ToLower().Equals("?")).Select(x => x);
			var showlist = args.Where(x => x.Trim().ToLower().Equals("/l") || x.Trim().ToLower().Equals("-l")).Select(x => x);

			try
			{
				Console.Clear();
			}
			catch (Exception)
			{
				//don't care right now
			}

			if (!showhelp.Count().Equals(0) || args.Count().Equals(0))
			{
				//ShowHelp(true);
				ShowHelp(false);
				return 1;
			}
			
			if (showlist.Any())
			{
				ShowFileExtensionList();
				return 1;
			}
			
			var dirname = args[0].Trim();
			if (!IsDirectoryValid(dirname)) 
			{
				ShowQuitEarly(string.Format("{0} invalid", dirname));
				ShowHelp(false);
				return 1;
			}
			
			var extensionConversion = BuildDictionaries();
			var extensionConversionReverse = BuildDictionaries(true);
			//extensionConversion.ConsoleWriteLine();
			//extensionConversionReverse.ConsoleWriteLine();
			var keylist = extensionConversion.Keys.ToList();
			try
			{
				var fi = myHelpers.GetFilesByExtensions(new DirectoryInfo(dirname), SearchOption.AllDirectories,keylist.ToArray());
				if (fi.Any())
				{
					//fi.Select(x => x.FullName).ConsoleWriteLine();
					RenameFile(fi,extensionConversion);
				}
				else
				{
					keylist = extensionConversionReverse.Keys.ToList();
					fi = myHelpers.GetFilesByExtensions(new DirectoryInfo(dirname), SearchOption.AllDirectories,keylist.ToArray());
					//fi.Select(x => x.FullName).ConsoleWriteLine();
					RenameFile(fi,extensionConversionReverse);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(string.Format("Error: {0}", ex.Message));
				return 1;
			}
			return 0;
        }

		private static void ShowFileExtensionList()
		{
			var dict = BuildDictionaries();
			var maxkeysize = dict.Keys.ToList().Aggregate("", (max, current) => max.Length > current.Length ? max : current).Length;
			var maxvaluesize  = dict.Select(x => x.Value).ToList().Aggregate("", (max, current) => max.Length > current.Length ? max : current).Length;
			var fmt =  "{0,-maxkeysize} ==> {1,-maxvaluesize} ==> {0,-maxkeysize}".Replace("maxkeysize",maxkeysize.ToString()).Replace("maxvaluesize",maxvaluesize.ToString());
			dict.Select(x => string.Format(fmt,x.Key, x.Value)).ToList().ConsoleWriteLine();
		}
		
		private static void RenameFile(IEnumerable<FileInfo> files, Dictionary<string, string> extensions)
		{
			try
			{
				foreach(var f in files)
				{
					if (extensions.ContainsKey(f.Extension))
					{
						//Console.WriteLine("from:" + f.FullName);
						var dir = Path.GetDirectoryName(f.FullName);
						var fn = Path.GetFileNameWithoutExtension(f.FullName);
						var newname = Path.Combine(dir,fn + extensions[f.Extension]);
						//Console.WriteLine("to  :" + newname);
						File.Move(f.FullName,newname);
						//Console.WriteLine("");
					}
				}
			}
			catch
			{
				//don't care
			}
		}
			
		private static Dictionary<string, string> BuildDictionaries(bool reverse=false)
		{
			List<string> ext = new List<string>();	
			ext.Add("ade");
			ext.Add("adp");
			ext.Add("bat");
			ext.Add("chm");
			ext.Add("cmd");
			ext.Add("com");
			ext.Add("cpl");
			ext.Add("exe");
			ext.Add("hta");
			ext.Add("ins");
			ext.Add("isp");
			ext.Add("jse");
			ext.Add("lib");
			ext.Add("lnk");
			ext.Add("mde");
			ext.Add("msc");
			ext.Add("msp");
			ext.Add("mst");
			ext.Add("pif");
			ext.Add("scr");
			ext.Add("sct");
			ext.Add("shb");
			ext.Add("sys");
			ext.Add("vb");
			ext.Add("vbe");
			ext.Add("vbs");
			ext.Add("vxd");
			ext.Add("wsc");
			ext.Add("wsf");
			ext.Add("wsh");

			Dictionary<string, string> dict = new Dictionary<string, string>();
			foreach(var s in ext)
			{
				dict.Add(string.Format(".{0}",s.Trim()),string.Format(".{0}", string.Join("_", s.Trim().ToCharArray().ToList())));
			}

			if (!reverse) return dict;
			
			List<string> keylist = new List<string>(dict.Keys);

			Dictionary<string, string> dictReverse = new Dictionary<string, string>();
			foreach (KeyValuePair<string, string> pair in dict)
			{
				dictReverse.Add(pair.Value,pair.Key);
			}
			
			return dictReverse;
		}
		
		private static bool IsDirectoryValid(string foldername)
		{
			if (string.IsNullOrEmpty(foldername)) return false;
			
			try
			{
				if(Directory.Exists(foldername)) 
				{
					return true;
				}
			}
			catch (Exception ex)
			{
				var dontreallycare = ex.Message;
			}
			return false;
		}
		 
		private static void ShowQuitEarly(string msg)
		{
            var sep = new String('*', 70);
            Console.WriteLine(string.Format("{0}", sep));
            Console.WriteLine(string.Format("{0}", msg));
            Console.WriteLine(string.Format("{0}", sep));
		}
		 
        private static void ShowHelp(bool withpause)
        {
            string fullName = Assembly.GetEntryAssembly().Location;
            string myName = Path.GetFileName(Path.GetFileName(Assembly.GetEntryAssembly().Location));
            var sep = new String('*', 70);
            Console.WriteLine(string.Format("{0}", sep));
            Console.WriteLine(string.Format("{0} Last built:{1}", myName, RetrieveLinkerTimestamp()));
            Console.WriteLine(string.Format("{0}", sep));
            Console.WriteLine(string.Format("Usage:"));
            Console.WriteLine(string.Format("{0} \"{1}\"", myName,"Fully-qualified-folder-to-process"));
			Console.WriteLine(string.Format("{0} /l (to list all the extensions)", myName));

            Console.WriteLine(string.Format("{0}", sep));
            if (withpause)
            {
                Console.WriteLine("Press any key to continue.({0})", System.DateTime.Now);
                Console.ReadKey();
            }
        }

		/*
			see http://stackoverflow.com/questions/1600962/displaying-the-build-date for this code
		*/
        private static DateTime RetrieveLinkerTimestamp()
        {
            string filePath = System.Reflection.Assembly.GetCallingAssembly().Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            byte[] b = new byte[2048];
            System.IO.Stream s = null;

            try
            {
                s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }

            int i = System.BitConverter.ToInt32(b, c_PeHeaderOffset);
            int secondsSince1970 = System.BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
            return dt;
        }
    }
} //ends namespace
public static class myHelpers
{
	/// <summary>
	/// GetFilesByExtensions
	/// </summary>
	/// <param name="dir"></param>
	/// <param name="searchOption"></param>
	/// <param name="extensions"></param>
	/// <returns></returns>
	public static IEnumerable<FileInfo> GetFilesByExtensions(DirectoryInfo dir, System.IO.SearchOption searchOption, params string[] extensions)
	{
		if (extensions == null)
			throw new ArgumentNullException("extensions");
		IEnumerable<FileInfo> files = dir.EnumerateFiles("*", searchOption);
		//Console.WriteLine("size of filelist={0}",files.Count());
		var ext = files.Select(x => x.Extension).ToList().Distinct();
		//ext.ConsoleWriteLine();
		return files.Where(f => extensions.Any(y => y.Equals(f.Extension, StringComparison.InvariantCultureIgnoreCase))).Select(x => x);
	}

	/*
		Dump a collection to the console window
	*/
	public static void ConsoleWriteLine<T>(this System.Collections.Generic.IEnumerable<T> inlist)
	{
		foreach (var i in inlist)
		{
			Console.WriteLine(i);
		}
	}
} //ends namespace
