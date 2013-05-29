
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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Peach.Core.Dom;
using Peach;
using Peach.Core.IO;
using System.Reflection;
using NLog;
using Action = Peach.Core.Dom.Action;
using System.Net;

namespace Peach.Core
{
	/// <summary>
	/// Publishers are I/O interfaces for Peach.  They glue the actions
	/// in a state model to the target interface.  Publishers can be 
	/// stream based such as files or sockets, and also call based like
	/// COM and shared libraries.  They can also be hybrids using both
	/// stream and call based methods to make more complex publishers.
	/// 
	/// Multiple publishers can be used in a single state model to allow
	/// for more complex opertions such as writeing to the registry and
	/// then calling an RPC method.
	/// </summary>
	public abstract class Publisher : Stream
	{
		protected abstract NLog.Logger Logger { get; }

		#region Private Members

		[NonSerialized]
		private Test _test;
		private bool _hasStarted;
		private bool _isOpen;
		private uint _iteration;
		private bool _isControlIteration;
		private string _result;

		#endregion

		#region Properties

		/// <summary>
		/// The top level test object.
		/// </summary>
		public virtual Test Test
		{
			get { return _test; }
			set { _test = value; }
		}

		/// <summary>
		/// Gets/sets the current fuzzing iteration.
		/// </summary>
		public virtual uint Iteration
		{
			get { return _iteration; }
			set { _iteration = value; }
		}

		/// <summary>
		/// Gets/sets if the current iteration is a control iteration.
		/// </summary>
		public virtual bool IsControlIteration
		{
			get { return _isControlIteration; }
			set { _isControlIteration = value; }
		}

		/// <summary>
		/// Get the result value (if any).
		/// </summary>
		public virtual string Result
		{
			get { return _result; }
			set { _result = value; }
		}

		#endregion

		#region Implementation Functions

		/// <summary>
		/// Called when the publisher is started.  This method will be called
		/// once per fuzzing "Session", not on every iteration.
		/// </summary>
		protected virtual void OnStart()
		{
		}

		/// <summary>
		/// Called when the publisher is stopped.  This method will be called
		/// once per fuzzing "Session", not on every iteration.
		/// </summary>
		protected virtual void OnStop()
		{
		}

		/// <summary>
		/// Open or connect to a resource.  Will be called
		/// automatically if not called specifically.
		/// </summary>
		protected virtual void OnOpen()
		{
		}

		/// <summary>
		/// Close a resource.  Will be called automatically when
		/// state model exists.  Can also be called explicitly when
		/// needed.
		/// </summary>
		protected virtual void OnClose()
		{
		}

		/// <summary>
		/// Accept an incoming connection.
		/// </summary>
		protected virtual void OnAccept()
		{
			throw new PeachException("Error, action 'accept' not supported by publisher");
		}

		/// <summary>
		/// Call a method on the Publishers resource
		/// </summary>
		/// <param name="method">Name of method to call</param>
		/// <param name="args">Arguments to pass</param>
		/// <returns>Returns resulting data</returns>
		protected virtual Variant OnCall(string method, List<ActionParameter> args)
		{
			throw new PeachException("Error, action 'call' not supported by publisher");
		}

		/// <summary>
		/// Set a property on the Publishers resource.
		/// </summary>
		/// <param name="property">Name of property to set</param>
		/// <param name="value">Value to set on property</param>
		protected virtual void OnSetProperty(string property, Variant value)
		{
			throw new PeachException("Error, action 'setProperty' not supported by publisher");
		}

		/// <summary>
		/// Get value of a property exposed by Publishers resource
		/// </summary>
		/// <param name="property">Name of property</param>
		/// <returns>Returns value of property</returns>
		protected virtual Variant OnGetProperty(string property)
		{
			throw new PeachException("Error, action 'getProperty' not supported by publisher");
		}

		/// <summary>
		/// Send data
		/// </summary>
		/// <param name="buffer">Data to send/write</param>
		/// <param name="offset">The byte offset in buffer at which to begin writing from.</param>
		/// <param name="count">The maximum number of bytes to write.</param>
		protected virtual void OnOutput(byte[] buffer, int offset, int count)
		{
			throw new PeachException("Error, action 'output' not supported by publisher");
		}

		/// <summary>
		/// Read data
		/// </summary>
		protected virtual void OnInput()
		{
			throw new PeachException("Error, action 'input' not supported by publisher");
		}

		#endregion

		#region Ctor

		public Publisher(Dictionary<string, Variant> args)
		{
			ParameterParser.Parse(this, args);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Called to Start publisher.  This action is always performed
		/// even if not specifically called.  This method will be called
		/// once per fuzzing "Session", not on every iteration.
		/// </summary>
		public void start()
		{
			if (_hasStarted)
				return;

			Logger.Debug("start()");
			OnStart();

			_hasStarted = true;
		}

		/// <summary>
		/// Called to Stop publisher.  This action is always performed
		/// even if not specifically called.  This method will be called
		/// once per fuzzing "Session", not on every iteration.
		/// </summary>
		public void stop()
		{
			if (!_hasStarted)
				return;

			Logger.Debug("stop()");
			OnStop();

			_hasStarted = false;
		}

		/// <summary>
		/// Accept an incoming connection.
		/// </summary>
		public void accept()
		{
			Logger.Debug("accept()");
			OnAccept();
		}

		/// <summary>
		/// Open or connect to a resource.  Will be called
		/// automatically if not called specifically.
		/// </summary>
		public void open()
		{
			if (_isOpen)
				return;

			Logger.Debug("open()");
			OnOpen();

			_isOpen = true;
		}

		/// <summary>
		/// Close a resource.  Will be called automatically when
		/// state model exists.  Can also be called explicitly when
		/// needed.
		/// </summary>
		public void close()
		{
			if (!_isOpen)
				return;

			Logger.Debug("close()");
			OnClose();

			_isOpen = false;
		}

		/// <summary>
		/// Call a method on the Publishers resource
		/// </summary>
		/// <param name="method">Name of method to call</param>
		/// <param name="args">Arguments to pass</param>
		/// <returns>Returns resulting data</returns>
		public Variant call(string method, List<ActionParameter> args)
		{
			Logger.Debug("call({0}, {1})", method, args);
			return OnCall(method, args);
		}

		/// <summary>
		/// Set a property on the Publishers resource.
		/// </summary>
		/// <param name="property">Name of property to set</param>
		/// <param name="value">Value to set on property</param>
		public void setProperty(string property, Variant value)
		{
			Logger.Debug("setProperty({0}, {1})", property, value);
			OnSetProperty(property, value);
		}

		/// <summary>
		/// Get value of a property exposed by Publishers resource
		/// </summary>
		/// <param name="property">Name of property</param>
		/// <returns>Returns value of property</returns>
		public Variant getProperty(string property)
		{
			Logger.Debug("getProperty({0})", property);
			return OnGetProperty(property);
		}

		/// <summary>
		/// Send data
		/// </summary>
		/// <param name="buffer">Data to send/write</param>
		/// <param name="offset">The byte offset in buffer at which to begin writing from.</param>
		/// <param name="count">The maximum number of bytes to write.</param>
		public void output(byte[] buffer, int offset, int count)
		{
			Logger.Debug("output({0} bytes)", count);
			OnOutput(buffer, offset, count);
		}

		/// <summary>
		/// Read data
		/// </summary>
		public void input()
		{
			Logger.Debug("input()");
			OnInput();
		}

		/// <summary>
		/// Blocking stream based publishers override this to wait
		/// for a certian amount of bytes to be available for reading.
		/// </summary>
		/// <param name="count">The requested byte count</param>
		public virtual void WantBytes(long count)
		{
		}

		#endregion

		#region Stream

		public override bool CanRead
		{
			get { return false; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return false; }
		}

		public override void Flush()
		{
			throw new NotSupportedException();
		}

		public override long Length
		{
			get { throw new NotSupportedException(); }
		}

		public override long Position
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		#endregion
	}

	/// <summary>
	/// Used to indicate a class is a valid Publisher and 
	/// provide it's invoking name used in the Pit XML file.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class PublisherAttribute : PluginAttribute
	{
		public PublisherAttribute(string name, bool isDefault = false)
			: base(typeof(Publisher), name, isDefault)
		{
		}
	}
}

// END
