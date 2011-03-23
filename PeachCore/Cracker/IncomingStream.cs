using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Peach.Core.Dom;

namespace Peach.Core.Cracker
{
	public class IncomingStream : Stream
	{
		protected DataElement _currentDataElement;
		protected Dictionary<DataElement, long> _positions = new Dictionary<DataElement, long>();
		protected List<DataElement> _relativeStack = new List<DataElement>();
		protected bool _relativePosition = false;
		protected DataElement _relativeToElement = null;
		protected long? _relativeToPosition = null;

		protected long? _position = null;

		/// <summary>
		/// Have all data from underlying publisher
		/// </summary>
		protected bool _haveAllData = false;

		/// <summary>
		/// Data for stream
		/// </summary>
		protected byte[] data = null;

		/// <summary>
		/// Enable relative stream position.  This is for 
		/// sized block support.  Enabling can be performed
		/// recursively.
		/// </summary>
		/// <param name="element">DataElement to make stream relative from.</param>
		public void EnableRelativePosition(DataElement element)
		{
			_relativePosition = true;
			_relativeToElement = element;
			_relativeToPosition = _positions[element];
			_relativeStack.Add(element);
		}

		/// <summary>
		/// Disable relative stream position.  This is for 
		/// sized block support.
		/// </summary>
		/// <param name="element">DataElement that stream was realtive to.</param>
		public void DisableRelativePosition(DataElement element)
		{
			_relativeStack.Remove(element);
			if (_relativeStack.Count == 0)
			{
				_relativePosition = false;
				_relativeToElement = null;
				_relativeToPosition = null;
			}
			else
			{
				_relativePosition = true;
				_relativeToElement = _relativeStack[_relativeStack.Count - 1];
				_relativeToPosition = _positions[_relativeToElement];
			}
		}

		public virtual DataElement CurrentDataElement
		{
			get { return _currentDataElement; }
			set
			{
				_currentDataElement = value;
				_positions[_currentDataElement] = Position;
			}
		}

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
			get{
				if (_haveAllData)
					return data.Length;

				throw new ApplicationException("unkown length");
			}
		}

		public override long Position
		{
			get
			{
				if (_relativePosition)
					return (long) (_position - _relativeToPosition);

				return (long)_position;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public virtual long RealPosition
		{
			get
			{
				return (long)_position;
			}
			set
			{
				throw new NotImplementedException();
			}
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
	}
}

// end
