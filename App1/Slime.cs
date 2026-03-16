using System;
using System.Collections.Generic;
using ClassLibrary;
using Microsoft.Xna.Framework;
using ClassLibrary.Graphic;

namespace App1;

public enum E_SlimeType
{
    Head,
    Body
}

public class Slime
{
    public E_SlimeType Type { get; set; }
    
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    
    /// <summary>
    /// 最后的移动方向（用于持续移动）
    /// </summary>
    public Vector2 LastDirection { get; set; }
    
    public AnimatedSprite Sprite { get; set; }
    
    // 体力值相关字段
    private float _stamina;           // 当前体力值
    private float _maxStamina = 100f; // 最大体力值
    private float _staminaRegenRate = 15f; // 体力恢复速度
    private float _staminaDrainRate = 20f; // 冲刺时体力消耗速度
    
    // 冲刺状态
    private bool _isSprinting;        // 是否正在冲刺
    private bool _tired;    // 是否曾经耗尽过体力
    
    /// <summary>
    /// 随机颜色数组
    /// </summary>
    private static readonly Color[] RandomColors = {
        Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Purple,
        Color.Orange, Color.Pink, Color.Cyan, Color.Magenta, Color.Lime
    };
    
    /// <summary>
    /// 历史位置队列，用于身体节点跟随
    /// </summary>
    private Queue<Vector2> _positionHistory;
    
    /// <summary>
    /// 历史队列的最大长度
    /// </summary>
    private int _maxHistoryLength;
    
    public Slime(E_SlimeType type, AnimatedSprite sprite, Vector2 initialPosition)
    {
        Type = type;
        Sprite = sprite;
        Position = initialPosition;
        Velocity = Vector2.Zero;
        LastDirection = Vector2.UnitX; // 默认向右移动
        
        // 初始化历史位置队列
        _positionHistory = new Queue<Vector2>();
        _maxHistoryLength = 1000; // 默认最大历史长度
    }
    
    /// <summary>
    /// 记录当前位置到历史队列（仅蛇头使用）
    /// </summary>
    public void RecordPosition()
    {
        if (Type == E_SlimeType.Head)
        {
            _positionHistory.Enqueue(Position);
            
            // 限制队列长度，防止内存无限增长
            while (_positionHistory.Count > _maxHistoryLength)
            {
                _positionHistory.Dequeue();
            }
        }
    }
    
    /// <summary>
    /// 获取历史位置队列的副本（用于身体节点访问）
    /// </summary>
    public Queue<Vector2> GetPositionHistory()
    {
        return new Queue<Vector2>(_positionHistory);
    }
    
    /// <summary>
    /// 获取指定帧数之前的位置
    /// </summary>
    /// <param name="framesAgo">多少帧之前</param>
    /// <returns>历史位置，如果不存在则返回当前位置</returns>
    public Vector2 GetPositionAtFrame(int framesAgo)
    {
        if (_positionHistory.Count <= framesAgo)
        {
            // 如果历史记录不够，返回最早的位置
            return _positionHistory.Count > 0 
                ? _positionHistory.ToArray()[0] 
                : Position;
        }
        
        // 获取指定帧数之前的位置
        var historyArray = _positionHistory.ToArray();
        int index = _positionHistory.Count - 1 - framesAgo;
        return historyArray[index];
    }
    
    /// <summary>
    /// 检查与身体的碰撞
    /// </summary>
    /// <param name="bodySegments">身体节点列表</param>
    /// <returns>是否发生碰撞</returns>
    public bool CheckBodyCollision(List<Slime> bodySegments)
    {
        // 蛇头的碰撞体积（比实际移动范围稍小）
        Circle headBounds = new Circle(
            (int)(Position.X + (Sprite.Width * 0.5f)),
            (int)(Position.Y + (Sprite.Height * 0.5f)),
            (int)(Sprite.Width * 0.25f) // 比移动范围稍小
        );
        
        // 检查与每个身体节点的碰撞
        foreach (var bodySegment in bodySegments)
        {
            // 身体节点的碰撞体积
            Circle bodyBounds = new Circle(
                (int)(bodySegment.Position.X + (bodySegment.Sprite.Width * 0.5f)),
                (int)(bodySegment.Position.Y + (bodySegment.Sprite.Height * 0.5f)),
                (int)(bodySegment.Sprite.Width * 0.01f)
            );
            
            // 如果发生碰撞
            if (headBounds.Intersects(bodyBounds))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 更新蛇头位置（基于速度）
    /// </summary>
    public void UpdateHead(GameTime gameTime)
    {
        if (Type == E_SlimeType.Head)
        {
            Position += Velocity;
            RecordPosition();
        }
    }
    
    /// <summary>
    /// 更新动画
    /// </summary>
    public void Update(GameTime gameTime)
    {
        Sprite?.Update(gameTime);
    }
    
    /// <summary>
    /// 绘制Slime
    /// </summary>
    public void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
    {
        Sprite?.Draw(spriteBatch, Position);
    }
    
    /// <summary>
    /// 设置随机颜色
    /// </summary>
    public void SetRandomColor()
    {
        if (Sprite != null)
        {
            Sprite.Color = RandomColors[Random.Shared.Next(RandomColors.Length)];
        }
    }
    
    /// <summary>
    /// 检查slime是否完全移出屏幕
    /// </summary>
    /// <param name="screenBounds">屏幕边界</param>
    /// <returns>是否完全移出屏幕</returns>
    public bool IsOffScreen(Rectangle screenBounds)
    {
        if (Sprite == null) return false;
        
        // 计算slime的边界
        Rectangle slimeBounds = new Rectangle(
            (int)Position.X,
            (int)Position.Y,
            (int)Sprite.Width,
            (int)Sprite.Height
        );
        
        // 检查是否完全移出屏幕
        return slimeBounds.Right < screenBounds.Left ||
               slimeBounds.Left > screenBounds.Right ||
               slimeBounds.Bottom < screenBounds.Top ||
               slimeBounds.Top > screenBounds.Bottom;
    }
    
    /// <summary>
    /// 更新体力值
    /// </summary>
    /// <param name="gameTime">游戏时间</param>
    public void UpdateStamina(GameTime gameTime)
    {
        if (Type == E_SlimeType.Head)
        {
            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // 只有在冲刺状态下才消耗体力
            if (_isSprinting && _stamina > 0)
            {
                _stamina -= _staminaDrainRate * elapsedSeconds;
                if (_stamina < 0) _stamina = 0;
                
                // 如果体力耗尽到0，标记为耗尽
                if (_stamina == 0)
                {
                    _tired = true;
                }
                
                if (_tired && _stamina >= 19)
                {
                    // 如果曾经耗尽过体力，现在恢复到20以上，重置耗尽状态
                    _tired = false;
                }
            }
            else
            {
                // 非冲刺状态，恢复体力
                _stamina += _staminaRegenRate * elapsedSeconds;
                if (_stamina > _maxStamina) _stamina = _maxStamina;
            }
        }
    }
    
    /// <summary>
    /// 检查是否可以冲刺
    /// </summary>
    /// <returns>是否可以冲刺</returns>
    public bool CanSprint()
    {
        if (Type != E_SlimeType.Head)
            return false;
            
        // 如果未耗尽体力，可以冲刺到0
        if (!_tired)
        {
            return _stamina > 0;
        }
        else
        {
            return _stamina > 20;
        }
    }
    
    /// <summary>
    /// 获取体力值百分比
    /// </summary>
    /// <returns>体力值百分比（0.0到1.0）</returns>
    public float GetStaminaPercentage()
    {
        return Type == E_SlimeType.Head ? _stamina / _maxStamina : 0f;
    }
    
    /// <summary>
    /// 获取当前体力值
    /// </summary>
    /// <returns>当前体力值</returns>
    public float GetStamina()
    {
        return Type == E_SlimeType.Head ? _stamina : 0f;
    }
    
    /// <summary>
    /// 设置冲刺状态
    /// </summary>
    /// <param name="isSprinting">是否正在冲刺</param>
    public void SetSprinting(bool isSprinting)
    {
        if (Type == E_SlimeType.Head)
        {
            _isSprinting = isSprinting;
        }
    }
    
    /// <summary>
    /// 获取冲刺状态
    /// </summary>
    /// <returns>是否正在冲刺</returns>
    public bool IsSprinting()
    {
        return Type == E_SlimeType.Head && _isSprinting;
    }
}
