using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using WDLT.Clients.POE.Models;
using WDLT.PoESpy.Models;

namespace WDLT.PoESpy.Helpers.Controls
{
    public class PropertyTextblock : UserControl
    {
        static PropertyTextblock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyTextblock), new FrameworkPropertyMetadata(typeof(PropertyTextblock)));
        }

        public static readonly DependencyProperty ValueProperty
            = DependencyProperty.Register(
                "Value",
                typeof(POEFetchItemProperty),
                typeof(PropertyTextblock),
                new PropertyMetadata(OnValueChanged));

        public static readonly DependencyProperty PropertiesProperty
            = DependencyProperty.Register(
                "Properties",
                typeof(List<AppItemProperty>),
                typeof(PropertyTextblock));

        [Bindable(true)]
        public POEFetchItemProperty Value
        {
            get => (POEFetchItemProperty)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        [Bindable(true)]
        public List<AppItemProperty> Properties
        {
            get => (List<AppItemProperty>)GetValue(PropertiesProperty);
            set => SetValue(PropertiesProperty, value);
        }

        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (obj is PropertyTextblock tb)
            {
                var list = new List<AppItemProperty>();

                if (tb.Value.Name.Contains("{0}"))
                {
                    foreach (var word in tb.Value.Name.Split(" ", StringSplitOptions.TrimEntries))
                    {
                        var match = Regex.Match(word, "(?<={)([0-9]{1,2})(?=})");

                        if (match.Success)
                        {
                            var val = tb.Value.Values[int.Parse(match.Value)];
                            list.Add(new AppItemProperty(val[0], true, int.Parse(val[1])));
                        }
                        else
                        {
                            list.Add(new AppItemProperty(word, false));
                        }
                    }
                }
                else
                {
                    if(!string.IsNullOrWhiteSpace(tb.Value.Name)) 
                        list.Add(new AppItemProperty(tb.Value.Name, false));

                    foreach (var value in tb.Value.Values)
                    {
                        list.Add(new AppItemProperty(value[0], true, int.Parse(value[1])));
                    }
                }

                tb.Properties = list;
            }
        }

        //private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        //{
        //    if (obj is PropertyTextblock tb)
        //    {
        //        var prop = tb.Value;

        //        tb.Inlines.Clear();

        //        if (prop.Name.Contains("{0}"))
        //        {

        //        }
        //        else
        //        {
        //            tb.Inlines.Add(new TextBlock
        //            {
        //                Text = prop.Name,
        //                Foreground = (SolidColorBrush)Application.Current.Resources["PoeCaption"],
        //                Margin = new Thickness(0, 0, 5, 0),
        //                FontSize = tb.FontSize
        //            });

        //            foreach (var value in prop.Values)
        //            {
        //                tb.Inlines.Add(new TextBlock
        //                {
        //                    Text = value[0],
        //                    Foreground = GetForegroundForValue(int.Parse(value[1])),
        //                    FontWeight = FontWeights.Medium,
        //                    Margin = new Thickness(0, 0, 3, 0),
        //                    FontSize = tb.FontSize
        //                });
        //            }
        //        }
        //    }
        //}

        //private static Brush GetForegroundForValue(int type)
        //{
        //    switch (type)
        //    {
        //        case 4:
        //            return (SolidColorBrush)Application.Current.Resources["POECorrupted"];
        //        case 5:
        //            return (SolidColorBrush)Application.Current.Resources["POEMagic"];
        //        case 6:
        //            return (SolidColorBrush)Application.Current.Resources["POERare"];
        //        default:
        //            return Brushes.White;
        //    }
        //}
    }
}