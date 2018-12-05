using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SLING
{
    public delegate void RegisterChangedFromMBREventHandler(int registerNumber);
    public delegate void RegisterChangedFromAluResultEventHandler(int registerNumber);
    public delegate void RegisterChangedFromBsResultEventHandler(int registerNumber);
    public delegate void MemoryChangedFromMBREventHandler();
    public delegate void MBRfromMemoryEventHandler();
    public delegate void MARChangedEventHandler();
    public delegate void MBRChangedEventHandler();
    public delegate void IRChangedEventHandler();
    public delegate void CpsrFlagsUpdatedEventHandler();
    public delegate void MBRChangedFromRegisterEventHandler(int registerNumber);
    public delegate void RegisterUpdatedEventHandler(int registerNumber);

    public class ARM6CPU
    {
        //event args should contain register number thats changed
        
        public event RegisterChangedFromMBREventHandler RegisterChangedFromMBR;
        public event RegisterChangedFromAluResultEventHandler RegisterChangedFromAluResult;
        public event RegisterChangedFromBsResultEventHandler RegisterChangedFromBsResult;
        public event MBRfromMemoryEventHandler MBRfromMemory;
        public event MemoryChangedFromMBREventHandler MemoryChangedFromMBR;
        public event MARChangedEventHandler MarChanged;
        public event MBRChangedEventHandler MbrChanged;
        public event IRChangedEventHandler IrChanged;
        public event MBRChangedFromRegisterEventHandler MBRChangedFromRegister;
        public event RegisterUpdatedEventHandler RegisterUpdated;
        public event CpsrFlagsUpdatedEventHandler CpsrFlagsUpdated;

        private UInt32[] _registers;
        private CpuMode _mode;
        private bool[] _cpsr;
        private ALU _alu;
        private BoothMultiplier _bm;
        private BarrelShifter _bs;

        private string _mar;
        private UInt32 _mbr;
        private ARMInstruction _ir;

        private bool _opResult;

        private Thread exeuctionThread;

        //Signalled when the file has finished being processed
        private ManualResetEvent _doneEvent = new ManualResetEvent(false);

        //Used to store variables
        MemoryArea _memory = new MemoryArea();

         public ARM6CPU()
        {
            //set registers etc to defaults            
            _registers = new UInt32[16];

            //32-bits
            _cpsr = new bool[32];
            //Mode its bits 0..4 of CPSR
            _mode = CpuMode.usr;

            _alu = new ALU();
            _bm = new BoothMultiplier();
            _bs = new BarrelShifter();

            _mar = "Empty";
            _mbr = 0;

            _opResult = true;
        }

        public void Reset()
        {
            for (int i = 0; i < _cpsr.Length; i++)
            {
                _cpsr[i] = false;
            }
            _mode = CpuMode.usr;
            for (int j = 0; j < _registers.Length; j++)
            {
                _registers[j] = 0;
            }

            _mar = "Empty";
            _mbr = 0;
            _alu.Reset();
            _bm.Reset();
            _bs.Reset();
            _doneEvent.Reset();
            CPSR_C = false;
            CPSR_F = false;
            CPSR_I = false;
            CPSR_N = false;
            CPSR_V = false;
            CPSR_Z = false;
            _opResult = true;
            _memory.Clear();
        }


        public void Run()
        {
            _doneEvent.Reset();
            RunEmulation();
            //exeuctionThread = new Thread(RunEmulation);
            //exeuctionThread.Start();
            _doneEvent.WaitOne();
        }

        private void RunEmulation()
        {                            
            SourceLine line = Manager.GetInstance().File.GetNextLine();
            do
            {                   
                executeLine(line);
                line = Manager.GetInstance().File.GetNextLine();
            }
            while (line.Type != SourceLineType.END && _opResult == true);

            Manager.GetInstance().File.Reset();
            //reset opResult
            _opResult = true;

            _doneEvent.Set();

        }

        private void executeLine(SourceLine line)
        {
            //Raise events here to animate fetch/execute,incrementer
            MAR = Manager.GetInstance().File.CurrentLineIndex.ToString();
            MBR = Convert.ToUInt32(Manager.GetInstance().File.CurrentInstruction.GetHashCode());
            IR = Manager.GetInstance().File.CurrentInstruction.Instruction;

            if (checkConditionCode(line.Code))
           {
                switch (line.Instruction)
                {
                    case ARMInstruction.ADD:
                        {
                            opAdd(line.Rd, line.Rn, line.Op1, line.SetFlags);
                            break;
                        }
                    case ARMInstruction.B:
                        {
                            opB(line.Op1);
                            break;
                        }
                    case ARMInstruction.CMP:
                        {
                            opCmp(line.Rn, line.Op1);
                            break;
                        }
                    case ARMInstruction.LDR:
                        {
                            opLdr(line.Rd, line.Op2);
                            break;
                        }
                    case ARMInstruction.MOV:
                        {
                            opMov(line.Rd, line.Op1, line.Shift, line.ShiftAmount, line.ShiftType, line.ShiftRegister);
                            break;
                        }
                    case ARMInstruction.MVN:
                        {
                            opMvn(line.Rd, line.Op1);
                            break;
                        }
                    case ARMInstruction.STR:
                        {
                            opStr(line.Rd, line.Op2);
                            break;
                        }
                    case ARMInstruction.SWI:
                        {
                            _opResult = opSwi(line.Op1);
                            break;
                        }
                    default:
                        break;
                        //Do Nothing
                }
            }
        }

        private void opB(string op1)
        {
            //branch to label in op1, condition will have been met
            Manager.GetInstance().File.SetNextLine(op1);
        }

        private void opCmp(int rn, string op1)
        {
            //Subtract op1 from Rn            
            string val = op1.Remove(0, 1);
            _alu.A= _registers[Int32.Parse(val)];

            //Get value from register
            _alu.B = _registers[rn];

            _alu.Command = ALUCommand.SUBTRACT;
            
            //Flags will be set in ALU
            _alu.Calculate();

            //Copy flags from ALU to CPSR
            CPSR_N = _alu.Flag_N;
            CPSR_Z = _alu.Flag_Z;
            CPSR_C = _alu.Flag_C;
            CPSR_V = _alu.Flag_V;
            //Raise event to animate from ALU to CPSR
            if (CpsrFlagsUpdated != null)
            {
                CpsrFlagsUpdated();
            } 
            
            
        }

        private void opMvn(int rd, string op1)
        {
            //NOT op1, store in Rd - via shifter or ALU?
            //C123 1100000100100011
            //3EDC 0011111011011100 
            //if (op1[0].Equals('R'))
            //{

            //Remove R from op1 to get register number
            string val = op1.Remove(0, 1);
            //Get value from register
            _alu.A = _registers[Int32.Parse(val)];
            //Raise event ALU A animation from register
            _alu.Command = ALUCommand.NOT;
            _alu.Calculate();

            if (RegisterChangedFromAluResult != null)
            {
                RegisterChangedFromAluResult(rd);
            }
            _registers[rd] = _alu.Result;
            if (RegisterUpdated != null)
            {
                RegisterUpdated(rd);
            }

        }

        private void opMov(int rd, string op1, bool shift, int shiftAmount, ShiftCommand shiftType, int shiftRegister)
        {
            if (shift == false)
            {
                //Rd <--- Op1
                //Check op1 is reg or immediate or label...
                if (op1[0].Equals('R'))
                {
                    string val = op1.Remove(0, 1);
                    _registers[rd] = _registers[Int32.Parse(val)];
                    if (RegisterUpdated != null)
                    {
                        RegisterUpdated(rd);
                    }
                }
                else
                {
                    //assume is '#' - base 10 number
                    string val = op1.Remove(0, 1);
                    _registers[rd] = UInt32.Parse(val);
                    if (RegisterUpdated != null)
                    {
                        RegisterUpdated(rd);
                    }
                }
            }
            else
            {
                //If Shift Register, then set shift _bs.B to amount in register
                if (op1[0].Equals('R'))
                {
                    string val = op1.Remove(0, 1);
                    _bs.B = (int)_registers[Int32.Parse(val)];
                }
                else
                {
                    //Immediate - use shiftAmount already set
                    _bs.B = shiftAmount;
                }
                
                //Put values in barrel shifter
                 _bs.A = _registers[shiftRegister];
                ///TODO: Animation from register to ALU

                _bs.Command = shiftType;

                //Perform the Shift...
                _bs.Calculate();

                //Copy result into destination register - other registers reamin unchanged
                if (RegisterChangedFromBsResult != null)
                {
                    RegisterChangedFromBsResult(rd);
                }
                _registers[rd] = _bs.Result;
                if (RegisterUpdated != null)
                {
                    RegisterUpdated(rd);
                }
            }
        }


        private void opAdd(int rd, int rn, string op1, bool setFlags)
        {           
            
            //Check op1 is reg or immediate or label...
            if (op1[0].Equals('R'))
            {
                string val = op1.Remove(0, 1);
                _alu.A = _registers[Int32.Parse(val)];
                //Raise Event to animate from Register, through Booth Multiplier to ALU A   
            }
            else if(op1.StartsWith("#0x"))
            {
                //hex number - remove #0x
                string val = op1.Remove(0, 3);
                _alu.A = UInt32.Parse(val);
            }
            else
            {
                //assume is '#' - base 10 number
                string val = op1.Remove(0, 1);
                _alu.A = UInt32.Parse(val);
            }         
            
            _alu.B = _registers[rn];
            //Raise Event to animate from Register, through Barrel Shifter to ALU B

            _alu.Command = ALUCommand.ADD;
            _alu.Calculate();

            if (RegisterChangedFromAluResult != null)
            {
                RegisterChangedFromAluResult(rd);
            }
            _registers[rd] = _alu.Result;
            if (RegisterUpdated != null)
            {
                RegisterUpdated(rd);
            }

            if (setFlags)
            {
                CPSR_N = _alu.Flag_N;
                CPSR_Z = _alu.Flag_Z;
                CPSR_C = _alu.Flag_C;
                CPSR_V = _alu.Flag_V;
                if (CpsrFlagsUpdated != null)
                {
                    CpsrFlagsUpdated();
                }  
                //Raise event to animate from ALU to CPSR
            }
        }

        private void opLdr(int rd, string op2)
        {

            string mem;
            if (op2[0].Equals('='))
            {
                //Address of memory location required
                //LDR	R0, =Value1 ;Load the address of first value
                //Get memory address of value1 and store it in register 0
                mem = op2.Remove(0, 1);

                _registers[rd] = _memory.Address(mem);
                if (RegisterUpdated != null)
                {
                    RegisterUpdated(rd);
                }

            }
            else if (op2[0].Equals('['))
            {
                //LDR	R1, [R0]		; Load what is at that address
                //Register 0 contains a memory address - load value from this address into Register 1
                //Remove first [ and R
                string val1 = op2.Remove(0, 2);
                //Remove last ]
                string val2 = val1.Remove(val1.Length - 1);
                uint regNumber = UInt32.Parse(val2);

                _registers[rd] = _memory.Data(_registers[regNumber]);
                if (RegisterUpdated != null)
                {
                    RegisterUpdated(rd);
                }

            }
            else
            {
                //LDR	R1, Value1		; Load the first number
                //Load data in Value1 into register 1
                MAR = op2;

                if (MBRfromMemory != null)
                {
                    MBRfromMemory();
                }
                MBR = _memory.Data(MAR);

                if (RegisterChangedFromMBR != null)
                {
                    RegisterChangedFromMBR(rd);
                }
                _registers[rd] = MBR;
                if (RegisterUpdated != null)
                {
                    RegisterUpdated(rd);
                }
            }
            
        }

        private void opStr(int rd, string op2)
        {
            MAR = op2;

            if (MBRChangedFromRegister!= null)
            {
                MBRChangedFromRegister(rd);
            }
            MBR = _registers[rd];

            if (MemoryChangedFromMBR != null)
            {
                MemoryChangedFromMBR();
            }
            _memory.Store(MAR,MBR);
        }

        private bool opSwi(string value)
        {
            //TODO: set processor mode etc - see ARM docs.
            uint val = UInt32.Parse(value);
            return val == 11 ? false : true;
        }


        private bool checkConditionCode(ConditionCode conditionCode)
        {
            switch (conditionCode)
            {
                case ConditionCode.CS: return CPSR_C == true;
                case ConditionCode.CC: return CPSR_C == false;
                case ConditionCode.EQ: return CPSR_Z == true;
                case ConditionCode.NE: return CPSR_Z == false;
                case ConditionCode.VS: return CPSR_V == true;
                case ConditionCode.VC: return CPSR_V == false;      
                case ConditionCode.GT: return CPSR_N == CPSR_V;
                case ConditionCode.GE: return CPSR_Z == false && CPSR_N == CPSR_V;
                case ConditionCode.LT: return CPSR_N != CPSR_V;
                case ConditionCode.LE: return CPSR_Z == true && CPSR_N != CPSR_V;
                case ConditionCode.MI: return CPSR_N == true;
                case ConditionCode.PL: return CPSR_N == false;
                case ConditionCode.HI: return CPSR_C == true && CPSR_Z == false;
                case ConditionCode.HS: return CPSR_C == true; //equiv to CS
                case ConditionCode.LO: return CPSR_C == false; //equiv to CC
                case ConditionCode.LS: return CPSR_C == false || CPSR_Z == true;
                case ConditionCode.AL: return true;
                default:
                    {
                        return false;
                    }
            }
        }

        public void Step()
        {
            //Reset if the last step bought us to the end of the program (ie a SWI)
            if (_opResult == false)
            {
                Manager.GetInstance().File.Reset();
                //reset opResult
                _opResult = true;
            }

            SourceLine nextLine = Manager.GetInstance().File.GetNextLine();

            executeLine(nextLine);
        }

        public bool CPSR_N
        {
            get { return _cpsr[31]; }
            set { _cpsr[31] = value; }
        }

        public bool CPSR_Z
        {
            get { return _cpsr[30]; }
            set { _cpsr[30] = value; }
        }

        public bool CPSR_C
        {
            get{ return _cpsr[29]; }
            set{ _cpsr[29] = value; }
        }

        public bool CPSR_V
        {
            get { return _cpsr[28]; }
            set { _cpsr[28] = value; }
        }

        public bool CPSR_I
        {
            get { return _cpsr[7]; }
            set { _cpsr[7] = value; }
        }

        public bool CPSR_F
        {
            get { return _cpsr[6]; }
            set { _cpsr[6] = value; }
        }

        public void AddData(string label, uint variable)
        {
            _memory.Add(label, variable);
        }

        //This property for testing purposes only
        public MemoryArea Memory
        {
            get { return _memory; }
        }

        public string MAR
        {
            get { return _mar; }
            set
            {
                _mar = value;
                if (MarChanged != null)
                {
                    MarChanged();
                }
            } 
        }

        public UInt32 MBR
        {
            get { return _mbr; }
            set
            {
                _mbr = value;
                if (MbrChanged != null)
                {
                    MbrChanged();
                }
            } 
        }

        public ARMInstruction IR
        {
            get { return _ir; }
            set
            {
                _ir = value;
                if (IrChanged != null)
                {
                    IrChanged();
                }
            }
        }

        //This property for testing purposes only
        public UInt32[] Registers
        {
            get { return _registers; }
        }

        public CpuMode CPUMode
        {
            get { return _mode; }
        }

        public ALU Alu
        {
            get { return _alu; }
        }

        public BoothMultiplier BM
        {
            get { return _bm; }
        }
        public BarrelShifter BS
        {
            get { return _bs; }
        }

        internal void Stop()
        {
            if(exeuctionThread != null)
            {
                exeuctionThread.Abort();
            }
        }

    }
        public enum ARMInstruction : int
        {
            ADD,
            B,
            CMP,
            LDR,
            MOV,
            MVN,
            STR,
            SWI,
            NOP //For lines in the source file that are not instructions
        }
        public enum ConditionCode : int
        {
            CS,
            CC,
            EQ,
            NE,
            VS,
            VC,
            GT,
            GE,
            LT,
            LE,
            MI,
            PL,
            HI,
            HS,
            LO,
            LS,
            AL
        }
        public enum VariableType
        {
            DCD,
            DCW,
            DCB,
            ALIGN //Previous variable should be aligned to a word
        }
    public enum SourceLineType
    {
        TTL,
        AREA,
        ENTRY,
        CODE,
        LABEL,
        VARIABLE,
        END,
        BLANK,
        COMMENT
    }
        public enum CpuMode : int
        {
            fiq,
            usr,
            irq,
            svc,
            abt,
            und,
            sys

        }//end CpuMode

        //Number modes
        //# = base 10 number - decimal eg #1234 = 1234
        //0x = hex number - eg 0xA = 10
        //00 = base 10 number - decimal eg 1234 = 1234
        //&num = hex number

}