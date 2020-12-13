namespace DSController.UI
{
    public class KeyActionEventArgs
    {
        public string Key { get; protected set; }
        public bool Pressed { get; protected set; }

        public KeyActionEventArgs(string key, bool pressed)
        {
            this.Key = key;
            this.Pressed = pressed;
        }
    }
}