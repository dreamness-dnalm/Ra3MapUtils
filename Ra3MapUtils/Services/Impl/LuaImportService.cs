using System.IO;
using System.Xml.Linq;
using MapCoreLib.Core.Util;
using Ra3MapUtils.Models;
using Ra3MapUtils.Services.Interface;
using Ra3MapUtils.Utils;
using UtilLib.mapXmlOperator;

namespace Ra3MapUtils.Services.Impl;

public class LuaImportService: ILuaImportService 
{
    public void SaveMapLuaLibConfig(LuaLibConfigModel luaLibConfigModel)
    {
        InitDB();
        SqliteDBUtil.Execute("""
            insert or replace into lua_import_config(MapName, ShowingName, LibPath, OrderNum) values(@mapName, @showingName, @libPath, @orderNum)
        """, new Dictionary<string, object>
        {
            {"@mapName", luaLibConfigModel.MapName},
            {"@showingName", luaLibConfigModel.ShowingName},
            {"@libPath", luaLibConfigModel.LibPath},
            {"@orderNum", luaLibConfigModel.OrderNum}
        });
    }
    
    public void RenameMapLuaLibConfig(string mapName, string oldShowingName, string newShowingName)
    {
        InitDB();
        SqliteDBUtil.Execute("""
            update lua_import_config set ShowingName = @newShowingName where MapName = @mapName and ShowingName = @oldShowingName
        """, new Dictionary<string, object>
        {
            {"@mapName", mapName},
            {"@oldShowingName", oldShowingName},
            {"@newShowingName", newShowingName}
        });
    }

    public void DeleteMapLuaLibConfig(string mapName, string showingName)
    {
        InitDB();
        SqliteDBUtil.Execute("""
            delete from lua_import_config where MapName = @mapName and ShowingName = @showingName
        """, new Dictionary<string, object>
        {
            {"@mapName", mapName},
            {"@showingName", showingName}
        });
    }

    public List<LuaLibConfigModel> LoadMapLuaLibConfig(string mapName)
    {
        InitDB();
        return SqliteDBUtil.Query("""
                   select * from lua_import_config where MapName = @mapName order by OrderNum
               """, new Dictionary<string, object> { { "@mapName", mapName } }, RowMapper);
    }

    public void UpsertScriptGroup(string mapName, XElement scriptGroupXElement, string behindScriptGroupName)
    {
        var xmlPath = Path.Combine(PathUtil.RA3MapFolder, mapName, mapName + ".edit.xml");
        var xmlOperator = MapXmlOperator.Load(xmlPath);
        xmlOperator.RemoveScriptGroup(scriptGroupXElement.Attribute("Name").Value);
        xmlOperator.AddScriptGroup(scriptGroupXElement, behindScriptGroupName);
        xmlOperator.Save(xmlPath);
    }

    private LuaLibConfigModel RowMapper(Dictionary<string, object> row)
    {
        return new LuaLibConfigModel(
            row["MapName"].ToString(),
            row["ShowingName"].ToString(),
            row["LibPath"].ToString(),
            int.Parse(row["OrderNum"].ToString())
            );
    }

    private void InitDB()
    {
        SqliteDBUtil.Execute("""
            create table if not exists lua_import_config
            (
                id         integer primary key autoincrement,
                MapName    text not null,
                ShowingName text not null,
                LibPath    text not null,
                OrderNum       integer not null,
                UNIQUE(MapName, ShowingName)
            )                     
        """) ;
    }
    
}