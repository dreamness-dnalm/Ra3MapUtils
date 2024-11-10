using LinqToDB.Mapping;
using LinqToDB.Reflection;

namespace SharedFunctionLib.Models;

[Table(Name = "program_settings")]
public class SimpleSettingModel
{
    // [PrimaryKey, Identity]
    // public int id;
    [Column(Name = "SettingKey")]
    public string SettingKey;
    [Column(Name = "SettingValue")]
    public string SettingValue;

}