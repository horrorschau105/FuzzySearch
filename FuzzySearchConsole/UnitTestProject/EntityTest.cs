using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace FuzzySearchConsole
{
    [TestClass]
    public class EntityTest
    {
        [TestMethod]
        public void ToStringTest()
        {
            Entity en = new Entity("egg", 300, "before chicken");
            Assert.AreEqual(en.ToString(), "egg*300*before chicken");
        }
    }
}
