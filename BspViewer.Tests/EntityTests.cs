using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace BspViewer.Tests
{
    [TestClass]
    public class EntityTests
        : TestBase
    {
        [TestMethod]
        [DataTestMethod]
        [DataRow("Entities.ValidEntites.txt")]
        public void TestReadValidEntities(string fileName)
        {
            // Given
            var validEntities = GetFileContents(fileName);
            var entitiesData = Encoding.ASCII.GetBytes(validEntities);

            // When
            var parser = new EntityParser(entitiesData);
            var entities = parser.Parse();

            // Then
            Assert.IsNotNull(entities);
            Assert.IsTrue(entities.Count == 114);
        }

        [TestMethod]
        [DataTestMethod]
        [DataRow("Entities.InvalidEmptyEntity.txt")]
        [DataRow("Entities.InvalidEntity.txt")]
        public void TestReadInvalidEntity(string fileName)
        {
            // Given
            var invalidEntity = GetFileContents(fileName);
            var entityData = Encoding.ASCII.GetBytes(invalidEntity);

            // When
            var parser = new EntityParser(entityData);
            var entities = parser.Parse();

            // Then
            Assert.IsNotNull(entities);
            Assert.IsTrue(entities.Count == 0);
        }
    }
}