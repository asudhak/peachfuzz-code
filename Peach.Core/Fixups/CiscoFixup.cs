using System;
using System.Collections.Generic;
using Peach.Core.Dom;

namespace Peach.Core.Fixups.Libraries
{
  /// <summary>
  /// Computes the checksum in Host order for an array of bytes and
  /// compensates for Cisco off-by-one.
  /// </summary>
  public class CiscoCDPChecksum : InternetChecksum
  {
    public CiscoCDPChecksum()
      {
      }
    
    public override void Update(byte[] buf)
    {
      int i = 0;
      for (; i < buf.Length - 1; i += 2)
	sum += (uint)((buf[i] << 8) + buf[i + 1]);

      if (i != buf.Length)
	{
	  if ((buf[buf.Length - 1] & 0x80) != 0)
	    sum += (uint)((buf[buf.Length - 1] - 1) &  0xff00);
	  else 
	    sum += (uint)(buf[buf.Length - 1]);
	}
    }
  }


  [Fixup("CiscoFixup", true)]
  [Parameter("ref", typeof(DataElement), "Reference to data element")]
  [Serializable] 
  public class CiscoFixup : Fixup
  {

    public CiscoFixup(DataElement parent, Dictionary<string, Variant> args) 
         : base(parent, args, "ref")
      {
      }

    protected override Variant fixupImpl()
    {
      var elem = elements["ref"];
      byte[] data = elem.Value.Value;
      CiscoCDPChecksum sum = new CiscoCDPChecksum();
      sum.Update(data);
      return new Variant(sum.Final());
    }
    
  }
}