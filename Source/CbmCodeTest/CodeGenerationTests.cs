using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CbmCode.CodeGeneration;

namespace CbmCodeTest
{
    [TestClass]
    public class CodeGenerationTests
    {
        [TestMethod]
        public void RemovesComments()
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

        [TestMethod]
        public void SubstitutesVariables()
        {
            var expected = A(
"1 print \"we're substitutin' variables here mate!\"",
"2 input \"give me a sum and a message\"; A, A$",
"3 A% = 42");

            var source = SA(
@"print ""we're substitutin' variables here mate!""
@variables: count%, message$, _sum
    input ""give me a sum and a message""; _sum, message$
    count% = 42");//Note: I wrote the sum variable as _sum to prevent the substring 'sum' to be replaced.

            var generate = new Generate(source);
            var (success, generatedLines) = generate.Do();
            Assert.IsTrue(success);
            CollectionAssert.AreEqual(expected, generatedLines);
        }

        [TestMethod]
        public void SubstitutesLabels()
        {
            var expected = A(
"1 input \"enter a number\"; X",
"2 if X > 42 then 1");

            var source = SA(
@"
:loop input ""enter a number""; X
      if X > 42 then loop");

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
