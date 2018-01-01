using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace FuzzySearchConsole
{
    [TestClass]
    public class QGramIndexTest
    {
        [TestMethod]
        public void Constructor()
        {
            QGramIndex qgi = new QGramIndex(5);
            Assert.AreEqual(qgi.entities.Count, 0);
            Assert.AreEqual(qgi.padding, "$$$$");
            Assert.AreEqual(qgi.qgramLength, 5);
        }
        [TestMethod]
        public void ComputeQGrams()
        {
            QGramIndex qgi = new QGramIndex(3);
            var qgrams = qgi.ComputeQGrams("pancake");
            var expected = new List<string>() {"$$p", "$pa", "pan", "anc", "nca", "cak", "ake" };
            Assert.AreEqual(qgrams.Count, expected.Count);
            for(int i=0;i<qgrams.Count;++i)
            {
                Assert.AreEqual(expected[i], qgrams[i]);
            }
        }
        [TestMethod]
        public void NormalizeString()
        {
            Assert.AreEqual(QGramIndex.NormalizeString("freiburg"), "freiburg");
            Assert.AreEqual(QGramIndex.NormalizeString(" FREI !$@ BURG!"), "freiburg");
        }
        [TestMethod]
        public void BuildFromFile()
        {
            QGramIndex qgi = new QGramIndex(3);
            qgi.BuildFromFile("example.txt");

            Assert.AreEqual(2, qgi.entities.Count);
            Assert.IsTrue(qgi.invertedLists.ContainsKey("$$f"));
            Assert.IsTrue(qgi.invertedLists.ContainsKey("$fr"));
            Assert.IsTrue(qgi.invertedLists.ContainsKey("$br"));
            Assert.IsTrue(qgi.invertedLists.ContainsKey("$$b"));
            Assert.IsTrue(qgi.invertedLists.ContainsKey("rei"));
            Assert.IsTrue(qgi.invertedLists.ContainsKey("fre"));
            Assert.IsTrue(qgi.invertedLists.ContainsKey("bre"));
            Assert.AreEqual(7, qgi.invertedLists.Keys.Count);
        }
        [TestMethod]
        public void PrefixEditDistance()
        {
            Assert.AreEqual(QGramIndex.PrefixEditDistance("frei", "frei", 0), 0);
            Assert.AreEqual(QGramIndex.PrefixEditDistance("frei", "freiburg", 0), 0);
            Assert.AreEqual(QGramIndex.PrefixEditDistance("frei", "breifurg", 1), 1);
            Assert.AreEqual(QGramIndex.PrefixEditDistance("freiburg", "stuttgart", 2), 3);
        }
        [TestMethod]
        public void CountIds()
        {
            var result = QGramIndex.CountIds(new List<int>() { 1, 1, 2, 3, 3, 3, 5, 9, 9 });
            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(new KeyValuePair<int, int>(1, 2), result[0]);
            Assert.AreEqual(new KeyValuePair<int, int>(2, 1), result[1]);
            Assert.AreEqual(new KeyValuePair<int, int>(3, 3), result[2]);
            Assert.AreEqual(new KeyValuePair<int, int>(5, 1), result[3]);
            Assert.AreEqual(new KeyValuePair<int, int>(9, 2), result[4]);
        }
        [TestMethod]
        public void Flatten()
        {
            var result = QGramIndex.Flatten(new List<List<int>>() { new List<int>() { 1, 3, 4 },
                                                             new List<int>() { 2, 3, 4 },
                                                             new List<int>() { 2, 2, 3 },
                                                             new List<int>() { 1, 1, 2 } });
            Assert.AreEqual(12, result.Count);
            var expected = new List<int>() { 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 4, 4 };
            for (int i = 0; i < result.Count; ++i) 
            {
                Assert.AreEqual(expected[i], result[i]);
            }
        }
        [TestMethod]
        public void RankMatches()
        {
            KeyValuePair<Entity, int> pair1 = new KeyValuePair<Entity, int>(new Entity("foo", 3, "word 0"), 2);
            KeyValuePair<Entity, int> pair2 = new KeyValuePair<Entity, int>(new Entity("bar", 7, "word 1"), 0);
            KeyValuePair<Entity, int> pair3 = new KeyValuePair<Entity, int>(new Entity("baz", 2, "word 2"), 1);
            KeyValuePair<Entity, int> pair4 = new KeyValuePair<Entity, int>(new Entity("boo", 5, "word 3"), 1);
            var result = QGramIndex.RankMatches(new List<KeyValuePair<Entity, int>>() { pair1, pair2, pair3, pair4 });
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(pair2, result[0]);
            Assert.AreEqual(pair4, result[1]);
            Assert.AreEqual(pair3, result[2]);
            Assert.AreEqual(pair1, result[3]);
        }
        [TestMethod]
        public void FindMatches()
        {
            QGramIndex qgi = new QGramIndex(3);
            qgi.BuildFromFile("example.txt");
            
            var output = qgi.FindMatches("frei", 0);
            Assert.AreEqual(1, output.Value);
            Assert.AreEqual(1, output.Key.Count);
            Assert.AreEqual(0, output.Key[0].Value);
            Assert.AreEqual("frei*3*a word", output.Key[0].Key.ToString());

            output = qgi.FindMatches("frei", 2);
            Assert.AreEqual(2, output.Value);
            Assert.AreEqual(2, output.Key.Count);
            Assert.AreEqual(0, output.Key[0].Value);
            Assert.AreEqual("frei*3*a word", output.Key[0].Key.ToString());
            Assert.AreEqual(1, output.Key[1].Value);
            Assert.AreEqual("brei*2*another word", output.Key[1].Key.ToString());

            output = qgi.FindMatches("freibu", 2);
            Assert.AreEqual(2, output.Value);
            Assert.AreEqual(1, output.Key.Count);
            Assert.AreEqual("frei*3*a word", output.Key[0].Key.ToString());
            Assert.AreEqual(2, output.Key[0].Value);
        }
    }
}
