using Microsoft.Net.Http.Headers;
using Moq;
using System.Net;
using System.Net.Mime;
using System.Numerics;
using System.Text;
using URLReader;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace PDFReader.IntegrationTests
{
    
    public class URLReaderIntegrationTest : IDisposable
    {

        private readonly WireMockServer stub;
        private readonly string baseUrl;
        //private readonly HttpClient _httpClient;
        public URLReaderIntegrationTest() {
            var port = new Random().Next(5000, 6000);
            baseUrl = "http://localhost:" + port;

            stub = WireMockServer.Start(new WireMockServerSettings
            {
                Urls = new[] { "http://+:" + port },
                ReadStaticMappings = true
            }); ;
        }
        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                stub.Stop();
                stub.Dispose();
            }
        }

        [Fact]
        public async Task UrlReader_ConnectsAndReadsContent()//(HttpStatusCode withResponseHttpStatusCode, string withResponseMediaType, bool expectSuccess, string expectedContentString)
        {
            var resp = "Some response body";
            var path = "/pdfstorage/file.pdf";
            stub
    .Given(
        Request.Create()
            .UsingGet()
            .WithPath(path))
    .RespondWith(
        Response.Create()
            .WithStatusCode(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, MediaTypeNames.Application.Pdf )
            .WithBody(resp));

            var sut = GetSut();
            var result = await sut.ReadURL(new Uri($"{baseUrl}{path}"));

            Assert.NotNull(result);
            Assert.True(result.Success);
            var content = await result.PDFContent.ReadAsStringAsync();
            Assert.Equal(resp, content);
        }

        private URLReader.URLReader GetSut()//(HttpStatusCode withResponseHttpStatusCode, string withResponseMediaType)
        {
            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(stub.Urls[0])
            };

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
            return new URLReader.URLReader(httpClientFactoryMock.Object);
        }

    }
}
