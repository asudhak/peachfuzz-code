
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
//   Mikhail Davidov (sirus@haxsys.net)

// $Id$

using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;


namespace Peach.Core.Fixups
{
    [FixupAttribute("ExpressionFixup", "XOR bytes of data.", true)]
    //checksums.ExpressionFixup is for backwards compatibility with Peach2
    [FixupAttribute("checksums.ExpressionFixup", "XOR bytes of data.")]
    [ParameterAttribute("ref", typeof(DataElement), "Reference to data element", true)]
    [ParameterAttribute("expression", typeof(string), "Expression returning string or int", true)]
    [Serializable]
    public class ExpressionFixup : Fixup
    {
		bool invalidateEvent = false;

        public ExpressionFixup(DataElement parent, Dictionary<string, Variant> args)
            : base(parent, args)
        {
            if (!args.ContainsKey("ref"))
                throw new PeachException("Error, ExpressionFixup requires a 'ref' argument!");

            if (!args.ContainsKey("expression"))
                throw new PeachException("Error, ExpressionFixup requires an 'expression' argument!");
        }

        protected override Variant fixupImpl(DataElement obj)
        {
            string objRef = (string)args["ref"];
            string expression = (string)args["expression"];

            DataElement from = obj.find(objRef);
			if (!invalidateEvent)
			{
				invalidateEvent = true;
				from.Invalidated += new InvalidatedEventHandler(from_Invalidated);
			}

            if (from == null)
                throw new PeachException(string.Format("ExpressionFixup could not find ref element '{0}'", objRef));

            byte[] data = from.Value.Value;

            Dictionary<string, object> state = new Dictionary<string, object>();
            state["self"] = this;
            state["ref"] = from;
            state["data"] = data;
            try
            {
                object value = Scripting.EvalExpression(expression, state);
                
                if (value is string)
                {
                    string str = value as string;
                    byte[] strbytes = new byte[str.Length];
                    
                    for(int i=0; i<strbytes.Length; ++i)
                        strbytes[i] = (byte)str[i];

                    return new Variant(strbytes);
                }
                else if (value is int)
                    return new Variant(Convert.ToInt32(value));
                else
                {
                    throw new PeachException(
                        string.Format("ExpressionFixup expected a return value of string or int but got '{0}'", value.GetType().Name));
                }

            }
            catch (System.Exception ex)
            {
                throw new PeachException(
                    string.Format("ExpressionFixup expression threw an exception!\nExpression: {0}\n Exception: {1}", expression, ex.ToString()));
            }   
        }

		void from_Invalidated(object sender, EventArgs e)
		{
			parent.Invalidate();
		}
    }
}

// end
