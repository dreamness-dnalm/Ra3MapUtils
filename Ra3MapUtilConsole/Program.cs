using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
using Ra3MapUtilConsole.functions;

namespace Ra3MapUtilConsole
{
    internal  class Program
    {
          
        static void Main(string[] args)
        {
            // Parser.Default
            var parser = new Parser(config =>
            {
                config.HelpWriter = null;
                config.AutoVersion = false;
                config.AutoHelp = false;
            });
            var parserResult = parser.ParseArguments<
                MoveOptions, 
                CopyOptions, 
                RemoveOptions,
                LoadLuaOptions, 
                VersionOptions,
                LsOptions
            >(args);
            parserResult
                .MapResult(
                    (MoveOptions opts) => MoveFunction.DoAction(opts),
                    (CopyOptions opts) => CopyFunction.DoAction(opts),
                    (RemoveOptions opts) => RemoveFunction.DoAction(opts),
                    (LoadLuaOptions opts) => LoadLuaFunction.DoAction(opts),
                    (VersionOptions opts) => VersionFunction.DoAction(opts),
                    (LsOptions opts) => LsFunction.DoAction(opts),
                    errs => HandleParseError(errs, parserResult, parser)
                );
        }

        static int HandleParseError(IEnumerable<Error> errs,  ParserResult<object> parserResult, Parser parser)
        {
            
            var helpText = HelpText.AutoBuild(parserResult, h =>
            {
                h.AdditionalNewLineAfterOption = false;
                h.Heading = ProgramInfo.VersionInfo; 
                h.Copyright = "";
                h.AutoVersion = false;
                h.AutoHelp = false;
                return h;
            }, e => e);

            Console.WriteLine(helpText);

            
            foreach (var error in errs)
            {
                Console.WriteLine($"Error: {error.ToString()}");
            }
            
            return 1;
        }
    }
}