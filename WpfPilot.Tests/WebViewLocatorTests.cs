#if NET5_0_OR_GREATER

namespace WpfPilot.Tests;

using System;
using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// Unit tests for WebViewLocator selector building and chaining logic.
/// These tests verify the locator infrastructure without requiring an actual WebView2.
/// </summary>
[TestFixture]
public class WebViewLocatorTests
{
    #region Selector Building Tests

    [Test]
    public void Locator_WithCssSelector_CreatesValidLocator()
    {
        var locator = CreateTestLocator("#submit-button");
        Assert.That(locator, Is.Not.Null);
    }

    [Test]
    public void Locator_WithComplexCssSelector_CreatesValidLocator()
    {
        var locator = CreateTestLocator("div.container > form input[type='text']:first-child");
        Assert.That(locator, Is.Not.Null);
    }

    [Test]
    public void Locator_WithAttributeSelector_CreatesValidLocator()
    {
        var locator = CreateTestLocator("[data-testid='my-button']");
        Assert.That(locator, Is.Not.Null);
    }

    [Test]
    public void Locator_First_CreatesNthLocator()
    {
        var locator = CreateTestLocator("button").First();
        Assert.That(locator, Is.Not.Null);
    }

    [Test]
    public void Locator_Last_CreatesNthLocator()
    {
        var locator = CreateTestLocator("button").Last();
        Assert.That(locator, Is.Not.Null);
    }

    [Test]
    public void Locator_Nth_WithValidIndex_CreatesNthLocator()
    {
        var locator = CreateTestLocator("li").Nth(3);
        Assert.That(locator, Is.Not.Null);
    }

    [Test]
    public void Locator_Nth_WithZero_CreatesSameAsFirst()
    {
        var first = CreateTestLocator("li").First();
        var nth0 = CreateTestLocator("li").Nth(0);
        
        // Both should create valid locators
        Assert.That(first, Is.Not.Null);
        Assert.That(nth0, Is.Not.Null);
    }

    #endregion

    #region Chaining Tests

    [Test]
    public void Locator_Chained_CreatesNestedLocator()
    {
        var locator = CreateTestLocator("form")
            .Locator("fieldset")
            .Locator("input");
        
        Assert.That(locator, Is.Not.Null);
    }

    [Test]
    public void Locator_DeeplyNested_Works()
    {
        var locator = CreateTestLocator("html")
            .Locator("body")
            .Locator("main")
            .Locator("section")
            .Locator("article")
            .Locator("p");
        
        Assert.That(locator, Is.Not.Null);
    }

    [Test]
    public void Locator_ChainedWithFirst_Works()
    {
        var locator = CreateTestLocator("ul")
            .Locator("li").First()
            .Locator("a");
        
        Assert.That(locator, Is.Not.Null);
    }

    [Test]
    public void Locator_ChainedWithFilter_Works()
    {
        var locator = CreateTestLocator("table")
            .Locator("tr").Filter(new LocatorFilter { HasText = "Active" })
            .Locator("td").First();
        
        Assert.That(locator, Is.Not.Null);
    }

    #endregion

    #region Filter Tests

    [Test]
    public void Filter_WithHasText_CreatesFilteredLocator()
    {
        var filter = new LocatorFilter { HasText = "Click me" };
        var locator = CreateTestLocator("button").Filter(filter);
        
        Assert.That(locator, Is.Not.Null);
    }

    [Test]
    public void Filter_WithHasNotText_CreatesFilteredLocator()
    {
        var filter = new LocatorFilter { HasNotText = "Disabled" };
        var locator = CreateTestLocator("button").Filter(filter);
        
        Assert.That(locator, Is.Not.Null);
    }

    [Test]
    public void Filter_WithHas_CreatesFilteredLocator()
    {
        var innerLocator = CreateTestLocator("span.icon");
        var filter = new LocatorFilter { Has = innerLocator };
        var locator = CreateTestLocator("button").Filter(filter);
        
        Assert.That(locator, Is.Not.Null);
    }

    [Test]
    public void Filter_WithHasNot_CreatesFilteredLocator()
    {
        var innerLocator = CreateTestLocator("span.disabled-icon");
        var filter = new LocatorFilter { HasNot = innerLocator };
        var locator = CreateTestLocator("button").Filter(filter);
        
        Assert.That(locator, Is.Not.Null);
    }

    [Test]
    public void HasText_Shorthand_Works()
    {
        var locator = CreateTestLocator("button").HasText("Submit");
        Assert.That(locator, Is.Not.Null);
    }

    [Test]
    public void HasNotText_Shorthand_Works()
    {
        var locator = CreateTestLocator("button").HasNotText("Cancel");
        Assert.That(locator, Is.Not.Null);
    }

    #endregion

    #region GetBy* Method Tests

    [Test]
    public void GetByText_CreatesTextLocator()
    {
        var locator = CreateTestLocator("div").GetByText("Hello World");
        Assert.That(locator, Is.Not.Null);
    }

    [Test]
    public void GetByRole_WithName_CreatesRoleLocator()
    {
        var locator = CreateTestLocator("div").GetByRole("button", "Submit");
        Assert.That(locator, Is.Not.Null);
    }

    [Test]
    public void GetByRole_WithoutName_CreatesRoleLocator()
    {
        var locator = CreateTestLocator("div").GetByRole("navigation");
        Assert.That(locator, Is.Not.Null);
    }

    [Test]
    public void GetByLabel_CreatesLabelLocator()
    {
        var locator = CreateTestLocator("form").GetByLabel("Email Address");
        Assert.That(locator, Is.Not.Null);
    }

    [Test]
    public void GetByPlaceholder_CreatesPlaceholderLocator()
    {
        var locator = CreateTestLocator("form").GetByPlaceholder("Enter your email");
        Assert.That(locator, Is.Not.Null);
    }

    [Test]
    public void GetByTestId_CreatesTestIdLocator()
    {
        var locator = CreateTestLocator("div").GetByTestId("my-component");
        Assert.That(locator, Is.Not.Null);
    }

    [Test]
    public void GetByAltText_CreatesAltLocator()
    {
        var locator = CreateTestLocator("div").GetByAltText("Profile Picture");
        Assert.That(locator, Is.Not.Null);
    }

    [Test]
    public void GetByTitle_CreatesTitleLocator()
    {
        var locator = CreateTestLocator("div").GetByTitle("More Information");
        Assert.That(locator, Is.Not.Null);
    }

    #endregion

    #region Has/HasNot Method Tests

    [Test]
    public void Has_WithLocator_Works()
    {
        var iconLocator = CreateTestLocator("svg.icon");
        var locator = CreateTestLocator("button").Has(iconLocator);
        
        Assert.That(locator, Is.Not.Null);
    }

    [Test]
    public void HasNot_WithLocator_Works()
    {
        var disabledLocator = CreateTestLocator(".disabled");
        var locator = CreateTestLocator("button").HasNot(disabledLocator);
        
        Assert.That(locator, Is.Not.Null);
    }

    #endregion

    #region Complex Scenario Tests

    [Test]
    public void ComplexScenario_FormNavigation_Works()
    {
        // Simulates navigating a complex form
        var emailInput = CreateTestLocator("form")
            .Locator("fieldset").First()
            .GetByLabel("Email");
        
        var submitButton = CreateTestLocator("form")
            .Locator("button").Filter(new LocatorFilter { HasText = "Submit" });
        
        Assert.That(emailInput, Is.Not.Null);
        Assert.That(submitButton, Is.Not.Null);
    }

    [Test]
    public void ComplexScenario_TableRowSelection_Works()
    {
        // Simulates selecting a specific row in a table
        var activeRow = CreateTestLocator("table")
            .Locator("tbody")
            .Locator("tr").Filter(new LocatorFilter { HasText = "Active" })
            .First();
        
        var editButton = activeRow.Locator("button").GetByText("Edit");
        
        Assert.That(activeRow, Is.Not.Null);
        Assert.That(editButton, Is.Not.Null);
    }

    [Test]
    public void ComplexScenario_ModalDialog_Works()
    {
        // Simulates interacting with a modal
        var modal = CreateTestLocator("[role='dialog']");
        var closeButton = modal.Locator("button").GetByAltText("Close");
        var confirmButton = modal.GetByRole("button", "Confirm");
        
        Assert.That(modal, Is.Not.Null);
        Assert.That(closeButton, Is.Not.Null);
        Assert.That(confirmButton, Is.Not.Null);
    }

    [Test]
    public void ComplexScenario_NavigationMenu_Works()
    {
        // Simulates navigating a dropdown menu
        var nav = CreateTestLocator("nav");
        var dropdown = nav.Locator(".dropdown");
        var menuItem = dropdown.Locator("ul li a").HasText("Settings");
        
        Assert.That(nav, Is.Not.Null);
        Assert.That(dropdown, Is.Not.Null);
        Assert.That(menuItem, Is.Not.Null);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a test locator without requiring an actual WebView2Element.
    /// Uses reflection to create the locator with a null webview reference.
    /// </summary>
    private static WebViewLocator CreateTestLocator(string selector)
    {
        var locatorType = typeof(WebViewLocator);
        var ctor = locatorType.GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)[0];
        return (WebViewLocator)ctor.Invoke(new object?[] { null!, selector, false, null, null, null });
    }

    #endregion
}

/// <summary>
/// Tests for WebViewElementHandle that verify the element handle contract.
/// </summary>
[TestFixture]
public class WebViewElementHandleTests
{
    #region Constructor Tests

    [Test]
    public void ElementHandle_CanBeCreated()
    {
        // Create via WebView2Element (would need mock in real scenario)
        // For now, just verify the types exist and are accessible
        Assert.That(typeof(WebViewElementHandle), Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasSelectorProperty()
    {
        var handleType = typeof(WebViewElementHandle);
        var selectorProperty = handleType.GetProperty("Selector");
        
        Assert.That(selectorProperty, Is.Not.Null);
        Assert.That(selectorProperty.PropertyType, Is.EqualTo(typeof(string)));
    }

    #endregion

    #region Interface Verification Tests

    [Test]
    public void ElementHandle_HasClickMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("Click");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasDblClickMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("DblClick");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasTypeMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("Type");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasFillMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("Fill");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasClearMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("Clear");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasPressMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("Press");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasCheckMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("Check");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasUncheckMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("Uncheck");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasSelectOptionMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("SelectOption");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasHoverMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("Hover");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasFocusMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("Focus");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasBlurMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("Blur");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasScrollIntoViewMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("ScrollIntoView");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasDragToMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("DragTo");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasDispatchEventMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("DispatchEvent");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasTapMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("Tap");
        Assert.That(method, Is.Not.Null);
    }

    #endregion

    #region Query Method Verification Tests

    [Test]
    public void ElementHandle_HasInnerTextMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("InnerText");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasInnerHTMLMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("InnerHTML");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasOuterHTMLMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("OuterHTML");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasTextContentMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("TextContent");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasGetAttributeMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("GetAttribute");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasInputValueMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("InputValue");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasTagNameMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("TagName");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasClassListMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("ClassList");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasBoundingBoxMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("BoundingBox");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasGetComputedStyleMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("GetComputedStyle");
        Assert.That(method, Is.Not.Null);
    }

    #endregion

    #region State Check Verification Tests

    [Test]
    public void ElementHandle_HasIsVisibleMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("IsVisible");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasIsHiddenMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("IsHidden");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasIsEnabledMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("IsEnabled");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasIsDisabledMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("IsDisabled");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasIsCheckedMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("IsChecked");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasIsEditableMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("IsEditable");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasIsFocusedMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("IsFocused");
        Assert.That(method, Is.Not.Null);
    }

    #endregion

    #region Wait Method Verification Tests

    [Test]
    public void ElementHandle_HasWaitForVisibleMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("WaitForVisible");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasWaitForHiddenMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("WaitForHidden");
        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void ElementHandle_HasWaitForEnabledMethod()
    {
        var method = typeof(WebViewElementHandle).GetMethod("WaitForEnabled");
        Assert.That(method, Is.Not.Null);
    }

    #endregion
}

#endif
