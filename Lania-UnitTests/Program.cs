using Xunit;

namespace Lania_UnitTests
{
    public class Program
    {
        [Fact]
        public void Test()
        {
            Assert.True(Lania.Program.IsImage("test.png"));
        }
    }
}
