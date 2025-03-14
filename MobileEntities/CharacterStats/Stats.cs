using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileEntities.CharacterStats
{
    public class Stats
    {
        public float Experience = 0;

        public float Health = 0;

        public float Attack = 0;

        public float Defense = 0;

        public float Speed = 0;

        #region Base Stats

        public float BaseHealth = 0;

        public float BaseAttack = 0;

        public float BaseDefense = 0;

        public float BaseSpeed = 0;

        #endregion

        public Stats(float _healthAmount)
        {
            Health = _healthAmount;
        }
    }
}
