using System;
using System.Collections.Generic;

namespace LightEcho.Model
{
    public class Node : GameObject
    {
        public float Energy { get; set; }
        public float MaxEnergy { get; set; } = 100f;
        public float EnergyDecayRate { get; set; } = 5f;
        
        public List<Node> Neighbors { get; private set; } = new List<Node>();

        public Node(float x, float y, float startEnergy = 50f) : base(x, y)
        {
            Energy = startEnergy;
        }

        public void AddNeighbor(Node other)
        {
            if (!Neighbors.Contains(other))
            {
                Neighbors.Add(other);
                other.Neighbors.Add(this);
            }
        }

        public override void Update(float deltaTime)
        {
            Energy -= EnergyDecayRate * deltaTime;
            if (Energy < 0) Energy = 0;
        }

        public void GainEnergy(float amount)
        {
            Energy += amount;
            if (Energy > MaxEnergy) Energy = MaxEnergy;
            
            // todo: bfs trigger 
        }
    }
}
