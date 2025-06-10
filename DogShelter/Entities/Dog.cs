using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DogShelter.Entities
{
    public class Dog
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Age { get; set; }
        public string Breed { get; set; } = null!;
        public bool IsAdopted { get; set; } = false;
    }
}

