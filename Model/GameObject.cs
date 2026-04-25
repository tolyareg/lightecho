using System;

namespace LightEcho.Model
{
    public abstract class GameObject
    {
        public float X { get; set; }
        public float Y { get; set; }

        public GameObject(float x, float y)
        {
            X = x;
            Y = y;
        }

        public abstract void Update(float deltaTime);
    }
}
