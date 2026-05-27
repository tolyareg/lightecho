using System;

namespace LightEcho.Model
{
    public class Leech : GameObject
    {
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public float Speed { get; set; } = 70f;
        
        private float _wanderAngle;
        private Random _random;

        public Leech(float x, float y) : base(x, y)
        {
            _random = new Random((int)(x * 1000 + y));
            _wanderAngle = (float)(_random.NextDouble() * Math.PI * 2);
        }

        public override void Update(float deltaTime)
        {
            _wanderAngle += (float)((_random.NextDouble() - 0.5) * 6f * deltaTime);
            
            VelocityX = (float)Math.Cos(_wanderAngle) * Speed;
            VelocityY = (float)Math.Sin(_wanderAngle) * Speed;

            X += VelocityX * deltaTime;
            Y += VelocityY * deltaTime;
            
            if (X < 20) _wanderAngle = 0;
            if (X > 780) _wanderAngle = (float)Math.PI;
            if (Y < 20) _wanderAngle = (float)Math.PI / 2;
            if (Y > 580) _wanderAngle = -(float)Math.PI / 2;
        }

        public void UpdateMovement(float deltaTime, System.Collections.Generic.List<Node> nodes)
        {
            Node nearestNode = null;
            float minDistance = float.MaxValue;
            foreach (var node in nodes)
            {
                if (node.Energy <= 0f) continue;

                float dist = DistanceTo(node);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestNode = node;
                }
            }

            if (nearestNode != null && minDistance < 250f) 
            {
                if (minDistance < 22f)
                {
                    float orbitSpeed = 2.0f + (float)(Math.Abs(GetHashCode()) % 5) * 0.3f;
                    float orbitRadius = 14f;
                    
                    double currentAngle = Math.Atan2(Y - nearestNode.Y, X - nearestNode.X);
                    currentAngle += orbitSpeed * deltaTime;
                    
                    X = nearestNode.X + (float)Math.Cos(currentAngle) * orbitRadius;
                    Y = nearestNode.Y + (float)Math.Sin(currentAngle) * orbitRadius;
                    
                    VelocityX = -(float)Math.Sin(currentAngle) * Speed * 0.6f;
                    VelocityY = (float)Math.Cos(currentAngle) * Speed * 0.6f;
                    _wanderAngle = (float)Math.Atan2(VelocityY, VelocityX);
                }
                else
                {
                    MoveTowards(nearestNode.X, nearestNode.Y, deltaTime);
                }
            }
            else
            {
                Update(deltaTime);
            }
        }

        public float DistanceTo(GameObject other)
        {
            float dx = other.X - X;
            float dy = other.Y - Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
        public void MoveTowards(float targetX, float targetY, float deltaTime)
        {
            float dx = targetX - X;
            float dy = targetY - Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            
            if (dist > 0)
            {
                VelocityX = (dx / dist) * Speed * 1.5f; 
                VelocityY = (dy / dist) * Speed * 1.5f;
                
                X += VelocityX * deltaTime;
                Y += VelocityY * deltaTime;
                
                _wanderAngle = (float)Math.Atan2(VelocityY, VelocityX);
            }
        }
    }
}
