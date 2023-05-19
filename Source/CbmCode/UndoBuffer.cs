using System;
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

        public bool CanUndo =>
            _buffer.Count > 0 && _indexPointer >= 0;
        
        public bool CanRedo =>
            _buffer.Count > 0 && _indexPointer < _buffer.Count - 1;

        public void PushState(string state)
        {
            while (_buffer.Count > _indexPointer + 1)
                _buffer.RemoveAt(_buffer.Count - 1);

            _buffer.Add(state);
            _indexPointer = _buffer.Count - 1;
        }

        public string Undo()
        {
            if (!CanUndo)
                throw new SystemException("Can't undo.");

            var ret = _buffer[_indexPointer];
            _indexPointer--;
            return ret;
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