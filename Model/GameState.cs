using System;
using System.Collections.Generic;

namespace LightEcho.Model
{
    public enum GamePhase
    {
        TutorialStep1_Move,
        TutorialStep2_Charge,
        TutorialStep3_Spread,
        MainGameplay
    }

    public class GameState
    {
        public Player Player { get; private set; }
        public List<Node> Nodes { get; private set; }
        
        public GamePhase CurrentPhase { get; private set; }
        public string OverlayMessage { get; private set; }
        
        // todo: win/lose 
        public float GlobalSystemStability { get; set; } = 100f;

        public event EventHandler StateUpdated;

        public GameState()
        {
            Player = new Player(400, 300);
            Nodes = new List<Node>();
            SetupPhase(GamePhase.TutorialStep1_Move);
        }

        private void SetupPhase(GamePhase phase)
        {
            CurrentPhase = phase;
            Nodes.Clear();
            Player.X = 400;
            Player.Y = 300;
            Player.VelocityX = 0;
            Player.VelocityY = 0;

            if (phase == GamePhase.TutorialStep1_Move)
            {
                OverlayMessage = "Step 1: Use WASD to move your Spark.";
            }
            else if (phase == GamePhase.TutorialStep2_Charge)
            {
                OverlayMessage = "Step 2: Charge this node to 100%.";
                Nodes.Add(new Node(400, 150, 0f));
            }
            else if (phase == GamePhase.TutorialStep3_Spread)
            {
                OverlayMessage = "Step 3: Energy can spread between nodes (preview).";
                Node n1 = new Node(300, 250, 0f);
                Node n2 = new Node(500, 250, 0f);
                n1.AddNeighbor(n2);
                Nodes.Add(n1);
                Nodes.Add(n2);
            }
            else if (phase == GamePhase.MainGameplay)
            {
                OverlayMessage = "";
                Node n1 = new Node(200, 200, 40f);
                Node n2 = new Node(600, 200, 30f);
                Node n3 = new Node(400, 450, 10f);
                Node n4 = new Node(300, 350, 0f);
                
                n1.AddNeighbor(n2);
                n1.AddNeighbor(n4);
                n3.AddNeighbor(n4);

                Nodes.Add(n1);
                Nodes.Add(n2);
                Nodes.Add(n3);
                Nodes.Add(n4);
            }
        }

        public void SkipTutorial()
        {
            if (CurrentPhase != GamePhase.MainGameplay)
            {
                SetupPhase(GamePhase.MainGameplay);
            }
        }

        public void Update(float deltaTime)
        {
            Player.Update(deltaTime);

            foreach (var node in Nodes)
            {
                node.Update(deltaTime);

                float dx = Player.X - node.X;
                float dy = Player.Y - node.Y;
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                if (distance < 80f)
                {
                    node.GainEnergy(60f * deltaTime);
                }
            }

            if (CurrentPhase == GamePhase.TutorialStep1_Move)
            {
                if (Player.HasMoved && (Math.Abs(Player.VelocityX) > 10f || Math.Abs(Player.VelocityY) > 10f))
                    SetupPhase(GamePhase.TutorialStep2_Charge);
            }
            else if (CurrentPhase == GamePhase.TutorialStep2_Charge)
            {
                if (Nodes.Count > 0 && Nodes[0].Energy >= 99f)
                    SetupPhase(GamePhase.TutorialStep3_Spread);
            }
            else if (CurrentPhase == GamePhase.TutorialStep3_Spread)
            {
                if (Nodes.Count > 1 && Nodes[0].Energy >= 99f && Nodes[1].Energy >= 99f)
                    SetupPhase(GamePhase.MainGameplay);
            }
            else if (CurrentPhase == GamePhase.MainGameplay)
            {
                float totalCurrent = 0f;
                float totalMax = 0f;
                foreach(var n in Nodes) {
                    totalCurrent += n.Energy;
                    totalMax += n.MaxEnergy;
                }
                
                if (totalMax > 0)
                    GlobalSystemStability = (totalCurrent / totalMax) * 100f;
            }

            StateUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}
