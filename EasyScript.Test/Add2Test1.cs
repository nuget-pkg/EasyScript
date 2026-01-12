using System;
using System.Collections.Generic;
using System.Linq;
 using NUnit.Framework;
using Global;

public class DynamicTest
{
    [SetUp]
    public void Setup()
    {
        Console.WriteLine("Setup() called");
    }

    [Test]
    public void Test01()
    {
            //Assert.That(EasyScript.Add2(111, 222), Is.EqualTo(333));
            //Assert.That(e.Value.Cast<string>(), Is.EqualTo("AAA"));
            //string ss = e.Value.Dynamic;
            //Assert.That(ss, Is.EqualTo("AAA"));
            //Assert.That((string)(e.Value.Dynamic), Is.EqualTo("AAA"));
    }
}