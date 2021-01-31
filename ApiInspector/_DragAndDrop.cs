using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ApiInspector
{
    static partial class _
    {
        #region Public Methods
        public static void AttachDragAndDropFunctionality(StackPanel panel, Action<int, int> onOperationCompleted)
        {
            var       isDown          = false;
            var       isDragging      = false;
            var       startPoint      = new Point(0, 0);
            UIElement realDragSource  = null;
            var       dummyDragSource = new UIElement();

            void previewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                if (Equals(e.Source, panel))
                {
                }
                else
                {
                    isDown     = true;
                    startPoint = e.GetPosition(panel);
                }
            }

            void previewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            {
                isDown     = false;
                isDragging = false;
                realDragSource?.ReleaseMouseCapture();
            }

            void previewMouseMove(object sender, MouseEventArgs e)
            {
                if (isDown)
                {
                    if (isDragging == false && (Math.Abs(e.GetPosition(panel).X - startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                                                Math.Abs(e.GetPosition(panel).Y - startPoint.Y) > SystemParameters.MinimumVerticalDragDistance))
                    {
                        isDragging     = true;
                        realDragSource = (UIElement) e.Source;
                        realDragSource.CaptureMouse();
                        DragDrop.DoDragDrop(dummyDragSource, new DataObject("UIElement", e.Source, true), DragDropEffects.Move);
                    }
                }
            }

            void dragEnter(object sender, DragEventArgs e)
            {
                if (e.Data.GetDataPresent("UIElement"))
                {
                    e.Effects = DragDropEffects.Move;
                }
            }

            void drop(object sender, DragEventArgs e)
            {
                if (e.Data.GetDataPresent("UIElement"))
                {
                    var dropTarget      = e.Source as UIElement;
                    int dropTargetIndex = -1, i = 0;
                    foreach (UIElement element in panel.Children)
                    {
                        if (element.Equals(dropTarget))
                        {
                            dropTargetIndex = i;
                            break;
                        }

                        i++;
                    }

                    if (dropTargetIndex != -1)
                    {
                        var sourceIndex = panel.Children.IndexOf(realDragSource);

                        panel.Children.Remove(realDragSource);
                        panel.Children.Insert(dropTargetIndex, realDragSource);

                        onOperationCompleted?.Invoke(sourceIndex, dropTargetIndex);
                    }

                    isDown     = false;
                    isDragging = false;
                    realDragSource.ReleaseMouseCapture();
                }
            }

            panel.PreviewMouseLeftButtonDown += previewMouseLeftButtonDown;
            panel.PreviewMouseLeftButtonUp   += previewMouseLeftButtonUp;
            panel.PreviewMouseMove           += previewMouseMove;
            panel.DragEnter                  += dragEnter;
            panel.Drop                       += drop;
            panel.AllowDrop                  =  true;
        }
        #endregion
    }
}