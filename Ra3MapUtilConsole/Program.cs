using System;
using System.Collections.Generic;
using CommandLine;
using Ra3MapUtilConsole.functions;

namespace Ra3MapUtilConsole
{
    internal  class Program
    {
        static void Main(string[] args)
        {
            // Parser.Default
                new Parser(config =>
                    {
                        config.AutoVersion = false;
                        config.AutoHelp = false;
                    })
                .ParseArguments<MoveOptions, CopyOptions, LoadLuaOptions, CommonOpts>(args)
                .MapResult(
                    (MoveOptions opts) => RunMoveAndReturnExitCode(opts),
                    (CopyOptions opts) => RunCopyAndReturnExitCode(opts),
                    (LoadLuaOptions opts) => RunLoadLuaAndReturnExitCode(opts),
                    (CommonOpts opts) => 0,
                    errs => HandleParseError(errs)
                );
            var parser = new Parser(config => config.HelpWriter = null);
        }

        static int RunMoveAndReturnExitCode(MoveOptions opts)
        {
            Console.WriteLine($"Moving from {opts.Path1} to {opts.Path2}");
            // 实现移动逻辑
            return 0;
        }

        static int RunCopyAndReturnExitCode(CopyOptions opts)
        {
            Console.WriteLine($"Copying from {opts.Path1} to {opts.Path2}");
            // 实现复制逻辑
            return 0;
        }

        static int RunLoadLuaAndReturnExitCode(LoadLuaOptions opts)
        {
            Console.WriteLine($"Loading Lua script from {opts.ConfigPath}");
            // 实现加载 Lua 脚本的逻辑
            return 0;
        }

        static int HandleParseError(IEnumerable<Error> errs)
        {
            
            Console.WriteLine("version: 99999");
            if (errs.IsVersion())
            {
                Console.WriteLine("0.1111");
            }
            
            // 处理解析错误
            foreach (var error in errs)
            {
                Console.WriteLine($"Error: {error.ToString()}");
            }
            return 1;
        }
    }
        
    

        

        

        

    
}