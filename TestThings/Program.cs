using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace TestThings
{
    class Program
    {
        static void Main(string[] args)
        {
            Test2 t2 = new Test2();
            Test3 t3 = new Test3();
            Test t;

            t = t2;
            t.Foo();

            t = t3;
            t.Foo();

            string s = "a";
        }
    }


    public class Test
    {
        public virtual void Foo()
        {
            Console.Out.WriteLine("Test::Foo()");
        }
    }
    public class Test2 : Test
    {
        public override void Foo()
        {
            Console.Out.WriteLine("Test2::Foo()");
        }
    }
    public class Test3 : Test
    {
        public new void Foo()
        {
            Console.Out.WriteLine("Test3::Foo()");
        }
    }
}
