using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GooseLib;
using GooseLib.Graphics;
using GooseLib.Input;
using GooseLib.Utils;
using GooseLib.AI.Movement;
using GooseLib.Inventory;
using System.Collections.Generic;
using Penumbra;

namespace mgProject_1;

public class Game1 : Core
{
    private Player _player;
    private const float MOVEMENT_SPEED = 3.0f;
    private double _jellySpawnTimer = 0;
    private List<Jelly> _jellies = new List<Jelly>();
    private bool _isJumping = false;
    private bool _isRunning = false;
    private const float JUMP_VELOCITY = -10.0f;
    private const float GRAVITY = 0.6f;
    private float _jumpStartY = 0f;
    private Direction lastDirection = Direction.Down;
    private bool _isIdling = true;
    private Texture2D _barTexture;
    private InventoryUI _inventoryUI;
    private bool _isInventoryVisible = true;

    private TextureAtlas atlas = new TextureAtlas();
    private TextureAtlas jellyAtlas = new TextureAtlas();
    private Tilemap _tilemap;
    private Texture2D slotTexture;

    public Game1() : base("Goosey & Moosey's Grand Adventures", 1920, 1080, false)
    {

    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        atlas = TextureAtlas.FromFile(Content, "Images/female-base-atlas.xml");
        _player = new Player(atlas, Content.Load<Texture2D>("Images/wooden_sword"), "female_base_front_idle", new Vector2(400, 300), new Vector2(4.0f, 4.0f), 100);
        atlas.CreateAnimatedSprite("female_base_front_idle");

        jellyAtlas = TextureAtlas.FromFile(Content, "Images/jelly-def.xml");
        for (int i = 0; i < 4; i++)
        {
            // _jellies.Add(new Jelly(jellyAtlas, 20, "jelly", new Vector2(100 + i * 150, 300), new Vector2(3.0f, 3.0f)));
        }

        _tilemap = Tilemap.FromFile(Content, "Images/tilemap-definition.xml");
        _tilemap.Scale = new Vector2(4.0f, 4.0f);

        _inventoryUI = new InventoryUI(
            _player.PlayerInventory, 
            null, 
            null, 
            Vector2.Zero, 
            new Vector2(64, 64), 
            8, 
            8f
        );
        _inventoryUI.CenterOnScreen(Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight);
    

        Light sun = new PointLight
        {
            Scale = new Vector2(10000f),
            ShadowType = ShadowType.Solid
        };

        penumbra.Lights.Add(sun);

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        if (_barTexture == null)
        {
            _barTexture = new Texture2D(GraphicsDevice, 1, 1);
            _barTexture.SetData(new[] { Color.White });
        }

        // Create slot texture if needed
        if (slotTexture == null)
        {
            slotTexture = new Texture2D(GraphicsDevice, 1, 1);
            slotTexture.SetData(new[] { Color.White });
        }
        _jellySpawnTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

        // Mouse click triggers weapon attack
        MouseState mouseState = Mouse.GetState();
        if (mouseState.LeftButton == ButtonState.Pressed && _player.CanAttack())
        {
            var weapon = _player.GetEquippedWeapon();
            weapon?.Attack();
        }

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (_jellySpawnTimer % 5000 < 16)
        {
            Random random = new Random();
            int r = random.Next(3, 6);
            _jellies.Add(new Jelly(jellyAtlas, r, "jelly", new Vector2(random.Next(0, 1920), random.Next(0, 1080)), new Vector2(r, r)));
        }

        for (int i = 0; i < _jellies.Count; i++)
        {
            if (!_jellies[i].IsKnockbackActive)
            {
                new MovementAIHoming { target = _player.PlayerSprite }.Move(_jellies[i].JellySprite);
            }
        }

        _player.Update(gameTime);
        for (int i = 0; i < _jellies.Count; i++)
        {
            _jellies[i].Update(gameTime);
        }

        CheckKeyboardInput(gameTime);

        Rectangle screenBounds = new Rectangle(0, 0, Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight);
        Rectangle playerBounds = new Rectangle((int)_player.PlayerSprite.getX(), (int)_player.PlayerSprite.getY(), (int)(_player.PlayerSprite.Region.Width * _player.PlayerSprite.Scale.X), (int)(_player.PlayerSprite.Region.Height * _player.PlayerSprite.Scale.Y));

        for (int i = 0; i < _jellies.Count; i++)
        {
            bool collided = _jellies[i].collidingWith(_player.PlayerSprite);
            if (collided && _jellies[i].CanAttackPlayer())
            {
                _player.TakeDamage(1);
                Console.WriteLine($"Player Health: {_player.CurrentHealth}");
                _jellies[i].OnAttack(_player.PlayerSprite.getPosition());
                if (_player.CurrentHealth <= 0)
                {
                    Console.WriteLine("Player has been defeated!");
                    Exit();
                }
            }
        }

        for (int i = 0; i < _jellies.Count; i++)
        {
            for (int j = i + 1; j < _jellies.Count; j++)
            {
                _jellies[i].collidingWith(_jellies[j].JellySprite);
            }
        }

        _player.checkBounds(screenBounds);

        // Remove dead jellies
        _jellies.RemoveAll(jelly => !jelly.IsAlive);

        // Weapon damage logic
        var equippedWeapon = _player.GetEquippedWeapon();
        if (equippedWeapon is GooseLib.Weapons.Sword sword && sword.IsSwinging())
        {
            Rectangle swordHitbox = sword.GetHitbox();
            foreach (var jelly in _jellies)
            {
                Rectangle jellyHitbox = new Rectangle(
                    (int)jelly.JellySprite.getX(),
                    (int)jelly.JellySprite.getY(),
                    (int)(jelly.JellySprite.Region.Width * jelly.JellySprite.Scale.X),
                    (int)(jelly.JellySprite.Region.Height * jelly.JellySprite.Scale.Y)
                );
                if (swordHitbox.Intersects(jellyHitbox) && jelly.IsAlive && sword.canHitEntity(jelly))
                {
                    jelly.TakeDamage(sword.Damage);
                    // Smooth knockback away from player
                    Vector2 playerPos = _player.PlayerSprite.getPosition();
                    Vector2 jellyPos = jelly.JellySprite.getPosition();
                    Vector2 knockbackDir = jellyPos - playerPos;
                    float knockbackStrength = 500f; // pixels per second
                    double knockbackDuration = 100; // ms
                    jelly.ApplyKnockback(knockbackDir, knockbackStrength, knockbackDuration);
                }
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        penumbra.BeginDraw();
        GraphicsDevice.Clear(Color.CornflowerBlue);

        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _tilemap.Draw(SpriteBatch);

        // Draw health bar
        int barWidth = 200;
        int barHeight = 20;
        int healthBarX = 10;
        int healthBarY = 10;
        float healthPercent = (float)_player.CurrentHealth / _player.MaxHealth;
        // Background
        SpriteBatch.Draw(_barTexture, new Rectangle(healthBarX, healthBarY, barWidth, barHeight), Color.DarkRed);
        // Foreground
        SpriteBatch.Draw(_barTexture, new Rectangle(healthBarX, healthBarY, (int)(barWidth * healthPercent), barHeight), Color.Red);

        // Draw stamina bar below health
        int staminaBarY = healthBarY + barHeight + 8;
        float staminaPercent = (float)_player.Stamina / 5000f;
        SpriteBatch.Draw(_barTexture, new Rectangle(healthBarX, staminaBarY, barWidth, barHeight), Color.DarkGreen);
        SpriteBatch.Draw(_barTexture, new Rectangle(healthBarX, staminaBarY, (int)(barWidth * staminaPercent), barHeight), Color.LimeGreen);

        _player.Draw(SpriteBatch, lastDirection);

        for (int i = 0; i < _jellies.Count; i++)
            _jellies[i].JellySprite.Draw(SpriteBatch, _jellies[i].JellySprite.getPosition());

        // Draw inventory UI
        _inventoryUI.Draw(SpriteBatch, GraphicsDevice);

        SpriteBatch.End();
        penumbra.Draw(gameTime);
        base.Draw(gameTime);
    }

    private void CheckKeyboardInput(GameTime gameTime)
    {
        // Toggle inventory with I key
        if (Input.Keyboard.WasKeyJustPressed(Keys.I))
        {
            _isInventoryVisible = !_isInventoryVisible;
            _inventoryUI.IsVisible = _isInventoryVisible;
            Console.WriteLine($"Inventory visibility toggled: {_isInventoryVisible}");
        }

        // Select inventory slots with number keys 1-8
        if (Input.Keyboard.WasKeyJustPressed(Keys.D1))
        {
            _player.SelectSlot(0);
            _inventoryUI.SelectedSlot = 0;
            Console.WriteLine("Selected slot 1");
        }
        if (Input.Keyboard.WasKeyJustPressed(Keys.D2))
        {
            _player.SelectSlot(1);
            _inventoryUI.SelectedSlot = 1;
            Console.WriteLine("Selected slot 2");
        }
        if (Input.Keyboard.WasKeyJustPressed(Keys.D3))
        {
            _player.SelectSlot(2);
            _inventoryUI.SelectedSlot = 2;
            Console.WriteLine("Selected slot 3");
        }
        if (Input.Keyboard.WasKeyJustPressed(Keys.D4))
        {
            _player.SelectSlot(3);
            _inventoryUI.SelectedSlot = 3;
            Console.WriteLine("Selected slot 4");
        }
        if (Input.Keyboard.WasKeyJustPressed(Keys.D5))
        {
            _player.SelectSlot(4);
            _inventoryUI.SelectedSlot = 4;
            Console.WriteLine("Selected slot 5");
        }
        if (Input.Keyboard.WasKeyJustPressed(Keys.D6))
        {
            _player.SelectSlot(5);
            _inventoryUI.SelectedSlot = 5;
            Console.WriteLine("Selected slot 6");
        }
        if (Input.Keyboard.WasKeyJustPressed(Keys.D7))
        {
            _player.SelectSlot(6);
            _inventoryUI.SelectedSlot = 6;
            Console.WriteLine("Selected slot 7");
        }
        if (Input.Keyboard.WasKeyJustPressed(Keys.D8))
        {
            _player.SelectSlot(7);
            _inventoryUI.SelectedSlot = 7;
            Console.WriteLine("Selected slot 8");
        }

        // Drop item from current slot with X key
        if (Input.Keyboard.WasKeyJustPressed(Keys.X))
        {
            var droppedItem = _player.PlayerInventory.RemoveItem(_player.SelectedSlot, 1);
            if (droppedItem != null)
                Console.WriteLine($"Dropped {droppedItem.Name} from slot {_player.SelectedSlot + 1}");
        }

        // Legacy drop functionality for testing
        if (Input.Keyboard.WasKeyJustPressed(Keys.A))
        {
            _player.PlayerInventory.RemoveItem(0, 1);
        }

        float speed = MOVEMENT_SPEED;

        if (Input.Keyboard.IsKeyDown(Keys.LeftShift) && _player.Stamina > 0)
        {
            speed *= 2.0f;
            _player.Stamina -= gameTime.ElapsedGameTime.TotalMilliseconds;
        }

        if (Input.Keyboard.WasKeyJustPressed(Keys.LeftShift) && _player.Stamina > 0)
        {
            _isRunning = true;
        }

        if (Input.Keyboard.WasKeyJustReleased(Keys.LeftShift))
        {
            _isRunning = false;
        }

        if (!Input.Keyboard.IsKeyDown(Keys.LeftShift) && _player.Stamina < 5000)
        {
            _player.Stamina += gameTime.ElapsedGameTime.TotalMilliseconds * 0.5;
        }

        if (Input.Keyboard.IsKeyDown(Keys.Z) || Input.Keyboard.IsKeyDown(Keys.Up))
        {
            if (Input.Keyboard.WasKeyJustPressed(Keys.Z) || Input.Keyboard.WasKeyJustPressed(Keys.Up) || Input.Keyboard.WasKeyJustPressed(Keys.LeftShift))
            {
                if (_isRunning)
                    _player.PlayerSprite.setAnimation(atlas.getAnimation("female_base_back_run"));
                else
                    _player.PlayerSprite.setAnimation(atlas.getAnimation("female_base_back_walk"));
            }
            _player.PlayerSprite.setY(_player.PlayerSprite.getY() - speed);
            _player.PlayerSprite.Update(gameTime);
            lastDirection = Direction.Up;
            _isIdling = false;
        }

        if (Input.Keyboard.IsKeyDown(Keys.S) || Input.Keyboard.IsKeyDown(Keys.Down))
        {
            if (Input.Keyboard.WasKeyJustPressed(Keys.S) || Input.Keyboard.WasKeyJustPressed(Keys.Down) || Input.Keyboard.WasKeyJustPressed(Keys.LeftShift))
            {
                if (_isRunning)
                    _player.PlayerSprite.setAnimation(atlas.getAnimation("female_base_front_run"));
                else
                    _player.PlayerSprite.setAnimation(atlas.getAnimation("female_base_front_walk"));
            }
            _player.PlayerSprite.setY(_player.PlayerSprite.getY() + speed);
            _player.PlayerSprite.Update(gameTime);
            lastDirection = Direction.Down;
            _isIdling = false;
        }

        if (Input.Keyboard.IsKeyDown(Keys.Q) || Input.Keyboard.IsKeyDown(Keys.Left))
        {
            if (Input.Keyboard.WasKeyJustPressed(Keys.Q) || Input.Keyboard.WasKeyJustPressed(Keys.Left) || Input.Keyboard.WasKeyJustPressed(Keys.LeftShift))
            {
                if (_isRunning)
                    _player.PlayerSprite.setAnimation(atlas.getAnimation("female_base_side_run"));
                else
                    _player.PlayerSprite.setAnimation(atlas.getAnimation("female_base_side_walk"));
            }
            _player.PlayerSprite.setX(_player.PlayerSprite.getX() - speed);
            _player.PlayerSprite.Effects = SpriteEffects.FlipHorizontally;
            _player.Update(gameTime);
            lastDirection = Direction.Left;
            _isIdling = false;
        }

        if (Input.Keyboard.IsKeyDown(Keys.D) || Input.Keyboard.IsKeyDown(Keys.Right))
        {
            if (Input.Keyboard.WasKeyJustPressed(Keys.D) || Input.Keyboard.WasKeyJustPressed(Keys.Right) || Input.Keyboard.WasKeyJustPressed(Keys.LeftShift))
            {
                if (_isRunning)
                    _player.PlayerSprite.setAnimation(atlas.getAnimation("female_base_side_run"));
                else
                    _player.PlayerSprite.setAnimation(atlas.getAnimation("female_base_side_walk"));
            }
            _player.PlayerSprite.setX(_player.PlayerSprite.getX() + speed);
            _player.Update(gameTime);
            _player.PlayerSprite.Effects = SpriteEffects.None;
            lastDirection = Direction.Right;
            _isIdling = false;
        }

        if (!Input.Keyboard.IsKeyDown(Keys.Z) && !Input.Keyboard.IsKeyDown(Keys.Up) &&
            !Input.Keyboard.IsKeyDown(Keys.S) && !Input.Keyboard.IsKeyDown(Keys.Down) &&
            !Input.Keyboard.IsKeyDown(Keys.Q) && !Input.Keyboard.IsKeyDown(Keys.Left) &&
            !Input.Keyboard.IsKeyDown(Keys.D) && !Input.Keyboard.IsKeyDown(Keys.Right))
        {
            if (!_isIdling)
            {
                switch (lastDirection)
                {
                    case Direction.Up:
                        _player.PlayerSprite.setAnimation(atlas.getAnimation("female_base_front_idle"));
                        break;
                    case Direction.Down: 
                        _player.PlayerSprite.setAnimation(atlas.getAnimation("female_base_back_idle"));
                        break;
                    case Direction.Left:
                        _player.PlayerSprite.setAnimation(atlas.getAnimation("female_base_side_idle"));
                        _player.PlayerSprite.Effects = SpriteEffects.FlipHorizontally;
                        break;
                    case Direction.Right:
                        _player.PlayerSprite.setAnimation(atlas.getAnimation("female_base_side_idle"));
                        _player.PlayerSprite.Effects = SpriteEffects.None;
                        break;
                }
            }
            _isIdling = true;
            _player.Update(gameTime);
        }
    }
}