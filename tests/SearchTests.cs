using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Victoria.Interfaces;
using Victoria.Responses.Search;
using Victoria.Tests.Objects;

namespace Victoria.Tests {
    [TestClass]
    public class SearchTests {
        private readonly ILavaNode _lavaNode
            = Global.Provider.GetRequiredService<ILavaNode>();

        [DataTestMethod]
        [DataRow("The Weeknd Often")]
        [DataRow("Logic Under Pressure")]
        [DataRow("lil uzi vert - xo tour life + p2 [best transition]")]
        [DataRow("No Role Modelz")]
        public async Task SearchYouTubeAsync(string query) {
            var searchResponse = await _lavaNode.SearchAsync(SearchType.YouTube, query);

            Assert.IsNotNull(searchResponse);
            Assert.IsNotNull(searchResponse.Exception, "searchResponse.Exception != null");
            Assert.IsNotNull(searchResponse.Playlist, "searchResponse.SearchPlaylist != null");
            Assert.IsNotNull(searchResponse.Tracks, "searchResponse.Tracks != null");
            Assert.IsNotNull(searchResponse.Status, "searchResponse.Status != null");
        }

        [DataTestMethod]
        [DataRow("https://www.youtube.com/watch?v=Rnd1itAw-Iw")]
        [DataRow("https://www.youtube.com/watch?v=7OtHA_Hvho4")]
        [DataRow("https://soundcloud.com/lil-baby-4pf/on-me")]
        [DataRow("https://soundcloud.com/morgan-smith-411229991/sets/morgans-hot-playlist")]
        [DataRow("https://www.youtube.com/watch?v=-S7nEOS1-84&list=RD-S7nEOS1-84&start_radio=1")]
        [DataRow("https://www.youtube.com/watch?v=JH398xAYpZA&list=OLAK5uy_lwaD8UXRautA8W9eWT4zZOvwf5Ktxpax8")]
        public async Task SearchDirectLinkAsync(string query) {
            var searchResponse = await _lavaNode.SearchAsync(SearchType.Direct, query);

            Assert.IsNotNull(searchResponse);
            Assert.IsNotNull(searchResponse.Exception, "searchResponse.Exception != null");
            Assert.IsNotNull(searchResponse.Playlist, "searchResponse.SearchPlaylist != null");
            Assert.IsNotNull(searchResponse.Tracks, "searchResponse.Tracks != null");
            Assert.IsNotNull(searchResponse.Status, "searchResponse.Status != null");
        }



        [DataTestMethod]
        [DataRow("South Side Suicide")]
        [DataRow("Drake Passionfruit")]
        public async Task IsTypeOfILavaTrack(string query) {
            var searchResponse = await _lavaNode.SearchAsync(SearchType.YouTube, query);
            if (searchResponse.Status == SearchStatus.LoadFailed || searchResponse.Status == SearchStatus.NoMatches) {
                Assert.Fail("Failed search response.");
            }

            var lavaTrack = searchResponse.Tracks.FirstOrDefault();
            var customTrack = new CustomTrack(lavaTrack) {
                Requester = ulong.MaxValue
            };

            Assert.IsInstanceOfType(customTrack, typeof(ILavaTrack));
            Assert.IsInstanceOfType(customTrack, typeof(AbstractLavaTrack));
            Assert.IsNotNull(customTrack);
            Assert.IsNotNull(customTrack.Requester);
            Assert.IsTrue(customTrack.Requester is ulong.MaxValue);
        }
    }
}