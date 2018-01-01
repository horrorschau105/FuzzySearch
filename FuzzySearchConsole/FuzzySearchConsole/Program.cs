using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace FuzzySearchConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string filename = "../../../wikidata.tsv"; 
            //string filename = "example.txt";
            string query = "!";
            QGramIndex qgi = new QGramIndex(3);
            var start = DateTime.Now;
            Console.WriteLine("Building index from {0}...", filename);
            qgi.BuildFromFile(filename);
            var end = DateTime.Now;
            Console.WriteLine("Built in {0}", end - start);
            while(!String.IsNullOrEmpty(query))
            {
                Console.Write("Your query: ");
                query = Console.ReadLine();
                start = DateTime.Now;
                var results = QGramIndex.RankMatches(qgi.FindMatches(query, query.Length / 4).Key);
                end = DateTime.Now;
                Console.WriteLine("Found {1} results in {0} seconds", end - start, results.Count);
                if(results.Count > 0) Console.WriteLine("Best results:");
                for(int i=0;i<Math.Min(results.Count, 5);++i)
                {
                    Console.WriteLine("Name: {0}\tScore: {1}\tDescription: {2}\tpED: {3}",
                        results[i].Key.name, results[i].Key.score, results[i].Key.description, 
                        results[i].Value);
                }
            }
        }
    }
}
