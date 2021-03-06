﻿using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Flurl.Http.Xml.Tests.Factories;
using Flurl.Http.Xml.Tests.Models;
using Xunit;

namespace Flurl.Http.Xml.Tests
{
    public class RequestExtensionsShould : TestBase
    {
        private void AssertTestModel(TestModel testModel, int expectedNumber, string expectedText)
        {
            Assert.Equal(expectedNumber, testModel.Number);
            Assert.Equal(expectedText, testModel.Text);
        }

        private void AssertXDocument(XDocument document, int expectedNumber, string expectedText)
        {
            Assert.Equal(expectedNumber.ToString(), document?.Element("TestModel")?.Element("Number")?.Value);
            Assert.Equal(expectedText, document?.Element("TestModel")?.Element("Text")?.Value);
        }

        [Fact]
        public async Task GetXmlAsync()
        {
            FlurlHttp.Configure(c => c.HttpClientFactory = new TestModelHttpClientFactory());

            var result = await new Url("https://some.url")
                .AllowAnyHttpStatus()
                .GetXmlAsync<TestModel>();

            AssertTestModel(result, 3, "Test");
        }

        [Fact]
        public async Task GetXDocumentAsync()
        {
            FlurlHttp.Configure(c => c.HttpClientFactory = new TestModelHttpClientFactory());

            var result = await new Url("https://some.url")
                .AllowAnyHttpStatus()
                .GetXDocumentAsync();

            AssertXDocument(result, 3, "Test");
        }

        [Fact]
        public async Task GetXElementsFromXPathAsync()
        {
            FlurlHttp.Configure(c => c.HttpClientFactory = new TestModelHttpClientFactory());

            var result = await new Url("https://some.url")
                .AllowAnyHttpStatus()
                .GetXElementsFromXPath("/TestModel");

            AssertXDocument(result.FirstOrDefault()?.Document, 3, "Test");
        }

        [Fact]
        public async Task GetXElementsFromXPathNamespaceResolverAsync()
        {
            FlurlHttp.Configure(c => c.HttpClientFactory = new TestModelHttpClientFactory());

            var result = await new Url("https://some.url")
                .AllowAnyHttpStatus()
                .GetXElementsFromXPath("/TestModel", new XmlNamespaceManager(new NameTable()));

            AssertXDocument(result.FirstOrDefault()?.Document, 3, "Test");
        }

        [Theory]
        [InlineData(HttpMethodTypes.Post)]
        [InlineData(HttpMethodTypes.Put)]
        public async Task SendXmlToModelAsync(HttpMethodTypes methodType)
        {
            FlurlHttp.Configure(c => c.HttpClientFactory = new EchoHttpClientFactory());

            var method = HttpMethodByType[methodType];
            var result = await new Url("https://some.url")
                .AllowAnyHttpStatus()
                .SendXmlAsync(method, new TestModel { Number = 3, Text = "Test" })
                .ReceiveXml<TestModel>();

            AssertTestModel(result, 3, "Test");
        }

        [Theory]
        [InlineData(HttpMethodTypes.Post)]
        [InlineData(HttpMethodTypes.Put)]
        public async Task SendXmlToXDocumentAsync(HttpMethodTypes methodType)
        {
            FlurlHttp.Configure(c => c.HttpClientFactory = new EchoHttpClientFactory());

            var method = HttpMethodByType[methodType];
            var result = await new Url("https://some.url")
                .AllowAnyHttpStatus()
                .SendXmlAsync(method, new TestModel { Number = 3, Text = "Test" })
                .ReceiveXDocument();

            AssertXDocument(result, 3, "Test");
        }

        [Fact]
        public async Task PostXmlToModelAsync()
        {
            FlurlHttp.Configure(c => c.HttpClientFactory = new EchoHttpClientFactory());

            var result = await new Url("https://some.url")
                .AllowAnyHttpStatus()
                .PostXmlAsync(new TestModel { Number = 3, Text = "Test" })
                .ReceiveXml<TestModel>();

            AssertTestModel(result, 3, "Test");
        }

        [Fact]
        public async Task PostXmlToXDocumentAsync()
        {
            FlurlHttp.Configure(c => c.HttpClientFactory = new EchoHttpClientFactory());

            var result = await new Url("https://some.url")
                .AllowAnyHttpStatus()
                .PostXmlAsync(new TestModel { Number = 3, Text = "Test" })
                .ReceiveXDocument();

            AssertXDocument(result, 3, "Test");
        }

        [Fact]
        public async Task PutXmlToModelAsync()
        {
            FlurlHttp.Configure(c => c.HttpClientFactory = new EchoHttpClientFactory());

            var result = await new Url("https://some.url")
                .AllowAnyHttpStatus()
                .PutXmlAsync(new TestModel { Number = 3, Text = "Test" })
                .ReceiveXml<TestModel>();

            AssertTestModel(result, 3, "Test");
        }

        [Fact]
        public async Task PutXmlToXDocumentAsync()
        {
            FlurlHttp.Configure(c => c.HttpClientFactory = new EchoHttpClientFactory());

            var result = await new Url("https://some.url")
                .AllowAnyHttpStatus()
                .PutXmlAsync(new TestModel { Number = 3, Text = "Test" })
                .ReceiveXDocument();

            AssertXDocument(result, 3, "Test");
        }

        [Theory]
        [InlineData("Accept", "text/xml, application/xml", "text/xml")]
        [InlineData("Accept", "text/something+xml", "text/something+xml")]
        [InlineData("Accept", null, "application/xml")]
        [InlineData("", null, "application/xml")]
        public async Task ReceiveCorrectMediaType(string headerName, string acceptMediaType, string expectedContentType)
        {
            FlurlHttp.Configure(c => c.HttpClientFactory = new EchoHttpClientFactory());

            var result = await new Url("https://some.url")
                .WithHeader(headerName, acceptMediaType)
                .SendXmlAsync(HttpMethod.Post, new TestModel { Number = 3, Text = "Test" })
                .ReceiveXmlResponseMessage();

            Assert.Equal(expectedContentType, result?.Content?.Headers?.ContentType?.MediaType);
        }
    }
}
