using System.IO;

namespace Ra3MapUtils.Utils;

public static class EmbeddedResourcesUtil
{
    public static string GetEmbeddedResourceContent(string resourceName)
    {
        using (var stream = GetEmbeddedResourceStream(resourceName))
        {
            using (var reader = new System.IO.StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }

    public static Stream GetEmbeddedResourceStream(string resourceName)
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceStream(resourceName);
    }
}