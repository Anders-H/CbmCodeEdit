using System;
using System.Collections.Generic;
using System.Linq;

namespace CbmCode
{
    public class UndoBuffer
    {
        private readonly List<string> _buffer;
        private int _indexPointer;

        public UndoBuffer()
        {
            _indexPointer = -1;
            _buffer = new List<string>();
            PushState("");
        }

        public bool CanUndo(string currentState) =>
            _buffer.Count > 0 && _indexPointer >= 0 && _indexPointer < _buffer.Count && _buffer[_indexPointer] != currentState;
        
        public bool CanRedo =>
            _buffer.Count > 0 && _indexPointer < _buffer.Count - 1;

        public void PushState(string state)
        {
            if (_buffer.Count > 0 && state == _buffer.Last())
                return;

            while (_buffer.Count > _indexPointer + 1)
                _buffer.RemoveAt(_buffer.Count - 1);

            _buffer.Add(state);
            _indexPointer = _buffer.Count - 1;
        }

        public string Undo(string currentState)
        {
            if (!CanUndo(currentState))
                throw new SystemException("Can't undo.");

            var ret = _buffer[_indexPointer];
            _indexPointer--;
            return ret;
        }

        public string Redo()
        {
            if (!CanRedo)
                throw new SystemException("Can't redo.");

            _indexPointer++;
            return _buffer[_indexPointer];
        }

        public void GetBufferState(out int bufferSize, out int indexPointer)
        {
            bufferSize = _buffer.Count;
            indexPointer = _indexPointer;
        }
    }
}