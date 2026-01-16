#if NET5_0_OR_GREATER

namespace WpfPilot;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// Represents a handle to a DOM element in the WebView2.
/// Provides methods to interact with and query the element.
/// </summary>
public class WebViewElementHandle
{
    private readonly WebView2Element _webView;
    private readonly string _selector;
    private readonly string? _elementId;

    /// <summary>
    /// Creates a new element handle.
    /// </summary>
    internal WebViewElementHandle(WebView2Element webView, string selector, string? elementId)
    {
        _webView = webView ?? throw new ArgumentNullException(nameof(webView));
        _selector = selector ?? throw new ArgumentNullException(nameof(selector));
        _elementId = elementId;
    }

    /// <summary>
    /// Gets the selector used to find this element.
    /// </summary>
    public string Selector => _selector;

    #region Actions

    /// <summary>
    /// Clicks the element.
    /// </summary>
    /// <param name="options">Optional click options.</param>
    public void Click(ClickOptions? options = null)
    {
        options ??= new ClickOptions();
        EnsureVisible();

        var button = options.Button switch
        {
            MouseButton.Right => 2,
            MouseButton.Middle => 1,
            _ => 0
        };

        var script = BuildScript($@"
            const rect = el.getBoundingClientRect();
            const x = rect.left + rect.width / 2;
            const y = rect.top + rect.height / 2;
            
            for (let i = 0; i < {options.ClickCount}; i++) {{
                const mousedown = new MouseEvent('mousedown', {{
                    bubbles: true,
                    cancelable: true,
                    view: window,
                    clientX: x,
                    clientY: y,
                    button: {button}
                }});
                const mouseup = new MouseEvent('mouseup', {{
                    bubbles: true,
                    cancelable: true,
                    view: window,
                    clientX: x,
                    clientY: y,
                    button: {button}
                }});
                const click = new MouseEvent('click', {{
                    bubbles: true,
                    cancelable: true,
                    view: window,
                    clientX: x,
                    clientY: y,
                    button: {button}
                }});
                
                el.dispatchEvent(mousedown);
                el.dispatchEvent(mouseup);
                el.dispatchEvent(click);
            }}
        ");

        _webView.Evaluate(script);

        if (options.Delay > 0)
            Task.Delay(options.Delay).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Double-clicks the element.
    /// </summary>
    /// <param name="options">Optional click options.</param>
    public void DblClick(ClickOptions? options = null)
    {
        options ??= new ClickOptions();
        EnsureVisible();

        var script = BuildScript($@"
            const rect = el.getBoundingClientRect();
            const x = rect.left + rect.width / 2;
            const y = rect.top + rect.height / 2;
            
            const dblclick = new MouseEvent('dblclick', {{
                bubbles: true,
                cancelable: true,
                view: window,
                clientX: x,
                clientY: y,
                button: 0
            }});
            
            el.dispatchEvent(dblclick);
        ");

        _webView.Evaluate(script);
    }

    /// <summary>
    /// Types text into the element character by character.
    /// </summary>
    /// <param name="text">The text to type.</param>
    /// <param name="options">Optional type options.</param>
    public void Type(string text, TypeOptions? options = null)
    {
        _ = text ?? throw new ArgumentNullException(nameof(text));
        options ??= new TypeOptions();

        Focus();

        foreach (var c in text)
        {
            var script = BuildScript($@"
                const char = {JsonConvert.SerializeObject(c.ToString())};
                
                const keydownEvent = new KeyboardEvent('keydown', {{
                    key: char,
                    bubbles: true
                }});
                el.dispatchEvent(keydownEvent);
                
                const keypressEvent = new KeyboardEvent('keypress', {{
                    key: char,
                    charCode: char.charCodeAt(0),
                    bubbles: true
                }});
                el.dispatchEvent(keypressEvent);
                
                if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA' || el.isContentEditable) {{
                    if (el.isContentEditable) {{
                        el.textContent += char;
                    }} else {{
                        el.value += char;
                    }}
                    el.dispatchEvent(new Event('input', {{ bubbles: true }}));
                }}
                
                const keyupEvent = new KeyboardEvent('keyup', {{
                    key: char,
                    bubbles: true
                }});
                el.dispatchEvent(keyupEvent);
            ");

            _webView.Evaluate(script);

            if (options.Delay > 0)
                Task.Delay(options.Delay).GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Fills the element with text (clears existing content first).
    /// </summary>
    /// <param name="value">The value to fill.</param>
    public void Fill(string value)
    {
        _ = value ?? throw new ArgumentNullException(nameof(value));

        Focus();
        Clear();

        var script = BuildScript($@"
            if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA') {{
                el.value = {JsonConvert.SerializeObject(value)};
                el.dispatchEvent(new Event('input', {{ bubbles: true }}));
                el.dispatchEvent(new Event('change', {{ bubbles: true }}));
            }} else if (el.isContentEditable) {{
                el.textContent = {JsonConvert.SerializeObject(value)};
                el.dispatchEvent(new Event('input', {{ bubbles: true }}));
            }} else {{
                // Try to fill by setting value property
                el.value = {JsonConvert.SerializeObject(value)};
                el.dispatchEvent(new Event('input', {{ bubbles: true }}));
                el.dispatchEvent(new Event('change', {{ bubbles: true }}));
            }}
        ");

        _webView.Evaluate(script);
    }

    /// <summary>
    /// Clears the element's value.
    /// </summary>
    public void Clear()
    {
        var script = BuildScript(@"
            if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA') {
                el.value = '';
                el.dispatchEvent(new Event('input', { bubbles: true }));
                el.dispatchEvent(new Event('change', { bubbles: true }));
            } else if (el.isContentEditable) {
                el.textContent = '';
                el.dispatchEvent(new Event('input', { bubbles: true }));
            }
        ");

        _webView.Evaluate(script);
    }

    /// <summary>
    /// Presses a key while focused on the element.
    /// </summary>
    /// <param name="key">The key to press (e.g., "Enter", "Tab", "Escape").</param>
    public void Press(string key)
    {
        _ = key ?? throw new ArgumentNullException(nameof(key));

        Focus();

        var script = BuildScript($@"
            const key = {JsonConvert.SerializeObject(key)};
            
            const keydownEvent = new KeyboardEvent('keydown', {{
                key: key,
                code: key,
                bubbles: true
            }});
            el.dispatchEvent(keydownEvent);
            
            const keyupEvent = new KeyboardEvent('keyup', {{
                key: key,
                code: key,
                bubbles: true
            }});
            el.dispatchEvent(keyupEvent);
        ");

        _webView.Evaluate(script);
    }

    /// <summary>
    /// Checks a checkbox or radio button.
    /// </summary>
    public void Check()
    {
        var script = BuildScript(@"
            if (el.type === 'checkbox' || el.type === 'radio') {
                if (!el.checked) {
                    el.checked = true;
                    el.dispatchEvent(new Event('change', { bubbles: true }));
                    el.dispatchEvent(new Event('input', { bubbles: true }));
                }
            }
        ");

        _webView.Evaluate(script);
    }

    /// <summary>
    /// Unchecks a checkbox.
    /// </summary>
    public void Uncheck()
    {
        var script = BuildScript(@"
            if (el.type === 'checkbox') {
                if (el.checked) {
                    el.checked = false;
                    el.dispatchEvent(new Event('change', { bubbles: true }));
                    el.dispatchEvent(new Event('input', { bubbles: true }));
                }
            }
        ");

        _webView.Evaluate(script);
    }

    /// <summary>
    /// Sets the checked state of a checkbox.
    /// </summary>
    /// <param name="isChecked">The desired checked state.</param>
    public void SetChecked(bool isChecked)
    {
        if (isChecked)
            Check();
        else
            Uncheck();
    }

    /// <summary>
    /// Selects options in a select element.
    /// </summary>
    /// <param name="values">The values to select.</param>
    /// <returns>The selected values.</returns>
    public IReadOnlyList<string> SelectOption(params string[] values)
    {
        var valuesJson = JsonConvert.SerializeObject(values);

        var script = BuildScript($@"
            if (el.tagName !== 'SELECT') {{
                return [];
            }}
            
            const valuesToSelect = {valuesJson};
            const selectedValues = [];
            
            for (const option of el.options) {{
                option.selected = valuesToSelect.includes(option.value) || valuesToSelect.includes(option.text);
                if (option.selected) {{
                    selectedValues.push(option.value);
                }}
            }}
            
            el.dispatchEvent(new Event('change', {{ bubbles: true }}));
            el.dispatchEvent(new Event('input', {{ bubbles: true }}));
            
            return selectedValues;
        ");

        return _webView.EvaluateAsync<List<string>>(script) ?? new List<string>();
    }

    /// <summary>
    /// Hovers over the element.
    /// </summary>
    public void Hover()
    {
        EnsureVisible();

        var script = BuildScript(@"
            const rect = el.getBoundingClientRect();
            const x = rect.left + rect.width / 2;
            const y = rect.top + rect.height / 2;
            
            const mouseenter = new MouseEvent('mouseenter', {
                bubbles: true,
                cancelable: true,
                view: window,
                clientX: x,
                clientY: y
            });
            const mouseover = new MouseEvent('mouseover', {
                bubbles: true,
                cancelable: true,
                view: window,
                clientX: x,
                clientY: y
            });
            
            el.dispatchEvent(mouseenter);
            el.dispatchEvent(mouseover);
        ");

        _webView.Evaluate(script);
    }

    /// <summary>
    /// Focuses the element.
    /// </summary>
    public void Focus()
    {
        var script = BuildScript("el.focus()");
        _webView.Evaluate(script);
    }

    /// <summary>
    /// Blurs (unfocuses) the element.
    /// </summary>
    public void Blur()
    {
        var script = BuildScript("el.blur()");
        _webView.Evaluate(script);
    }

    /// <summary>
    /// Scrolls the element into view.
    /// </summary>
    public void ScrollIntoView()
    {
        var script = BuildScript("el.scrollIntoView({ behavior: 'instant', block: 'center', inline: 'center' })");
        _webView.Evaluate(script);
    }

    /// <summary>
    /// Sets the input files for a file input element.
    /// </summary>
    /// <param name="filePaths">The file paths to set.</param>
    public void SetInputFiles(params string[] filePaths)
    {
        // Note: Due to browser security, setting files programmatically is limited.
        // This implementation provides a best-effort approach.
        var script = BuildScript($@"
            if (el.type !== 'file') {{
                throw new Error('Element is not a file input');
            }}
            // Files cannot be set directly via JavaScript due to security restrictions.
            // The user would need to use native automation or CDP for this.
            console.warn('SetInputFiles: Browser security prevents programmatic file setting');
        ");

        _webView.Evaluate(script);
    }

    /// <summary>
    /// Drags this element to another element.
    /// </summary>
    /// <param name="target">The target element handle.</param>
    public void DragTo(WebViewElementHandle target)
    {
        _ = target ?? throw new ArgumentNullException(nameof(target));

        var sourceRect = BoundingBox();
        var targetRect = target.BoundingBox();

        if (sourceRect == null || targetRect == null)
            throw new InvalidOperationException("Cannot get bounding box for drag operation");

        var script = $@"
            (() => {{
                const source = document.querySelector({JsonConvert.SerializeObject(_selector)});
                const targetSel = {JsonConvert.SerializeObject(target._selector)};
                const targetEl = document.querySelector(targetSel);
                
                if (!source || !targetEl) return;
                
                const sourceRect = source.getBoundingClientRect();
                const targetRect = targetEl.getBoundingClientRect();
                
                const dragStart = new DragEvent('dragstart', {{
                    bubbles: true,
                    cancelable: true,
                    clientX: sourceRect.left + sourceRect.width / 2,
                    clientY: sourceRect.top + sourceRect.height / 2
                }});
                
                const dragEnd = new DragEvent('dragend', {{
                    bubbles: true,
                    cancelable: true,
                    clientX: targetRect.left + targetRect.width / 2,
                    clientY: targetRect.top + targetRect.height / 2
                }});
                
                const drop = new DragEvent('drop', {{
                    bubbles: true,
                    cancelable: true,
                    clientX: targetRect.left + targetRect.width / 2,
                    clientY: targetRect.top + targetRect.height / 2
                }});
                
                source.dispatchEvent(dragStart);
                targetEl.dispatchEvent(drop);
                source.dispatchEvent(dragEnd);
            }})()";

        _webView.Evaluate(script);
    }

    /// <summary>
    /// Dispatches a DOM event on the element.
    /// </summary>
    /// <param name="eventType">The event type (e.g., "click", "focus").</param>
    /// <param name="eventInit">Optional event initialization properties as JSON.</param>
    public void DispatchEvent(string eventType, string? eventInit = null)
    {
        _ = eventType ?? throw new ArgumentNullException(nameof(eventType));

        var initProps = eventInit ?? "{}";
        var script = BuildScript($@"
            const event = new Event({JsonConvert.SerializeObject(eventType)}, {{ bubbles: true, ...{initProps} }});
            el.dispatchEvent(event);
        ");

        _webView.Evaluate(script);
    }

    /// <summary>
    /// Taps the element (for touch interfaces).
    /// </summary>
    public void Tap()
    {
        EnsureVisible();

        var script = BuildScript(@"
            const rect = el.getBoundingClientRect();
            const x = rect.left + rect.width / 2;
            const y = rect.top + rect.height / 2;
            
            const touchStart = new TouchEvent('touchstart', {
                bubbles: true,
                cancelable: true,
                touches: [new Touch({ identifier: 1, target: el, clientX: x, clientY: y })]
            });
            const touchEnd = new TouchEvent('touchend', {
                bubbles: true,
                cancelable: true,
                touches: []
            });
            
            el.dispatchEvent(touchStart);
            el.dispatchEvent(touchEnd);
        ");

        _webView.Evaluate(script);
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Gets the inner text of the element.
    /// </summary>
    /// <returns>The inner text.</returns>
    public string InnerText()
    {
        var script = BuildScript("return el.innerText || ''");
        return _webView.EvaluateAsync<string>(script) ?? "";
    }

    /// <summary>
    /// Gets the inner HTML of the element.
    /// </summary>
    /// <returns>The inner HTML.</returns>
    public string InnerHTML()
    {
        var script = BuildScript("return el.innerHTML || ''");
        return _webView.EvaluateAsync<string>(script) ?? "";
    }

    /// <summary>
    /// Gets the outer HTML of the element.
    /// </summary>
    /// <returns>The outer HTML.</returns>
    public string OuterHTML()
    {
        var script = BuildScript("return el.outerHTML || ''");
        return _webView.EvaluateAsync<string>(script) ?? "";
    }

    /// <summary>
    /// Gets the text content of the element.
    /// </summary>
    /// <returns>The text content.</returns>
    public string? TextContent()
    {
        var script = BuildScript("return el.textContent");
        return _webView.EvaluateAsync<string>(script);
    }

    /// <summary>
    /// Gets an attribute value.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <returns>The attribute value, or null if not found.</returns>
    public string? GetAttribute(string name)
    {
        _ = name ?? throw new ArgumentNullException(nameof(name));
        var script = BuildScript($"return el.getAttribute({JsonConvert.SerializeObject(name)})");
        return _webView.EvaluateAsync<string>(script);
    }

    /// <summary>
    /// Gets the input value.
    /// </summary>
    /// <returns>The input value.</returns>
    public string InputValue()
    {
        var script = BuildScript("return el.value || ''");
        return _webView.EvaluateAsync<string>(script) ?? "";
    }

    /// <summary>
    /// Gets the tag name of the element.
    /// </summary>
    /// <returns>The tag name in lowercase.</returns>
    public string TagName()
    {
        var script = BuildScript("return el.tagName.toLowerCase()");
        return _webView.EvaluateAsync<string>(script) ?? "";
    }

    /// <summary>
    /// Gets all class names of the element.
    /// </summary>
    /// <returns>A list of class names.</returns>
    public IReadOnlyList<string> ClassList()
    {
        var script = BuildScript("return [...el.classList]");
        return _webView.EvaluateAsync<List<string>>(script) ?? new List<string>();
    }

    /// <summary>
    /// Gets a property value from the element.
    /// </summary>
    /// <typeparam name="T">The expected property type.</typeparam>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The property value.</returns>
    public T? GetProperty<T>(string propertyName)
    {
        _ = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        var script = BuildScript($"return el[{JsonConvert.SerializeObject(propertyName)}]");
        return _webView.EvaluateAsync<T>(script);
    }

    /// <summary>
    /// Gets the bounding box of the element.
    /// </summary>
    /// <returns>The bounding box, or null if the element is not visible.</returns>
    public BoundingBox? BoundingBox()
    {
        var script = BuildScript(@"
            const rect = el.getBoundingClientRect();
            return {
                x: rect.x,
                y: rect.y,
                width: rect.width,
                height: rect.height
            };
        ");

        return _webView.EvaluateAsync<BoundingBox>(script);
    }

    /// <summary>
    /// Gets the computed style of a CSS property.
    /// </summary>
    /// <param name="propertyName">The CSS property name.</param>
    /// <returns>The computed value.</returns>
    public string GetComputedStyle(string propertyName)
    {
        _ = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        var script = BuildScript($"return window.getComputedStyle(el).getPropertyValue({JsonConvert.SerializeObject(propertyName)})");
        return _webView.EvaluateAsync<string>(script) ?? "";
    }

    #endregion

    #region State Checks

    /// <summary>
    /// Checks if the element is visible.
    /// </summary>
    /// <returns>True if visible.</returns>
    public bool IsVisible()
    {
        var script = BuildScript(@"
            if (!el) return false;
            const style = window.getComputedStyle(el);
            if (style.display === 'none') return false;
            if (style.visibility === 'hidden') return false;
            if (style.opacity === '0') return false;
            const rect = el.getBoundingClientRect();
            return rect.width > 0 && rect.height > 0;
        ");

        return _webView.EvaluateAsync<bool>(script);
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
        var script = BuildScript("return !el.disabled");
        return _webView.EvaluateAsync<bool>(script);
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
        var script = BuildScript("return el.checked === true");
        return _webView.EvaluateAsync<bool>(script);
    }

    /// <summary>
    /// Checks if the element is editable.
    /// </summary>
    /// <returns>True if editable.</returns>
    public bool IsEditable()
    {
        var script = BuildScript(@"
            if (el.isContentEditable) return true;
            if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA') {
                return !el.disabled && !el.readOnly;
            }
            return false;
        ");

        return _webView.EvaluateAsync<bool>(script);
    }

    /// <summary>
    /// Checks if the element has focus.
    /// </summary>
    /// <returns>True if focused.</returns>
    public bool IsFocused()
    {
        var script = BuildScript("return document.activeElement === el");
        return _webView.EvaluateAsync<bool>(script);
    }

    #endregion

    #region Child Queries

    /// <summary>
    /// Queries for a child element.
    /// </summary>
    /// <param name="selector">The CSS selector relative to this element.</param>
    /// <returns>The element handle, or null if not found.</returns>
    public WebViewElementHandle? QuerySelector(string selector)
    {
        _ = selector ?? throw new ArgumentNullException(nameof(selector));

        var script = BuildScript($"return el.querySelector({JsonConvert.SerializeObject(selector)}) !== null");
        var exists = _webView.EvaluateAsync<bool>(script);

        if (!exists)
            return null;

        var combinedSelector = $"{_selector} {selector}";
        return _webView.QuerySelector(combinedSelector);
    }

    /// <summary>
    /// Queries for all matching child elements.
    /// </summary>
    /// <param name="selector">The CSS selector relative to this element.</param>
    /// <returns>A list of element handles.</returns>
    public IReadOnlyList<WebViewElementHandle> QuerySelectorAll(string selector)
    {
        _ = selector ?? throw new ArgumentNullException(nameof(selector));

        var combinedSelector = $"{_selector} {selector}";
        return _webView.QuerySelectorAll(combinedSelector);
    }

    #endregion

    #region Evaluation

    /// <summary>
    /// Evaluates a JavaScript expression with this element as 'el'.
    /// </summary>
    /// <typeparam name="TResult">The expected result type.</typeparam>
    /// <param name="expression">The JavaScript expression.</param>
    /// <returns>The result of the evaluation.</returns>
    public TResult Evaluate<TResult>(string expression)
    {
        var script = BuildScript($"return {expression}");
        return _webView.EvaluateAsync<TResult>(script);
    }

    #endregion

    #region Wait Methods

    /// <summary>
    /// Waits for the element to be visible.
    /// </summary>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    public void WaitForVisible(int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? _webView.DefaultTimeout;
        var start = Environment.TickCount;

        while (Environment.TickCount - start < timeout)
        {
            if (IsVisible())
                return;
            Task.Delay(100).GetAwaiter().GetResult();
        }

        throw new TimeoutException($"Element '{_selector}' did not become visible within {timeout}ms");
    }

    /// <summary>
    /// Waits for the element to be hidden.
    /// </summary>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    public void WaitForHidden(int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? _webView.DefaultTimeout;
        var start = Environment.TickCount;

        while (Environment.TickCount - start < timeout)
        {
            if (IsHidden())
                return;
            Task.Delay(100).GetAwaiter().GetResult();
        }

        throw new TimeoutException($"Element '{_selector}' did not become hidden within {timeout}ms");
    }

    /// <summary>
    /// Waits for the element to be enabled.
    /// </summary>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    public void WaitForEnabled(int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? _webView.DefaultTimeout;
        var start = Environment.TickCount;

        while (Environment.TickCount - start < timeout)
        {
            if (IsEnabled())
                return;
            Task.Delay(100).GetAwaiter().GetResult();
        }

        throw new TimeoutException($"Element '{_selector}' did not become enabled within {timeout}ms");
    }

    #endregion

    #region Private Helpers

    private string BuildScript(string body)
    {
        if (_elementId != null)
        {
            return $@"
                (() => {{
                    const el = [...document.querySelectorAll('*')].find(e => e.__wpfPilotId === {JsonConvert.SerializeObject(_elementId)});
                    if (!el) {{
                        const fallback = document.querySelector({JsonConvert.SerializeObject(_selector)});
                        if (fallback) {{
                            {body}
                        }}
                        return null;
                    }}
                    {body}
                }})()";
        }

        return $@"
            (() => {{
                const el = document.querySelector({JsonConvert.SerializeObject(_selector)});
                if (!el) return null;
                {body}
            }})()";
    }

    private void EnsureVisible()
    {
        if (!IsVisible())
        {
            ScrollIntoView();
            Task.Delay(50).GetAwaiter().GetResult();
        }
    }

    #endregion
}

#endif
