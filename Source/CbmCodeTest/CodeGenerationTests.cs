using Microsoft.VisualStudio.TestTools.UnitTesting;
using CbmCodeTest.Common;

namespace CbmCodeTest
{
    [TestClass]
    public class CodeGenerationTests : CodeGenerationBase
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
            Test(expected, source);
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

            Test(expected, source);
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

            Test(expected, source);
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

            Test(expected, source);
        }

        [TestMethod]
        public void InlineConstants()
        {
            var expected = A("0 let X = 3.14 * 3 + 2.7");
            var source = SA(
@"@constants: PI = 3.14, E = 2.7
let X = PI * 3 + E");

            Test(expected, source);
        }

        [TestMethod]
        public void CompoundAssignment()
        {
            var expected = A("0 VAR = VAR + 42");
            var source = A("var += 42");
            Test(expected, source);

            expected = A("0 VAR = VAR AND 42");
            source = A("var and= 42");
            Test(expected, source);
        }

        [TestMethod]
        public void LabelCanStandOnItsOwn()
        {
            var expected = A(
"0 rem\"start",
"1 goto 0");
            var source = SA(
@":start
goto start");

            Test(expected, source);
        }

        [TestMethod]
        public void NotCompoundAssignment()
        {
            var expected = A("0 for A=1 to 10");
            var source = SA(
@"@variables: counter
for counter=1 to 10");

            Test(expected, source);

            expected = A("0 for counter=1 to 10");
            source = SA(@"for counter=1 to 10");
            Test(expected, source);

            expected = A("0 for var%=1 to 10");
            source = SA(@"for var%=1 to 10");
            Test(expected, source);

            expected = A("0 for var$=1 to 10");
            source = SA(@"for var$=1 to 10");
            Test(expected, source);
        }
    }
}
