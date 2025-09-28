using System;
using GooseLib.Graphics;
using GooseLib.Utils;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GooseLib.Input;

namespace GooseLib.Weapons;

public class Sword : Weapon
{
    private bool _isSwinging = false;
    private double _swingTimer = 0;
    private const double SWING_DURATION = 500; // in ms
    private Rectangle bounds;
    private Direction _direction;

    private AnimatedSprite User;


    public Sword(AnimatedSprite _user, Texture2D texture, Vector2 anchor, Vector2 scale, string name, int damage, float range)
        : base(texture, anchor, scale, name, damage, range)
    {
        User = _user;
        Texture = texture;
    }

    public override void Attack()
    {
        if (!_isSwinging)
        {
            _isSwinging = true;
            _swingTimer = SWING_DURATION;
        }
    }

    public override void Update(GameTime gameTime)
    {
        if (_isSwinging)
        {
            _swingTimer -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_swingTimer <= 0)
            {
                _isSwinging = false;
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch, Direction direction)
    {
        _direction = direction;
        if (_isSwinging)
        {
            float progress = (float)(1.0 - _swingTimer / SWING_DURATION);
            Vector2 swordPos = GetSwordSwingPosition(User, direction, progress);
            float swordRotation = GetSwordSwingRotation(direction, progress);
            SpriteEffects swordEffect = (direction == Direction.Left) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 origin = (direction == Direction.Left) ? new Vector2(12, 12) : new Vector2(3, 12);
            spriteBatch.Draw(Texture, swordPos, null, Color.White, swordRotation, origin, User.Scale, swordEffect, 0f);
        }
    }

public Rectangle GetHitbox()
{
    if (!_isSwinging)
        return Rectangle.Empty;

    float progress = (float)(1.0 - _swingTimer / SWING_DURATION);
    Vector2 swordPos = GetSwordSwingPosition(User, _direction, progress);
    Vector2 origin = (_direction == Direction.Left) ? new Vector2(12, 12) : new Vector2(3, 12);
    int width = Texture.Width;
    int height = Texture.Height;

    return new Rectangle(
        (int)(swordPos.X - origin.X * User.Scale.X),
        (int)(swordPos.Y - origin.Y * User.Scale.Y),
        (int)(width * User.Scale.X),
        (int)(height * User.Scale.Y)
    );
}


    private Vector2 GetSwordSwingPosition(AnimatedSprite player, Direction direction, float progress)
    {
        // The sword's handle (origin) should stay fixed at the player's hand position
        // So we just return the hand position (player.getPosition() + offset)
        Vector2 offset = direction == Direction.Right ? new Vector2(24, 96) : new Vector2(8, 96);
        return player.getPosition() + offset;
    }

    private float GetSwordSwingRotation(Direction direction, float progress)
    {
        if (direction == Direction.Right)
        {
            float startAngle = -MathHelper.PiOver4;
            float endAngle = MathHelper.PiOver4;
            return MathHelper.Lerp(startAngle, endAngle, progress);
        }
        else
        {
            float startAngle = MathHelper.PiOver4;
            float endAngle = -MathHelper.PiOver4;
            return MathHelper.Lerp(startAngle, endAngle, progress);
        }
    }

    public Rectangle GetBounds()
    {
        return bounds;
    }

    public bool IsSwinging()
    {
        return _isSwinging;
    }
}