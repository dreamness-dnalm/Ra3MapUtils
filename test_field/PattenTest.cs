using System.Text.RegularExpressions;

namespace test_field;

public class PattenTest
{
    private static string normalLogPattern = @"\[(?<time>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3})\].*?\](?<frame>\d+)\s(?<detail>.+)";
    private static string systemLogPattern = @"\[(?<time>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3})\]\s.*?\]\s(?<detail>.+)";
    private static string detailLogpattern = @"<(?<level>\w+)><pos:(?<position>[^>]+)>(?<content>.+)";

    public static void main()
    {
        // string log = "[2025-02-06 02:04:01.158] [ra3GameDebug] [info] [appendMessage]3428 <DEBUG><pos:Archon2vsync_money>after_unit_player_money: 25750";
        // string log = "[2025-02-06 02:04:01.222] [ra3GameDebug] [info] [appendMessage]3429 Run script - /SchedulerModuleRunner.lua";
        // string log =
        //     "[2025-02-06 01:36:09.443] [ra3GameDebug] [info] map path: c:\\users\\mmmmm\\appdata\\roaming\\red alert 3\\maps\\cor_archon_2v2v2_caldera_of_chaos_v0_6_dev\\cor_archon_2v2v2_caldera_of_chaos_v0_6_dev.map";
        string log =
            "[2025-02-09 20:37:42.808] [ra3GameDebug] [info] Ra3GameDebugParam: enableDebugTool: true, fastIntoSkirmishGame: falseRa3GameDebugParam: enableDebugTool: true, fastIntoSkirmishGame: falseRa3GameDebugParam: enableDebugTool: true, fastIntoSkirmishGame: falseRa3GameDebugParam: enableDebugTool: true, fastIntoSkirmishGame: falseRa3GameDebugParam: enableDebugTool: true, fastIntoSkirmishGame: false";
        var normalLogMatch = Regex.Match(log, normalLogPattern);
        if (normalLogMatch.Success)
        {
            Console.WriteLine("normalLogMatch");
            Console.WriteLine("time: " + normalLogMatch.Groups["time"].Value);
            Console.WriteLine("frameIndex: " + normalLogMatch.Groups["frame"].Value);
            string logDetail = normalLogMatch.Groups["detail"].Value;
            Console.WriteLine("logDetail: " + logDetail);

            var detailMatch = Regex.Match(logDetail, detailLogpattern);
            if (detailMatch.Success)
            {
                Console.WriteLine("detailMatch");
                Console.WriteLine("logLevel: " + detailMatch.Groups["level"].Value);
                Console.WriteLine("logPos: " + detailMatch.Groups["position"].Value);
                Console.WriteLine("logContent: " + detailMatch.Groups["content"].Value);
            }
            else
            {
                Console.WriteLine("detailLogpattern failed");
            }
        }
        else
        {
            Console.WriteLine("normalLogMatch failed");
            var systemLogMatch = Regex.Match(log, systemLogPattern);
            if (systemLogMatch.Success)
            {
                Console.WriteLine("systemLogMatch");
                Console.WriteLine("time: " + systemLogMatch.Groups["time"].Value);
                Console.WriteLine("logContent: " + systemLogMatch.Groups["detail"].Value);
            }
            else
            {
                Console.WriteLine("systemLogMatch failed");
            }
        }
        // Console.WriteLine();

    }
    
    //     var normalLogMatch = Regex.Match(log, normalLogPattern);
    //     if (normalLogMatch.Success)
    //     {
    //         time = normalLogMatch.Groups["time"].Value;
    //         frameIndex = normalLogMatch.Groups["frame"].Value;
    //         logDetail = normalLogMatch.Groups["detail"].Value;
    //
    //         var detailMatch = Regex.Match(logDetail, detailLogpattern);
    //         if (detailMatch.Success)
    //         {
    //             logLevel = detailMatch.Groups["level"].Value;
    //             logPos = detailMatch.Groups["position"].Value;
    //             logContent = detailMatch.Groups["content"].Value;
    //         }
    //         else
    //         {
    //             logLevel = "TRACE";
    //             logContent = logDetail;
    //         }
    //     }
    //     else
    //     {
    //         var systemLogMatch = Regex.Match(log, systemLogPattern);
    //         if (systemLogMatch.Success)
    //         {
    //             time = systemLogMatch.Groups["time"].Value;
    //             logContent = systemLogMatch.Groups["detail"].Value;
    //             logLevel = "INFO";
    //         }
    //         else
    //         {
    //             logContent = log;
    //         }
    //     }
    //     

    // }
}