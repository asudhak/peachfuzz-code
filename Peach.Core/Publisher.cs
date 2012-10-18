
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
	public delegate void StartEventHandler(Publisher publisher, Action action);
	public delegate void StopEventHandler(Publisher publisher, Action action);
	public delegate void AcceptEventHandler(Publisher publisher, Action action);
	public delegate void OpenEventHandler(Publisher publisher, Action action);
	public delegate void CloseEventHandler(Publisher publisher, Action action);
	public delegate void InputEventHandler(Publisher publisher, Action action);
	public delegate void OutputEventHandler(Publisher publisher, Action action, Stream data);
	public delegate void CallEventHandler(Publisher publisher, Action action, string method, List<ActionParameter> aregs);
	public delegate void SetPropertyEventHandler(Publisher publisher, Action action, string property, Variant value);
	public delegate void GetPropertyEventHandler(Publisher publisher, Action action, string property);

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

		#region Properties

		/// <summary>
		/// Gets/sets the current fuzzing iteration.
		/// </summary>
		public uint Iteration { get; set; }

		/// <summary>
		/// Gets a value that indicates whether the publisher has been started.
		/// </summary>
		public bool HasStarted { get; private set; }

		/// <summary>
		/// Gets a value that indicates whether the publisher has been opened.
		/// </summary>
		public bool IsOpen { get; private set; }

		/// <summary>
		/// Gets the most recent action that called the publisher.
		/// </summary>
		public Action CurrentAction { get; private set; }

		#endregion

		#region Events

		public static event StartEventHandler StartEvent;
		public static event StopEventHandler StopEvent;
		public static event AcceptEventHandler AcceptEvent;
		public static event OpenEventHandler OpenEvent;
		public static event CloseEventHandler CloseEvent;
		public static event InputEventHandler InputEvent;
		public static event OutputEventHandler OutputEvent;
		public static event CallEventHandler CallEvent;
		public static event SetPropertyEventHandler SetPropertyEvent;
		public static event GetPropertyEventHandler GetPropertyEvent;

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
		/// <param name="data">Data to send/write</param>
		protected virtual void OnOutput(Stream data)
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
			foreach (var attr in GetType().GetAttributes<ParameterAttribute>(null))
			{
				Variant value;

				if (args.TryGetValue(attr.name, out value))
					ApplyProperty(attr, (string)value);
				else if (!attr.required && attr.defaultVaue != null)
					ApplyProperty(attr, attr.defaultVaue);
				else if (attr.required)
					RaiseError("is missing required parameter '{0}'.", attr.name);
			}
		}

		private void ApplyProperty(ParameterAttribute attr, string value)
		{
			object obj = null;
			try
			{
				if (attr.type == typeof(IPAddress))
					obj = IPAddress.Parse(value);
				else if (attr.type.IsEnum)
					obj = Enum.Parse(attr.type, value);
				else
					obj = Convert.ChangeType(value, attr.type);
			}
			catch (Exception ex)
			{
				RaiseError("could not set parameter '{0}'.  {1}",attr.name, ex.Message);
			}

			var prop = GetType().GetProperty(attr.name, attr.type);
			if (prop != null)
				prop.SetValue(this, obj, null);
			else
				RaiseError("has no public property for parameter '{0}'.",attr.name);
		}

		private void RaiseError(string fmt, params string[] args)
		{
			var attrs = GetType().GetAttributes<PublisherAttribute>(null);
			var pub = attrs.FirstOrDefault(a => a.isDefault == true);
			if (pub == null) pub = attrs.First();

			string msg = string.Format("{0} publisher {1}", pub.Name, string.Format(fmt, args));
			throw new PeachException(msg);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Called to Start publisher.  This action is always performed
		/// even if not specifically called.  This method will be called
		/// once per fuzzing "Session", not on every iteration.
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		public void start(Action action)
		{
			if (HasStarted)
				return;

			CurrentAction = action;

			if (StartEvent != null)
				StartEvent(this, CurrentAction);

			Logger.Debug("start({0})", action.name);
			OnStart();

			HasStarted = true;
		}

		/// <summary>
		/// Called to Stop publisher.  This action is always performed
		/// even if not specifically called.  This method will be called
		/// once per fuzzing "Session", not on every iteration.
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		public void stop(Action action)
		{
			if (!HasStarted)
				return;

			CurrentAction = action;

			if (StopEvent != null)
				StopEvent(this, CurrentAction);

			Logger.Debug("stop({0})", action == null ? "<null>" : action.name);
			OnStop();

			HasStarted = false;
		}

		/// <summary>
		/// Accept an incoming connection.
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		public void accept(Action action)
		{
			CurrentAction = action;

			if (AcceptEvent != null)
				AcceptEvent(this, CurrentAction);

			Logger.Debug("accept({0})", action.name);
			OnAccept();
		}

		/// <summary>
		/// Open or connect to a resource.  Will be called
		/// automatically if not called specifically.
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		public void open(Action action)
		{
			if (IsOpen)
				return;

			CurrentAction = action;

			if (OpenEvent != null)
				OpenEvent(this, CurrentAction);

			Logger.Debug("open({0})", action.name);
			OnOpen();

			IsOpen = true;
		}

		/// <summary>
		/// Close a resource.  Will be called automatically when
		/// state model exists.  Can also be called explicitly when
		/// needed.
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		public void close(Action action)
		{
			if (!IsOpen)
				return;

			CurrentAction = action;

			if (CloseEvent != null)
				CloseEvent(this, CurrentAction);

			Logger.Debug("close({0})", action == null ? "<null>" : action.name);
			OnClose();

			IsOpen = false;
		}

		/// <summary>
		/// Call a method on the Publishers resource
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		/// <param name="method">Name of method to call</param>
		/// <param name="args">Arguments to pass</param>
		/// <returns>Returns resulting data</returns>
		public Variant call(Action action, string method, List<ActionParameter> args)
		{
			CurrentAction = action;

			if (CallEvent != null)
				CallEvent(this, CurrentAction, method, args);

			Logger.Debug("call({0}, {1}, {2})", action.name, method, args);
			return OnCall(method, args);
		}

		/// <summary>
		/// Set a property on the Publishers resource.
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		/// <param name="property">Name of property to set</param>
		/// <param name="value">Value to set on property</param>
		public void setProperty(Action action, string property, Variant value)
		{
			CurrentAction = action;

			if (SetPropertyEvent != null)
				SetPropertyEvent(this, CurrentAction, property, value);

			Logger.Debug("setProperty({0}, {1}, {2})", action.name, property, value);
			OnSetProperty(property, value);
		}

		/// <summary>
		/// Get value of a property exposed by Publishers resource
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		/// <param name="property">Name of property</param>
		/// <returns>Returns value of property</returns>
		public Variant getProperty(Action action, string property)
		{
			CurrentAction = action;

			if (GetPropertyEvent != null)
				GetPropertyEvent(this, CurrentAction, property);

			Logger.Debug("getProperty({0}, {1})", action.name, property);
			return OnGetProperty(property);
		}

		/// <summary>
		/// Send data
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		/// <param name="data">Data to send/write</param>
		public void output(Action action, Stream data)
		{
			CurrentAction = action;

			if (OutputEvent != null)
				OutputEvent(this, CurrentAction, data);

			Logger.Debug("output({0}, {1} bytes)", action.name, data.Length);

			var pos = data.Position;
			data.Seek(0, SeekOrigin.Begin);
			OnOutput(data);
			data.Seek(pos, SeekOrigin.Begin);
		}

		/// <summary>
		/// Read data
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		/// <param name="data">Minimum length of data to read</param>
		public void input(Action action)
		{
			CurrentAction = action;

			if (InputEvent != null)
				InputEvent(this, CurrentAction);

			Logger.Debug("input({0})", action.name);
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
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
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
			throw new NotImplementedException();
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
		public bool isDefault = false;

		public PublisherAttribute(string name, bool isDefault = false)
			: base(name)
		{
			this.isDefault = isDefault;
		}
	}
}

// END
