namespace KnowledgeBaseLib.Models;

public class KnowledgeBaseRecord
{
    public string Title { get; private set; }
    
    public string Content { get; private set; }
    
    public string Tags { get; private set; }
    
    public string DataType { get; private set; }
        
    public KnowledgeBaseRecord(string title, string content, string tags, string dataType)
    {
        Title = title;
        Content = content;
        Tags = tags;
        DataType = dataType;
    }
        
    public override string ToString()
    {
        return $"Title: {Title}, Content: {Content}, Tags: {Tags}";
    }
}