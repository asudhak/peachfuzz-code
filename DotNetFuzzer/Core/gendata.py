
import sys

sys.path.append("c:/projects/peach")
sys.path.append("/root/peach")

from Peach.Generators.data	import *

########################################################################

PeachDom_Strings = BadString()
PeachDom_Numbers = BadNumbers()

########################################################################

def JsString(strInput, default=''):
	
	if strInput == None or len(strInput) == 0:
		strInput = default
		
		if strInput == None or len(strInput) == 0:
			return '""'
	
	# Allow: a-z A-Z 0-9 SPACE , .
	# Allow (dec): 97-122 65-90 48-57 32 44 46
	
	out = ''
	for char in strInput:
		c = ord(char)
		if ((c >= 97 and c <= 122) or
			(c >= 65 and c <= 90 ) or
			(c >= 48 and c <= 57 ) or
			c == 32 or c == 44 or c == 46):
			out += char
		elif c <= 127:
			out += "\\x%02X" % c
		else:
			out += "\\u%04X" % c
	
	return '"%s"' % out

fd = open('PeachData.cs', 'w+')
fd.write("""
/*
 * Copyright (c) 2007 Michael Eddington
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights 
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in	
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 *
 * Authors:
 *   Michael Eddington (mike@phed.org)
 *
 * $Id$
 */

// ////////////////////////////////////////////////////////////////////////////

namespace Peach.DotNetFuzzer
{

	public static class PeachData
	{
""")

fd.write("\t\tpublic static string [] badStrings = new string[] {\n");

try:
	while True:
		fd.write("\t\t\t")
		fd.write(JsString(PeachDom_Strings.getValue()))
		fd.write(",\n")
		PeachDom_Strings.next()
except:
	pass

fd.write("\t\t\t};\n\n");
fd.write("\t\tpublic static long [] badNumbers = new long[] {\n");

try:
	while True:
		val = PeachDom_Numbers.getValue()
		if int(val) > 18446744073709551615:
			PeachDom_Nubmers.next()
			continue
		
		fd.write("\t\t\t")
		fd.write(val)
		fd.write(',\n')
		PeachDom_Numbers.next()
except:
	pass

fd.write("\t\t\t};\n\n");
fd.write('\t}\n');
fd.write('\n}\n\n// end\n\n')
fd.close()

