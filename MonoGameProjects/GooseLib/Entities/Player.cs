using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GooseLib.Graphics;
using GooseLib.Utils;
using GooseLib.Weapons;


public class Player
{
    public AnimatedSprite PlayerSprite { get; set; }
    public Weapon EquippedWeapon { get; set; }
    public Rectangle playerBounds => new Rectangle((int)PlayerSprite.getX(), (int)PlayerSprite.getY(), (int)(PlayerSprite.Region.Width * PlayerSprite.Scale.X), (int)(PlayerSprite.Region.Height * PlayerSprite.Scale.Y));
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;

    private const float GRAVITY = 0.5f;
    private bool _isJumping = false;
    private float _playerVerticalVelocity = 0f;
    private float _jumpStartY = 0f;

    public Player(TextureAtlas atlas, Texture2D weapon, string startingAnimation, Vector2 position, Vector2 scale, int maxHealth)
    {
        PlayerSprite = new AnimatedSprite();
        PlayerSprite.setAnimation(atlas.getAnimation(startingAnimation));
        PlayerSprite.setPosition(position);
        PlayerSprite.Scale = scale;
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        EquippedWeapon = new Sword(PlayerSprite, weapon, Vector2.Zero, Vector2.One, "sword", 1, 1.0f);
    }

    public void Update(GameTime gameTime)
    {
        if (_isJumping)
        {
            _playerVerticalVelocity += GRAVITY;
            PlayerSprite.setY(PlayerSprite.getY() + _playerVerticalVelocity);
            // Land when falling back to or below the jump start Y
            if (_playerVerticalVelocity > 0 && PlayerSprite.getY() >= _jumpStartY)
            {
                PlayerSprite.setY(_jumpStartY);
                _playerVerticalVelocity = 0f;
                _isJumping = false;
            }
        }

        EquippedWeapon.Update(gameTime);
        PlayerSprite.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch, Direction direction)
    {
        PlayerSprite.Draw(spriteBatch, PlayerSprite.getPosition());
        EquippedWeapon.Draw(spriteBatch, direction);
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth -= amount;
        if (CurrentHealth < 0) CurrentHealth = 0;
    }

    public void Heal(int amount)
    {
        CurrentHealth += amount;
        if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;
    }

    public void EquipWeapon(Weapon weapon)
    {
        EquippedWeapon = weapon;
    }

    public void checkBounds(Rectangle bounds)
    {
        if (playerBounds.Left < bounds.Left)
        {
            PlayerSprite.setX(bounds.Left);
        }
        else if (playerBounds.Right > bounds.Right)
        {
            PlayerSprite.setX(bounds.Right - playerBounds.Width);
        }
        if (playerBounds.Top < bounds.Top)
        {
            PlayerSprite.setY(bounds.Top);
        }
        else if (playerBounds.Bottom > bounds.Bottom)
        {
            PlayerSprite.setY(bounds.Bottom - playerBounds.Height);
        }
    }
}
