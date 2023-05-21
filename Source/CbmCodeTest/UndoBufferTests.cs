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
            AssertCantUndoOrRedo(undoBuffer);
            AssertBufferSizeAndIndexPointer(undoBuffer, 0, -1);
        }

        [TestMethod]
        public void OnePushedStateEqualsBufferSizeOfOne()
        {
            var undoBuffer = new UndoBuffer();
            undoBuffer.PushState("Hello");
            AssertCanUndoButNotRedo(undoBuffer);
            AssertBufferSizeAndIndexPointer(undoBuffer, 1, 0);
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
            var undo = undoBuffer.Undo();
            Assert.AreEqual("Hello 1", undo);
            Assert.IsFalse(undoBuffer.CanUndo);
            Assert.IsTrue(undoBuffer.CanRedo);
            Assert.ThrowsException<SystemException>(() => undoBuffer.Undo());
        }

        [TestMethod]
        public void IgnoreDuplicatePush()
        {
            var undoBuffer = new UndoBuffer();
            undoBuffer.PushState("Hello 1");
            AssertBufferSizeAndIndexPointer(undoBuffer, 1, 0);
            undoBuffer.PushState("Hello 2");
            AssertBufferSizeAndIndexPointer(undoBuffer, 2, 1);
            undoBuffer.PushState("Hello Duplicate");
            AssertBufferSizeAndIndexPointer(undoBuffer, 3, 2);
            undoBuffer.PushState("Hello Duplicate");
            AssertBufferSizeAndIndexPointer(undoBuffer, 3, 2);
        }

        [TestMethod]
        public void CanRedo()
        {
            var undoBuffer = new UndoBuffer();
            undoBuffer.PushState("Hello 1");
            undoBuffer.PushState("Hello 2");
            undoBuffer.PushState("Hello 3");
            AssertCanUndoButNotRedo(undoBuffer);
            AssertBufferSizeAndIndexPointer(undoBuffer, 3, 2);
            undoBuffer.Undo();
            AssertCanUndoAndRedo(undoBuffer);
            AssertBufferSizeAndIndexPointer(undoBuffer, 3, 1);
            var redo = undoBuffer.Redo();
            AssertCanUndoButNotRedo(undoBuffer);
            AssertBufferSizeAndIndexPointer(undoBuffer, 3, 2);
            Assert.AreEqual("Hello 3", redo);
        }

        [TestMethod]
        public void PushCutsBufferAfterUndo()
        {
            var undoBuffer = new UndoBuffer();
            undoBuffer.PushState("Hello 1");
            undoBuffer.PushState("Hello 2");
            undoBuffer.PushState("Hello 3");
            undoBuffer.PushState("Hello 4");
            undoBuffer.PushState("Hello 5");
            undoBuffer.PushState("Hello 6");
            AssertCanUndoButNotRedo(undoBuffer);
            AssertBufferSizeAndIndexPointer(undoBuffer, 6, 5);
            undoBuffer.Undo();
            undoBuffer.Undo();
            undoBuffer.Undo();
            AssertCanUndoAndRedo(undoBuffer);
            AssertBufferSizeAndIndexPointer(undoBuffer, 6, 2);
            undoBuffer.PushState("New Hello");
            AssertBufferSizeAndIndexPointer(undoBuffer, 4, 3);
            AssertCanUndoButNotRedo(undoBuffer);
        }

        private void AssertCantUndoOrRedo(UndoBuffer b)
        {
            Assert.IsFalse(b.CanUndo);
            Assert.IsFalse(b.CanRedo);
        }

        private void AssertCanUndoButNotRedo(UndoBuffer b)
        {
            Assert.IsTrue(b.CanUndo);
            Assert.IsFalse(b.CanRedo);
        }

        private void AssertCanUndoAndRedo(UndoBuffer b)
        {
            Assert.IsTrue(b.CanUndo);
            Assert.IsTrue(b.CanRedo);
        }

        private void AssertBufferSizeAndIndexPointer(UndoBuffer b, int bufferSize, int indexPointer)
        {
            b.GetBufferState(out var actualBufferSize, out var actualIndexPointer);
            Assert.AreEqual(bufferSize, actualBufferSize);
            Assert.AreEqual(indexPointer, actualIndexPointer);
        }
    }
}