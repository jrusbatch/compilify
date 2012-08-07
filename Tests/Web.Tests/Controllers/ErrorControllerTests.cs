using System.Web;
using System.Web.Mvc;
using Compilify.Web.Tests.Extensions;
using Moq;
using Xunit;

namespace Compilify.Web.Controllers.Tests
{
    public class ErrorControllerTests
    {
        [Fact]
        public void Index_returns_the_error_view()
        {
            var target = new ErrorController().WithContext(false);

            var result = target.Index() as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("Error", result.ViewName);
        }

        [Fact]
        public void Index_sets_status_code_to_500_if_parameter_is_not_a_valid_HTTP_status()
        {
            var mockResponse = new Mock<HttpResponseBase>();

            mockResponse.SetupProperty(x => x.StatusCode, 200);

            var target = new ErrorController().WithContext(false, mockResponse: mockResponse);

            var result = target.Index(0) as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("Error", result.ViewName);
            Assert.Equal(500, mockResponse.Object.StatusCode);
        }
    }
}
