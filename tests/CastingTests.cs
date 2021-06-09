using Microsoft.VisualStudio.TestTools.UnitTesting;
using Victoria.Interfaces;
using Victoria.Tests.Objects;

namespace Victoria.Tests {
    [TestClass]
    public sealed class CastingTests {
        [TestMethod]
        public void IsLavaTrack() {
            Assert.IsInstanceOfType(new CustomTrack(default), typeof(ILavaTrack));
        }
    }
}