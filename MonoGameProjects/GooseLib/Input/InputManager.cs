using Microsoft.Xna.Framework;

namespace GooseLib.Input;

public class InputManager
{
    public KeyboardInfo Keyboard { get; private set; }

    public InputManager()
    {
        Keyboard = new KeyboardInfo();
        // TODO Mouse & GamePad
    }

    public void Update(GameTime gameTime)
    {
        Keyboard.Update();
    }
}