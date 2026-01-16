#if NET5_0_OR_GREATER

namespace WpfPilot;

using System;

/// <summary>
/// Represents the state to wait for during page load.
/// </summary>
public enum WaitUntilState
{
    /// <summary>
    /// Do not wait for any specific state.
    /// </summary>
    None,

    /// <summary>
    /// Wait for the DOMContentLoaded event.
    /// </summary>
    DOMContentLoaded,

    /// <summary>
    /// Wait for the load event.
    /// </summary>
    Load,

    /// <summary>
    /// Wait for network to be idle (no more than 0 network connections for 500ms).
    /// </summary>
    NetworkIdle,

    /// <summary>
    /// Wait for the document to commit (navigation response received).
    /// </summary>
    Commit
}

/// <summary>
/// Represents the state of an element.
/// </summary>
public enum ElementState
{
    /// <summary>
    /// Element is attached to the DOM.
    /// </summary>
    Attached,

    /// <summary>
    /// Element is detached from the DOM.
    /// </summary>
    Detached,

    /// <summary>
    /// Element is visible.
    /// </summary>
    Visible,

    /// <summary>
    /// Element is hidden (not visible or not in DOM).
    /// </summary>
    Hidden
}

/// <summary>
/// Mouse button types.
/// </summary>
public enum MouseButton
{
    /// <summary>
    /// Left mouse button.
    /// </summary>
    Left,

    /// <summary>
    /// Right mouse button.
    /// </summary>
    Right,

    /// <summary>
    /// Middle mouse button.
    /// </summary>
    Middle
}

/// <summary>
/// Keyboard modifier keys.
/// </summary>
[Flags]
public enum KeyModifier
{
    /// <summary>
    /// No modifier.
    /// </summary>
    None = 0,

    /// <summary>
    /// Alt key.
    /// </summary>
    Alt = 1,

    /// <summary>
    /// Control key.
    /// </summary>
    Control = 2,

    /// <summary>
    /// Meta/Command key.
    /// </summary>
    Meta = 4,

    /// <summary>
    /// Shift key.
    /// </summary>
    Shift = 8
}

/// <summary>
/// Types of console messages.
/// </summary>
public enum ConsoleMessageType
{
    /// <summary>
    /// Log message.
    /// </summary>
    Log,

    /// <summary>
    /// Debug message.
    /// </summary>
    Debug,

    /// <summary>
    /// Info message.
    /// </summary>
    Info,

    /// <summary>
    /// Warning message.
    /// </summary>
    Warning,

    /// <summary>
    /// Error message.
    /// </summary>
    Error,

    /// <summary>
    /// Directory message.
    /// </summary>
    Dir,

    /// <summary>
    /// Directory XML message.
    /// </summary>
    DirXml,

    /// <summary>
    /// Table message.
    /// </summary>
    Table,

    /// <summary>
    /// Trace message.
    /// </summary>
    Trace,

    /// <summary>
    /// Clear message.
    /// </summary>
    Clear,

    /// <summary>
    /// Start group message.
    /// </summary>
    StartGroup,

    /// <summary>
    /// Start group collapsed message.
    /// </summary>
    StartGroupCollapsed,

    /// <summary>
    /// End group message.
    /// </summary>
    EndGroup,

    /// <summary>
    /// Assert message.
    /// </summary>
    Assert,

    /// <summary>
    /// Profile message.
    /// </summary>
    Profile,

    /// <summary>
    /// Profile end message.
    /// </summary>
    ProfileEnd,

    /// <summary>
    /// Count message.
    /// </summary>
    Count,

    /// <summary>
    /// Time end message.
    /// </summary>
    TimeEnd
}

#endif
