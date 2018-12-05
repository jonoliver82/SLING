using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace SLING
{
    public partial class Form1 : Form
    {


        private delegate void RegisterUpdatedDelegate(int registerNumber);
        private delegate void MemoryUpdatedDelegate(int sourceRegister);
        private delegate void MBRfromMemoryUpdatedDelegate();
        private delegate void AluAUpdatedDelegate();
        private delegate void AluBUpdatedDelegate();
        private delegate void AluCmdUpdatedDelegate();
        private delegate void AluResultUpdatedDelegate();
        private delegate void AluFlagsUpdatedDelegate();
        private delegate void CpsrFlagsUpdatedDelegate();
        private delegate void CpuMarUpdatedDelegate();
        private delegate void CpuMbrUpdatedDelegate();
        private delegate void CpuIrUpdatedDelegate();
        private delegate void BsAUpdatedDelegate();
        private delegate void BsBUpdatedDelegate();
        private delegate void BsCmdUpdatedDelegate();
        private delegate void BsRUpdatedDelegate();
        private delegate void BmAUpdatedDelegate();
        private delegate void BmBUpdatedDelegate();
        private delegate void BmRUpdatedDelegate();

        private Manager _theManager;
        private bool _animationOn = true;
        private int _animationSpeed = 10;
        private int _flashSpeed = 750;
        

        
        public Form1()
        {
            InitializeComponent();

            _theManager = Manager.GetInstance();

            InitialiseDisplay();

            
        }

        private void InitialiseDisplay()
        {
            ARM6CPU theCPU = _theManager.CPU;
            
            //Registers
            r0textBox.Text = theCPU.Registers[0].ToString("X");
            r1textBox.Text = theCPU.Registers[1].ToString("X");
            r2textBox.Text = theCPU.Registers[2].ToString("X");
            r3textBox.Text = theCPU.Registers[3].ToString("X");
            r4textBox.Text = theCPU.Registers[4].ToString("X");
            r5textBox.Text = theCPU.Registers[5].ToString("X");
            r6textBox.Text = theCPU.Registers[6].ToString("X");
            r7textBox.Text = theCPU.Registers[7].ToString("X");
            r8textBox.Text = theCPU.Registers[8].ToString("X");
            r9textBox.Text = theCPU.Registers[9].ToString("X");
            r10textBox.Text = theCPU.Registers[10].ToString("X");
            r11textBox.Text = theCPU.Registers[11].ToString("X");
            r12textBox.Text = theCPU.Registers[12].ToString("X");
            r13textBox.Text = theCPU.Registers[13].ToString("X");
            r14textBox.Text = theCPU.Registers[14].ToString("X");
            r15textBox.Text = theCPU.Registers[15].ToString("X");

            //CPSR    
            cpsrNcheckBox.Checked = theCPU.CPSR_N;
            cpsrZcheckBox.Checked = theCPU.CPSR_Z;
            cpsrCcheckBox.Checked = theCPU.CPSR_C;
            cpsrVcheckBox.Checked = theCPU.CPSR_V;
            cpsrIcheckBox.Checked = theCPU.CPSR_I;
            cpsrFcheckBox.Checked = theCPU.CPSR_F;
            cpuModeTextBox.Text = theCPU.CPUMode.ToString();

            //ALU
            aluRatextBox.Text = theCPU.Alu.A.ToString("X");
            aluRbtextBox.Text = theCPU.Alu.B.ToString("X");
            aluCmdtextBox.Text = theCPU.Alu.Command.ToString();
            aluRtextBox.Text = theCPU.Alu.Result.ToString("X");
            aluNcheckBox.Checked = theCPU.Alu.Flag_N;
            aluZcheckBox.Checked = theCPU.Alu.Flag_Z;
            aluCcheckBox.Checked = theCPU.Alu.Flag_C;
            aluVcheckBox.Checked = theCPU.Alu.Flag_V;

            //Booth Multiplier
            bmRatextBox.Text = theCPU.BM.A.ToString("X");
            bmRbtextBox.Text = theCPU.BM.B.ToString("X");
            bmRestextBox.Text = theCPU.BM.Result.ToString("X");

            //Barrel Shifter
            bsAtextBox.Text = theCPU.BS.A.ToString();
            bsBtextBox.Text = theCPU.BS.B.ToString();
            bsCmdtextBox.Text = theCPU.BS.Command.ToString();
            bsRtextBox.Text = theCPU.BS.Result.ToString();

            //Memory
            martextBox.Text = theCPU.MAR;
            mbrtextBox.Text = theCPU.MBR.ToString("X");

            stepToolStripMenuItem.Enabled = false;
            runToolStripMenuItem.Enabled = false;



        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog odlg = new OpenFileDialog();

            odlg.InitialDirectory = @"C:\ARM200\WORK";
            odlg.Title = "Open File";
            odlg.Filter = "S files (*.s)|*.s|All files (*.*)|*.*";

            if (odlg.ShowDialog() == DialogResult.OK)
            {

                listBox1.Items.Clear();
                
                string line;

                ArrayList lines = new ArrayList();

                // Read the file and display it line by line.
                listBox1.BeginUpdate();
                
                System.IO.StreamReader file = new System.IO.StreamReader(odlg.FileName);
                while ((line = file.ReadLine()) != null)
                {
                    lines.Add(line);
                    listBox1.Items.Add(line + Environment.NewLine);
                }

                listBox1.EndUpdate();
                try
                {

                    _theManager.LoadFile(lines);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to parse file:" + ex.Message);
                    listBox1.Items.Clear();
                    return;

                }
                finally
                {
                    file.Close();
                }

                listBox1.SelectedIndex = _theManager.File.EntryLineIndex;

                _theManager.CPU.MarChanged += new MARChangedEventHandler(CPU_MarChanged);
                _theManager.CPU.MbrChanged += new MBRChangedEventHandler(CPU_MbrChanged);
                _theManager.CPU.RegisterChangedFromMBR += new RegisterChangedFromMBREventHandler(CPU_RegisterChangedFromMBR);
                _theManager.CPU.RegisterChangedFromAluResult += new RegisterChangedFromAluResultEventHandler(CPU_RegisterChangedFromAluResult);
                _theManager.CPU.RegisterChangedFromBsResult += new RegisterChangedFromBsResultEventHandler(CPU_RegisterChangedFromBsResult);
                _theManager.CPU.MemoryChangedFromMBR += new MemoryChangedFromMBREventHandler(CPU_MemoryChangedFromMBR);
                _theManager.CPU.IrChanged += new IRChangedEventHandler(CPU_IrChanged);
                _theManager.CPU.MBRfromMemory += new MBRfromMemoryEventHandler(CPU_MBRfromMemory);
                _theManager.CPU.MBRChangedFromRegister += new MBRChangedFromRegisterEventHandler(CPU_MBRChangedFromRegister);
                _theManager.CPU.RegisterUpdated += new RegisterUpdatedEventHandler(CPU_RegisterUpdated);
                _theManager.CPU.CpsrFlagsUpdated += new CpsrFlagsUpdatedEventHandler(Cpu_CpsrFlagsUpdated);

                _theManager.File.LineChanged += new LineChangedEventHandler(File_LineChanged);

                _theManager.CPU.Alu.AluAUpdated += new AluAUpdatedEventHandler(Alu_AluAUpdated);
                _theManager.CPU.Alu.AluBUpdated += new AluBUpdatedEventHandler(Alu_AluBUpdated);
                _theManager.CPU.Alu.AluCmdUpdated += new AluCmdUpdatedEventHandler(Alu_AluCmdUpdated);
                _theManager.CPU.Alu.AluFlagsUpdated += new AluFlagsUpdatedEventHandler(Alu_AluFlagsUpdated);
                _theManager.CPU.Alu.AluRUpdated += new AluRUpdatedEventHandler(Alu_AluRUpdated);

                _theManager.CPU.BM.BmAUpdated += new BmAUpdatedEventHandler(BM_BmAUpdated);
                _theManager.CPU.BM.BmBUpdated += new BmBUpdatedEventHandler(BM_BmBUpdated);
                _theManager.CPU.BM.BmRUpdated += new BmRUpdatedEventHandler(BM_BmRUpdated);

                _theManager.CPU.BS.BsAUpdated += new BsAUpdatedEventHandler(BS_BsAUpdated);
                _theManager.CPU.BS.BsBUpdated += new BsBUpdatedEventHandler(BS_BsBUpdated);
                _theManager.CPU.BS.BsCmdUpdated += new BsCmdUpdatedEventHandler(BS_BsCmdUpdated);
                _theManager.CPU.BS.BsRUpdated += new BsRUpdatedEventHandler(BS_BsRUpdated);

                //IR
                irtextBox.Text = _theManager.File.CurrentInstruction.Instruction.ToString();

                stepToolStripMenuItem.Enabled = true;
                runToolStripMenuItem.Enabled = true;

            }
            odlg.Dispose();

        }

        void CPU_MBRChangedFromRegister(int registerNumber)
        {
            if (_animationOn == true)
            {
                //animate from Register to MBR
                //TODO: account for animate in X-axis to R>4

                _x = _mbrXPos;
                _width = 20;
                _height = 20;
                _brush.Color = Color.Blue;

                for (float pos = _registerYPositions[registerNumber]; pos > _mbrYPos; pos--)
                {
                    _y = (int)pos;
                    pictureBox1.Refresh();
                    Thread.Sleep(_animationSpeed);
                }

                //Remove the graphic after the operation is complete
                _brush.Color = Color.Empty;
                pictureBox1.Refresh();
            }

        }

        void CPU_MemoryChangedFromMBR()
        {
            if (_animationOn == true)
            {
                //Animate from MBR to External Memory

                _x = _mbrXPos;
                _width = 20;
                _height = 20;
                _brush.Color = Color.Green;

                for (float pos = _mbrYPos; pos > _externalMemoryYpos; pos--)
                {
                    _y = (int)pos;
                    pictureBox1.Refresh();
                    Thread.Sleep(_animationSpeed);
                }

                //Remove the graphic after the operation is complete
                _brush.Color = Color.Empty;
                pictureBox1.Refresh();
            }

        }


        void CPU_RegisterChangedFromMBR(int registerNumber)
        {
            if (_animationOn == true)
            {
                //Animate from MBR to RegisterN

                //TODO: account for animate in X-axis to R>4.
                _x = _mbrXPos;
                _width = 20;
                _height = 20;
                _brush.Color = Color.Blue;

                for (float pos = _mbrYPos; pos < _registerYPositions[registerNumber]; pos++)
                {
                    _y = (int)pos;
                    pictureBox1.Refresh();
                    Thread.Sleep(_animationSpeed);
                }

                //Remove the graphic after the operation is complete
                _brush.Color = Color.Empty;
                pictureBox1.Refresh();
            }

        }

        void CPU_RegisterChangedFromBsResult(int registerNumber)
        {
            if (_animationOn == true)
            {
                //Animate from BS to RegisterN
                _x = _bsXPos;
                _width = 20;
                _height = 20;
                _brush.Color = Color.Blue;

                //BS to ALU
                for (float pos = _bsYPos; pos < _aluBottomYPos; pos++)
                {
                    _y = (int)pos;
                    pictureBox1.Refresh();
                    Thread.Sleep(_animationSpeed);
                }

                _x = _aluXPos;
                //Then ALU to Result as before...
                //ALU to bottom
                for (float pos = _aluYPos; pos < _aluBottomYPos; pos++)
                {
                    _y = (int)pos;
                    pictureBox1.Refresh();
                    Thread.Sleep(_animationSpeed);
                }

                //ALU to Point 1
                for (float pos = _aluBottomXPos; pos > _aluAnimPt1XPos; pos--)
                {
                    _x = (int)pos;
                    pictureBox1.Refresh();
                    Thread.Sleep(_animationSpeed);
                }

                //Point 1 to Point 2
                for (float pos = _aluAnimPt1YPos; pos > _registerYPositions[registerNumber]; pos--)
                {
                    _y = (int)pos;
                    pictureBox1.Refresh();
                    Thread.Sleep(_animationSpeed);
                }


                //Point 2 to Register N
                for (float pos = _aluAnimPt2XPos; pos < _registerXPositions[registerNumber]; pos++)
                {
                    _x = (int)pos;
                    pictureBox1.Refresh();
                    Thread.Sleep(_animationSpeed);
                }

                //Remove the graphic after the operation is complete
                _brush.Color = Color.Empty;
                pictureBox1.Refresh();
            }
        }


        void CPU_RegisterChangedFromAluResult(int registerNumber)
        {
            if (_animationOn == true)
            {
                //Animate from ALU to RegisterN
                _x = _aluXPos;
                _width = 20;
                _height = 20;
                _brush.Color = Color.Blue;

                //ALU to bottom
                for (float pos = _aluYPos; pos < _aluBottomYPos; pos++)
                {
                    _y = (int)pos;
                    pictureBox1.Refresh();
                    Thread.Sleep(_animationSpeed);
                }

                //ALU to Point 1
                for (float pos = _aluBottomXPos; pos > _aluAnimPt1XPos; pos--)
                {
                    _x = (int)pos;
                    pictureBox1.Refresh();
                    Thread.Sleep(_animationSpeed);
                }

                //Point 1 to Point 2
                for (float pos = _aluAnimPt1YPos; pos > _registerYPositions[registerNumber]; pos--)
                {
                    _y = (int)pos;
                    pictureBox1.Refresh();
                    Thread.Sleep(_animationSpeed);
                }


                //Point 2 to Register N
                for (float pos = _aluAnimPt2XPos; pos < _registerXPositions[registerNumber]; pos++)
                {
                    _x = (int)pos;
                    pictureBox1.Refresh();
                    Thread.Sleep(_animationSpeed);
                }

                //Remove the graphic after the operation is complete
                _brush.Color = Color.Empty;
                pictureBox1.Refresh();
            }

        }

        void CPU_MBRfromMemory()
        {
            if (_animationOn == true)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new MBRfromMemoryUpdatedDelegate(CPU_MBRfromMemory));
                }
                else
                {
                    //Draw line from MAR to Memory
                    _x = _marXPos;
                    _width = 20;
                    _height = 20;
                    _brush.Color = Color.Green;

                    for (float pos = _marYPos; pos > _externalMemoryYpos; pos--)
                    {
                        _y = (int)pos;
                        pictureBox1.Refresh();
                        Thread.Sleep(_animationSpeed);
                    }


                    //Draw line from Memory to MBR
                    _x = _mbrXPos;
                    _width = 20;
                    _height = 20;
                    _brush.Color = Color.Green;

                    for (float pos = _externalMemoryYpos; pos < _mbrYPos; pos++)
                    {
                        _y = (int)pos;
                        pictureBox1.Refresh();
                        Thread.Sleep(_animationSpeed);
                    }

                    //Remove the graphic after the operation is complete
                    _brush.Color = Color.Empty;
                    pictureBox1.Refresh();

                }
            }
        }

        void CPU_IrChanged()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new CpuIrUpdatedDelegate(CPU_IrChanged));
            }
            else
            {
                irtextBox.Text = _theManager.CPU.IR.ToString();
                Flash(ref irtextBox);
            }
        }

        void BS_BsRUpdated()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new BsRUpdatedDelegate(BS_BsRUpdated));
            }
            else
            {
                bsRtextBox.Text = _theManager.CPU.BS.Result.ToString("X");
                Flash(ref bsRtextBox);
            }
        }

        void BS_BsCmdUpdated()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new BsCmdUpdatedDelegate(BS_BsCmdUpdated));
            }
            else
            {
                bsCmdtextBox.Text = _theManager.CPU.BS.Command.ToString();
                Flash(ref bsCmdtextBox);
            }
        }

        void BS_BsBUpdated()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new BsBUpdatedDelegate(BS_BsBUpdated));
            }
            else
            {
                bsBtextBox.Text = _theManager.CPU.BS.B.ToString();
                Flash(ref bsBtextBox);
            }
        }

        void BS_BsAUpdated()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new BsAUpdatedDelegate(BS_BsAUpdated));
            }
            else
            {
                bsAtextBox.Text = _theManager.CPU.BS.A.ToString("X");
                Flash(ref bsAtextBox);
            }
        }

        void BM_BmRUpdated()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new BmRUpdatedDelegate(BM_BmRUpdated));
            }
            else
            {
                bmRestextBox.Text = _theManager.CPU.BM.Result.ToString("X");
                Flash(ref bmRestextBox);
            }
        }

        void BM_BmBUpdated()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new BmBUpdatedDelegate(BM_BmBUpdated));
            }
            else
            {
                bmRbtextBox.Text = _theManager.CPU.BM.B.ToString("X");
                Flash(ref bmRbtextBox);
            }
        }

        void BM_BmAUpdated()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new BmAUpdatedDelegate(BM_BmAUpdated));
            }
            else
            {
                bmRatextBox.Text = _theManager.CPU.BM.A.ToString("X");
                Flash(ref bmRatextBox);
            }
        }

        void Alu_AluRUpdated()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new AluResultUpdatedDelegate(Alu_AluRUpdated));
            }
            else
            {
                aluRtextBox.Text = _theManager.CPU.Alu.Result.ToString("X");
                Flash(ref aluRtextBox);
            }
        }

        private void Flash(ref TextBox box)
        {
            box.BackColor = Color.Red;
            box.Refresh();
            Thread.Sleep(_flashSpeed);
            box.BackColor = Color.Empty;
            box.Refresh();
        }

        void Alu_AluFlagsUpdated()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new AluFlagsUpdatedDelegate(Alu_AluFlagsUpdated));
            }
            else
            {

                aluNcheckBox.Checked = _theManager.CPU.Alu.Flag_N;
                aluZcheckBox.Checked = _theManager.CPU.Alu.Flag_Z;
                aluCcheckBox.Checked = _theManager.CPU.Alu.Flag_C;
                aluVcheckBox.Checked = _theManager.CPU.Alu.Flag_V;

                aluNcheckBox.BackColor = Color.Red;
                aluZcheckBox.BackColor = Color.Red;
                aluCcheckBox.BackColor = Color.Red;
                aluVcheckBox.BackColor = Color.Red;

                aluNcheckBox.Refresh();
                aluZcheckBox.Refresh();
                aluCcheckBox.Refresh();
                aluVcheckBox.Refresh();

                Thread.Sleep(_flashSpeed);

                aluNcheckBox.BackColor = Color.Empty;
                aluZcheckBox.BackColor = Color.Empty;
                aluCcheckBox.BackColor = Color.Empty;
                aluVcheckBox.BackColor = Color.Empty;

                aluNcheckBox.Refresh();
                aluZcheckBox.Refresh();
                aluCcheckBox.Refresh();
                aluVcheckBox.Refresh();

            }
        }

        void Cpu_CpsrFlagsUpdated()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new CpsrFlagsUpdatedDelegate(Cpu_CpsrFlagsUpdated));
            }
            else
            {

                cpsrNcheckBox.Checked = _theManager.CPU.CPSR_N;
                cpsrZcheckBox.Checked = _theManager.CPU.CPSR_Z;
                cpsrCcheckBox.Checked = _theManager.CPU.CPSR_C;
                cpsrVcheckBox.Checked = _theManager.CPU.CPSR_V;

                cpsrNcheckBox.BackColor = Color.Red;
                cpsrZcheckBox.BackColor = Color.Red;
                cpsrCcheckBox.BackColor = Color.Red;
                cpsrVcheckBox.BackColor = Color.Red;

                cpsrNcheckBox.Refresh();
                cpsrZcheckBox.Refresh();
                cpsrCcheckBox.Refresh();
                cpsrVcheckBox.Refresh();

                Thread.Sleep(_flashSpeed);

                cpsrNcheckBox.BackColor = Color.Empty;
                cpsrZcheckBox.BackColor = Color.Empty;
                cpsrCcheckBox.BackColor = Color.Empty;
                cpsrVcheckBox.BackColor = Color.Empty;

                cpsrNcheckBox.Refresh();
                cpsrZcheckBox.Refresh();
                cpsrCcheckBox.Refresh();
                cpsrVcheckBox.Refresh();

            }
        }

        void Alu_AluCmdUpdated()
        {
           if (this.InvokeRequired)
            {
                this.Invoke(new AluCmdUpdatedDelegate(Alu_AluCmdUpdated));
            }
            else
            {
                aluCmdtextBox.Text = _theManager.CPU.Alu.Command.ToString();
                Flash(ref aluCmdtextBox);
            }
        }

        void Alu_AluBUpdated()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new AluBUpdatedDelegate(Alu_AluBUpdated));
            }
            else
            {
                aluRbtextBox.Text = _theManager.CPU.Alu.B.ToString("X");
                Flash(ref aluRbtextBox);
            }
        }

        void Alu_AluAUpdated()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new AluAUpdatedDelegate(Alu_AluAUpdated));
            }
            else
            {
                aluRatextBox.Text = _theManager.CPU.Alu.A.ToString("X");
                Flash(ref aluRatextBox);
            }
        }

        void CPU_MbrChanged()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new CpuMbrUpdatedDelegate(CPU_MbrChanged));
            }
            else
            {
                mbrtextBox.Text = _theManager.CPU.MBR.ToString("X");
                Flash(ref mbrtextBox);
            }
        }

        void CPU_MarChanged()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new CpuMarUpdatedDelegate(CPU_MarChanged));
            }
            else
            {
                martextBox.Text = _theManager.CPU.MAR;
                Flash(ref martextBox);
            }
        }

        private void File_LineChanged(int index)
        {
            listBox1.SelectedIndex = index;
            r15textBox.Text = Convert.ToString(index + 1);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(" SLING ARM Support Tool \n Jonathan Oliver \n Bournemouth University \n BSc Computing (Hons) \n 14.04.07");
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            _theManager.Run();
        }

        private void CPU_RegisterUpdated(int registerNumber)
        {          
            
            if (this.InvokeRequired)
            {
                // Pass the same function to Invoke,
                // but the call would come on the correct
                // thread and InvokeRequired will be false.
                this.Invoke(new RegisterUpdatedDelegate(CPU_RegisterUpdated),
                    new object[] {registerNumber});
            }
            else
            {
                switch (registerNumber)
                {
                    case 0: 
                        {
                            r0textBox.Text = _theManager.CPU.Registers[0].ToString("X");
                            Flash(ref r0textBox);
                            break;
                        }
                    case 1:
                        {
                            r1textBox.Text = _theManager.CPU.Registers[1].ToString("X");
                            Flash(ref r1textBox);
                            break;
                        }
                    case 2:
                        {
                            r2textBox.Text = _theManager.CPU.Registers[2].ToString("X");
                            Flash(ref r2textBox);
                            break;
                        }
                    case 3:
                        {
                            r3textBox.Text = _theManager.CPU.Registers[3].ToString("X");
                            Flash(ref r3textBox);
                            break;
                        }
                    case 4:
                        {
                            r4textBox.Text = _theManager.CPU.Registers[4].ToString("X");
                            Flash(ref r4textBox);
                            break;
                        }
                    case 5:
                        {
                            r5textBox.Text = _theManager.CPU.Registers[5].ToString("X");
                            Flash(ref r5textBox);
                            break;
                        }
                    case 6:
                        {
                            r6textBox.Text = _theManager.CPU.Registers[6].ToString("X");
                            Flash(ref r6textBox);
                            break;
                        }
                    case 7:
                        {
                            r7textBox.Text = _theManager.CPU.Registers[7].ToString("X");
                            Flash(ref r7textBox);
                            break;
                        }
                    case 8:
                        {
                            r8textBox.Text = _theManager.CPU.Registers[8].ToString("X");
                            Flash(ref r8textBox);
                            break;
                        }
                    case 9:
                        {
                            r9textBox.Text = _theManager.CPU.Registers[9].ToString("X");
                            Flash(ref r9textBox);
                            break;
                        }
                    case 10:
                        {
                            r10textBox.Text = _theManager.CPU.Registers[10].ToString("X");
                            Flash(ref r10textBox);
                            break;
                        }
                    case 11:
                        {
                            r11textBox.Text = _theManager.CPU.Registers[11].ToString("X");
                            Flash(ref r11textBox);
                            break;
                        }
                    case 12:
                        {
                            r12textBox.Text = _theManager.CPU.Registers[12].ToString("X");
                            Flash(ref r12textBox);
                            break;
                        }
                    case 13:
                        {
                            r13textBox.Text = _theManager.CPU.Registers[13].ToString("X");
                            Flash(ref r13textBox);
                            break;
                        }
                    case 14:
                        {
                            r14textBox.Text = _theManager.CPU.Registers[14].ToString("X");
                            Flash(ref r14textBox);
                            break;
                        }
                    default:
                        {
                            r15textBox.Text = _theManager.CPU.Registers[15].ToString("X");
                            Flash(ref r15textBox);
                            break;
                        }
                }

            }
        }

        //private void updateMemoryDisplay(int sourceRegister)
        //{
        //    if (this.InvokeRequired)
        //    {
        //        // Pass the same function to Invoke,
        //        // but the call would come on the correct
        //        // thread and InvokeRequired will be false.
        //        this.Invoke(new MemoryUpdatedDelegate(updateMemoryDisplay),
        //            new object[] { sourceRegister });
        //    }
        //    else
        //    {
        //        //Draw line from register[n] to memory

        //        _x = _registerXPositions[sourceRegister];
        //        _width = 20;
        //        _height = 20;
        //        _brush.Color = Color.Blue;

        //        for (float pos = _registerYPositions[sourceRegister]; pos > _externalMemoryYpos; pos--)
        //        {
        //            _y = (int)pos;
        //            pictureBox1.Refresh();
        //            Thread.Sleep(_animationSpeed);                    
        //        }

        //        //Remove the graphic after the operation is complete
        //        _brush.Color = Color.Empty;
        //        pictureBox1.Refresh();

        //    }
        //}

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _theManager.Stop();
            this.Close();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.FillEllipse(_brush, _x, _y, _width, _height);
        }

        private void Reset()
        {
            Manager.Reset();
            //Get new instance of Manager
            _theManager = Manager.GetInstance();
            InitialiseDisplay();
            listBox1.Items.Clear();


        }

        private void displayCPUToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.pictureBox1.Visible == true)
            {
                this.pictureBox1.Visible = false;
                //this.panel4.Location.X = pictureBox1.Location.X;
            }
            else
            {
                this.pictureBox1.Visible = true;
            }
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _theManager.Run();
        }

        private void stepToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _theManager.Step();
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private void animationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_animationOn == true)
            {
                //Turn off animation
                _animationOn = false;
                animationToolStripMenuItem.Checked = false;
            }
            else
            {
                //Turn on animation
                _animationOn = true;
                animationToolStripMenuItem.Checked = true;
            }
        }

    }
}