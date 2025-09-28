using System;
using GooseLib.Input;
using GooseLib.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GooseLib.Weapons;

public class Weapon
{
    public Texture2D Texture { get; set; }
    public Vector2 Anchor { get; set; }
    public Vector2 Scale { get; set; }
    public string Name { get; set; }
    public int Damage { get; set; }
    public float Range { get; set; }

    public Weapon(Texture2D texture, Vector2 anchor, Vector2 scale, string name, int damage, float range)
    {
        Texture = texture;
        Anchor = anchor;
        Scale = scale;
        Name = name;
        Damage = damage;
        Range = range;
    }

    // Should overriden by specific weapon types
    public virtual void Attack()
    {
        Console.WriteLine($"{Name} attacks for {Damage} damage");
    }

    public virtual void Update(GameTime gameTime)
    {

    }

    public virtual void Draw(SpriteBatch spriteBatch, Direction direction)
    {

    }
}
