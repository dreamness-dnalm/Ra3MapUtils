using System.ComponentModel.DataAnnotations;
using LinqToDB.Mapping;

namespace SharedFunctionLib.Models;

[Table(Name = "lua_import_config")]
public class SimpleLuaLibConfigModel
{
        // [PrimaryKey, Identity]
        // public int id;
        [Column(Name="MapName")]
        public string MapName;
        [Column(Name="ShowingName")]
        public string ShowingName;
        [Column(Name="LibPath")]
        public string LibPath;
        [Column(Name="OrderNum")]
        public int OrderNum;
        [Column(Name="IsEnabled")]
        public int IsEnabled = 1;
        

        public override string ToString()
        {
                return "SimpleLuaLibConfigModel{" +
                       "MapName='" + MapName + '\'' +
                       ", ShowingName='" + ShowingName + '\'' +
                       ", LibPath='" + LibPath + '\'' +
                       ", OrderNum=" + OrderNum +
                          ", IsEnabled=" + IsEnabled +
                       '}';
        }
}