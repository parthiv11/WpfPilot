#if NET5_0_OR_GREATER

namespace WpfPilot;

using System;

/// <summary>
/// Exception thrown when an assertion fails in WebView2 operations.
/// </summary>
public class WebViewAssertionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebViewAssertionException"/> class.
    /// </summary>
    public WebViewAssertionException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebViewAssertionException"/> class with a message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public WebViewAssertionException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebViewAssertionException"/> class with a message and inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public WebViewAssertionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when an element cannot be found in the WebView2.
/// </summary>
public class WebViewElementNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebViewElementNotFoundException"/> class.
    /// </summary>
    public WebViewElementNotFoundException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebViewElementNotFoundException"/> class with a message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public WebViewElementNotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebViewElementNotFoundException"/> class with a message and inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public WebViewElementNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a WebView2 navigation fails.
/// </summary>
public class WebViewNavigationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebViewNavigationException"/> class.
    /// </summary>
    public WebViewNavigationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebViewNavigationException"/> class with a message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public WebViewNavigationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebViewNavigationException"/> class with a message and inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public WebViewNavigationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// The URL that failed to load.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// The HTTP status code, if available.
    /// </summary>
    public int? StatusCode { get; set; }
}

/// <summary>
/// Exception thrown when a JavaScript evaluation fails in the WebView2.
/// </summary>
public class WebViewEvaluationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebViewEvaluationException"/> class.
    /// </summary>
    public WebViewEvaluationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebViewEvaluationException"/> class with a message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public WebViewEvaluationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebViewEvaluationException"/> class with a message and inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public WebViewEvaluationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// The JavaScript expression that failed.
    /// </summary>
    public string? Expression { get; set; }

    /// <summary>
    /// The JavaScript error message.
    /// </summary>
    public string? JsErrorMessage { get; set; }

    /// <summary>
    /// The JavaScript stack trace, if available.
    /// </summary>
    public string? JsStackTrace { get; set; }
}

/// <summary>
/// Exception thrown when an element is not actionable (not visible, disabled, etc.).
/// </summary>
public class WebViewElementNotActionableException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebViewElementNotActionableException"/> class.
    /// </summary>
    public WebViewElementNotActionableException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebViewElementNotActionableException"/> class with a message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public WebViewElementNotActionableException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebViewElementNotActionableException"/> class with a message and inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public WebViewElementNotActionableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// The selector that matched the non-actionable element.
    /// </summary>
    public string? Selector { get; set; }

    /// <summary>
    /// The reason the element is not actionable.
    /// </summary>
    public string? Reason { get; set; }
}

#endif
