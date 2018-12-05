using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Collections;

namespace SLING.Tests
{
    [TestClass]
    public class SlingTests
    {
        private Manager _theManager;
        //Used in ADD
        private uint _value1 = UInt32.Parse("37E3C123", NumberStyles.HexNumber);
        private uint _value2 = UInt32.Parse("367402AA", NumberStyles.HexNumber);
        //Used in MOV
        private uint _value3 = UInt32.Parse("4242", NumberStyles.HexNumber);
        //Used in MVN
        private uint _value4 = UInt32.Parse("C123", NumberStyles.HexNumber);

        [TestInitialize]
        public void TestInitialize()
        {
            _theManager = Manager.GetInstance();

            ArrayList lines = new ArrayList();
            lines.Add("ENTRY");
            lines.Add("MAIN");
            lines.Add("Done"); //Labelled line for Branch test
            lines.Add("SWI &11");
            _theManager.LoadFile(lines);
        }

        [TestMethod]
        public void Test1ManagerSingleton()
        {
            Assert.AreSame(_theManager, Manager.GetInstance());
        }

        [TestMethod]
        public void Test2MemoryAccess()
        {
            //Setup memory
            _theManager.AddData("Value1", _value1);
            _theManager.AddData("Value2", _value2);
            _theManager.AddData("Value3", _value3);
            _theManager.AddData("Value4", _value4);
            //Allocate space for result
            _theManager.AddData("Result", 0);

            //Used in ADD
            Assert.AreEqual(_value1, _theManager.CPU.Memory.Data("Value1"));
            Assert.AreEqual(_value2, _theManager.CPU.Memory.Data("Value2"));
            //Used in MOV
            Assert.AreEqual(_value3, _theManager.CPU.Memory.Data("Value3"));
            //Used in MVN
            Assert.AreEqual(_value4, _theManager.CPU.Memory.Data("Value4"));

        }

        [TestMethod]
        public void Test3LdrOperation()
        {

            //Load registers with values from memory
            SourceLine line1 = new SourceLine("LDR R1,Value1");
            _theManager.File.SetNextLine(line1);
            _theManager.Step();
            SourceLine line2 = new SourceLine("LDR R2,Value2");
            _theManager.File.SetNextLine(line2);
            _theManager.Step();

            SourceLine line3 = new SourceLine("LDR R3,Value3");
            _theManager.File.SetNextLine(line3);
            _theManager.Step();

            SourceLine line4 = new SourceLine("LDR R4,Value4");
            _theManager.File.SetNextLine(line4);
            _theManager.Step();

            Assert.AreEqual(_value1, _theManager.CPU.Registers[1]);
            Assert.AreEqual(_value2, _theManager.CPU.Registers[2]);
            Assert.AreEqual(_value3, _theManager.CPU.Registers[3]);
            Assert.AreEqual(_value4, _theManager.CPU.Registers[4]);

        }

        [TestMethod]
        public void Test4AddOperation()
        {

            //Run the ADD operation
            SourceLine line = new SourceLine("ADD R1,R1,R2");
            _theManager.File.SetNextLine(line);
            _theManager.Step();

            //assert values updated as expected
            Assert.AreEqual(UInt32.Parse("6E57C3CD", NumberStyles.HexNumber), _theManager.CPU.Registers[1]);
            Assert.AreEqual(_value2, _theManager.CPU.Registers[2]);
            Assert.AreEqual(CpuMode.usr, _theManager.CPU.CPUMode);
            Assert.AreEqual(_value2, _theManager.CPU.Alu.A);
            Assert.AreEqual(_value1, _theManager.CPU.Alu.B);
            Assert.AreEqual(ALUCommand.ADD, _theManager.CPU.Alu.Command);
            Assert.AreEqual(UInt32.Parse("6E57C3CD", NumberStyles.HexNumber), _theManager.CPU.Alu.Result);
            Assert.AreEqual(false, _theManager.CPU.Alu.Flag_C);
            Assert.AreEqual(false, _theManager.CPU.Alu.Flag_N);
            Assert.AreEqual(false, _theManager.CPU.Alu.Flag_V);
            Assert.AreEqual(false, _theManager.CPU.Alu.Flag_Z);

        }

        [TestMethod]
        public void Test5StrOperation()
        {
            //Run the STR operation
            SourceLine line = new SourceLine("STR R1,Result");
            _theManager.File.SetNextLine(line);
            _theManager.Step();

            Assert.AreEqual(_theManager.CPU.Registers[1], _theManager.CPU.Memory.Data("Result"));
        }

        [TestMethod]
        public void Test6MovOperation()
        {
            //Run the MOV operation
            SourceLine line = new SourceLine("MOV R3,R3,LSL #0x1");
            _theManager.File.SetNextLine(line);
            _theManager.Step();

            uint temp = UInt32.Parse((4242 << 1).ToString(), NumberStyles.HexNumber);
            Assert.AreEqual(temp, _theManager.CPU.Registers[3]);
            Assert.AreEqual(UInt32.Parse("4242", NumberStyles.HexNumber), _theManager.CPU.BS.A); //Register
            Assert.AreEqual(1, _theManager.CPU.BS.B); //shift amount
            Assert.AreEqual(ShiftCommand.LSL, _theManager.CPU.BS.Command); //shift amount
            Assert.AreEqual(temp, _theManager.CPU.BS.Result); //shift amount


        }

        [TestMethod]
        public void Test7MvnOperation()
        {
            //Run the MVN operation
            SourceLine line = new SourceLine("MVN R4,R4");
            _theManager.File.SetNextLine(line);
            _theManager.Step();

            uint result = ~_value4;
            Assert.AreEqual(result, _theManager.CPU.Registers[4]);

        }

        //Test Branch (BHI)
        [TestMethod]
        public void Test8BranchOperation()
        {
            //Set CPSR flags so branch condition passes
            //HI
            _theManager.CPU.CPSR_C = true;
            _theManager.CPU.CPSR_Z = false;

            SourceLine line = new SourceLine("BHI Done");
            _theManager.File.SetNextLine(line);
            _theManager.Step();

            Assert.AreEqual("Done", _theManager.File.LastLabel);
            SourceLine nextLine = _theManager.File.GetNextLine();
            Assert.AreEqual(true, nextLine.HasLabel);
            Assert.AreEqual(SourceLineType.LABEL, nextLine.Type);
            Assert.AreEqual("Done", nextLine.Label);


        }    
    }
}
