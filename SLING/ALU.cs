using System;
using System.Collections.Generic;
using System.Text;

namespace SLING
{
    public delegate void AluAUpdatedEventHandler();
    public delegate void AluBUpdatedEventHandler();
    public delegate void AluCmdUpdatedEventHandler();
    public delegate void AluRUpdatedEventHandler();
    public delegate void AluFlagsUpdatedEventHandler();
    
    public class ALU
    {
        public event AluAUpdatedEventHandler AluAUpdated;
        public event AluBUpdatedEventHandler AluBUpdated;
        public event AluCmdUpdatedEventHandler AluCmdUpdated;
        public event AluRUpdatedEventHandler AluRUpdated;
        public event AluFlagsUpdatedEventHandler AluFlagsUpdated;
        
        private UInt32 _A;
        private UInt32 _B;
        private UInt32 _result;
        private ALUCommand _command;
        private UInt32 _flags;
        private bool _n;
        private bool _z;
        private bool _c;
        private bool _v;

        public ALU()
        {
            Reset();
        }

        public void Reset()
        {
            _A = 0;
            _B = 0;
            _result = 0;
            _command = ALUCommand.NONE;
            _flags = 0;
            _n = false;
            _z = false;
            _c = false;
            _v = false;
        }


        public void Calculate()
        {
            switch (_command)
            {
                case ALUCommand.ADD:
                    {
                        _result = _A + _B;                        
                        SetFlags();
                        break;
                    }
                case ALUCommand.SUBTRACT:
                    {
                        _result = _A - _B;
                        SetFlags();
                        break;
                    }
                case ALUCommand.NOT:
                    {
                        _result = ~_A;
                        SetFlags();
                        break;
                    }
                default:
                    break;
            }
            if (AluRUpdated != null)
            {
                AluRUpdated();
            }
        }

        private void SetFlags()
        {
            _z = _result == 0 ? true : false;
            _n = _result < 0 ? true : false;
            
            ///TODO: Check this
            _c = _result < 0 ? true : false;//NOT borrowFrom(Rn-ShifterOperand)

            if (_command == ALUCommand.ADD)
            {
                //OverFlowFrom
                //Generates overflow if both operands (A,B) have same sign (bit 31) a
                //and sign of result is different to the sign of both operands
                if ((_A >= 0 && _B >= 0 && _result < 0) || (_A < 0 && _B < 0 && _result > 0))
                {
                    _v = true;
                }
            }
            else if(_command == ALUCommand.SUBTRACT)
            {
                if ((_A >= 0 && _B < 0 && _result < 0) || (_A < 0 && _B >= 0 && _result >= 0))
                {
                    _v = true;
                }
            }                                                   
            else
            {
                _v = false; //OverflowFrom(Rn-ShifterOperand)
            }
            
            if (AluFlagsUpdated != null)
            {
                AluFlagsUpdated();
            }
        }

        public UInt32 A
        {
            get { return _A; }
            set
            {
                _A = value; 
                if (AluAUpdated != null)
                {
                    AluAUpdated();
                }                    
            }
        }

        public UInt32 B
        {
            get { return _B; }
            set
            {
                _B = value;
                if (AluBUpdated != null)
                {
                    AluBUpdated();
                }
            }
        }

        public UInt32 Result
        {
            get { return _result; }
        }

        public UInt32 Flags
        {
            get { return _flags; }
        }

        public bool Flag_N
        {
            get { return _n; }
            set { _n = value; }
        }

        public bool Flag_Z
        {
            get { return _z; }
            set { _z = value; }
        }

        public bool Flag_C
        {
            get { return _c; }
            set { _c = value; }
        }

        public bool Flag_V
        {
            get { return _v; }
            set { _v = value; }
        }

        public ALUCommand Command
        {
            get { return _command; }
            set
            {
                _command = value;
                if (AluCmdUpdated != null)
                {
                    AluCmdUpdated();
                }
            }
        }
    }

    public enum ALUCommand : int
    {
        ADD,
        SUBTRACT,
        AND,
        OR,
        EOR,
        NOT,
        NONE
    }
}
