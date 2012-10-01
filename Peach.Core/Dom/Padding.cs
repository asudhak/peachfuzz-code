
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
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;

using Peach.Core.Analyzers;
using Peach.Core.IO;
using Peach.Core.Cracker;

using NLog;

namespace Peach.Core.Dom
{
	/// <summary>
	/// Providing padding bytes to a DataElementContainer.
	/// </summary>
	[DataElement("Padding")]
	[PitParsable("Padding")]
	[DataElementChildSupportedAttribute(DataElementTypes.NonDataElements)]
  [ParameterAttribute("name", typeof(string), "", true)]
  [ParameterAttribute("aligned", typeof(bool), "Align parent to 8 byte boundry", false)]
	[ParameterAttribute("alignment", typeof(int), "Align to this byte boundry (e.g. 8, 16, etc.)", false)]
	[ParameterAttribute("alignedTo", typeof(DataElement), "Name of element to base our padding on (default is parent)", false)]
	[ParameterAttribute("lengthCalc", typeof(string), "Length calculation", false)]
	[Serializable]
	public class Padding : DataElement
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		bool _aligned = false;
		int _alignment = 8;
		DataElement _alignedTo = null;

		/// <summary>
		/// Create a padding element.
		/// </summary>
		public Padding()
		{
			_defaultValue = new Variant(new byte[] { });
		}

		/// <summary>
		/// Create a padding element.
		/// </summary>
		/// <param name="name">Name of padding element</param>
		/// <param name="aligned">Align data to a byte boundry</param>
		/// <param name="alignment">Byte boundry for alignment (8, 16, etc)</param>
		/// <param name="alignedTo">Align to another element (default is parent)</param>
		public Padding(string name, bool aligned = true, int alignment = 8, DataElement alignedTo = null)
			: base(name)
		{
			this._aligned = aligned;
			this._alignment = alignment;
			this._alignedTo = alignedTo;

			_defaultValue = new Variant(new byte[] { });
		}

		/// <summary>
		/// Create a padding element.
		/// </summary>
		/// <param name="aligned">Align data to a byte boundry</param>
		/// <param name="alignment">Byte boundry for alignment (8, 16, etc)</param>
		/// <param name="alignedTo">Align to another element (default is parent)</param>
		public Padding(bool aligned = true, int alignment = 8, DataElement alignedTo = null)
		{
			this._aligned = aligned;
			this._alignment = alignment;
			this._alignedTo = alignedTo;

			_defaultValue = new Variant(new byte[] { });
		}

		/// <summary>
		/// Create a padding element.
		/// </summary>
		/// <param name="lengthCalc">Scripting expression that calculates pad amount</param>
		public Padding(string lengthCalc)
		{
			this.lengthCalc = lengthCalc;
			_defaultValue = new Variant(new byte[] { });
		}

		public override void Crack(DataCracker context, BitStream data)
		{
			Padding element = this;

			logger.Trace("Crack: {0} data.TellBits: {1}", element.fullName, data.TellBits());

			// Length in bits
			long paddingLength = element.Value.LengthBits;

			if ((data.TellBits() + paddingLength) > data.LengthBits)
				throw new CrackingFailure("Placement '" + element.fullName +
					"' has length of '" + paddingLength + "' bits but buffer only has '" +
					(data.LengthBits - data.TellBits()) + "' bits left.", element, data);

			data.SeekBits(paddingLength, System.IO.SeekOrigin.Current);
		}

		public static DataElement PitParser(PitParser context, XmlNode node, DataElementContainer parent)
		{
			if (node.Name != "Padding")
				return null;

			var padding = DataElement.Generate<Padding>(node);

			padding.aligned = node.getAttributeBool("aligned", false);

			if (node.hasAttribute("alignment"))
				padding.alignment = int.Parse(node.getAttribute("alignment"));

			string strTo = node.getAttribute("alignedTo");
			if (strTo != null)
			{
				padding.alignedTo = parent.find(strTo);
				if (padding.alignedTo == null)
					throw new PeachException("Error, unable to resolve alignedTo '" + strTo + "'.");
			}

			context.handleCommonDataElementAttributes(node, padding);
			context.handleCommonDataElementChildren(node, padding);

			return padding;
		}

		/// <summary>
		/// Align data to a specified byte boundry
		/// </summary>
		public virtual bool aligned
		{
			get { return _aligned; }
			set
			{
				_aligned = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Byte alignment (8, 16, etc).
		/// </summary>
		public virtual int alignment
		{
			get { return _alignment; }
			set
			{
				_alignment = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Element to pull size to align.  If null use parent.
		/// </summary>
		public virtual DataElement alignedTo
		{
			get { return _alignedTo; }
			set
			{
				if (_alignedTo != null)
					_alignedTo.Invalidated -= _alignedTo_Invalidated;

				_alignedTo = value;
				_alignedTo.Invalidated += new InvalidatedEventHandler(_alignedTo_Invalidated);

				Invalidate();
			}
		}

		void _alignedTo_Invalidated(object sender, EventArgs e)
		{
			Invalidate();
		}

		bool _inDefaultValue = false;

		public override Variant DefaultValue
		{
			get
			{
				if (_inDefaultValue)
					return new Variant(new byte[] { });

				// Prevent recursion
				_inDefaultValue = true;

				try
				{
					DataElement alignedElement = parent;
					if (_alignedTo != null)
						alignedElement = _alignedTo;

					if (_aligned)
					{
						long currentLength = alignedElement.Value.LengthBits;

						if (currentLength > 0 && currentLength % _alignment == 0)
							return _defaultValue;

						BitStream data = new BitStream();
						data.WriteBit(0);

						while (((currentLength + data.LengthBits) % _alignment) != 0)
							data.WriteBit(0);

						data.SeekBits(0, System.IO.SeekOrigin.Begin);

						return new Variant(data);
					}
					else
					{
						// Otherwise do some scripting foo!
						Dictionary<string, object> state = new Dictionary<string, object>();
						state["alignedTo"] = alignedElement;
						state["self"] = this._parent;

						object value = Scripting.EvalExpression(_lengthCalc, state);
						long paddingLength = Convert.ToInt64(value);

						BitStream data = new BitStream();
						for (long i = 0; i < paddingLength; i++)
							data.WriteBit(0);

						data.SeekBits(0, System.IO.SeekOrigin.Begin);
						return new Variant(data);
					}
				}
				finally
				{
					_inDefaultValue = false;
				}
			}

			set
			{
				throw new InvalidOperationException("DefaultValue cannot be set on Padding element!");
			}
		}


    public override object GetParameter(string parameterName)
    {
      switch (parameterName)
      {
        case "name":
          return this.name;
        case "aligned":
          return this.aligned;
        case "alignment":
          return this.alignment;
        case "alignedTo":
          return this.alignedTo.name;
        case "lengthCalc":
          return this.lengthCalc;
        default:
          throw new PeachException(System.String.Format("Parameter '{0}' does not exist in Peach.Core.Dom.Padding", parameterName));
      }
    }
	}
}

// end
