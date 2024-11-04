namespace WpfPilot.Tests;

using System;
using System.IO;
using System.Linq;
using System.Windows;
using NUnit.Framework;
using WpfPilot;
using WpfPilot.Elements;
using WpfPilot.Tests.TestUtility;

[TestFixture]
public sealed class WebView2ElementTests : AppTestBase
{
    [OneTimeSetUp]
    public override void Setup()
    {
        base.Setup();
        ExePath = $"../../../../../WpfPilot.ExampleApp/bin/x64/Debug/{CurrentFramework.GetVersion()}/WpfPilot.ExampleApp.exe";
        ExePath = Path.GetFullPath(ExePath);
        Assert.True(File.Exists(ExePath), $"Could not find ExampleApp at {ExePath}. Ensure the project has been compiled.");
    }

    [Test]
    public void TestWebView2ElementSelectors()
    {
        using var appDriver = AppDriver.Launch(ExePath);
        var webView2Element = appDriver.GetElement<WebView2Element>(x => x.TypeName == "WebView2");

        Assert.IsNotNull(webView2Element);
    }

    [Test]
    public void TestWebView2ElementClick()
    {
        using var appDriver = AppDriver.Launch(ExePath);
        var webView2Element = appDriver.GetElement<WebView2Element>(x => x.TypeName == "WebView2");

        webView2Element.Click();
        // Add assertions to verify the click action
    }

    [Test]
    public void TestWebView2ElementType()
    {
        using var appDriver = AppDriver.Launch(ExePath);
        var webView2Element = appDriver.GetElement<WebView2Element>(x => x.TypeName == "WebView2");

        webView2Element.Type("Hello World!");
        // Add assertions to verify the type action
    }

    [Test]
    public void TestWebView2ElementSetProperty()
    {
        using var appDriver = AppDriver.Launch(ExePath);
        var webView2Element = appDriver.GetElement<WebView2Element>(x => x.TypeName == "WebView2");

        webView2Element.SetProperty("IsEnabled", true);
        Assert.IsTrue(webView2Element["IsEnabled"]);
    }

    [Test]
    public void TestWebView2ElementInvoke()
    {
        using var appDriver = AppDriver.Launch(ExePath);
        var webView2Element = appDriver.GetElement<WebView2Element>(x => x.TypeName == "WebView2");

        webView2Element.Invoke(element => element.Focus());
        Assert.IsTrue(webView2Element["IsFocused"]);
    }

    private string ExePath { get; set; } = "";
}
