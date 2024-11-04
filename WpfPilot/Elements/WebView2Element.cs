namespace WpfPilot.Elements;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows;
using WpfPilot;
using WpfPilot.AppDriverPayload.Commands;
using WpfPilot.Interop;
using WpfPilot.Utility.WpfUtility;

public class WebView2Element : Element
{
    public WebView2Element(Element other) : base(other)
    {
    }

    public WebView2Element(
        string targetId,
        string typeName,
        string? parentId,
        IReadOnlyList<string> childIds,
        IReadOnlyDictionary<string, object> properties,
        IReadOnlyDictionary<string, List<Element>> targetIdToElement,
        NamedPipeClient channel,
        Action onAction,
        Action<string> onAccessProperty)
        : base(targetId, typeName, parentId, childIds, properties, targetIdToElement, channel, onAction, onAccessProperty)
    {
    }

    public override Element Click()
    {
        return DoClick("Left");
    }

    public override Element RightClick()
    {
        return DoClick("Right");
    }

    public override Element DoubleClick()
    {
        return RaiseEvent<UIElement>(_ => new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, Environment.TickCount, MouseButton.Left)
        {
            RoutedEvent = Control.MouseDoubleClickEvent,
        });
    }

    public override Element Focus()
    {
        return Invoke(element => element.Focus());
    }

    public override Element Select()
    {
        return SetProperty("IsSelected", true);
    }

    public override Element Expand()
    {
        return SetProperty("IsExpanded", true);
    }

    public override Element Collapse()
    {
        return SetProperty("IsExpanded", false);
    }

    public override Element SelectText(string text)
    {
        var currentText = (string)Properties["Text"];
        var startIndex = currentText.IndexOf(text, StringComparison.Ordinal);

        if (startIndex == -1)
        {
            SetProperty("SelectedText", text);
            return this;
        }

        SetProperty("SelectionStart", startIndex);
        SetProperty("SelectionLength", text.Length);
        return this;
    }

    public override Element Check()
    {
        return SetProperty("IsChecked", true);
    }

    public override Element Uncheck()
    {
        return SetProperty("IsChecked", false);
    }

    public override Element ScrollIntoView()
    {
        return Invoke<FrameworkElement>(element => element.BringIntoView());
    }

    public override Element Type(string text)
    {
        Invoke(element => element.Focus());

        Expression<Action<Application>> code = app => KeyboardInput.Type(text);
        var response = Channel.GetResponse(new
        {
            Kind = nameof(InvokeStaticCommand),
            Code = Eval.SerializeCode(code)
        });

        OnAction();
        return this;
    }

    public override Element Screenshot(string fileOutputPath)
    {
        _ = fileOutputPath ?? throw new ArgumentNullException(nameof(fileOutputPath));
        fileOutputPath = Environment.ExpandEnvironmentVariables(fileOutputPath);
        fileOutputPath = Path.GetFullPath(fileOutputPath);

        var start = Environment.TickCount;
        dynamic? previousResponse = null;
        while (Environment.TickCount - start < 5000)
        {
            var response = Channel.GetResponse(new
            {
                Kind = nameof(ScreenshotCommand),
                Format = Path.GetExtension(fileOutputPath).Replace(".", ""),
                TargetId,
            });

            if (previousResponse != null && previousResponse!.Base64Screenshot == response!.Base64Screenshot)
            {
                SaveImage(Convert.FromBase64String(response!.Base64Screenshot));
                return this;
            }

            previousResponse = response;
            Task.Delay(500).GetAwaiter().GetResult();
        }

        SaveImage(Convert.FromBase64String(previousResponse!.Base64Screenshot));

        void SaveImage(byte[] bytes)
        {
            var folders = Path.GetDirectoryName(fileOutputPath);
            Directory.CreateDirectory(folders);
            File.WriteAllBytes(fileOutputPath, bytes);
        }

        return this;
    }

    public override Element Screenshot(out byte[] screenshotBytes, ImageFormat format = ImageFormat.Jpeg)
    {
        var start = Environment.TickCount;
        dynamic? previousResponse = null;
        while (Environment.TickCount - start < 5000)
        {
            var response = Channel.GetResponse(new
            {
                Kind = nameof(ScreenshotCommand),
                Format = format.ToString().ToLowerInvariant(),
                TargetId,
            });

            if (previousResponse != null && previousResponse!.Base64Screenshot == response!.Base64Screenshot)
            {
                screenshotBytes = Convert.FromBase64String(response!.Base64Screenshot);
                return this;
            }

            previousResponse = response;
            Task.Delay(500).GetAwaiter().GetResult();
        }

        screenshotBytes = Convert.FromBase64String(previousResponse!.Base64Screenshot);
        return this;
    }

    public override byte[] Screenshot(ImageFormat format = ImageFormat.Jpeg)
    {
        Screenshot(out var bytes, format);
        return bytes;
    }

    public override Element RaiseEvent<TInput>(Expression<Func<TInput, RoutedEventArgs>> code)
    {
        var response = Channel.GetResponse(new
        {
            Kind = nameof(RaiseEventCommand),
            TargetId,
            GetRoutedEventArgs = Eval.SerializeCode(code),
        });

        OnAction();
        return this;
    }

    public override Element SetProperty(string propertyName, object? value)
    {
        var response = Channel.GetResponse(new
        {
            Kind = nameof(SetPropertyCommand),
            TargetId,
            PropertyName = propertyName,
            PropertyValue = value switch
            {
                Primitive p => WrappedArg<object>.Wrap(p.V),
                _ => WrappedArg<object>.Wrap(value),
            }
        });

        OnAction();
        return this;
    }

    public override Element SetProperty<TInput, TOutput>(string propertyName, Expression<Func<TInput, TOutput>> getValue)
    {
        var response = Channel.GetResponse(new
        {
            Kind = nameof(SetPropertyCommand),
            TargetId,
            PropertyName = propertyName,
            PropertyValue = Eval.SerializeCode(getValue),
        });

        OnAction();
        return this;
    }

    private Element DoClick(string mouseButton)
    {
        var response = Channel.GetResponse(new
        {
            Kind = nameof(ClickCommand),
            TargetId,
            MouseButton = mouseButton,
        });

        OnAction();
        return this;
    }
}
