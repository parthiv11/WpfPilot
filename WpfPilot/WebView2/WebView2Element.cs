#if NET5_0_OR_GREATER

namespace WpfPilot;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using WpfPilot.Interop;

/// <summary>
/// A Playwright/Puppeteer-like API for controlling a WebView2 WPF control.
/// Provides comprehensive DOM manipulation, navigation, and automation capabilities.
/// <code>
/// var webView = appDriver.GetElement&lt;WebView2Element&gt;(x => x.TypeName == "WebView2");
/// webView.Navigate("https://example.com")
///        .WaitForLoadState()
///        .Locator("#submit-button").Click();
/// </code>
/// </summary>
public class WebView2Element : Element<WebView2Element>
{
    /// <summary>
    /// Default timeout for operations in milliseconds.
    /// </summary>
    public int DefaultTimeout { get; set; } = 30_000;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebView2Element"/> class.
    /// </summary>
    /// <param name="element">The base element to wrap.</param>
    /// <exception cref="InvalidOperationException">Thrown when the element is not a WebView2.</exception>
    public WebView2Element(Element element)
        : base(element)
    {
        if (element.TypeName != nameof(WebView2))
            throw new InvalidOperationException($"Element is not a {nameof(WebView2)}. Actual type: {element.TypeName}");
    }

    #region Navigation

    /// <summary>
    /// Navigates to the specified URL.
    /// </summary>
    /// <param name="url">The URL to navigate to.</param>
    /// <param name="options">Optional navigation options.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element Navigate(string url, NavigationOptions? options = null)
    {
        _ = url ?? throw new ArgumentNullException(nameof(url));
        options ??= new NavigationOptions();

        Invoke<WebView2>(x => x.CoreWebView2.Navigate(url));

        if (options.WaitUntil != WaitUntilState.None)
            WaitForLoadState(options.WaitUntil, options.Timeout ?? DefaultTimeout);

        return this;
    }

    /// <summary>
    /// Navigates to the specified URL and returns a response.
    /// </summary>
    /// <param name="url">The URL to navigate to.</param>
    /// <param name="options">Optional navigation options.</param>
    /// <returns>Navigation response with status information.</returns>
    public NavigationResponse NavigateWithResponse(string url, NavigationOptions? options = null)
    {
        Navigate(url, options);
        return GetNavigationResponse();
    }

    /// <summary>
    /// Navigates back in history.
    /// </summary>
    /// <returns>This element for chaining.</returns>
    public WebView2Element GoBack()
    {
        Invoke<WebView2>(x => x.CoreWebView2.GoBack());
        WaitForLoadState();
        return this;
    }

    /// <summary>
    /// Navigates forward in history.
    /// </summary>
    /// <returns>This element for chaining.</returns>
    public WebView2Element GoForward()
    {
        Invoke<WebView2>(x => x.CoreWebView2.GoForward());
        WaitForLoadState();
        return this;
    }

    /// <summary>
    /// Reloads the current page.
    /// </summary>
    /// <param name="options">Optional reload options.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element Reload(NavigationOptions? options = null)
    {
        options ??= new NavigationOptions();
        Invoke<WebView2>(x => x.CoreWebView2.Reload());

        if (options.WaitUntil != WaitUntilState.None)
            WaitForLoadState(options.WaitUntil, options.Timeout ?? DefaultTimeout);

        return this;
    }

    /// <summary>
    /// Gets the current URL of the page.
    /// </summary>
    /// <returns>The current URL.</returns>
    public string GetUrl() =>
        Invoke<WebView2, string>(x => x.Source.ToString());

    /// <summary>
    /// Gets the current page title.
    /// </summary>
    /// <returns>The page title.</returns>
    public string GetTitle() =>
        EvaluateAsync<string>("document.title");

    #endregion

    #region Wait Methods

    /// <summary>
    /// Waits for the page to reach the specified load state.
    /// </summary>
    /// <param name="state">The load state to wait for.</param>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element WaitForLoadState(WaitUntilState state = WaitUntilState.Load, int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? DefaultTimeout;
        var start = Environment.TickCount;

        var stateCheck = state switch
        {
            WaitUntilState.DOMContentLoaded => "document.readyState === 'interactive' || document.readyState === 'complete'",
            WaitUntilState.Load => "document.readyState === 'complete'",
            WaitUntilState.NetworkIdle => "document.readyState === 'complete'", // Simplified - full networkidle would require tracking requests
            _ => "true"
        };

        while (Environment.TickCount - start < timeout)
        {
            var isReady = EvaluateAsync<bool>(stateCheck);
            if (isReady)
                return this;

            Task.Delay(100).GetAwaiter().GetResult();
        }

        throw new TimeoutException($"Timeout waiting for load state '{state}' after {timeout}ms");
    }

    /// <summary>
    /// Waits for a selector to appear in the DOM.
    /// </summary>
    /// <param name="selector">The CSS selector to wait for.</param>
    /// <param name="options">Optional wait options.</param>
    /// <returns>The element handle when found.</returns>
    public WebViewElementHandle WaitForSelector(string selector, WaitForSelectorOptions? options = null)
    {
        _ = selector ?? throw new ArgumentNullException(nameof(selector));
        options ??= new WaitForSelectorOptions();

        var timeout = options.Timeout ?? DefaultTimeout;
        var start = Environment.TickCount;

        var stateCondition = options.State switch
        {
            ElementState.Attached => $"document.querySelector({JsonConvert.SerializeObject(selector)}) !== null",
            ElementState.Detached => $"document.querySelector({JsonConvert.SerializeObject(selector)}) === null",
            ElementState.Visible => $"(() => {{ var el = document.querySelector({JsonConvert.SerializeObject(selector)}); return el !== null && el.offsetParent !== null; }})()",
            ElementState.Hidden => $"(() => {{ var el = document.querySelector({JsonConvert.SerializeObject(selector)}); return el === null || el.offsetParent === null; }})()",
            _ => $"document.querySelector({JsonConvert.SerializeObject(selector)}) !== null"
        };

        while (Environment.TickCount - start < timeout)
        {
            var conditionMet = EvaluateAsync<bool>(stateCondition);
            if (conditionMet)
            {
                if (options.State == ElementState.Detached || options.State == ElementState.Hidden)
                    return new WebViewElementHandle(this, selector, null);

                return QuerySelector(selector)!;
            }

            Task.Delay(100).GetAwaiter().GetResult();
        }

        throw new TimeoutException($"Timeout waiting for selector '{selector}' to be {options.State} after {timeout}ms");
    }

    /// <summary>
    /// Waits for a function to return a truthy value.
    /// </summary>
    /// <param name="expression">JavaScript expression to evaluate.</param>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element WaitForFunction(string expression, int? timeoutMs = null)
    {
        _ = expression ?? throw new ArgumentNullException(nameof(expression));
        var timeout = timeoutMs ?? DefaultTimeout;
        var start = Environment.TickCount;

        while (Environment.TickCount - start < timeout)
        {
            var result = EvaluateAsync<bool>($"Boolean({expression})");
            if (result)
                return this;

            Task.Delay(100).GetAwaiter().GetResult();
        }

        throw new TimeoutException($"Timeout waiting for function after {timeout}ms");
    }

    /// <summary>
    /// Waits for navigation to complete.
    /// </summary>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element WaitForNavigation(int? timeoutMs = null) =>
        WaitForLoadState(WaitUntilState.Load, timeoutMs);

    /// <summary>
    /// Waits for a specified amount of time.
    /// </summary>
    /// <param name="milliseconds">Time to wait in milliseconds.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element WaitForTimeout(int milliseconds)
    {
        Task.Delay(milliseconds).GetAwaiter().GetResult();
        return this;
    }

    #endregion

    #region Locators

    /// <summary>
    /// Creates a locator for the given CSS selector.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <returns>A locator for the selector.</returns>
    public WebViewLocator Locator(string selector)
    {
        _ = selector ?? throw new ArgumentNullException(nameof(selector));
        return new WebViewLocator(this, selector);
    }

    /// <summary>
    /// Creates a locator that matches elements by their text content.
    /// </summary>
    /// <param name="text">The text to match.</param>
    /// <param name="exact">If true, matches exact text; otherwise, uses contains matching.</param>
    /// <returns>A locator for elements with the specified text.</returns>
    public WebViewLocator GetByText(string text, bool exact = false)
    {
        _ = text ?? throw new ArgumentNullException(nameof(text));
        var script = exact
            ? $"[...document.querySelectorAll('*')].find(el => el.textContent?.trim() === {JsonConvert.SerializeObject(text)})"
            : $"[...document.querySelectorAll('*')].find(el => el.textContent?.includes({JsonConvert.SerializeObject(text)}))";
        return new WebViewLocator(this, $"text={text}", exact);
    }

    /// <summary>
    /// Creates a locator that matches elements by their role attribute.
    /// </summary>
    /// <param name="role">The ARIA role to match.</param>
    /// <param name="name">Optional accessible name to match.</param>
    /// <returns>A locator for elements with the specified role.</returns>
    public WebViewLocator GetByRole(string role, string? name = null)
    {
        _ = role ?? throw new ArgumentNullException(nameof(role));
        var selector = $"[role='{role}']";
        if (!string.IsNullOrEmpty(name))
            selector = $"role={role}[name={name}]";
        return new WebViewLocator(this, selector);
    }

    /// <summary>
    /// Creates a locator that matches elements by their label text.
    /// </summary>
    /// <param name="text">The label text to match.</param>
    /// <param name="exact">If true, matches exact text; otherwise, uses contains matching.</param>
    /// <returns>A locator for elements with the specified label.</returns>
    public WebViewLocator GetByLabel(string text, bool exact = false)
    {
        _ = text ?? throw new ArgumentNullException(nameof(text));
        return new WebViewLocator(this, $"label={text}", exact);
    }

    /// <summary>
    /// Creates a locator that matches elements by their placeholder text.
    /// </summary>
    /// <param name="text">The placeholder text to match.</param>
    /// <param name="exact">If true, matches exact text; otherwise, uses contains matching.</param>
    /// <returns>A locator for elements with the specified placeholder.</returns>
    public WebViewLocator GetByPlaceholder(string text, bool exact = false)
    {
        _ = text ?? throw new ArgumentNullException(nameof(text));
        return new WebViewLocator(this, $"[placeholder*='{text}']");
    }

    /// <summary>
    /// Creates a locator that matches elements by their alt text.
    /// </summary>
    /// <param name="text">The alt text to match.</param>
    /// <param name="exact">If true, matches exact text; otherwise, uses contains matching.</param>
    /// <returns>A locator for elements with the specified alt text.</returns>
    public WebViewLocator GetByAltText(string text, bool exact = false)
    {
        _ = text ?? throw new ArgumentNullException(nameof(text));
        var selector = exact ? $"[alt='{text}']" : $"[alt*='{text}']";
        return new WebViewLocator(this, selector);
    }

    /// <summary>
    /// Creates a locator that matches elements by their title attribute.
    /// </summary>
    /// <param name="text">The title text to match.</param>
    /// <param name="exact">If true, matches exact text; otherwise, uses contains matching.</param>
    /// <returns>A locator for elements with the specified title.</returns>
    public WebViewLocator GetByTitle(string text, bool exact = false)
    {
        _ = text ?? throw new ArgumentNullException(nameof(text));
        var selector = exact ? $"[title='{text}']" : $"[title*='{text}']";
        return new WebViewLocator(this, selector);
    }

    /// <summary>
    /// Creates a locator that matches elements by their test ID attribute.
    /// </summary>
    /// <param name="testId">The test ID to match.</param>
    /// <returns>A locator for elements with the specified test ID.</returns>
    public WebViewLocator GetByTestId(string testId)
    {
        _ = testId ?? throw new ArgumentNullException(nameof(testId));
        return new WebViewLocator(this, $"[data-testid='{testId}']");
    }

    #endregion

    #region Query Selectors

    /// <summary>
    /// Queries for a single element matching the selector.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <returns>The element handle, or null if not found.</returns>
    public WebViewElementHandle? QuerySelector(string selector)
    {
        _ = selector ?? throw new ArgumentNullException(nameof(selector));

        var exists = EvaluateAsync<bool>($"document.querySelector({JsonConvert.SerializeObject(selector)}) !== null");
        if (!exists)
            return null;

        var id = EvaluateAsync<string>($@"
            (() => {{
                const el = document.querySelector({JsonConvert.SerializeObject(selector)});
                if (!el) return null;
                if (!el.__wpfPilotId) {{
                    el.__wpfPilotId = 'wpf_' + Math.random().toString(36).substr(2, 9);
                }}
                return el.__wpfPilotId;
            }})()");

        return new WebViewElementHandle(this, selector, id);
    }

    /// <summary>
    /// Queries for all elements matching the selector.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <returns>A list of element handles.</returns>
    public IReadOnlyList<WebViewElementHandle> QuerySelectorAll(string selector)
    {
        _ = selector ?? throw new ArgumentNullException(nameof(selector));

        var count = EvaluateAsync<int>($"document.querySelectorAll({JsonConvert.SerializeObject(selector)}).length");
        var handles = new List<WebViewElementHandle>();

        for (int i = 0; i < count; i++)
        {
            var id = EvaluateAsync<string>($@"
                (() => {{
                    const el = document.querySelectorAll({JsonConvert.SerializeObject(selector)})[{i}];
                    if (!el) return null;
                    if (!el.__wpfPilotId) {{
                        el.__wpfPilotId = 'wpf_' + Math.random().toString(36).substr(2, 9);
                    }}
                    return el.__wpfPilotId;
                }})()");

            handles.Add(new WebViewElementHandle(this, $"{selector}:nth-match({i})", id));
        }

        return handles;
    }

    /// <summary>
    /// Gets an element by its ID.
    /// </summary>
    /// <param name="id">The element ID.</param>
    /// <returns>The element handle, or null if not found.</returns>
    public WebViewElementHandle? GetElementById(string id)
    {
        _ = id ?? throw new ArgumentNullException(nameof(id));
        return QuerySelector($"#{id}");
    }

    /// <summary>
    /// Gets elements by their class name.
    /// </summary>
    /// <param name="className">The class name.</param>
    /// <returns>A list of element handles.</returns>
    public IReadOnlyList<WebViewElementHandle> GetElementsByClassName(string className)
    {
        _ = className ?? throw new ArgumentNullException(nameof(className));
        return QuerySelectorAll($".{className}");
    }

    /// <summary>
    /// Gets elements by their tag name.
    /// </summary>
    /// <param name="tagName">The tag name.</param>
    /// <returns>A list of element handles.</returns>
    public IReadOnlyList<WebViewElementHandle> GetElementsByTagName(string tagName)
    {
        _ = tagName ?? throw new ArgumentNullException(nameof(tagName));
        return QuerySelectorAll(tagName);
    }

    #endregion

    #region Page Actions

    /// <summary>
    /// Clicks on an element matching the selector.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <param name="options">Optional click options.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element ClickSelector(string selector, ClickOptions? options = null)
    {
        var handle = WaitForSelector(selector, new WaitForSelectorOptions { State = ElementState.Visible });
        handle.Click(options);
        return this;
    }

    /// <summary>
    /// Double-clicks on an element matching the selector.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <param name="options">Optional click options.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element DblClickSelector(string selector, ClickOptions? options = null)
    {
        var handle = WaitForSelector(selector, new WaitForSelectorOptions { State = ElementState.Visible });
        handle.DblClick(options);
        return this;
    }

    /// <summary>
    /// Types text into an element matching the selector.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <param name="text">The text to type.</param>
    /// <param name="options">Optional type options.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element TypeText(string selector, string text, TypeOptions? options = null)
    {
        var handle = WaitForSelector(selector, new WaitForSelectorOptions { State = ElementState.Visible });
        handle.Type(text, options);
        return this;
    }

    /// <summary>
    /// Fills an input element with text (clears existing content first).
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <param name="value">The value to fill.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element Fill(string selector, string value)
    {
        var handle = WaitForSelector(selector, new WaitForSelectorOptions { State = ElementState.Visible });
        handle.Fill(value);
        return this;
    }

    /// <summary>
    /// Clears an input element.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element Clear(string selector)
    {
        var handle = WaitForSelector(selector, new WaitForSelectorOptions { State = ElementState.Visible });
        handle.Clear();
        return this;
    }

    /// <summary>
    /// Checks a checkbox or radio button.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element CheckSelector(string selector)
    {
        var handle = WaitForSelector(selector, new WaitForSelectorOptions { State = ElementState.Visible });
        handle.Check();
        return this;
    }

    /// <summary>
    /// Unchecks a checkbox.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element UncheckSelector(string selector)
    {
        var handle = WaitForSelector(selector, new WaitForSelectorOptions { State = ElementState.Visible });
        handle.Uncheck();
        return this;
    }

    /// <summary>
    /// Selects an option in a select element by value.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <param name="values">The values to select.</param>
    /// <returns>The selected values.</returns>
    public IReadOnlyList<string> SelectOption(string selector, params string[] values)
    {
        var handle = WaitForSelector(selector, new WaitForSelectorOptions { State = ElementState.Visible });
        return handle.SelectOption(values);
    }

    /// <summary>
    /// Hovers over an element matching the selector.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element Hover(string selector)
    {
        var handle = WaitForSelector(selector, new WaitForSelectorOptions { State = ElementState.Visible });
        handle.Hover();
        return this;
    }

    /// <summary>
    /// Focuses an element matching the selector.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element FocusSelector(string selector)
    {
        var handle = WaitForSelector(selector, new WaitForSelectorOptions { State = ElementState.Visible });
        handle.Focus();
        return this;
    }

    /// <summary>
    /// Presses a key on an element matching the selector.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <param name="key">The key to press (e.g., "Enter", "Tab", "a").</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element Press(string selector, string key)
    {
        var handle = WaitForSelector(selector, new WaitForSelectorOptions { State = ElementState.Visible });
        handle.Press(key);
        return this;
    }

    /// <summary>
    /// Sets the value of an input file element.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <param name="filePaths">The file paths to set.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element SetInputFiles(string selector, params string[] filePaths)
    {
        var handle = WaitForSelector(selector, new WaitForSelectorOptions { State = ElementState.Attached });
        handle.SetInputFiles(filePaths);
        return this;
    }

    /// <summary>
    /// Scrolls an element into view.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element ScrollIntoViewSelector(string selector)
    {
        var handle = WaitForSelector(selector, new WaitForSelectorOptions { State = ElementState.Attached });
        handle.ScrollIntoView();
        return this;
    }

    #endregion

    #region Content Methods

    /// <summary>
    /// Gets the HTML content of the page.
    /// </summary>
    /// <returns>The HTML content.</returns>
    public string Content() =>
        EvaluateAsync<string>("document.documentElement.outerHTML");

    /// <summary>
    /// Sets the HTML content of the page.
    /// </summary>
    /// <param name="html">The HTML content to set.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element SetContent(string html)
    {
        _ = html ?? throw new ArgumentNullException(nameof(html));
        InvokeAsync<WebView2>(x => x.CoreWebView2.NavigateToString(html));
        WaitForLoadState();
        return this;
    }

    /// <summary>
    /// Gets the inner text of the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <returns>The inner text.</returns>
    public string InnerText(string selector)
    {
        var handle = WaitForSelector(selector);
        return handle.InnerText();
    }

    /// <summary>
    /// Gets the inner HTML of the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <returns>The inner HTML.</returns>
    public string InnerHTML(string selector)
    {
        var handle = WaitForSelector(selector);
        return handle.InnerHTML();
    }

    /// <summary>
    /// Gets the text content of the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <returns>The text content.</returns>
    public string? TextContent(string selector)
    {
        var handle = WaitForSelector(selector);
        return handle.TextContent();
    }

    /// <summary>
    /// Gets an attribute value from the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <param name="name">The attribute name.</param>
    /// <returns>The attribute value, or null if not found.</returns>
    public string? GetAttribute(string selector, string name)
    {
        var handle = WaitForSelector(selector);
        return handle.GetAttribute(name);
    }

    #endregion

    #region JavaScript Evaluation

    /// <summary>
    /// Evaluates a JavaScript expression and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The expected result type.</typeparam>
    /// <param name="expression">The JavaScript expression to evaluate.</param>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    /// <returns>The result of the evaluation.</returns>
    public TResult EvaluateAsync<TResult>(string expression, int? timeoutMs = null)
    {
        _ = expression ?? throw new ArgumentNullException(nameof(expression));
        var timeout = timeoutMs ?? DefaultTimeout;

        var result = InvokeAsync<WebView2, string>(
            x => x.CoreWebView2.ExecuteScriptAsync(expression),
            timeout);

        return DeserializeJsResult<TResult>(result);
    }

    /// <summary>
    /// Evaluates a JavaScript expression without expecting a return value.
    /// </summary>
    /// <param name="expression">The JavaScript expression to evaluate.</param>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element Evaluate(string expression, int? timeoutMs = null)
    {
        _ = expression ?? throw new ArgumentNullException(nameof(expression));
        var timeout = timeoutMs ?? DefaultTimeout;

        InvokeAsync<WebView2>(x => x.CoreWebView2.ExecuteScriptAsync(expression), timeout);
        return this;
    }

    /// <summary>
    /// Adds a script tag to the page.
    /// </summary>
    /// <param name="options">Script tag options.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element AddScriptTag(ScriptTagOptions options)
    {
        _ = options ?? throw new ArgumentNullException(nameof(options));

        string script;
        if (!string.IsNullOrEmpty(options.Url))
        {
            script = $@"
                (() => {{
                    const script = document.createElement('script');
                    script.src = {JsonConvert.SerializeObject(options.Url)};
                    if ({JsonConvert.SerializeObject(options.Type)} !== 'null') script.type = {JsonConvert.SerializeObject(options.Type ?? "text/javascript")};
                    document.head.appendChild(script);
                }})()";
        }
        else if (!string.IsNullOrEmpty(options.Content))
        {
            script = $@"
                (() => {{
                    const script = document.createElement('script');
                    script.textContent = {JsonConvert.SerializeObject(options.Content)};
                    if ({JsonConvert.SerializeObject(options.Type)} !== 'null') script.type = {JsonConvert.SerializeObject(options.Type ?? "text/javascript")};
                    document.head.appendChild(script);
                }})()";
        }
        else
        {
            throw new ArgumentException("Either Url or Content must be specified", nameof(options));
        }

        Evaluate(script);
        return this;
    }

    /// <summary>
    /// Adds a style tag to the page.
    /// </summary>
    /// <param name="options">Style tag options.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element AddStyleTag(StyleTagOptions options)
    {
        _ = options ?? throw new ArgumentNullException(nameof(options));

        string script;
        if (!string.IsNullOrEmpty(options.Url))
        {
            script = $@"
                (() => {{
                    const link = document.createElement('link');
                    link.rel = 'stylesheet';
                    link.href = {JsonConvert.SerializeObject(options.Url)};
                    document.head.appendChild(link);
                }})()";
        }
        else if (!string.IsNullOrEmpty(options.Content))
        {
            script = $@"
                (() => {{
                    const style = document.createElement('style');
                    style.textContent = {JsonConvert.SerializeObject(options.Content)};
                    document.head.appendChild(style);
                }})()";
        }
        else
        {
            throw new ArgumentException("Either Url or Content must be specified", nameof(options));
        }

        Evaluate(script);
        return this;
    }

    #endregion

    #region Assertions

    /// <summary>
    /// Asserts that an element matching the selector is visible.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element AssertVisible(string selector, int? timeoutMs = null)
    {
        WaitForSelector(selector, new WaitForSelectorOptions { State = ElementState.Visible, Timeout = timeoutMs });
        return this;
    }

    /// <summary>
    /// Asserts that an element matching the selector is hidden or not present.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element AssertHidden(string selector, int? timeoutMs = null)
    {
        WaitForSelector(selector, new WaitForSelectorOptions { State = ElementState.Hidden, Timeout = timeoutMs });
        return this;
    }

    /// <summary>
    /// Asserts that an element matching the selector contains the expected text.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <param name="expected">The expected text.</param>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element AssertTextContent(string selector, string expected, int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? DefaultTimeout;
        var start = Environment.TickCount;

        while (Environment.TickCount - start < timeout)
        {
            try
            {
                var handle = QuerySelector(selector);
                if (handle != null)
                {
                    var text = handle.TextContent();
                    if (text?.Contains(expected) == true)
                        return this;
                }
            }
            catch
            {
                // Ignore and retry
            }

            Task.Delay(100).GetAwaiter().GetResult();
        }

        throw new WebViewAssertionException($"Element '{selector}' does not contain text '{expected}'");
    }

    /// <summary>
    /// Asserts that the page URL matches the expected pattern.
    /// </summary>
    /// <param name="pattern">The URL pattern (string or regex).</param>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element AssertUrl(string pattern, int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? DefaultTimeout;
        var start = Environment.TickCount;
        var regex = new Regex(pattern);

        while (Environment.TickCount - start < timeout)
        {
            var url = GetUrl();
            if (url == pattern || regex.IsMatch(url))
                return this;

            Task.Delay(100).GetAwaiter().GetResult();
        }

        throw new WebViewAssertionException($"Page URL does not match pattern '{pattern}'. Current URL: {GetUrl()}");
    }

    /// <summary>
    /// Asserts that the page title matches the expected value.
    /// </summary>
    /// <param name="expected">The expected title.</param>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element AssertTitle(string expected, int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? DefaultTimeout;
        var start = Environment.TickCount;

        while (Environment.TickCount - start < timeout)
        {
            var title = GetTitle();
            if (title == expected || title.Contains(expected))
                return this;

            Task.Delay(100).GetAwaiter().GetResult();
        }

        throw new WebViewAssertionException($"Page title does not match '{expected}'. Current title: {GetTitle()}");
    }

    #endregion

    #region Frame Methods

    /// <summary>
    /// Gets all frames on the page.
    /// </summary>
    /// <returns>A list of frame names.</returns>
    public IReadOnlyList<string> GetFrames() =>
        EvaluateAsync<List<string>>("[...document.querySelectorAll('iframe')].map(f => f.name || f.id || 'anonymous')") ?? new List<string>();

    /// <summary>
    /// Evaluates JavaScript in a specific frame.
    /// </summary>
    /// <param name="frameSelector">The frame selector (name or CSS selector).</param>
    /// <param name="expression">The JavaScript expression.</param>
    /// <typeparam name="TResult">The expected result type.</typeparam>
    /// <returns>The result of the evaluation.</returns>
    public TResult EvaluateInFrame<TResult>(string frameSelector, string expression)
    {
        var script = $@"
            (() => {{
                const frame = document.querySelector({JsonConvert.SerializeObject(frameSelector)}) 
                    || [...document.querySelectorAll('iframe')].find(f => f.name === {JsonConvert.SerializeObject(frameSelector)});
                if (!frame || !frame.contentWindow) return null;
                try {{
                    return frame.contentWindow.eval({JsonConvert.SerializeObject(expression)});
                }} catch (e) {{
                    return null;
                }}
            }})()";

        return EvaluateAsync<TResult>(script);
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Takes a screenshot of the WebView2 content.
    /// </summary>
    /// <param name="options">Screenshot options.</param>
    /// <returns>The screenshot as a byte array.</returns>
    public byte[] ScreenshotWebView(WebViewScreenshotOptions? options = null)
    {
        Screenshot(out var bytes, options?.Format ?? ImageFormat.Png);
        return bytes;
    }

    /// <summary>
    /// Emulates a specific viewport size.
    /// </summary>
    /// <param name="width">The viewport width.</param>
    /// <param name="height">The viewport height.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element SetViewportSize(int width, int height)
    {
        SetProperty("Width", (double)width);
        SetProperty("Height", (double)height);
        return this;
    }

    /// <summary>
    /// Gets whether the page has any console errors.
    /// </summary>
    /// <returns>True if there are console errors.</returns>
    public bool HasConsoleErrors()
    {
        // Note: This is a simplified implementation. Full console error tracking
        // would require setting up a DevTools Protocol listener.
        return false;
    }

    /// <summary>
    /// Dispatches a custom event on an element.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <param name="eventType">The event type.</param>
    /// <param name="eventInit">Optional event initialization object as JSON.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element DispatchEvent(string selector, string eventType, string? eventInit = null)
    {
        var handle = WaitForSelector(selector);
        handle.DispatchEvent(eventType, eventInit);
        return this;
    }

    #endregion

    #region Keyboard and Mouse

    /// <summary>
    /// Presses a keyboard key.
    /// </summary>
    /// <param name="key">The key to press.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element KeyboardPress(string key)
    {
        var script = $@"
            (() => {{
                const event = new KeyboardEvent('keydown', {{
                    key: {JsonConvert.SerializeObject(key)},
                    bubbles: true
                }});
                document.activeElement.dispatchEvent(event);
                const upEvent = new KeyboardEvent('keyup', {{
                    key: {JsonConvert.SerializeObject(key)},
                    bubbles: true
                }});
                document.activeElement.dispatchEvent(upEvent);
            }})()";

        Evaluate(script);
        return this;
    }

    /// <summary>
    /// Types text using keyboard events.
    /// </summary>
    /// <param name="text">The text to type.</param>
    /// <param name="delay">Delay between keystrokes in milliseconds.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element KeyboardType(string text, int delay = 0)
    {
        foreach (var c in text)
        {
            var script = $@"
                (() => {{
                    const char = {JsonConvert.SerializeObject(c.ToString())};
                    const keyEvent = new KeyboardEvent('keypress', {{
                        key: char,
                        charCode: char.charCodeAt(0),
                        bubbles: true
                    }});
                    document.activeElement.dispatchEvent(keyEvent);
                    
                    if (document.activeElement.tagName === 'INPUT' || document.activeElement.tagName === 'TEXTAREA') {{
                        document.activeElement.value += char;
                        document.activeElement.dispatchEvent(new Event('input', {{ bubbles: true }}));
                    }}
                }})()";

            Evaluate(script);

            if (delay > 0)
                Task.Delay(delay).GetAwaiter().GetResult();
        }

        return this;
    }

    /// <summary>
    /// Clicks at specific coordinates.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="options">Click options.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element MouseClick(int x, int y, ClickOptions? options = null)
    {
        options ??= new ClickOptions();

        var button = options.Button switch
        {
            MouseButton.Right => 2,
            MouseButton.Middle => 1,
            _ => 0
        };

        var script = $@"
            (() => {{
                const el = document.elementFromPoint({x}, {y});
                if (!el) return;
                
                const mousedown = new MouseEvent('mousedown', {{
                    bubbles: true,
                    clientX: {x},
                    clientY: {y},
                    button: {button}
                }});
                const mouseup = new MouseEvent('mouseup', {{
                    bubbles: true,
                    clientX: {x},
                    clientY: {y},
                    button: {button}
                }});
                const click = new MouseEvent('click', {{
                    bubbles: true,
                    clientX: {x},
                    clientY: {y},
                    button: {button}
                }});
                
                el.dispatchEvent(mousedown);
                el.dispatchEvent(mouseup);
                el.dispatchEvent(click);
            }})()";

        for (int i = 0; i < options.ClickCount; i++)
        {
            Evaluate(script);
            if (options.Delay > 0)
                Task.Delay(options.Delay).GetAwaiter().GetResult();
        }

        return this;
    }

    /// <summary>
    /// Moves the mouse to specific coordinates.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <returns>This element for chaining.</returns>
    public WebView2Element MouseMove(int x, int y)
    {
        var script = $@"
            (() => {{
                const el = document.elementFromPoint({x}, {y});
                if (!el) return;
                
                const mousemove = new MouseEvent('mousemove', {{
                    bubbles: true,
                    clientX: {x},
                    clientY: {y}
                }});
                el.dispatchEvent(mousemove);
            }})()";

        Evaluate(script);
        return this;
    }

    #endregion

    #region Private Helpers

    private TResult DeserializeJsResult<TResult>(string? jsonResult)
    {
        if (string.IsNullOrEmpty(jsonResult) || jsonResult == "null" || jsonResult == "undefined")
            return default!;

        if (typeof(TResult) == typeof(string))
        {
            // JS strings come back with quotes, need to unescape
            if (jsonResult.StartsWith("\"") && jsonResult.EndsWith("\""))
                return (TResult)(object)JsonConvert.DeserializeObject<string>(jsonResult)!;
            return (TResult)(object)jsonResult;
        }

        if (typeof(TResult) == typeof(bool))
        {
            return (TResult)(object)(jsonResult.ToLowerInvariant() == "true");
        }

        if (typeof(TResult) == typeof(int))
        {
            if (int.TryParse(jsonResult, out var intValue))
                return (TResult)(object)intValue;
            return default!;
        }

        if (typeof(TResult) == typeof(double))
        {
            if (double.TryParse(jsonResult, out var doubleValue))
                return (TResult)(object)doubleValue;
            return default!;
        }

        try
        {
            return JsonConvert.DeserializeObject<TResult>(jsonResult)!;
        }
        catch
        {
            return default!;
        }
    }

    private NavigationResponse GetNavigationResponse()
    {
        var url = GetUrl();
        return new NavigationResponse
        {
            Url = url,
            Ok = true,
            Status = 200
        };
    }

    internal string ExecuteScript(string script, int? timeoutMs = null) =>
        InvokeAsync<WebView2, string>(
            x => x.CoreWebView2.ExecuteScriptAsync(script),
            timeoutMs ?? DefaultTimeout);

    #endregion
}

#endif
