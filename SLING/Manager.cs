using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SLING
{
    public class Manager
    {               
        private SourceFile _file;
        private ARM6CPU _cpu;
        private static Manager instance;

        public Manager()
        {
            _cpu = new ARM6CPU();
        }

        public static void Reset()
        {
            Manager.instance = null;
            //_cpu.Reset();
            //_file.Reset();
        }

        public static Manager GetInstance()
        {
            if (Manager.instance == null)
            {
                Manager.instance = new Manager();
            }

            return Manager.instance;
        }
        
        public void LoadFile(ArrayList sourceLines)
        {
            _cpu.Reset();
            _file = new SourceFile(sourceLines);
        }

        public void Run()
        {
            _cpu.Run();
        }

        public void Step()
        {
            _cpu.Step();
        }

        public SourceFile File
        {
            get { return _file; }
        }

        public  void AddData(string label, uint variable)
        {
            _cpu.AddData(label, variable);
        }

        //This property for testing purposes only
        public ARM6CPU CPU
        {
            get { return _cpu; }
        }

        internal void Stop()
        {
            _cpu.Stop();
        }

    }
}
