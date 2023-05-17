using System.Collections.Generic;

namespace CbmCode
{
    public class UndoBuffer
    {
        private List<string> _buffer;
        private int _indexPointer;

        public UndoBuffer()
        {
            _indexPointer = -1;
            _buffer = new List<string>();
        }

        public bool CanUndo => false;
        
        public bool CanRedo => false;

        public void PushState(string state)
        {

        }

        public string Undo()
        {
            return "";
        }

        public string Redo()
        {
            return "";
        }

        public void GetBufferState(out int bufferSize, out int indexPointer)
        {
            bufferSize = _buffer.Count;
            indexPointer = _indexPointer;
        }
    }
}