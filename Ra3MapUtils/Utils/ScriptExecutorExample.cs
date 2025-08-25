using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Ra3MapUtils.Utils
{
    /// <summary>
    /// 脚本执行器使用示例
    /// </summary>
    public static class ScriptExecutorExample
    {
        /// <summary>
        /// 基本使用示例
        /// </summary>
        public static void BasicUsageExample()
        {
            Console.WriteLine("=== 基本使用示例 ===");
            
            // 准备脚本执行环境
            var references = new[] { 
                typeof(Console).Assembly,
                typeof(DateTime).Assembly
            };
            
            var usings = new[] { 
                "System", 
                "System.Threading"
            };
            
            // 创建脚本
            var script = @"
                Console.WriteLine(""Hello from script!"");
                var now = DateTime.Now;
                Console.WriteLine($""Current time: {now:yyyy-MM-dd HH:mm:ss}"");
                return ""Script completed successfully"";
            ";
            
            // 使用扩展方法执行脚本
            var executor = script.ExecuteWithControl(
                references: references,
                usings: usings,
                outputHandler: (output, isError) =>
                {
                    var prefix = isError ? "[错误] " : "[脚本] ";
                    Console.WriteLine($"{prefix}{output.TrimEnd()}");
                },
                statusHandler: (status) =>
                {
                    Console.WriteLine($"[状态] {status}");
                }
            );
            
            // 等待执行完成
            executor.WaitForCompletionAsync(TimeSpan.FromSeconds(10)).Wait();
            
            // 清理资源
            executor.Dispose();
            
            Console.WriteLine("=== 基本示例完成 ===");
        }
        
        /// <summary>
        /// 长时间运行脚本的中止示例
        /// </summary>
        public static void CancellationExample()
        {
            Console.WriteLine("=== 脚本中止示例 ===");
            
            var references = new[] { typeof(Console).Assembly };
            var usings = new[] { "System", "System.Threading" };
            
            // 创建一个会长时间运行的脚本
            var longRunningScript = @"
                Console.WriteLine(""开始长时间运行的任务..."");
                for (int i = 1; i <= 100; i++)
                {
                    Console.WriteLine($""处理进度: {i}%"");
                    Thread.Sleep(100); // 每100ms输出一次
                }
                Console.WriteLine(""任务完成!"");
                return ""Task completed"";
            ";
            
            var executor = longRunningScript.ExecuteWithControl(
                references: references,
                usings: usings,
                outputHandler: (output, isError) =>
                {
                    if (!string.IsNullOrWhiteSpace(output.Trim()))
                    {
                        Console.WriteLine($"[脚本] {output.TrimEnd()}");
                    }
                }
            );
            
            // 等待2秒后中止脚本
            Thread.Sleep(2000);
            Console.WriteLine("中止脚本执行...");
            executor.Cancel();
            
            // 等待中止完成
            executor.WaitForCompletionAsync(TimeSpan.FromSeconds(5)).Wait();
            Console.WriteLine($"脚本状态: {executor.CurrentStatus}");
            
            executor.Dispose();
            Console.WriteLine("=== 中止示例完成 ===");
        }
        
        /// <summary>
        /// 错误处理示例
        /// </summary>
        public static void ErrorHandlingExample()
        {
            Console.WriteLine("=== 错误处理示例 ===");
            
            var references = new[] { typeof(Console).Assembly };
            var usings = new[] { "System" };
            
            // 创建一个会产生错误的脚本
            var errorScript = @"
                Console.WriteLine(""开始执行有问题的脚本..."");
                var result = 10 / 0; // 除零错误
                return ""This won't execute"";
            ";
            
            var executor = errorScript.ExecuteWithControl(
                references: references,
                usings: usings,
                outputHandler: (output, isError) =>
                {
                    var prefix = isError ? "[错误] " : "[脚本] ";
                    Console.WriteLine($"{prefix}{output.TrimEnd()}");
                }
            );
            
            executor.WaitForCompletionAsync(TimeSpan.FromSeconds(5)).Wait();
            Console.WriteLine($"脚本状态: {executor.CurrentStatus}");
            
            executor.Dispose();
            Console.WriteLine("=== 错误处理示例完成 ===");
        }
        
        /// <summary>
        /// 手动创建执行器示例
        /// </summary>
        public static void ManualExecutorExample()
        {
            Console.WriteLine("=== 手动创建执行器示例 ===");
            
            var references = new[] { typeof(Console).Assembly };
            var usings = new[] { "System", "System.Collections.Generic" };
            
            var script = @"
                var items = new List<string>();
                for (int i = 1; i <= 5; i++)
                {
                    var item = $""Item {i}"";
                    items.Add(item);
                    Console.WriteLine(item);
                }
                return string.Join("", "", items);
            ";
            
            // 手动创建执行器
            var executor = new InteractiveScriptExecutor();
            
            // 订阅事件
            executor.OutputReceived += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Output.Trim()))
                {
                    var prefix = e.IsError ? "[错误] " : "[脚本] ";
                    Console.WriteLine($"{prefix}{e.Output.TrimEnd()}");
                }
            };
            
            executor.StatusChanged += (sender, e) =>
            {
                Console.WriteLine($"[状态变化] {e}");
            };
            
            // 执行脚本
            var result = executor.ExecuteScriptAsync(script, null, references, usings).Result;
            
            Console.WriteLine($"执行结果: {result.Status}");
            Console.WriteLine($"返回值: {result.ReturnValue}");
            Console.WriteLine($"执行时间: {result.ExecutionTime.TotalMilliseconds:F2}ms");
            
            executor.Dispose();
            Console.WriteLine("=== 手动创建执行器示例完成 ===");
        }
        
        /// <summary>
        /// 运行所有示例
        /// </summary>
        public static void RunAllExamples()
        {
            try
            {
                BasicUsageExample();
                Console.WriteLine();
                
                CancellationExample();
                Console.WriteLine();
                
                ErrorHandlingExample();
                Console.WriteLine();
                
                ManualExecutorExample();
                Console.WriteLine();
                
                Console.WriteLine("所有示例执行完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"执行示例时发生错误: {ex.Message}");
                Console.WriteLine($"错误详情: {ex}");
            }
        }
    }
}
