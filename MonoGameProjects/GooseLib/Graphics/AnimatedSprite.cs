using System;
using Microsoft.Xna.Framework;

namespace GooseLib.Graphics;

public class AnimatedSprite : Sprite
{
    private int _currentFrame;
    private TimeSpan _elapsedTime;
    private Animation _animation;

    public Animation Animation
    {
        get => _animation;
        set
        {
            _animation = value;
            Region = _animation.Frames[0];
        }
    }

    public void setAnimation(Animation animation)
    {
        Animation = animation;
        _currentFrame = 0;
        _elapsedTime = TimeSpan.Zero;
        Region = _animation.Frames[0];
    }

    public Animation getAnimation(Animation animation)
    {
        return Animation;
    }



    public AnimatedSprite() { }

    public AnimatedSprite(Animation animation)
    {
        Animation = animation;
    }

    public void Update(GameTime gameTime)
    {
        _elapsedTime += gameTime.ElapsedGameTime;

        if (_elapsedTime >= Animation.Delay)
        {
            _elapsedTime -= _animation.Delay;
            _currentFrame++;

            if (_currentFrame >= _animation.Frames.Count)
            {
                _currentFrame = 0;
            }

            Region = _animation.Frames[_currentFrame];
        }
    }
}