using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peach.Core.Mutators
{
    public partial class UnicodeBomMutator
    {
        string[] boms = new string[] { "0xFE,0xFF", "0xFF,0xFE", "0xEF,0xBB,0xBF" };
        string[] values = new string[] { };
        byte[] values2 = new byte[] { (byte)'A', (byte)'\xff', (byte)'\x0f' };
    }
}
