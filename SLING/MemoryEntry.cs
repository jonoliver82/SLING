using System;
using System.Collections.Generic;
using System.Text;

namespace SLING
{
    class MemoryEntry
    {
        private string _label;
        private UInt32 _data;
        private UInt32 _address;

        public MemoryEntry()
        {
        }

        public MemoryEntry(string label, UInt32 data, UInt32 address)
        {
            _label = label;
            _data = data;
            _address = address;
        }

        public string Label
        {
            get { return _label; }
        }

        public UInt32 Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public UInt32 Address
        {
            get { return _address; }
        }
    }
}
