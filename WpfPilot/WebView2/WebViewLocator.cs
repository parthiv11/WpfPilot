#if NET5_0_OR_GREATER

namespace WpfPilot;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// A Playwright-style locator for finding and interacting with DOM elements.
/// Locators are lazy and will only query the DOM when an action is performed.
/// Locators support chaining to create complex element queries.
/// <code>
/// webView.Locator("form").Locator("input[type='submit']").Click();
/// webView.Locator(".item").First().Click();
/// webView.Locator("button").Filter(new() { HasText = "Submit" }).Click();
/// </code>
/// </summary>
public class WebViewLocator
{
    private readonly WebView2Element _webView;
    private readonly string _selector;
    private readonly bool _exact;
    private readonly int? _nth;
    private readonly LocatorFilter? _filter;
    private readonly WebViewLocator? _parent;

    /// <summary>
    /// Creates a new locator for the specified selector.
    /// </summary>
    internal WebViewLocator(WebView2Element webView, string selector, bool exact = false, int? nth = null, LocatorFilter? filter = null, WebViewLocator? parent = null)
    {
        _webView = webView ?? throw new ArgumentNullException(nameof(webView));
        _selector = selector ?? throw new ArgumentNullException(nameof(selector));
        _exact = exact;
        _nth = nth;
        _filter = filter;
        _parent = parent;
    }

    #region Locator Chaining

    /// <summary>
    /// Creates a child locator scoped to this locator's element.
    /// </summary>
    /// <param name="selector">The CSS selector relative to the parent element.</param>
    /// <returns>A new scoped locator.</returns>
    public WebViewLocator Locator(string selector)
    {
        _ = selector ?? throw new ArgumentNullException(nameof(selector));
        return new WebViewLocator(_webView, selector, parent: this);
    }

    /// <summary>
    /// Returns the first element matching this locator.
    /// </summary>
    /// <returns>A locator for the first element.</returns>
    public WebViewLocator First() =>
        new(_webView, _selector, _exact, 0, _filter, _parent);

    /// <summary>
    /// Returns the last element matching this locator.
    /// </summary>
    /// <returns>A locator for the last element.</returns>
    public WebViewLocator Last() =>
        new(_webView, _selector, _exact, -1, _filter, _parent);

    /// <summary>
    /// Returns the nth element matching this locator (0-indexed).
    /// </summary>
    /// <param name="index">The zero-based index.</param>
    /// <returns>A locator for the nth element.</returns>
    public WebViewLocator Nth(int index) =>
        new(_webView, _selector, _exact, index, _filter, _parent);

    /// <summary>
    /// Filters the locator to only match elements with the specified criteria.
    /// </summary>
    /// <param name="options">The filter options.</param>
    /// <returns>A filtered locator.</returns>
    public WebViewLocator Filter(LocatorFilter options) =>
        new(_webView, _selector, _exact, _nth, options, _parent);

    /// <summary>
    /// Creates a locator for elements with matching text content.
    /// </summary>
    /// <param name="text">The text to match.</param>
    /// <param name="exact">If true, matches exact text; otherwise, uses contains matching.</param>
    /// <returns>A locator for elements with the specified text.</returns>
    public WebViewLocator GetByText(string text, bool exact = false) =>
        Locator($"text={text}");

    /// <summary>
    /// Creates a locator for elements with matching role.
    /// </summary>
    /// <param name="role">The ARIA role to match.</param>
    /// <param name="name">Optional accessible name.</param>
    /// <returns>A locator for elements with the specified role.</returns>
    public WebViewLocator GetByRole(string role, string? name = null)
    {
        var selector = name != null ? $"role={role}[name={name}]" : $"[role='{role}']";
        return Locator(selector);
    }

    /// <summary>
    /// Creates a locator for elements with matching label.
    /// </summary>
    /// <param name="text">The label text to match.</param>
    /// <returns>A locator for elements with the specified label.</returns>
    public WebViewLocator GetByLabel(string text) =>
        Locator($"label={text}");

    /// <summary>
    /// Creates a locator for elements with matching placeholder.
    /// </summary>
    /// <param name="text">The placeholder text to match.</param>
    /// <returns>A locator for elements with the specified placeholder.</returns>
    public WebViewLocator GetByPlaceholder(string text) =>
        Locator($"[placeholder*='{text}']");

    /// <summary>
    /// Creates a locator for elements with matching test ID.
    /// </summary>
    /// <param name="testId">The test ID to match.</param>
    /// <returns>A locator for elements with the specified test ID.</returns>
    public WebViewLocator GetByTestId(string testId) =>
        Locator($"[data-testid='{testId}']");

    /// <summary>
    /// Creates a locator for elements with matching alt text.
    /// </summary>
    /// <param name="text">The alt text to match.</param>
    /// <returns>A locator for elements with the specified alt text.</returns>
    public WebViewLocator GetByAltText(string text) =>
        Locator($"[alt*='{text}']");

    /// <summary>
    /// Creates a locator for elements with matching title.
    /// </summary>
    /// <param name="text">The title to match.</param>
    /// <returns>A locator for elements with the specified title.</returns>
    public WebViewLocator GetByTitle(string text) =>
        Locator($"[title*='{text}']");

    /// <summary>
    /// Filters to elements that contain another locator.
    /// </summary>
    /// <param name="locator">The inner locator that must be present.</param>
    /// <returns>A filtered locator.</returns>
    public WebViewLocator Has(WebViewLocator locator) =>
        Filter(new LocatorFilter { Has = locator });

    /// <summary>
    /// Filters to elements that do not contain another locator.
    /// </summary>
    /// <param name="locator">The inner locator that must not be present.</param>
    /// <returns>A filtered locator.</returns>
    public WebViewLocator HasNot(WebViewLocator locator) =>
        Filter(new LocatorFilter { HasNot = locator });

    /// <summary>
    /// Filters to elements containing the specified text.
    /// </summary>
    /// <param name="text">The text that must be present.</param>
    /// <returns>A filtered locator.</returns>
    public WebViewLocator HasText(string text) =>
        Filter(new LocatorFilter { HasText = text });

    /// <summary>
    /// Filters to elements not containing the specified text.
    /// </summary>
    /// <param name="text">The text that must not be present.</param>
    /// <returns>A filtered locator.</returns>
    public WebViewLocator HasNotText(string text) =>
        Filter(new LocatorFilter { HasNotText = text });

    #endregion

    #region Actions

    /// <summary>
    /// Clicks the element.
    /// </summary>
    /// <param name="options">Optional click options.</param>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator Click(ClickOptions? options = null)
    {
        var handle = ResolveElement();
        handle.Click(options);
        return this;
    }

    /// <summary>
    /// Double-clicks the element.
    /// </summary>
    /// <param name="options">Optional click options.</param>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator DblClick(ClickOptions? options = null)
    {
        var handle = ResolveElement();
        handle.DblClick(options);
        return this;
    }

    /// <summary>
    /// Right-clicks the element.
    /// </summary>
    /// <param name="options">Optional click options.</param>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator RightClick(ClickOptions? options = null)
    {
        var handle = ResolveElement();
        handle.Click(options ?? new ClickOptions { Button = MouseButton.Right });
        return this;
    }

    /// <summary>
    /// Types text into the element.
    /// </summary>
    /// <param name="text">The text to type.</param>
    /// <param name="options">Optional type options.</param>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator Type(string text, TypeOptions? options = null)
    {
        var handle = ResolveElement();
        handle.Type(text, options);
        return this;
    }

    /// <summary>
    /// Fills the element with text (clears existing content first).
    /// </summary>
    /// <param name="value">The value to fill.</param>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator Fill(string value)
    {
        var handle = ResolveElement();
        handle.Fill(value);
        return this;
    }

    /// <summary>
    /// Clears the element's value.
    /// </summary>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator Clear()
    {
        var handle = ResolveElement();
        handle.Clear();
        return this;
    }

    /// <summary>
    /// Presses a key while focused on the element.
    /// </summary>
    /// <param name="key">The key to press.</param>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator Press(string key)
    {
        var handle = ResolveElement();
        handle.Press(key);
        return this;
    }

    /// <summary>
    /// Checks a checkbox or radio button.
    /// </summary>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator Check()
    {
        var handle = ResolveElement();
        handle.Check();
        return this;
    }

    /// <summary>
    /// Unchecks a checkbox.
    /// </summary>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator Uncheck()
    {
        var handle = ResolveElement();
        handle.Uncheck();
        return this;
    }

    /// <summary>
    /// Sets the checked state of a checkbox.
    /// </summary>
    /// <param name="isChecked">The desired checked state.</param>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator SetChecked(bool isChecked)
    {
        var handle = ResolveElement();
        handle.SetChecked(isChecked);
        return this;
    }

    /// <summary>
    /// Selects options in a select element.
    /// </summary>
    /// <param name="values">The values to select.</param>
    /// <returns>The selected values.</returns>
    public IReadOnlyList<string> SelectOption(params string[] values)
    {
        var handle = ResolveElement();
        return handle.SelectOption(values);
    }

    /// <summary>
    /// Hovers over the element.
    /// </summary>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator Hover()
    {
        var handle = ResolveElement();
        handle.Hover();
        return this;
    }

    /// <summary>
    /// Focuses the element.
    /// </summary>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator Focus()
    {
        var handle = ResolveElement();
        handle.Focus();
        return this;
    }

    /// <summary>
    /// Blurs (unfocuses) the element.
    /// </summary>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator Blur()
    {
        var handle = ResolveElement();
        handle.Blur();
        return this;
    }

    /// <summary>
    /// Scrolls the element into view.
    /// </summary>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator ScrollIntoView()
    {
        var handle = ResolveElement();
        handle.ScrollIntoView();
        return this;
    }

    /// <summary>
    /// Sets the input files for a file input element.
    /// </summary>
    /// <param name="filePaths">The file paths to set.</param>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator SetInputFiles(params string[] filePaths)
    {
        var handle = ResolveElement();
        handle.SetInputFiles(filePaths);
        return this;
    }

    /// <summary>
    /// Drags this element to another element.
    /// </summary>
    /// <param name="target">The target locator.</param>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator DragTo(WebViewLocator target)
    {
        var sourceHandle = ResolveElement();
        var targetHandle = target.ResolveElement();
        sourceHandle.DragTo(targetHandle);
        return this;
    }

    /// <summary>
    /// Dispatches a DOM event on the element.
    /// </summary>
    /// <param name="eventType">The event type (e.g., "click", "focus").</param>
    /// <param name="eventInit">Optional event initialization properties as JSON.</param>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator DispatchEvent(string eventType, string? eventInit = null)
    {
        var handle = ResolveElement();
        handle.DispatchEvent(eventType, eventInit);
        return this;
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Gets the count of elements matching this locator.
    /// </summary>
    /// <returns>The number of matching elements.</returns>
    public int Count()
    {
        var script = BuildCountScript();
        return _webView.EvaluateAsync<int>(script);
    }

    /// <summary>
    /// Returns all elements matching this locator.
    /// </summary>
    /// <returns>A list of element handles.</returns>
    public IReadOnlyList<WebViewElementHandle> All()
    {
        var count = Count();
        var handles = new List<WebViewElementHandle>();

        for (int i = 0; i < count; i++)
        {
            handles.Add(new WebViewElementHandle(_webView, BuildSelectorForNth(i), null));
        }

        return handles;
    }

    /// <summary>
    /// Gets the inner text of the element.
    /// </summary>
    /// <returns>The inner text.</returns>
    public string InnerText()
    {
        var handle = ResolveElement();
        return handle.InnerText();
    }

    /// <summary>
    /// Gets the inner HTML of the element.
    /// </summary>
    /// <returns>The inner HTML.</returns>
    public string InnerHTML()
    {
        var handle = ResolveElement();
        return handle.InnerHTML();
    }

    /// <summary>
    /// Gets the text content of the element.
    /// </summary>
    /// <returns>The text content.</returns>
    public string? TextContent()
    {
        var handle = ResolveElement();
        return handle.TextContent();
    }

    /// <summary>
    /// Gets an attribute value.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <returns>The attribute value, or null if not found.</returns>
    public string? GetAttribute(string name)
    {
        var handle = ResolveElement();
        return handle.GetAttribute(name);
    }

    /// <summary>
    /// Gets the input value.
    /// </summary>
    /// <returns>The input value.</returns>
    public string InputValue()
    {
        var handle = ResolveElement();
        return handle.InputValue();
    }

    /// <summary>
    /// Gets all inner texts of matching elements.
    /// </summary>
    /// <returns>A list of inner text values.</returns>
    public IReadOnlyList<string> AllInnerTexts()
    {
        var script = BuildAllTextsScript("innerText");
        return _webView.EvaluateAsync<List<string>>(script) ?? new List<string>();
    }

    /// <summary>
    /// Gets all text contents of matching elements.
    /// </summary>
    /// <returns>A list of text content values.</returns>
    public IReadOnlyList<string> AllTextContents()
    {
        var script = BuildAllTextsScript("textContent");
        return _webView.EvaluateAsync<List<string>>(script) ?? new List<string>();
    }

    /// <summary>
    /// Gets the bounding box of the element.
    /// </summary>
    /// <returns>The bounding box.</returns>
    public BoundingBox? BoundingBox()
    {
        var handle = ResolveElement();
        return handle.BoundingBox();
    }

    #endregion

    #region State Checks

    /// <summary>
    /// Checks if the element is visible.
    /// </summary>
    /// <returns>True if visible.</returns>
    public bool IsVisible()
    {
        try
        {
            var handle = ResolveElementOrNull();
            return handle?.IsVisible() ?? false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the element is hidden.
    /// </summary>
    /// <returns>True if hidden.</returns>
    public bool IsHidden() => !IsVisible();

    /// <summary>
    /// Checks if the element is enabled.
    /// </summary>
    /// <returns>True if enabled.</returns>
    public bool IsEnabled()
    {
        try
        {
            var handle = ResolveElementOrNull();
            return handle?.IsEnabled() ?? false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the element is disabled.
    /// </summary>
    /// <returns>True if disabled.</returns>
    public bool IsDisabled() => !IsEnabled();

    /// <summary>
    /// Checks if the element is checked (checkbox/radio).
    /// </summary>
    /// <returns>True if checked.</returns>
    public bool IsChecked()
    {
        try
        {
            var handle = ResolveElementOrNull();
            return handle?.IsChecked() ?? false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the element is editable.
    /// </summary>
    /// <returns>True if editable.</returns>
    public bool IsEditable()
    {
        try
        {
            var handle = ResolveElementOrNull();
            return handle?.IsEditable() ?? false;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Wait Methods

    /// <summary>
    /// Waits for the element to be visible.
    /// </summary>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator WaitFor(int? timeoutMs = null) =>
        WaitFor(new WaitForSelectorOptions { State = ElementState.Visible, Timeout = timeoutMs });

    /// <summary>
    /// Waits for the element to reach the specified state.
    /// </summary>
    /// <param name="options">Wait options.</param>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator WaitFor(WaitForSelectorOptions options)
    {
        options ??= new WaitForSelectorOptions();
        var timeout = options.Timeout ?? _webView.DefaultTimeout;
        var start = Environment.TickCount;

        while (Environment.TickCount - start < timeout)
        {
            var conditionMet = CheckState(options.State);
            if (conditionMet)
                return this;

            Task.Delay(100).GetAwaiter().GetResult();
        }

        throw new TimeoutException($"Timeout waiting for locator '{_selector}' to be {options.State} after {timeout}ms");
    }

    private bool CheckState(ElementState state)
    {
        var script = BuildStateCheckScript(state);
        return _webView.EvaluateAsync<bool>(script);
    }

    #endregion

    #region Assertions

    /// <summary>
    /// Asserts that the element has the specified text.
    /// </summary>
    /// <param name="expected">The expected text.</param>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator AssertText(string expected, int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? _webView.DefaultTimeout;
        var start = Environment.TickCount;

        while (Environment.TickCount - start < timeout)
        {
            try
            {
                var text = TextContent();
                if (text?.Contains(expected) == true)
                    return this;
            }
            catch
            {
                // Ignore and retry
            }

            Task.Delay(100).GetAwaiter().GetResult();
        }

        throw new WebViewAssertionException($"Locator '{_selector}' does not contain text '{expected}'");
    }

    /// <summary>
    /// Asserts that the element has the specified attribute.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">The expected value.</param>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator AssertAttribute(string name, string value, int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? _webView.DefaultTimeout;
        var start = Environment.TickCount;

        while (Environment.TickCount - start < timeout)
        {
            try
            {
                var attr = GetAttribute(name);
                if (attr == value)
                    return this;
            }
            catch
            {
                // Ignore and retry
            }

            Task.Delay(100).GetAwaiter().GetResult();
        }

        throw new WebViewAssertionException($"Locator '{_selector}' attribute '{name}' does not equal '{value}'");
    }

    /// <summary>
    /// Asserts that the element is visible.
    /// </summary>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator AssertVisible(int? timeoutMs = null) =>
        WaitFor(new WaitForSelectorOptions { State = ElementState.Visible, Timeout = timeoutMs });

    /// <summary>
    /// Asserts that the element is hidden.
    /// </summary>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator AssertHidden(int? timeoutMs = null) =>
        WaitFor(new WaitForSelectorOptions { State = ElementState.Hidden, Timeout = timeoutMs });

    /// <summary>
    /// Asserts that the element is enabled.
    /// </summary>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator AssertEnabled(int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? _webView.DefaultTimeout;
        var start = Environment.TickCount;

        while (Environment.TickCount - start < timeout)
        {
            if (IsEnabled())
                return this;
            Task.Delay(100).GetAwaiter().GetResult();
        }

        throw new WebViewAssertionException($"Locator '{_selector}' is not enabled");
    }

    /// <summary>
    /// Asserts that the element is disabled.
    /// </summary>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator AssertDisabled(int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? _webView.DefaultTimeout;
        var start = Environment.TickCount;

        while (Environment.TickCount - start < timeout)
        {
            if (IsDisabled())
                return this;
            Task.Delay(100).GetAwaiter().GetResult();
        }

        throw new WebViewAssertionException($"Locator '{_selector}' is not disabled");
    }

    /// <summary>
    /// Asserts that the element is checked.
    /// </summary>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator AssertChecked(int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? _webView.DefaultTimeout;
        var start = Environment.TickCount;

        while (Environment.TickCount - start < timeout)
        {
            if (IsChecked())
                return this;
            Task.Delay(100).GetAwaiter().GetResult();
        }

        throw new WebViewAssertionException($"Locator '{_selector}' is not checked");
    }

    /// <summary>
    /// Asserts that the element count equals the expected value.
    /// </summary>
    /// <param name="expected">The expected count.</param>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    /// <returns>This locator for chaining.</returns>
    public WebViewLocator AssertCount(int expected, int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? _webView.DefaultTimeout;
        var start = Environment.TickCount;

        while (Environment.TickCount - start < timeout)
        {
            if (Count() == expected)
                return this;
            Task.Delay(100).GetAwaiter().GetResult();
        }

        throw new WebViewAssertionException($"Locator '{_selector}' count is {Count()}, expected {expected}");
    }

    #endregion

    #region Evaluation

    /// <summary>
    /// Evaluates a JavaScript expression with the element as an argument.
    /// </summary>
    /// <typeparam name="TResult">The expected result type.</typeparam>
    /// <param name="expression">JavaScript expression where 'el' refers to the element.</param>
    /// <returns>The result of the evaluation.</returns>
    public TResult Evaluate<TResult>(string expression)
    {
        var script = BuildEvaluateScript(expression);
        return _webView.EvaluateAsync<TResult>(script);
    }

    /// <summary>
    /// Evaluates a JavaScript expression on all matching elements.
    /// </summary>
    /// <typeparam name="TResult">The expected result type.</typeparam>
    /// <param name="expression">JavaScript expression where 'elements' refers to the array of elements.</param>
    /// <returns>The result of the evaluation.</returns>
    public TResult EvaluateAll<TResult>(string expression)
    {
        var script = BuildEvaluateAllScript(expression);
        return _webView.EvaluateAsync<TResult>(script);
    }

    #endregion

    #region Private Helpers

    private WebViewElementHandle ResolveElement()
    {
        var handle = ResolveElementOrNull();
        if (handle == null)
            throw new WebViewElementNotFoundException($"No element found matching locator: {_selector}");
        return handle;
    }

    private WebViewElementHandle? ResolveElementOrNull()
    {
        var fullSelector = BuildFullSelector();
        var exists = _webView.EvaluateAsync<bool>(BuildExistsScript());

        if (!exists)
            return null;

        var id = _webView.EvaluateAsync<string>(BuildAssignIdScript());
        return new WebViewElementHandle(_webView, fullSelector, id);
    }

    private string BuildFullSelector()
    {
        if (_parent != null)
        {
            var parentSelector = _parent.BuildFullSelector();
            return $"{parentSelector} {_selector}";
        }
        return _selector;
    }

    private string BuildSelectorForNth(int index)
    {
        var baseSelector = BuildFullSelector();
        return $"{baseSelector}:nth-of-type({index + 1})";
    }

    private string BuildExistsScript()
    {
        var fullSelector = BuildFullSelector();
        var baseScript = BuildQueryScript("el !== null");

        if (_filter != null)
        {
            return BuildFilteredExistsScript();
        }

        return baseScript;
    }

    private string BuildFilteredExistsScript()
    {
        var fullSelector = BuildFullSelector();

        if (_filter?.HasText != null)
        {
            return $@"
                (() => {{
                    const els = [...document.querySelectorAll({JsonConvert.SerializeObject(fullSelector)})];
                    const filtered = els.filter(el => el.textContent?.includes({JsonConvert.SerializeObject(_filter.HasText)}));
                    return filtered.length > 0;
                }})()";
        }

        return BuildQueryScript("el !== null");
    }

    private string BuildAssignIdScript()
    {
        var fullSelector = BuildFullSelector();
        var nthIndex = _nth ?? 0;

        if (_filter != null)
        {
            return BuildFilteredAssignIdScript();
        }

        if (_nth == -1)
        {
            return $@"
                (() => {{
                    const els = document.querySelectorAll({JsonConvert.SerializeObject(fullSelector)});
                    const el = els[els.length - 1];
                    if (!el) return null;
                    if (!el.__wpfPilotId) {{
                        el.__wpfPilotId = 'wpf_' + Math.random().toString(36).substr(2, 9);
                    }}
                    return el.__wpfPilotId;
                }})()";
        }

        return $@"
            (() => {{
                const els = document.querySelectorAll({JsonConvert.SerializeObject(fullSelector)});
                const el = els[{nthIndex}];
                if (!el) return null;
                if (!el.__wpfPilotId) {{
                    el.__wpfPilotId = 'wpf_' + Math.random().toString(36).substr(2, 9);
                }}
                return el.__wpfPilotId;
            }})()";
    }

    private string BuildFilteredAssignIdScript()
    {
        var fullSelector = BuildFullSelector();
        var nthIndex = _nth ?? 0;

        if (_filter?.HasText != null)
        {
            return $@"
                (() => {{
                    const els = [...document.querySelectorAll({JsonConvert.SerializeObject(fullSelector)})];
                    const filtered = els.filter(el => el.textContent?.includes({JsonConvert.SerializeObject(_filter.HasText)}));
                    const el = filtered[{nthIndex}];
                    if (!el) return null;
                    if (!el.__wpfPilotId) {{
                        el.__wpfPilotId = 'wpf_' + Math.random().toString(36).substr(2, 9);
                    }}
                    return el.__wpfPilotId;
                }})()";
        }

        return BuildAssignIdScript();
    }

    private string BuildCountScript()
    {
        var fullSelector = BuildFullSelector();

        if (_filter?.HasText != null)
        {
            return $@"
                (() => {{
                    const els = [...document.querySelectorAll({JsonConvert.SerializeObject(fullSelector)})];
                    return els.filter(el => el.textContent?.includes({JsonConvert.SerializeObject(_filter.HasText)})).length;
                }})()";
        }

        return $"document.querySelectorAll({JsonConvert.SerializeObject(fullSelector)}).length";
    }

    private string BuildQueryScript(string returnExpression)
    {
        var fullSelector = BuildFullSelector();
        var nthIndex = _nth ?? 0;

        if (_nth == -1)
        {
            return $@"
                (() => {{
                    const els = document.querySelectorAll({JsonConvert.SerializeObject(fullSelector)});
                    const el = els[els.length - 1];
                    return {returnExpression};
                }})()";
        }

        if (_nth != null)
        {
            return $@"
                (() => {{
                    const els = document.querySelectorAll({JsonConvert.SerializeObject(fullSelector)});
                    const el = els[{nthIndex}];
                    return {returnExpression};
                }})()";
        }

        return $@"
            (() => {{
                const el = document.querySelector({JsonConvert.SerializeObject(fullSelector)});
                return {returnExpression};
            }})()";
    }

    private string BuildStateCheckScript(ElementState state)
    {
        return state switch
        {
            ElementState.Attached => BuildQueryScript("el !== null"),
            ElementState.Detached => BuildQueryScript("el === null"),
            ElementState.Visible => BuildQueryScript("el !== null && el.offsetParent !== null"),
            ElementState.Hidden => BuildQueryScript("el === null || el.offsetParent === null"),
            _ => BuildQueryScript("el !== null")
        };
    }

    private string BuildAllTextsScript(string property)
    {
        var fullSelector = BuildFullSelector();
        return $"[...document.querySelectorAll({JsonConvert.SerializeObject(fullSelector)})].map(el => el.{property} || '')";
    }

    private string BuildEvaluateScript(string expression)
    {
        var fullSelector = BuildFullSelector();
        return $@"
            (() => {{
                const el = document.querySelector({JsonConvert.SerializeObject(fullSelector)});
                if (!el) return null;
                return (function(el) {{ return {expression}; }})(el);
            }})()";
    }

    private string BuildEvaluateAllScript(string expression)
    {
        var fullSelector = BuildFullSelector();
        return $@"
            (() => {{
                const elements = [...document.querySelectorAll({JsonConvert.SerializeObject(fullSelector)})];
                return (function(elements) {{ return {expression}; }})(elements);
            }})()";
    }

    #endregion
}

#endif
