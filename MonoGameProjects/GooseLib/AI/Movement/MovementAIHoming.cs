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
        subject.setPosition(subject.getPosition() + randomDirection * 0.5f); // much less random movement

        if (target == null) return;

        var direction = target.getPosition() - subject.getPosition();
        // Only a slight random wobble added to homing direction
        direction += randomDirection * 0.1f;
        if (direction.Length() > 0)
        {
            direction.Normalize();
            subject.setPosition(subject.getPosition() + direction * 4.0f);
        }
    }
}