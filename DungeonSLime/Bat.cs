using MonoGameLibrary.Graphic;
using Microsoft.Xna.Framework;

namespace DungeonSLime;

public class Bat
{ 
    public AnimatedSprite Sprite { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
}