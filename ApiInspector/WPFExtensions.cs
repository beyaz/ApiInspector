using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using static FunctionalPrograming.FPExtensions;

namespace ApiInspector
{
    /// <summary>
    ///     The WPF extensions
    /// </summary>
    static class WPFExtensions
    {
        #region Public Methods

        public static SolidColorBrush DescriptionForeground = Brushes.RosyBrown;


        public static T SearchInMergedDictionaries<T>(string key)
        {
            foreach (var dictionary in System.Windows.Application.Current.Resources.MergedDictionaries)
            {
                foreach (var item in dictionary.Keys)
                {
                    if (item.ToString() == key)
                    {
                        return (T)dictionary[item];
                    }
                }
            }

            return default(T);
        }



        /// <summary>
        ///     Binds the specified target.
        /// </summary>
        public static void Bind(DependencyObject target, DependencyProperty dependencyProperty, object source, string propertyPath)
        {
            BindingOperations.SetBinding(target, dependencyProperty, new Binding
            {
                Source              = source,
                Path                = new PropertyPath(propertyPath),
                Mode                = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
        }

        /// <summary>
        ///     News the stack panel.
        /// </summary>
        public static StackPanel NewStackPanel(params FrameworkElement[] childElements)
        {
            var sp = new StackPanel();

            foreach (var element in childElements)
            {
                sp.Children.Add(element);
            }

            return sp;
        }

        public static StackPanel NewHorizontalStackPanel(params FrameworkElement[] childElements)
        {
            var sp = new StackPanel{Orientation = Orientation.Horizontal};

            foreach (var element in childElements)
            {
                sp.Children.Add(element);
            }

            return sp;
        }

        public static StackPanel NewStackPanel(int indent, params FrameworkElement[] childElements)
        {
            var sp = new StackPanel();

            var i = 0;
            foreach (var element in childElements)
            {
                if (i>0)
                {
                    element.Margin = new Thickness(0, indent, 0, 0);
                }
                sp.Children.Add(element);
                i++;
            }

            return sp;
        }

        public static void HorizontalIndent(StackPanel stackPanel, int indent)
        {

            var i = 0;
            foreach (FrameworkElement element in stackPanel.Children)
            {
                if (i>0)
                {
                    element.Margin = new Thickness(indent, element.Margin.Top, element.Margin.Right, element.Margin.Bottom);
                }
                i++;
            }

        }

        public static void VerticalIndent(StackPanel stackPanel, int indent)
        {

            var i = 0;
            foreach (FrameworkElement element in stackPanel.Children)
            {
                if (i>0)
                {
                    element.Margin = new Thickness(element.Margin.Left, indent, element.Margin.Right, element.Margin.Bottom);
                }
                i++;
            }

        }

        public static TextBlock NewTextBlock(string text,FontWeight fontWeight)
        {
            return new TextBlock {FontWeight = fontWeight, Text = text};
        }
        public static TextBlock NewBoldTextBlock(string text)
        {
            return new TextBlock {FontWeight = FontWeights.Bold, Text = text};
        }

        public static GroupBox NewGroupBox(UIElement header, UIElement content)
        {
            return new GroupBox
            {
                Header = header,
                Content = content
            };
        }

        public static T UpdatePadding<T>(this T element,int padding)where  T:Control
        {
            element.Padding = new Thickness(padding);

            return element;
        }

        public static T WithMargin<T>(this T element,Thickness thickness)where  T:FrameworkElement
        {
            element.Margin = thickness;

            return element;
        }

        public static T WithMarginLeft<T>(this T element,int left)where  T:FrameworkElement
        {
            element.Margin = new Thickness(left,element.Margin.Top,element.Margin.Right,element.Margin.Bottom);

            return element;
        }

        public static T WithVerticalAlignmentCenter<T>(this T element)where  T:FrameworkElement
        {
            element.VerticalAlignment = VerticalAlignment.Center;

            return element;
        }

        public static Expander NewExpander(string header, int contentMargin, FrameworkElement content)
        {
            content.Margin = new Thickness(contentMargin);

            var expander = new Expander
            {
                Header     = NewTextBlock(header,FontWeights.Bold),
                Content    = content,
                IsExpanded = true
            };

            return expander;
        }


        public static Grid NewColumnSplittedGrid(FrameworkElement left,FrameworkElement right,Action<GridSplitter> afterGridSplitterInitialized =null)
        {
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)},
                    new ColumnDefinition {Width = new GridLength(0, GridUnitType.Auto)},
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)}
                }
            };

            var gridSplitter = new GridSplitter
            {
                Width          = 7, 
                Height         = 90,
                ResizeBehavior = GridResizeBehavior.PreviousAndNext
            };

            gridSplitter.SetValue(FrameworkElement.VerticalAlignmentProperty,VerticalAlignment.Center);
           
            afterGridSplitterInitialized?.Invoke(gridSplitter);
          
            left.SetValue(Grid.ColumnProperty,0);

            gridSplitter.SetValue(Grid.ColumnProperty,1);
            right.SetValue(Grid.ColumnProperty,2);
            
            

            grid.Children.Add(left);
            grid.Children.Add(gridSplitter);
            grid.Children.Add(right);

            return grid;
        }

        public static Grid NewGridWithColumns(int[] columnSizes,params FrameworkElement[] childElements)
        {
            var grid = new Grid();

            foreach (var size in columnSizes)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(size,GridUnitType.Star)});
            }

            int i = 0;
            foreach (var element in childElements)
            {
                element.SetValue(Grid.ColumnProperty, i++);
                grid.Children.Add(element);
            }
            
            return grid;
        }

        
        public static Grid NewGridWithRows(string[] rowDefinitions,params FrameworkElement[] childElements)
        {
            var grid = new Grid();

            foreach (var size in rowDefinitions)
            {
                var getLength = fun(() =>
                {
                    if (size == "Auto")
                    {
                        return new GridLength(0, GridUnitType.Auto);

                    }

                    if (size == "*")
                    {
                        return new GridLength(1, GridUnitType.Star);

                    }

                    return new GridLength(int.Parse(size), GridUnitType.Star);
                });

                grid.RowDefinitions.Add(new RowDefinition{Height = getLength()});
            }

            int i = 0;
            foreach (var element in childElements)
            {
                element.SetValue(Grid.RowProperty, i++);
                grid.Children.Add(element);
            }
            
            return grid;
        }

        public static Grid NewGridWithColumns(string[] columnSizes,params FrameworkElement[] childElements)
        {
            var grid = new Grid();

            foreach (var size in columnSizes)
            {
                var getLength = fun(() =>
                {
                    if (size == "Auto")
                    {
                        return new GridLength(0, GridUnitType.Auto);

                    }

                    if (size == "*")
                    {
                        return new GridLength(1, GridUnitType.Star);

                    }

                    return new GridLength(int.Parse(size), GridUnitType.Star);
                });


                grid.ColumnDefinitions.Add(new ColumnDefinition{Width = getLength()});
            }

            int i = 0;
            foreach (var element in childElements)
            {
                element.SetValue(Grid.ColumnProperty, i++);
                grid.Children.Add(element);
            }
            
            return grid;
        }






        public static StackPanel WithIndent(this StackPanel stackPanel, int indent)
        {

            var i = 0;
            if (stackPanel.Orientation == Orientation.Vertical)
            {
                foreach (FrameworkElement element in stackPanel.Children)
                {
                    if (i > 0)
                    {
                        element.Margin = new Thickness(element.Margin.Left, indent, element.Margin.Right, element.Margin.Bottom);
                    }

                    i++;
                }
            }
            else
            {
                foreach (FrameworkElement element in stackPanel.Children)
                {
                    if (i > 0)
                    {
                        element.Margin = new Thickness(indent,element.Margin.Top, element.Margin.Right, element.Margin.Bottom);
                    }

                    i++;
                }
            }

            return stackPanel;

        }



        #endregion
    }
}