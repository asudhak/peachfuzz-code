using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Peach.Core.Debuggers.DebugEngine;
using System.IO;
using System.Xml;
using System.Xml.Schema;

using System.Web.Services.Description;

using Peach.Core;
using Peach.Core.IO;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.Cracker;
using Peach.Core.Debuggers.WindowsSystem;

namespace TestThings
{
	class Program
	{
		public static void Main(string[] argv)
		{
			new Program();
		}
		public Program()
		{
			//SerializeTest root = new SerializeTest();
			//root.Value = "Root";
			//root.Others.Add(new SerializeTest("Child 1"));
			//root.Others.Add(new SerializeTest("Child 2"));
			//root.Others.Add(new SerializeTest("Child 3"));

			//var newRoot = ObjectCopier.Clone<SerializeTest>(root);

			//root.Others[0].Value = "Changed by Root";

			//Console.WriteLine(newRoot.Others[0].Value);
			//Console.ReadKey();

			var wsdl = ServiceDescription.Read(@"C:\peach3\AmazonWebServices.wsdl.xml");

			foreach (PortType portType in wsdl.PortTypes)
			{
				Console.WriteLine("PortType: " + portType.Name);

				foreach (Operation operation in portType.Operations)
				{
					Console.WriteLine("  " + operation.Name);

					foreach (object obj in operation.Messages)
					{
						if (obj is OperationInput)
						{
							OperationInput oInput = obj as OperationInput;

							Console.WriteLine("    IN: " + ((OperationInput)obj).Message.Name);

							Message msg = wsdl.Messages[oInput.Message.Name];
							foreach (MessagePart mPart in msg.Parts)
							{
								Console.WriteLine("      " + mPart.Name + ":" + mPart.Type.Namespace + ":" + mPart.Type.Name);

								var schema = wsdl.Types.Schemas[mPart.Type.Namespace];

								var xmlObj = schema.SchemaTypes[new XmlQualifiedName(mPart.Type.Name, mPart.Type.Namespace)];
								var xmlComplex = xmlObj as XmlSchemaComplexType;

								Console.WriteLine("xmlObj: " + xmlObj.GetType().ToString() + " - " + xmlObj.ToString());
							}
						}
						else if (obj is OperationOutput)
							Console.WriteLine("    OT: " + ((OperationOutput)obj).Message.Name);
						else
							Console.WriteLine("Unknown type: " + obj.GetType().ToString());
					}
				}
			}

			Console.ReadKey();
		}

		//protected Message GetMessageForOperation(ServiceDescription wsdl, Operation operation)
		//{
		//    foreach (Message msg in wsdl.Messages)
		//    {
		//        if(msg.
		//    }
		//}
	}

	[Serializable]
	public class SerializeTest
	{
		public string Value = null;
		public List<SerializeTest> Others = new List<SerializeTest>();

		public SerializeTest()
		{
		}

		public SerializeTest(string Value)
		{
			this.Value = Value;
		}
	}
}

// end
