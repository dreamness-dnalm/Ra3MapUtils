# 交互式脚本执行器使用说明

## 概述

`InteractiveScriptExecutor` 是一个支持即时输出捕获、错误处理和中止功能的C#脚本执行器。它基于 Roslyn 的 `CSharpScript` 实现，提供了完整的脚本执行控制能力。

## 主要特性

- ✅ **即时输出捕获**: 实时获取脚本的 `Console.WriteLine()` 输出
- ✅ **错误处理**: 自动捕获和报告脚本执行错误
- ✅ **执行控制**: 支持随时中止正在运行的脚本
- ✅ **状态监控**: 实时监控脚本执行状态
- ✅ **资源管理**: 自动管理脚本执行资源
- ✅ **异步支持**: 完全异步的脚本执行

## 基本用法

### 1. 简单使用

```csharp
using Ra3MapUtils.Utils;

// 准备脚本执行环境
var references = new[] { typeof(Console).Assembly };
var usings = new[] { "System", "System.Threading" };

// 创建脚本
var script = @"
    Console.WriteLine(""Hello from script!"");
    for (int i = 1; i <= 5; i++)
    {
        Console.WriteLine($""Processing item {i}..."");
        Thread.Sleep(100);
    }
    return ""Script completed"";
";

// 执行脚本并获取执行器
var executor = script.ExecuteWithControl(
    references: references,
    usings: usings,
    outputHandler: (output, isError) =>
    {
        var prefix = isError ? "[错误] " : "[脚本] ";
        Console.WriteLine($"{prefix}{output.TrimEnd()}");
    }
);

// 等待执行完成
await executor.WaitForCompletionAsync(TimeSpan.FromSeconds(10));

// 清理资源
executor.Dispose();
```

### 2. 手动创建执行器

```csharp
// 手动创建执行器
var executor = new InteractiveScriptExecutor();

// 订阅输出事件
executor.OutputReceived += (sender, e) =>
{
    var prefix = e.IsError ? "[错误] " : "[脚本] ";
    Console.WriteLine($"{prefix}{e.Output.TrimEnd()}");
};

// 订阅状态变化事件
executor.StatusChanged += (sender, e) =>
{
    Console.WriteLine($"[状态变化] {e}");
};

// 执行脚本
var result = await executor.ExecuteScriptAsync(script, null, references, usings);

// 检查结果
Console.WriteLine($"执行状态: {result.Status}");
Console.WriteLine($"返回值: {result.ReturnValue}");
Console.WriteLine($"执行时间: {result.ExecutionTime.TotalMilliseconds:F2}ms");

// 清理资源
executor.Dispose();
```

## 高级功能

### 1. 脚本中止

```csharp
var longRunningScript = @"
    for (int i = 1; i <= 100; i++)
    {
        Console.WriteLine($""Progress: {i}%"");
        Thread.Sleep(100);
    }
    return ""Done"";
";

var executor = longRunningScript.ExecuteWithControl(
    references: references,
    usings: usings,
    outputHandler: (output, isError) =>
    {
        Console.WriteLine($"[脚本] {output.TrimEnd()}");
    }
);

// 等待一段时间后中止
await Task.Delay(2000);
Console.WriteLine("中止脚本执行...");
executor.Cancel();

// 等待中止完成
await executor.WaitForCompletionAsync(TimeSpan.FromSeconds(5));
```

### 2. 错误处理

```csharp
var errorScript = @"
    Console.WriteLine(""开始执行..."");
    var result = 10 / 0; // 除零错误
    return ""This won't execute"";
";

var executor = errorScript.ExecuteWithControl(
    references: references,
    usings: usings,
    outputHandler: (output, isError) =>
    {
        if (isError)
        {
            Console.WriteLine($"[错误] {output.TrimEnd()}");
        }
        else
        {
            Console.WriteLine($"[输出] {output.TrimEnd()}");
        }
    }
);

await executor.WaitForCompletionAsync(TimeSpan.FromSeconds(5));
Console.WriteLine($"脚本状态: {executor.CurrentStatus}");
```

### 3. 超时控制

```csharp
// 设置超时时间
var completed = await executor.WaitForCompletionAsync(TimeSpan.FromSeconds(30));
if (!completed)
{
    Console.WriteLine("脚本执行超时，强制中止...");
    executor.Cancel();
}
```

## 事件说明

### OutputReceived 事件

当脚本产生输出时触发：

```csharp
executor.OutputReceived += (sender, e) =>
{
    // e.Output: 输出内容
    // e.IsError: 是否为错误输出
    // e.Timestamp: 输出时间戳
};
```

### StatusChanged 事件

当脚本执行状态发生变化时触发：

```csharp
executor.StatusChanged += (sender, e) =>
{
    // e 是 ScriptExecutionStatus 枚举值
    // NotStarted: 未开始
    // Running: 正在运行
    // Completed: 已完成
    // Cancelled: 已取消
    // Error: 发生错误
};
```

## 执行结果

`ScriptExecutionResult` 包含脚本执行的详细信息：

```csharp
public class ScriptExecutionResult
{
    public object ReturnValue { get; set; }           // 脚本返回值
    public ScriptExecutionStatus Status { get; set; } // 执行状态
    public Exception Error { get; set; }              // 错误信息
    public List<string> Outputs { get; set; }         // 所有输出
    public List<string> Errors { get; set; }          // 所有错误
    public TimeSpan ExecutionTime { get; set; }       // 执行时间
    public bool WasCancelled { get; set; }            // 是否被取消
}
```

## 最佳实践

### 1. 资源管理

始终使用 `using` 语句或手动调用 `Dispose()` 来清理执行器：

```csharp
using (var executor = new InteractiveScriptExecutor())
{
    // 使用执行器
    await executor.ExecuteScriptAsync(script);
}
```

### 2. 错误处理

```csharp
try
{
    var result = await executor.ExecuteScriptAsync(script);
    if (result.Status == ScriptExecutionStatus.Error)
    {
        Console.WriteLine($"脚本执行失败: {result.Error?.Message}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"执行器异常: {ex.Message}");
}
```

### 3. 输出处理

避免在输出处理器中调用 `Console.WriteLine()`，以防止递归：

```csharp
// 正确做法
executor.OutputReceived += (sender, e) =>
{
    // 收集输出到列表
    outputs.Add(e.Output);
    
    // 或者写入日志文件
    File.AppendAllText("script.log", e.Output);
};

// 错误做法 - 可能导致递归
executor.OutputReceived += (sender, e) =>
{
    Console.WriteLine(e.Output); // 这可能导致递归！
};
```

## 注意事项

1. **递归风险**: 避免在输出处理器中调用 `Console.WriteLine()`
2. **资源清理**: 始终调用 `Dispose()` 方法清理资源
3. **异常处理**: 脚本执行异常会被包装在 `ScriptExecutionResult.Error` 中
4. **线程安全**: 执行器是线程安全的，可以在多个线程中使用
5. **性能考虑**: 长时间运行的脚本应该定期检查取消令牌

## 示例代码

完整的示例代码请参考 `ScriptExecutorExample.cs` 文件，其中包含了各种使用场景的详细示例。
