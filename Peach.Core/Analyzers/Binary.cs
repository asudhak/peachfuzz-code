
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
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;

using Peach.Core.Dom;
using Peach.Core.IO;
using Peach.Core.Cracker;

using NLog;

namespace Peach.Core.Analyzers
{
    [Analyzer("Binary", true)]
    [Analyzer("BinaryAnalyzer")]
    [Parameter("Tokens", typeof(string), "List of character tokens to pass to the StringToken analyzer", StringTokenAnalyzer.TOKENS)]
    [Parameter("AnalyzeStrings", typeof(string), "Call the StringToken analyzer on string elements", "true")]
    [Serializable]
    public class Binary : Analyzer
    {
        static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        protected Dictionary<string, Variant> args = null;
        protected bool analyzeStrings = true;

        static Binary()
        {
            supportParser = false;
            supportDataElement = true;
            supportCommandLine = false;
            supportTopLevel = false;
        }

        public Binary()
        {
        }

        public Binary(Dictionary<string, Variant> args)
        {
            this.args = args;

            Variant val = null;
            if (args != null && args.TryGetValue("AnalyzeStrings", out val))
                analyzeStrings = ((string)val).ToLower() == "true";
        }

        const int MINCHARS = 5;

        public override void asDataElement(DataElement parent, Dictionary<DataElement, Position> positions)
        {
            if (!(parent is Dom.Blob))
                throw new PeachException("Error, Binary analyzer only operates on Blob elements!");

            var blob = parent as Dom.Blob;
            var data = blob.Value;

            if (data.Length == 0)
                return;

            var block = new Block(blob.name);
            var bs = new BitStream();
            long chars = 0;

            while (true)
            {
                int value = data.ReadByte();
                if (value == -1)
                    break;

                //if (isGzip(b, data))
                //{
                //    throw new NotImplementedException("Handle Gzip data stream");
                //}

                if (isAsciiChar(value))
                {
                    ++chars;
                }
                else
                {
                    if (chars >= MINCHARS)
                    {
                        // Only treat this as a string if MINCHARS were found
                        bs.Seek(-chars, SeekOrigin.End);

                        var str = new Dom.String();
                        str.DefaultValue = new Variant(bs.SliceBits(bs.LengthBits - bs.PositionBits));

                        bs.Seek(-chars, SeekOrigin.End);
                        bs.SetLength(bs.Position);

                        // Save off any data before the string 1st
                        bs = saveData(block, bs);

                        // Add the string 2nd
                        block.Add(str);

                        // Potentially analyze the string further
                        if (analyzeStrings)
                            new StringTokenAnalyzer(args).asDataElement(str, positions);
                    }

                    chars = 0;
                }

                bs.WriteByte((byte)value);
            }

            bs = saveData(block, bs);

            if (logger.IsDebugEnabled)
            {
                int count = block.EnumerateAllElements().Count();
                logger.Debug("Created {0} data elements from binary data.", count);
            }

            parent.parent[parent.name] = block;
        }

        protected BitStream saveData(Block block, BitStream data)
        {
            if (data.Length == 0)
                return data;

            var elem = new Blob();
            elem.DefaultValue = new Variant(data);

            block.Add(elem);

            return new BitStream();
        }

        protected bool isGzip(int b, BitStream data)
        {
            if (b == 0x1f)
            {
                if (data.ReadByte() == 0x8b)
                {
                    data.Seek(-1, SeekOrigin.Current);
                    return true;
                }

                data.Seek(-1, SeekOrigin.Current);
            }

            return false;
        }

        protected bool isAsciiChar(int b)
        {
            if (b == 0x09 || /* tab */
                b == 0x0a || b == 0x0d || /* crlf */
                (b >= 32 && b <= 126))
            {
                return true;
            }

            return false;
        }
    }
}

// end
