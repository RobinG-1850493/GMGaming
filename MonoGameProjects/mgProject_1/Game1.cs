using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GooseLib;
using GooseLib.Graphics;
using GooseLib.Input;
using GooseLib.Utils;

namespace mgProject_1;

public class Game1 : Core
{
    private const float MOVEMENT_SPEED = 3.0f;

    private AnimatedSprite _witch;
    private Vector2 _witchPos;
    private float _witchVerticalVelocity = 0f;
    private bool _isJumping = false;
    private const float JUMP_VELOCITY = -10.0f;
    private const float GRAVITY = 0.6f;
    private float _groundY = 0f;
    private float _jumpStartY = 0f;
    private Direction? lastDirection = Direction.Down;
    private bool _isIdling = true;

    private TextureAtlas atlas = new TextureAtlas();
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
        atlas = TextureAtlas.FromFile(Content, "Images/atlas-def.xml");
        _witch = atlas.CreateAnimatedSprite("witch_idle_backward");
        _witch.Scale = new Vector2(4.0f, 4.0f);

        _tilemap = Tilemap.FromFile(Content, "Images/tilemap-definition.xml");
        _tilemap.Scale = new Vector2(4.0f, 4.0f);
        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();


        // Apply jump physics
        if (_isJumping)
        {
            _witchVerticalVelocity += GRAVITY;
            _witchPos.Y += _witchVerticalVelocity;
            // Land when falling back to or below the jump start Y
            if (_witchVerticalVelocity > 0 && _witchPos.Y >= _jumpStartY)
            {
                _witchPos.Y = _jumpStartY;
                _witchVerticalVelocity = 0f;
                _isJumping = false;
            }
        }

        CheckKeyboardInput(gameTime);
        CheckGamePadInput(gameTime);

        Rectangle screenBounds = new Rectangle(0, 0, Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight);
        Rectangle witchBounds = new Rectangle((int)_witchPos.X, (int)_witchPos.Y, (int)(_witch.Region.Width * _witch.Scale.X), (int)(_witch.Region.Height * _witch.Scale.Y));

        if (witchBounds.Left < screenBounds.Left)
        {
            _witchPos.X = screenBounds.Left;
        }
        else if (witchBounds.Right > screenBounds.Right)
        {
            _witchPos.X = screenBounds.Right - witchBounds.Width;
        }
        if (witchBounds.Top < screenBounds.Top)
        {
            _witchPos.Y = screenBounds.Top;
        }
        else if (witchBounds.Bottom > screenBounds.Bottom)
        {
            _witchPos.Y = screenBounds.Bottom - witchBounds.Height;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _tilemap.Draw(SpriteBatch);

        _witch.Draw(SpriteBatch, _witchPos);
        SpriteBatch.End();

        base.Draw(gameTime);
    }

    private void CheckKeyboardInput(GameTime gameTime)
    {
        float speed = MOVEMENT_SPEED;
       
        if (Input.Keyboard.IsKeyDown(Keys.LeftShift))
        {
            speed *= 2.0f;
        }

        if (Input.Keyboard.IsKeyDown(Keys.W) || Input.Keyboard.IsKeyDown(Keys.Up))
        {
            if (Input.Keyboard.WasKeyJustPressed(Keys.W) || Input.Keyboard.WasKeyJustPressed(Keys.Up))
            {
                _witch.setAnimation(atlas.getAnimation("witch_running_forward"));
            }
            _witchPos.Y -= speed;
            _witch.Update(gameTime);
            lastDirection = Direction.Up;
            _isIdling = false;
        }

        if (Input.Keyboard.IsKeyDown(Keys.S) || Input.Keyboard.IsKeyDown(Keys.Down))
        {
            if (Input.Keyboard.WasKeyJustPressed(Keys.S) || Input.Keyboard.WasKeyJustPressed(Keys.Down))
            {
                _witch.setAnimation(atlas.getAnimation("witch_running_backward"));
            }
            _witchPos.Y += speed;
            _witch.Update(gameTime);
            lastDirection = Direction.Down;
            _isIdling = false;
        }

        if (Input.Keyboard.IsKeyDown(Keys.A) || Input.Keyboard.IsKeyDown(Keys.Left))
        {
            if (Input.Keyboard.WasKeyJustPressed(Keys.A) || Input.Keyboard.WasKeyJustPressed(Keys.Left))
            {
                _witch.setAnimation(atlas.getAnimation("witch_running_left"));
            }
            _witchPos.X -= speed;
            _witch.Update(gameTime);
            lastDirection = Direction.Left;
            _isIdling = false;
        }

        if (Input.Keyboard.IsKeyDown(Keys.D) || Input.Keyboard.IsKeyDown(Keys.Right))
        {
            if (Input.Keyboard.WasKeyJustPressed(Keys.D) || Input.Keyboard.WasKeyJustPressed(Keys.Right))
            {
                _witch.setAnimation(atlas.getAnimation("witch_running_right"));
            }
            _witchPos.X += speed;
            _witch.Update(gameTime);
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
                        _witch.setAnimation(atlas.getAnimation("witch_idle_forward"));
                        break;
                    case Direction.Down:
                        _witch.setAnimation(atlas.getAnimation("witch_idle_backward"));
                        break;
                    case Direction.Left:
                        _witch.setAnimation(atlas.getAnimation("witch_idle_left"));
                        break;
                    case Direction.Right:
                        _witch.setAnimation(atlas.getAnimation("witch_idle_right"));
                        break;
                }
            }
            _isIdling = true;
            _witch.Update(gameTime);
        }

        // Make the witch jump
        if (Input.Keyboard.WasKeyJustPressed(Keys.Space))
        {
            if (!_isJumping)
            {
                _isJumping = true;
                _witchVerticalVelocity = JUMP_VELOCITY;
                _jumpStartY = _witchPos.Y;
            }
        }
    }

    private void CheckGamePadInput(GameTime gameTime)
    {
        GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

        float speed = MOVEMENT_SPEED;
        if (gamePadState.IsButtonDown(Buttons.A))
        {
            speed *= 2.0f;
            GamePad.SetVibration(PlayerIndex.One, 1.0f, 1.0f);
        }
        else
        {
            GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
        }

        if (gamePadState.ThumbSticks.Left != Vector2.Zero)
        {
            _witchPos.X += gamePadState.ThumbSticks.Left.X * speed;
            _witchPos.Y -= gamePadState.ThumbSticks.Left.Y * speed;
            _witch.Update(gameTime);
        }
        else
        {
            if (gamePadState.IsButtonDown(Buttons.DPadUp))
            {
                _witchPos.Y -= speed;
                _witch.Update(gameTime);
            }

            if (gamePadState.IsButtonDown(Buttons.DPadDown))
            {
                _witchPos.Y += speed;
                _witch.Update(gameTime);
            }

            if (gamePadState.IsButtonDown(Buttons.DPadLeft))
            {
                _witchPos.X -= speed;
                _witch.Update(gameTime);
            }

            if (gamePadState.IsButtonDown(Buttons.DPadRight))
            {
                _witchPos.X += speed;
                _witch.Update(gameTime);
            }
        }
    }
}
