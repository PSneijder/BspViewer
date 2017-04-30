using System.IO;
using System.Reflection;

namespace BspViewer.Tests
{
    public abstract class TestBase
    {
        protected string GetFileContents(string sampleFile)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resource = string.Format("BspViewer.Tests.TestData.{0}", sampleFile);

            using (var stream = assembly.GetManifestResourceStream(resource))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }

            return string.Empty;
        }
    }
}