using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileEntities.CharacterStats
{
    public class Stats
    {
        public Experience Experience = new Experience();

        public Health Health = new Health();

        public float Attack = 0;

        public float Defense = 0;

        public float Speed = 0;

        public Stats(float _healthAmount)
        {
            Health.HealthAmount = _healthAmount;
        }
    }
}
