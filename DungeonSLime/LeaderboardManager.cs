using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DungeonSLime
{
    /// <summary>
    /// 排行榜管理器，负责排行榜数据的读取、写入、排序和插入
    /// </summary>
    public static class LeaderboardManager
    {
        /// <summary>
        /// 排行榜文件路径
        /// </summary>
        private static readonly string LeaderboardFilePath = Path.Combine("Content", "leaderboard.json");
        
        /// <summary>
        /// JSON序列化选项
        /// </summary>
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        
        /// <summary>
        /// 加载排行榜数据
        /// </summary>
        /// <returns>排序后的排行榜列表</returns>
        public static List<ScoreEntry> LoadLeaderboard()
        {
            try
            {
                if (File.Exists(LeaderboardFilePath))
                {
                    string json = File.ReadAllText(LeaderboardFilePath);
                    var entries = JsonSerializer.Deserialize<List<ScoreEntry>>(json, JsonOptions);
                    
                    if (entries != null)
                    {
                        return SortEntries(entries);
                    }
                }
            }
            catch (Exception ex)
            {
                // 如果读取失败，返回空列表
                Console.WriteLine($"加载排行榜失败: {ex.Message}");
            }
            
            return new List<ScoreEntry>();
        }
        
        /// <summary>
        /// 保存排行榜数据
        /// </summary>
        /// <param name="entries">排行榜列表</param>
        public static void SaveLeaderboard(List<ScoreEntry> entries)
        {
            try
            {
                // 确保目录存在
                string directory = Path.GetDirectoryName(LeaderboardFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                string json = JsonSerializer.Serialize(entries, JsonOptions);
                File.WriteAllText(LeaderboardFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存排行榜失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 添加新分数记录到排行榜
        /// </summary>
        /// <param name="score">分数</param>
        /// <param name="survivalTime">存活时间</param>
        /// <returns>是否成功添加到前三名</returns>
        public static bool AddEntry(int score, TimeSpan survivalTime)
        {
            var entries = LoadLeaderboard();
            
            // 创建新的分数记录
            var newEntry = new ScoreEntry(score, survivalTime, DateTime.Now);
            
            // 添加到列表
            entries.Add(newEntry);
            
            // 排序
            entries = SortEntries(entries);
            
            // 保存
            SaveLeaderboard(entries);
            
            // 检查是否进入前三名
            return entries.Take(3).Contains(newEntry);
        }
        
        /// <summary>
        /// 获取前N名排行榜记录
        /// </summary>
        /// <param name="count">获取的记录数量</param>
        /// <returns>前N名记录列表</returns>
        public static List<ScoreEntry> GetTopEntries(int count)
        {
            var entries = LoadLeaderboard();
            return entries.Take(count).ToList();
        }
        
        /// <summary>
        /// 对排行榜进行排序
        /// 排序规则：分数降序，分数相同时存活时间升序（时间短者排名靠前）
        /// </summary>
        /// <param name="entries">待排序的记录列表</param>
        /// <returns>排序后的记录列表</returns>
        private static List<ScoreEntry> SortEntries(List<ScoreEntry> entries)
        {
            return entries.OrderByDescending(entry => entry.Score)
                         .ThenBy(entry => entry.SurvivalTime)
                         .ToList();
        }
    }
}