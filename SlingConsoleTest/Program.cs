using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SLING;
using System.Collections;

namespace SlingConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Manager theManager = Manager.GetInstance();

            ArrayList lines = new ArrayList();
            lines.Add("ENTRY");
            lines.Add("MAIN");
            lines.Add("SWI &11");
            theManager.LoadFile(lines);

            uint value1 = UInt32.Parse("37E3C123", System.Globalization.NumberStyles.HexNumber);
            uint value2 = UInt32.Parse("367402AA", System.Globalization.NumberStyles.HexNumber);

            //Setup memory
            theManager.AddData("Value1", value1);
            theManager.AddData("Value2", value2);

            //set file next instruction to run
            SourceLine line = new SourceLine("ADD R1,R1,R2");
            //line.Instruction = ARMInstruction.ADD;
            theManager.File.SetNextLine(line);

            //Run our instruction          
            theManager.Step();

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
