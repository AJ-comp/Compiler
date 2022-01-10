using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TestProjectOnConsole
{
    [StructLayout(LayoutKind.Sequential)]
    public class Parent
    {
        private int a;
        public int pa;

        public void A()
        {
            Console.WriteLine(Marshal.SizeOf(this));
            a = 10;
        }

        public virtual void B()
        {
            Console.WriteLine(Marshal.SizeOf(this));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class ChildA : Parent
    {
        private int b;
        public int pb;

        public override void B()
        {
            base.B();
        }
    }
}
