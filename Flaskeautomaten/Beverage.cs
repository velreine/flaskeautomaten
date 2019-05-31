using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaskeautomaten
{
    public abstract class Beverage : INameable
    {
        public string Name { get; set; }

        protected Beverage(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"{this.GetType()}: {this.Name}";
        }
    }
}
