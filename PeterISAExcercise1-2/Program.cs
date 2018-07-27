using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace PeterISAExcercise1_2
{
    class Program
    {
        public static ObjStack GiantStack;
        //        public static Stack TheStack = new Stack(8192);
        static void Main(string[] args)
        {
            //NOTE TO SELF!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Next, work on static variables in HelperToAsm. Ex: *i = 5 --> ldc 5 0025. i would map to 25 in a dictionary, and someone could do *i = *a + *b -> lod r1 00aa; lod r2 00bb; add r1 r1 r2; str r1 0025
            GiantStack = new ObjStack(15);
            string inPath = Console.ReadLine();
            HelperToAsm(inPath, $"{inPath.Remove(inPath.Length - 4)}Middle.txt");
            Assemble($"{inPath.Remove(inPath.Length - 4)}Middle.txt", GetOpCodesFromCSV(@"C:\Users\PeterHusman\Downloads\Assembly To Machine Code Chart - Sheet1 (2).csv"), $"{inPath.Remove(inPath.Length - 4)}Out.txt", true);
            while (true)
            {
                Console.WriteLine("0x" + DoMath(uint.Parse(Console.ReadLine(), System.Globalization.NumberStyles.HexNumber)).ToString("X2"));
            }

            //   Console.ReadKey();
        }

        public static void HelperToAsm(string fileInPath, string fileOutPath)
        {
            string[] lines = File.ReadAllLines(fileInPath);
            for (int i = 0; i < lines.Length; i++)
            {
                string[] subParams = lines[i].Split(' ');
                if (subParams.Length < 2)
                {
                    continue;
                }
                if (subParams[1] == "=")
                {
                    if (subParams.Length <= 3)
                    {
                        switch (subParams[2])
                        {
                            default:
                                if (subParams[0].StartsWith("*r"))
                                {
                                    lines[i] = $"sti {subParams[2]} {subParams[0].Remove(0, 1)}";
                                }
                                else if (subParams[0].StartsWith("*"))
                                {
                                    lines[i] = $"str {subParams[2]} {subParams[0].Remove(0, 1)}";
                                }
                                else if (subParams[2].StartsWith("*r"))
                                {
                                    lines[i] = $"ldi {subParams[0]} {subParams[2].Remove(0, 1)}";
                                }
                                else if (subParams[2].StartsWith("*"))
                                {
                                    lines[i] = $"lod {subParams[0]} {subParams[2].Remove(0, 1)}";
                                }
                                else if (subParams[2].StartsWith("r"))
                                {
                                    lines[i] = $"mov {subParams[0]} {subParams[2]}";
                                }
                                else
                                {
                                    lines[i] = $"set {subParams[0]} {subParams[2]}";
                                }
                                break;
                        }

                        continue;
                    }
                    switch (subParams[3])
                    {
                        case "+":
                            lines[i] = $"add {subParams[0]} {subParams[2]} {subParams[4]}";
                            break;
                        case "-":
                            lines[i] = $"sub {subParams[0]} {subParams[2]} {subParams[4]}";
                            break;
                        case "*":
                            lines[i] = $"mul {subParams[0]} {subParams[2]} {subParams[4]}";
                            break;
                        case "/":
                            lines[i] = $"div {subParams[0]} {subParams[2]} {subParams[4]}";
                            break;
                        case "%":
                            lines[i] = $"mod {subParams[0]} {subParams[2]} {subParams[4]}";
                            break;
                    }
                }
            }
            File.WriteAllLines(fileOutPath, lines);
        }

        public static Dictionary<string, int[]> GetOpCodesFromCSV(string filePath)
        {
            Dictionary<string, int[]> output = new Dictionary<string, int[]>();
            Dictionary<string, int> phraseToBytes = new Dictionary<string, int>() { ["DEST REG"] = 1, ["REG TO CHECK"] = 1, ["SRC REG"] = 1, ["SRC1"] = 1, ["SRC2"] = 1, ["CONS"] = 2, ["ADD"] = 2, ["SRC ADD"] = 2, ["DEST ADD"] = 2, ["0"] = -1, ["NUM TO POP"] = 2, ["OFF"] = 2, ["PTR REG"] = 1, ["OFFSET"] = 1 };
            string[] rows = File.ReadAllLines(filePath);
            for (int k = 0; k < rows.Length; k++)
            {
                string[] cells = rows[k].Split(',');
                ushort opCode = 0;
                int[] columns;
                bool success = false;
                try
                {
                    opCode = ushort.Parse(cells[1], NumberStyles.HexNumber);
                    success = true;
                }
                catch
                {

                }
                if (success)
                {
                    columns = new int[4];
                    columns[0] = opCode;
                    for (int i = 0; i < 3; i++)
                    {
                        columns[1 + i] = phraseToBytes[cells[4 + i]];
                        if (columns[i + 1] == 2)
                        {
                            columns[i + 2] = 0;
                            break;
                        }
                    }
                    output.Add(cells[0], columns);
                }
            }
            return output;
        }

        public static void Assemble(string filePathIn, Dictionary<string, int[]> opCodes, string filePathOut, bool outputAsBytes)
        {
            string[] lines = File.ReadAllText(filePathIn).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            uint[] outlines = new uint[lines.Length];
            Dictionary<string, ushort> labels = new Dictionary<string, ushort>();
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Split(';')[0];
                lines[i] = lines[i].Replace("\r", "");
                lines[i] = lines[i].Trim(' ');
                if (lines[i].EndsWith(":"))
                {
                    labels.Add(lines[i].Remove(lines[i].Length - 1), (ushort)i);
                    lines[i] = "nop";
                }
            }
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Replace(",", "");
                foreach (string label in labels.Keys.ToArray())
                {
                    lines[i] = lines[i].Replace(label, labels[label].ToString("X"));
                }
                string[] subParams = lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (subParams.Length <= 0)
                {
                    continue;
                }
                for (int k = 1; k < subParams.Length; k++)
                {
                    subParams[k] = subParams[k].Replace("r", "");
                }
                foreach (string o in opCodes.Keys.ToArray())
                {
                    if (subParams[0] == o)
                    {
                        int[] cols = opCodes[o];
                        outlines[i] = (uint)cols[0];
                        int subParamsI = 1;
                        bool brk = false;
                        for (int j = 1; j <= 3; j++)
                        {
                            outlines[i] = (uint)(outlines[i] << 8);
                            switch (cols[j])
                            {
                                case -1:
                                    break;
                                case 0:
                                    brk = true;
                                    break;
                                case 1:
                                    outlines[i] += uint.Parse(subParams[subParamsI], NumberStyles.HexNumber);
                                    subParamsI++;
                                    break;
                                case 2:
                                    outlines[i] = (uint)(outlines[i] << 8);
                                    outlines[i] += uint.Parse(subParams[subParamsI], NumberStyles.HexNumber);
                                    subParamsI++;
                                    brk = true;
                                    break;


                            }
                            if (brk)
                            {
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            if (!outputAsBytes)
            {
                string[] outputLines = new string[outlines.Length];
                for (int i = 0; i < outlines.Length; i++)
                {
                    outputLines[i] = outlines[i].ToString("X8");
                }
                File.WriteAllLines(filePathOut, outputLines);
            }
            else
            {
                byte[] outputBytes = new byte[outlines.Length * 4];
                for (int i = 0; i < outlines.Length; i ++)
                {
                    outputBytes[i*4] = (byte)(outlines[i] >> 24);
                    outputBytes[i*4 + 1] = (byte)(outlines[i] >> 16);
                    outputBytes[i*4 + 2] = (byte)(outlines[i] >> 8);
                    outputBytes[i*4 + 3] = (byte)(outlines[i]);
                }
                File.WriteAllBytes(filePathOut, outputBytes);
            }
            //File.WriteAllText(filePathOut, allText);
        }







        public static unsafe void LoppThroughArrayAgain(int* start, int length)
        {
            while (start < start + length)
            {
                int val = *start;
                start++;
            }
        }

        public static ushort CheatMath(ushort left, ushort right, ushort op)
        {
            GiantStack.Push(left);
            GiantStack.Push(right);
            GiantStack.Push(op);
            CalculateWithStack();
            return (ushort)GiantStack.Peek(0);
        }

        public static void CalculateWithStack()
        {
            ushort left = (ushort)GiantStack.Peek(2);
            ushort right = (ushort)GiantStack.Peek(1);
            Action oper = (Action)GiantStack.Peek(0);
            GiantStack.Pop(3);
            oper();
            void Add()
            {
                GiantStack.Push((ushort)(left + right));
            }
            void Subtract()
            {
                GiantStack.Push((ushort)(left + (~right + 1)));
            }
            void Mult()
            {
                ushort output = 0;
                for (int i = 0; i < right; i++)
                {
                    output += left;
                }
                GiantStack.Push(output);
            }
            void Div()
            {
                if (right == 0)
                {
                    throw new DivideByZeroException();
                }
                if (GiantStack.Size <= 0)
                {
                    GiantStack.Push(0);
                    GiantStack.Push(left);
                }
                ushort counter = (ushort)GiantStack.Peek(1);
                ushort num = (ushort)GiantStack.Peek(0);
                if (num < right)
                {
                    GiantStack.Pop(2);
                    GiantStack.Push(counter);
                    return;
                }
                else
                {
                    num = (ushort)(num + ~right + 1);
                    counter++;
                    GiantStack.Pop(2);
                    GiantStack.Push(counter);
                    GiantStack.Push(num);
                    GiantStack.Push(left);
                    GiantStack.Push(right);
                    GiantStack.Push(new Action(Div));
                    CalculateWithStack();
                }
            }
            void Mod()
            {
                if (right == 0)
                {
                    throw new DivideByZeroException();
                }
                if (GiantStack.Size <= 0)
                {
                    GiantStack.Push(0);
                    GiantStack.Push(left);
                }
                ushort counter2 = (ushort)GiantStack.Peek(1);
                ushort num2 = (ushort)GiantStack.Peek(0);
                if (num2 < right)
                {
                    GiantStack.Pop(2);
                    GiantStack.Push(num2);
                    return;
                }
                else
                {
                    num2 = (ushort)(num2 + ~right + 1);
                    counter2++;
                    GiantStack.Pop(2);
                    GiantStack.Push(counter2);
                    GiantStack.Push(num2);
                    GiantStack.Push(left);
                    GiantStack.Push(right);
                    GiantStack.Push(new Action(Mod));
                    CalculateWithStack();
                }
            }

        }



        public static unsafe void LoopThroughArray(int* start, int length)
        {
            LoopThroughArray(start, start + length);
        }

        public static unsafe void LoopThroughArray(int* start, int* end)
        {
            while (start < end)
            {
                //Do stuff
                Console.WriteLine(*start);
                start++;
            }
        }

        public static byte DoMath(uint data)
        {
            byte inst = GetByte(data, 0);
            //byte dest = GetByte(data, 1);
            byte s1 = GetByte(data, 2);
            byte s2 = GetByte(data, 3);
            return DoMath(inst, s1, s2);
        }
        public static byte DoMath(byte inst, byte s1, byte s2)
        {
            switch (inst)
            {
                case 1:
                    return (byte)(s1 + s2);
                case 2:
                    return (byte)(s1 - s2);
                case 3:
                    return (byte)(s1 * s2);
                case 4:
                    return (byte)(s1 / s2);
                case 5:
                    return (byte)(s1 % s2);
                default:
                    throw new InvalidOperationException();
            }
        }

        public static byte GetByte(uint number, int bte)
        {
            unsafe
            {
                byte* ptr = (byte*)&number;
                return *(ptr + bte);
            }
        }

        //TODO
        public static uint FlipBit(uint num, int bitToFlip)
        {
            uint tempNum = num & (uint)(1 << bitToFlip);
            tempNum >>= bitToFlip;
            tempNum = ~tempNum;
            throw new NotImplementedException();
        }


        public static int DivideByPowerOfTwo(int num, int power)
        {
            return num >> power;
        }

        public static int MultByPowerOfTwo(int num, int power)
        {
            return num << power;
        }

        public static uint ModByPowerOfTwo(uint num, int power)
        {
            return (uint)(num << (31 - power)) >> (31 - power);
        }

        public static byte GetByteShifting(uint number, int bte)
        {
            //number <<= (8 * (3 - bte));
            //number >>= (8 * (3 - bte));
            number >>= (8 * bte);
            return (byte)number;
        }

        public static int GetByteMasking(int number, int bte)
        {
            int mask = 0xFF << (8 * bte);
            int temp = number & mask;
            return temp >> (bte * 8);
        }


        public static int NearestPowerOfTwo(int number)
        {
            /*0001
             *0010
             *0100
             *1000
             */
            if (number == 0)
            {
                return 1;
            }
            int i = 0;
            while (number != 1)
            {
                number = number >> 1;
                i++;
            }
            return number << i;
        }

        public static bool IsPowerOfTwo(int number)
        {
            int i = 0;
            int num = number;
            while (number != 1)
            {
                number = number >> 1;
                i++;
            }
            return number << i == num;
        }

        public static bool IsPowerOfTwoWithMasking(int number)
        {
            int count = 1;
            if (number == 0)
            {
                return false;
            }
            while (true)
            {
                if ((number & count) > 0)
                {
                    return count == number;
                }
                count <<= 1;
            }
        }

        public static int NearestPowerOfTwoWithMasking(int number)
        {
            int count = 1;
            int mostSignificantBit = 1;
            if (number == 0)
            {
                return 1;
            }
            while (true)
            {
                if ((number & count) > 0)
                {
                    mostSignificantBit = count;
                }
                else if (count > number)
                {
                    return mostSignificantBit;
                }

                count <<= 1;
            }
        }
    }
}
