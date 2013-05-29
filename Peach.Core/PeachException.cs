
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
using System.Text;
using System.Runtime.Serialization;

namespace Peach.Core
{
	/// <summary>
	/// Unrecoverable error.  Causes Peach to exit with an error
	/// message, but no stack trace.
	/// </summary>
	[Serializable]
	public class PeachException : ApplicationException
	{
		public PeachException(string message)
			: base(message)
		{
		}

		public PeachException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected PeachException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	/// Thrown to cause the Peach Engine to re-run
	/// the same test iteration.
	/// </summary>
	[Serializable]
	public class RedoIterationException : ApplicationException
	{
		protected RedoIterationException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	/// Thrown to stop current iteration and move to next.
	/// </summary>
	[Serializable]
	public class SoftException : ApplicationException
	{
		public SoftException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public SoftException(string message)
			: base(message)
		{
		}

		public SoftException(Exception innerException)
			: base(innerException.Message, innerException)
		{
		}

		protected SoftException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	/// Similar to SoftException but used by state model
	/// path code.
	/// </summary>
	[Serializable]
	public class PathException : ApplicationException
	{
		protected PathException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	/// Thrown when peach catches an exception from an agent.
	/// </summary>
	[Serializable]
	public class AgentException : ApplicationException
	{
		public AgentException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public AgentException(string message)
			: base(message)
		{
		}

		public AgentException(Exception innerException)
			: base(innerException.Message, innerException)
		{
		}

		protected AgentException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}


}

// end
