using System;
using Microsoft.Xna.Framework;

namespace GooseLib.Graphics;

public class AnimatedSprite : Sprite
{
    private int _currentFrame;
    private TimeSpan _elapsedTime;
    private Animation _animation;
    private Vector2 _position;

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

    public float getX()
    {
        return _position.X;
    }

    public void setX(float x)
    {
        _position.X = x;
    }

    public float getY()
    {
        return _position.Y;
    }

    public void setY(float y)
    {
        _position.Y = y;
    }

    public Vector2 getPosition()
    {
        return _position;
    }

    public void setPosition(Vector2 position)
    {
        _position = position;
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