namespace EntityCore.Tools.Views.Components
{
    /// <summary>
    /// Guid typeli inputlar uchun
    /// </summary>
    public class InputGuid
    {
        public string Generate()
        {
            return @"/// https://stackoverflow.com/questions/73567617/how-to-accept-a-guid-as-an-input-type-in-blazor-server

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

public class InputGuid : InputBase<Guid>{
    public ElementReference? Element { get; protected set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);
        builder.OpenElement(0, ""input"");
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttribute(2, ""type"", ""text"");
        builder.AddAttribute(3, ""class"", CssClass);
        builder.AddAttribute(4, ""value"", BindConverter.FormatValue(CurrentValueAsString));
        builder.AddAttribute(5, ""oninput"", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
        builder.AddElementReferenceCapture(6, (__value) => { Element = __value; });
        builder.CloseElement();
    }

    protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out Guid result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = new Guid();
            validationErrorMessage = null;
            return true;
        }

        if (Guid.TryParse(value, out Guid guidValue))
        {
            result = guidValue;
            validationErrorMessage = null;
            return true;
        }
        else
        {
            result = new Guid();
            validationErrorMessage = ""The value could not be parsed to a Guid."";
            return false;
        }
    }

    protected override string FormatValueAsString(Guid value)
    {
        // Converts the Guid? to string, handling null appropriately
        return value.ToString() ?? string.Empty;
    }
}
";
        }
    }
}
