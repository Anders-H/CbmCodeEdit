using System;
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

        [TestMethod]
        public void CanUndo()
        {
            var undoBuffer = new UndoBuffer();
            undoBuffer.PushState("Hello 1");
            undoBuffer.PushState("Hello 2");
            undoBuffer.PushState("Hello 3");
            AssertCanUndoButNotRedo(undoBuffer);
            AssertBufferSizeAndIndexPointer(undoBuffer, 3, 2);
            var result = undoBuffer.Undo();
            Assert.AreEqual("Hello 3", result);
            AssertBufferSizeAndIndexPointer(undoBuffer, 3, 1);
            result = undoBuffer.Undo();
            Assert.AreEqual("Hello 2", result);
            AssertBufferSizeAndIndexPointer(undoBuffer, 3, 0);
            Assert.IsTrue(undoBuffer.CanUndo);
            Assert.IsTrue(undoBuffer.CanRedo);
            result = undoBuffer.Undo();
            Assert.AreEqual("Hello 1", result);
            AssertBufferSizeAndIndexPointer(undoBuffer, 3, -1);
            Assert.IsFalse(undoBuffer.CanUndo);
            Assert.IsTrue(undoBuffer.CanRedo);
        }

        [TestMethod]
        public void CrashIfUndoWhenCant()
        {
            var undoBuffer = new UndoBuffer();
            undoBuffer.PushState("Hello 1");
            undoBuffer.PushState("Hello 2");
            undoBuffer.PushState("Hello 3");
            undoBuffer.Undo();
            undoBuffer.Undo();
            undoBuffer.Undo();
            Assert.IsFalse(undoBuffer.CanUndo);
            Assert.IsTrue(undoBuffer.CanRedo);
            Assert.ThrowsException<SystemException>(() => undoBuffer.Undo());
        }

        private void AssertCanUndoButNotRedo(UndoBuffer b)
        {
            Assert.IsTrue(b.CanUndo);
            Assert.IsFalse(b.CanRedo);
        }

        private void AssertBufferSizeAndIndexPointer(UndoBuffer b, int bufferSize, int indexPointer)
        {
            b.GetBufferState(out var actualBufferSize, out var actualIndexPointer);
            Assert.AreEqual(bufferSize, actualBufferSize);
            Assert.AreEqual(indexPointer, actualIndexPointer);
        }
    }
}