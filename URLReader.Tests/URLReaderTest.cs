using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;

namespace URLReader.Tests
{
    public class URLReaderTest
    {
        [Theory]
        [InlineData(HttpStatusCode.OK, MediaTypeNames.Application.Pdf, true, "Test content", TestDisplayName="HTTP 200 File PDF is good")]
        [InlineData(HttpStatusCode.Created, MediaTypeNames.Application.Pdf, true, "Test content", TestDisplayName = "HTTP 201 File PDF is good")]
        [InlineData(HttpStatusCode.Accepted, MediaTypeNames.Application.Pdf, true, "Test content", TestDisplayName = "HTTP 202 File PDF is good")]

        [InlineData(HttpStatusCode.Redirect, MediaTypeNames.Application.Pdf, false, "", TestDisplayName = "HTTP 302 File PDF is bad")]
        [InlineData(HttpStatusCode.BadRequest, MediaTypeNames.Application.Pdf, false, "", TestDisplayName = "HTTP 400 File PDF is bad")]
        [InlineData(HttpStatusCode.Unauthorized, MediaTypeNames.Application.Pdf, false, "", TestDisplayName = "HTTP 401 File PDF is bad")]
        [InlineData(HttpStatusCode.Forbidden, MediaTypeNames.Application.Pdf, false, "", TestDisplayName = "HTTP 403 File PDF is bad")]
        [InlineData(HttpStatusCode.NotFound, MediaTypeNames.Application.Pdf, false, "", TestDisplayName = "HTTP 404 File PDF is bad")]
        [InlineData(HttpStatusCode.InternalServerError, MediaTypeNames.Application.Pdf, false, "", TestDisplayName = "HTTP 500 File PDF is bad")]
        [InlineData(HttpStatusCode.BadGateway, MediaTypeNames.Application.Pdf, false, "", TestDisplayName = "HTTP 502 File PDF is bad")]

        [InlineData(HttpStatusCode.OK, MediaTypeNames.Application.Json, false, "", TestDisplayName = "HTTP OK File Json is bad")]
        [InlineData(HttpStatusCode.OK, MediaTypeNames.Application.Octet, false, "", TestDisplayName = "HTTP OK File Octet is bad")]
        [InlineData(HttpStatusCode.OK, MediaTypeNames.Text.Html, false, "", TestDisplayName = "HTTP OK File Html is bad")]
        [InlineData(HttpStatusCode.OK, MediaTypeNames.Text.Xml, false, "", TestDisplayName = "HTTP OK File Xml is bad")]
        [InlineData(HttpStatusCode.OK, MediaTypeNames.Multipart.FormData, false, "", TestDisplayName = "HTTP OK File FormData is bad")]
        [InlineData(HttpStatusCode.OK, MediaTypeNames.Image.Gif, false, "", TestDisplayName = "HTTP OK File Gif is bad")]
        [InlineData(HttpStatusCode.OK, MediaTypeNames.Image.Jpeg, false, "", TestDisplayName = "HTTP OK File Jpeg is bad")]
        [InlineData(HttpStatusCode.OK, MediaTypeNames.Image.Png, false, "", TestDisplayName = "HTTP OK File Png is bad")]
        [InlineData(HttpStatusCode.OK, MediaTypeNames.Image.Icon, false, "", TestDisplayName = "HTTP OK File Icon is bad")]
        [InlineData(HttpStatusCode.OK, MediaTypeNames.Image.Bmp, false, "", TestDisplayName = "HTTP OK File Bmp is bad")]
        [InlineData(HttpStatusCode.OK, MediaTypeNames.Image.Tiff, false, "", TestDisplayName = "HTTP OK File Tiff is bad")]
        [InlineData(HttpStatusCode.OK, MediaTypeNames.Image.Svg, false, "", TestDisplayName = "HTTP OK File SVG is bad")]
        public async Task UrlReader_Only_Reads_PDF_Content(HttpStatusCode withResponseHttpStatusCode, string withResponseMediaType, bool expectSuccess, string expectedContentString)
        {
            var sut = GetSut(withResponseHttpStatusCode, withResponseMediaType);

            var result = await sut.ReadURL(new Uri("http://fakeUriForTest.com"));

            Assert.NotNull(result);
            Assert.Equal(expectSuccess, result.Success);

            var contentString = await result.PDFContent.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Equal(expectedContentString, contentString);

        }
        private URLReader GetSut(HttpStatusCode withResponseHttpStatusCode, string withResponseMediaType)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               // Setup the PROTECTED method to mock
               .Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               // prepare the expected response of the mocked http call
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = withResponseHttpStatusCode,
                   Content = new StringContent("Test content",
                                                Encoding.UTF8,
                                                withResponseMediaType),
               })
               .Verifiable();
            // use real http client with mocked handler here
            var httpClient = new HttpClient(handlerMock.Object);
            
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
            return new URLReader(httpClientFactoryMock.Object);
        }

        //[Fact]
        //public async Task UrlReader_Gets_Valid_Content_For_Realz()
        //{
        //    var sut = GetSutFoRealz();
        //
        //    var result = await sut.ReadURL(new Uri("http://arpeissig.at/wp-content/uploads/2016/02/D7_NHB_ARP_Final_2.pdf"));
        //
        //    Assert.NotNull(result);
        //
        //    var contentString = await result.PDFContent.ReadAsStringAsync();
        //    Assert.NotNull (contentString);
        //}
        //private URLReader GetSutFoRealz()
        //{
        //    var services = new ServiceCollection();
        //    services.AddHttpClient("pdfClient", x => { x.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/pdf")); });
        //    var actualHttpClientFactory = services
        //                                  .BuildServiceProvider()
        //                                  .GetRequiredService<IHttpClientFactory>();
        //
        //    return new URLReader(actualHttpClientFactory);
        //}
    }
}
