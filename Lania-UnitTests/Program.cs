using Xunit;

namespace Lania_UnitTests
{
    public class Program
    {
        [Fact]
        public void ToKatakana()
        {
            Assert.True(Lania.Program.IsImage("test.png"));
        }
    }
}
