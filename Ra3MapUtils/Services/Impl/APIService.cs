using System.Net;
using System.Text;
using System.Text.Json;

namespace Ra3MapUtils.Services.Impl;

public class APIService
{
    private readonly HttpListener _listener = new();
    private CancellationTokenSource _cts;

    public void Start()
    {
        _listener.Prefixes.Add("http://localhost:24880/api/");
        _listener.Start();
        _cts = new CancellationTokenSource();
        Task.Run(() => ListenLoop(_cts.Token));
    }

    public void Stop()
    {
        _cts?.Cancel();
        _listener?.Stop();
    }

    private async Task ListenLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var ctx = await _listener.GetContextAsync();

            string path = ctx.Request.Url.AbsolutePath;
            string response = path switch
            {
                "/api/ping" => "pong",
                "/api/maps" => "HandleGetMaps",
                "/api/maps/create" => "await HandleCreateMap(ctx)",
                _ => "unknown path"
            };

            byte[] buffer = Encoding.UTF8.GetBytes(response);
            ctx.Response.ContentType = "application/json";
            ctx.Response.ContentLength64 = buffer.Length;
            await ctx.Response.OutputStream.WriteAsync(buffer);
            ctx.Response.Close();
        }
    }

    // private string HandleGetMaps()
    // {
    //     // TODO: 实际数据来源可以读取本地文件或内存结构
    //     var maps = new List<MapInfo>
    //     {
    //         new() { Id = "map001", Name = "测试地图1", Modified = DateTime.Now }
    //     };
    //     return JsonSerializer.Serialize(maps);
    // }
    //
    // private async Task<string> HandleCreateMap(HttpListenerContext ctx)
    // {
    //     using var reader = new StreamReader(ctx.Request.InputStream);
    //     var body = await reader.ReadToEndAsync();
    //     var newMap = JsonSerializer.Deserialize<MapInfo>(body);
    //
    //     // TODO: 实际创建地图的逻辑（调用地图管理器）
    //     Console.WriteLine($"创建地图: {newMap.Name}");
    //
    //     return JsonSerializer.Serialize(new { status = "ok" });
    // }
}