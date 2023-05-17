using Microsoft.VisualStudio.TestTools.UnitTesting;
using CbmCode;

namespace CbmCodeTest
{
    [TestClass]
    public class UndoBufferTests
    {
        [TestMethod]
        public void EmptyBufferCantUndoOrRedo()
        {
            var undoBuffer = new UndoBuffer();
            Assert.IsFalse(undoBuffer.CanUndo);
            Assert.IsFalse(undoBuffer.CanRedo);
            undoBuffer.GetBufferState(out var bufferSize, out var indexPointer);
            Assert.AreEqual(0, bufferSize);
            Assert.AreEqual(-1, indexPointer);
        }

        [TestMethod]
        public void OnePushedStateEqualsBufferSizeOfOne()
        {
            var undoBuffer = new UndoBuffer();
            undoBuffer.PushState("Hello");
            Assert.IsTrue(undoBuffer.CanUndo);
            Assert.IsFalse(undoBuffer.CanRedo);
            undoBuffer.GetBufferState(out var bufferSize, out var indexPointer);
            Assert.AreEqual(1, bufferSize);
            Assert.AreEqual(0, indexPointer);
        }
    }
}