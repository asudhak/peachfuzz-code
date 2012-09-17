
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
using System.IO;
using System.Reflection;
using Peach.Core.Dom;
using Ionic.Zip;

namespace Peach.Core.Mutators
{
    [Mutator("Performs the W3C parser tests. Only works on <String> elements with a <Hint name=\"type\" value=\"xml\">")]
    [Hint("type", "Allows string to be mutated by the XmlW3CMutator.")]
    public class XmlW3CParserTestsMutator : Mutator
    {
        // members
        //
        uint pos;
        string[] values = new string[] { };
        Stream s;
        ZipFile zip;
        ZipEntry entry;

        // CTOR
        //
        public XmlW3CParserTestsMutator(DataElement obj)
        {
            // some data
            pos = 0;
            name = "XmlW3CParserTestsMutator";
            string temp = null;
            char[] delim = new char[] { '\r', '\n' };
            string[] errorValues = new string[] { };
            string[] invalidValues = new string[] { };
            string[] nonwfValues = new string[] { };
            string[] validValues = new string[] { };

            // create a memory stream of the buffer so that the ZipFile class can be used, then read in the zip file
            Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("Peach.Core.xmltests.zip");
            zip = ZipFile.Read(s);

            // pull data from the zip file
            
            // ERROR
            entry = zip["xmltests/error.txt"];
            s = entry.OpenReader();
            byte[] data2 = new byte[s.Length];
            s.Read(data2, 0, data2.Length);
            temp = Encoding.ASCII.GetString(data2);
            errorValues = temp.Split(delim, StringSplitOptions.RemoveEmptyEntries);

            // INVALID
            entry = zip["xmltests/invalid.txt"];
            s = entry.OpenReader();
            data2 = new byte[s.Length];
            s.Read(data2, 0, data2.Length);
            temp = Encoding.ASCII.GetString(data2);
            invalidValues = temp.Split(delim, StringSplitOptions.RemoveEmptyEntries);

            // NONWF
            entry = zip["xmltests/nonwf.txt"];
            s = entry.OpenReader();
            data2 = new byte[s.Length];
            s.Read(data2, 0, data2.Length);
            temp = Encoding.ASCII.GetString(data2);
            nonwfValues = temp.Split(delim, StringSplitOptions.RemoveEmptyEntries);
            
            // VALID
            entry = zip["xmltests/valid.txt"];
            s = entry.OpenReader();
            data2 = new byte[s.Length];
            s.Read(data2, 0, data2.Length);
            temp = Encoding.ASCII.GetString(data2);
            validValues = temp.Split(delim, StringSplitOptions.RemoveEmptyEntries);

            // build one array of values out of all of our lists
            // ORDER IS:
            // -- 1) error.txt
            // -- 2) invalid.txt
            // -- 3) nonwf.txt
            // -- 4) valid.txt
            int totalEntries = errorValues.Length + invalidValues.Length + nonwfValues.Length + validValues.Length;
            string[] tempHolder = new string[totalEntries];
            errorValues.CopyTo(tempHolder, 0);
            invalidValues.CopyTo(tempHolder, errorValues.Length);
            nonwfValues.CopyTo(tempHolder, errorValues.Length + invalidValues.Length);
            validValues.CopyTo(tempHolder, errorValues.Length + invalidValues.Length + nonwfValues.Length);
            values = tempHolder;           
        }

        // DTOR
        //
        ~XmlW3CParserTestsMutator()
        {
            // clean-up
            zip.Dispose();
            s.Close();
        }

        // MUTATION
        //
        public override uint mutation
        {
            get { return pos; }
            set { pos = value; }
        }

        // COUNT
        //
        public override int count
        {
            get { return values.Length; }
        }

        // SUPPORTED
        //
        public new static bool supportedDataElement(DataElement obj)
        {
            if (obj is Dom.String && obj.isMutable)
            {
                Hint h = null;
                if (obj.Hints.TryGetValue("XMLhint", out h))
                {
                    if (h.Value == "xml")
                        return true;
                }
            }

            return false;
        }

        // SEQUENTIAL_MUTATION
        //
        public override void sequentialMutation(DataElement obj)
        {
            string filePath = "xmltests/" + values[pos];
            entry = zip[filePath];
            s = entry.OpenReader();
            byte[] data2 = new byte[s.Length];
            s.Read(data2, 0, data2.Length);
            obj.MutatedValue = new Variant(data2);
            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
            obj.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;
        }

        // RANDOM_MUTATION
        //
        public override void randomMutation(DataElement obj)
        {
            string filePath = "xmltests/" + context.Random.Choice(values);
            entry = zip[filePath];
            s = entry.OpenReader();
            byte[] data2 = new byte[s.Length];
            s.Read(data2, 0, data2.Length);
            obj.MutatedValue = new Variant(data2);
            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
            obj.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;
        }
    }
}

// end
