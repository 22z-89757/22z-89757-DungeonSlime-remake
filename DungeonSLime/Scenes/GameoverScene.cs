using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Graphic;
using MonoGameLibrary.Input;
using MonoGameLibrary.Scenes;

namespace DungeonSLime.Scenes;

public class GameoverScene : Scene
{
    private const string GAME_OVER_TEXT = "Game Over!";
    private const string PRESS_ENTER_TEXT = "Press Enter To Return";
    private const string SCORE_TEXT = "Final Score: ";
    private const string TIME_TEXT = "Survival Time: ";
    
    // The font to use to render normal text.
    private SpriteFont _font;

    // The font used to render the title text.
    private SpriteFont _font5x;

    // The position to draw the game over text at.
    private Vector2 _gameOverTextPos;

    // The origin to set for the game over text.
    private Vector2 _gameOverTextOrigin;

    // The position to draw the score text at.
    private Vector2 _scoreTextPos;

    // The origin to set for the score text.
    private Vector2 _scoreTextOrigin;
    
    // The position to draw the time text at.
    private Vector2 _timeTextPos;
    
    // The origin to set for the time text.
    private Vector2 _timeTextOrigin;

    // The position to draw the press enter text at.
    private Vector2 _pressEnterPos;

    // The origin to set for the press enter text when drawing it.
    private Vector2 _pressEnterOrigin;
    
    // 游戏分数
    private int _score;
    
    // 存活时间
    private TimeSpan _survivalTime;
    
    // 蝙蝠列表
    private List<Bat> _bats;
    
    // 蝙蝠生成计时器
    private float _batSpawnTimer;
    private const float BAT_SPAWN_INTERVAL = 0.5f; // 0.5秒生成一个蝙蝠
    
    // 移动速度
    private const float MOVEMENT_SPEED = 3.0f;
    
    
    public GameoverScene(int score, TimeSpan survivalTime)
    {
        _score = score;
        _survivalTime = survivalTime;
        _bats = new List<Bat>();
    }
    
    
    public override void LoadContent()
    {
        // Load the font for the standard text.
        _font = Core.Content.Load<SpriteFont>("fonts/04B_30");

        // Load the font for the title text.
        _font5x = Content.Load<SpriteFont>("fonts/04B_30_5x");
    }

    
    
    public override void Initialize()
    {
        // LoadContent is called during base.Initialize().
        base.Initialize();

        // While on the game over screen, we can enable exit on escape so the player
        // can close the game by pressing the escape key.
        Core.ExitOnEscape = true;

        // 保存分数到排行榜
        LeaderboardManager.AddEntry(_score, _survivalTime);

        // Set the position and origin for the game over text.
        Vector2 size = _font5x.MeasureString(GAME_OVER_TEXT);
        _gameOverTextPos = new Vector2(640, 150);
        _gameOverTextOrigin = size * 0.5f;

        // Set the position and origin for the score text.
        size = _font.MeasureString($"{SCORE_TEXT}{_score}");
        _scoreTextPos = new Vector2(640, 300);
        _scoreTextOrigin = size * 0.5f;
        
        // Set the position and origin for the time text.
        string timeText = $"{TIME_TEXT}{FormatSurvivalTime(_survivalTime)}";
        size = _font.MeasureString(timeText);
        _timeTextPos = new Vector2(640, 380);
        _timeTextOrigin = size * 0.5f;

        // Set the position and origin for the press enter text.
        size = _font.MeasureString(PRESS_ENTER_TEXT);
        _pressEnterPos = new Vector2(640, 550);
        _pressEnterOrigin = size * 0.5f;
        
        // 初始化蝙蝠生成计时器
        _batSpawnTimer = 0f;
    }
    
    public override void Update(GameTime gameTime)
    {
        // 如果用户按下回车键，返回主菜单
        if (Core.InputMgr.Keyboard.WasKeyJustReleased(Keys.Enter) || Core.InputMgr.GamePads[0].WasButtonJustReleased(Buttons.A))
        {
            Core.ChangeScene(new TitleScene());
        }
        
        // 更新蝙蝠生成
        UpdateBatSpawning(gameTime);
        
        // 更新所有蝙蝠
        UpdateBats(gameTime);
    }
    
    /// <summary>
    /// 更新蝙蝠生成
    /// </summary>
    private void UpdateBatSpawning(GameTime gameTime)
    {
        // 增加计时器
        _batSpawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // 如果计时器超过间隔时间且蝙蝠数量少于10个，则生成新蝙蝠
        if (_batSpawnTimer >= BAT_SPAWN_INTERVAL && _bats.Count < 10)
        {
            SpawnBat();
            _batSpawnTimer = 0f;
        }
    }
    
    /// <summary>
    /// 生成一个蝙蝠
    /// </summary>
    private void SpawnBat()
    {
        // 创建纹理图集
        TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/File.xml");
        
        // 创建蝙蝠精灵
        AnimatedSprite batSprite = atlas.CreateAnimatedSprite("bat-animation");
        batSprite.Scale = new Vector2(4.0f, 4.0f);
        
        // 随机位置
        int totalColumns = Core.GraphicsDevice.PresentationParameters.BackBufferWidth / (int)batSprite.Width;
        int totalRows = Core.GraphicsDevice.PresentationParameters.BackBufferHeight / (int)batSprite.Height;
        
        int column = Random.Shared.Next(0, totalColumns);
        int row = Random.Shared.Next(0, totalRows);
        
        Vector2 position = new Vector2(column * batSprite.Width, row * batSprite.Height);
        
        // 随机速度
        float angle = (float)(Random.Shared.NextDouble() * Math.PI * 2);
        float x = (float)Math.Cos(angle);
        float y = (float)Math.Sin(angle);
        Vector2 velocity = new Vector2(x, y) * MOVEMENT_SPEED;
        
        // 创建蝙蝠对象
        Bat bat = new Bat
        {
            Sprite = batSprite,
            Position = position,
            Velocity = velocity
        };
        
        _bats.Add(bat);
    }
    
    /// <summary>
    /// 更新所有蝙蝠
    /// </summary>
    private void UpdateBats(GameTime gameTime)
    {
        Rectangle screenBounds = new Rectangle(
            0, 0,
            Core.GraphicsDevice.PresentationParameters.BackBufferWidth,
            Core.GraphicsDevice.PresentationParameters.BackBufferHeight
        );
        
        for (int i = _bats.Count - 1; i >= 0; i--)
        {
            Bat bat = _bats[i];
            
            // 更新蝙蝠位置
            Vector2 newBatPosition = bat.Position + bat.Velocity;
            
            Circle batBounds = new Circle(
                (int)(newBatPosition.X + (bat.Sprite.Width * 0.5f)),
                (int)(newBatPosition.Y + (bat.Sprite.Height * 0.5f)),
                (int)(bat.Sprite.Width * 0.5f)
            );
            
            Vector2 normal = Vector2.Zero;
            
            // 蝙蝠碰到墙壁反弹
            if (batBounds.Left < screenBounds.Left)
            {
                normal.X = Vector2.UnitX.X;
                newBatPosition.X = screenBounds.Left;
            }
            else if (batBounds.Right > screenBounds.Right)
            {
                normal.X = -Vector2.UnitX.X;
                newBatPosition.X = screenBounds.Right - bat.Sprite.Width;
            }
            
            if (batBounds.Top < screenBounds.Top)
            {
                normal.Y = Vector2.UnitY.Y;
                newBatPosition.Y = screenBounds.Top;
            }
            else if (batBounds.Bottom > screenBounds.Bottom)
            {
                normal.Y = -Vector2.UnitY.Y;
                newBatPosition.Y = screenBounds.Bottom - bat.Sprite.Height;
            }
            
            if (normal != Vector2.Zero)
            {
                normal.Normalize();
                bat.Velocity = Vector2.Reflect(bat.Velocity, normal);
            }
            
            bat.Position = newBatPosition;
            
            // 更新动画
            bat.Sprite.Update(gameTime);
        }
    }
    
    /// <summary>
    /// 格式化存活时间为"X min Y sec"格式
    /// </summary>
    private string FormatSurvivalTime(TimeSpan timeSpan)
    {
        int minutes = timeSpan.Minutes;
        int seconds = timeSpan.Seconds;
        return $"{minutes} min {seconds} sec";
    }
    
    public override void Draw(GameTime gameTime)
    {
        // 纯红色背景
        Core.GraphicsDevice.Clear(Color.Red);

        // Begin the sprite batch to prepare for rendering.
        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // The color to use for the drop shadow text.
        Color dropShadowColor = Color.Black * 0.5f;

        // Draw the game over text slightly offset from it is original position and
        // with a transparent color to give it a drop shadow.
        Core.SpriteBatch.DrawString(_font5x, GAME_OVER_TEXT, _gameOverTextPos + new Vector2(10, 10), dropShadowColor, 0.0f, _gameOverTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

        // Draw the game over text on top of that at its original position.
        Core.SpriteBatch.DrawString(_font5x, GAME_OVER_TEXT, _gameOverTextPos, Color.White, 0.0f, _gameOverTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

        // Draw the score text.
        Core.SpriteBatch.DrawString(_font, $"{SCORE_TEXT}{_score}", _scoreTextPos, Color.White, 0.0f, _scoreTextOrigin, 1.0f, SpriteEffects.None, 0.0f);
        
        // Draw the time text.
        string timeText = $"{TIME_TEXT}{FormatSurvivalTime(_survivalTime)}";
        Core.SpriteBatch.DrawString(_font, timeText, _timeTextPos, Color.White, 0.0f, _timeTextOrigin, 1.0f, SpriteEffects.None, 0.0f);

        // Draw the press enter text.
        Core.SpriteBatch.DrawString(_font, PRESS_ENTER_TEXT, _pressEnterPos, Color.White, 0.0f, _pressEnterOrigin, 1.0f, SpriteEffects.None, 0.0f);
        
        // 绘制所有蝙蝠
        foreach (Bat bat in _bats)
        {
            bat.Sprite.Draw(Core.SpriteBatch, bat.Position);
        }

        // Always end the sprite batch when finished.
        Core.SpriteBatch.End();
    }




}