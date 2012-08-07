using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;

namespace Compilify.Web.Tests.Extensions
{
    internal static class MoqExtensions
    {
        internal static TController WithContext<TController>(
            this TController controller,
            bool isAjaxRequest,
            Mock<HttpServerUtilityBase> mockServerUtility = null,
            Mock<HttpResponseBase> mockResponse = null,
            Mock<HttpRequestBase> mockRequest = null,
            Mock<HttpSessionStateBase> mockSession = null,
            RouteData routeData = null)
            where TController : ControllerBase
        {
            var server = mockServerUtility ?? new Mock<HttpServerUtilityBase>(MockBehavior.Loose);
            var response = mockResponse ?? new Mock<HttpResponseBase>(MockBehavior.Loose);
            var request = mockRequest ?? new Mock<HttpRequestBase>(MockBehavior.Loose);

            request.Setup(x => x["X-Requested-With"]).Returns(isAjaxRequest ? "XMLHttpRequest" : null);

            var session = mockSession ?? new Mock<HttpSessionStateBase>();
            session.Setup(s => s.SessionID).Returns(Guid.NewGuid().ToString());
 
            var mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.SetupGet(c => c.Request).Returns(request.Object);
            mockHttpContext.SetupGet(c => c.Response).Returns(response.Object);
            mockHttpContext.SetupGet(c => c.Server).Returns(server.Object);
            mockHttpContext.SetupGet(c => c.Session).Returns(session.Object);

            controller.ControllerContext =
                new ControllerContext(mockHttpContext.Object, routeData ?? new RouteData(), controller);

            return controller;
        }
    }
}
