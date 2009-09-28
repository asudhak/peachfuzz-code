
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
//   Michael Eddington (mike@phed.org)

// $Id$

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeachCore.Dom;

namespace PeachCore
{
	public abstract class Analyzer
	{
		public static bool supportParser = false;
		public static bool supportDataElement = false;
		public static bool supportCommandLine = false;
		public static bool supportTopLevel = false;

		public static Analyzer defaultParser = null;

		/// <summary>
		/// Replaces the parser for fuzzer definition.
		/// </summary>
		/// <param name="args">Command line arguments</param>
		public virtual Dom.Dom asParser(Dictionary<string, string> args, string fileName)
		{
			return null;
		}

		public virtual void asDataElement(DataElement parent, Dictionary<string, string> args, object dataBuffer)
		{
		}

		public virtual void asCommandLine(Dictionary<string, string> args)
		{
		}

		public virtual void asTopLevel(Dom.Dom dom, Dictionary<string, string> args)
		{
		}
	}
}

// end
