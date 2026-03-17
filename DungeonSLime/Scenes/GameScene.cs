using System;
using System.Collections.Generic;
using Gum.DataTypes;
using Gum.Wireframe;
using Gum.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Graphic;
using MonoGameLibrary.Input;
using MonoGameLibrary.Scenes;
using MonoGameGum;
using Gum.Forms.Controls;
using MonoGameGum.GueDeriving;
using DungeonSlime.UI;

namespace DungeonSLime.Scenes;

public class GameScene : Scene
{
    // 蛇头
    private Slime _snakeHead;
    
    // 蛇身体节点列表
    private List<Slime> _snakeBody;
    
    // 蝙蝠列表
    private List<Bat> _bats;
    
    // 蝙蝠生成计时器
    private float _batSpawnTimer;
    private const float BAT_SPAWN_INTERVAL = 2f; // 2秒生成一个蝙蝠
    
    // 移动速度
    private const float MOVEMENT_SPEED = 3.0f;
    
    // 身体节点之间的距离（以帧数计算）
    private const int BODY_DISTANCE_IN_FRAMES = 20;

    // 游戏开始时间
    private DateTime _gameStartTime;
    
    // The sound effect to play when the bat bounces off the edge of the screen.
    private SoundEffect _bounceSoundEffect;

    // The sound effect to play when the slime eats a bat.
    private SoundEffect _collectSoundEffect;
    
    // The SpriteFont Description used to draw text.
    private SpriteFont _font;

    // Tracks the players score.
    private int _score;

    // Defines the position to draw the score text at.
    private Vector2 _scoreTextPosition;

    // Defines the origin used when drawing the score text.
    private Vector2 _scoreTextOrigin;
    
    // The SpriteFont used to draw time text.
    private SpriteFont _timeFont;

    // Defines the position to draw the time text at.
    private Vector2 _timeTextPosition;

    // Defines the origin used when drawing the time text.
    private Vector2 _timeTextOrigin;
    
    // 体力值UI相关字段
    private SpriteFont _staminaFont; // 体力值字体
    private Vector2 _staminaBarPosition; // 体力值条位置
    private Vector2 _staminaTextPosition; // 体力值文字位置
    private Texture2D _pixelTexture; // 用于绘制矩形的1x1像素纹理
    
    // A reference to the pause panel UI element so we can set its visibility
    // when the game is paused.
    private Panel _pausePanel;

    // A reference to the resume button UI element so we can focus it
    // when the game is paused.
    private AnimatedButton _resumeButton;

    // The UI sound effect to play when a UI event is triggered.
    private SoundEffect _uiSoundEffect;

    // Reference to the texture atlas that we can pass to UI elements when they
    // are created.
    private TextureAtlas _atlas;
    
    
    
    public GameScene()
    {
        _snakeBody = new List<Slime>();
        _bats = new List<Bat>();
        _gameStartTime = DateTime.Now;
    }

    public override void Initialize()
    {
        base.Initialize();
        
        // 初始化蝙蝠生成计时器
        _batSpawnTimer = 0f;
        
        // Set the position of the score text
        _scoreTextPosition = new Vector2(15, 20);

        // Set the origin of the text so it is left-centered.
        float scoreTextYOrigin = _font.MeasureString("Score").Y * 0.5f;
        _scoreTextOrigin = new Vector2(0, scoreTextYOrigin);
        
        // Set the position of the time text (below the score)
        _timeTextPosition = new Vector2(15, 60);

        // Set the origin of the time text so it is left-centered.
        float timeTextYOrigin = _timeFont.MeasureString("Time: 00:00").Y * 0.5f;
        _timeTextOrigin = new Vector2(0, timeTextYOrigin);
        
        // 初始化体力值条位置（右上角）
        _staminaBarPosition = new Vector2(
            Core.GraphicsDevice.PresentationParameters.BackBufferWidth - 600, 
            20
        );
        _staminaTextPosition = new Vector2(
            Core.GraphicsDevice.PresentationParameters.BackBufferWidth - 340, 
            45
        );
        
        // Initialize the Gum UI for the pause menu.
        InitializeUI();
    }

    public override void LoadContent()
    {
        // 创建纹理图集
        _atlas = TextureAtlas.FromFile(Core.Content, "images/File.xml");

        // 创建蛇头
        AnimatedSprite headSprite = _atlas.CreateAnimatedSprite("slime-animation");
        headSprite.Scale = new Vector2(4.0f, 4.0f);
        headSprite.Color = Color.Orange;
        
        _snakeHead = new Slime(
            E_SlimeType.Head, 
            headSprite, 
            new Vector2(640, 360) // 屏幕中心
        );
        
        // Load the bounce sound effect
        _bounceSoundEffect = Content.Load<SoundEffect>("audio/bounce");

        // Load the collect sound effect
        _collectSoundEffect = Content.Load<SoundEffect>("audio/collect");
        
        // Load the font
        _font = Content.Load<SpriteFont>("fonts/04B_30");
        
        // Load the time font
        _timeFont = Content.Load<SpriteFont>("fonts/04B_30");
        
        // Load the stamina font
        _staminaFont = Content.Load<SpriteFont>("fonts/04B_30");
        
        // Create a 1x1 pixel texture for drawing rectangles
        _pixelTexture = new Texture2D(Core.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
        
        // Load the sound effect to play when ui actions occur.
        _uiSoundEffect = Core.Content.Load<SoundEffect>("audio/ui");
        
        base.LoadContent();
    }

    public override void Update(GameTime gameTime)
    {
        
        // Ensure the UI is always updated
        GumService.Default.Update(gameTime);

        // If the game is paused, do not continue
        if (_pausePanel.IsVisible)
        {
            return;
        }

        // 处理输入
        HandleInput();
        
        // 更新蛇头位置
        _snakeHead.UpdateHead();
        
        // 更新蛇身体节点位置（基于历史路径）
        UpdateSnakeBody();
        
        // 更新蝙蝠生成
        UpdateBatSpawning(gameTime);
        
        // 更新所有蝙蝠
        UpdateBats(gameTime);
        
        // 更新所有动画
        _snakeHead.Update(gameTime);
        _snakeHead.UpdateStamina(gameTime);
        foreach (var bodySegment in _snakeBody)
        {
            bodySegment.Update(gameTime);
        }
        
        // 边界检测
        HandleScreenBounds();
        
        // 碰撞检测
        CheckCollisions();
        
        base.Update(gameTime);
    }
    
    private void PauseGame()
    {
        // Make the pause panel UI element visible.
        _pausePanel.IsVisible = true;

        // Set the resume button to have focus
        _resumeButton.IsFocused = true;
    }

    
    /// <summary>
    /// 处理输入，更新蛇头速度
    /// </summary>
    private void HandleInput()
    {
        Vector2 direction = Vector2.Zero;
        float speed = MOVEMENT_SPEED;
        bool hasInput = false;
        bool isSprinting = false;
        
        // 键盘输入
        if (Core.InputMgr.Keyboard.IsKeyDown(Keys.Space) && _snakeHead.CanSprint())
        {
            speed *= 1.5f;
            isSprinting = true;
        }
        
        if (Core.InputMgr.Keyboard.IsKeyDown(Keys.W) || Core.InputMgr.Keyboard.IsKeyDown(Keys.Up))
        {
            direction.Y -= 1;
            hasInput = true;
        }
        if (Core.InputMgr.Keyboard.IsKeyDown(Keys.S) || Core.InputMgr.Keyboard.IsKeyDown(Keys.Down))
        {
            direction.Y += 1;
            hasInput = true;
        }
        if (Core.InputMgr.Keyboard.IsKeyDown(Keys.A) || Core.InputMgr.Keyboard.IsKeyDown(Keys.Left))
        {
            direction.X -= 1;
            hasInput = true;
        }
        if (Core.InputMgr.Keyboard.IsKeyDown(Keys.D) || Core.InputMgr.Keyboard.IsKeyDown(Keys.Right))
        {
            direction.X += 1;
            hasInput = true;
        }
        
        // 暂停界面
        if (Core.InputMgr.Keyboard.WasKeyJustPressed(Keys.M))
        {
            PauseGame();
            return;
        }

        // If the + button is pressed, increase the volume.
        if (Core.InputMgr.Keyboard.WasKeyJustPressed(Keys.OemPlus))
        {
            Core.Audio.SongVolume += 0.1f;
            Core.Audio.SoundEffectVolume += 0.1f;
        }

        // If the - button was pressed, decrease the volume.
        if (Core.InputMgr.Keyboard.WasKeyJustPressed(Keys.OemMinus))
        {
            Core.Audio.SongVolume -= 0.1f;
            Core.Audio.SoundEffectVolume -= 0.1f;
        }
        
        
        
        // 手柄输入
        GamePadInfo gamePadOne = Core.InputMgr.GamePads[(int)PlayerIndex.One];
        if (gamePadOne.IsButtonDown(Buttons.RightTrigger) && _snakeHead.CanSprint())
        {
            speed *= 1.5f;
            isSprinting = true;
            gamePadOne.SetVibration(1.0f, TimeSpan.FromSeconds(1));
        }
        else
        {
            gamePadOne.StopVibration();
        }
        
        // 手柄摇杆输入（带死区和归一化）
        const float DEADZONE = 0.2f;
        Vector2 thumbStick = gamePadOne.LeftThumbStick;
        if (Math.Abs(thumbStick.X) > DEADZONE || Math.Abs(thumbStick.Y) > DEADZONE)
        {
            direction.X = thumbStick.X;
            direction.Y = -thumbStick.Y;
            hasInput = true;
        }
        else
        {
            // 手柄方向键输入
            if (gamePadOne.IsButtonDown(Buttons.DPadUp))
            {
                direction.Y -= 1;
                hasInput = true;
            }
            if (gamePadOne.IsButtonDown(Buttons.DPadDown))
            {
                direction.Y += 1;
                hasInput = true;
            }
            if (gamePadOne.IsButtonDown(Buttons.DPadLeft))
            {
                direction.X -= 1;
                hasInput = true;
            }
            if (gamePadOne.IsButtonDown(Buttons.DPadRight))
            {
                direction.X += 1;
                hasInput = true;
            }
        }
        
        // 暂停界面
        if (gamePadOne.WasButtonJustPressed(Buttons.Start))
        {
            PauseGame();
            return;
        }
        
        // 更新方向和速度
        if (hasInput && direction != Vector2.Zero)
        {
            // 有输入：归一化方向并更新LastDirection
            direction.Normalize();
            _snakeHead.LastDirection = direction;
        }
        
        // 始终使用LastDirection移动（持续移动）
        _snakeHead.Velocity = _snakeHead.LastDirection * speed;
        
        // 设置冲刺状态
        _snakeHead.SetSprinting(isSprinting);
    }

    #region 暂停界面UI处理

    private void CreatePausePanel()
    {
        _pausePanel = new Panel();
        _pausePanel.Anchor(Anchor.Center);
        _pausePanel.WidthUnits = DimensionUnitType.Absolute;
        _pausePanel.HeightUnits = DimensionUnitType.Absolute;
        _pausePanel.Height = 70;
        _pausePanel.Width = 264;
        _pausePanel.IsVisible = false;
        _pausePanel.AddToRoot();

        TextureRegion backgroundRegion = _atlas.GetRegion("panel-background");

        NineSliceRuntime background = new NineSliceRuntime();
        background.Dock(Dock.Fill);
        background.Texture = backgroundRegion.Texture;
        background.TextureAddress = TextureAddress.Custom;
        background.TextureHeight = backgroundRegion.Height;
        background.TextureLeft = backgroundRegion.SourceRectangle.Left;
        background.TextureTop = backgroundRegion.SourceRectangle.Top;
        background.TextureWidth = backgroundRegion.Width;
        _pausePanel.AddChild(background);

        var textInstance = new TextRuntime();
        textInstance.Text = "PAUSED";
        textInstance.CustomFontFile = @"fonts/04b_30.fnt";
        textInstance.UseCustomFont = true;
        textInstance.FontScale = 0.5f;
        textInstance.X = 10f;
        textInstance.Y = 10f;
        _pausePanel.AddChild(textInstance);

        _resumeButton = new AnimatedButton(_atlas);
        _resumeButton.Text = "RESUME";
        _resumeButton.Anchor(Anchor.BottomLeft);
        _resumeButton.X = 9f;
        _resumeButton.Y = -9f;
        _resumeButton.Width = 80;
        _resumeButton.Click += HandleResumeButtonClicked;
        _pausePanel.AddChild(_resumeButton);

        AnimatedButton quitButton = new AnimatedButton(_atlas);
        quitButton.Text = "QUIT";
        quitButton.Anchor(Anchor.BottomRight);
        quitButton.X = -9f;
        quitButton.Y = -9f;
        quitButton.Width = 80;
        quitButton.Click += HandleQuitButtonClicked;

        _pausePanel.AddChild(quitButton);
    }

    private void HandleResumeButtonClicked(object sender, EventArgs e)
    {
        // A UI interaction occurred, play the sound effect
        Core.Audio.PlaySoundEffect(_uiSoundEffect);

        // Make the pause panel invisible to resume the game.
        _pausePanel.IsVisible = false;
    }

    private void HandleQuitButtonClicked(object sender, EventArgs e)
    {
        Core.Audio.ToggleMute();
        
        // A UI interaction occurred, play the sound effect
        Core.Audio.PlaySoundEffect(_uiSoundEffect);

        // Go back to the title scene.
        Core.ChangeScene(new TitleScene());
    }

    private void InitializeUI()
    {
        GumService.Default.Root.Children.Clear();

        CreatePausePanel();
    }
    
    #endregion
    
    /// <summary>
    /// 更新蛇身体节点位置（历史路径回溯法）
    /// </summary>
    private void UpdateSnakeBody()
    {
        for (int i = 0; i < _snakeBody.Count; i++)
        {
            // 第 i 个身体节点的位置 = 蛇头在 (i+1) * BODY_DISTANCE_IN_FRAMES 帧之前的位置
            int framesAgo = (i + 1) * BODY_DISTANCE_IN_FRAMES;
            _snakeBody[i].Position = _snakeHead.GetPositionAtFrame(framesAgo);
        }
    }
    
    /// <summary>
    /// 添加一个身体节点
    /// </summary>
    private void AddBodySegment(TextureAtlas atlas)
    {
        AnimatedSprite bodySprite = atlas.CreateAnimatedSprite("slime-animation");
        bodySprite.Scale = new Vector2(4.0f, 4.0f);
        
        // 为每个身体节点设置不同的颜色
        Color[] colors = { Color.LightGreen, Color.LightBlue, Color.LightPink, 
                          Color.LightYellow, Color.LightCoral, Color.LightCyan };
        bodySprite.Color = colors[_snakeBody.Count % colors.Length];
        
        Slime bodySegment = new Slime(
            E_SlimeType.Body,
            bodySprite,
            _snakeHead.Position // 初始位置设为蛇头位置
        );
        
        _snakeBody.Add(bodySegment);
    }
    
    /// <summary>
    /// 处理屏幕边界
    /// </summary>
    private void HandleScreenBounds()
    {
        Rectangle screenBounds = new Rectangle(
            0, 0,
            Core.GraphicsDevice.PresentationParameters.BackBufferWidth,
            Core.GraphicsDevice.PresentationParameters.BackBufferHeight
        );
        
        Circle headBounds = new Circle(
            (int)(_snakeHead.Position.X + (_snakeHead.Sprite.Width * 0.5f)),
            (int)(_snakeHead.Position.Y + (_snakeHead.Sprite.Height * 0.5f)),
            (int)(_snakeHead.Sprite.Width * 0.3f)
        );
        
        // 边界限制
        Vector2 newPosition = _snakeHead.Position;
        
        if (headBounds.Left < screenBounds.Left)
        {
            newPosition.X = screenBounds.Left;
        }
        else if (headBounds.Right > screenBounds.Right)
        {
            newPosition.X = screenBounds.Right - _snakeHead.Sprite.Width;
        }
        
        if (headBounds.Top < screenBounds.Top)
        {
            newPosition.Y = screenBounds.Top;
        }
        else if (headBounds.Bottom > screenBounds.Bottom)
        {
            newPosition.Y = screenBounds.Bottom - _snakeHead.Sprite.Height;
        }
        
        _snakeHead.Position = newPosition;
    }
    
    /// <summary>
    /// 更新蝙蝠生成
    /// </summary>
    private void UpdateBatSpawning(GameTime gameTime)
    {
        // 增加计时器
        _batSpawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // 如果计时器超过间隔时间且蝙蝠数量少于4个，则生成新蝙蝠
        if (_batSpawnTimer >= BAT_SPAWN_INTERVAL && _bats.Count < 4)
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
        _atlas = TextureAtlas.FromFile(Content, "images/File.xml");
        
        // 创建蝙蝠精灵
        AnimatedSprite batSprite = _atlas.CreateAnimatedSprite("bat-animation");
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
                
                // Play the bounce sound effect
                Core.Audio.PlaySoundEffect(_bounceSoundEffect);
            }
            
            bat.Position = newBatPosition;
            
            // 更新动画
            bat.Sprite.Update(gameTime);
        }
    }
    
    /// <summary>
    /// 检测碰撞
    /// </summary>
    private void CheckCollisions()
    {
        Circle headBounds = new Circle(
            (int)(_snakeHead.Position.X + (_snakeHead.Sprite.Width * 0.5f)),
            (int)(_snakeHead.Position.Y + (_snakeHead.Sprite.Height * 0.5f)),
            (int)(_snakeHead.Sprite.Width * 0.3f)
        );
        
        // 检查与墙壁的碰撞
        Rectangle screenBounds = new Rectangle(
            0, 0,
            Core.GraphicsDevice.PresentationParameters.BackBufferWidth,
            Core.GraphicsDevice.PresentationParameters.BackBufferHeight
        );
        
        if (headBounds.Left < screenBounds.Left ||
            headBounds.Right > screenBounds.Right ||
            headBounds.Top < screenBounds.Top ||
            headBounds.Bottom > screenBounds.Bottom)
        {
            GameOver();
            return;
        }
        
        // 检查与身体的碰撞
        if (_snakeHead.CheckBodyCollision(_snakeBody))
        {
            GameOver();
            return;
        }
        
        // 检查与所有蝙蝠的碰撞
        for (int i = _bats.Count - 1; i >= 0; i--)
        {
            Bat bat = _bats[i];
            
            Circle batBounds = new Circle(
                (int)(bat.Position.X + (bat.Sprite.Width * 0.5f)),
                (int)(bat.Position.Y + (bat.Sprite.Height * 0.5f)),
                (int)(bat.Sprite.Width * 0.5f)
            );
            
            // 蛇头吃到蝙蝠
            if (headBounds.Intersects(batBounds))
            {
                // Play the collect sound effect
                Core.Audio.PlaySoundEffect(_collectSoundEffect);
                
                // Increase the player's score.
                _score += 100;
                
                // 添加一个身体节点
                TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/File.xml");
                AddBodySegment(atlas);
                
                // 移除被吃掉的蝙蝠
                _bats.RemoveAt(i);
            }
        }
    }
    
    /// <summary>
    /// 游戏结束处理
    /// </summary>
    private void GameOver()
    {
        // 计算存活时间
        TimeSpan survivalTime = DateTime.Now - _gameStartTime;
        
        // 切换到游戏结束场景
        Core.ChangeScene(new GameoverScene(_score, survivalTime));
    }
    
    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(Color.BlueViolet);

        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // 先绘制身体（从后往前，这样蛇头在最上层）
        for (int i = _snakeBody.Count - 1; i >= 0; i--)
        {
            _snakeBody[i].Draw(Core.SpriteBatch);
        }
        
        // 绘制蛇头
        _snakeHead.Draw(Core.SpriteBatch);
        
        // 绘制所有蝙蝠
        foreach (Bat bat in _bats)
        {
            bat.Sprite.Draw(Core.SpriteBatch, bat.Position);
        }
        
        // Draw the score
        Core.SpriteBatch.DrawString(
            _font,                  // spriteFont
            $"Score: {_score}", // text
            _scoreTextPosition,     // position
            Color.White,            // color
            0.0f,           // rotation
            _scoreTextOrigin,       // origin
            1.0f,              // scale
            SpriteEffects.None,     // effects
            0.0f          // layerDepth
        );
        
        // Draw the time
        TimeSpan elapsedTime = DateTime.Now - _gameStartTime;
        string timeText = $"Time: {elapsedTime.Minutes:00}:{elapsedTime.Seconds:00}";
        Core.SpriteBatch.DrawString(
            _timeFont,              // spriteFont
            timeText,               // text
            _timeTextPosition,      // position
            Color.White,            // color
            0.0f,                   // rotation
            _timeTextOrigin,        // origin
            1.0f,                   // scale
            SpriteEffects.None,     // effects
            0.0f                    // layerDepth
        );
        
        // 绘制体力值条
        DrawStaminaBar();
        
        Core.SpriteBatch.End();
        
        // Draw the Gum UI
        GumService.Default.Draw();
        
        base.Draw(gameTime);
    }
    
    /// <summary>
    /// 绘制体力值条
    /// </summary>
    private void DrawStaminaBar()
    {
        // 获取体力值百分比
        float staminaPercentage = _snakeHead.GetStaminaPercentage();
        
        // 计算体力值条的宽度
        int staminaBarWidth = (int)(500 * staminaPercentage);
        
        // 计算体力值条的颜色（从红色到绿色渐变）
        Color staminaColor = new Color(
            (byte)(255 * (1 - staminaPercentage)),  // 红色成分随体力增加而减少
            (byte)(255 * staminaPercentage),        // 绿色成分随体力增加而增加
            0
        );
        
        // 绘制体力值条背景（灰色）
        Core.SpriteBatch.Draw(
            _pixelTexture,
            new Rectangle((int)_staminaBarPosition.X, (int)_staminaBarPosition.Y, 500, 8),
            Color.DarkGray
        );
        
        // 绘制体力值条内容（颜色渐变）
        if (staminaBarWidth > 0)
        {
            Core.SpriteBatch.Draw(
                _pixelTexture,
                new Rectangle((int)_staminaBarPosition.X, (int)_staminaBarPosition.Y , staminaBarWidth, 8),
                staminaColor
            );
        }
        
        // 绘制体力值文字
        string staminaText = $"Stamina: {Math.Round(staminaPercentage * 100)}%";
        Vector2 textSize = _staminaFont.MeasureString(staminaText);
        Vector2 textOrigin = new Vector2(0, textSize.Y * 0.5f);
        
        Core.SpriteBatch.DrawString(
            _staminaFont,
            staminaText,
            _staminaTextPosition,
            Color.White,
            0.0f,
            textOrigin,
            1.0f,
            SpriteEffects.None,
            0.0f
        );
    }
}
