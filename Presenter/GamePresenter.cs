using System;
using System.Diagnostics;
using System.Windows.Forms;
using LightEcho.Model;
using LightEcho.View;

namespace LightEcho.Presenter
{
    public class GamePresenter
    {
        private GameState _model;
        private Control _viewControl;
        private GameRenderer _renderer;
        private Stopwatch _stopwatch;
        private Timer _gameLoopTimer;
        private float _lastTime;

        public GamePresenter(GameState model, Control viewControl)
        {
            _model = model;
            _viewControl = viewControl;
            _renderer = new GameRenderer(_model);

            _model.StateUpdated += OnModelStateUpdated;

            _viewControl.KeyDown += HandleKeyDown;
            _viewControl.KeyUp += HandleKeyUp;
            _viewControl.Paint += HandlePaint;
            _viewControl.MouseDown += HandleMouseDown;

            _stopwatch = new Stopwatch();
            _stopwatch.Start();

            _gameLoopTimer = new Timer();
            _gameLoopTimer.Interval = 16; 
            _gameLoopTimer.Tick += GameLoop;
            _gameLoopTimer.Start();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            float currentTime = (float)_stopwatch.Elapsed.TotalSeconds;
            float deltaTime = currentTime - _lastTime;
            _lastTime = currentTime;

            if (deltaTime > 0.08f) deltaTime = 0.08f;

            _model.Update(deltaTime);
        }

        private void OnModelStateUpdated()
        {
            _viewControl.Invalidate();
        }

        private void HandlePaint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            
            _renderer.Draw(e.Graphics, _viewControl.Width, _viewControl.Height, 0.016f);
        }

        public void HandleMouseDown(object sender, MouseEventArgs e)
        {
            if (_model.CurrentPhase != GamePhase.Menu) return;

            int mouseX = e.X;
            int mouseY = e.Y;

            int btnWidth = 320;
            int posX = (_viewControl.Width - btnWidth) / 2;
            int startY = 240;
            int stepY = 70;

            if (mouseX >= posX && mouseX <= posX + btnWidth)
            {
                for (int i = 0; i < _model.MenuCount; i++)
                {
                    int topY = startY + i * stepY;
                    int bottomY = topY + 42;

                    if (mouseY >= topY && mouseY <= bottomY)
                    {
                        AudioSynth.PlaySelect();
                        _model.SelectedMenuIndex = i;
                        ExecuteMenuAction(i);
                        break;
                    }
                }
            }
        }

        private void ExecuteMenuAction(int index)
        {
            switch (index)
            {
                case 0: // Tutorial
                    _model.SetPhase(GamePhase.Tutorial);
                    break;
                case 1: // Start Game with automated leveling progression
                    _model.StartNewGame();
                    break;
                case 2: // Exit
                    Application.Exit();
                    break;
            }
        }

        public void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (_model.CurrentPhase == GamePhase.Menu)
            {
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W)
                {
                    AudioSynth.PlaySelect();
                    _model.SelectedMenuIndex--;
                    if (_model.SelectedMenuIndex < 0) _model.SelectedMenuIndex = 2;
                }
                else if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
                {
                    AudioSynth.PlaySelect();
                    _model.SelectedMenuIndex++;
                    if (_model.SelectedMenuIndex > 2) _model.SelectedMenuIndex = 0;
                }
                else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
                {
                    AudioSynth.PlaySelect();
                    ExecuteMenuAction(_model.SelectedMenuIndex);
                }
                return;
            }

            if (_model.CurrentPhase == GamePhase.GameOver || _model.CurrentPhase == GamePhase.Victory)
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape || e.KeyCode == Keys.Space)
                {
                    AudioSynth.PlaySelect();
                    if (_model.CurrentPhase == GamePhase.Victory && (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space))
                    {
                        _model.AdvanceToNextLevel();
                    }
                    else
                    {
                        _model.SetPhase(GamePhase.Menu);
                    }
                }
                return;
            }

            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up) _model.Player.MovingUp = true;
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down) _model.Player.MovingDown = true;
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) _model.Player.MovingLeft = true;
            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) _model.Player.MovingRight = true;
            
            if (e.KeyCode == Keys.Space && _model.CurrentPhase == GamePhase.Tutorial)
            {
                _model.SkipTutorial();
            }
        }

        public void HandleKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up) _model.Player.MovingUp = false;
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down) _model.Player.MovingDown = false;
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) _model.Player.MovingLeft = false;
            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) _model.Player.MovingRight = false;
        }
    }
}
