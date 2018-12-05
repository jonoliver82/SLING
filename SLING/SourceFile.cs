using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SLING
{

    public delegate void LineChangedEventHandler(int lineNumber);
    
    public class SourceFile
    {

        public event LineChangedEventHandler LineChanged;
        private List<SourceLine> _lines = new List<SourceLine>();
        private int _currentLineIndex = 0;
        private int _nextLineIndex = 0;
        private int _entryLineIndex = 0;
        private string _lastLabel;


        public SourceFile(ArrayList lines)
        {
            //Add lines to the list
            foreach (object obj in lines)
            {
                _lines.Add(new SourceLine(obj as string));
            }

            //Set Indexes
            _entryLineIndex = _lines.FindIndex(EntryLine);
            _currentLineIndex = _entryLineIndex;
            //Next Line is first code line after Entry
            _nextLineIndex = _lines.FindIndex(_entryLineIndex, CodeLine);
            _lastLabel = String.Empty;

        }

        /// <summary>
        /// Finds the line in the list with type Entry
        /// </summary>
        /// <param name="theLine"></param>
        /// <returns></returns>
        private bool EntryLine(SourceLine theLine)
        {
            if (theLine.Type == SourceLineType.ENTRY)
            {
               return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Finds the next line in the list, with type CODE
        /// </summary>
        /// <param name="theLine"></param>
        /// <returns></returns>
        private bool CodeLine(SourceLine theLine)
        {
            if (theLine.Type == SourceLineType.CODE)
            {
               return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the next code line to be executed
        /// </summary>
        /// <returns></returns>
        public SourceLine GetNextLine()
        {
            _currentLineIndex = _nextLineIndex;
            if (LineChanged != null)
            {
                LineChanged(_currentLineIndex);
            }

            _nextLineIndex++;
            return _lines[_currentLineIndex];
        }

        /// <summary>
        /// Sets the nextLine to be executed back to the entry line
        /// </summary>
        public void Reset()
        {
            _nextLineIndex = _entryLineIndex;
            _currentLineIndex = _entryLineIndex;
            _lastLabel = String.Empty;
            if (LineChanged != null)
            {
                EventArgs e = new EventArgs();
                LineChanged(_currentLineIndex);
            }
        }

        public void SetNextLine(string label)
        {
            //Needed for support of Branch/Jump to labelled instructions
            _lastLabel = label;
            _nextLineIndex = _lines.FindIndex(LabelLine);
        }

        private bool LabelLine(SourceLine theLine)
        {
            if (theLine.HasLabel && theLine.Label.Equals(_lastLabel))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int EntryLineIndex
        {
            get { return _entryLineIndex; }
        }

        public int CurrentLineIndex
        {
            get { return _currentLineIndex; }
        }

        public SourceLine CurrentInstruction
        {
            get { return _lines[_currentLineIndex]; }
        }

        /// <summary>
        /// Used for unit testing purposes
        /// </summary>
        /// <param name="line"></param>
        public void SetNextLine(SourceLine line)
        {
            
            _lines.Add(line);
            _nextLineIndex = _lines.IndexOf(line);
        }

        public string LastLabel
        {
            get { return _lastLabel; }
        }
    }
}
