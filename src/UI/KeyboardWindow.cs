using SFML.Window;
using SFML.Graphics;
using SFML.System;
using System.Threading;
using System;
using System.Collections.Generic;

namespace DSController.UI
{
    public class KeyboardWindow
    {
        protected RenderWindow WindowHandle;
        protected Font TextFont;
        protected Dictionary<string, bool> pressedKeys;

        public uint BackgroundColor { get; set; }

        public string CurrentStatus { get; set; }

        public delegate void KeyActionHandler(object sender, KeyActionEventArgs e);
        public event KeyActionHandler OnKeyAction;

        public KeyboardWindow()
        {
        }

        public void Init()
        {
            VideoMode videoMode = new SFML.Window.VideoMode(400,50);
            this.WindowHandle = new SFML.Graphics.RenderWindow(videoMode, "DS Controller");
            this.pressedKeys = new Dictionary<string, bool>();

            this.WindowHandle.SetKeyRepeatEnabled(false);

            // this.WindowHandle.KeyPressed += this.OnKeyPressed;
            // this.WindowHandle.KeyReleased += this.OnKeyReleased;
            this.WindowHandle.Closed += this.OnWindowClosed;

            this.TextFont = new Font("roboto.ttf");
        }

        public void Loop()
        {
            while(this.WindowHandle.IsOpen) {
                this.LoopOnce();

                foreach(string key in Enum.GetNames(typeof(Keyboard.Key))) {
                    if(Keyboard.IsKeyPressed((Keyboard.Key) Enum.Parse(typeof(Keyboard.Key), key))) {
                        if(!this.pressedKeys.ContainsKey(key) || this.pressedKeys[key] == false) {
                            this.GenerateKeyEvent(key, true);
                            this.pressedKeys[key] = true;
                        }
                    } else if(this.pressedKeys.ContainsKey(key) && this.pressedKeys[key] == true) {
                        this.GenerateKeyEvent(key, false);
                        this.pressedKeys[key] = false;
                    }
                }

                Thread.Sleep(10);
            }
        }

        public void LoopOnce()
        {
            // Process events, draw the message and button status and display the window
            this.WindowHandle.DispatchEvents();
            this.WindowHandle.Clear();

            RectangleShape background = new RectangleShape();
            background.Size = new Vector2f(this.WindowHandle.Size.X, this.WindowHandle.Size.Y);
            background.Position = new Vector2f(0, 0);
            background.FillColor = new Color(this.BackgroundColor);

            Text statusText = new Text(this.CurrentStatus, this.TextFont);
            statusText.CharacterSize = 16;
            statusText.Style = Text.Styles.Regular;
            statusText.FillColor = Color.White;

            this.WindowHandle.Draw(background);
            this.WindowHandle.Draw(statusText);
            this.WindowHandle.Display();
        }

        public void ShowStatus(string status, uint color = 0)
        {
            if(color == 0) {
                color = Color.Black.ToInteger();
            }

            this.BackgroundColor = color;
            this.CurrentStatus = status;
            this.LoopOnce();
        }

        public void Stop()
        {
            this.WindowHandle.Close();
        }
        
        public void OnKeyPressed(object sender, KeyEventArgs e)
        {
            this.GenerateKeyEvent(e.Code.ToString(), true);
        }

        public void OnKeyReleased(object sender, KeyEventArgs e)
        {
            this.GenerateKeyEvent(e.Code.ToString(), false);
        }

        public void OnWindowClosed(object sender, EventArgs e)
        {
            this.WindowHandle.Close();
        }

        protected void GenerateKeyEvent(string key, bool pressed)
        {
            KeyActionEventArgs kaEvent = new KeyActionEventArgs(key, pressed);
            this.OnKeyAction(this, kaEvent);
        }
    }
}