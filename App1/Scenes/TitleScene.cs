using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ClassLibrary;
using ClassLibrary.Scenes;
using ClassLibrary.Graphic;
using System.Collections.Generic;

namespace App1.Scenes;

public class TitleScene : Scene
{
    private const string DUNGEON_TEXT = "Dungeon";
    private const string SLIME_TEXT = "Slime";
    private const string PRESS_ENTER_TEXT = "Press Enter To Start";

    // The font to use to render normal text.
    private SpriteFont _font;

    // The font used to render the title text.
    private SpriteFont _font5x;

    // The position to draw the dungeon text at.
    private Vector2 _dungeonTextPos;

    // The origin to set for the dungeon text.
    private Vector2 _dungeonTextOrigin;

    // The position to draw the slime text at.
    private Vector2 _slimeTextPos;

    // The origin to set for the slime text.
    private Vector2 _slimeTextOrigin;

    // The position to draw the press enter text at.
    private Vector2 _pressEnterPos;

    // The origin to set for the press enter text when drawing it.
    private Vector2 _pressEnterOrigin;
    
    // Title界面的slime列表
    private List<Slime> _titleSlimes;
    
    // 移动速度
    private const float MOVEMENT_SPEED = 3.0f;
    
    // 排行榜相关变量
    private List<ScoreEntry> _leaderboardEntries;
    private Vector2 _leaderboardTitlePos;
    private Vector2 _leaderboardTitleOrigin;
    private Vector2 _leaderboardEntriesPos;
    private Vector2 _leaderboardEntriesOrigin;
    
    
    
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

        // While on the title screen, we can enable exit on escape so the player
        // can close the game by pressing the escape key.
        Core.ExitOnEscape = true;

        // 初始化title界面的slime
        _titleSlimes = new List<Slime>();
        InitializeTitleSlimes();

        // 加载排行榜数据
        _leaderboardEntries = LeaderboardManager.GetTopEntries(3);

        // Set the position and origin for the Dungeon text.
        Vector2 size = _font5x.MeasureString(DUNGEON_TEXT);
        _dungeonTextPos = new Vector2(640, 100);
        _dungeonTextOrigin = size * 0.5f;

        // Set the position and origin for the Slime text.
        size = _font5x.MeasureString(SLIME_TEXT);
        _slimeTextPos = new Vector2(757, 207);
        _slimeTextOrigin = size * 0.5f;

        // Set the position and origin for the press enter text.
        size = _font.MeasureString(PRESS_ENTER_TEXT);
        _pressEnterPos = new Vector2(640, 620);
        _pressEnterOrigin = size * 0.5f;

        // Set the position and origin for the leaderboard title.
        string leaderboardTitle = "Top 3";
        size = _font.MeasureString(leaderboardTitle);
        _leaderboardTitlePos = new Vector2(640, 300); // 左侧20%位置
        _leaderboardTitleOrigin = size * 0.5f;

        // Set the position and origin for the leaderboard entries.
        string leaderboardEntries = GenerateLeaderboardText();
        size = _font.MeasureString(leaderboardEntries);
        _leaderboardEntriesPos = new Vector2(640, 400); // 左侧20%位置，下方
        _leaderboardEntriesOrigin = size * 0.5f;
    }
    
    /// <summary>
    /// 初始化title界面的8只slime
    /// </summary>
    private void InitializeTitleSlimes()
    {
        // 创建纹理图集
        TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/File.xml");

        for (int i = 0; i < 8; i++)
        {
            // 创建slime精灵
            AnimatedSprite slimeSprite = atlas.CreateAnimatedSprite("slime-animation");
            slimeSprite.Scale = new Vector2(4.0f, 4.0f);
            
            // 设置随机颜色
            slimeSprite.Color = GetRandomColor();
            
            // 随机位置
            int totalColumns = Core.GraphicsDevice.PresentationParameters.BackBufferWidth / (int)slimeSprite.Width;
            int totalRows = Core.GraphicsDevice.PresentationParameters.BackBufferHeight / (int)slimeSprite.Height;
            
            int column = Random.Shared.Next(0, totalColumns);
            int row = Random.Shared.Next(0, totalRows);
            
            Vector2 position = new Vector2(column * slimeSprite.Width, row * slimeSprite.Height);
            
            // 随机左右移动方向
            float direction = Random.Shared.Next(0, 2) == 0 ? -1 : 1;
            Vector2 velocity = new Vector2(direction * MOVEMENT_SPEED, 0);
            
            // 创建slime对象
            Slime slime = new Slime(E_SlimeType.Head, slimeSprite, position)
            {
                Velocity = velocity,
                LastDirection = velocity
            };
            
            _titleSlimes.Add(slime);
        }
    }
    
    /// <summary>
    /// 获取随机颜色
    /// </summary>
    private Color GetRandomColor()
    {
        Color[] colors = {
            Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Purple,
            Color.Orange, Color.Pink, Color.Cyan, Color.Magenta, Color.Lime,
            Color.White, Color.Gray, Color.Brown, Color.Violet, Color.Turquoise
        };
        
        return colors[Random.Shared.Next(colors.Length)];
    }
    
    public override void Update(GameTime gameTime)
    {
        // 更新title界面的slime
        UpdateTitleSlimes(gameTime);
        
        // If the user presses enter, switch to the game scene.
        if (Core.InputMgr.Keyboard.WasKeyJustPressed(Keys.Enter) || Core.InputMgr.GamePads[0].WasButtonJustPressed(Buttons.A))
        {
            Core.ChangeScene(new GameScene());
        }
        
        // 刷新排行榜数据（每次更新都重新加载，确保显示最新数据）
        _leaderboardEntries = LeaderboardManager.GetTopEntries(3);
    }
    
    /// <summary>
    /// 更新title界面的slime
    /// </summary>
    private void UpdateTitleSlimes(GameTime gameTime)
    {
        Rectangle screenBounds = new Rectangle(
            0, 0,
            Core.GraphicsDevice.PresentationParameters.BackBufferWidth,
            Core.GraphicsDevice.PresentationParameters.BackBufferHeight
        );
        
        // 更新所有slime的位置和动画
        for (int i = _titleSlimes.Count - 1; i >= 0; i--)
        {
            Slime slime = _titleSlimes[i];
            
            // 更新位置
            slime.Position += slime.Velocity;
            
            // 更新动画
            slime.Update(gameTime);
            
            // 检查是否完全移出屏幕
            if (slime.IsOffScreen(screenBounds))
            {
                // 删除完全移出屏幕的slime
                _titleSlimes.RemoveAt(i);
                
                // 生成新的slime
                SpawnNewSlime();
            }
        }
    }
    
    /// <summary>
    /// 生成新的slime
    /// </summary>
    private void SpawnNewSlime()
    {
        // 创建纹理图集
        TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/File.xml");

        // 创建slime精灵
        AnimatedSprite slimeSprite = atlas.CreateAnimatedSprite("slime-animation");
        slimeSprite.Scale = new Vector2(4.0f, 4.0f);
        
        // 设置随机颜色
        slimeSprite.Color = GetRandomColor();
        
        // 随机位置
        int totalColumns = Core.GraphicsDevice.PresentationParameters.BackBufferWidth / (int)slimeSprite.Width;
        int totalRows = Core.GraphicsDevice.PresentationParameters.BackBufferHeight / (int)slimeSprite.Height;
        
        int column = Random.Shared.Next(0, totalColumns);
        int row = Random.Shared.Next(0, totalRows);
        
        Vector2 position = new Vector2(column * slimeSprite.Width, row * slimeSprite.Height);
        
        // 随机左右移动方向
        float direction = Random.Shared.Next(0, 2) == 0 ? -1 : 1;
        Vector2 velocity = new Vector2(direction * MOVEMENT_SPEED, 0);
        
        // 创建slime对象
        Slime slime = new Slime(E_SlimeType.Head, slimeSprite, position)
        {
            Velocity = velocity,
            LastDirection = velocity
        };
        
        _titleSlimes.Add(slime);
    }
    
    /// <summary>
    /// 生成排行榜文本
    /// </summary>
    /// <returns>格式化的排行榜文本</returns>
    private string GenerateLeaderboardText()
    {
        if (_leaderboardEntries == null || _leaderboardEntries.Count == 0)
        {
            return "No scores yet";
        }
        
        string text = "";
        for (int i = 0; i < _leaderboardEntries.Count; i++)
        {
            var entry = _leaderboardEntries[i];
            text += $"{i + 1}. {entry.Score} - {entry.FormatSurvivalTime()}\n\n";
        }
        
        // 移除最后一个换行符
        if (text.EndsWith("\n"))
        {
            text = text.Substring(0, text.Length - 1);
        }
        
        return text;
    }
    
    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

        // Begin the sprite batch to prepare for rendering.
        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // 绘制title界面的slime
        foreach (Slime slime in _titleSlimes)
        {
            slime.Draw(Core.SpriteBatch);
        }

        // The color to use for the drop shadow text.
        Color dropShadowColor = Color.Black * 0.5f;

        // Draw the Dungeon text slightly offset from it is original position and
        // with a transparent color to give it a drop shadow.
        Core.SpriteBatch.DrawString(_font5x, DUNGEON_TEXT, _dungeonTextPos + new Vector2(10, 10), dropShadowColor, 0.0f, _dungeonTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

        // Draw the Dungeon text on top of that at its original position.
        Core.SpriteBatch.DrawString(_font5x, DUNGEON_TEXT, _dungeonTextPos, Color.White, 0.0f, _dungeonTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

        // Draw the Slime text slightly offset from it is original position and
        // with a transparent color to give it a drop shadow.
        Core.SpriteBatch.DrawString(_font5x, SLIME_TEXT, _slimeTextPos + new Vector2(10, 10), dropShadowColor, 0.0f, _slimeTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

        // Draw the Slime text on top of that at its original position.
        Core.SpriteBatch.DrawString(_font5x, SLIME_TEXT, _slimeTextPos, Color.White, 0.0f, _slimeTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

        // Draw the press enter text.
        Core.SpriteBatch.DrawString(_font, PRESS_ENTER_TEXT, _pressEnterPos, Color.White, 0.0f, _pressEnterOrigin, 1.0f, SpriteEffects.None, 0.0f);

        // Draw the leaderboard title.
        Core.SpriteBatch.DrawString(_font, "Top 3", _leaderboardTitlePos, Color.Yellow, 0.0f, _leaderboardTitleOrigin, 1.0f, SpriteEffects.None, 0.0f);

        // Draw the leaderboard entries.
        string leaderboardText = GenerateLeaderboardText();
        Core.SpriteBatch.DrawString(_font, leaderboardText, _leaderboardEntriesPos, Color.Yellow, 0.0f, _leaderboardEntriesOrigin, 0.7f, SpriteEffects.None, 0.0f);

        // Always end the sprite batch when finished.
        Core.SpriteBatch.End();
    }




}