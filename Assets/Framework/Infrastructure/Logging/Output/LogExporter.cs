using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Logging.Core;
using UnityEngine;

namespace BH.Framework.Infrastructure.Logging.Output
{
    /// <summary>
    /// HTML 日志导出器
    /// </summary>
    public static class LogExporter
    {
        /// <summary>
        /// 导出为HTML文件
        /// </summary>
        /// <param name="logs">日志列表</param>
        /// <param name="filePath">保存路径（null时自动生成）</param>
        /// <returns>文件完整路径</returns>
        public static string ExportToHtml(IEnumerable<LogEntry> logs, string filePath = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                string dir = Path.Combine(Application.persistentDataPath, "Logs");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                filePath = Path.Combine(dir, $"BattleLog_{DateTime.Now:yyyyMMdd_HHmmss}.html");
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='utf-8'>");
            sb.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            sb.AppendLine("<title>崩坏星穹铁道 · 战斗日志</title>");
            sb.AppendLine("<style>");
            sb.AppendLine(@"* { margin: 0; padding: 0; box-sizing: border-box; }
body { font-family: 'Segoe UI', 'Microsoft YaHei', monospace; background: #0a0c14; color: #e6e9f0; padding: 20px; }
.container { max-width: 1400px; margin: 0 auto; }
.header { background: #1a1e2c; padding: 20px; border-radius: 8px; margin-bottom: 20px; border-left: 6px solid #ffd700; }
.header h1 { color: #ffd700; font-size: 24px; margin-bottom: 8px; display: flex; align-items: center; gap: 10px; }
.header .meta { color: #a0b0c0; font-size: 14px; }
.stats { display: flex; gap: 20px; margin-top: 10px; }
.stat-item { background: #252a3a; padding: 10px 16px; border-radius: 20px; font-size: 13px; }
.filter-bar { margin-bottom: 20px; display: flex; gap: 10px; flex-wrap: wrap; }
.filter-btn { background: #2a3040; border: none; color: white; padding: 8px 16px; border-radius: 4px; cursor: pointer; }
.filter-btn.active { background: #3a6ea5; }
.log-table { width: 100%; border-collapse: collapse; background: #151a24; border-radius: 8px; overflow: hidden; }
.log-table th { background: #1f2533; padding: 12px; text-align: left; font-weight: 600; color: #c8d3e5; }
.log-table td { padding: 10px 12px; border-bottom: 1px solid #2a3140; font-size: 14px; }
.log-table tr:hover { background: #1e2432; }
.timestamp { color: #6ab0e6; font-family: monospace; }
.level-Debug { color: #aaa; }
.level-Info { color: #fff; }
.level-Warning { color: #ffb86b; }
.level-Error { color: #ff6b6b; }
.level-Critical { color: #ff4444; font-weight: bold; background: rgba(255, 68, 68, 0.1); }
.category-badge { background: #3a4055; padding: 2px 8px; border-radius: 12px; font-size: 12px; }
.stacktrace { color: #aaa; font-size: 12px; margin-top: 4px; white-space: pre-wrap; }
.footer { margin-top: 30px; text-align: center; color: #8899aa; font-size: 12px; }
</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");
            sb.AppendLine("<div class='header'>");
            sb.AppendLine("<h1>🎮 崩坏星穹铁道 · 战斗日志</h1>");
            sb.AppendLine(
                $"<div class='meta'>生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss} | 日志条数: {CountLogs(logs)}</div>");
            sb.AppendLine("<div class='stats'>");
            sb.AppendLine($"<span class='stat-item'>📊 Debug: {CountByLevel(logs, LogLevel.Debug)}</span>");
            sb.AppendLine($"<span class='stat-item'>ℹ️ Info: {CountByLevel(logs, LogLevel.Info)}</span>");
            sb.AppendLine($"<span class='stat-item'>⚠️ Warning: {CountByLevel(logs, LogLevel.Warning)}</span>");
            sb.AppendLine($"<span class='stat-item'>❌ Error: {CountByLevel(logs, LogLevel.Error)}</span>");
            sb.AppendLine($"<span class='stat-item'>🔥 Critical: {CountByLevel(logs, LogLevel.Critical)}</span>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            // 过滤按钮（静态示例，实际可通过JS实现动态过滤）
            sb.AppendLine("<div class='filter-bar'>");
            sb.AppendLine("<button class='filter-btn active' data-level='all'>全部</button>");
            sb.AppendLine("<button class='filter-btn' data-level='Debug'>Debug</button>");
            sb.AppendLine("<button class='filter-btn' data-level='Info'>Info</button>");
            sb.AppendLine("<button class='filter-btn' data-level='Warning'>Warning</button>");
            sb.AppendLine("<button class='filter-btn' data-level='Error'>Error</button>");
            sb.AppendLine("<button class='filter-btn' data-level='Critical'>Critical</button>");
            sb.AppendLine("</div>");

            sb.AppendLine("<table class='log-table'>");
            sb.AppendLine("<thead><tr><th>时间</th><th>级别</th><th>分类</th><th>消息</th><th>堆栈</th></tr></thead>");
            sb.AppendLine("<tbody>");

            foreach (var entry in logs)
            {
                string timeStr = entry.Timestamp.ToString("HH:mm:ss.fff");
                string levelClass = $"level-{entry.Level}";
                string stackHtml = string.IsNullOrEmpty(entry.StackTrace)
                    ? "-"
                    : $"<div class='stacktrace'>{EscapeHtml(entry.StackTrace)}</div>";

                sb.AppendLine($"<tr class='{levelClass}'>");
                sb.AppendLine($"<td class='timestamp'>{timeStr}</td>");
                sb.AppendLine($"<td><span class='{levelClass}'>{entry.Level}</span></td>");
                sb.AppendLine($"<td><span class='category-badge'>{entry.Type}</span></td>");
                sb.AppendLine($"<td>{EscapeHtml(entry.Message)}</td>");
                sb.AppendLine($"<td>{stackHtml}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("<div class='footer'>Powered by BattleSystem.Logging · 此文件为自动生成，包含完整战斗回放信息</div>");
            sb.AppendLine("</div>");

            // 添加简单的JavaScript过滤功能
            sb.AppendLine(@"
<script>
document.querySelectorAll('.filter-btn').forEach(btn => {
    btn.addEventListener('click', function() {
        let level = this.dataset.level;
        document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
        this.classList.add('active');
        
        let rows = document.querySelectorAll('.log-table tbody tr');
        rows.forEach(row => {
            if (level === 'all') {
                row.style.display = '';
            } else {
                let levelCell = row.querySelector('td:nth-child(2) span');
                if (levelCell && levelCell.textContent === level) {
                    row.style.display = '';
                } else {
                    row.style.display = 'none';
                }
            }
        });
    });
});
</script>");

            sb.AppendLine("</body></html>");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            Debug.Log($"日志已导出至: {filePath}");
            return filePath;
        }

        /// <summary>
        /// 计算日志条目集合的总数
        /// </summary>
        /// <param name="logs">日志条目枚举集合</param>
        /// <returns>日志条目的总数</returns>
        /// <remarks>
        /// 优化点：如果集合实现了<see cref="ICollection{T}"/>接口，直接返回Count属性以提升性能；
        /// 否则遍历枚举集合统计总数。
        /// </remarks>
        private static int CountLogs(IEnumerable<LogEntry> logs)
        {
            if (logs is ICollection<LogEntry> col) return col.Count;
            int c = 0;
            foreach (var _ in logs) c++;
            return c;
        }

        /// <summary>
        /// 统计指定日志级别的日志条目数量
        /// </summary>
        /// <param name="logs">日志条目枚举集合</param>
        /// <param name="level">要统计的日志级别</param>
        /// <returns>指定日志级别的日志条目数量</returns>
        private static int CountByLevel(IEnumerable<LogEntry> logs, LogLevel level)
        {
            int c = 0;
            foreach (var log in logs)
                if (log.Level == level)
                    c++;
            return c;
        }

        /// <summary>
        /// 对字符串进行HTML转义处理
        /// </summary>
        /// <param name="text">需要转义的原始字符串</param>
        /// <returns>转义后的字符串；如果输入为null或空字符串，直接返回原值</returns>
        /// <remarks>
        /// 内部使用<see cref="System.Security.SecurityElement.Escape(string)"/>实现，
        /// </remarks>
        private static string EscapeHtml(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return System.Security.SecurityElement.Escape(text);
        }
    }
}