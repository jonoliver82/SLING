using System;
using System.Collections.Generic;
using System.Text;

namespace SLING
{

    public delegate void BsAUpdatedEventHandler();
    public delegate void BsBUpdatedEventHandler();
    public delegate void BsCmdUpdatedEventHandler();
    public delegate void BsRUpdatedEventHandler();
    
    public class BarrelShifter
    {
        private UInt32 _A; //Number
        private int _B; //Value to shift by
        private ShiftCommand _command;
        private UInt32 _result;

        public event BsAUpdatedEventHandler BsAUpdated;
        public event BsBUpdatedEventHandler BsBUpdated;
        public event BsCmdUpdatedEventHandler BsCmdUpdated;
        public event BsRUpdatedEventHandler BsRUpdated;

        public BarrelShifter()
        {
            Reset();
        }

        public void Reset()
        {
            _A = 0;
            _B = 0;
            _command = ShiftCommand.NON;
            _result = 0;
        }

        public void Calculate()
        {
            ///TODO: Set Flags (C)
            switch (_command)
            {
                case ShiftCommand.LSL:
                    {
                        _result = _A << _B;
                        break;
                    }
                case ShiftCommand.LSR:
                    {
                        _result = _A >> _B;
                        break;
                    }
                default:
                    break;
            }
            if (BsRUpdated != null)
            {
                BsRUpdated();
            }
        }

        public UInt32 A
        {
            get { return _A; }
            set
            {
                _A = value;
                if (BsAUpdated != null)
                {
                    BsAUpdated();
                }
            }
        }

        public int B
        {
            get { return _B; }
            set
            {
                _B = value;
                if (BsBUpdated != null)
                {
                    BsBUpdated();
                }
            }
        }

        public UInt32 Result
        {
            get { return _result; }
        }

        public ShiftCommand Command
        {
            get { return _command; }
            set
            {
                _command = value;
                if (BsCmdUpdated != null)
                {
                    BsCmdUpdated();
                }
            }
        }

    }

    public enum ShiftCommand : int
    {
        LSL,
        LSR,
        ASR,
        ROR,
        RRE,
        NON
    }
}
