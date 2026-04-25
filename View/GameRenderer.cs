using System;
using System.Drawing;
using LightEcho.Model;

namespace LightEcho.View
{
    public class GameRenderer
    {
        private GameState _gameState;
        private Brush _backgroundBrush = new SolidBrush(Color.FromArgb(20, 20, 25));
        private float _timeTime = 0f;

        public GameRenderer(GameState gameState)
        {
            _gameState = gameState;
        }

        public void Draw(Graphics g, int width, int height, float deltaTime)
        {
            _timeTime += deltaTime;

            // background
            g.FillRectangle(_backgroundBrush, 0, 0, width, height);

            // connections
            using (Pen connectionPen = new Pen(Color.FromArgb(40, 100, 255, 100), 2))
            {
                foreach (var node in _gameState.Nodes)
                {
                    foreach (var neighbor in node.Neighbors)
                    {
                        g.DrawLine(connectionPen, node.X, node.Y, neighbor.X, neighbor.Y);
                    }
                }
            }

            // nodes
            foreach (var node in _gameState.Nodes)
            {
                DrawNode(g, node);
            }

            // player
            DrawPlayer(g, _gameState.Player);

            // ui overlays
            DrawUI(g, width, height);
        }

        private void DrawNode(Graphics g, Node node)
        {
            int alpha = (int)(255 * (node.Energy / node.MaxEnergy));
            if (alpha < 0) alpha = 0;
            if (alpha > 255) alpha = 255;

            Color nodeColor = Color.FromArgb(alpha, 100, 255, 100);
            
            using (Brush brush = new SolidBrush(nodeColor))
            {
                g.FillEllipse(brush, node.X - 15, node.Y - 15, 30, 30);
            }

            using (Pen pen = new Pen(Color.FromArgb(50, 100, 255, 100), 2))
            {
                g.DrawEllipse(pen, node.X - 15, node.Y - 15, 30, 30);
            }

            // energy percentage
            int percent = (int)((node.Energy / node.MaxEnergy) * 100);
            string pctString = $"{percent}%";
            using (Font font = new Font("Consolas", 10, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(Color.White))
            {
                SizeF size = g.MeasureString(pctString, font);
                g.DrawString(pctString, font, textBrush, node.X - (size.Width / 2), node.Y - 25);
            }
        }

        private void DrawPlayer(Graphics g, Player player)
        {
            float pulse = (float)Math.Sin(_timeTime * 8f) * 4f;
            float radius = 20f + pulse;

            using (Brush brush = new SolidBrush(Color.White))
            {
                g.FillEllipse(brush, player.X - 10, player.Y - 10, 20, 20);
            }

            using (Pen glow = new Pen(Color.Cyan, 3))
            {
                g.DrawEllipse(glow, player.X - radius, player.Y - radius, radius * 2, radius * 2);
            }
        }

        private void DrawUI(Graphics g, int width, int height)
        {
            // tutorial or overlay text
            if (!string.IsNullOrEmpty(_gameState.OverlayMessage))
            {
                using (Font font = new Font("Consolas", 16, FontStyle.Bold))
                using (Brush textBrush = new SolidBrush(Color.Cyan))
                {
                    SizeF size = g.MeasureString(_gameState.OverlayMessage, font);
                    g.DrawString(_gameState.OverlayMessage, font, textBrush, (width - size.Width) / 2, 50);
                }

                // if in tutorial, show skip option
                if (_gameState.CurrentPhase != GamePhase.MainGameplay)
                {
                    string skipStr = "press [space] to skip tutorial";
                    using (Font fontSmall = new Font("Consolas", 10, FontStyle.Regular))
                    using (Brush skipBrush = new SolidBrush(Color.Gray))
                    {
                        SizeF skipSize = g.MeasureString(skipStr, fontSmall);
                        g.DrawString(skipStr, fontSmall, skipBrush, (width - skipSize.Width) / 2, 80);
                    }
                }
            }

            // debug info
            if (_gameState.CurrentPhase == GamePhase.MainGameplay)
            {
                string stateStr = $"Global Stability: {(int)_gameState.GlobalSystemStability}%";
                using (Font fontSmall = new Font("Consolas", 10, FontStyle.Regular))
                using (Brush stateBrush = new SolidBrush(_gameState.GlobalSystemStability > 50 ? Color.LightGreen : Color.LightCoral))
                {
                    SizeF stateSize = g.MeasureString(stateStr, fontSmall);
                    g.DrawString(stateStr, fontSmall, stateBrush, width - stateSize.Width - 20, height - 40);
                }
            }
        }
    }
}
