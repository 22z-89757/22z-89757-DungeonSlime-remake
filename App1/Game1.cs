using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ClassLibrary;
using ClassLibrary.Graphic;
using ClassLibrary.Input;

namespace App1;

public class Game1 : Core
{
    // 蛇头
    private Slime _snakeHead;
    
    // 蛇身体节点列表
    private List<Slime> _snakeBody;
    
    // 蝙蝠（食物）
    private AnimatedSprite _bat;
    
    // 蝙蝠位置
    private Vector2 _batPosition;
    
    // 蝙蝠速度
    private Vector2 _batVelocity;
    
    // 移动速度
    private const float MOVEMENT_SPEED = 3.0f;
    
    // 身体节点之间的距离（以帧数计算）
    private const int BODY_DISTANCE_IN_FRAMES = 15;
    
    public Game1() : base("贪吃蛇大作战", 1280, 720, false)
    {
        _snakeBody = new List<Slime>();
    }

    protected override void Initialize()
    {
        base.Initialize();
        
        // 设置蝙蝠初始位置
        _batPosition = new Vector2(_snakeHead.Sprite.Width + 10, 0);
        
        // 分配随机速度给蝙蝠
        AssignRandomBatVelocity();
    }

    protected override void LoadContent()
    {
        // 创建纹理图集
        TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/File.xml");

        // 创建蛇头
        AnimatedSprite headSprite = atlas.CreateAnimatedSprite("slime-animation");
        headSprite.Scale = new Vector2(4.0f, 4.0f);
        headSprite.Color = Color.Orange;
        
        _snakeHead = new Slime(
            E_SlimeType.Head, 
            headSprite, 
            new Vector2(640, 360) // 屏幕中心
        );
        
        // 初始化时添加3个身体节点
        for (int i = 0; i < 3; i++)
        {
            AddBodySegment(atlas);
        }
        
        // 创建蝙蝠动画精灵
        _bat = atlas.CreateAnimatedSprite("bat-animation");
        _bat.Scale = new Vector2(4.0f, 4.0f);
        
        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // 处理输入
        HandleInput();
        
        // 更新蛇头位置
        _snakeHead.UpdateHead(gameTime);
        
        // 更新蛇身体节点位置（基于历史路径）
        UpdateSnakeBody();
        
        // 更新所有动画
        _snakeHead.Update(gameTime);
        foreach (var bodySegment in _snakeBody)
        {
            bodySegment.Update(gameTime);
        }
        _bat.Update(gameTime);
        
        // 边界检测
        HandleScreenBounds();
        
        // 更新蝙蝠
        UpdateBat();
        
        // 碰撞检测
        CheckCollisions();
        
        base.Update(gameTime);
    }
    
    /// <summary>
    /// 处理输入，更新蛇头速度
    /// </summary>
    private void HandleInput()
    {
        Vector2 direction = Vector2.Zero;
        float speed = MOVEMENT_SPEED;
        bool hasInput = false;
        
        // 键盘输入
        if (InputMgr.Keyboard.IsKeyDown(Keys.Space))
        {
            speed *= 1.5f;
        }
        
        if (InputMgr.Keyboard.IsKeyDown(Keys.W) || InputMgr.Keyboard.IsKeyDown(Keys.Up))
        {
            direction.Y -= 1;
            hasInput = true;
        }
        if (InputMgr.Keyboard.IsKeyDown(Keys.S) || InputMgr.Keyboard.IsKeyDown(Keys.Down))
        {
            direction.Y += 1;
            hasInput = true;
        }
        if (InputMgr.Keyboard.IsKeyDown(Keys.A) || InputMgr.Keyboard.IsKeyDown(Keys.Left))
        {
            direction.X -= 1;
            hasInput = true;
        }
        if (InputMgr.Keyboard.IsKeyDown(Keys.D) || InputMgr.Keyboard.IsKeyDown(Keys.Right))
        {
            direction.X += 1;
            hasInput = true;
        }
        
        // 手柄输入
        GamePadInfo gamePadOne = InputMgr.GamePads[(int)PlayerIndex.One];
        if (gamePadOne.IsButtonDown(Buttons.A))
        {
            speed *= 1.5f;
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
        
        // 更新方向和速度
        if (hasInput && direction != Vector2.Zero)
        {
            // 有输入：归一化方向并更新LastDirection
            direction.Normalize();
            _snakeHead.LastDirection = direction;
        }
        
        // 始终使用LastDirection移动（持续移动）
        _snakeHead.Velocity = _snakeHead.LastDirection * speed;
    }
    
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
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight
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
    /// 更新蝙蝠位置
    /// </summary>
    private void UpdateBat()
    {
        Rectangle screenBounds = new Rectangle(
            0, 0,
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight
        );
        
        Vector2 newBatPosition = _batPosition + _batVelocity;
        
        Circle batBounds = new Circle(
            (int)(newBatPosition.X + (_bat.Width * 0.5f)),
            (int)(newBatPosition.Y + (_bat.Height * 0.5f)),
            (int)(_bat.Width * 0.5f)
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
            newBatPosition.X = screenBounds.Right - _bat.Width;
        }
        
        if (batBounds.Top < screenBounds.Top)
        {
            normal.Y = Vector2.UnitY.Y;
            newBatPosition.Y = screenBounds.Top;
        }
        else if (batBounds.Bottom > screenBounds.Bottom)
        {
            normal.Y = -Vector2.UnitY.Y;
            newBatPosition.Y = screenBounds.Bottom - _bat.Height;
        }
        
        if (normal != Vector2.Zero)
        {
            normal.Normalize();
            _batVelocity = Vector2.Reflect(_batVelocity, normal);
        }
        
        _batPosition = newBatPosition;
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
        
        Circle batBounds = new Circle(
            (int)(_batPosition.X + (_bat.Width * 0.5f)),
            (int)(_batPosition.Y + (_bat.Height * 0.5f)),
            (int)(_bat.Width * 0.5f)
        );
        
        // 蛇头吃到蝙蝠
        if (headBounds.Intersects(batBounds))
        {
            // 添加一个身体节点
            TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/File.xml");
            AddBodySegment(atlas);
            
            // 重新定位蝙蝠
            int totalColumns = GraphicsDevice.PresentationParameters.BackBufferWidth / (int)_bat.Width;
            int totalRows = GraphicsDevice.PresentationParameters.BackBufferHeight / (int)_bat.Height;
            
            int column = Random.Shared.Next(0, totalColumns);
            int row = Random.Shared.Next(0, totalRows);
            
            _batPosition = new Vector2(column * _bat.Width, row * _bat.Height);
            
            // 分配新的随机速度
            AssignRandomBatVelocity();
        }
    }
    
    /// <summary>
    /// 为蝙蝠分配随机速度
    /// </summary>
    private void AssignRandomBatVelocity()
    {
        float angle = (float)(Random.Shared.NextDouble() * Math.PI * 2);
        float x = (float)Math.Cos(angle);
        float y = (float)Math.Sin(angle);
        Vector2 direction = new Vector2(x, y);
        _batVelocity = direction * MOVEMENT_SPEED;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // 先绘制身体（从后往前，这样蛇头在最上层）
        for (int i = _snakeBody.Count - 1; i >= 0; i--)
        {
            _snakeBody[i].Draw(SpriteBatch);
        }
        
        // 绘制蛇头
        _snakeHead.Draw(SpriteBatch);
        
        // 绘制蝙蝠
        _bat.Draw(SpriteBatch, _batPosition);

        SpriteBatch.End();
        
        base.Draw(gameTime);
    }
}
