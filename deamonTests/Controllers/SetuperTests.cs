using System.IO.Abstractions;
using deamon;
using Moq;

namespace deamonTests.Controllers;

[TestFixture]
public class SetuperTests
{
    private Mock<IFileSystem> _fileSystemMock;
    private Setuper _setuper;

    [SetUp]
    public void SetUp()
    {
        _fileSystemMock = new Mock<IFileSystem>();
        _setuper = new Setuper(_fileSystemMock.Object);
        
        Environment.SetEnvironmentVariable("AppFolder", "C:\\Users\\TestUser\\AppData\\Roaming");
    }

    [Test]
    public void Setup_ShouldCreateAppDirectory_IfNotExists()
    {
        // Arrange
        string appDir = "C:\\Users\\TestUser\\AppData\\Roaming\\Firefly-Displays";
        _fileSystemMock.Setup(fs => fs.Directory.Exists(appDir)).Returns(false);

        foreach  (string entity in new [] {"Content", "Display", "Queue", "QueueTriggerPair", "SchedulerEntity"})
        {
            string filePath = Path.Combine(appDir, entity + ".json");
            _fileSystemMock.Setup(fs => fs.File.Exists(filePath)).Returns(false);
        }

        // Act
        _setuper.Setup();

        // Assert
        _fileSystemMock.Verify(fs => fs.Directory.CreateDirectory(appDir), Times.Once);
    }

}