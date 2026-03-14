using System.Collections.Generic;
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
}
