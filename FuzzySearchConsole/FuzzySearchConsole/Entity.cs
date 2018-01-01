using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuzzySearchConsole
{
    public class Entity
    {
        public string name;
        public string description;
        public int score;
        /// <summary>
        /// Construct new entity
        /// </summary>
        /// <param name="name">Name of entity</param>
        /// <param name="score">Score</param>
        /// <param name="desc">Description of entity</param>
        public Entity(string name, int score, string desc)
        {
            this.name = name;
            this.score = score;
            this.description = desc;
        }
        public override string ToString()
        {
            return String.Join("*", new string[] 
                { name, score.ToString(), description });
        }
    }
}
