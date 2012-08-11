using System;
using System.Web.Mvc;
using Compilify.LanguageServices;
using Compilify.Models;
using Compilify.Web.Models;
using Compilify.Web.Tests.Extensions;
using Moq;
using Xunit;

namespace Compilify.Web.Controllers.Tests
{
    public class HomeControllerTests
    {
        [Fact]
        public void Can_be_created()
        {
            var repository = Mock.Of<IPostRepository>();
            var validator = Mock.Of<ICodeValidator>();

            var target = new HomeController(repository, validator);

            Assert.NotNull(target);
        }
        
        [Fact]
        public void Ctor_throws_when_IPostRepository_is_null()
        {
            var validator = Mock.Of<ICodeValidator>();

            Assert.Throws<ArgumentNullException>(() => new HomeController(null, validator));
        }
        
        [Fact]
        public void Ctor_throws_when_ICodeValidator_is_null()
        {
            var repository = Mock.Of<IPostRepository>();

            Assert.Throws<ArgumentNullException>(() => new HomeController(repository, null));
        }

        [Fact]
        public void Index_returns_the_Show_view()
        {
            var repository = Mock.Of<IPostRepository>();
            var validator = Mock.Of<ICodeValidator>();

            var controller = new HomeController(repository, validator);

            var result = controller.Index() as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<PostViewModel>(result.Model);
        }
        
        [Fact]
        public void About_returns_the_About_view()
        {
            var repository = Mock.Of<IPostRepository>();
            var validator = Mock.Of<ICodeValidator>();

            var controller = new HomeController(repository, validator);

            var result = controller.About() as ViewResult;

            Assert.NotNull(result);
        }
        
        [Fact]
        public void Show_redirects_if_version_is_less_than_1()
        {
            const string Slug = "slug";

            var repository = Mock.Of<IPostRepository>();
            var validator = Mock.Of<ICodeValidator>();

            var controller = new HomeController(repository, validator);

            var result = controller.Show(Slug, 0) as RedirectToRouteResult;

            Assert.NotNull(result);
            Assert.Equal(Slug, result.RouteValues[Slug]);
        }
        
        [Fact]
        public void Show_redirects_if_version_is_equal_to_1()
        {
            const string Slug = "slug";

            var repository = Mock.Of<IPostRepository>();
            var validator = Mock.Of<ICodeValidator>();

            var controller = new HomeController(repository, validator);

            var result = controller.Show(Slug, 1) as RedirectToRouteResult;

            Assert.NotNull(result);
            Assert.Equal(Slug, result.RouteValues[Slug]);
        }

        [Fact]
        public void Show_sets_version_to_1_when_not_provided()
        {
            const string Slug = "slug";

            var validator = Mock.Of<ICodeValidator>();
            var mockRepository = new Mock<IPostRepository>();

            mockRepository.Setup(x => x.GetVersion(Slug, 1)).Returns(new Post());

            var controller = new HomeController(mockRepository.Object, validator).WithContext(false);
            
            var result = controller.Show(Slug, null) as ViewResult;

            Assert.NotNull(result);

            var viewModel = result.Model as PostViewModel;

            Assert.NotNull(viewModel);
            mockRepository.Verify(x => x.GetVersion(Slug, 1), Times.Once());
        }
        
        [Fact]
        public void Show_returns_the_Error_view_when_the_post_could_not_be_found()
        {
            var validator = Mock.Of<ICodeValidator>();
            var mockRepository = new Mock<IPostRepository>();

            mockRepository.Setup(x => x.GetVersion(It.IsAny<string>(), It.IsAny<int>())).Returns<Post>(null);

            var controller = new HomeController(mockRepository.Object, validator).WithContext(false);
            
            var result = controller.Show(null, null) as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("Error", result.ViewName);
        }

        [Fact]
        public void Show_returns_Json_when_responding_to_ajax_requests()
        {
            var validator = Mock.Of<ICodeValidator>();
            var mockRepository = new Mock<IPostRepository>();

            mockRepository.Setup(x => x.GetVersion(It.IsAny<string>(), It.IsAny<int>())).Returns(new Post());

            var controller = new HomeController(mockRepository.Object, validator).WithContext(true);

            var result = controller.Show(null, null) as JsonResult;

            Assert.NotNull(result);
        }

        [Fact]
        public void Latest_redirects_to_Show_when_post_is_found()
        {
            var validator = Mock.Of<ICodeValidator>();
            var mockRepository = new Mock<IPostRepository>();

            mockRepository.Setup(x => x.GetLatestVersion(It.IsAny<string>())).Returns(2);

            var controller = new HomeController(mockRepository.Object, validator).WithContext(false);

            var result = controller.Latest(null) as RedirectToRouteResult;

            Assert.NotNull(result);
            Assert.Equal(2, result.RouteValues["version"]);
        }

        [Fact]
        public void Latest_returns_the_Error_view_when_the_post_could_not_be_found()
        {
            var validator = Mock.Of<ICodeValidator>();
            var mockRepository = new Mock<IPostRepository>();

            mockRepository.Setup(x => x.GetLatestVersion(It.IsAny<string>())).Returns(0);

            var controller = new HomeController(mockRepository.Object, validator).WithContext(false);

            var result = controller.Latest(null) as ViewResult;
            
            Assert.NotNull(result);
            Assert.Equal("Error", result.ViewName);
        }
        
        [Fact]
        public void Save_returns_redirect_to_Show()
        {
            var postViewModel = new PostViewModel();
            var validator = Mock.Of<ICodeValidator>();
            var mockRepository = new Mock<IPostRepository>();

            mockRepository.Setup(x => x.Save(It.IsAny<string>(), It.IsAny<Post>())).Returns<string, Post>((s, x) => x);

            var controller = new HomeController(mockRepository.Object, validator);

            var result = controller.Save(null, postViewModel) as RedirectToRouteResult;
            
            Assert.NotNull(result);
        }
        
        [Fact]
        public void Validate_returns_JsonResult()
        {
            var viewModel = new PostViewModel();
            var mockValidator = new Mock<ICodeValidator>();
            var repository = Mock.Of<IPostRepository>();

            mockValidator.Setup(x => x.GetCompilationErrors(It.IsAny<Post>()));

            var controller = new HomeController(repository, mockValidator.Object);

            var result = controller.Validate(viewModel) as JsonResult;
            
            Assert.NotNull(result);
            mockValidator.Verify(x => x.GetCompilationErrors(It.IsAny<Post>()), Times.Once());
        }
    }
}
