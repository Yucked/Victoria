using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Victoria.Tests;

[TestClass]
public class RestTests {
    [DataRow("https://www.youtube.com/watch?v=XXYlFuWEuKI")]
    [DataRow("https://www.youtube.com/watch?v=J7e70aw_x_E")]
    [DataRow("https://www.youtube.com/shorts/POa3-kPYlSo")]
    [DataRow("https://www.youtube.com/watch?v=l9cmyFG6fwU&list=OLAK5uy_lFg2GlZVx-emFsB2ScdFc5y5v2EEy-Ijk")]
    [DataRow("https://soundcloud.com/futureisnow/future-feat-drake-tems-wait")]
    [DataRow("ytsearch:The Weeknd Valeria")]
    [DataTestMethod]
    public async Task SearchAsync(string search) {
        var searchResponse = await Globals.Node.LoadTrackAsync(search);
        Assert.IsNotNull(searchResponse);
        Assert.IsNull(searchResponse.Exception.Message);
    }

    [TestMethod]
    public async Task VerfiyVersionAsync() {
        var version = await Globals.Node.GetLavalinkVersion();
        Assert.IsNotNull(version);
    }
}