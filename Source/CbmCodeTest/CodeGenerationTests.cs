using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
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
"0 print \"hello \";",
"1 print \"world\"");

            var source = SA(
@"//this is a comment
    print ""hello "";//so is this
    print ""world""     //as well as this
");
            var generate = new Generate(source);
            var (success, generatedLines) = generate.Do();
            Assert.IsTrue(success);
            CollectionAssert.AreEqual(expected, generatedLines.Last());
        }

        [TestMethod]
        public void SubstitutesVariables()
        {
            var expected = A(
"0 print \"we're substitutin' variables here mate!\"",
"1 input \"give me a sum and a message\"; A, A$",
"2 A% = 42");

            var source = SA(
@"print ""we're substitutin' variables here mate!""
@variables: count%, message$, _sum
    input ""give me a sum and a message""; _sum, message$//I wrote _sum to protect sum from being replaced.
    count% = 42");

            var generate = new Generate(source);
            var (success, generatedLines) = generate.Do();
            Assert.IsTrue(success);
            CollectionAssert.AreEqual(expected, generatedLines.Last());
        }

        [TestMethod]
        public void SubstituteOneVariable()
        {
            var expected = A(
"0 print \"we start here!\"",
"1 A = A + 1",
"2 goto 0");

            var source = SA(
@"@variables: counter
:loop   print ""we start here!""
        counter += 1
        goto loop
");

            var generate = new Generate(source);
            var (success, generatedLines) = generate.Do();
            Assert.IsTrue(success);
            CollectionAssert.AreEqual(expected, generatedLines.Last());
        }

        [TestMethod]
        public void SubstitutesLabels()
        {
            var expected = A(
"0 input \"enter a number\"; X",
"1 if X > 42 then 0");

            var source = SA(
@"
:loop input ""enter a number""; X
      if X > 42 then loop");

            var generate = new Generate(source);
            var (success, generatedLines) = generate.Do();
            Assert.IsTrue(success);
            CollectionAssert.AreEqual(expected, generatedLines.Last());
        }

        [TestMethod]
        public void LabelsCanBeDelimitedByTabs()
        {
            var expected = A(
"0 input \"enter a number\"; X",
"1 if X > 42 then 0");

            var source = SA(
@"
:loop	input ""enter a number""; X
		if X > 42 then loop");

            var generate = new Generate(source);
            var (success, generatedLines) = generate.Do();
            Assert.IsTrue(success);
            CollectionAssert.AreEqual(expected, generatedLines.Last());
        }

        [TestMethod]
        public void InlineConstants()
        {
            var expected = A("0 let X = 3.14 * 3 + 2.7");
            var source = SA(
@"@constants: PI = 3.14, E = 2.7
let X = PI * 3 + E");

            var generate = new Generate(source);
            var (success, generatedLines) = generate.Do();
            Assert.IsTrue(success);
            CollectionAssert.AreEqual(expected, generatedLines.Last());
        }

        [TestMethod]
        public void CompoundAssignment()
        {
            var expected = A("0 var = var + 42");
            var source = A("var += 42");

            var generate = new Generate(source);
            var (success, generatedLines) = generate.Do();
            Assert.IsTrue(success);
            CollectionAssert.AreEqual(expected, generatedLines.Last());
        }

        [TestMethod]

        string[] SA(string source) =>
            source.Split(A(Environment.NewLine), StringSplitOptions.None);

        T[] A<T>(params T[] args) =>
            args;
    }
}
