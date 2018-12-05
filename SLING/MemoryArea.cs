using System;
using System.Collections.Generic;
using System.Text;

namespace SLING
{
    public class MemoryArea
    {
        private uint _writeIndex = 0;

        //Memory Address -> MemoryEntry
        private Dictionary<UInt32, MemoryEntry> _data = new Dictionary<UInt32, MemoryEntry>();

        /// <summary>
        /// Constructor
        /// </summary>
        public MemoryArea()
        {
            _writeIndex = 0;
            _data.Clear();
        }

        /// <summary>
        /// Add data with given label into next available word in memory
        /// </summary>
        /// <param name="label"></param>
        /// <param name="data"></param>
        public void Add(string label, UInt32 data)
        {
            MemoryEntry entry = new MemoryEntry(label, data, _writeIndex);

            //Could add check here that no entry already exists with this label first

            _data.Add(_writeIndex, entry);

            //Increment index by 4 bytes (32 bits)
            _writeIndex = _writeIndex + 4; 
        }

        /// <summary>
        /// Return address in memory of labelled data
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public UInt32 Address(string label)
        {
            //Loop through memory entries
            //return address of MemoryEntry where label matches label in memory
            //Reverse Lookup of dictionary
            foreach (MemoryEntry entry in _data.Values)
            {
                //Only returns first address found - so assumes no memory contents with same label
                if(entry.Label.Equals(label))
                {
                    return entry.Address;
                }
            }

            return 0;
        }

        /// <summary>
        /// Return data held at specified memory address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public UInt32 Data(UInt32 address)
        {
            return (_data[address]).Data;
        }

        /// <summary>
        /// Return data held at specified labelled memory location
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public UInt32 Data(string label)
        {
            foreach (MemoryEntry entry in _data.Values)
            {
                //Only returns first address found - so assumes no memory contents with same label
                if (entry.Label.Equals(label))
                {
                    return entry.Data;
                }
            }
            return 0;
        }

        /// <summary>
        /// Store the given data in the pre-existing named memory location
        /// </summary>
        /// <param name="label"></param>
        /// <param name="data"></param>
        public void Store(string label, UInt32 data)
        {
            foreach (MemoryEntry entry in _data.Values)
            {
                if (entry.Label.Equals(label))
                {
                    //Update data
                    entry.Data = data;
                }
            }
        }

        /// <summary>
        /// Clear memory - CPU reset
        /// </summary>
        public void Clear()
        {
            _writeIndex = 0;
            _data.Clear();
        }


    }
}
