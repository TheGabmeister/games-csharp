using Microsoft.Xna.Framework.Input;

namespace monogame_simple.Core;

public sealed class InputState
{
    private KeyboardState _currentKeyboard;
    private KeyboardState _previousKeyboard;

    public void Update()
    {
        _previousKeyboard = _currentKeyboard;
        _currentKeyboard = Keyboard.GetState();
    }

    public bool IsDown(Keys key)
    {
        return _currentKeyboard.IsKeyDown(key);
    }

    public bool IsNewPress(Keys key)
    {
        return _currentKeyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
    }
}
