using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileEntities.CharacterStats
{
    public class Stats
    {
        public int Health = 0;
               
        public int Attack = 0;
               
        public int Defense = 0;
               
        public int Speed = 0;

        #region Base Stats

        public int BaseHealth = 0;
               
        public int BaseAttack = 0;
               
        public int BaseDefense = 0;
               
        public int BaseSpeed = 0;

        #endregion

        public Stats(int _healthAmount)
        {
            Health = _healthAmount;
        }

        public void CalculateStatsOnLevelUp()
        {
            RandomNumberGenerator _rng = new RandomNumberGenerator();

            Health = _rng.RandiRange(BaseHealth, BaseHealth + (int)Mathf.Ceil(BaseHealth / 2));
            Attack = _rng.RandiRange(BaseAttack, BaseAttack + (int)Mathf.Ceil(BaseAttack / 2));
            Defense = _rng.RandiRange(BaseDefense, BaseDefense + (int)Mathf.Ceil(BaseDefense / 2));
            Speed = _rng.RandiRange(BaseSpeed, BaseSpeed + (int)Mathf.Ceil(BaseSpeed / 2));

            GD.Print($"Health: {Health}, Attack: {Attack}, Defense: {Defense}, Speed: {Speed}");
        }
    }
}
