using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CbmCode.CodeGeneration;

namespace CbmCodeTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var expected = A(
"1 print \"hello \";",
"2 print \"world\"");

            var source = SA(
@"//this is a comment
    print ""hello "";//so is this
    print ""world""     //as well as this
");
            var generate = new Generate(source);
            var (success, generatedLines) = generate.Do();
            Assert.IsTrue(success);
            CollectionAssert.AreEqual(expected, generatedLines);
        }

        string[] SA(string source) =>
            source.Split(A(Environment.NewLine), StringSplitOptions.None);

        string[] A(params string[] strings) =>
            strings;
    }
}
