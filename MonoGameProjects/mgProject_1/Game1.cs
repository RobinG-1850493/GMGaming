using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GooseLib;
using GooseLib.Graphics;
using GooseLib.Input;
using GooseLib.Utils;
using GooseLib.AI.Movement;
using System.Collections.Generic;

namespace mgProject_1;

public class Game1 : Core
{
    private Player _player;
    private const float MOVEMENT_SPEED = 3.0f;
    private double _jellySpawnTimer = 0;
    private double stamina = 5000;
    private int _health = 1000;
    private List<Jelly> _jellies = new List<Jelly>();
    private float _playerVerticalVelocity = 0f;
    private bool _isJumping = false;
    private bool _isRunning = false;
    private const float JUMP_VELOCITY = -10.0f;
    private const float GRAVITY = 0.6f;
    private float _jumpStartY = 0f;
    private Direction lastDirection = Direction.Down;
    private bool _isIdling = true;

    private TextureAtlas atlas = new TextureAtlas();
    private TextureAtlas jellyAtlas = new TextureAtlas();
    private Tilemap _tilemap;
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
           // _jellies.Add(new Jelly(jellyAtlas, "jelly", new Vector2(100 + i * 150, 300), new Vector2(3.0f, 3.0f)));
        }

        _tilemap = Tilemap.FromFile(Content, "Images/tilemap-definition.xml");
        _tilemap.Scale = new Vector2(4.0f, 4.0f);


        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        _jellySpawnTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
        Console.WriteLine($"Stamina: {stamina}");

        // Mouse click triggers sword swing
        MouseState mouseState = Mouse.GetState();
        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            _player.EquippedWeapon.Attack();
        }

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (_jellySpawnTimer % 5000 < 16)
        {
            Random random = new Random();
            _jellies.Add(new Jelly(jellyAtlas, "jelly", new Vector2(random.Next(0, 1920), random.Next(0, 1080)), new Vector2(random.Next(2, 4), random.Next(2, 4))));
        }

        for (int i = 0; i < _jellies.Count; i++)
        {
            new MovementAIHoming { target = _player.PlayerSprite }.Move(_jellies[i].JellySprite);
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

            if (collided)
            {
                _health -= 1;
                Console.WriteLine($"Player Health: {_health}");
                if (_health <= 0)
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

        // Sword damage logic
        if (_player.EquippedWeapon is GooseLib.Weapons.Sword sword && sword.IsSwinging())
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
                if (swordHitbox.Intersects(jellyHitbox) && jelly.IsAlive)
                {
                    jelly.TakeDamage(sword.Damage);
                }
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _tilemap.Draw(SpriteBatch);

        _player.Draw(SpriteBatch, lastDirection);

        for (int i = 0; i < _jellies.Count; i++)
            _jellies[i].JellySprite.Draw(SpriteBatch, _jellies[i].JellySprite.getPosition());

        SpriteBatch.End();

        base.Draw(gameTime);
    }

    private void CheckKeyboardInput(GameTime gameTime)
    {
        float speed = MOVEMENT_SPEED;

        if (Input.Keyboard.IsKeyDown(Keys.LeftShift) && stamina > 0)
        {
            speed *= 2.0f;
            stamina -= gameTime.ElapsedGameTime.TotalMilliseconds;
        }

        if (Input.Keyboard.WasKeyJustPressed(Keys.LeftShift) && stamina > 0)
        {
            _isRunning = true;
        }

        if (Input.Keyboard.WasKeyJustReleased(Keys.LeftShift))
        {
            _isRunning = false;
        }

        if (!Input.Keyboard.IsKeyDown(Keys.LeftShift) && stamina < 5000)
        {
            stamina += gameTime.ElapsedGameTime.TotalMilliseconds * 0.5;
        }

        if (Input.Keyboard.IsKeyDown(Keys.W) || Input.Keyboard.IsKeyDown(Keys.Up))
        {
            if (Input.Keyboard.WasKeyJustPressed(Keys.W) || Input.Keyboard.WasKeyJustPressed(Keys.Up) || Input.Keyboard.WasKeyJustPressed(Keys.LeftShift))
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

        if (Input.Keyboard.IsKeyDown(Keys.A) || Input.Keyboard.IsKeyDown(Keys.Left))
        {
            if (Input.Keyboard.WasKeyJustPressed(Keys.A) || Input.Keyboard.WasKeyJustPressed(Keys.Left) || Input.Keyboard.WasKeyJustPressed(Keys.LeftShift))
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

        if (!Input.Keyboard.IsKeyDown(Keys.W) && !Input.Keyboard.IsKeyDown(Keys.Up) &&
            !Input.Keyboard.IsKeyDown(Keys.S) && !Input.Keyboard.IsKeyDown(Keys.Down) &&
            !Input.Keyboard.IsKeyDown(Keys.A) && !Input.Keyboard.IsKeyDown(Keys.Left) &&
            !Input.Keyboard.IsKeyDown(Keys.D) && !Input.Keyboard.IsKeyDown(Keys.Right))
        {
            if (!_isIdling && lastDirection != null)
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

        // Make the witch jump
        if (Input.Keyboard.WasKeyJustPressed(Keys.Space))
        {
            if (!_isJumping)
            {
                _isJumping = true;
                _playerVerticalVelocity = JUMP_VELOCITY;
                _jumpStartY = _player.PlayerSprite.getY();
            }
        }

    }
}