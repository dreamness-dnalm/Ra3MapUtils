using LinqToDB.Mapping;

namespace SharedFunctionLib.Models;

[Table(Name = "nano_program_meta")]
public class SimpleNanoProgramMetaModel
{
    [Column(Name = "ID")]
    public string ID { get; set; }
    [Column(Name = "IsEnabled")]
    public int IsEnabled { get; set; }
    [Column(Name = "IsWbVisible")]
    public int IsWbVisible { get; set; }
    [Column(Name = "OrderNum")]
    public int Order { get; set; } = -1;

    public override string ToString()
    {
        return "SimpleNanoProgramMetaModel{" +
               $"ID='{ID}', " +
               $"IsEnabled={IsEnabled}, " +
               $"IsWbVisible={IsWbVisible}, " +
               $"Order={Order}" +
               "}";
    }
}