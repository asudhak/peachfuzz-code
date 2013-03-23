
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peach.Core.Publishers.Com
{
	public interface IComContainer
	{
		/// <summary>
		/// Initialize COM control
		/// </summary>
		/// <param name="control">Control name or CLSID</param>
		void Intialize(string control);

		/// <summary>
		/// Shutdown ComCOntainer process.
		/// </summary>
		void Shutdown();

		/// <summary>
		/// Call method on COM control
		/// </summary>
		/// <param name="method">Name of method</param>
		/// <param name="args">Arguments to pass</param>
		/// <returns>Returns result if any</returns>
		object CallMethod(string method, object[] args);

		/// <summary>
		/// Get Property value
		/// </summary>
		/// <param name="property">Name of property</param>
		/// <returns>Returns property value or null.</returns>
		object GetProperty(string property);

		/// <summary>
		/// Set property value
		/// </summary>
		/// <param name="property">Name of property</param>
		/// <param name="value">Value to set</param>
		void SetProperty(string property, object value);
	}
}
