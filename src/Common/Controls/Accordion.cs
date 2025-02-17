using System.ComponentModel;

using Blish_HUD.Controls;

namespace SL.Common.Controls;

public sealed class Accordion : FlowPanel
{
    private event EventHandler? Expanding;

    private Control? _active;

    public Accordion()
    {
        FlowDirection = ControlFlowDirection.SingleTopToBottom;
        WidthSizingMode = SizingMode.Fill;
        HeightSizingMode = SizingMode.AutoSize;
        PropertyChanged += OnPropertyChanged;
    }

    public void AddSection(string title, Control content)
    {
        Panel section = new()
        {
            Parent = this,
            WidthSizingMode = SizingMode.Fill,
            HeightSizingMode = SizingMode.AutoSize,
            Title = title,
            CanCollapse = true,
            Collapsed = true
        };

        section.PropertyChanged += OnPropertyChanged;
        content.Parent = section;

        Expanding += (_, _) =>
        {
            if (section != _active)
            {
                section.Collapse();
            }
        };

        section.Resized += (_, _) =>
        {
            if (section.Height >= 300)
            {
                section.HeightSizingMode = SizingMode.Standard;
                section.Height = 300;
                section.CanScroll = true;
            }
            else
            {
                section.HeightSizingMode = SizingMode.AutoSize;
                section.CanScroll = false;
            }
        };
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Expand" && sender is Panel { Collapsed: false } panel)
        {
            _active = panel;
            Expanding?.Invoke(this, EventArgs.Empty);
        }
    }
}
