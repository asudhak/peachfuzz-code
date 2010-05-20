using System;
using System.Collections.Generic;
using System.Text;
using Peach.DotNetFuzzer;
using System.Reflection;
using System.Diagnostics;

namespace TestObjFuzzer
{
	/// <summary>
	/// This is an example of fuzzing an existing type instance.
	/// </summary>
    class Program
    {
        static void Main(string[] args)
        {
			ObjectFuzzer fuzzer = new ObjectFuzzer();

			fuzzer.AddObject(new Class1());
			fuzzer.Run();

			Debugger.Break();
        }
    }

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
        Class3 _class3 = new Class3();
		Class2()
		{
            _class3.ID = "CLASS2";
			Console.Out.WriteLine("private Class2.Class2()");
		}

		Class2(string p1)
		{
            _class3.ID = "CLASS2";
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

        public Class3 TestProperty2
        {
            get
            {
                Console.Out.WriteLine("private Class2.get_TestPropery()");
                return _class3;
            }
        }
    }

    public class Class3
    {
        public string ID = "";
        public void Foo(string s)
        {
            Console.Out.WriteLine("Class3::Foo(\"" +s + "\"): ID: "+ID);
        }
    }
}
