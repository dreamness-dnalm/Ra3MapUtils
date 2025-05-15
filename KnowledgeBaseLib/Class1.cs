
using JiebaNet.Integration.LuceneNet;
using JiebaNet.Segmenter;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Version = Lucene.Net.Util.Version;

namespace KnowledgeBaseLib;

public class Class1
{
    public static void init_index()
    {
        var indexDirectory = @"H:\tmp\index";
        var dir = FSDirectory.Open(indexDirectory);
        var analyzer = new JiebaAnalyzer();
        using var writer = new IndexWriter(dir, analyzer, IndexWriter.MaxFieldLength.UNLIMITED);


        var doc = new Document();
        doc.Add(new Field("title", "快速的棕色狐狸跳过懒狗", Field.Store.YES, Field.Index.ANALYZED));
        doc.Add(new Field("category", "animal_story", Field.Store.YES, Field.Index.NOT_ANALYZED));
        doc.Add(new Field("publish_date", DateTime.UtcNow.ToString("o"), Field.Store.YES, Field.Index.NOT_ANALYZED));
        doc.Add(new Field("view_count", "12345", Field.Store.YES, Field.Index.NOT_ANALYZED));

        writer.AddDocument(doc);
    }
    
    private static Query ParseQuery(string searchQuery, QueryParser parser)
    {
        Query query;
        try
        {
            query = parser.Parse(searchQuery.Trim());
        }
        catch (ParseException pe)
        {
            query = parser.Parse(QueryParser.Escape(searchQuery.Trim() + "*"));
        }

        return query;
    }

    public static void query()
    {
        var indexDirectory = @"H:\tmp\index";
        var dir = FSDirectory.Open(indexDirectory);
        var analyzer = new JiebaAnalyzer();

        var parser = new QueryParser(Version.LUCENE_30, "title", analyzer);
        var query = ParseQuery("狗", parser);
        using var searcher = new IndexSearcher(dir, true);
        var scoreDocs = searcher.Search(query, 10).ScoreDocs;
        foreach (var scoreDoc in scoreDocs)
        {
            var doc = searcher.Doc(scoreDoc.Doc);
            var title = doc.Get("title");
            var category = doc.Get("category");
            var publishDate = doc.Get("publish_date");
            var viewCount = doc.Get("view_count");

            Console.WriteLine($"Title: {title}, Category: {category}, Publish Date: {publishDate}, View Count: {viewCount}");
        }
    }
}