using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace SLING
{
    public partial class Form1
    {
        
        // Following animations are required:
        //
        // External Memory to register (LDR)
        // *Done* register to external memory (STR)
        // register to ALU (A)
        // register to ALU (B)
        // ALU (Result) to register
        // ALU (Flags) to CPSR

        private int _mbrXPos = 252;
        private int _mbrYPos = 80;

        private int _marXPos = 80;
        private int _marYPos = 80;

        private int _aluXPos = 140;
        private int _aluYPos = 420;
        private int _aluAnimPt1XPos = 10;
        private int _aluAnimPt1YPos = 440;

        private int _aluAnimPt2XPos = 30;
        private int _aluAnimPt2YPos = 180;

        private int _aluBottomXPos = 140;
        private int _aluBottomYPos = 440;

        private int _bsXPos = 240;
        private int _bsYPos = 380;


        private int _x = 0;
        private int _y = 0;
        private int _width = 0;
        private int _height = 0;
        private SolidBrush _brush = new SolidBrush(Color.Red);

        //Graphic definitions
        //Need to check these, dependent on size of final form
        private int[] _registerXPositions = new int[17] {0,250,230,210,190,250,230,210,190,250,230,210,190,250,230,210,190};
        private int[] _registerYPositions = new int[17] { 0, 210, 190, 170, 150, 210, 190, 170, 150, 210, 190, 170, 150, 210, 190, 170, 150 };
        
        
        private float _externalMemoryYpos = 10.0F;

    }
}
