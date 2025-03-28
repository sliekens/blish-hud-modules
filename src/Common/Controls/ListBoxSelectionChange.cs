﻿namespace SL.Common.Controls;

internal sealed class ListBoxSelectionChange<T>
{
    public List<ListItem<T>> Added { get; } = [];

    public List<ListItem<T>> Removed { get; } = [];
}
