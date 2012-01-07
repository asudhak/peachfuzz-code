
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
	public abstract class Publisher
	{
		public object parent;

		#region Events

		public static event StartEventHandler Start;
		public static event StopEventHandler Stop;
		public static event AcceptEventHandler Accept;
		public static event OpenEventHandler Open;
		public static event CloseEventHandler Close;
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
			if (Close != null)
				Close(this, action);
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
		/// Called to Start publisher.  This action is always performed
		/// even if not specifically called.
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		public virtual void start(Core.Dom.Action action)
		{
			OnStart(action);
		}
		/// <summary>
		/// Called to Stop publisher.  This action is always performed
		/// even if not specifically called.
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		public virtual void stop(Core.Dom.Action action)
		{
			OnStop(action);
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

		public virtual Variant input(Core.Dom.Action action)
		{
			OnInput(action);
			throw new PeachException("Error, action 'input' not supported by publisher");
		}
		public virtual Variant input(Core.Dom.Action action, int size)
		{
			OnInput(action, size);
			throw new PeachException("Error, action 'input' not supported by publisher");
		}
		public virtual void output(Core.Dom.Action action, Variant data)
		{
			OnOutput(action, data);
			throw new PeachException("Error, action 'output' not supported by publisher");
		}

		public virtual Variant call(Core.Dom.Action action, string method, List<ActionParameter> args)
		{
			OnCall(action, method, args);
			throw new PeachException("Error, action 'call' not supported by publisher");
		}
		public virtual void setProperty(Core.Dom.Action action, string property, Variant value)
		{
			OnSetProperty(action, property, value);
			throw new PeachException("Error, action 'setProperty' not supported by publisher");
		}
		public virtual Variant getProperty(Core.Dom.Action action, string property)
		{
			OnGetProperty(action, property);
			throw new PeachException("Error, action 'getProperty' not supported by publisher");
		}
	}

	/// <summary>
	/// Used to indicate a class is a valid Publisher and 
	/// provide it's invoking name used in the Pit XML file.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
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
