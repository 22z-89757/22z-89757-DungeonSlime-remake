using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ClassLibrary;
using ClassLibrary.Graphic;

namespace App1;

public class Game1 : Core
{
    // Defines the slime animated sprite.
    private AnimatedSprite _slime;

    // Defines the bat animated sprite.
    private AnimatedSprite _bat;
    
    private Texture2D _logo;  //the monogame logo texture
    
    
    
    public Game1() : base("awk's monogame", 1280, 720, false)
    {
        
    }

    protected override void Initialize()
    
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        // TODO: use this.Content to load your game content here
        
        // Create the texture atlas from the XML configuration file
        TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");  //这种方法可以使精灵定义与游戏逻辑分开

        // Create the slime animated sprite from the atlas.
        _slime = atlas.CreateAnimatedSprite("slime-animation");
        _slime.Scale = new Vector2(4.0f, 4.0f);

        // Create the bat animated sprite from the atlas.
        _bat = atlas.CreateAnimatedSprite("bat-animation");
        _bat.Scale = new Vector2(4.0f, 4.0f);
        
        
        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        
        // Update the slime animated sprite.
        _slime.Update(gameTime);

        // Update the bat animated sprite.
        _bat.Update(gameTime);
        
        

        base.Update(gameTime);
    }

    //all rendering should be done inside the Draw method. The Draw method's responsibility is to render the game state that was calculated in Update;
    //it should not contain any game logic or complex calculations.
    protected override void Draw(GameTime gameTime)  //Gametime类型参数可以提供delta时间(上一帧执行的时间)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        // TODO: Add your drawing code here

        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Draw the slime sprite.
        _slime.Draw(SpriteBatch, Vector2.One);

        // Draw the bat sprite 10px to the right of the slime.
        _bat.Draw(SpriteBatch, new Vector2(_slime.Width + 10, 0));

        // Always end the sprite batch when finished.
        SpriteBatch.End();
        
        base.Draw(gameTime);
    }
}