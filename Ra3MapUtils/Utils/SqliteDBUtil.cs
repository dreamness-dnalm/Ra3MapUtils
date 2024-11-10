// using Microsoft.Data.Sqlite;
//
// namespace Ra3MapUtils.Utils;
//
// public class SqliteDBUtil
// {
//     private static string connStr =  "Data Source=data.db";
//
//     public static void Execute(string sql)
//     {
//         using (var conn = new SqliteConnection(connStr))
//         {
//             conn.Open();
//             using (var cmd = conn.CreateCommand())
//             {
//                 cmd.CommandText = sql;
//                 cmd.ExecuteNonQuery();
//             }
//         }
//     }
//
//     public static void Execute(string sql, Dictionary<string, object> parameters)
//     {
//         using (var conn = new SqliteConnection(connStr))
//         {
//             conn.Open();
//             using (var cmd = conn.CreateCommand())
//             {
//                 cmd.CommandText = sql;
//                 foreach (var param in parameters)
//                 {
//                     cmd.Parameters.AddWithValue(param.Key, param.Value);
//                 }
//                 cmd.ExecuteNonQuery();
//             }
//         }
//     }
//
//     public static List<Dictionary<string, object>> Query(string sql)
//     {
//         var result = new List<Dictionary<string, object>>();
//         using (var conn = new SqliteConnection(connStr))
//         {
//             conn.Open();
//             using (var cmd = conn.CreateCommand())
//             {
//                 cmd.CommandText = sql;
//                 using (var reader = cmd.ExecuteReader())
//                 {
//                     while (reader.Read())
//                     {
//                         var row = new Dictionary<string, object>();
//                         for (var i = 0; i < reader.FieldCount; i++)
//                         {
//                             row[reader.GetName(i)] = reader.GetValue(i);
//                         }
//                         result.Add(row);
//                     }
//                 }
//             }
//         }
//         return result;
//     }
//
//     public static List<Dictionary<string, object>> Query(string sql, Dictionary<string, object> parameters)
//     {
//         var result = new List<Dictionary<string, object>>();
//         using (var conn = new SqliteConnection(connStr))
//         {
//             conn.Open();
//             using (var cmd = conn.CreateCommand())
//             {
//                 cmd.CommandText = sql;
//                 foreach (var param in parameters)
//                 {
//                     cmd.Parameters.AddWithValue(param.Key, param.Value);
//                 }
//
//                 using (var reader = cmd.ExecuteReader())
//                 {
//                     while (reader.Read())
//                     {
//                         var row = new Dictionary<string, object>();
//                         for (var i = 0; i < reader.FieldCount; i++)
//                         {
//                             row[reader.GetName(i)] = reader.GetValue(i);
//                         }
//                         result.Add(row);
//                     }
//                 }
//             }
//         }
//         return result;
//     }
//
//     public static List<T> Query<T>(string sql, Func<Dictionary<string, object>, T> rowMapper)
//     {
//         var result = new List<T>();
//         using (var conn = new SqliteConnection(connStr))
//         {
//             conn.Open();
//             using (var cmd = conn.CreateCommand())
//             {
//                 cmd.CommandText = sql;
//                 using (var reader = cmd.ExecuteReader())
//                 {
//                     while (reader.Read())
//                     {
//                         var row = new Dictionary<string, object>();
//                         for (var i = 0; i < reader.FieldCount; i++)
//                         {
//                             row[reader.GetName(i)] = reader.GetValue(i);
//                         }
//                         result.Add(rowMapper(row));
//                     }
//                 }
//             }
//         }
//         return result;
//     }
//
//     public static List<T> Query<T>(string sql, Dictionary<string, object> parameters,
//         Func<Dictionary<string, object>, T> rowMapper)
//     {
//         var result = new List<T>();
//         using (var conn = new SqliteConnection(connStr))
//         {
//             conn.Open();
//             using (var cmd = conn.CreateCommand())
//             {
//                 cmd.CommandText = sql;
//                 foreach (var param in parameters)
//                 {
//                     cmd.Parameters.AddWithValue(param.Key, param.Value);
//                 }
//
//                 using (var reader = cmd.ExecuteReader())
//                 {
//                     while (reader.Read())
//                     {
//                         var row = new Dictionary<string, object>();
//                         for (var i = 0; i < reader.FieldCount; i++)
//                         {
//                             row[reader.GetName(i)] = reader.GetValue(i);
//                         }
//                         result.Add(rowMapper(row));
//                     }
//                 }
//             }
//         }
//         return result;
//     }
// }