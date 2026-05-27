using System;
using System.Collections.Generic;

namespace LightEcho.Model
{
    public enum GamePhase
    {
        Menu,
        Tutorial,
        Playing,
        GameOver,
        Victory
    }

    public class GameState
    {
        public Player Player { get; private set; }
        public List<Node> Nodes { get; private set; }
        public List<Connection> Connections { get; private set; }
        public List<Leech> Leeches { get; private set; }
        
        public GamePhase CurrentPhase { get; private set; }
        
        public int CurrentLevel { get; private set; } = 1;
        public int MenuCount { get; } = 3; // 0: Tutorial, 1: Play Game, 2: Exit
        
        private float _leechSpawnTimer = 5f;
        private float _leechSpawnCooldown = 12f;
        
        public string OverlayMessage { get; private set; }
        public string NarrativeHint { get; private set; }
        
        public float GlobalSystemStability { get; set; } = 100f;
        
        public float HoldTimer { get; private set; } = 3.0f;
        
        public int SelectedMenuIndex { get; set; } = 0; // 0: Tutorial, 1: Play Game, 2: Exit

        public int TutorialStep { get; private set; } = 1; // 1: WASD Movement, 2: Charge Node, 3: Repair connection

        private float _chargeBeepTimer = 0f;
        private float _repairBeepTimer = 0f;
        private float _leechAlertTimer = 0f;
        
        public event Action StateUpdated;

        public GameState()
        {
            Player = new Player(400, 300);
            Nodes = new List<Node>();
            Connections = new List<Connection>();
            Leeches = new List<Leech>();
            SetPhase(GamePhase.Menu);
        }

        public void StartNewGame()
        {
            CurrentLevel = 1;
            SetPhase(GamePhase.Playing);
        }

        public void AdvanceToNextLevel()
        {
            CurrentLevel++;
            SetPhase(GamePhase.Playing);
        }

        public void SetPhase(GamePhase phase)
        {
            CurrentPhase = phase;
            Nodes.Clear();
            Connections.Clear();
            Leeches.Clear();
            
            Player.X = 400;
            Player.Y = 300;
            Player.VelocityX = 0;
            Player.VelocityY = 0;
            
            HoldTimer = 3.0f;
            GlobalSystemStability = 50f;
            _leechSpawnTimer = 4f;

            _chargeBeepTimer = 0f;
            _repairBeepTimer = 0f;
            _leechAlertTimer = 0f;

            if (phase == GamePhase.Menu)
            {
                OverlayMessage = "ЭХО СВЕТА";
                NarrativeHint = "Используйте курсор мыши или СТРЕЛКИ + ENTER для выбора";
            }
            else if (phase == GamePhase.Tutorial)
            {
                TutorialStep = 1;
                OverlayMessage = "ОБУЧЕНИЕ";
                NarrativeHint = "Шаг 1: Используйте WASD для движения светлячка-'Искры'";
                
                Node tNode = new Node(400, 180, 20f);
                Nodes.Add(tNode);
                
                Node tNode2 = new Node(400, 420, 10f);
                Nodes.Add(tNode2);
                Connections.Add(new Connection(tNode, tNode2, true)); // damaged

                foreach (var node in Nodes)
                {
                    node.EnergyDecayRate = 2f;
                }
            }
            else if (phase == GamePhase.Playing)
            {
                OverlayMessage = "";
                NarrativeHint = $"Уровень {CurrentLevel}: Спасите искаженную сеть Эха!";
                
                GenerateProceduralLevel(CurrentLevel);
                ScaleDifficultyForLevel(CurrentLevel);
                
                Random rand = new Random();
                int initialLeeches = 1 + (CurrentLevel / 4);
                if (initialLeeches > 5) initialLeeches = 5;

                for (int i = 0; i < initialLeeches; i++)
                {
                    float sx = rand.Next(0, 2) == 0 ? rand.Next(50, 120) : rand.Next(680, 750);
                    float sy = rand.Next(0, 2) == 0 ? rand.Next(50, 120) : rand.Next(480, 550);
                    Leeches.Add(new Leech(sx, sy));
                }

                float currentSpeed = Math.Min(140f, 55f + (CurrentLevel * 7f));
                foreach (var leech in Leeches)
                {
                    leech.Speed = currentSpeed;
                }
            }
            else if (phase == GamePhase.GameOver)
            {
                OverlayMessage = "ВЫГАСАНИЕ РАЗУМА";
                NarrativeHint = "Нажмите ENTER, чтобы вернуться в главное меню";
                AudioSynth.PlayGameOver();
            }
            else if (phase == GamePhase.Victory)
            {
                OverlayMessage = $"УРОВЕНЬ {CurrentLevel} ПРОЙДЕН!";
                NarrativeHint = "Нажмите ENTER, чтобы перейти на следующий уровень";
                AudioSynth.PlayVictory();
            }
        }

        private void GenerateProceduralLevel(int level)
        {
            Random rand = new Random();
            int nodeCount = 5 + (level / 2);
            if (nodeCount > 14) nodeCount = 14;

            int layoutStyle = rand.Next(3);

            if (layoutStyle == 0)
            {
                Node center = new Node(400, 290, 40f);
                Nodes.Add(center);

                int outerRingNodes = nodeCount - 1;
                float radius = 175f;
                for (int i = 0; i < outerRingNodes; i++)
                {
                    double angle = (2 * Math.PI * i) / outerRingNodes;
                    float px = 400f + (float)(Math.Cos(angle) * radius);
                    float py = 290f + (float)(Math.Sin(angle) * radius);
                    Nodes.Add(new Node(px, py, rand.Next(15, 45)));
                }

                for (int i = 1; i < Nodes.Count; i++)
                {
                    bool startsBrokenCenter = rand.Next(100) < (25 + level * 2);
                    Connections.Add(new Connection(center, Nodes[i], startsBrokenCenter));

                    int nextIdx = i + 1;
                    if (nextIdx >= Nodes.Count) nextIdx = 1;
                    
                    bool startsBrokenRing = rand.Next(100) < (15 + level * 2);
                    Connections.Add(new Connection(Nodes[i], Nodes[nextIdx], startsBrokenRing));
                }
            }
            else if (layoutStyle == 1)
            {
                int attempts = 0;
                while (Nodes.Count < nodeCount && attempts < 250)
                {
                    attempts++;
                    float rx = rand.Next(120, 680);
                    float ry = rand.Next(100, 480);

                    bool distanceCheckSucceeds = true;
                    foreach (var node in Nodes)
                    {
                        float dx = node.X - rx;
                        float dy = node.Y - ry;
                        if (Math.Sqrt(dx * dx + dy * dy) < 135)
                        {
                            distanceCheckSucceeds = false;
                            break;
                        }
                    }

                    if (distanceCheckSucceeds)
                    {
                        Nodes.Add(new Node(rx, ry, rand.Next(20, 50)));
                    }
                }

                while (Nodes.Count < nodeCount)
                {
                    Nodes.Add(new Node(rand.Next(150, 650), rand.Next(120, 450), rand.Next(25, 45)));
                }

                List<Node> connectedList = new List<Node> { Nodes[0] };
                List<Node> unconnectedList = new List<Node>(Nodes);
                unconnectedList.RemoveAt(0);

                while (unconnectedList.Count > 0)
                {
                    double shortestSqDistance = double.MaxValue;
                    Node bestSrcNode = null;
                    Node bestDstNode = null;

                    foreach (var src in connectedList)
                    {
                        foreach (var dst in unconnectedList)
                        {
                            float dx = src.X - dst.X;
                            float dy = src.Y - dst.Y;
                            double dSq = dx * dx + dy * dy;

                            if (dSq < shortestSqDistance)
                            {
                                shortestSqDistance = dSq;
                                bestSrcNode = src;
                                bestDstNode = dst;
                            }
                        }
                    }

                    if (bestSrcNode != null && bestDstNode != null)
                    {
                        bool startsBroken = rand.Next(100) < (20 + (level * 3));
                        Connections.Add(new Connection(bestSrcNode, bestDstNode, startsBroken));

                        connectedList.Add(bestDstNode);
                        unconnectedList.Remove(bestDstNode);
                    }
                }

                int cycleLines = Math.Max(1, nodeCount / 4);
                for (int c = 0; c < cycleLines; c++)
                {
                    int idA = rand.Next(Nodes.Count);
                    int idB = rand.Next(Nodes.Count);
                    if (idA != idB)
                    {
                        bool alreadyJoined = false;
                        foreach (var conn in Connections)
                        {
                            if ((conn.NodeA == Nodes[idA] && conn.NodeB == Nodes[idB]) ||
                                (conn.NodeA == Nodes[idB] && conn.NodeB == Nodes[idA]))
                            {
                                alreadyJoined = true;
                                break;
                            }
                        }

                        if (!alreadyJoined)
                        {
                            bool startsBroken = rand.Next(100) < (25 + level * 2);
                            Connections.Add(new Connection(Nodes[idA], Nodes[idB], startsBroken));
                        }
                    }
                }
            }
            else
            {
                int half = nodeCount / 2;
                if (half < 3) half = 3;

                for (int i = 0; i < half; i++)
                {
                    Nodes.Add(new Node(rand.Next(130, 310), rand.Next(120, 460), rand.Next(25, 45)));
                }
                for (int i = 0; i < half; i++)
                {
                    Nodes.Add(new Node(rand.Next(490, 670), rand.Next(120, 460), rand.Next(25, 45)));
                }

                for (int i = 0; i < half - 1; i++)
                {
                    Connections.Add(new Connection(Nodes[i], Nodes[i + 1], rand.Next(100) < 22));
                }
                for (int i = half; i < Nodes.Count - 1; i++)
                {
                    Connections.Add(new Connection(Nodes[i], Nodes[i + 1], rand.Next(100) < 22));
                }

                int leftHook = rand.Next(half);
                int rightHook = half + rand.Next(half);
                Connections.Add(new Connection(Nodes[leftHook], Nodes[rightHook], true));
                
                int leftHook2 = rand.Next(half);
                int rightHook2 = half + rand.Next(half);
                Connections.Add(new Connection(Nodes[leftHook2], Nodes[rightHook2], rand.Next(100) < 40));
            }
        }

        private void ScaleDifficultyForLevel(int level)
        {
            float decayRate = Math.Min(11.5f, 3.2f + (level * 0.7f));
            _leechSpawnCooldown = Math.Max(5.0f, 14.5f - (level * 0.85f));

            foreach (var node in Nodes)
            {
                node.EnergyDecayRate = decayRate;
            }
        }

        public void SkipTutorial()
        {
            if (CurrentPhase == GamePhase.Tutorial)
            {
                SetPhase(GamePhase.Menu);
            }
        }

        public void TriggerBFS(Node startNode)
        {
            Queue<Node> queue = new Queue<Node>();
            HashSet<Node> visited = new HashSet<Node>();

            queue.Enqueue(startNode);
            visited.Add(startNode);

            float energyToSpread = 25f;

            while (queue.Count > 0)
            {
                Node current = queue.Dequeue();

                foreach (var conn in Connections)
                {
                    if (conn.IsBroken) continue;

                    Node neighbor = null;
                    if (conn.NodeA == current) neighbor = conn.NodeB;
                    else if (conn.NodeB == current) neighbor = conn.NodeA;

                    if (neighbor != null && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);

                        neighbor.GainEnergy(energyToSpread);
                    }
                }
            }
        }

        public void Update(float deltaTime)
        {
            if (CurrentPhase == GamePhase.Menu || CurrentPhase == GamePhase.GameOver || CurrentPhase == GamePhase.Victory)
            {
                StateUpdated?.Invoke();
                return;
            }

            if (_chargeBeepTimer > 0) _chargeBeepTimer -= deltaTime;
            if (_repairBeepTimer > 0) _repairBeepTimer -= deltaTime;
            if (_leechAlertTimer > 0) _leechAlertTimer -= deltaTime;

            Player.Update(deltaTime);

            if (Player.X < 20) Player.X = 20;
            if (Player.X > 780) Player.X = 780;
            if (Player.Y < 20) Player.Y = 20;
            if (Player.Y > 580) Player.Y = 580;

            foreach (var node in Nodes)
            {
                node.IsBeingAttacked = false;
            }

            if (CurrentPhase == GamePhase.Playing)
            {
                _leechSpawnTimer -= deltaTime;
                if (_leechSpawnTimer <= 0)
                {
                    Random rand = new Random();
                    float spawnX = rand.Next(0, 2) == 0 ? rand.Next(0, 100) : rand.Next(700, 800);
                    float spawnY = rand.Next(0, 2) == 0 ? rand.Next(0, 100) : rand.Next(500, 600);
                    
                    Leech newLeech = new Leech(spawnX, spawnY);
                    
                    newLeech.Speed = Math.Min(140f, 55f + (CurrentLevel * 7f));

                    Leeches.Add(newLeech);
                    _leechSpawnTimer = _leechSpawnCooldown;
                }
            }

            for (int i = Leeches.Count - 1; i >= 0; i--)
            {
                var leech = Leeches[i];
                leech.UpdateMovement(deltaTime, Nodes);

                float dx = Player.X - leech.X;
                float dy = Player.Y - leech.Y;
                float distToPlayer = (float)Math.Sqrt(dx * dx + dy * dy);

                if (distToPlayer < 25f)
                {
                    Leeches.RemoveAt(i);
                    NarrativeHint = "Теневая Пиявка поглощена вашей вспышкой!";
                    AudioSynth.PlayKill();
                    continue;
                }

                foreach (var node in Nodes)
                {
                    float ndx = node.X - leech.X;
                    float ndy = node.Y - leech.Y;
                    float distToNode = (float)Math.Sqrt(ndx * ndx + ndy * ndy);

                    if (distToNode < 20f && node.Energy > 0)
                    {
                        float drainSpeed = 14f + (CurrentLevel * 0.9f);
                        if (drainSpeed > 26f) drainSpeed = 26f;
                        
                        node.Energy -= drainSpeed * deltaTime;
                        if (node.Energy < 0) node.Energy = 0;
                        node.IsBeingAttacked = true;
                        NarrativeHint = "Узел очищается Тенью! Срочно отгоните пиявку!";

                        if (_leechAlertTimer <= 0)
                        {
                            AudioSynth.PlayLeechDrain();
                            _leechAlertTimer = 1.35f;
                        }
                    }
                }
            }

            foreach (var conn in Connections)
            {
                if (conn.IsBroken)
                {
                    float dist = conn.DistanceTo(Player.X, Player.Y);
                    if (dist < 22f)
                    {
                        conn.RepairProgress += deltaTime;
                        NarrativeHint = $"Идёт ремонт связующей магистрали ({(int)((conn.RepairProgress / conn.RepairTimeRequired) * 100)}%)";
                        
                        if (_repairBeepTimer <= 0)
                        {
                            AudioSynth.PlayRepair();
                            _repairBeepTimer = 0.82f;
                        }

                        if (conn.RepairProgress >= conn.RepairTimeRequired)
                        {
                            conn.IsBroken = false;
                            conn.RepairProgress = 0f;
                            NarrativeHint = "Магистраль успешно восстановлена!";
                            AudioSynth.PlayBurst(); 
                        }
                    }
                    else
                    {
                        conn.RepairProgress = 0f;
                    }
                }
            }

            float totalEnergy = 0f;
            float maxTotalEnergy = Nodes.Count * 100f;

            foreach (var node in Nodes)
            {
                node.Update(deltaTime);

                float ndx = Player.X - node.X;
                float ndy = Player.Y - node.Y;
                float distToPlayer = (float)Math.Sqrt(ndx * ndx + ndy * ndy);

                if (distToPlayer < 65f)
                {
                    float oldEnergy = node.Energy;
                    node.GainEnergy(55f * deltaTime);

                    if (node.Energy < 100f && _chargeBeepTimer <= 0)
                    {
                        AudioSynth.PlayCharge();
                        _chargeBeepTimer = 1.45f;
                    }

                    if (oldEnergy < 100f && node.Energy >= 100f && !node.HasTriggeredOverload)
                    {
                        node.HasTriggeredOverload = true;
                        TriggerBFS(node);
                        NarrativeHint = "Узел Перегружен! Импульс побежал по активным связям!";
                        AudioSynth.PlayBurst();
                    }
                }

                totalEnergy += node.Energy;
            }

            if (maxTotalEnergy > 0)
            {
                GlobalSystemStability = (totalEnergy / maxTotalEnergy) * 100f;
            }

            if (CurrentPhase == GamePhase.Tutorial)
            {
                if (TutorialStep == 1)
                {
                    if (Player.HasMoved && (Math.Abs(Player.VelocityX) > 15f || Math.Abs(Player.VelocityY) > 15f))
                    {
                        TutorialStep = 2;
                        NarrativeHint = "Шаг 2: Встаньте близко к узлу сверху, чтобы зарядить его до 100%";
                    }
                }
                else if (TutorialStep == 2)
                {
                    if (Nodes[0].Energy >= 99.9f)
                    {
                        TutorialStep = 3;
                        NarrativeHint = "Шаг 3: Встаньте на центр КРАСНОЙ пунктирной линии (ремонт связи) и задержитесь на 2с";
                    }
                }
                else if (TutorialStep == 3)
                {
                    if (!Connections[0].IsBroken)
                    {
                        SetPhase(GamePhase.Menu);
                        NarrativeHint = "Обучение завершено успешно! Нажмите Играть, чтобы запустить двигатель сети.";
                    }
                }
            }
            else if (CurrentPhase == GamePhase.Playing)
            {
                if (GlobalSystemStability <= 0.1f)
                {
                    SetPhase(GamePhase.GameOver);
                }

                if (GlobalSystemStability >= 85f)
                {
                    HoldTimer -= deltaTime;
                    NarrativeHint = $"Предел частоты! Держите стабильность сети ещё {HoldTimer:F1} сек!";
                    if (HoldTimer <= 0)
                    {
                        SetPhase(GamePhase.Victory);
                    }
                }
                else
                {
                    HoldTimer = 3.0f; 
                }
            }

            StateUpdated?.Invoke();
        }
    }
}
