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

            // Observer pattern: Listen to model updates 
            _model.StateUpdated += OnModelStateUpdated;

            // Setup view event listening (input & rendering)
            _viewControl.KeyDown += HandleKeyDown;
            _viewControl.KeyUp += HandleKeyUp;
            _viewControl.Paint += HandlePaint;

            // Setup loop
            _stopwatch = new Stopwatch();
            _stopwatch.Start();

            // WinForms timer to drive the game loop (approx 60 ticks per second)
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

            // Limit deltaTime to prevent huge jumps if the window freezes or lags
            if (deltaTime > 0.1f) deltaTime = 0.1f;

            // Step the game model logic
            _model.Update(deltaTime);
        }

        private void OnModelStateUpdated(object sender, EventArgs e)
        {
            // Model dictates the state has changed -> View must redraw
            _viewControl.Invalidate();
        }

        private void HandlePaint(object sender, PaintEventArgs e)
        {
            // Enable anti-aliasing for smooth circles
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            _renderer.Draw(e.Graphics, _viewControl.Width, _viewControl.Height, 0.016f);
        }

        public void HandleKeyDown(object sender, KeyEventArgs e)
        {
            // input logic bound
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up) _model.Player.MovingUp = true;
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down) _model.Player.MovingDown = true;
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) _model.Player.MovingLeft = true;
            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) _model.Player.MovingRight = true;
            
            // tutorial controls
            if (e.KeyCode == Keys.Space) _model.SkipTutorial();
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
