using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peach.Core.Language.TestAssembly
{
	public class Class1
	{
		public Class1()
		{
			Console.Out.WriteLine("public Class1.Class1()");
		}

		public Class1(string p1)
		{
			Console.Out.WriteLine(String.Format("public Class1.Class1(\"{0}\")", p1));
		}

		public void Foo(string p1)
		{
			Console.Out.WriteLine(String.Format("public Class1.Foo(\"{0}\")", p1));
		}

		public void Foo2(string p1, string p2)
		{
			Console.Out.WriteLine(String.Format("public Class1.Foo(\"{0}\", \"{0}\")", p1, p2));
		}

		private void Bar(string p1)
		{
			Console.Out.WriteLine(String.Format("private Class1.Bar(\"{0}\")", p1));
		}

		public void CreateClass2(Class2 c)
		{
			Console.Out.WriteLine(String.Format("public Class1.CreateClass2(Class2.helloWorld: \"{0}\")", c.helloWorld));
		}
	}

	public class Class2
	{
		Class2()
		{
			Console.Out.WriteLine("private Class2.Class2()");
		}

		Class2(string p1)
		{
			Console.Out.WriteLine(String.Format("private Class1.Class1(\"{0}\")", p1));
		}

		public string helloWorld = "HelloWorld";

		public string TestProperty
		{
			get
			{
				Console.Out.WriteLine("private Class2.get_TestPropery()");
				return helloWorld;
			}
			set
			{
				Console.Out.WriteLine("private Class2.set_TestPropery()");
				helloWorld = value;
			}
		}
	}
}
