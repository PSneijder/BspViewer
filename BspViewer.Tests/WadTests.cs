using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace BspViewer.Tests
{
    [TestClass]
    public class WadTests
        : TestBase
    {
         // This TestClass consists more of IntegrationTests then UnitTests.
         // We should refactor this.

        [TestMethod]
        public void TestExportTextureToBitmap()
        {
            // Given
            var fileName = @"C:\Sierra\Half-Life\valve\maps\hldemo1.bsp";
            var wadFileNames = new string[] { @"C:\Sierra\Half-Life\valve\halflife.wad" };

            var loader = new BspLoader(fileName);
            var map = loader.Load();

            var wadLoader = new WadLoader(map, fileName, wadFileNames);
            var textures = wadLoader.Load();

            if (!Directory.Exists("Textures"))
            {
                Directory.CreateDirectory("Textures");
            }

            // When
            foreach (var texture in textures)
            {
                using (var bitmap = TexUtil.PixelsToTexture(texture.TextureData, texture.Width, texture.Height, 4))
                {
                    bitmap.Save("Textures\\" + texture.Name + ".bmp");
                }
            }

            // Then
            foreach (var texture in textures)
            {
                var isExisting = File.Exists("Textures\\" + texture.Name + ".bmp");

                Assert.IsTrue(isExisting);
            }
        }
    }
}