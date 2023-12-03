using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileEntities.CharacterStats
{
    public class CharacterStats
    {
        public Experience Experience = new Experience();

        public Health Health = new Health();

        public float Attack = 0;

        public float Defense = 0;

        public float Speed = 0;
    }
}
