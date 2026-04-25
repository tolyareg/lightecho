using System;
using System.Reflection;
using System.Windows.Forms;
using LightEcho.Model;
using LightEcho.Presenter;

namespace LightEcho
{
    public class MainForm : Form
    {
        private GameState _gameState;
        private GamePresenter _presenter;

        public MainForm()
        {
            this.Text = "Эхо света";
            this.Width = 800;
            this.Height = 600;
            
            this.DoubleBuffered = true; 
            
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            _gameState = new GameState(); // model
            _presenter = new GamePresenter(_gameState, this);
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
