#if NET5_0_OR_GREATER

namespace WpfPilot;

using System;

/// <summary>
/// Options for navigation operations.
/// </summary>
public class NavigationOptions
{
    /// <summary>
    /// Maximum time to wait for navigation in milliseconds.
    /// </summary>
    public int? Timeout { get; set; }

    /// <summary>
    /// When to consider navigation successful.
    /// </summary>
    public WaitUntilState WaitUntil { get; set; } = WaitUntilState.Load;

    /// <summary>
    /// Referer header to use during navigation.
    /// </summary>
    public string? Referer { get; set; }
}

/// <summary>
/// Options for click operations.
/// </summary>
public class ClickOptions
{
    /// <summary>
    /// The mouse button to use.
    /// </summary>
    public MouseButton Button { get; set; } = MouseButton.Left;

    /// <summary>
    /// Number of times to click.
    /// </summary>
    public int ClickCount { get; set; } = 1;

    /// <summary>
    /// Delay between mousedown and mouseup in milliseconds.
    /// </summary>
    public int Delay { get; set; } = 0;

    /// <summary>
    /// Whether to force the click even if the element is not actionable.
    /// </summary>
    public bool Force { get; set; } = false;

    /// <summary>
    /// Modifier keys to hold during the click.
    /// </summary>
    public KeyModifier Modifiers { get; set; } = KeyModifier.None;

    /// <summary>
    /// Position within the element to click.
    /// </summary>
    public Position? Position { get; set; }

    /// <summary>
    /// Maximum time to wait for the element to be actionable in milliseconds.
    /// </summary>
    public int? Timeout { get; set; }

    /// <summary>
    /// When set, bypasses actionability checks.
    /// </summary>
    public bool? Trial { get; set; }
}

/// <summary>
/// Options for type operations.
/// </summary>
public class TypeOptions
{
    /// <summary>
    /// Delay between keystrokes in milliseconds.
    /// </summary>
    public int Delay { get; set; } = 0;

    /// <summary>
    /// Maximum time to wait in milliseconds.
    /// </summary>
    public int? Timeout { get; set; }

    /// <summary>
    /// When set to true, does not clear existing content before typing.
    /// </summary>
    public bool NoWaitAfter { get; set; } = false;
}

/// <summary>
/// Options for waiting for selectors.
/// </summary>
public class WaitForSelectorOptions
{
    /// <summary>
    /// The element state to wait for.
    /// </summary>
    public ElementState State { get; set; } = ElementState.Visible;

    /// <summary>
    /// Maximum time to wait in milliseconds.
    /// </summary>
    public int? Timeout { get; set; }

    /// <summary>
    /// When set, wait will be less strict about visibility.
    /// </summary>
    public bool? Strict { get; set; }
}

/// <summary>
/// Filter options for locators.
/// </summary>
public class LocatorFilter
{
    /// <summary>
    /// Matches elements containing this text.
    /// </summary>
    public string? HasText { get; set; }

    /// <summary>
    /// Matches elements not containing this text.
    /// </summary>
    public string? HasNotText { get; set; }

    /// <summary>
    /// Matches elements that contain an element matching this locator.
    /// </summary>
    public WebViewLocator? Has { get; set; }

    /// <summary>
    /// Matches elements that do not contain an element matching this locator.
    /// </summary>
    public WebViewLocator? HasNot { get; set; }
}

/// <summary>
/// Options for adding script tags.
/// </summary>
public class ScriptTagOptions
{
    /// <summary>
    /// URL of the script to add.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Raw JavaScript content to add.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Script type (e.g., "module", "text/javascript").
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Unique identifier for the script tag.
    /// </summary>
    public string? Id { get; set; }
}

/// <summary>
/// Options for adding style tags.
/// </summary>
public class StyleTagOptions
{
    /// <summary>
    /// URL of the stylesheet to add.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Raw CSS content to add.
    /// </summary>
    public string? Content { get; set; }
}

/// <summary>
/// Options for taking screenshots.
/// </summary>
public class WebViewScreenshotOptions
{
    /// <summary>
    /// The image format for the screenshot.
    /// </summary>
    public ImageFormat Format { get; set; } = ImageFormat.Png;

    /// <summary>
    /// Whether to capture the full scrollable page.
    /// </summary>
    public bool FullPage { get; set; } = false;

    /// <summary>
    /// The quality of the image (0-100, JPEG only).
    /// </summary>
    public int? Quality { get; set; }

    /// <summary>
    /// A specific clip region to capture.
    /// </summary>
    public ClipRegion? Clip { get; set; }

    /// <summary>
    /// Whether to hide default white background and capture transparency.
    /// </summary>
    public bool OmitBackground { get; set; } = false;
}

/// <summary>
/// Represents a clip region for screenshots.
/// </summary>
public class ClipRegion
{
    /// <summary>
    /// X coordinate of the clip region.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Y coordinate of the clip region.
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Width of the clip region.
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Height of the clip region.
    /// </summary>
    public double Height { get; set; }
}

/// <summary>
/// Represents a position (point).
/// </summary>
public class Position
{
    /// <summary>
    /// X coordinate.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Y coordinate.
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Creates a new position.
    /// </summary>
    public Position() { }

    /// <summary>
    /// Creates a new position with the specified coordinates.
    /// </summary>
    public Position(double x, double y)
    {
        X = x;
        Y = y;
    }
}

/// <summary>
/// Represents the bounding box of an element.
/// </summary>
public class BoundingBox
{
    /// <summary>
    /// X coordinate of the bounding box.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Y coordinate of the bounding box.
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Width of the bounding box.
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Height of the bounding box.
    /// </summary>
    public double Height { get; set; }
}

/// <summary>
/// Response from a navigation operation.
/// </summary>
public class NavigationResponse
{
    /// <summary>
    /// The URL that was navigated to.
    /// </summary>
    public string Url { get; set; } = "";

    /// <summary>
    /// Whether the navigation was successful.
    /// </summary>
    public bool Ok { get; set; }

    /// <summary>
    /// The HTTP status code.
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// The HTTP status text.
    /// </summary>
    public string? StatusText { get; set; }

    /// <summary>
    /// Response headers (simplified representation).
    /// </summary>
    public System.Collections.Generic.Dictionary<string, string>? Headers { get; set; }
}

#endif
