using System;

namespace DungeonSLime
{
    /// <summary>
    /// 分数记录类，用于存储排行榜数据
    /// </summary>
    public class ScoreEntry
    {
        /// <summary>
        /// 分数
        /// </summary>
        public int Score { get; set; }
        
        /// <summary>
        /// 存活时间
        /// </summary>
        public TimeSpan SurvivalTime { get; set; }
        
        /// <summary>
        /// 游戏结束时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="score">分数</param>
        /// <param name="survivalTime">存活时间</param>
        /// <param name="timestamp">时间戳</param>
        public ScoreEntry(int score, TimeSpan survivalTime, DateTime timestamp)
        {
            Score = score;
            SurvivalTime = survivalTime;
            Timestamp = timestamp;
        }
        
        /// <summary>
        /// 格式化存活时间为"X min Y sec"格式
        /// </summary>
        /// <returns>格式化后的字符串</returns>
        public string FormatSurvivalTime()
        {
            int minutes = SurvivalTime.Minutes;
            int seconds = SurvivalTime.Seconds;
            return $"{minutes} min {seconds} sec";
        }
    }
}