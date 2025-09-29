using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GooseLib.Inventory
{
    public class InventoryUI
    {
        private Inventory _inventory;
        private Texture2D _slotTexture;
        private Texture2D _emptySlotTexture;
        private SpriteFont _font;
        private Vector2 _position;
        private Vector2 _slotSize;
        private int _slotsPerRow;
        private float _slotSpacing;
        private bool _isVisible;
        private int _selectedSlot = 0;

        public bool IsVisible 
        { 
            get => _isVisible; 
            set => _isVisible = value; 
        }

        public int SelectedSlot
        {
            get => _selectedSlot;
            set => _selectedSlot = value;
        }

        public InventoryUI(Inventory inventory, Texture2D slotTexture, SpriteFont font, Vector2 position, Vector2 slotSize, int slotsPerRow = 8, float slotSpacing = 10f)
        {
            _inventory = inventory;
            _slotTexture = slotTexture;
            _font = font;
            _position = position;
            _slotSize = slotSize;
            _slotsPerRow = slotsPerRow;
            _slotSpacing = slotSpacing;
            _isVisible = true;

            CreateEmptySlotTexture();
        }

        private void CreateEmptySlotTexture()
        {
            _emptySlotTexture = _slotTexture;
        }

        public void SetEmptySlotTexture(Texture2D emptySlotTexture)
        {
            _emptySlotTexture = emptySlotTexture;
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (!_isVisible) return;

            if (_slotTexture == null)
            {
                _slotTexture = new Texture2D(graphicsDevice, 1, 1);
                _slotTexture.SetData(new[] { Color.White });
            }
            if (_emptySlotTexture == null)
            {
                _emptySlotTexture = _slotTexture;
            }

            for (int i = 0; i < _inventory.Size; i++)
            {
                Vector2 slotPosition = GetSlotPosition(i);
                Rectangle slotRect = new Rectangle((int)slotPosition.X, (int)slotPosition.Y, (int)_slotSize.X, (int)_slotSize.Y);

                Color slotColor = Color.DarkGray;
                InventoryItem item = _inventory.GetItem(i);
                
                if (item != null)
                {
                    slotColor = Color.LightGray;
                }

                // Highlight selected slot
                if (i == _selectedSlot)
                {
                    slotColor = Color.Yellow; // Bright highlight for selected slot
                }

                spriteBatch.Draw(_emptySlotTexture ?? CreatePixelTexture(graphicsDevice, Color.DarkGray), slotRect, slotColor);

                if (item != null && item.Icon != null)
                {
                    Rectangle iconRect = new Rectangle(
                        (int)(slotPosition.X + 4), 
                        (int)(slotPosition.Y + 4), 
                        (int)(_slotSize.X - 8), 
                        (int)(_slotSize.Y - 8)
                    );
                    spriteBatch.Draw(item.Icon, iconRect, Color.White);

                    if (item.Quantity > 1 && _font != null)
                    {
                        string quantityText = item.Quantity.ToString();
                        Vector2 textSize = _font.MeasureString(quantityText);
                        Vector2 textPosition = new Vector2(
                            slotPosition.X + _slotSize.X - textSize.X - 2,
                            slotPosition.Y + _slotSize.Y - textSize.Y - 2
                        );
                        
                        spriteBatch.DrawString(_font, quantityText, textPosition + Vector2.One, Color.Black);
                        spriteBatch.DrawString(_font, quantityText, textPosition, Color.White);
                    }
                }

                DrawBorder(spriteBatch, graphicsDevice, slotRect, Color.White, 2);
            }
        }

        private Vector2 GetSlotPosition(int slotIndex)
        {
            int row = slotIndex / _slotsPerRow;
            int col = slotIndex % _slotsPerRow;

            float x = _position.X + col * (_slotSize.X + _slotSpacing);
            float y = _position.Y + row * (_slotSize.Y + _slotSpacing);

            return new Vector2(x, y);
        }

        private Texture2D CreatePixelTexture(GraphicsDevice graphicsDevice, Color color)
        {
            Texture2D texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new[] { color });
            return texture;
        }

        private void DrawBorder(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Rectangle rectangle, Color color, int thickness)
        {
            Texture2D pixel = CreatePixelTexture(graphicsDevice, color);
            
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - thickness, rectangle.Width, thickness), color);
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X + rectangle.Width - thickness, rectangle.Y, thickness, rectangle.Height), color);
        }

        public void SetPosition(Vector2 position)
        {
            _position = position;
        }

        public void CenterOnScreen(int screenWidth, int screenHeight)
        {
            int totalRows = (_inventory.Size + _slotsPerRow - 1) / _slotsPerRow;
            float totalWidth = _slotsPerRow * _slotSize.X + (_slotsPerRow - 1) * _slotSpacing;
            float totalHeight = totalRows * _slotSize.Y + (totalRows - 1) * _slotSpacing;

            _position = new Vector2(
                (screenWidth - totalWidth) / 2,
                screenHeight - totalHeight 
            );
        }

        public int GetSlotAt(Vector2 mousePosition)
        {
            for (int i = 0; i < _inventory.Size; i++)
            {
                Vector2 slotPosition = GetSlotPosition(i);
                Rectangle slotRect = new Rectangle((int)slotPosition.X, (int)slotPosition.Y, (int)_slotSize.X, (int)_slotSize.Y);

                if (slotRect.Contains(mousePosition))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}