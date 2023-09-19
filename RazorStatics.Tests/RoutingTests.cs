using Xunit;
using Xunit.Abstractions;

namespace RazorStatics.Tests
{
    public class RoutingTests
    {
        private readonly ITestOutputHelper _output;

        public RoutingTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData("http://localhost")] // root
        [InlineData("http://localhost/dilbert")] // path base
        public async Task RazorPage(string path)
        {
            using var ts = new TestScope(_output)
            {
                UsePathBase = true
            };
            var httpClient = ts.CreateClient();
            var response = await httpClient.GetStringAsync(path);

            Assert.Contains("<html", response, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("http://localhost/bundles/bob.js")] // root
        [InlineData("http://localhost/dilbert/bundles/bob.js")] // path base
        public async Task StaticFile(string path)
        {
            using var ts = new TestScope(_output)
            {
                UsePathBase = true
            };
            var httpClient = ts.CreateClient();
            var response = await httpClient.GetStringAsync(path);

            Assert.Contains("// this is bob", response, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("http://localhost/bundles/bob.js")] // root
        [InlineData("http://localhost/dilbert/bundles/bob.js")] // path base
        public async Task StaticFile_UsePathBaseNet7Workaround(string path)
        {
            using var ts = new TestScope(_output)
            {
                UsePathBase = true,
                UsePathBaseNet7Workaround = true
            }; 

            var httpClient = ts.CreateClient();
            var response = await httpClient.GetStringAsync(path);

            Assert.Contains("// this is bob", response, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("http://localhost/bundles/bob.js")] // root
        //[InlineData("http://localhost/dilbert/bundles/bob.js")] // path base
        public async Task StaticFile_NoPathBase(string path)
        {
            using var ts = new TestScope(_output);
            
            var httpClient = ts.CreateClient();
            var response = await httpClient.GetStringAsync(path);

            Assert.Contains("// this is bob", response, StringComparison.OrdinalIgnoreCase);
        }
    }
}
