// // using System.Data.SQLite;
// // using LinqToDB;
// // using LinqToDB.Data;
// // using LinqToDB.Mapping;
// // using SharedFunctionLib.Utils;
//
// // using SharedFunctionLib.Utils;
//
// using KnowledgeBaseLib.Models;
// using SharedFunctionLib.Utils;
// using SqliteConnection = Microsoft.Data.Sqlite.SqliteConnection;
//
// namespace KnowledgeBaseLib;
//
// public class Test
// {
//     
//     public static SqliteConnection open()
//     {
//         var knowledgeBaseReady = false;
//         
//         var extensionPath = Path.Combine(Ra3MapUtilsPathUtil.SimpleExtensionBasePath, "simple.dll");
//         var dictPath = Path.Combine(Ra3MapUtilsPathUtil.SimpleExtensionBasePath, "dict");
//         knowledgeBaseReady = (File.Exists(extensionPath) && Directory.Exists(dictPath));
//
//         if (!knowledgeBaseReady)
//         {
//             throw new Exception($"Cannot open {extensionPath}");
//         }
//         // var dataConnection = new DataConnection("SQLite", $"Data Source={Ra3MapUtilsPathUtil.KnowledgeBasePath};Version=3;");
//         // dataConnection.Execute($"SELECT load_extension('{extensionPath}');");
//         // dataConnection.Execute($"SELECT jieba_dict('{dictPath}');");
//         // return dataConnection;
//         var conn = new SqliteConnection($"Data Source={Ra3MapUtilsPathUtil.KnowledgeBasePath};");
//         // SQLitePCL.Batteries.Init();
//         conn.Open();
//         conn.LoadExtension(extensionPath);
//         conn.EnableExtensions(true);
//
//         var command = conn.CreateCommand();
//         command.CommandText = $"SELECT jieba_dict('{dictPath}');";
//         command.ExecuteNonQuery();
//         // Console.WriteLine(version);
//         return conn;
//     }
//
//     public static void add()
//     {
//         var conn = open();
//         var createTableCommand = conn.CreateCommand();
//         
//         // createTableCommand.CommandText = "CREATE VIRTUAL TABLE kb using fts5(title, content, tags,tokenize='simple')";
//         // createTableCommand.ExecuteNonQuery();
//         
//
//         var r1 = new KnowledgeBaseRecord(
//             "1-齐次坐标-平移",
//             """
//             :::success
//             作者: dreamness
//             
//             :::
//             
//             ![](https://cdn.nlark.com/yuque/0/2025/gif/34558013/1735747661067-aa39fe21-bad8-4365-baef-9f6e7da024f0.gif)
//             
//             ## 实现思路
//             在本例子中, 僚机在主机的左前方, 因此实现思路为
//             
//             1. 将僚机与主机重合
//             2. 相对主机, 僚机移动{30, 90, 0}
//             
//             ## ![](https://cdn.nlark.com/yuque/0/2025/png/34558013/1735748024875-6028fecf-27e1-4a34-a3fe-cdc0efce3f00.png)
//             ## 使用Lua库封装的函数实现:
//             ```lua
//             -- 以下代码需要每帧运行
//             
//             local main_unit_hc = main_unit:get_homogeneous_coordinates()
//             -- 将僚机的齐次坐标和主机一致, 可保证两者重合
//             left_sub_unit:set_homogeneous_coordinates(main_unit_hc)
//             -- 相对自己的坐标系进行平移
//             left_sub_unit:translate_relative({30, 90, 0})
//             ```
//             
//             
//             
//             ## (选看) 实现原理
//             以下公式实现了单位在全局坐标中平移了(x,y,z)
//             
//             ![image](https://cdn.nlark.com/yuque/__latex/2c0a21ea7ee3a08e4627282725f672e7.svg)
//             
//             但是在本例中, 不是相对于地图平移, 而是相对于主机的坐标进行平移了(x', y', z'), 因此需要将相对坐标(x', y', z')转为全局坐标(x, y, z):
//             
//             公式中_"由a组成"的矩阵_是主机的变换矩阵
//             
//             ![image](https://cdn.nlark.com/yuque/__latex/31dc3c4049e761ef2cc177882e88336f.svg)
//             
//             ```lua
//             -- 以下代码需要每帧运行
//             
//             local hc = main_unit:get_homogeneous_coordinates()
//             hc = MatrixUtil.dot(
//                 HomogeneousCoordinatesUtil.get_translation_matrix_by_vec(
//                     MatrixUtil.dot_vector(HomogeneousCoordinatesUtil.get_transform_matrix_by_hc(hc), {30, 90, 0})
//                 ), 
//                 hc
//             )
//             left_sub_unit:set_homogeneous_coordinates(hc)
//             
//             ```
//             
//             ## 
//             
//             """,
//             "教程,lua,日冕"
//         );
//
//         var insertCommand = conn.CreateCommand();
//         insertCommand.CommandText = "INSERT INTO kb (title, content, tags) VALUES (@title, @content, @tags)";
//         insertCommand.Parameters.AddWithValue("@title", r1.Title);
//         insertCommand.Parameters.AddWithValue("@content", r1.Content);
//         insertCommand.Parameters.AddWithValue("@tags", r1.Tags);
//
//         insertCommand.ExecuteNonQuery();
//
//         // var sqLiteCommand = new SQLiteCommand("INSERT INTO kb (title, content, tags) VALUES (@title, @content, @tags)", conn);
//         // sqLiteCommand.Parameters.AddWithValue("@title", r1.Title);
//         // sqLiteCommand.Parameters.AddWithValue("@content", r1.Content);
//         // sqLiteCommand.Parameters.AddWithValue("@tags", r1.Tags);
//         // sqLiteCommand.ExecuteNonQuery();
//         
//         conn.Close();
//     }
//     
//     public static void search()
//     {
//         var conn = open();
//
//         var queryCommand = conn.CreateCommand();
//         queryCommand.CommandText = "SELECT * FROM kb WHERE content MATCH simple_query('平移 僚机')";
//         // queryCommand.CommandText = "SELECT count(1) as cnt FROM kb";
//         var reader = queryCommand.ExecuteReader();
//         
//         var results = new List<KnowledgeBaseRecord>();
//         while (reader.Read())
//         {
//             // Console.WriteLine(reader.GetString(0));
//             var title = reader["title"].ToString();
//             var content = reader["content"].ToString();
//             var tags = reader["tags"].ToString();
//             results.Add(new KnowledgeBaseRecord(title, content, tags));
//         }
//     
//         foreach (var r in results)
//         {
//             Console.WriteLine(r);
//         }
//         
//         conn.Close();
//     }
// }