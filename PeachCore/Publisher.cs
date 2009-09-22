
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

namespace PeachCore
{
	public abstract class Publisher
	{
		/// <summary>
		/// Static method that provides expected and optional
		/// arguments, along with a description.
		/// </summary>
		/// <returns></returns>
		public virtual static Dictionary<string,string> getArguments()
		{
			return new Dictionary<string, string>();
		}

		public Publisher(Dictionary<string, Variant> args)
		{
		}

		/// <summary>
		/// Called to Start publisher.  This action is always performed
		/// even if not specifically called.
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		public virtual void start(Action action)
		{
		}
		/// <summary>
		/// Called to Stop publisher.  This action is always performed
		/// even if not specifically called.
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		public virtual void stop(Action action)
		{
		}

		/// <summary>
		/// Accept an incoming connection.
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		public virtual void accept(Action action)
		{
			throw new PeachException("Error, action 'accept' not supported by publisher");
		}
		/// <summary>
		/// Open or connect to a resource.  Will be called
		/// automatically if not called specifically.
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		public virtual void open(Action action)
		{
			throw new PeachException("Error, action 'open' not supported by publisher");
		}
		/// <summary>
		/// Close a resource.  Will be called automatically when
		/// state model exists.  Can also be called explicitly when
		/// needed.
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		public virtual void close(Action action)
		{
			throw new PeachException("Error, action 'close' not supported by publisher");
		}

		public virtual Variant input(Action action)
		{
			throw new PeachException("Error, action 'input' not supported by publisher");
		}
		public virtual Variant input(Action action, int size)
		{
			throw new PeachException("Error, action 'input' not supported by publisher");
		}
		public virtual void output(Action action, Variant data)
		{
			throw new PeachException("Error, action 'output' not supported by publisher");
		}

		public virtual Variant call(Action action, string method, Dictionary<string, Variant> args )
		{
			throw new PeachException("Error, action 'call' not supported by publisher");
		}
		public virtual void setProperty(Action action, string property, Variant value)
		{
			throw new PeachException("Error, action 'setProperty' not supported by publisher");
		}
		public virtual Variant getProperty(Action action, string property)
		{
			throw new PeachException("Error, action 'getProperty' not supported by publisher");
		}
	}

	/// <summary>
	/// Used to indicate a class is a valid Publisher and 
	/// provide it's invoking name used in the Pit XML file.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class PublisherAttribute : Attribute
	{
		public string invokeName;

		public PublisherAttribute(string invokeName)
		{
			this.invokeName = invokeName;
		}
	}
}

// END
