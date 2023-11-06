using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace UPNew.Views;

public partial class CourseGrid : UserControl
{
    public CourseGrid()
    {
        InitializeComponent();
    }

    private void CourseBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("123123");
    }
}