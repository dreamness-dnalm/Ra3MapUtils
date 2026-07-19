using NUnit.Framework;
using Ra3MapUtils.Models;
using Ra3MapUtils.Services.Impl;

namespace Ra3MapUtils.Tests;

[TestFixture]
public class LuaCompletionParserTests
{
    [Test]
    public void Parse_ExtractsLua4NativeFunctionDocumentation()
    {
        const string code = """
            --- 获取单位
            --- @param name string 单位名
            --- @return Unit 单位
            function GetUnit(name) end
            """;

        var item = LuaCompletionParser.Parse(
                code,
                "origin_func.lua",
                LuaCompletionSourceKind.NativeApi,
                null)
            .Single(candidate => candidate.Name == "GetUnit");

        Assert.That(item.Signature, Is.EqualTo("GetUnit(name)"));
        Assert.That(item.ReturnType, Is.EqualTo("Unit"));
        Assert.That(item.Description, Does.Contain("获取单位"));
        Assert.That(item.Description, Does.Contain("@param name string 单位名"));
    }

    [Test]
    public void Parse_ExtractsAssignedFunctionsColonMethodsAndLua4Upvalues()
    {
        const string code = """
            Demo = {}
            create_global = function(name) end
            local create_local = function(name) end
            Demo.create = function(name)
                local callback;
                callback = function()
                    return %name
                end
            end
            function Demo:tick(frame) end
            """;

        var items = LuaCompletionParser.Parse(
            code,
            "demo.lua",
            LuaCompletionSourceKind.UserLibrary,
            null);

        Assert.That(items.Any(item => item.QualifiedName == "create_global" && item.IsCallable), Is.True);
        Assert.That(items.Any(item => item.QualifiedName == "create_local" && item.IsCallable), Is.True);
        Assert.That(items.Any(item => item.QualifiedName == "Demo.create"), Is.True);
        Assert.That(items.Any(item => item.QualifiedName == "Demo:tick" && item.Kind == LuaCompletionItemKind.Method), Is.True);
        Assert.That(items.Any(item => item.Name == "callback" && item.Kind == LuaCompletionItemKind.Variable), Is.True);
    }

    [Test]
    public void Parse_ExtractsClassesFieldsAndExplicitTypes()
    {
        const string code = """
            --- @class Train
            --- @field speed number 每帧速度
            --- @field unit Unit
            --- @type Train
            Train = {}
            """;

        var items = LuaCompletionParser.Parse(
            code,
            "train.lua",
            LuaCompletionSourceKind.UserLibrary,
            null);

        Assert.That(items.Any(item => item.QualifiedName == "Train" && item.Kind == LuaCompletionItemKind.Class), Is.True);
        Assert.That(items.Any(item => item.QualifiedName == "Train.speed" && item.ReturnType == "number"), Is.True);
        Assert.That(items.Any(item => item.QualifiedName == "Train.unit" && item.ReturnType == "Unit"), Is.True);
    }

    [Test]
    public void Parse_DoesNotTreatLua5LocalFunctionAsLua4Declaration()
    {
        const string code = "local function unsupported() end";

        var items = LuaCompletionParser.Parse(
            code,
            "invalid.lua",
            LuaCompletionSourceKind.UserLibrary,
            null);

        Assert.That(items.Any(item => item.Name == "unsupported"), Is.False);
    }

    [Test]
    public void Parse_ExtractsMultipleLua4LocalVariables()
    {
        const string code = "local x, y, z = GetPosition()";

        var items = LuaCompletionParser.Parse(
            code,
            "locals.lua",
            LuaCompletionSourceKind.UserLibrary,
            null);

        Assert.That(items.Where(item => item.Kind == LuaCompletionItemKind.Variable).Select(item => item.Name),
            Is.EquivalentTo(new[] { "x", "y", "z" }));
    }

    [Test]
    public void Completion_ShowsIndexedItemsInEmptyDocument()
    {
        using var service = new LuaCompletionService();

        var items = service.GetCompletions("", 0);

        Assert.That(items.Any(item => item.Name == "function" && item.Kind == LuaCompletionItemKind.Keyword), Is.True);
    }

    [Test]
    public void Completion_AtRootExcludesModuleMembers()
    {
        const string code = """
            GameModule = {}
            GameModule.game_over_with_defeat_screen = function() end
            GameModule.game_over_with_game_over_screen = function() end
            GameModule.game_over_with_victory_screen = function() end
            Game
            """;
        using var service = new LuaCompletionService();

        var items = service.GetCompletions(code, code.Length);

        Assert.That(items.Any(item => item.Name == "GameModule"), Is.True);
        Assert.That(items.Any(item => item.ContainerName == "GameModule"), Is.False);
    }

    [Test]
    public void Completion_AtRootExcludesQualifiedTableAssignments()
    {
        const string code = """
            FilterBuilder = {}
            function FilterBuilder:add_exclusions()
                self.exclude_kind_ofs = {}
                self.exclude_statuses = {}
                self.exclude_things = {}
            end
            ex
            """;
        using var service = new LuaCompletionService();

        var items = service.GetCompletions(code, code.Length);

        Assert.That(items.Any(item => item.Name == "exclude_kind_ofs"), Is.False);
        Assert.That(items.Any(item => item.Name == "exclude_statuses"), Is.False);
        Assert.That(items.Any(item => item.Name == "exclude_things"), Is.False);
    }

    [Test]
    public void Completion_UsesAnnotatedReturnTypeForMemberFiltering()
    {
        const string code = """
            --- @class Unit
            --- @field health number
            --- @return Unit
            function GetUnit() end
            local unit = GetUnit()
            unit.he
            """;
        using var service = new LuaCompletionService();

        var items = service.GetCompletions(code, code.Length);

        Assert.That(items.Any(item => item.Name == "health" && item.ContainerName == "Unit"), Is.True);
        Assert.That(items.All(item => item.Name.StartsWith("he", StringComparison.OrdinalIgnoreCase)), Is.True);
    }

    [Test]
    public void Completion_UsesCallReturnTypeAfterColon()
    {
        const string code = """
            --- @class Unit
            function Unit:set_name(name) end
            --- @return Unit
            function UnitHelper.get_unit_from_id(id) end
            UnitHelper.get_unit_from_id(10):
            """;
        using var service = new LuaCompletionService();

        var items = service.GetCompletions(code, code.Length);

        Assert.That(items.Any(item => item.Name == "set_name" && item.ContainerName == "Unit"), Is.True);
        Assert.That(items.All(item => item.ContainerName == "Unit"), Is.True);
    }

    [Test]
    public void Documentation_ResolvesAnnotatedMemberAfterCallResult()
    {
        const string code = """
            --- @class Unit
            --- 添加状态
            --- @param status string 状态名
            function Unit:add_status(status) end
            --- 根据 ID 获取单位
            --- @return Unit
            function UnitHelper.get_unit_from_id(id) end
            UnitHelper.get_unit_from_id(12):add_status("NO_ATTACK")
            """;
        using var service = new LuaCompletionService();
        var offset = code.LastIndexOf("add_status", StringComparison.Ordinal) + 2;

        var item = service.GetDocumentation(code, offset);

        Assert.That(item, Is.Not.Null);
        Assert.That(item!.QualifiedName, Is.EqualTo("Unit:add_status"));
        Assert.That(item.Description, Does.Contain("添加状态"));
        Assert.That(item.Description, Does.Contain("@param status string 状态名"));
    }

    [Test]
    public void Completion_ExecuteActionStringUsesActionIds()
    {
        const string code = "ExecuteAction(\"MAP_";
        using var service = new LuaCompletionService();

        var items = service.GetCompletions(code, code.Length);

        Assert.That(items, Is.Not.Empty);
        Assert.That(items.All(item => item.Kind == LuaCompletionItemKind.ScriptActionId), Is.True);
        Assert.That(items.Any(item => item.Name == "MAP_REVEAL_ALL_PERM"), Is.True);
        Assert.That(items.Any(item => item.Name == "GetFrame"), Is.False);
    }

    [Test]
    public void Completion_EvaluateConditionStringUsesConditionIdsAndSingleQuote()
    {
        const string code = "EvaluateCondition('NAMED_";
        using var service = new LuaCompletionService();

        var items = service.GetCompletions(code, code.Length);

        Assert.That(items, Is.Not.Empty);
        Assert.That(items.All(item => item.Kind == LuaCompletionItemKind.ScriptConditionId), Is.True);
        Assert.That(items.Any(item => item.Name == "NAMED_NOT_DESTROYED"), Is.True);
    }

    [Test]
    public void Completion_OpeningQuoteReturnsAllScriptIds()
    {
        using var service = new LuaCompletionService();

        var actions = service.GetCompletions("ExecuteAction(\"", "ExecuteAction(\"".Length);
        var conditions = service.GetCompletions("EvaluateCondition(\"", "EvaluateCondition(\"".Length);

        Assert.That(actions.Count, Is.GreaterThan(500));
        Assert.That(actions.All(item => item.Kind == LuaCompletionItemKind.ScriptActionId), Is.True);
        Assert.That(conditions.Count, Is.GreaterThan(150));
        Assert.That(conditions.All(item => item.Kind == LuaCompletionItemKind.ScriptConditionId), Is.True);
    }

    [Test]
    public void Completion_DoesNotOfferScriptIdsInOtherStringsOrArguments()
    {
        const string ordinaryString = "print(\"MAP_";
        const string secondArgument = "ExecuteAction(\"MAP_REVEAL_ALL_PERM\", \"Player_";
        const string commentedCall = "-- ExecuteAction(\"MAP_";
        using var service = new LuaCompletionService();

        var ordinaryItems = service.GetCompletions(ordinaryString, ordinaryString.Length);
        var secondArgumentItems = service.GetCompletions(secondArgument, secondArgument.Length);
        var commentedItems = service.GetCompletions(commentedCall, commentedCall.Length);

        Assert.That(ordinaryItems, Is.Empty);
        Assert.That(secondArgumentItems, Is.Empty);
        Assert.That(commentedItems.Any(item => item.Kind == LuaCompletionItemKind.ScriptActionId), Is.False);
    }

    [Test]
    public void Documentation_ResolvesScriptActionIdMetadata()
    {
        const string code = "ExecuteAction(\"MAP_REVEAL_ALL_PERM\")";
        using var service = new LuaCompletionService();
        var offset = code.IndexOf("MAP_REVEAL_ALL_PERM", StringComparison.Ordinal) + 2;

        var item = service.GetDocumentation(code, offset);

        Assert.That(item, Is.Not.Null);
        Assert.That(item!.Kind, Is.EqualTo(LuaCompletionItemKind.ScriptActionId));
        Assert.That(item.Signature, Does.StartWith("ExecuteAction(\"MAP_REVEAL_ALL_PERM\""));
        Assert.That(item.Description, Does.Contain("编辑器编号："));
        Assert.That(item.Description, Does.Contain("中文："));
    }

    [Test]
    public void BuildIndex_RespectsLuaLibraryOptionAndExcludesFutureApis()
    {
        var root = Path.Combine(Path.GetTempPath(), "Ra3MapUtilsLuaCompletionTests", Guid.NewGuid().ToString("N"));
        var nativePath = Directory.CreateDirectory(Path.Combine(root, "origin_funcs")).FullName;
        var libraryPath = Directory.CreateDirectory(Path.Combine(root, "lib")).FullName;
        var userPath = Directory.CreateDirectory(Path.Combine(root, "user")).FullName;

        try
        {
            File.WriteAllText(Path.Combine(nativePath, "origin_func_1000.lua"), "function NativeApi() end");
            File.WriteAllText(Path.Combine(nativePath, "origin_func_future.lua"), "function FutureApi() end");
            File.WriteAllText(Path.Combine(libraryPath, "library.lua"), "function LibraryApi() end");
            File.WriteAllText(Path.Combine(userPath, "user.lua"), "function UserApi() end");

            var enabled = LuaCompletionService.BuildIndex(
                new LuaCompletionOptions(true, userPath),
                root,
                CancellationToken.None);
            var disabled = LuaCompletionService.BuildIndex(
                new LuaCompletionOptions(false, userPath),
                root,
                CancellationToken.None);

            Assert.That(enabled.Items.Any(item => item.Name == "NativeApi" && item.IntroducedVersion == "1.000"), Is.True);
            Assert.That(enabled.Items.Any(item => item.Name == "LibraryApi"), Is.True);
            Assert.That(enabled.Items.Any(item => item.Name == "UserApi"), Is.True);
            Assert.That(enabled.Items.Any(item => item.Name == "FutureApi"), Is.False);
            Assert.That(disabled.Items.Any(item => item.Name == "NativeApi"), Is.True);
            Assert.That(disabled.Items.Any(item => item.Name == "LibraryApi"), Is.False);
            Assert.That(disabled.Items.Any(item => item.Name == "UserApi"), Is.True);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Test]
    public void ParseDevelopmentWorkspace_WhenConfigured()
    {
        var workspace = Environment.GetEnvironmentVariable("RA3_LUA_WORKSPACE");
        if (string.IsNullOrWhiteSpace(workspace) || !Directory.Exists(workspace))
        {
            Assert.Ignore("未配置 RA3_LUA_WORKSPACE，跳过真实 Lua 4 工程验证。");
        }

        var luaRoot = Path.Combine(workspace!, "RA3CoronaMapLuaLib");
        var files = Directory.EnumerateFiles(Path.Combine(luaRoot, "lib"), "*.lua", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(Path.Combine(luaRoot, "origin_funcs"), "*.lua", SearchOption.TopDirectoryOnly))
            .Where(path => !Path.GetFileNameWithoutExtension(path).EndsWith("_future", StringComparison.OrdinalIgnoreCase));
        var items = files.SelectMany(path => LuaCompletionParser.Parse(
                File.ReadAllText(path),
                path,
                path.Contains("origin_funcs", StringComparison.OrdinalIgnoreCase)
                    ? LuaCompletionSourceKind.NativeApi
                    : LuaCompletionSourceKind.LuaLibrary,
                null))
            .ToArray();

        Assert.That(items.Length, Is.GreaterThan(500));
        Assert.That(items.Any(item => item.QualifiedName == "GetFrame"), Is.True);
        Assert.That(items.Any(item => item.QualifiedName.StartsWith("GameModule.", StringComparison.Ordinal)), Is.True);
    }
}
