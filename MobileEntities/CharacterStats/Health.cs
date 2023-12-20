using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileEntities.CharacterStats
{
    public class Health
    {
        public float HealthAmount;

        public Health() { }

        public Health(float _healthAmount)
        {
            HealthAmount = _healthAmount;
        }

        //public void CheckIfDead()
        //{
        //    if (HealthAmount <= 0)
        //    {
        //        GD.Print("DEAD");
        //    }
        //}
    }
}
