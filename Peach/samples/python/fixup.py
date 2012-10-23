#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Microsoft Public License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Public License, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

import clr
import clrtype
import System
from System.Reflection import BindingFlags
from Peach.Core import Fixup, Variant
from Peach.Core.Dom import DataElement

class MyFixup(Fixup):
    __metaclass__ = clrtype.ClrClass
    
    _clrnamespace = "IronPython.Samples.ClrType"   
    
    _clrfields = {
    #    "name":str,
    #    "cost":float,
    #    "_quantity":int
    }
      
    CLSCompliant = clrtype.attribute(System.CLSCompliantAttribute)
    clr.AddReference("System")
    clr.AddReference("Peach.Core")
    AttrSerializable = clrtype.attribute(System.SerializableAttribute)
    AttrParameter = clrtype.attribute(Peach.Core.ParameterAttribute)
    AttrFixup = clrtype.attribute(Peach.Core.FixupAttribute)
    
    _clrclassattribs = [
        # Use System.Attribute subtype directly for custom attributes without arguments
        System.ObsoleteAttribute,
        # Use clrtype.attribute for custom attributes with arguments (either positional, named, or both)
        CLSCompliant(False),
        AttrSerializable(),
        AttrParameter("ref", type(DataElement), "Reference to data element", True),
        AttrFixup("MyFixup"),
    ]

    def __init__(self, parent, args):
        Fixup.__init__(parent, args, "ref")
        pass
    
    @override
    @clrtype.accepts()
    @clrtype.returns(Variant)
    def fixupImpl(self):
        print "MyFixup.fixupImpl()"
        elem = self.elements["ref"]
        data = elem.Value.Value
        
        print "MyFixup.fixupImpl(): len(data): " + len(data)
        
        return Variant("MyFixup")


