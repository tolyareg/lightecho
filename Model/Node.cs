using System;
using System.Collections.Generic;

namespace LightEcho.Model
{
    public class Node : GameObject
    {
        public float Energy { get; set; }
        public float MaxEnergy { get; set; } = 100f;
        public float EnergyDecayRate { get; set; } = 5f;
        public bool IsBeingAttacked { get; set; }
        public bool HasTriggeredOverload { get; set; }
        
        public Node(float x, float y, float startEnergy = 50f) : base(x, y)
        {
            Energy = startEnergy;
            if (startEnergy >= 100f)
            {
                HasTriggeredOverload = true;
            }
        }

        public override void Update(float deltaTime)
        {
            Energy -= EnergyDecayRate * deltaTime;
            if (Energy < 0) Energy = 0;

            if (Energy < 95f)
            {
                HasTriggeredOverload = false;
            }
        }

        public void GainEnergy(float amount)
        {
            Energy += amount;
            if (Energy > MaxEnergy) Energy = MaxEnergy;
            
        }
    }
}
