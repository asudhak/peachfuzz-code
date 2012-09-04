
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
using Peach.Core.Dom;
using Peach;

namespace Peach.Core
{
	public delegate void StartEventHandler(Publisher publisher, Core.Dom.Action action);
	public delegate void StopEventHandler(Publisher publisher, Core.Dom.Action action);
	public delegate void AcceptEventHandler(Publisher publisher, Core.Dom.Action action);
	public delegate void OpenEventHandler(Publisher publisher, Core.Dom.Action action);
	public delegate void CloseEventHandler(Publisher publisher, Core.Dom.Action action);
	public delegate void InputEventHandler(Publisher publisher, Core.Dom.Action action, int size);
	public delegate void OutputEventHandler(Publisher publisher, Core.Dom.Action action, Variant data);
	public delegate void CallEventHandler(Publisher publisher, Core.Dom.Action action, string method, List<ActionParameter> aregs);
	public delegate void SetPropertyEventHandler(Publisher publisher, Core.Dom.Action action, string property, Variant value);
	public delegate void GetPropertyEventHandler(Publisher publisher, Core.Dom.Action action, string property);

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
		public object parent;
		public uint Iteration { get; set; }
		public bool HasStarted { get; protected set; }
		public bool IsOpen { get; protected set; }

		#region Events

		public static event StartEventHandler Start;
		public static event StopEventHandler Stop;
		public static event AcceptEventHandler Accept;
		public static event OpenEventHandler Open;
		public static event CloseEventHandler Closing;
		public static event InputEventHandler Input;
		public static event OutputEventHandler Output;
		public static event CallEventHandler Call;
		public static event SetPropertyEventHandler SetProperty;
		public static event GetPropertyEventHandler GetProperty;

		public void OnStart(Core.Dom.Action action)
		{
			if (Start != null)
				Start(this, action);
		}
		public void OnStop(Core.Dom.Action action)
		{
			if (Stop != null)
				Stop(this, action);
		}
		public void OnAccept(Core.Dom.Action action)
		{
			if (Accept != null)
				Accept(this, action);
		}
		public void OnOpen(Core.Dom.Action action)
		{
			if (Open != null)
				Open(this, action);
		}
		public void OnClose(Core.Dom.Action action)
		{
			if (Closing != null)
				Closing(this, action);
		}
		public void OnInput(Core.Dom.Action action)
		{
			if (Input != null)
				Input(this, action, -1);
		}
		public void OnInput(Core.Dom.Action action, int size)
		{
			if (Input != null)
				Input(this, action, size);
		}
		public void OnOutput(Core.Dom.Action action, Variant data)
		{
			if (Output != null)
				Output(this, action, data);
		}
		public void OnCall(Core.Dom.Action action, string method, List<ActionParameter> args)
		{
			if (Call != null)
				Call(this, action, method, args);
		}
		public void OnSetProperty(Core.Dom.Action action, string property, Variant value)
		{
			if (SetProperty != null)
				SetProperty(this, action, property, value);
		}
		public void OnGetProperty(Core.Dom.Action action, string property)
		{
			if (GetProperty != null)
				GetProperty(this, action, property);
		}

		#endregion

		public Publisher(Dictionary<string, Variant> args)
		{
		}

		/// <summary>
		/// The current Action operating on the Publisher.
		/// </summary>
		public virtual Core.Dom.Action currentAction
		{
			get;
			set;
		}

		/// <summary>
		/// Called to Start publisher.  This action is always performed
		/// even if not specifically called.  THis method will be called
		/// once per fuzzing "Session", not on every iteration.
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		public virtual void start(Core.Dom.Action action)
		{
			OnStart(action);
			HasStarted = true;
		}
		/// <summary>
		/// Called to Stop publisher.  This action is always performed
		/// even if not specifically called.  THis method will be called
		/// once per fuzzing "Session", not on every iteration.
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		public virtual void stop(Core.Dom.Action action)
		{
			OnStop(action);
			HasStarted = false;
		}

		/// <summary>
		/// Accept an incoming connection.
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		public virtual void accept(Core.Dom.Action action)
		{
			OnAccept(action);
			throw new PeachException("Error, action 'accept' not supported by publisher");
		}
		/// <summary>
		/// Open or connect to a resource.  Will be called
		/// automatically if not called specifically.
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		public virtual void open(Core.Dom.Action action)
		{
			OnOpen(action);
			throw new PeachException("Error, action 'open' not supported by publisher");
		}
		/// <summary>
		/// Close a resource.  Will be called automatically when
		/// state model exists.  Can also be called explicitly when
		/// needed.
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		public virtual void close(Core.Dom.Action action)
		{
			OnClose(action);
			throw new PeachException("Error, action 'close' not supported by publisher");
		}

		/// <summary>
		/// Call a method on the Publishers resource
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		/// <param name="method">Name of method to call</param>
		/// <param name="args">Arguments to pass</param>
		/// <returns>Returns resulting data</returns>
		public virtual Variant call(Core.Dom.Action action, string method, List<ActionParameter> args)
		{
			OnCall(action, method, args);
			throw new PeachException("Error, action 'call' not supported by publisher");
		}

		/// <summary>
		/// Set a property on the Publishers resource.
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		/// <param name="property">Name of property to set</param>
		/// <param name="value">Value to set on property</param>
		public virtual void setProperty(Core.Dom.Action action, string property, Variant value)
		{
			OnSetProperty(action, property, value);
			throw new PeachException("Error, action 'setProperty' not supported by publisher");
		}

		/// <summary>
		/// Get value of a property exposed by Publishers resource
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		/// <param name="property">Name of property</param>
		/// <returns>Returns value of property</returns>
		public virtual Variant getProperty(Core.Dom.Action action, string property)
		{
			OnGetProperty(action, property);
			throw new PeachException("Error, action 'getProperty' not supported by publisher");
		}

		/// <summary>
		/// Send data
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		/// <param name="data">Data to send/write</param>
		public virtual void output(Core.Dom.Action action, Variant data)
		{
			OnOutput(action, data);
			throw new PeachException("Error, action 'output' not supported by publisher");
		}

		/// <summary>
		/// Called from cracker when we need data.  This allows
		/// us to block until we have enough data.
		/// </summary>
		/// <param name="bytes"></param>
		public virtual void WantBytes(long bytes)
		{
		}

		#region Stream

		public override bool CanRead
		{
			get { throw new NotImplementedException(); }
		}

		public override bool CanSeek
		{
			get { throw new NotImplementedException(); }
		}

		public override bool CanWrite
		{
			get { throw new NotImplementedException(); }
		}

		public override void Flush()
		{
			throw new NotImplementedException();
		}

		public override long Length
		{
			get { throw new NotImplementedException(); }
		}

		public override long Position
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			OnInput(currentAction);

			throw new NotImplementedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			OnOutput(currentAction, new Variant(buffer));

			throw new NotImplementedException();
		}

		#endregion
	}

	/// <summary>
	/// Used to indicate a class is a valid Publisher and 
	/// provide it's invoking name used in the Pit XML file.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class PublisherAttribute : Attribute
	{
		public string invokeName;
		public bool isDefault = false;

		public PublisherAttribute(string invokeName, bool isDefault = false)
		{
			this.invokeName = invokeName;
      this.isDefault = isDefault;
		}
	}
}

// END
