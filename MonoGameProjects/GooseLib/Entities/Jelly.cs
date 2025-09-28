using Microsoft.Xna.Framework;
using GooseLib.Graphics;
using Microsoft.Xna.Framework.Graphics;
using System;

public class Jelly
{
    public AnimatedSprite JellySprite { get; private set; }
    public Vector2 Bounds => new Vector2(JellySprite.Region.Width * JellySprite.Scale.X, JellySprite.Region.Height * JellySprite.Scale.Y);
    public int Health { get; private set; } = 3;
    public bool IsAlive => Health > 0;

    public Jelly(TextureAtlas atlas, string animationName, Vector2 position, Vector2 scale)
    {
        JellySprite = new AnimatedSprite();
        JellySprite.setAnimation(atlas.getAnimation(animationName));
        JellySprite.setPosition(position);
        JellySprite.Scale = scale;
    }

    public void Update(GameTime gameTime)
    {
        JellySprite.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        JellySprite.Draw(spriteBatch, JellySprite.getPosition());
    }

    public bool collidingWith(AnimatedSprite other)
    {
        Rectangle jellyBounds = new Rectangle((int)JellySprite.getX(), (int)JellySprite.getY(), (int)(JellySprite.Region.Width * JellySprite.Scale.X), (int)(JellySprite.Region.Height * JellySprite.Scale.Y));
        Rectangle otherBounds = new Rectangle((int)other.getX(), (int)other.getY(), (int)(other.Region.Width * other.Scale.X), (int)(other.Region.Height * other.Scale.Y));

        if (jellyBounds.Intersects(otherBounds))
        {
            Console.WriteLine("Collision detected between jelly and other sprite!");

            Vector2 direction = other.getPosition() - JellySprite.getPosition();
            if (direction.Length() > 0)
            {
                direction.Normalize();
                JellySprite.setPosition(JellySprite.getPosition() - direction * 10.0f);
            }
            return true;
        }
        return false;
    }
    
    public void TakeDamage(int amount)
    {
        Health -= amount;
    }
}