using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GooseLib.Graphics;
using GooseLib.Utils;
using GooseLib.Weapons;
using GooseLib.Inventory;


public class Player
{
    public AnimatedSprite PlayerSprite { get; set; }
    public InventoryItem EquippedItem { get; set; }
    public int SelectedSlot { get; set; } = 0; // Currently selected inventory slot
    public GooseLib.Inventory.Inventory PlayerInventory { get; set; }
    public Rectangle playerBounds => new Rectangle((int)PlayerSprite.getX(), (int)PlayerSprite.getY(), (int)(PlayerSprite.Region.Width * PlayerSprite.Scale.X), (int)(PlayerSprite.Region.Height * PlayerSprite.Scale.Y));
    public int CurrentHealth { get; set; } = 100;
    public int MaxHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;
    public double Stamina { get; set; } = 500;

    private bool isFlashing = false;
    private double flashTimer = 0;

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
        
        PlayerInventory = new GooseLib.Inventory.Inventory(8);
        
        var startingSword = new Sword(PlayerSprite, weapon, Vector2.Zero, Vector2.One, "sword", 1, 1.0f);
        var swordItem = InventoryItemFactory.CreateWeaponItem(startingSword);
        PlayerInventory.AddItem(swordItem);
        
        // Equip the first item by default
        UpdateEquippedItem();
    }

    public void Update(GameTime gameTime)
    {
        UpdateEquippedItem();

        if (_isJumping)
        {
            _playerVerticalVelocity += GRAVITY;
            PlayerSprite.setY(PlayerSprite.getY() + _playerVerticalVelocity);
            if (_playerVerticalVelocity > 0 && PlayerSprite.getY() >= _jumpStartY)
            {
                PlayerSprite.setY(_jumpStartY);
                _playerVerticalVelocity = 0f;
                _isJumping = false;
            }
        }

        if (isFlashing)
        {
            PlayerSprite.Color = Color.Red;
            flashTimer -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (flashTimer <= 0)
            {
                isFlashing = false;
                PlayerSprite.Color = Color.White;
            }
        }

        if (EquippedItem != null && EquippedItem.Data is Weapon weapon)
        {
            weapon.Update(gameTime);
        }
        PlayerSprite.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch, Direction direction)
    {
        PlayerSprite.Draw(spriteBatch, PlayerSprite.getPosition());
        if (EquippedItem != null && EquippedItem.Data is Weapon weapon)
        {
            weapon.Draw(spriteBatch, direction);
        }
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth -= amount;
        isFlashing = true;
        flashTimer = 75;
        if (CurrentHealth < 0) CurrentHealth = 0;
    }

    public void TakeDamage(int amount, int staminaDrain)
    {
        CurrentHealth -= amount;
        Stamina -= staminaDrain;
        isFlashing = true;
        flashTimer = 75;
        if (CurrentHealth < 0) CurrentHealth = 0;
    }

    public void Heal(int amount)
    {
        CurrentHealth += amount;
        if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;
    }

    public void SelectSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < PlayerInventory.Size)
        {
            SelectedSlot = slotIndex;
            UpdateEquippedItem();
        }
    }

    public void UpdateEquippedItem()
    {
        EquippedItem = PlayerInventory.GetItem(SelectedSlot);
    }

    public bool CanAttack()
    {
        return EquippedItem != null && EquippedItem.Data is Weapon;
    }

    public Weapon GetEquippedWeapon()
    {
        return EquippedItem?.Data as Weapon;
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
