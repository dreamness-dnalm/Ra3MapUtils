using System.IO;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using System.Dynamic;
using System.Text;
using Dreamness.Ra3.Map.Facade.Core;
using Dreamness.Ra3.Map.Facade.enums;
using Dreamness.Ra3.Map.Facade.Util;
using Dreamness.Ra3.Map.Parser.Asset.Impl.GameObject;
using Dreamness.Ra3.Map.Parser.Asset.Impl.Player;
using Dreamness.Ra3.Map.Parser.Asset.Impl.Team;
using Dreamness.Ra3.Map.Parser.Util;
using System.Threading;

namespace Ra3MapUtils.Utils;

public class CSharpScriptUtil
{
    /// <summary>
    /// 执行C#脚本字符串
    /// </summary>
    /// <param name="scriptCode">要执行的C#脚本代码</param>
    /// <param name="globals">全局变量对象，脚本可以访问这些变量</param>
    /// <param name="references">需要引用的程序集</param>
    /// <param name="usings">需要添加的using语句</param>
    /// <returns>脚本执行结果</returns>
    public static async Task<object> ExecuteScriptAsync(
        string scriptCode, 
        object globals = null, 
        IEnumerable<Assembly> references = null, 
        IEnumerable<string> usings = null)
    {
        try
        {
            // 创建脚本选项
            var scriptOptions = ScriptOptions.Default;
            
            // 添加引用
            if (references != null)
            {
                scriptOptions = scriptOptions.AddReferences(references);
            }
            
            // 添加using语句
            if (usings != null)
            {
                scriptOptions = scriptOptions.AddImports(usings);
            }
            
            // 执行脚本
            var result = await CSharpScript.EvaluateAsync(scriptCode, scriptOptions, globals);
            return result;
        }
        catch (CompilationErrorException ex)
        {
            throw new Exception($"脚本编译错误: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"脚本执行错误: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// 同步执行C#脚本字符串
    /// </summary>
    /// <param name="scriptCode">要执行的C#脚本代码</param>
    /// <param name="globals">全局变量对象，脚本可以访问这些变量</param>
    /// <param name="references">需要引用的程序集</param>
    /// <param name="usings">需要添加的using语句</param>
    /// <returns>脚本执行结果</returns>
    public static object ExecuteScript(
        string scriptCode, 
        object globals = null, 
        IEnumerable<Assembly> references = null, 
        IEnumerable<string> usings = null)
    {
        try
        {
            // 创建脚本选项
            var scriptOptions = ScriptOptions.Default;
            
            // 添加引用
            if (references != null)
            {
                scriptOptions = scriptOptions.AddReferences(references);
            }
            
            // 添加using语句
            if (usings != null)
            {
                scriptOptions = scriptOptions.AddImports(usings);
            }
            
            // 同步执行脚本
            var result = CSharpScript.EvaluateAsync(scriptCode, scriptOptions, globals).Result;
            return result;
        }
        catch (CompilationErrorException ex)
        {
            throw new Exception($"脚本编译错误: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"脚本执行错误: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// 执行C#脚本字符串并返回指定类型的结果
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="scriptCode">要执行的C#脚本代码</param>
    /// <param name="globals">全局变量对象，脚本可以访问这些变量</param>
    /// <param name="references">需要引用的程序集</param>
    /// <param name="usings">需要添加的using语句</param>
    /// <returns>指定类型的脚本执行结果</returns>
    public static async Task<T> ExecuteScriptAsync<T>(
        string scriptCode, 
        object globals = null, 
        IEnumerable<Assembly> references = null, 
        IEnumerable<string> usings = null)
    {
        try
        {
            // 创建脚本选项
            var scriptOptions = ScriptOptions.Default;
            
            // 添加引用
            if (references != null)
            {
                scriptOptions = scriptOptions.AddReferences(references);
            }
            
            // 添加using语句
            if (usings != null)
            {
                scriptOptions = scriptOptions.AddImports(usings);
            }
            
            // 执行脚本并返回指定类型
            var result = await CSharpScript.EvaluateAsync<T>(scriptCode, scriptOptions, globals);
            return result;
        }
        catch (CompilationErrorException ex)
        {
            throw new Exception($"脚本编译错误: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"脚本执行错误: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// 同步执行C#脚本字符串并返回指定类型的结果
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="scriptCode">要执行的C#脚本代码</param>
    /// <param name="globals">全局变量对象，脚本可以访问这些变量</param>
    /// <param name="references">需要引用的程序集</param>
    /// <param name="usings">需要添加的using语句</param>
    /// <returns>指定类型的脚本执行结果</returns>
    public static T ExecuteScript<T>(
        string scriptCode, 
        object globals = null, 
        IEnumerable<Assembly> references = null, 
        IEnumerable<string> usings = null)
    {
        try
        {
            // 创建脚本选项
            var scriptOptions = ScriptOptions.Default;
            
            // 添加引用
            if (references != null)
            {
                scriptOptions = scriptOptions.AddReferences(references);
            }
            
            // 添加using语句
            if (usings != null)
            {
                scriptOptions = scriptOptions.AddImports(usings);
            }
            
            // 同步执行脚本并返回指定类型
            var result = CSharpScript.EvaluateAsync<T>(scriptCode, scriptOptions, globals).Result;
            return result;
        }
        catch (CompilationErrorException ex)
        {
            throw new Exception($"脚本编译错误: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"脚本执行错误: {ex.Message}", ex);
        }
    }

    public static void demo()
    {
        try
        {
            Console.WriteLine("=== C#脚本执行演示 ===");
            
            // 准备脚本执行所需的引用和using语句
            var references = new[] { 
                typeof(Console).Assembly,           // System.Console
                typeof(DateTime).Assembly,          // System.DateTime
                typeof(System.Linq.Enumerable).Assembly  // System.Linq
            };
            
            var usings = new[] { 
                "System", 
                "System.Linq", 
                "System.Collections.Generic",
                "System.Text"
            };
            
            // 1. 基本数学运算
            Console.WriteLine("\n1. 基本数学运算:");
            var mathResult = CSharpScriptUtil.ExecuteScript("2 + 3 * 4", null, references, usings);
            Console.WriteLine($"2 + 3 * 4 = {mathResult}");
            
            // 2. 字符串操作
            Console.WriteLine("\n2. 字符串操作:");
            var stringResult = CSharpScriptUtil.ExecuteScript("\"Hello, \" + \"World!\"", null, references, usings);
            Console.WriteLine($"字符串拼接结果: {stringResult}");
            
            // 3. 日期时间操作
            Console.WriteLine("\n3. 日期时间操作:");
            var dateResult = CSharpScriptUtil.ExecuteScript("DateTime.Now.ToString(\"yyyy-MM-dd HH:mm:ss\")", null, references, usings);
            Console.WriteLine($"当前时间: {dateResult}");
            
            
            // 5. 指定返回类型的脚本
            Console.WriteLine("\n5. 指定返回类型的脚本:");
            var intResult = CSharpScriptUtil.ExecuteScript<int>("10 * 5", null, references, usings);
            Console.WriteLine($"整数结果: {intResult} (类型: {intResult.GetType().Name})");
            
            // 6. 复杂计算
            Console.WriteLine("\n6. 复杂计算:");
            var complexResult = CSharpScriptUtil.ExecuteScript(@"
                var numbers = new[] { 1, 2, 3, 4, 5 };
                var sum = numbers.Sum();
                var average = numbers.Average();
                return $""总和: {sum}, 平均值: {average:F2}"";
            ", null, references, usings);
            Console.WriteLine($"复杂计算结果: {complexResult}");
            
            // 7. 条件判断
            Console.WriteLine("\n7. 条件判断:");
            var conditionResult = CSharpScriptUtil.ExecuteScript(@"
                var hour = DateTime.Now.Hour;
                if (hour < 12) return ""上午"";
                else if (hour < 18) return ""下午"";
                else return ""晚上"";
            ", null, references, usings);
            Console.WriteLine($"当前时段: {conditionResult}");
            
            // 8. 循环操作
            Console.WriteLine("\n8. 循环操作:");
            var loopResult = CSharpScriptUtil.ExecuteScript(@"
                var result = """";
                for (int i = 1; i <= 5; i++)
                {
                    result += i + "" "";
                }
                return result.Trim();
            ", null, references, usings);
            Console.WriteLine($"循环结果: {loopResult}");
            
            // 9. RA3地图相关操作演示
            Console.WriteLine("\n9. RA3地图相关操作演示:");
            try
            {
                var ra3References = new[] { 
                    typeof(Console).Assembly,
                    typeof(DateTime).Assembly,
                    typeof(System.Linq.Enumerable).Assembly,
                    typeof(Dreamness.Ra3.Map.Facade.Core.Ra3MapFacade).Assembly
                };
                
                var ra3Usings = new[] { 
                    "System", 
                    "System.Linq", 
                    "System.Collections.Generic",
                    "System.Text",
                    "Dreamness.Ra3.Map.Facade.Core",
                    "Dreamness.Ra3.Map.Facade.Util"
                };
                // var ra3map = Ra3MapFacade.NewMap(100, 100, 0);
                // ra3map.SaveAs(Ra3PathUtil.RA3MapFolder, "r_test");
                var ra3Result = CSharpScriptUtil.ExecuteScript(@"
                var ra3map = Ra3MapFacade.NewMap(100, 100, 0);
                ra3map.SaveAs(Ra3PathUtil.RA3MapFolder, ""r_test"");
                    
                ", null, ra3References, ra3Usings);
                Console.WriteLine($"RA3地图信息: {ra3Result}");
            }
            catch (Exception ra3Ex)
            {
                Console.WriteLine($"RA3地图操作演示失败: {ra3Ex.Message}");
            }
            
            Console.WriteLine("\n=== 演示完成 ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"演示过程中发生错误: {ex.Message}");
            Console.WriteLine($"错误详情: {ex}");
        }
    }
    
    /// <summary>
    /// 演示交互式脚本执行器的使用
    /// </summary>
    public static void DemoInteractiveExecutorAsync()
    {
        try
        {
            Console.WriteLine("=== 交互式脚本执行器演示 ===");
            
            // 准备脚本执行所需的引用和using语句
            var references = new[] { 
                typeof(Console).Assembly,
                typeof(DateTime).Assembly,
                typeof(System.Linq.Enumerable).Assembly
            };
            
            var usings = new[] { 
                "System", 
                "System.Linq", 
                "System.Collections.Generic",
                "System.Threading"
            };
            
            // 创建输出收集器
            var outputCollector = new List<string>();
            var errorCollector = new List<string>();
            
            // 1. 基本使用方式 - 带输出回调
            Console.WriteLine("\n1. 基本使用方式 - 带输出回调:");
            var script1 = @"
                Console.WriteLine(""开始执行脚本..."");
                for (int i = 1; i <= 5; i++)
                {
                    Console.WriteLine($""处理第 {i} 项..."");
                    Thread.Sleep(50); // 模拟耗时操作
                }
                Console.WriteLine(""脚本执行完成!"");
                return ""处理完成"";
            ";
            
            var executor1 = script1.ExecuteWithControl(
                references: references,
                usings: usings,
                outputHandler: (output, isError) =>
                {
                    if (isError)
                        errorCollector.Add(output.TrimEnd());
                    else
                        outputCollector.Add(output.TrimEnd());
                },
                statusHandler: (status) =>
                {
                    Console.WriteLine($"[状态] 脚本状态: {status}");
                }
            );
            
            Thread.Sleep(2000);
            
            // 等待脚本执行完成
            executor1.WaitForCompletionAsync(TimeSpan.FromSeconds(10)).Wait();
            Console.WriteLine($"脚本1执行结果: {executor1.CurrentStatus}");
            
            // 显示收集的输出
            Console.WriteLine("脚本1的输出:");
            foreach (var output in outputCollector)
            {
                if (!string.IsNullOrWhiteSpace(output))
                    Console.WriteLine($"[输出] {output}");
            }
            outputCollector.Clear();
            
            // 2. 长时间运行的脚本 - 演示中止功能
            Console.WriteLine("\n2. 长时间运行的脚本 - 演示中止功能:");
            var script2 = @"
                Console.WriteLine(""开始长时间运行的任务..."");
                for (int i = 1; i <= 20; i++)
                {
                    Console.WriteLine($""任务进度: {i}/20"");
                    Thread.Sleep(100); // 减少等待时间
                }
                Console.WriteLine(""长时间任务完成!"");
                return ""任务完成"";
            ";
            
            var executor2 = script2.ExecuteWithControl(
                references: references,
                usings: usings,
                outputHandler: (output, isError) =>
                {
                    if (!string.IsNullOrWhiteSpace(output.Trim()))
                    {
                        if (isError)
                            errorCollector.Add(output.TrimEnd());
                        else
                            outputCollector.Add(output.TrimEnd());
                    }
                }
            );
            
            // 等待1秒后中止脚本
            // Thread.Sleep(1000);
            // Task.Delay(1000).Wait();
            Console.WriteLine("中止脚本执行...");
            executor2.Cancel();
            
            // 等待中止完成
            executor2.WaitForCompletionAsync(TimeSpan.FromSeconds(5)).Wait();
            Console.WriteLine($"脚本2执行结果: {executor2.CurrentStatus}");
            
            // 显示收集的输出
            Console.WriteLine("脚本2的输出:");
            foreach (var output in outputCollector)
            {
                if (!string.IsNullOrWhiteSpace(output))
                    Console.WriteLine($"[输出] {output}");
            }
            outputCollector.Clear();
            
            // 3. 错误处理演示
            Console.WriteLine("\n3. 错误处理演示:");
            var script3 = @"
                Console.WriteLine(""开始执行有错误的脚本..."");
                var invalidOperation = 10 / 0; // 这将导致除零错误
                return ""这行不会执行"";
            ";
            
            var executor3 = script3.ExecuteWithControl(
                references: references,
                usings: usings,
                outputHandler: (output, isError) =>
                {
                    if (isError)
                        errorCollector.Add(output.TrimEnd());
                    else
                        outputCollector.Add(output.TrimEnd());
                }
            );
            
            executor3.WaitForCompletionAsync(TimeSpan.FromSeconds(5)).Wait();
            Console.WriteLine($"脚本3执行结果: {executor3.CurrentStatus}");
            
            // 显示收集的输出和错误
            Console.WriteLine("脚本3的输出:");
            foreach (var output in outputCollector)
            {
                if (!string.IsNullOrWhiteSpace(output))
                    Console.WriteLine($"[输出] {output}");
            }
            Console.WriteLine("脚本3的错误:");
            foreach (var error in errorCollector)
            {
                if (!string.IsNullOrWhiteSpace(error))
                    Console.WriteLine($"[错误] {error}");
            }
            outputCollector.Clear();
            errorCollector.Clear();
            
            // 4. 手动创建执行器的方式
            Console.WriteLine("\n4. 手动创建执行器的方式:");
            var script4 = @"
                Console.WriteLine(""手动创建的执行器..."");
                var result = new List<string>();
                for (int i = 1; i <= 3; i++)
                {
                    var item = $""项目 {i}"";
                    result.Add(item);
                    Console.WriteLine(item);
                }
                return string.Join("", "", result);
            ";
            
            var executor4 = new InteractiveScriptExecutor();
            executor4.OutputReceived += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Output.Trim()))
                {
                    if (e.IsError)
                        errorCollector.Add(e.Output.TrimEnd());
                    else
                        outputCollector.Add(e.Output.TrimEnd());
                }
            };
            
            executor4.StatusChanged += (sender, e) =>
            {
                Console.WriteLine($"[状态变化] {e}");
            };
            
            var result4 = executor4.ExecuteScriptAsync(script4, null, references, usings).Result;
            Console.WriteLine($"脚本4执行结果: {result4.Status}, 返回值: {result4.ReturnValue}");
            Console.WriteLine($"执行时间: {result4.ExecutionTime.TotalMilliseconds:F2}ms");
            
            // 显示收集的输出
            Console.WriteLine("脚本4的输出:");
            foreach (var output in outputCollector)
            {
                if (!string.IsNullOrWhiteSpace(output))
                    Console.WriteLine($"[输出] {output}");
            }
            
            // 5. 清理资源
            executor1.Dispose();
            executor2.Dispose();
            executor3.Dispose();
            executor4.Dispose();
            
            Console.WriteLine("\n=== 交互式脚本执行器演示完成 ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"交互式演示过程中发生错误: {ex.Message}");
            Console.WriteLine($"错误详情: {ex}");
        }
    }
}

/// <summary>
/// 脚本执行事件参数
/// </summary>
public class ScriptOutputEventArgs : EventArgs
{
    public string Output { get; set; }
    public bool IsError { get; set; }
    public DateTime Timestamp { get; set; }
    
    public ScriptOutputEventArgs(string output, bool isError = false)
    {
        Output = output;
        IsError = isError;
        Timestamp = DateTime.Now;
    }
}

/// <summary>
/// 脚本执行状态
/// </summary>
public enum ScriptExecutionStatus
{
    NotStarted,
    Running,
    Completed,
    Cancelled,
    Error
}

/// <summary>
/// 脚本执行结果
/// </summary>
public class ScriptExecutionResult
{
    public object ReturnValue { get; set; }
    public ScriptExecutionStatus Status { get; set; }
    public Exception Error { get; set; }
    public List<string> Outputs { get; set; } = new List<string>();
    public List<string> Errors { get; set; } = new List<string>();
    public TimeSpan ExecutionTime { get; set; }
    public bool WasCancelled { get; set; }
}

/// <summary>
/// 支持即时输出和中止的脚本执行器
/// </summary>
public class InteractiveScriptExecutor : IDisposable
{
    private CancellationTokenSource _cancellationTokenSource;
    private readonly object _lockObject = new object();
    private bool _disposed = false;
    
    /// <summary>
    /// 脚本输出事件
    /// </summary>
    public event EventHandler<ScriptOutputEventArgs> OutputReceived;
    
    /// <summary>
    /// 脚本执行状态变化事件
    /// </summary>
    public event EventHandler<ScriptExecutionStatus> StatusChanged;
    
    /// <summary>
    /// 当前执行状态
    /// </summary>
    public ScriptExecutionStatus CurrentStatus { get; private set; }
    
    /// <summary>
    /// 是否正在执行
    /// </summary>
    public bool IsRunning => CurrentStatus == ScriptExecutionStatus.Running;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public InteractiveScriptExecutor()
    {
        CurrentStatus = ScriptExecutionStatus.NotStarted;
        _cancellationTokenSource = new CancellationTokenSource();
    }
    
    /// <summary>
    /// 执行脚本并支持即时输出和中止
    /// </summary>
    /// <param name="scriptCode">脚本代码</param>
    /// <param name="globals">全局变量</param>
    /// <param name="references">程序集引用</param>
    /// <param name="usings">using语句</param>
    /// <param name="cancellationToken">外部取消令牌</param>
    /// <returns>执行结果</returns>
    public async Task<ScriptExecutionResult> ExecuteScriptAsync(
        string scriptCode,
        object globals = null,
        IEnumerable<Assembly> references = null,
        IEnumerable<string> usings = null,
        CancellationToken? cancellationToken = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(InteractiveScriptExecutor));
            
        var result = new ScriptExecutionResult();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // 重置状态
            lock (_lockObject)
            {
                if (CurrentStatus == ScriptExecutionStatus.Running)
                    throw new InvalidOperationException("脚本已在执行中");
                    
                CurrentStatus = ScriptExecutionStatus.Running;
                _cancellationTokenSource = new CancellationTokenSource();
                StatusChanged?.Invoke(this, CurrentStatus);
            }
            
            // 合并取消令牌
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(
                _cancellationTokenSource.Token,
                cancellationToken ?? CancellationToken.None
            ).Token;
            
            // 创建自定义控制台重定向器
            var consoleRedirector = new ConsoleRedirector(this);
            var originalOut = Console.Out;
            var originalError = Console.Error;
            
            try
            {
                // 重定向控制台输出
                Console.SetOut(consoleRedirector.OutputWriter);
                Console.SetError(consoleRedirector.ErrorWriter);
                
                // 创建脚本选项
                var scriptOptions = ScriptOptions.Default;
                if (references != null)
                    scriptOptions = scriptOptions.AddReferences(references);
                if (usings != null)
                    scriptOptions = scriptOptions.AddImports(usings);
                
                // 执行脚本
                var scriptResult = await CSharpScript.EvaluateAsync(scriptCode, scriptOptions, globals, cancellationToken: combinedToken);
                
                result.ReturnValue = scriptResult;
                result.Status = ScriptExecutionStatus.Completed;
                
                // 输出最终结果
                if (scriptResult != null)
                {
                    var output = $"脚本执行完成，返回值: {scriptResult}";
                    OnOutputReceived(output, false);
                    result.Outputs.Add(output);
                }
            }
            finally
            {
                // 恢复控制台输出
                Console.SetOut(originalOut);
                Console.SetError(originalError);
            }
        }
        catch (OperationCanceledException)
        {
            result.Status = ScriptExecutionStatus.Cancelled;
            result.WasCancelled = true;
            var output = "脚本执行被用户取消";
            OnOutputReceived(output, false);
            result.Outputs.Add(output);
        }
        catch (CompilationErrorException ex)
        {
            result.Status = ScriptExecutionStatus.Error;
            result.Error = ex;
            var error = $"脚本编译错误: {ex.Message}";
            OnOutputReceived(error, true);
            result.Errors.Add(error);
        }
        catch (Exception ex)
        {
            result.Status = ScriptExecutionStatus.Error;
            result.Error = ex;
            var error = $"脚本执行错误: {ex.Message}";
            OnOutputReceived(error, true);
            result.Errors.Add(error);
        }
        finally
        {
            stopwatch.Stop();
            result.ExecutionTime = stopwatch.Elapsed;
            
            lock (_lockObject)
            {
                CurrentStatus = result.Status;
                StatusChanged?.Invoke(this, CurrentStatus);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// 中止脚本执行
    /// </summary>
    public void Cancel()
    {
        lock (_lockObject)
        {
            if (CurrentStatus == ScriptExecutionStatus.Running)
            {
                _cancellationTokenSource?.Cancel();
            }
        }
    }
    
    /// <summary>
    /// 等待脚本执行完成
    /// </summary>
    /// <param name="timeout">超时时间</param>
    /// <returns>是否在超时前完成</returns>
    public async Task<bool> WaitForCompletionAsync(TimeSpan timeout)
    {
        var startTime = DateTime.Now;
        while (IsRunning && (DateTime.Now - startTime) < timeout)
        {
            await Task.Delay(100);
        }
        return !IsRunning;
    }
    
    /// <summary>
    /// 触发输出事件
    /// </summary>
    public void OnOutputReceived(string output, bool isError)
    {
        // 避免递归调用，直接触发事件而不使用Console.WriteLine
        OutputReceived?.Invoke(this, new ScriptOutputEventArgs(output, isError));
    }
    
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            Cancel();
            _cancellationTokenSource?.Dispose();
            _disposed = true;
        }
    }
    
    /// <summary>
    /// 析构函数
    /// </summary>
    ~InteractiveScriptExecutor()
    {
        Dispose(false);
    }
}

/// <summary>
/// 控制台输出重定向器
/// </summary>
internal class ConsoleRedirector
{
    private readonly InteractiveScriptExecutor _executor;
    
    public ConsoleRedirector(InteractiveScriptExecutor executor)
    {
        _executor = executor;
    }
    
    public TextWriter OutputWriter => new RedirectedTextWriter(_executor, false);
    public TextWriter ErrorWriter => new RedirectedTextWriter(_executor, true);
}

/// <summary>
/// 重定向的文本写入器
/// </summary>
internal class RedirectedTextWriter : TextWriter
{
    private readonly InteractiveScriptExecutor _executor;
    private readonly bool _isError;
    
    public RedirectedTextWriter(InteractiveScriptExecutor executor, bool isError)
    {
        _executor = executor;
        _isError = isError;
    }
    
    public override Encoding Encoding => Encoding.UTF8;
    
    public override void Write(char value)
    {
        _executor.OnOutputReceived(value.ToString(), _isError);
    }
    
    public override void Write(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _executor.OnOutputReceived(value, _isError);
        }
    }
    
    public override void WriteLine(string value)
    {
        var output = string.IsNullOrEmpty(value) ? Environment.NewLine : value + Environment.NewLine;
        _executor.OnOutputReceived(output, _isError);
    }
    
    public override void WriteLine()
    {
        _executor.OnOutputReceived(Environment.NewLine, _isError);
    }
    
    protected override void Dispose(bool disposing)
    {
        // 不需要特殊处理
        base.Dispose(disposing);
    }
}

/// <summary>
/// 脚本执行器扩展方法
/// </summary>
public static class ScriptExecutorExtensions
{
    /// <summary>
    /// 创建并配置脚本执行器
    /// </summary>
    /// <param name="scriptCode">脚本代码</param>
    /// <param name="globals">全局变量</param>
    /// <param name="references">程序集引用</param>
    /// <param name="usings">using语句</param>
    /// <returns>配置好的脚本执行器</returns>
    public static InteractiveScriptExecutor CreateExecutor(
        this string scriptCode,
        object globals = null,
        IEnumerable<Assembly> references = null,
        IEnumerable<string> usings = null)
    {
        var executor = new InteractiveScriptExecutor();
        return executor;
    }
    
    /// <summary>
    /// 执行脚本并返回执行器（用于后续控制）
    /// </summary>
    /// <param name="scriptCode">脚本代码</param>
    /// <param name="globals">全局变量</param>
    /// <param name="references">程序集引用</param>
    /// <param name="usings">using语句</param>
    /// <param name="outputHandler">输出处理回调</param>
    /// <param name="statusHandler">状态变化回调</param>
    /// <returns>脚本执行器</returns>
    public static InteractiveScriptExecutor ExecuteWithControl(
        this string scriptCode,
        object globals = null,
        IEnumerable<Assembly> references = null,
        IEnumerable<string> usings = null,
        Action<string, bool> outputHandler = null,
        Action<ScriptExecutionStatus> statusHandler = null)
    {
        var executor = new InteractiveScriptExecutor();
        
        if (outputHandler != null)
        {
            executor.OutputReceived += (sender, e) => outputHandler(e.Output, e.IsError);
        }
        
        if (statusHandler != null)
        {
            executor.StatusChanged += (sender, e) => statusHandler(e);
        }
        
        // 异步执行脚本
        _ = Task.Run(async () =>
        {
            await executor.ExecuteScriptAsync(scriptCode, globals, references, usings);
        });
        
        return executor;
    }
}