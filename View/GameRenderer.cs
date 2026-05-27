using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using LightEcho.Model;

namespace LightEcho.View
{
    public class GameRenderer
    {
        private GameState _gameState;
        private Brush _backgroundBrush = new SolidBrush(Color.FromArgb(10, 10, 14));
        private float _timeTime = 0f;

        public GameRenderer(GameState gameState)
        {
            _gameState = gameState;
        }

        public void Draw(Graphics g, int width, int height, float deltaTime)
        {
            _timeTime += deltaTime;

            g.FillRectangle(_backgroundBrush, 0, 0, width, height);

            DrawAmbientSpace(g, width, height);

            if (_gameState.CurrentPhase == GamePhase.Menu)
            {
                DrawMenu(g, width, height);
                return;
            }

            if (_gameState.CurrentPhase == GamePhase.GameOver)
            {
                DrawGameOver(g, width, height);
                return;
            }

            if (_gameState.CurrentPhase == GamePhase.Victory)
            {
                DrawVictory(g, width, height);
                return;
            }

            foreach (var conn in _gameState.Connections)
            {
                DrawConnection(g, conn);
            }

            foreach (var node in _gameState.Nodes)
            {
                DrawNode(g, node);
            }

            foreach (var leech in _gameState.Leeches)
            {
                DrawLeech(g, leech);
            }

            DrawPlayer(g, _gameState.Player);
            DrawUI(g, width, height);
        }

        private void DrawAmbientSpace(Graphics g, int width, int height)
        {
            using (Pen gridPen = new Pen(Color.FromArgb(8, 255, 255, 255), 1))
            {
                int gridSize = 50;
                for (int x = 0; x < width; x += gridSize)
                {
                    g.DrawLine(gridPen, x, 0, x, height);
                }
                for (int y = 0; y < height; y += gridSize)
                {
                    g.DrawLine(gridPen, 0, y, width, y);
                }
            }

            for (int i = 0; i < 20; i++)
            {
                float seedX = (float)(Math.Sin(i * 143.52) * 0.5 + 0.5) * width;
                float seedY = (float)(Math.Cos(i * 941.13) * 0.5 + 0.5) * height;
                float size = (float)(Math.Sin(_timeTime + i) * 1.5 + 2.5);
                
                int op = (int)(30 + 35 * Math.Sin(_timeTime + i));
                if (op < 10) op = 10;
                if (op > 255) op = 255;

                using (Brush pBrush = new SolidBrush(Color.FromArgb(op, 0, 190, 255)))
                {
                    g.FillEllipse(pBrush, seedX - size/2, seedY - size/2, size, size);
                }
            }
        }

        private void DrawConnection(Graphics g, Connection conn)
        {
            float dx = conn.NodeB.X - conn.NodeA.X;
            float dy = conn.NodeB.Y - conn.NodeA.Y;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            
            int segmentCount = 14;
            PointF[] points = new PointF[segmentCount + 1];

            if (conn.IsBroken)
            {
                float perpX = len > 0 ? -dy / len : 0f;
                float perpY = len > 0 ? dx / len : 0f;

                Random rnd = new Random(conn.GetHashCode());

                for (int j = 0; j <= segmentCount; j++)
                {
                    float frac = (float)j / segmentCount;
                    float px = conn.NodeA.X + dx * frac;
                    float py = conn.NodeA.Y + dy * frac;

                    float envelope = (float)Math.Sin(frac * Math.PI);
                    
                    float cracklePhase = _timeTime * 40f + frac * 22f;
                    float offset = envelope * ((float)Math.Sin(cracklePhase) * 3f + (float)(rnd.NextDouble() - 0.5f) * 4f);

                    points[j] = new PointF(px + perpX * offset, py + perpY * offset);
                }

                using (Pen brokenPen = new Pen(Color.FromArgb(170, 255, 60, 60), 1.8f))
                {
                    g.DrawCurve(brokenPen, points);
                }

                float pulse = (float)Math.Sin(_timeTime * 4f) * 0.5f + 0.5f;
                int ringAlpha = (int)(50 + 60 * pulse);
                float midX = (conn.NodeA.X + conn.NodeB.X) / 2;
                float midY = (conn.NodeA.Y + conn.NodeB.Y) / 2;

                using (Pen ringPen = new Pen(Color.FromArgb(ringAlpha, 255, 80, 80), 1.5f))
                {
                    g.DrawEllipse(ringPen, midX - 10, midY - 10, 20, 20);
                }

                if (conn.RepairProgress > 0)
                {
                    int barWidth = 60;
                    int barHeight = 6;
                    int fillWidth = (int)((conn.RepairProgress / conn.RepairTimeRequired) * barWidth);
                    
                    g.FillRectangle(new SolidBrush(Color.FromArgb(60, 0, 0, 0)), midX - barWidth / 2, midY - 24, barWidth, barHeight);
                    g.DrawRectangle(Pens.White, midX - barWidth / 2, midY - 24, barWidth, barHeight);
                    using (Brush neonCyanBrush = new SolidBrush(Color.Cyan))
                    {
                        g.FillRectangle(neonCyanBrush, midX - barWidth / 2, midY - 24, fillWidth, barHeight);
                    }
                }
            }
            else
            {
                float perpX = len > 0 ? -dy / len : 0f;
                float perpY = len > 0 ? dx / len : 0f;

                for (int j = 0; j <= segmentCount; j++)
                {
                    float frac = (float)j / segmentCount;
                    float px = conn.NodeA.X + dx * frac;
                    float py = conn.NodeA.Y + dy * frac;

                    float envelope = (float)Math.Sin(frac * Math.PI);
                    
                    float wavePhase = _timeTime * 5.0f - frac * 3.5f * (float)Math.PI;
                    float offset = envelope * (float)Math.Sin(wavePhase) * 4.5f;

                    points[j] = new PointF(px + perpX * offset, py + perpY * offset);
                }

                float pulseThickness = (float)Math.Sin(_timeTime * 6f) * 0.6f + 2.2f;

                using (Pen backingPen = new Pen(Color.FromArgb(65, 0, 255, 120), pulseThickness + 3f))
                {
                    g.DrawCurve(backingPen, points);
                }
                
                using (Pen corePen = new Pen(Color.FromArgb(220, 190, 255, 210), pulseThickness))
                {
                    g.DrawCurve(corePen, points);
                }

                float[] packetOffsets = { (_timeTime * 0.35f) % 1f, (_timeTime * 0.35f + 0.5f) % 1f };
                foreach (float travelT in packetOffsets)
                {
                    float rawIdx = travelT * segmentCount;
                    int idx1 = (int)Math.Floor(rawIdx);
                    int idx2 = Math.Min(segmentCount, idx1 + 1);
                    float lerpFrac = rawIdx - idx1;

                    float pX = points[idx1].X + (points[idx2].X - points[idx1].X) * lerpFrac;
                    float pY = points[idx1].Y + (points[idx2].Y - points[idx1].Y) * lerpFrac;

                    using (Brush particleGlow = new SolidBrush(Color.FromArgb(140, 0, 255, 150)))
                    {
                        g.FillEllipse(particleGlow, pX - 4f, pY - 4f, 8f, 8f);
                    }
                    
                    using (Brush particleCore = new SolidBrush(Color.White))
                    {
                        g.FillEllipse(particleCore, pX - 2f, pY - 2f, 4f, 4f);
                    }
                }
            }
        }

        private void DrawNode(Graphics g, Node node)
        {
            float fillRatio = node.Energy / node.MaxEnergy;
            int alpha = (int)(255 * fillRatio);
            if (alpha < 40) alpha = 40;
            if (alpha > 255) alpha = 255;

            Color coreColor, glowColor;
            
            if (node.IsBeingAttacked)
            {
                float pulse = (float)Math.Sin(_timeTime * 15f) * 0.3f + 0.7f;
                coreColor = Color.FromArgb((int)(200 * pulse), 160, 0, 230);
                glowColor = Color.FromArgb((int)(90 * pulse), 200, 30, 255);
            }
            else
            {
                coreColor = Color.FromArgb(alpha, 100, 255, 120);
                glowColor = Color.FromArgb(alpha / 3, 50, 220, 100);
            }
            
            float nodeRadius = 18f;
            float glowOffset = (float)(Math.Sin(_timeTime * 3f + node.X) * 2f + 5f);
            using (Pen glowPen = new Pen(glowColor, 2f + glowOffset/3f))
            {
                g.DrawEllipse(glowPen, node.X - (nodeRadius + glowOffset), node.Y - (nodeRadius + glowOffset), (nodeRadius + glowOffset) * 2, (nodeRadius + glowOffset) * 2);
            }

            using (Brush brush = new SolidBrush(coreColor))
            {
                g.FillEllipse(brush, node.X - 12, node.Y - 12, 24, 24);
            }

            using (Pen borderPen = new Pen(Color.FromArgb(160, 255, 255, 255), 1.5f))
            {
                g.DrawEllipse(borderPen, node.X - 12, node.Y - 12, 24, 24);
            }

            int percentValue = (int)(fillRatio * 100);
            string pctStr = $"{percentValue}%";
            
            using (Font font = new Font("Consolas", 9, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(node.IsBeingAttacked ? Color.Magenta : Color.FromArgb(235, 255, 235)))
            {
                SizeF size = g.MeasureString(pctStr, font);
                g.DrawString(pctStr, font, textBrush, node.X - (size.Width / 2), node.Y - 28);
            }
        }

        private void DrawLeech(Graphics g, Leech leech)
        {
            float heading = (float)Math.Atan2(leech.VelocityY, leech.VelocityX);
            if (leech.VelocityX == 0 && leech.VelocityY == 0)
            {
                heading = 0f;
            }

            float cosH = (float)Math.Cos(heading);
            float sinH = (float)Math.Sin(heading);

            float perpX = -sinH;
            float perpY = cosH;

            int segmentCount = 4;
            float[] segmentRadii = { 8f, 6.5f, 5f, 3.5f };
            float[] segmentOffsets = { 0f, 8f, 16f, 23f };
            
            using (Brush alphaShadowBrush = new SolidBrush(Color.FromArgb(40, 90, 0, 150)))
            {
                g.FillEllipse(alphaShadowBrush, leech.X - 18, leech.Y - 18, 36, 36);
            }

            using (Pen trailPen = new Pen(Color.FromArgb(80, 180, 0, 255), 2.5f))
            {
                PointF[] trailPoints = new PointF[4];
                for (int s = 0; s < 4; s++)
                {
                    float dist = segmentOffsets[s];
                    float wriggle = (float)Math.Sin(_timeTime * 14f - s * 1.2f) * 4.5f;
                    float segX = leech.X - cosH * dist + perpX * wriggle;
                    float segY = leech.Y - sinH * dist + perpY * wriggle;
                    trailPoints[s] = new PointF(segX, segY);
                }
                g.DrawCurve(trailPen, trailPoints);
            }

            for (int s = segmentCount - 1; s >= 0; s--)
            {
                float dist = segmentOffsets[s];
                float wriggle = (float)Math.Sin(_timeTime * 14f - s * 1.2f) * 4.5f;
                
                float segX = leech.X - cosH * dist + perpX * wriggle;
                float segY = leech.Y - sinH * dist + perpY * wriggle;
                float radius = segmentRadii[s];

                using (Brush segBrush = new SolidBrush(Color.FromArgb(255, 12, 6, 20)))
                using (Pen segOutline = new Pen(Color.FromArgb(255, 180, 0, 255), 1.5f + (1f - (float)s / segmentCount)))
                {
                    g.FillEllipse(segBrush, segX - radius, segY - radius, radius * 2, radius * 2);
                    g.DrawEllipse(segOutline, segX - radius, segY - radius, radius * 2, radius * 2);
                }
            }

            float eyeOffsetDist = 3.5f;
            float eyeForwardDist = 2f;
            
            float leftEyeX = leech.X + cosH * eyeForwardDist + perpX * eyeOffsetDist;
            float leftEyeY = leech.Y + sinH * eyeForwardDist + perpY * eyeOffsetDist;
            
            float rightEyeX = leech.X + cosH * eyeForwardDist - perpX * eyeOffsetDist;
            float rightEyeY = leech.Y + sinH * eyeForwardDist - perpY * eyeOffsetDist;

            using (Brush eyeBrush = new SolidBrush(Color.FromArgb((int)(190 + 65 * Math.Sin(_timeTime * 15f)), 255, 0, 50)))
            {
                g.FillEllipse(eyeBrush, leftEyeX - 1.5f, leftEyeY - 1.5f, 3, 3);
                g.FillEllipse(eyeBrush, rightEyeX - 1.5f, rightEyeY - 1.5f, 3, 3);
            }
        }

        private void DrawPlayer(Graphics g, Player player)
        {
            float pulse = (float)Math.Sin(_timeTime * 11f) * 3f;
            float speed = (float)Math.Sqrt(player.VelocityX * player.VelocityX + player.VelocityY * player.VelocityY);

            using (Brush backGlow = new SolidBrush(Color.FromArgb(45, 0, 180, 255)))
            {
                g.FillEllipse(backGlow, player.X - 25f, player.Y - 25f, 50f, 50f);
            }

            if (speed > 18f)
            {
                float dirX = -player.VelocityX / speed;
                float dirY = -player.VelocityY / speed;

                for (int tIdx = 0; tIdx < 3; tIdx++)
                {
                    float dist = 10f + tIdx * 9f + (float)Math.Sin(_timeTime * 25f + tIdx) * 3f;
                    float trailX = player.X + dirX * dist;
                    float trailY = player.Y + dirY * dist;
                    float trailRadius = Math.Max(1.5f, 5f - tIdx * 1.5f + (float)Math.Sin(_timeTime * 18f) * 1f);

                    int alpha = Math.Min(255, Math.Max(0, 180 - tIdx * 55));
                    using (Brush trailBrush = new SolidBrush(Color.FromArgb(alpha, 0, 220, 255)))
                    {
                        g.FillEllipse(trailBrush, trailX - trailRadius, trailY - trailRadius, trailRadius * 2, trailRadius * 2);
                    }
                }
            }

            float bracketAngle = _timeTime * 2.2f;
            using (Pen bracketPen = new Pen(Color.FromArgb(145, 0, 240, 255), 1.6f))
            {
                float rad = 17f + pulse;
                g.DrawArc(bracketPen, player.X - rad, player.Y - rad, rad * 2, rad * 2, (float)(bracketAngle * 180f / Math.PI), 80f);
                g.DrawArc(bracketPen, player.X - rad, player.Y - rad, rad * 2, rad * 2, (float)(bracketAngle * 180f / Math.PI + 180f), 80f);
            }

            int satCount = 3;
            for (int s = 0; s < satCount; s++)
            {
                double angle = (_timeTime * 3.5f) + (s * Math.PI * 2.0 / satCount);
                float satRad = 23f + (float)Math.Sin(_timeTime * 8f) * 1.5f;
                float satX = player.X + (float)Math.Cos(angle) * satRad;
                float satY = player.Y + (float)Math.Sin(angle) * satRad;

                using (Brush satBrush = new SolidBrush(Color.FromArgb(240, 0, 255, 230)))
                {
                    g.FillEllipse(satBrush, satX - 2.5f, satY - 2.5f, 5, 5);
                }
                
                using (Pen connectionBeam = new Pen(Color.FromArgb(40, 0, 255, 200), 1f))
                {
                    g.DrawLine(connectionBeam, player.X, player.Y, satX, satY);
                }
            }

            using (Brush coreBrush = new SolidBrush(Color.White))
            {
                g.FillEllipse(coreBrush, player.X - 7.5f, player.Y - 7.5f, 15f, 15f);
            }

            using (Pen coreOutline = new Pen(Color.FromArgb(255, 0, 200, 255), 2f))
            {
                g.DrawEllipse(coreOutline, player.X - 7.5f, player.Y - 7.5f, 15f, 15f);
            }
        }

        private void DrawMenu(Graphics g, int width, int height)
        {
            using (Font titleFont = new Font("Impact", 44, FontStyle.Italic))
            using (Font subRegFont = new Font("Consolas", 11, FontStyle.Bold))
            using (Brush titleBrush = new SolidBrush(Color.FromArgb(240, 255, 255, 255)))
            using (Brush shadowBrush = new SolidBrush(Color.FromArgb(80, 0, 220, 255)))
            {
                string mainTitle = "ЭХО  СВЕТА";
                string subtitle = "SYSTEMS RESTORATION ENGINE // 2D WINFORMS MVC";

                g.DrawString(mainTitle, titleFont, shadowBrush, (width - g.MeasureString(mainTitle, titleFont).Width) / 2 + 3, 103);
                g.DrawString(mainTitle, titleFont, titleBrush, (width - g.MeasureString(mainTitle, titleFont).Width) / 2, 100);

                g.DrawString(subtitle, subRegFont, Brushes.Cyan, (width - g.MeasureString(subtitle, subRegFont).Width) / 2, 175);
            }

            string[] items = { "ЗАПУСТИТЬ ОБУЧЕНИЕ (TUTORIAL)", "СВЯЗАТЬСЯ С ЭХОМ (PLAY GAME)", "ОТКЛЮЧИТЬ ПИТАНИЕ (EXIT GAME)" };
            int startY = 240;
            int stepY = 70;

            for (int i = 0; i < items.Length; i++)
            {
                bool isSelected = (_gameState.SelectedMenuIndex == i);
                Rectangle rect = new Rectangle((width - 320) / 2, startY + i * stepY, 320, 42);

                if (isSelected)
                {
                    float flashPulse = (float)Math.Sin(_timeTime * 8f) * 0.12f + 0.88f;
                    using (Brush selBg = new SolidBrush(Color.FromArgb(30, 0, 255, 255)))
                    using (Pen selPen = new Pen(Color.FromArgb((int)(255 * flashPulse), 0, 240, 255), 2.5f))
                    {
                        g.FillRectangle(selBg, rect);
                        g.DrawRectangle(selPen, rect);
                    }
                }
                else
                {
                    using (Pen regPen = new Pen(Color.FromArgb(70, 255, 255, 255), 1))
                    {
                        g.DrawRectangle(regPen, rect);
                    }
                }

                using (Font font = new Font("Consolas", 11, FontStyle.Bold))
                using (Brush brush = new SolidBrush(isSelected ? Color.Cyan : Color.LightGray))
                {
                    string label = items[i];
                    SizeF sz = g.MeasureString(label, font);
                    g.DrawString(label, font, brush, rect.X + (rect.Width - sz.Width) / 2, rect.Y + (rect.Height - sz.Height) / 2);
                }
            }

            if (!string.IsNullOrEmpty(_gameState.NarrativeHint))
            {
                using (Font italicFont = new Font("Consolas", 10, FontStyle.Italic))
                using (Brush hintBrush = new SolidBrush(Color.FromArgb(200, 240, 190, 80)))
                {
                    SizeF size = g.MeasureString(_gameState.NarrativeHint, italicFont);
                    g.DrawString(_gameState.NarrativeHint, italicFont, hintBrush, (width - size.Width) / 2, height - 60);
                }
            }
        }

        private void DrawGameOver(Graphics g, int width, int height)
        {
            using (Font maxTitleFont = new Font("Impact", 46, FontStyle.Regular))
            using (Font regFont = new Font("Consolas", 12, FontStyle.Regular))
            {
                string overStr = "СИСТЕМА  УГАСЛА";
                string descStr = $"Канал прерван на уровне {_gameState.CurrentLevel}. Вся чистая энергия поглощена Тенью.";
                string retryStr = "[ НАЖМИТЕ  ENTER  ДЛЯ  ВЫХОДА  В  МЕНЮ ]";

                using (Brush glowBrush = new SolidBrush(Color.DarkRed))
                using (Brush textBrush = new SolidBrush(Color.FromArgb(255, 100, 100)))
                {
                    g.DrawString(overStr, maxTitleFont, glowBrush, (width - g.MeasureString(overStr, maxTitleFont).Width) / 2 + 2, height / 2 - 100 + 2);
                    g.DrawString(overStr, maxTitleFont, textBrush, (width - g.MeasureString(overStr, maxTitleFont).Width) / 2, height / 2 - 100);
                }

                g.DrawString(descStr, regFont, Brushes.LightGray, (width - g.MeasureString(descStr, regFont).Width) / 2, height / 2);
                
                float pulse = (float)Math.Sin(_timeTime * 4f) * 0.5f + 0.5f;
                using (Brush pulseRed = new SolidBrush(Color.FromArgb((int)(150 + 105 * pulse), 200, 70, 70)))
                {
                    g.DrawString(retryStr, regFont, pulseRed, (width - g.MeasureString(retryStr, regFont).Width) / 2, height / 2 + 100);
                }
            }
        }

        private void DrawVictory(Graphics g, int width, int height)
        {
            using (Font maxTitleFont = new Font("Impact", 46, FontStyle.Regular))
            using (Font regFont = new Font("Consolas", 12, FontStyle.Regular))
            {
                string winStr = $"УРОВЕНЬ {_gameState.CurrentLevel} ПРОЙДЕН!";
                string descStr = "Световой контур выдержал натиск. Информационные мосты спасены!";
                string retryStr = "[ НАЖМИТЕ  ENTER  ДЛЯ  ПЕРЕХОДА  НА  СЛЕДУЮЩИЙ  УРОВЕНЬ ]";

                using (Brush shadowBrush = new SolidBrush(Color.DarkCyan))
                using (Brush textBrush = new SolidBrush(Color.FromArgb(100, 255, 240)))
                {
                    g.DrawString(winStr, maxTitleFont, shadowBrush, (width - g.MeasureString(winStr, maxTitleFont).Width) / 2 + 2, height / 2 - 100 + 2);
                    g.DrawString(winStr, maxTitleFont, textBrush, (width - g.MeasureString(winStr, maxTitleFont).Width) / 2, height / 2 - 100);
                }

                g.DrawString(descStr, regFont, Brushes.White, (width - g.MeasureString(descStr, regFont).Width) / 2, height / 2);
                
                float pulse = (float)Math.Sin(_timeTime * 4f) * 0.5f + 0.5f;
                using (Brush pulseCyan = new SolidBrush(Color.FromArgb((int)(150 + 105 * pulse), 100, 255, 240)))
                {
                    g.DrawString(retryStr, regFont, pulseCyan, (width - g.MeasureString(retryStr, regFont).Width) / 2, height / 2 + 100);
                }
            }
        }

        private void DrawUI(Graphics g, int width, int height)
        {
            if (_gameState.CurrentPhase == GamePhase.Tutorial)
            {
                using (Font subFont = new Font("Consolas", 14, FontStyle.Bold))
                using (Font smallFont = new Font("Consolas", 10, FontStyle.Regular))
                {
                    string stepHeader = $"ЭТАП  ОБУЧЕНИЯ: { _gameState.TutorialStep } / 3";
                    
                    g.DrawString(stepHeader, subFont, Brushes.Cyan, (width - g.MeasureString(stepHeader, subFont).Width) / 2, 45);
                    
                    string skipTip = "[Space] - Пропустить обучение и выйти в Меню";
                    g.DrawString(skipTip, smallFont, Brushes.Gray, (width - g.MeasureString(skipTip, smallFont).Width) / 2, 75);
                }
            }

            if (_gameState.CurrentPhase == GamePhase.Playing)
            {
                int barWidth = 450;
                int barHeight = 16;
                int posX = (width - barWidth) / 2;
                int posY = height - 55;

                g.FillRectangle(new SolidBrush(Color.FromArgb(40, 20, 20, 25)), posX, posY, barWidth, barHeight);
                g.DrawRectangle(new Pen(Color.FromArgb(90, 255, 255, 255), 1.5f), posX, posY, barWidth, barHeight);

                float safetyVal = _gameState.GlobalSystemStability / 100f;
                if (safetyVal < 0) safetyVal = 0;
                if (safetyVal > 1) safetyVal = 1;

                Color fillColor = Color.FromArgb(120, 50, 250, 100); // stable green
                if (safetyVal < 0.35f) fillColor = Color.FromArgb(180, 255, 50, 50); // dangerous red
                else if (safetyVal < 0.65f) fillColor = Color.FromArgb(150, 240, 200, 50); // warnings gold

                int filledWidth = (int)(safetyVal * barWidth);
                if (filledWidth > 0)
                {
                    using (Brush coreFill = new SolidBrush(fillColor))
                    {
                        g.FillRectangle(coreFill, posX + 2, posY + 2, filledWidth - 4, barHeight - 4);
                    }
                }

                // stability status label
                string stabilityText = $"СТАБИЛЬНОСТЬ СЕТИ: {(int)_gameState.GlobalSystemStability}%";
                using (Font barFont = new Font("Consolas", 9, FontStyle.Bold))
                {
                    SizeF textSz = g.MeasureString(stabilityText, barFont);
                    g.DrawString(stabilityText, barFont, Brushes.White, posX + (barWidth - textSz.Width)/2, posY + (barHeight - textSz.Height)/2 + 1);
                }

                if (_gameState.GlobalSystemStability >= 85f)
                {
                    string holdMessage = $"ФИКСАЦИЯ СИГНАЛА: {_gameState.HoldTimer:F1}s...";
                    using (Font holdFont = new Font("Consolas", 15, FontStyle.Bold))
                    {
                        SizeF textSz2 = g.MeasureString(holdMessage, holdFont);
                        float blink = (float)Math.Sin(_timeTime * 14f) * 0.4f + 0.6f;
                        using (Brush holdBrush = new SolidBrush(Color.FromArgb((int)(200 * blink), 0, 255, 255)))
                        {
                            g.DrawString(holdMessage, holdFont, holdBrush, (width - textSz2.Width)/2, posY - 40);
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(_gameState.NarrativeHint) && _gameState.CurrentPhase != GamePhase.Menu)
            {
                using (Font italicFont = new Font("Consolas", 10, FontStyle.Italic))
                using (Brush textBrush = new SolidBrush(Color.FromArgb(230, 255, 210, 50)))
                {
                    SizeF sz = g.MeasureString(_gameState.NarrativeHint, italicFont);
                    g.DrawString(_gameState.NarrativeHint, italicFont, textBrush, (width - sz.Width) / 2, height - (80 + (_gameState.CurrentPhase == GamePhase.Playing ? 10 : 0)));
                }
            }
        }
    }
}
