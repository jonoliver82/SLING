using System;
using System.Collections.Generic;
using System.Text;

namespace SLING
{

    public delegate void BmAUpdatedEventHandler();
    public delegate void BmBUpdatedEventHandler();
    public delegate void BmRUpdatedEventHandler();
    
    public class BoothMultiplier
    {
        private UInt16 _A;
        private UInt16 _B;
        private UInt32 _result;

        public event BmAUpdatedEventHandler BmAUpdated;
        public event BmBUpdatedEventHandler BmBUpdated;
        public event BmRUpdatedEventHandler BmRUpdated;

        public BoothMultiplier()
        {
            Reset();
        }

        public void Reset()
        {
            _A = 0;
            _B = 0;
            _result = 0;
        }

        public void Calculate()
        {
            _result = (UInt32)_A * _B;
            if (BmRUpdated != null)
            {
                BmRUpdated();
            }
        }

        public UInt16 A
        {
            get { return _A; }
            set
            {
                _A = value;
                if (BmAUpdated != null)
                {
                    BmAUpdated();
                }
            }
        }

        public UInt16 B
        {
            get { return _B; }
            set
            {
                _B = value;
                if (BmBUpdated != null)
                {
                    BmBUpdated();
                }
            }
        }

        public UInt32 Result
        {
            get { return _result; }
        }
    }
}
