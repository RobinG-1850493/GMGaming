namespace GooseLib.AI.Movement;

using GooseLib.Graphics;
using System;

public class MovementAIHoming : MovementAI
{
    public AnimatedSprite target { get; set; }

    public override void Move(AnimatedSprite subject)
    {
        var rand = new System.Random();
        float angle = (float)(rand.NextDouble() * 2 * Math.PI);
        var randomDirection = new Microsoft.Xna.Framework.Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        subject.setPosition(subject.getPosition() + randomDirection * 4.0f);


        if (target == null) return;

        var direction = target.getPosition() - subject.getPosition();
        direction += randomDirection * 0.5f; // Adding some randomness to the direction
        if (direction.Length() > 0)
        {
            direction.Normalize();
            subject.setPosition(subject.getPosition() + direction * 4.0f);
        }
    }
}