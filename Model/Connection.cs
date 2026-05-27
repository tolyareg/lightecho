using System;

namespace LightEcho.Model
{
    public class Connection
    {
        public Node NodeA { get; private set; }
        public Node NodeB { get; private set; }
        public bool IsBroken { get; set; }
        public float RepairProgress { get; set; } = 0f;
        public float RepairTimeRequired { get; set; } = 2f; // 2 seconds

        public Connection(Node a, Node b, bool isBroken = false)
        {
            NodeA = a;
            NodeB = b;
            IsBroken = isBroken;
        }

        public float DistanceTo(float px, float py)
        {
            float l2 = (NodeA.X - NodeB.X) * (NodeA.X - NodeB.X) + (NodeA.Y - NodeB.Y) * (NodeA.Y - NodeB.Y);
            if (l2 == 0) return (float)Math.Sqrt((px - NodeA.X) * (px - NodeA.X) + (py - NodeA.Y) * (py - NodeA.Y));
            
            float t = Math.Max(0, Math.Min(1, ((px - NodeA.X) * (NodeB.X - NodeA.X) + (py - NodeA.Y) * (NodeB.Y - NodeA.Y)) / l2));
            float projX = NodeA.X + t * (NodeB.X - NodeA.X);
            float projY = NodeA.Y + t * (NodeB.Y - NodeA.Y);
            
            return (float)Math.Sqrt((px - projX) * (px - projX) + (py - projY) * (py - projY));
        }
    }
}
