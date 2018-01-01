using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FuzzySearchConsole
{
    public class QGramIndex
    {
        public List<Entity> entities;
        public int qgramLength;
        public Dictionary<String, List<int>> invertedLists;
        public String padding;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="q">length of qgram in class</param>
        public QGramIndex(int q)
        {
            qgramLength = q;
            entities = new List<Entity>();
            invertedLists = new Dictionary<string, List<int>>();
            padding = "";
            for(int i = 1; i < q; ++i)
            {
                padding += '$';
            }
        }
        public void BuildFromFile(string filename)
        {
            string[] lines = File.ReadAllLines(filename);
            int lineId = 0;
            foreach(var line in lines)
            {
                if (lineId != 0)
                {
                    var words = line.Split('\t');
                    Entity en = new Entity(words[0], Int32.Parse(words[1]),
                                           words[2]);
                    entities.Add(en);
                    foreach (var qgram in ComputeQGrams(words[0]))
                    {
                        if (!invertedLists.ContainsKey(qgram))
                        {
                            invertedLists.Add(qgram, new List<int>());
                        }
                        invertedLists[qgram].Add(lineId);
                    }
                }
                lineId++;
            }
        }
        public static int PrefixEditDistance(string word1, string word2, int delta)
        {
            int length1 = word1.Length + 1;
            int length2 = Math.Min(length1 + delta, word2.Length) + 1;
            int[,] dpMatrix = new int[length1,length2];
            for(int i = 0; i < length1; ++i)
            {
                dpMatrix[i, 0] = i;
            }
            for (int i = 0; i < length2; ++i) 
            {
                dpMatrix[0, i] = i;
            }
            int cost;
            for (int i = 1; i < length1; ++i) 
            {
                for (int j = 1; j < length2; ++j)
                {
                    cost = (word1[i - 1] == word2[j - 1]) ? 0 : 1;
                    dpMatrix[i, j] = Math.Min(1 + Math.Min(dpMatrix[i, j - 1], 
                                                           dpMatrix[i - 1, j]),
                                              dpMatrix[i - 1, j - 1] + cost);
                }
            }
            int result = dpMatrix[length1 - 1, 0];
            for (int i = 1; i < length2; ++i)
            {
                result = Math.Min(result, dpMatrix[length1 - 1, i]);
            }
            if(result <= delta)
            {
                return result;
            }
            return delta + 1;
        }
        public KeyValuePair<List<KeyValuePair<Entity, int>>, int> FindMatches
            (string query, int delta)
        {
            List<List<int>> lists = new List<List<int>>();
            foreach(string qgram in ComputeQGrams(query))
            {
                if(invertedLists.Keys.Contains(qgram))
                {
                    lists.Add(invertedLists[qgram]);
                }
            }
            int computations = 0;
            int ped;
            List<KeyValuePair<int, int>> potencials = CountIds(Flatten(lists));
            List<KeyValuePair<Entity, int>> results = new List<KeyValuePair<Entity, int>>();
            foreach(var potencial in potencials)
            {
                if(potencial.Value >= query.Length - qgramLength * delta)
                {
                    computations++;
                    ped = PrefixEditDistance(query, entities[potencial.Key - 1].name, delta);
                    if(ped < delta + 1)
                    {
                        results.Add(new KeyValuePair<Entity, int>(entities[potencial.Key - 1], ped));
                    }
                }
            }
            return new KeyValuePair<List<KeyValuePair<Entity, int>>, int>(results, computations);

        }
        public static List<KeyValuePair<Entity, int>> RankMatches(List<KeyValuePair<Entity, int>> list)
        {
            list = list.OrderBy(elem => elem.Value)
                       .ThenByDescending(elem => elem.Key.score)
                       .ToList();
            return list;
        }

        // helpers

        public static string NormalizeString(string word)
        {
            return new string(word.ToLower().ToCharArray().Where(c => Char.IsLetter(c)).ToArray());
        }
        public static List<KeyValuePair<int, int>> CountIds(List<int> list)
        {
            List<KeyValuePair<int, int>> result = new List<KeyValuePair
                                                             <int, int>>();
            int count;
            for (int i = 1; i < list.Count(); ++i)
            {
                count = 1;
                while (i < list.Count() && list[i] == list[i - 1])
                {
                    i++;
                    count++;
                }
                result.Add(new KeyValuePair<int, int>(list[i - 1], count));
                if (i == list.Count() - 1)
                {
                    result.Add(new KeyValuePair<int, int>(list[i], 1));
                }
            }
            return result;
        }
        public static List<int> Flatten(List<List<int>> lists)
        {
            var result = new List<int>();
            foreach (var list in lists)
            {
                result.AddRange(list);
            }
            result.Sort();
            return result;
        }
        public List<string> ComputeQGrams(string word)
        {
            List<string> result = new List<string>();
            String paddedWord = padding + NormalizeString(word);
            for (int i = 0; i < paddedWord.Length - qgramLength + 1; ++i)
            {
                result.Add(paddedWord.Substring(i, qgramLength));
            }
            return result;
        }


    }
}
