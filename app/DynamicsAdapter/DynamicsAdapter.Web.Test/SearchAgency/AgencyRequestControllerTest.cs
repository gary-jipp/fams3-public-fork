﻿using DynamicsAdapter.Web.SearchAgency;
using DynamicsAdapter.Web.SearchAgency.Models;
using Fams3Adapter.Dynamics.SearchRequest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace DynamicsAdapter.Web.Test.SearchAgency
{
    public class AgencyRequestControllerTest
    {
        private AgencyRequestController _sut;
        private Mock<ILogger<AgencyRequestController>> _loggerMock;
        private Mock<IAgencyRequestService> _agencyRequestServiceMock;

        [SetUp]
        public void Init()
        {
            _loggerMock = new Mock<ILogger<AgencyRequestController>>();
            _agencyRequestServiceMock = new Mock<IAgencyRequestService>();

            _sut = new AgencyRequestController(_loggerMock.Object, _agencyRequestServiceMock.Object);
        }

        [Test]
        public async Task With_valid_searchRequestOrdered_it_should_return_ok_with_correct_content()
        {
            SearchRequestOrdered validSearchRequestOrdered = new SearchRequestOrdered()
            {
                Action = RequestAction.NEW,
                RequestId = "121212121212",
                TimeStamp = DateTime.Now,
                Person = new Person()
                {
                    Agency = new Agency()
                    {
                        RequestId = "121212121212",
                        Code = "FMEP"
                    }
                }
            };

            _agencyRequestServiceMock.Setup(x => x.ProcessSearchRequestOrdered(It.IsAny<SearchRequestOrdered>()))
                .Returns(Task.FromResult<SSG_SearchRequest>( new SSG_SearchRequest() { 
                    FileId="fileId",
                    SearchRequestId = Guid.NewGuid()
                }));

            var result = await _sut.CreateSearchRequest("normalsearchRequest", validSearchRequestOrdered);

            _agencyRequestServiceMock.Verify(x => x.ProcessSearchRequestOrdered(It.IsAny<SearchRequestOrdered>()), Times.Once);
            var resultValue = result as OkObjectResult;
            Assert.NotNull(resultValue);
            var submitted = resultValue.Value as SearchRequestSubmitted;
            Assert.NotNull(submitted);
            Assert.AreEqual("fileId", submitted.SearchRequestKey);
            Assert.AreEqual("FMEP", submitted.ProviderProfile.Name);
            Assert.AreEqual("The new Search Request reference: 121212121212 has been submitted successfully.", submitted.Message);
        }

        [Test]
        public async Task With_null_key_searchRequestOrdered_it_should_return_BadRequest()
        {
            SearchRequestOrdered validSearchRequestOrdered = new SearchRequestOrdered()
            {
                Action = RequestAction.NEW,
                RequestId = Guid.NewGuid().ToString(),
                TimeStamp = DateTime.Now,
            };
            var result = await _sut.CreateSearchRequest(null, validSearchRequestOrdered);

            _agencyRequestServiceMock.Verify(x => x.ProcessSearchRequestOrdered(It.IsAny<SearchRequestOrdered>()), Times.Never);
            Assert.IsInstanceOf(typeof(BadRequestResult), result);
        }

        [Test]
        public async Task With_update_action_searchRequestOrdered_it_should_return_BadRequest()
        {
            SearchRequestOrdered updateSearchRequestOrdered = new SearchRequestOrdered()
            {
                Action = RequestAction.UPDATE,
                RequestId = Guid.NewGuid().ToString(),
                TimeStamp = DateTime.Now,
            };
            var result = await _sut.CreateSearchRequest(null, updateSearchRequestOrdered);

            _agencyRequestServiceMock.Verify(x => x.ProcessSearchRequestOrdered(It.IsAny<SearchRequestOrdered>()), Times.Never);
            Assert.IsInstanceOf(typeof(BadRequestResult), result);
        }

        [Test]
        public async Task With_exception_throws_searchRequestOrdered_it_should_return_BadRequest()
        {
            string requestId = "exception";
            SearchRequestOrdered updateSearchRequestOrdered = new SearchRequestOrdered()
            {
                Action = RequestAction.NEW,
                RequestId = requestId,
                TimeStamp = DateTime.Now,
            };

            _agencyRequestServiceMock.Setup(
                x => x.ProcessSearchRequestOrdered(It.Is<SearchRequestOrdered>(x => x.RequestId == requestId)))
            .Throws(new Exception("exception throws"));
            var result = await _sut.CreateSearchRequest("exceptionrequest", updateSearchRequestOrdered);
            _agencyRequestServiceMock.Verify(x => x.ProcessSearchRequestOrdered(It.IsAny<SearchRequestOrdered>()), Times.Once);
            Assert.IsInstanceOf(typeof(BadRequestResult), result);
        }
    }
}
