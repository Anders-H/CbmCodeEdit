using CbmCodeTest.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CbmCodeTest
{
    [TestClass]
    public class ReadmeSamplesTests : CodeGenerationBase
    {
        [TestMethod]
        public void HelloWorldExample1()
        {
            var expected = SA(@"0 print ""hello world""
1 goto 0");

            var source = SA(@":loop print ""hello world""
goto loop");

            Test(expected, source);
        }

        [TestMethod]
        public void HelloWorldExample2()
        {
            var expected = SA(@"0 rem""loop
1 print ""hello world""
2 goto 0");

            var source = SA(@":loop
print ""hello world""
goto loop");

            Test(expected, source);
        }
    }
}