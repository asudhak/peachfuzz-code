
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Michael Eddington (mike@dejavusecurity.com)

// $Id$

using System;
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;

using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Analyzers
{
	[Analyzer("Com", true)]
	[Analyzer("ComAnalyzer")]
	[Parameter("TypeLibrary", typeof(string), "Path to a COM type library (sometimes this is the DLL/EXE otherwise a .TLB)")]
	[Serializable]
	public class ComAnalyzer : Analyzer
	{
		string _typeLibrary = null;

		static ComAnalyzer()
		{
			supportParser = false;
			supportDataElement = false;
			supportCommandLine = true;
			supportTopLevel = false;
		}

		public ComAnalyzer()
		{
		}

		public ComAnalyzer(Dictionary<string, Variant> args)
		{
			_typeLibrary = (string)args["TypeLibrary"];
		}

		public override void asCommandLine(Dictionary<string, string> args)
		{
			_typeLibrary = (string)args["TypeLibrary"];
			//_typeLibrary = @"C:\Peach3\ComTest\Release\ComTest.dll";

			if (!File.Exists(_typeLibrary))
				throw new PeachException("Error, the TypeLibrary was not found.");

			ITypeLib typeLib = null;
			int ret = LoadTypeLib(_typeLibrary, out typeLib);
			if (ret != 0)
				throw new PeachException("Error loading TypeLibrary.  LoadTypeLib returned " + ret);

			if (typeLib == null)
				throw new PeachException("Error, LoadTypeLib returned a null ITypeLib interface.");

			string name;
			string doc;
			int helpid;
			string helpfile;
			string []  arrClassification = new string [] { "Enum","Struct","Module","Interface",
					"Dispinterface","Coclass","Typedef","Union"};
	

			typeLib.GetDocumentation(-1, out name, out doc, out helpid, out helpfile);
			Console.WriteLine(name);

			ITypeInfo typeInfo = null;
			for (int cnt = 0; cnt < typeLib.GetTypeInfoCount(); cnt++)
			{
				// http://www.codeguru.com/cpp/com-tech/activex/misc/article.php/c2569

				Console.WriteLine(" ------------- ");

				typeInfo = null;
				typeLib.GetTypeInfo(cnt, out typeInfo);
				if (typeInfo == null)
				{
					Console.WriteLine("typeInfo was null, continue!");
					continue;
				}
				
				typeLib.GetDocumentation(cnt, out name, out doc, out helpid, out helpfile);
				Console.WriteLine("  "+name);

				System.Runtime.InteropServices.ComTypes.TYPEKIND typeKind;
				typeLib.GetTypeInfoType(cnt, out typeKind);

				Console.WriteLine("  " + arrClassification[(int)typeKind]);

				IntPtr ppTypeAttributes;
				typeInfo.GetTypeAttr(out ppTypeAttributes);
				var typeAttributes = (System.Runtime.InteropServices.ComTypes.TYPEATTR)Marshal.PtrToStructure(ppTypeAttributes, typeof(System.Runtime.InteropServices.ComTypes.TYPEATTR));

				for (int cntFuncs = 0; cntFuncs < typeAttributes.cFuncs; cntFuncs++)
				{
					IntPtr ppFuncDesc;
					typeInfo.GetFuncDesc(cntFuncs, out ppFuncDesc);
					var funcDesc = (System.Runtime.InteropServices.ComTypes.FUNCDESC)Marshal.PtrToStructure(ppFuncDesc, typeof(System.Runtime.InteropServices.ComTypes.FUNCDESC));

					int memberID = funcDesc.memid;
					//var elemDesc = funcDesc.elemdescFunc;

					typeInfo.GetDocumentation(memberID, out name, out doc, out helpid, out helpfile);
					Console.WriteLine("    " + name);

					//funcDesc.

					typeInfo.ReleaseFuncDesc(ppFuncDesc);
				}

				for (int cntVars = 0; cntVars < typeAttributes.cVars; cntVars++)
				{
					IntPtr ppVarDesc;
					typeInfo.GetVarDesc(cntVars, out ppVarDesc);
					var varDesc = (System.Runtime.InteropServices.ComTypes.VARDESC)Marshal.PtrToStructure(ppVarDesc, typeof(System.Runtime.InteropServices.ComTypes.VARDESC));

					int memberID = varDesc.memid;

					typeInfo.GetDocumentation(memberID, out name, out doc, out helpid, out helpfile);
					Console.WriteLine("    " + name);

					typeInfo.ReleaseVarDesc(ppVarDesc);
				}

				typeInfo.ReleaseTypeAttr(ppTypeAttributes);
			}
		}

		[DllImport("oleaut32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		static extern int LoadTypeLib(string fileName, out ITypeLib typeLib);
	}
}
