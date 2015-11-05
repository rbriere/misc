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
 
namespace NewGuid
{
    class Program
    {
		//To compile:
		//"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"  /out:C:\utils\bin\NewGuid.exe  C:\utils\cs\NewGuid.cs 
		// Note: Update locations for your machine
		//
		//To Run:
		//NewGuid  
		//
		//To use in a Windows cmd/batch file:
		//@echo off
		//set guid=
		//for /f %%i in ('newguid') do set guid=%%i
		//echo Result is %guid%
		//	
		//rem  Make a location were we can test - delete this folder later
		//call mkdir "c:\somewhere\newguidTest"
		//for /f %%i in ('newguid') do set guid=%%i
		//call mkdir "c:\somewhere\newguidTest\%guid%"
                                
        [STAThread]
		static int Main(string[] args)
		{
			Console.Write(Guid.NewGuid());
			return 0;
		}
	}
}
