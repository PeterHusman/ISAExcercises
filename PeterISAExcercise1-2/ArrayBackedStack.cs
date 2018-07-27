using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeterISAExcercise1_2
{
    public class Stack
    {
        private ushort[] array;
        private int index;
        public int Capacity
        {
            get { return array.Length; }
        }
        public int Size
        {
            get { return index; }
        }
        public Stack(int size)
        {
            index = 0;
            array = new ushort[size];
        }

        public void Push(ushort value)
        {
            array[index] = value;
            index++;
        }

        public void Pop(int numberToPop = 1)
        {
            index -= numberToPop;
            ////if(index < 0)
            ////{
            ////    throw new IndexOutOfRangeException();
            ////}
            //return array[index];
        }

        public object Peek(int numberDownTheStack)
        {
            return array[index - numberDownTheStack - 1];
        }

    }
    public class ObjStack
    {
        private object[] array;
        private int index;
        public int Capacity
        {
            get { return array.Length; }
        }
        public int Size
        {
            get { return index; }
        }
        public ObjStack(int size)
        {
            index = 0;
            array = new object[size];
        }

        public void Push(object value)
        {
            array[index] = value;
            index++;
        }

        public void Pop(int numberToPop = 1)
        {
            index -= numberToPop;
            ////if(index < 0)
            ////{
            ////    throw new IndexOutOfRangeException();
            ////}
            //return array[index];
        }

        public object Peek(int numberDownTheStack)
        {
            return array[index - numberDownTheStack - 1];
        }

    }
}
