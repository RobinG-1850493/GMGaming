using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GooseLib.Graphics;

public class TextureAtlas
{
    private Dictionary<string, TextureRegion> _regions = new();
    private Dictionary<string, Animation> _animations = new();

    public Texture2D Texture { get; private set; }

    public TextureAtlas()
    {
        _regions = new Dictionary<string, TextureRegion>();
        _animations = new Dictionary<string, Animation>();
    }

    public TextureAtlas(Texture2D texture)
    {
        Texture = texture;
        _regions = new Dictionary<string, TextureRegion>();
        _animations = new Dictionary<string, Animation>();
    }

    public void addAnimation(string name, Animation animation)
    {
        _animations.Add(name, animation);
    }

    public Animation getAnimation(string name)
    {
        return _animations[name];
    }

    public bool removeAnimation(string name)
    {
        return _animations.Remove(name);
    }

    public void AddRegion(string name, int x, int y, int width, int height)
    {
        TextureRegion region = new TextureRegion(Texture, x, y, width, height);
        _regions.Add(name, region);
    }

    public TextureRegion GetRegion(string name)
    {
        return _regions[name];
    }

    public bool RemoveRegion(string name)
    {
        return _regions.Remove(name);
    }

    public void Clear()
    {
        _regions.Clear();
    }

    public Sprite CreateSprite(string regionName)
    {
        TextureRegion region = GetRegion(regionName);
        return new Sprite(region);
    }

    public AnimatedSprite CreateAnimatedSprite(string animationName)
    {
        Animation animation = getAnimation(animationName);
        return new AnimatedSprite(animation);
    }

    public static TextureAtlas FromFile(ContentManager content, string fileName)
    {
        TextureAtlas atlas = new TextureAtlas();
        string filePath = Path.Combine(content.RootDirectory, fileName);

        using (Stream stream = TitleContainer.OpenStream(filePath))
        {
            using (XmlReader reader = XmlReader.Create(stream))
            {
                XDocument doc = XDocument.Load(reader);
                XElement root = doc.Root;

                string texturePath = root.Element("Texture").Value;
                atlas.Texture = content.Load<Texture2D>(texturePath);

                var regions = root.Element("Regions")?.Elements("Region");

                if (regions != null)
                {
                    foreach (var region in regions)
                    {
                        string name = region.Attribute("name").Value;
                        int x = int.Parse(region.Attribute("x").Value);
                        int y = int.Parse(region.Attribute("y").Value);
                        int width = int.Parse(region.Attribute("width").Value);
                        int height = int.Parse(region.Attribute("height").Value);

                        if (!string.IsNullOrEmpty(name))
                        {
                            atlas.AddRegion(name, x, y, width, height);
                        }
                    }
                }

                var animationElements = root.Element("Animations").Elements("Animation");

                if (animationElements != null)
                {
                    foreach (var animElem in animationElements)
                    {
                        string name = animElem.Attribute("name").Value;
                        float delayInMs = float.Parse(animElem.Attribute("delay").Value);
                        TimeSpan delay = TimeSpan.FromMilliseconds(delayInMs);

                        List<TextureRegion> frames = new List<TextureRegion>();

                        var frameElements = animElem.Elements("Frame");

                        if (frameElements != null)
                        {
                            foreach (var frameElem in frameElements)
                            {
                                string regionName = frameElem.Attribute("region").Value;
                                TextureRegion region = atlas.GetRegion(regionName);
                                frames.Add(region);
                            }
                        }
                        Animation animation = new Animation(frames, delay);
                        atlas.addAnimation(name, animation);
                    }
                }
            }
        }


        return atlas;
    }
}