using System;
using System.Diagnostics;
using System.Linq;
using CbmCode.CodeGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CbmCodeTest.Common
{
    public abstract class CodeGenerationBase
    {
        protected T[] A<T>(params T[] args) =>
            args;

        protected string[] SA(string source) =>
            source.Split(A(Environment.NewLine), StringSplitOptions.None);

        protected static void Test(string[] expected, string[] actual)
        {
            var generate = new Generate(actual);
            var (success, generations) = generate.Do();
            Assert.IsTrue(success);

            var result = generations.Last().ToArray();

            Debug.WriteLine("EXPECTED:");
            Debug.WriteLine("=========");
            foreach (var s in expected)
                Debug.WriteLine(s);


            Debug.WriteLine("RESULT:");
            Debug.WriteLine("-------");
            foreach (var s in result)
                Debug.WriteLine(s);

            CollectionAssert.AreEqual(expected, result);
        }
    }
}