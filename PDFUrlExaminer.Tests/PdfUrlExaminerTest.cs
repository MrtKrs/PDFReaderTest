using Moq;
using PdfUrlExaminer;
using URLReader;

namespace PDFUrlExaminer.Tests
{
    public class PdfUrlExaminerTest
    {
        private readonly string validBRnumber = "BR123";
        private readonly string validPrimaryUrl = "http://validPrimaryUrl.com/";
        private readonly string validSecondaryUrl = "http://validSecondaryUrl.com/";

        [Theory]
        [InlineData("BR123", "http://validPrimaryUrl.com/", "http://validSecondaryUrl.com/", 1, true, TestDisplayName = "Valid Primary url is GOOD")]
        [InlineData("BR123", "http://NOTvalidPrimaryUrl.com/", "http://validSecondaryUrl.com/", 2, true, TestDisplayName = "Valid Secondary url is GOOD")]
        [InlineData("BR123", "http://NOTvalidPrimaryUrl.com/", "http://NOTvalidSecondaryUrl.com/", 2, false, TestDisplayName = "NOT Valid urls is BAD")]
        public async Task PdfUrlExaminer_Reads_Valid_PDF_Correctly(string brNumber, string primaryUrl, string secondaryUrl, int expectedUrlCallTimes, bool expectedFileSave)
        {
            var urlReaderMock = GivenUrlReader();
            var fileWriterMock = GivenFileWriter();

            var sut = GetSut(urlReaderMock, fileWriterMock);
            var urlSets = new List<(string brNumber, string primaryUrl, string secondaryUrl)>
            {
                (brNumber, primaryUrl, secondaryUrl)
            };

            await sut.ExamineUrls(urlSets);

            urlReaderMock.Verify(r => r.ReadURL(It.IsAny<Uri>()), Times.Exactly(expectedUrlCallTimes));
            fileWriterMock.Verify(f => f.SaveFile(validBRnumber, It.IsAny<HttpContent>()), expectedFileSave ? Times.Once : Times.Never);
        }
        private Mock<IURLReader> GivenUrlReader()
        {
            var urlReaderMock = new Mock<IURLReader>(MockBehavior.Strict);
            urlReaderMock.Setup(r => r.ReadURL(It.IsAny<Uri>())).ReturnsAsync
                ((Uri x) => x.OriginalString.Equals(validPrimaryUrl, StringComparison.OrdinalIgnoreCase)
                        ? new PDFContentResult(true, new StringContent("Fake Primary PDF content"))
                        : x.OriginalString.Equals(validSecondaryUrl, StringComparison.OrdinalIgnoreCase)
                         ? new PDFContentResult(true, new StringContent("Fake Secondary PDF content"))
                            : new PDFContentResult(false, new StringContent("")) );
            return urlReaderMock;
        }
        private Mock<IFileWriter> GivenFileWriter()
        {
            var fileWriterMock = new Mock<IFileWriter>(MockBehavior.Strict);
            fileWriterMock.Setup(f => f.SaveFile(It.IsAny<string>(), It.IsAny<HttpContent>())).Returns(Task.CompletedTask);
            return fileWriterMock;
        }
        private PdfUrlExaminer.PdfUrlExaminer GetSut(Mock<IURLReader> urlReaderMock, Mock<IFileWriter> fileWriterMock) =>
            new PdfUrlExaminer.PdfUrlExaminer(urlReaderMock.Object, fileWriterMock.Object);
    }
}
