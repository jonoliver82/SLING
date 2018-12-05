using System;
using System.Collections.Generic;
using System.Text;

namespace SLING
{
    public class SourceLine
    {
        private string _text;
        
        private ARMInstruction _instruction = ARMInstruction.NOP;
        private ConditionCode _code = ConditionCode.AL;
        private SourceLineType _type = SourceLineType.BLANK;

        private bool _setFlags = false;
        private int _Rd = 0;
        private int _Rn = 0;

        private int _Rm = 0;
        private int _Rs = 0;

        private string _Op1;
        private string _Op2;

        private string _label;

        private bool _shift = false;
        private int _shiftRegister = 0;
        private ShiftCommand _shiftType = ShiftCommand.NON;
        private int _shiftAmount = 0;
  
        private UInt32 _variable;
        //Needed for 64 bit data processing
        //private UInt32 _variable2;
        
        public SourceLine(string text)
        {
            _text = text;
            bool valid = ParseLine();
            if (!valid)
            {
                throw new ArgumentException("Source Line not vaid", "text");
            }
        }

        private bool ParseLine()
        {

            //A Blank line in the source file
            if (_text.Length == 0)
            {
                _type = SourceLineType.BLANK;
                _instruction = ARMInstruction.NOP;
                return true;
            }

            //Line is a comment
            if (_text[0] == ';')
            {
                _type = SourceLineType.COMMENT;
                _instruction = ARMInstruction.NOP;
                return true;
            }
               
            //Remove any comments from the line
            string temp = _text;
            int commentStartIndex = _text.IndexOf(';');
            if (commentStartIndex > 0)
            {
                temp = _text.Remove(commentStartIndex);
            }

            //Trim white space
            temp = temp.Trim();

            //Split into fields delimited by whitespace
            string[] fields = temp.Split(new char[] { ' ', '\t' });

            //Is the first field a directive, label, or instruction?
            // Refactor into bool isDirective(string) 
            Array typeArray = Enum.GetValues(typeof(SourceLineType));
            foreach (SourceLineType st in typeArray)
            {
                if (fields[0].Equals(st.ToString()))
                {
                    _type = st;
                    _label = st.ToString();
                    _instruction = ARMInstruction.NOP;

                    

                    //Ignore any text following a directive to the assembler
                    return true;
                }
            }

            //MOV has 3 fields at this point:
            //MOV
            //R1,R1,LSL
            //#0x1


            //Only one field, not a directive so must be a label
            if(fields.Length == 1)
            {
               _label = fields[0];
               _type = SourceLineType.LABEL;
               _instruction = ARMInstruction.NOP;
               return true; 
            }

            //If there are 3 fields, the first must be a label, the second the instruction 
            //and third operands, or theres is no label and a shift in the second field
            bool shiftOperation = false;
            foreach (string cmd in Enum.GetNames(typeof(ShiftCommand)))
            {
                if (fields[1].Contains(cmd))
                {
                    shiftOperation = true;
                    break;
                }
            }

            int instructionIndex = 0;
            int operandIndex = 0;
            int shiftAmountIndex = 0;

            if (shiftOperation == true)
            {
                instructionIndex = 0;
                operandIndex = 1;
                shiftAmountIndex = 2;
            }
            else
            {
                //If there are 2 fields, the first must be the instruction and the second the operand
                //int labelIndex = fields.Length > 2 ? 0 : -1;
                instructionIndex = fields.Length > 2 ? 1 : 0;
                operandIndex = fields.Length > 2 ? 2 : 1;
            }

            //Store the label if present
            if ((fields.Length > 2) && (shiftOperation == false))
            {
                _label = fields[0]; 
               
            }

            //Instruction field contains instruction, condition code and set flags, or variable type
            bool foundInstruction = false;
            Array opArray = Enum.GetValues(typeof(ARMInstruction));
            foreach (ARMInstruction op in opArray)
            {
                if (fields[instructionIndex].StartsWith(op.ToString()))
                {
                    _type = SourceLineType.CODE;
                    _instruction = op;
                    foundInstruction = true;
                    break;
                }
            }

            if (foundInstruction)
            {
                int instrLength = _instruction.ToString().Length;
                int codeLength = fields[instructionIndex].Length - instrLength;
                switch (codeLength)
                {
                    case 0:
                        {
                            //No Condition or set flags directive
                            _code = ConditionCode.AL;
                            _setFlags = false;
                            break;
                        }
                    case 1:
                        {
                            //Must be Set Flags directive only
                            if (fields[instructionIndex][instrLength + 1] == 'S')
                            {
                                _setFlags = true;
                            }
                            else
                            {
                                //Single character should only be S - all condition codes are two characters
                                _setFlags = false;
                                return false;
                            }
                            _code = ConditionCode.AL;
                            break;
                        }
                    case 2:
                        {
                            //Must be a condition code only
                            string codeString = fields[instructionIndex].Substring(instrLength, 2);
                            Array ccArray = Enum.GetValues(typeof(ConditionCode));
                            foreach (ConditionCode cc in ccArray)
                            {
                                if (codeString.Equals(cc.ToString()))
                                {
                                    _code = cc;
                                    break;
                                }
                            }
                            _setFlags = false;
                            break;
                        }
                    case 3:
                        {
                            //Must be a condition code then set flags
                            string codeString = fields[instructionIndex].Substring(instrLength, 2);
                            Array ccArray = Enum.GetValues(typeof(ConditionCode));
                            foreach (ConditionCode cc in ccArray)
                            {
                                if (codeString.Equals(cc.ToString()))
                                {
                                    _code = cc;
                                    break;
                                }
                            }
                            if (fields[instructionIndex][instrLength + 2] == 'S')
                            {
                                _setFlags = true;
                            }
                            else
                            {
                                //Character following condition code must only be S
                                _setFlags = false;
                                return false;
                            }
                            break;
                        }
                    default:
                        {
                            //Invalid characters following instruction 
                            _setFlags = false;
                            return false;
                        }
                }
            }
            else           
            {
                //Not directive, label or instruction. Should be a variable definition.
                Array vtArray = Enum.GetValues(typeof(VariableType));
                foreach (VariableType vt in vtArray)
                {
                    if (fields[instructionIndex].Equals(vt.ToString()))
                    {
                        _type = SourceLineType.VARIABLE;
                        _instruction = ARMInstruction.NOP;
                        _code = ConditionCode.AL;
                        _setFlags = false;
                    }
                }

            }

            //Final field contains operands, split by commas
            string[] operands = fields[operandIndex].Split(new char[] { ',' });

            //For a variable definition, store first operand only for now
            if (_type == SourceLineType.VARIABLE)
            {
                //Variable definitions typically have one operand, but some may have two to 
                //perform operations with 64bit numbers

                //The type of the value to be assigned to the variable
                System.Globalization.NumberStyles type = System.Globalization.NumberStyles.HexNumber;
                string tempVar = operands[0];

                if (tempVar[0] == '&')
                {
                    type = System.Globalization.NumberStyles.HexNumber;
                    tempVar = tempVar.Remove(0, 1);
                }
                else if (tempVar[0] == '#')
                {
                    type = System.Globalization.NumberStyles.Number;
                    tempVar = tempVar.Remove(0, 1);
                }
                else if (tempVar.StartsWith("0x"))
                {
                    type = System.Globalization.NumberStyles.HexNumber;
                    tempVar = tempVar.Remove(0, 2);
                }
                else
                {
                    type = System.Globalization.NumberStyles.Number;
                }

                //temp now contains only a value, hex or decimal
                _variable = UInt32.Parse(tempVar, type);

                //Call the manager to add this to the memory representation
                Manager.GetInstance().AddData(_label, _variable);
            }
            else
            {
                //Operands are for an instruction

                //SWI &11           Op1
                if (operands.Length == 1)
                {
                    //Also check for B, BL offset values

                    if (_instruction == ARMInstruction.SWI)
                    {
                        string val = operands[0];
                        //Remove '&'
                        _Op1 = val.Remove(0, 1);
                    }

                    if (_instruction == ARMInstruction.B)
                    {
                        //store label to branch to if condition met
                        _Op1 = operands[0];
                    }

                }
                else if (operands.Length == 2)
                {
                    if (_instruction == ARMInstruction.CMP)
                    {
                        string val = operands[0];
                        //Remove 'R'
                        val = val.Remove(0, 1);
                        _Rn = Int32.Parse(val, System.Globalization.NumberStyles.Number);
                    }
                    else
                    {

                        //First operand is always Rd (destination register)
                        string val = operands[0];
                        //Remove 'R'
                        val = val.Remove(0, 1);
                        _Rd = Int32.Parse(val, System.Globalization.NumberStyles.Number);
                    }
                    //Second operand will be stored in Op1 or Op2


                    //LDR R1,Value1     Rd, Op2
                    //LDR R2,Value1     Rd, Op2
                    //STR R1,Result     Rd, Op2
                    if (_instruction == ARMInstruction.LDR || _instruction == ARMInstruction.STR)
                    {
                        //Store label for op2
                        _Op2 = operands[1];
                    }
                    //MOV R1, R1        Rd, Op1  (register)
                    //MOV R1, #1234     Rd, Op1  (immediate)
                    //MVN R1, R1        Rd, Op1  (register)
                    //CMP R1, R2        Rn, Op1   (register)
                    else
                    {
                        //Need to store indication of register or immediate for analysis at run-time
                        _Op1 = operands[1];
                    }
                }
                else
                {
                    //First operand is always Rd (destination register)
                    string val = operands[0];
                    //Remove 'R'
                    val = val.Remove(0, 1);
                    _Rd = Int32.Parse(val, System.Globalization.NumberStyles.Number);
                    
                    
                    //ADD R1,R1,R2      Rd, Rn, Op1
                    if (_instruction == ARMInstruction.ADD)
                    {
                        string rnVal = operands[1];
                        rnVal = rnVal.Remove(0, 1);
                        _Rn = Int32.Parse(rnVal, System.Globalization.NumberStyles.Number);

                        //Need to store indication of register or immediate for analysis at run-time
                        _Op1 = operands[2];

                    }
                    //MOV R1,R1,LSL #0x1    Rd,Op1
                    //MOV R1,R1,LSL R2    Rd,Op1 //Shift by amount in R2
                    if (_instruction == ARMInstruction.MOV)
                    {
                        //MOV with 3 operands - must be a shift
                        //shift op1 by the method in op2 by the amount in field[shiftIndex]
                        _shift = true;

                        //Destination Always a register - remove R indicator
                        //Register contains the number that we want to shift
                        string shiftRegVal = operands[1];
                        shiftRegVal = shiftRegVal.Remove(0, 1);
                        _shiftRegister = Int32.Parse(shiftRegVal, System.Globalization.NumberStyles.Number);

                        //Operand 2 contains the type of shift
                          _shiftType = (ShiftCommand)Enum.Parse(typeof(ShiftCommand), operands[2]);

                        //Shift amount immediate (&,# or register?)
                        System.Globalization.NumberStyles type = System.Globalization.NumberStyles.HexNumber;
                        string tempVar = fields[shiftAmountIndex];
                        if (tempVar[0] == '&')
                        {
                            type = System.Globalization.NumberStyles.HexNumber;
                            tempVar = tempVar.Remove(0, 1);
                        }
                        else if (tempVar[0] == '#')
                        {
                            if (fields[shiftAmountIndex].StartsWith("#0x"))
                            {
                                type = System.Globalization.NumberStyles.HexNumber;
                                tempVar = tempVar.Remove(0, 3);
                            }
                            else
                            {
                                type = System.Globalization.NumberStyles.Number;
                                tempVar = tempVar.Remove(0, 1);
                            }
                        }
                        else
                        {
                            type = System.Globalization.NumberStyles.Number;
                        }

                        //temp now contains only a value, hex or decimal
                        _shiftAmount = Int32.Parse(tempVar, type);

                        //Store shifter operand - if register then we use this value, otherwise use immediate value
                        //stored in shiftAmount
                        _Op1 = fields[shiftAmountIndex];



                    }

                    //Need special case for SWP - uses Rm, Rd, Rn...
                }
       


            }





            return true;
       }

        public ARMInstruction Instruction
        {
            get
            {
                return _instruction;
            }
            set
            {
                _instruction = value;
            }

        }

        public ConditionCode Code
        {
            get
            {
                return _code;
            }
        }

        public bool SetFlags
        {
            get
            {
                return _setFlags;
            }
        }

        public bool HasLabel
        {
            get
            {
                return _label != null;
            }
        }

        public string Text
        {
            get
            {
                return _text;
            }
        }

        public string Label
        {
            get
            {
                return _label;
            }
        }

        public SourceLineType Type
        {
            get
            {
                return _type;
            }
        }

        public UInt32 Variable
        {
            get
            {
                return _variable;
            }
        }

        public int Rd
        {
            get
            {
                return _Rd;
            }
        }

        public int Rn
        {
            get
            {
                return _Rn;
            }
        }

        public int Rm
        {
            get
            {
                return _Rm;
            }
        }

        public int Rs
        {
            get
            {
                return _Rs;
            }
        }

        public string Op1
        {
            get
            {
                return _Op1;
            }
        }

        public string Op2
        {
            get
            {
                return _Op2;
            }
        }

        public bool Shift
        {
            get { return _shift; }
        }

        public int ShiftRegister
        {
            get { return _shiftRegister; }
        }

        public int ShiftAmount
        {
            get { return _shiftAmount; }
        }

        public ShiftCommand ShiftType
        {
            get { return _shiftType; }
        }
    }
}
