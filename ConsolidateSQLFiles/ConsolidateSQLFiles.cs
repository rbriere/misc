/*
The MIT License (MIT)

Copyright (c) 2015 RBriere

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
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ConsolidateSQLFiles
{
	/*
		Consolidate SQL files into one file
	*/
    class ConsolidateSQLFiles
    {
		//To compile:
		//"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"  /out:C:\utils\bin\csf.exe  C:\Programming\cs\ConsolidateSQLFiles.cs 
		//      
		//Change the location of the csc.exe to fit your machine.
		//Change the location of the compiled executable to your location.
		//Change the location of where this file resides to your location.

		//To Run:
		//csf give-it-a-fully-qualified-folder  
		//
        private static int displayLineSize = 80;
        private static string outputFilename = "";
        private static bool outputFilenamedSQL = false;

		//Set DoPatternReplacement to true if you want to do some replacement on each line. See LoadRegex for the patterns
		private static bool DoPatternReplacement = true; //Note: Since I do this all the time I decided to not make this an input parm.
        private static List<Regex> PatternsToProcessAlter = new List<Regex>();
        private static List<string> ReplacementsAlter = new List<string>();

        static void Main(string[] args)
        {
			Console.Clear();
            if (args.Length == 0 || args.Length > 2)
            {
                HelpText();
                return;
            }

            outputFilenamedSQL = true;
            if (args.Length == 2)
            {
                outputFilenamedSQL = !args[1].Trim().ToLower().Equals("-t");
            }

            var tfolder = args[0].ToString().Replace('\'', ' ').Replace('\"', ' ').Trim();
            if (tfolder.StartsWith("/") || tfolder.StartsWith("?") || tfolder.StartsWith("-"))
            {
                HelpText();
                return;
            }
                
			if (Directory.Exists(tfolder))
			{
				LoadRegex();
				DirectoryInfo di = new DirectoryInfo(tfolder);
				string newFolderName = string.Format("{0}{1}{2}", di.ToString(), Path.DirectorySeparatorChar, Guid.NewGuid().ToString("N"));
				DirectoryInfo diNew = Directory.CreateDirectory(newFolderName);
				outputFilename = BuildOutputFile(di);
				FileInfo[] fi = di.GetFiles("*.sql");
				StringBuilder line = new StringBuilder();
				StringBuilder newfileData = new StringBuilder();
				if (fi.Count() > 0)
				{
					TextWriter tw = new StreamWriter(outputFilename); //Consolidated file
					var orderedFiles = fi.OrderBy(f => f.FullName);
					
					foreach (FileInfo f in orderedFiles)
					{
						TextWriter twNew = new StreamWriter(Path.Combine(newFolderName, f.Name)); //Modified version of original file
						List<int[]> thelist = new List<int[]>();
						decimal linecount = 0;
						using (StreamReader reader = File.OpenText(f.FullName))
						{
							while (!reader.EndOfStream)
							{
								line.Length = 0;
								line.Append(DoPatternReplacement ? ReformatLine(reader.ReadLine()) : reader.ReadLine());
								tw.WriteLine(line.ToString());
								twNew.WriteLine(line.ToString());
								linecount++;
							}
							reader.Close();
						}
						tw.WriteLine("GO");  
						twNew.Close();  
					}

					tw.Close();
					//show the consolidated file name
					Console.WriteLine(outputFilename);
				}
			}
			else
			{
				Console.WriteLine("Invalid folder:\n[{0}]", args[0].ToString().Trim());
			}
	
        } //ends main

		/*
			Load up a regex pattern and the replacement text into two separate containers.  
			Order will matter if you have multiple patterns with similar text. 

			This might be useful if you are going to run some of the SQL locally 
			where you may not have an ARCHIVE or DATA storage location (as an example).
		*/
        private static void LoadRegex()
        {
            string p = string.Empty;
            string r = string.Empty;

            p = @" ON \[DATA\]";
            r = "";
            PatternsToProcessAlter.Add(new Regex(p, RegexOptions.IgnoreCase));
            ReplacementsAlter.Add(r);

            p = @" ON \[PRIMARY\]";
            r = "";
            PatternsToProcessAlter.Add(new Regex(p, RegexOptions.IgnoreCase));
            ReplacementsAlter.Add(r);

            p = @" ON \[ARCHIVE\]";
            r = "";
            PatternsToProcessAlter.Add(new Regex(p, RegexOptions.IgnoreCase));
            ReplacementsAlter.Add(r);

            p = @" TEXTIMAGE_ON \[DATA\]";
            r = "";
            PatternsToProcessAlter.Add(new Regex(p, RegexOptions.IgnoreCase));
            ReplacementsAlter.Add(r);

            p = " ALTER AUTHORIZATION";
            r = "--ALTER AUTHORIZATION";
            PatternsToProcessAlter.Add(new Regex(p, RegexOptions.IgnoreCase));
            ReplacementsAlter.Add(r);
        }

		/*
			Alter the string using the regex patterns set previously.
		*/
        private static string ReformatLine(string s)
        {
			if (!DoPatternReplacement) return s;
            StringBuilder line = new StringBuilder();
            StringBuilder temp = new StringBuilder();
            int regexcounter = 0;
            line.Append(s);
            regexcounter = 0;

            foreach (Regex re in PatternsToProcessAlter)
            {
                if (re.IsMatch(s))
                {
                    temp.Length = 0;
                    temp.Append(Regex.Replace(line.ToString(), re.ToString(), ReplacementsAlter[regexcounter], re.Options));
                    line.Length = 0;
                    line.Append(temp);
                }
                regexcounter++;
            }

            return line.ToString();
        }

        private static string BuildOutputFile(DirectoryInfo folder)
        {
            return Path.Combine(folder.FullName, string.Format("{0}_{1}.{2}", System.DateTime.Now.ToString("yyyyMMdd_hhmmss"), Guid.NewGuid().ToString("N"), outputFilenamedSQL ? "sql" : "txt"));
        } 

        private static void ShowVersion()
        {
			Version version = new Version(1, 0, 0, 0); //Assembly.GetExecutingAssembly().GetName().Version.ToString() works if you put this in a solution
            List<string> outlines = new List<string>();
            int linelen = displayLineSize;
            string v = string.Format(" {0} Version: {1} {2} ", Path.GetFileNameWithoutExtension(Assembly.GetCallingAssembly().Location), version, RetrieveLinkerTimestamp());
            string tagline = (new string('*', (linelen / 2) - (v.Length / 2)))
                + v
                + (new string('*', (linelen / 2) - (v.Length / 2) - ((v.Length % 2) == 0 ? 0 : 1)))
               ;
            tagline += new string('*', linelen - tagline.Length);
            outlines.Add(tagline);
            foreach (string s in outlines)
                Console.WriteLine(s);
        }

        private static void HelpText()
        {
            ShowVersion();
            List<string> outlines = new List<string>();
            int linelen = displayLineSize;
            //outlines.Add("");

			string fullName = Assembly.GetEntryAssembly().Location;
			string myName = Path.GetFileName(Path.GetFileName(Assembly.GetEntryAssembly().Location));
			var sep = new String('*', 70);
			outlines.Add("Consolidate SQL files in a folder into one SQL file.");
            outlines.Add("");
			outlines.Add(string.Format("{0} path [-t]", Path.GetFileNameWithoutExtension(Assembly.GetCallingAssembly().Location)));
            outlines.Add("where path is the fully qualified path to your SQL files");
			outlines.Add("[-t] sets the output file extension to .txt");
            outlines.Add("");
            outlines.Add("All *.sql files in the path will be processed, so you might want ");
            outlines.Add("to clear out any that you do not want included in the new consolidated file.");
            outlines.Add("");
            outlines.Add("The consolidate SQL file will be in the same location as the input folder.");
            outlines.Add("");
            foreach (string s in outlines)
                Console.WriteLine(s);
            ShowVersion();
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
			DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			dt = dt.AddSeconds(secondsSince1970);
			dt = dt.ToLocalTime();
			return dt;
		}
    } //ends class
} //ends namespace
