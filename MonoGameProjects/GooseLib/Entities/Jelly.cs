using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework;
using GooseLib.Graphics;
using Microsoft.Xna.Framework.Graphics;
using System;

public class Jelly
{
    private double attackCooldown = 0;
    private Vector2 knockbackVelocity = Vector2.Zero;
    private double knockbackTimer = 0;
    public AnimatedSprite JellySprite { get; private set; }
    public Vector2 Bounds => new Vector2(JellySprite.Region.Width * JellySprite.Scale.X, JellySprite.Region.Height * JellySprite.Scale.Y);
    public int Health { get; private set; } = 3;
    public bool IsAlive => Health > 0;
    public bool IsKnockbackActive => knockbackTimer > 0;

    private bool isFlashing = false;
    private double flashTimer = 0;

    public Jelly(TextureAtlas atlas, int health, string animationName, Vector2 position, Vector2 scale)
    {
        JellySprite = new AnimatedSprite();
        JellySprite.setAnimation(atlas.getAnimation(animationName));
        JellySprite.setPosition(position);
        JellySprite.Scale = scale;
        Health = health;
    }

    public void Update(GameTime gameTime)
    {
        // Update attack cooldown
        if (attackCooldown > 0)
        {
            attackCooldown -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (attackCooldown < 0) attackCooldown = 0;
        }

        JellySprite.Update(gameTime);
        // Apply knockback if active
        if (knockbackTimer > 0)
        {
            JellySprite.setPosition(JellySprite.getPosition() + knockbackVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds);
            knockbackTimer -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (knockbackTimer <= 0)
            {
                knockbackVelocity = Vector2.Zero;
                knockbackTimer = 0;
            }
        }
        if (isFlashing)
        {
            JellySprite.Color = Color.Red;
            flashTimer -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (flashTimer <= 0)
            {
                isFlashing = false;
                JellySprite.Color = Color.White;
            }
        }
    }

    public bool CanAttackPlayer() => attackCooldown <= 0;

    public void OnAttack(Vector2 playerPosition)
    {
        attackCooldown = 100;
        // Knockback away from player
        Vector2 knockbackDir = JellySprite.getPosition() - playerPosition;
        float knockbackStrength = 500f; // pixels per second
        double knockbackDuration = 100; // ms
        ApplyKnockback(knockbackDir, knockbackStrength, knockbackDuration);
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
        isFlashing = true;
        flashTimer = 150;
        // Knockback will be set externally
    }

    public void ApplyKnockback(Vector2 direction, float strength, double durationMs)
    {
        if (direction.Length() > 0)
        {
            direction.Normalize();
            knockbackVelocity = direction * strength;
            knockbackTimer = durationMs;
        }
    }
}
