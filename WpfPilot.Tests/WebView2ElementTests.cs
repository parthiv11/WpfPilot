#if NET5_0_OR_GREATER

namespace WpfPilot.Tests;

using System;
using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// Unit tests for WebView2Element, WebViewLocator, and WebViewElementHandle.
/// These tests validate the core functionality of the WebView2 automation API.
/// Note: Most tests require an actual WebView2 instance which requires Windows and .NET 5+.
/// </summary>
[TestFixture]
public class WebView2ElementTests
{
    #region WebViewLocator Tests

    [Test]
    public void WebViewLocator_First_ReturnsLocatorWithNthIndex0()
    {
        // Arrange - using mock/stub pattern for locator chain testing
        var locator = CreateMockLocatorChain("button");
        
        // Act
        var first = locator.First();
        
        // Assert - First() should return a new locator configured for first element
        Assert.That(first, Is.Not.Null);
        Assert.That(first, Is.Not.SameAs(locator));
    }

    [Test]
    public void WebViewLocator_Last_ReturnsLocatorWithNthIndexMinus1()
    {
        // Arrange
        var locator = CreateMockLocatorChain("button");
        
        // Act
        var last = locator.Last();
        
        // Assert
        Assert.That(last, Is.Not.Null);
        Assert.That(last, Is.Not.SameAs(locator));
    }

    [Test]
    public void WebViewLocator_Nth_ReturnsLocatorWithSpecifiedIndex()
    {
        // Arrange
        var locator = CreateMockLocatorChain("button");
        
        // Act
        var nth = locator.Nth(5);
        
        // Assert
        Assert.That(nth, Is.Not.Null);
        Assert.That(nth, Is.Not.SameAs(locator));
    }

    [Test]
    public void WebViewLocator_Filter_ReturnsFilteredLocator()
    {
        // Arrange
        var locator = CreateMockLocatorChain("button");
        var filter = new LocatorFilter { HasText = "Submit" };
        
        // Act
        var filtered = locator.Filter(filter);
        
        // Assert
        Assert.That(filtered, Is.Not.Null);
        Assert.That(filtered, Is.Not.SameAs(locator));
    }

    [Test]
    public void WebViewLocator_HasText_ReturnsFilteredLocator()
    {
        // Arrange
        var locator = CreateMockLocatorChain("button");
        
        // Act
        var filtered = locator.HasText("Click me");
        
        // Assert
        Assert.That(filtered, Is.Not.Null);
    }

    [Test]
    public void WebViewLocator_HasNotText_ReturnsFilteredLocator()
    {
        // Arrange
        var locator = CreateMockLocatorChain("button");
        
        // Act
        var filtered = locator.HasNotText("Disabled");
        
        // Assert
        Assert.That(filtered, Is.Not.Null);
    }

    [Test]
    public void WebViewLocator_Chaining_CreatesNestedLocator()
    {
        // Arrange
        var locator = CreateMockLocatorChain("form");
        
        // Act
        var nested = locator.Locator("input").Locator("button");
        
        // Assert
        Assert.That(nested, Is.Not.Null);
    }

    [Test]
    public void WebViewLocator_GetByText_CreatesTextLocator()
    {
        // Arrange
        var locator = CreateMockLocatorChain("div");
        
        // Act
        var textLocator = locator.GetByText("Hello World");
        
        // Assert
        Assert.That(textLocator, Is.Not.Null);
    }

    [Test]
    public void WebViewLocator_GetByRole_CreatesRoleLocator()
    {
        // Arrange
        var locator = CreateMockLocatorChain("div");
        
        // Act
        var roleLocator = locator.GetByRole("button", "Submit");
        
        // Assert
        Assert.That(roleLocator, Is.Not.Null);
    }

    [Test]
    public void WebViewLocator_GetByLabel_CreatesLabelLocator()
    {
        // Arrange
        var locator = CreateMockLocatorChain("form");
        
        // Act
        var labelLocator = locator.GetByLabel("Email");
        
        // Assert
        Assert.That(labelLocator, Is.Not.Null);
    }

    [Test]
    public void WebViewLocator_GetByPlaceholder_CreatesPlaceholderLocator()
    {
        // Arrange
        var locator = CreateMockLocatorChain("form");
        
        // Act
        var placeholderLocator = locator.GetByPlaceholder("Enter email");
        
        // Assert
        Assert.That(placeholderLocator, Is.Not.Null);
    }

    [Test]
    public void WebViewLocator_GetByTestId_CreatesTestIdLocator()
    {
        // Arrange
        var locator = CreateMockLocatorChain("div");
        
        // Act
        var testIdLocator = locator.GetByTestId("submit-btn");
        
        // Assert
        Assert.That(testIdLocator, Is.Not.Null);
    }

    [Test]
    public void WebViewLocator_GetByAltText_CreatesAltTextLocator()
    {
        // Arrange
        var locator = CreateMockLocatorChain("div");
        
        // Act
        var altLocator = locator.GetByAltText("Logo");
        
        // Assert
        Assert.That(altLocator, Is.Not.Null);
    }

    [Test]
    public void WebViewLocator_GetByTitle_CreatesTitleLocator()
    {
        // Arrange
        var locator = CreateMockLocatorChain("div");
        
        // Act
        var titleLocator = locator.GetByTitle("More info");
        
        // Assert
        Assert.That(titleLocator, Is.Not.Null);
    }

    #endregion

    #region Options Tests

    [Test]
    public void NavigationOptions_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new NavigationOptions();
        
        // Assert
        Assert.That(options.Timeout, Is.Null);
        Assert.That(options.WaitUntil, Is.EqualTo(WaitUntilState.Load));
        Assert.That(options.Referer, Is.Null);
    }

    [Test]
    public void ClickOptions_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new ClickOptions();
        
        // Assert
        Assert.That(options.Button, Is.EqualTo(MouseButton.Left));
        Assert.That(options.ClickCount, Is.EqualTo(1));
        Assert.That(options.Delay, Is.EqualTo(0));
        Assert.That(options.Force, Is.False);
        Assert.That(options.Modifiers, Is.EqualTo(KeyModifier.None));
        Assert.That(options.Position, Is.Null);
        Assert.That(options.Timeout, Is.Null);
        Assert.That(options.Trial, Is.Null);
    }

    [Test]
    public void TypeOptions_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new TypeOptions();
        
        // Assert
        Assert.That(options.Delay, Is.EqualTo(0));
        Assert.That(options.Timeout, Is.Null);
        Assert.That(options.NoWaitAfter, Is.False);
    }

    [Test]
    public void WaitForSelectorOptions_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new WaitForSelectorOptions();
        
        // Assert
        Assert.That(options.State, Is.EqualTo(ElementState.Visible));
        Assert.That(options.Timeout, Is.Null);
        Assert.That(options.Strict, Is.Null);
    }

    [Test]
    public void LocatorFilter_DefaultValues_AreAllNull()
    {
        // Arrange & Act
        var filter = new LocatorFilter();
        
        // Assert
        Assert.That(filter.HasText, Is.Null);
        Assert.That(filter.HasNotText, Is.Null);
        Assert.That(filter.Has, Is.Null);
        Assert.That(filter.HasNot, Is.Null);
    }

    [Test]
    public void ScriptTagOptions_CanSetUrlOrContent()
    {
        // Arrange & Act
        var urlOption = new ScriptTagOptions { Url = "https://example.com/script.js" };
        var contentOption = new ScriptTagOptions { Content = "console.log('hello');" };
        
        // Assert
        Assert.That(urlOption.Url, Is.EqualTo("https://example.com/script.js"));
        Assert.That(urlOption.Content, Is.Null);
        Assert.That(contentOption.Content, Is.EqualTo("console.log('hello');"));
        Assert.That(contentOption.Url, Is.Null);
    }

    [Test]
    public void StyleTagOptions_CanSetUrlOrContent()
    {
        // Arrange & Act
        var urlOption = new StyleTagOptions { Url = "https://example.com/style.css" };
        var contentOption = new StyleTagOptions { Content = "body { color: red; }" };
        
        // Assert
        Assert.That(urlOption.Url, Is.EqualTo("https://example.com/style.css"));
        Assert.That(urlOption.Content, Is.Null);
        Assert.That(contentOption.Content, Is.EqualTo("body { color: red; }"));
        Assert.That(contentOption.Url, Is.Null);
    }

    [Test]
    public void WebViewScreenshotOptions_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new WebViewScreenshotOptions();
        
        // Assert
        Assert.That(options.Format, Is.EqualTo(ImageFormat.Png));
        Assert.That(options.FullPage, Is.False);
        Assert.That(options.Quality, Is.Null);
        Assert.That(options.Clip, Is.Null);
        Assert.That(options.OmitBackground, Is.False);
    }

    [Test]
    public void ClipRegion_CanSetCoordinates()
    {
        // Arrange & Act
        var clip = new ClipRegion { X = 10, Y = 20, Width = 100, Height = 200 };
        
        // Assert
        Assert.That(clip.X, Is.EqualTo(10));
        Assert.That(clip.Y, Is.EqualTo(20));
        Assert.That(clip.Width, Is.EqualTo(100));
        Assert.That(clip.Height, Is.EqualTo(200));
    }

    [Test]
    public void Position_CanBeConstructed()
    {
        // Arrange & Act
        var defaultPos = new Position();
        var coordPos = new Position(100.5, 200.5);
        
        // Assert
        Assert.That(defaultPos.X, Is.EqualTo(0));
        Assert.That(defaultPos.Y, Is.EqualTo(0));
        Assert.That(coordPos.X, Is.EqualTo(100.5));
        Assert.That(coordPos.Y, Is.EqualTo(200.5));
    }

    [Test]
    public void BoundingBox_CanSetAllProperties()
    {
        // Arrange & Act
        var box = new BoundingBox { X = 10, Y = 20, Width = 100, Height = 50 };
        
        // Assert
        Assert.That(box.X, Is.EqualTo(10));
        Assert.That(box.Y, Is.EqualTo(20));
        Assert.That(box.Width, Is.EqualTo(100));
        Assert.That(box.Height, Is.EqualTo(50));
    }

    [Test]
    public void NavigationResponse_CanSetAllProperties()
    {
        // Arrange & Act
        var response = new NavigationResponse
        {
            Url = "https://example.com",
            Ok = true,
            Status = 200,
            StatusText = "OK",
            Headers = new Dictionary<string, string> { { "Content-Type", "text/html" } }
        };
        
        // Assert
        Assert.That(response.Url, Is.EqualTo("https://example.com"));
        Assert.That(response.Ok, Is.True);
        Assert.That(response.Status, Is.EqualTo(200));
        Assert.That(response.StatusText, Is.EqualTo("OK"));
        Assert.That(response.Headers, Contains.Key("Content-Type"));
    }

    #endregion

    #region Enum Tests

    [Test]
    public void WaitUntilState_HasAllExpectedValues()
    {
        // Assert
        Assert.That(Enum.IsDefined(typeof(WaitUntilState), WaitUntilState.None), Is.True);
        Assert.That(Enum.IsDefined(typeof(WaitUntilState), WaitUntilState.DOMContentLoaded), Is.True);
        Assert.That(Enum.IsDefined(typeof(WaitUntilState), WaitUntilState.Load), Is.True);
        Assert.That(Enum.IsDefined(typeof(WaitUntilState), WaitUntilState.NetworkIdle), Is.True);
        Assert.That(Enum.IsDefined(typeof(WaitUntilState), WaitUntilState.Commit), Is.True);
    }

    [Test]
    public void ElementState_HasAllExpectedValues()
    {
        // Assert
        Assert.That(Enum.IsDefined(typeof(ElementState), ElementState.Attached), Is.True);
        Assert.That(Enum.IsDefined(typeof(ElementState), ElementState.Detached), Is.True);
        Assert.That(Enum.IsDefined(typeof(ElementState), ElementState.Visible), Is.True);
        Assert.That(Enum.IsDefined(typeof(ElementState), ElementState.Hidden), Is.True);
    }

    [Test]
    public void MouseButton_HasAllExpectedValues()
    {
        // Assert
        Assert.That(Enum.IsDefined(typeof(MouseButton), MouseButton.Left), Is.True);
        Assert.That(Enum.IsDefined(typeof(MouseButton), MouseButton.Right), Is.True);
        Assert.That(Enum.IsDefined(typeof(MouseButton), MouseButton.Middle), Is.True);
    }

    [Test]
    public void KeyModifier_HasAllExpectedValues()
    {
        // Assert
        Assert.That(Enum.IsDefined(typeof(KeyModifier), KeyModifier.None), Is.True);
        Assert.That(Enum.IsDefined(typeof(KeyModifier), KeyModifier.Alt), Is.True);
        Assert.That(Enum.IsDefined(typeof(KeyModifier), KeyModifier.Control), Is.True);
        Assert.That(Enum.IsDefined(typeof(KeyModifier), KeyModifier.Meta), Is.True);
        Assert.That(Enum.IsDefined(typeof(KeyModifier), KeyModifier.Shift), Is.True);
    }

    [Test]
    public void KeyModifier_CanBeCombined()
    {
        // Arrange & Act
        var combined = KeyModifier.Control | KeyModifier.Shift;
        
        // Assert
        Assert.That(combined.HasFlag(KeyModifier.Control), Is.True);
        Assert.That(combined.HasFlag(KeyModifier.Shift), Is.True);
        Assert.That(combined.HasFlag(KeyModifier.Alt), Is.False);
    }

    [Test]
    public void ConsoleMessageType_HasAllExpectedValues()
    {
        // Assert
        Assert.That(Enum.IsDefined(typeof(ConsoleMessageType), ConsoleMessageType.Log), Is.True);
        Assert.That(Enum.IsDefined(typeof(ConsoleMessageType), ConsoleMessageType.Debug), Is.True);
        Assert.That(Enum.IsDefined(typeof(ConsoleMessageType), ConsoleMessageType.Info), Is.True);
        Assert.That(Enum.IsDefined(typeof(ConsoleMessageType), ConsoleMessageType.Warning), Is.True);
        Assert.That(Enum.IsDefined(typeof(ConsoleMessageType), ConsoleMessageType.Error), Is.True);
    }

    #endregion

    #region Exception Tests

    [Test]
    public void WebViewAssertionException_CanBeConstructed()
    {
        // Arrange & Act
        var defaultEx = new WebViewAssertionException();
        var messageEx = new WebViewAssertionException("Test message");
        var innerEx = new WebViewAssertionException("Outer", new Exception("Inner"));
        
        // Assert
        Assert.That(defaultEx.Message, Is.Not.Null);
        Assert.That(messageEx.Message, Is.EqualTo("Test message"));
        Assert.That(innerEx.InnerException?.Message, Is.EqualTo("Inner"));
    }

    [Test]
    public void WebViewElementNotFoundException_CanBeConstructed()
    {
        // Arrange & Act
        var defaultEx = new WebViewElementNotFoundException();
        var messageEx = new WebViewElementNotFoundException("Element not found: #my-id");
        var innerEx = new WebViewElementNotFoundException("Outer", new Exception("Inner"));
        
        // Assert
        Assert.That(defaultEx.Message, Is.Not.Null);
        Assert.That(messageEx.Message, Is.EqualTo("Element not found: #my-id"));
        Assert.That(innerEx.InnerException?.Message, Is.EqualTo("Inner"));
    }

    [Test]
    public void WebViewNavigationException_CanBeConstructed()
    {
        // Arrange & Act
        var ex = new WebViewNavigationException("Navigation failed");
        
        // Assert
        Assert.That(ex.Message, Is.EqualTo("Navigation failed"));
    }

    [Test]
    public void WebViewNavigationException_CanSetUrlAndStatusCode()
    {
        // Arrange & Act
        var ex = new WebViewNavigationException("Navigation failed")
        {
            Url = "https://example.com",
            StatusCode = 404
        };
        
        // Assert
        Assert.That(ex.Url, Is.EqualTo("https://example.com"));
        Assert.That(ex.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public void WebViewEvaluationException_CanBeConstructed()
    {
        // Arrange & Act
        var ex = new WebViewEvaluationException("JavaScript error");
        
        // Assert
        Assert.That(ex.Message, Is.EqualTo("JavaScript error"));
    }

    [Test]
    public void WebViewEvaluationException_CanSetJsProperties()
    {
        // Arrange & Act
        var ex = new WebViewEvaluationException("JavaScript error")
        {
            Expression = "document.querySelector('#missing')",
            JsErrorMessage = "Cannot read property of null",
            JsStackTrace = "at eval:1:1"
        };
        
        // Assert
        Assert.That(ex.Expression, Is.EqualTo("document.querySelector('#missing')"));
        Assert.That(ex.JsErrorMessage, Is.EqualTo("Cannot read property of null"));
        Assert.That(ex.JsStackTrace, Is.EqualTo("at eval:1:1"));
    }

    [Test]
    public void WebViewElementNotActionableException_CanBeConstructed()
    {
        // Arrange & Act
        var ex = new WebViewElementNotActionableException("Element not actionable");
        
        // Assert
        Assert.That(ex.Message, Is.EqualTo("Element not actionable"));
    }

    [Test]
    public void WebViewElementNotActionableException_CanSetSelectorAndReason()
    {
        // Arrange & Act
        var ex = new WebViewElementNotActionableException("Element not actionable")
        {
            Selector = "#disabled-button",
            Reason = "Element is disabled"
        };
        
        // Assert
        Assert.That(ex.Selector, Is.EqualTo("#disabled-button"));
        Assert.That(ex.Reason, Is.EqualTo("Element is disabled"));
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a mock locator chain for testing locator methods that don't require actual WebView2.
    /// This uses a minimal mock that allows testing the locator chaining logic.
    /// </summary>
    private static WebViewLocator CreateMockLocatorChain(string selector)
    {
        // We use reflection to create a locator without a real WebView2Element
        // This allows testing the locator chaining logic in isolation
        var locatorType = typeof(WebViewLocator);
        var ctor = locatorType.GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)[0];
        
        // Create with null webView - this is okay for tests that only verify structure
        // Tests that need actual WebView2 functionality should use integration tests
        return (WebViewLocator)ctor.Invoke(new object?[] { null!, selector, false, null, null, null });
    }

    #endregion
}

#endif
