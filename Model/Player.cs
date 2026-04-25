using System;

namespace LightEcho.Model
{
    public class Player : GameObject
    {
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public float Acceleration { get; set; } = 800f;
        public float MaxSpeed { get; set; } = 300f;
        public float Friction { get; set; } = 0.85f;
        public bool MovingUp { get; set; }
        public bool MovingDown { get; set; }
        public bool MovingLeft { get; set; }
        public bool MovingRight { get; set; }
        
        public bool HasMoved { get; private set; } // for tutorial

        public Player(float x, float y) : base(x, y) { }

        public override void Update(float deltaTime)
        {
            // movement physics
            if (MovingUp) { VelocityY -= Acceleration * deltaTime; HasMoved = true; }
            if (MovingDown) { VelocityY += Acceleration * deltaTime; HasMoved = true; }
            if (MovingLeft) { VelocityX -= Acceleration * deltaTime; HasMoved = true; }
            if (MovingRight) { VelocityX += Acceleration * deltaTime; HasMoved = true; }

            // friction
            VelocityX *= Friction;
            VelocityY *= Friction;

            float speed = (float)Math.Sqrt(VelocityX * VelocityX + VelocityY * VelocityY);
            if (speed > MaxSpeed)
            {
                VelocityX = (VelocityX / speed) * MaxSpeed;
                VelocityY = (VelocityY / speed) * MaxSpeed;
            }

            // update position
            X += VelocityX * deltaTime;
            Y += VelocityY * deltaTime;
        }
    }
}
