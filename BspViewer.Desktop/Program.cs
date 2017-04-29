
namespace BspViewer
{
    class Program
    {
        static void Main()
        {
            using (var viewer = new Viewer())
            {
                viewer.Run();
            }
        }
    }
}