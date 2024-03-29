﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Wpf.UI.Advance
{
    [TemplatePart(Name = "PART_TITLEBAR", Type = typeof(UIElement))]
    [TemplatePart(Name = "PART_CONTENT", Type = typeof(UIElement))]
    [TemplatePart(Name = "PART_MINIMIZE", Type = typeof(Button))]
    [TemplatePart(Name = "PART_MAXIMIZE_RESTORE", Type = typeof(Button))]
    [TemplatePart(Name = "PART_CLOSE", Type = typeof(Button))]
    [TemplatePart(Name = "PART_LEFT_BORDER", Type = typeof(UIElement))]
    [TemplatePart(Name = "PART_RIGHT_BORDER", Type = typeof(UIElement))]
    [TemplatePart(Name = "PART_TOP_BORDER", Type = typeof(UIElement))]
    [TemplatePart(Name = "PART_BOTTOM_BORDER", Type = typeof(UIElement))]
    /// <summary>
    /// Custom Window
    /// </summary>
    public partial class AdvancedWindow : Window
    {
        public delegate void CloseButtonEventHandler();

        #region Dependency Properties for appearance.
        public int TitleBarHeight
        {
            get { return (int)GetValue(TitleBarHeightProperty); }
            set { SetValue(TitleBarHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleBarHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleBarHeightProperty =
            DependencyProperty.Register("TitleBarHeight", typeof(int), typeof(AdvancedWindow), new PropertyMetadata(30));


        public int TitleBarFontSize
        {
            get { return (int)GetValue(TitleBarFontSizeProperty); }
            set { SetValue(TitleBarFontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleBarFontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleBarFontSizeProperty =
            DependencyProperty.Register("TitleBarFontSize", typeof(int), typeof(AdvancedWindow), new PropertyMetadata(12));


        public SolidColorBrush TitleTextBrush
        {
            get { return (SolidColorBrush)GetValue(TitleTextBrushProperty); }
            set { SetValue(TitleTextBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleTextBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleTextBrushProperty =
            DependencyProperty.Register("TitleTextBrush", typeof(SolidColorBrush), typeof(AdvancedWindow), new PropertyMetadata(TitleTextBrushChanged));

        public static void TitleTextBrushChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            AdvancedWindow window = dp as AdvancedWindow;

            if (!(window.TitleBar is Border)) return;

            // find the textblock control of the children of the titlebar and change the value of the foreground of the control.
        }
        #endregion


        /// <summary>
        /// Initializes a new instance of the <see cref="AdvancedWindow"/> class.
        /// </summary>
        public AdvancedWindow()
        {
            CreateCommandBindings();
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code 
        /// or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            AttachToVisualTree();
        }

        private void ChangeWindowState()
        {
            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else this.WindowState = WindowState.Normal;
        }

        public event EventHandler<ClosedEventArgs> CloseButtonClicked;

        /// <summary>
        /// Creates the command bindings
        /// </summary>
        private void CreateCommandBindings()
        {
            // Command binding for close button
            CommandBindings.Add(new CommandBinding(
                ApplicationCommands.Close,
                (a, b) => 
                {
                    //if(CloseButtonClicked != null)
                    //{
                    //    if (CloseButtonClicked.Invoke(this, null) == false) return;
                    //}
//                    CloseButtonClicked?.Invoke(this, );

                    Close();
                }));

            // Command binding for minimize button
            CommandBindings.Add(new CommandBinding(
                MinimizeCommand,
                (a, b) => { WindowState = WindowState.Minimized; }));

            // Command binding for maximize / restore button
            CommandBindings.Add(new CommandBinding(MaximizeRestoreCommand,
                (a, b) =>
                {
                    this.ChangeWindowState();
                }));
        }

        /// <summary>
        /// Attaches to visual tree to the template
        /// </summary>
        private void AttachToVisualTree()
        {
            AttachCloseButton();
            AttachMinimizeButton();
            AttachMaximizeRestoreButton();
            AttachTitleBar();
            AttachBorders();
        }

        /// <summary>
        /// Attaches the close button
        /// </summary>
        private void AttachCloseButton()
        {
            if (CloseButton != null)
            {
                CloseButton.Command = null;
            }

            Button closeButton = GetChildControl<Button>("PART_CLOSE");
            if (closeButton != null)
            {
                closeButton.Command = ApplicationCommands.Close;
                CloseButton = closeButton;
            }
        }

        /// <summary>
        /// Attaches the minimize button
        /// </summary>
        private void AttachMinimizeButton()
        {
            if (MinimizeButton != null)
            {
                MinimizeButton.Command = null;
            }

            Button minimizeButton = GetChildControl<Button>("PART_MINIMIZE");
            if (minimizeButton != null)
            {
                minimizeButton.Command = MinimizeCommand;
                MinimizeButton = minimizeButton;
            }
        }

        /// <summary>
        /// Attaches the maximize restore button
        /// </summary>
        private void AttachMaximizeRestoreButton()
        {
            if (MaximizeRestoreButton != null)
            {
                MaximizeRestoreButton.Command = null;
            }

            Button maximizeRestoreButton = GetChildControl<Button>("PART_MAXIMIZE_RESTORE");
            if (maximizeRestoreButton != null)
            {
                maximizeRestoreButton.Command = MaximizeRestoreCommand;
                MaximizeRestoreButton = maximizeRestoreButton;
            }
        }

        /// <summary>
        /// Attaches the title bar to visual tree
        /// </summary>
        private void AttachTitleBar()
        {
            if (TitleBar != null)
            {
                TitleBar.RemoveHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnTitlebarClick));
            }

            UIElement titleBar = GetChildControl<UIElement>("PART_TITLEBAR");
            if (titleBar != null)
            {
                TitleBar = titleBar;
                titleBar.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnTitlebarClick));
            }
        }

        /// <summary>
        /// Called when titlebar is clicked or double clicked
        /// </summary>
        /// <param name="source">Event source</param>
        /// <param name="args">The <see cref="System.Windows.Input.MouseLeftButtonEventArgs"/> instance containing the event data</param>
        private void OnTitlebarClick(object source, MouseButtonEventArgs args)
        {
            switch (args.ClickCount)
            {
                case 1:
                    if (this.WindowState == WindowState.Normal)
                    {
                        DragMove();
                    }
                    break;
                case 2:
                    this.ChangeWindowState();
                    break;
            }
        }

        /// <summary>
        /// Attaches the borders to the visual tree
        /// </summary>
        private void AttachBorders()
        {
            AttachLeftBorder();
            AttachRightBorder();
            AttachTopBorder();
            AttachBottomBorder();
        }

        /// <summary>
        /// Attaches the left border to the visual tree
        /// </summary>
        private void AttachLeftBorder()
        {
            if (LeftBorder != null)
            {
                LeftBorder.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnBorderMouseLeftButtonDown));
                LeftBorder.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnBorderMouseLeftButtonUp));
                LeftBorder.MouseMove -= OnLeftBorderMouseMove;
            }

            UIElement leftBorder = GetChildControl<UIElement>("PART_LEFT_BORDER");
            if (leftBorder != null)
            {
                LeftBorder = leftBorder;
                leftBorder.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnBorderMouseLeftButtonDown));
                leftBorder.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnBorderMouseLeftButtonUp));
                leftBorder.MouseMove += OnLeftBorderMouseMove;
            }
        }

        /// <summary>
        /// Called when mouse left button is down on a border
        /// </summary>
        /// <param name="source">Event source</param>
        /// <param name="args">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data</param>
        private void OnBorderMouseLeftButtonDown(object source, MouseButtonEventArgs args)
        {
            IsResizing = true;
        }

        /// <summary>
        /// Called when mouse left button is up on a border
        /// </summary>
        /// <param name="source">Event source</param>
        /// <param name="args">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data</param>
        private void OnBorderMouseLeftButtonUp(object source, MouseButtonEventArgs args)
        {
            IsResizing = false;
            if (source is UIElement)
            {
                (source as UIElement).ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// Called when mouse moves over left border
        /// </summary>
        /// <param name="source">Event source</param>
        /// <param name="args">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data</param>
        private void OnLeftBorderMouseMove(object source, MouseEventArgs args)
        {
            if ((!LeftBorder.IsMouseCaptured) && IsResizing)
            {
                LeftBorder.CaptureMouse();
            }

            if (IsResizing)
            {
                double position = args.GetPosition(this).X;

                if (Math.Abs(position) < 10)
                {
                    return;
                }

                if ((position > 0) && ((Width - position) > MinWidth) && (Width > position))
                {
                    Left += position;
                    Width -= position;
                }
                else if ((position < 0) && (Left > 0))
                {
                    position = (Left + position > 0) ? position : -1 * Left;
                    Width = ActualWidth - position;
                    Left += position;
                }
            }
        }

        /// <summary>
        /// Attaches the right border to the visual tree
        /// </summary>
        private void AttachRightBorder()
        {
            if (RightBorder != null)
            {
                RightBorder.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnBorderMouseLeftButtonDown));
                RightBorder.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnBorderMouseLeftButtonUp));
                RightBorder.MouseMove -= OnRightBorderMouseMove;
            }

            UIElement rightBorder = GetChildControl<UIElement>("PART_RIGHT_BORDER");
            if (rightBorder != null)
            {
                RightBorder = rightBorder;
                rightBorder.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnBorderMouseLeftButtonDown));
                rightBorder.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnBorderMouseLeftButtonUp));
                rightBorder.MouseMove += OnRightBorderMouseMove;
            }
        }

        /// <summary>
        /// Called when mouse moves over right border
        /// </summary>
        /// <param name="source">Event source</param>
        /// <param name="args">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data</param>
        private void OnRightBorderMouseMove(object source, MouseEventArgs args)
        {
            if ((!RightBorder.IsMouseCaptured) && IsResizing)
            {
                RightBorder.CaptureMouse();
            }

            if (IsResizing)
            {
                double position = args.GetPosition(this).X;

                if (Math.Abs(position) < 10)
                {
                    return;
                }

                if (position > 0)
                {
                    Width = position;
                }
                else if ((position < 0) && (ActualWidth > MinWidth))
                {
                    position = (ActualWidth + position < MinWidth) ? MinWidth - ActualWidth : position;
                    Width = ActualWidth + position;
                }
            }
        }

        /// <summary>
        /// Attaches the top border to the visual tree
        /// </summary>
        private void AttachTopBorder()
        {
            if (TopBorder != null)
            {
                TopBorder.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnBorderMouseLeftButtonDown));
                TopBorder.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnBorderMouseLeftButtonUp));
                TopBorder.MouseMove -= OnRightBorderMouseMove;
            }

            UIElement topBorder = GetChildControl<UIElement>("PART_TOP_BORDER");
            if (topBorder != null)
            {
                TopBorder = topBorder;
                topBorder.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnBorderMouseLeftButtonDown));
                topBorder.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnBorderMouseLeftButtonUp));
                topBorder.MouseMove += OnTopBorderMouseMove;
            }
        }

        /// <summary>
        /// Called when mouse moves over top border
        /// </summary>
        /// <param name="source">Event source</param>
        /// <param name="args">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data</param>
        private void OnTopBorderMouseMove(object source, MouseEventArgs args)
        {
            if ((!TopBorder.IsMouseCaptured) && IsResizing)
            {
                TopBorder.CaptureMouse();
            }

            if (IsResizing)
            {
                double position = args.GetPosition(this).Y;

                if (Math.Abs(position) < 10)
                {
                    return;
                }

                if (position < 0)
                {
                    position = Top + position < 0 ? -Top : position;
                    Top += position;
                    Height = ActualHeight - position;
                }
                else if ((position > 0) && (ActualHeight - position > MinHeight))
                {
                    position = (ActualHeight - position < MinHeight) ? MinHeight - ActualHeight : position;
                    Height = ActualHeight - position;
                    Top += position;
                }
            }
        }

        /// <summary>
        /// Attaches the bottom border to the visual tree
        /// </summary>
        private void AttachBottomBorder()
        {
            if (BottomBorder != null)
            {
                BottomBorder.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnBorderMouseLeftButtonDown));
                BottomBorder.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnBorderMouseLeftButtonUp));
                BottomBorder.MouseMove -= OnBottomBorderMouseMove;
            }

            UIElement bottomBorder = GetChildControl<UIElement>("PART_BOTTOM_BORDER");
            if (bottomBorder != null)
            {
                BottomBorder = bottomBorder;
                bottomBorder.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnBorderMouseLeftButtonDown));
                bottomBorder.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnBorderMouseLeftButtonUp));
                bottomBorder.MouseMove += OnBottomBorderMouseMove;
            }
        }

        /// <summary>
        /// Called when mouse moves over bottom border
        /// </summary>
        /// <param name="source">Event source</param>
        /// <param name="args">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data</param>
        private void OnBottomBorderMouseMove(object source, MouseEventArgs args)
        {
            if ((!BottomBorder.IsMouseCaptured) && IsResizing)
            {
                BottomBorder.CaptureMouse();
            }

            if (IsResizing)
            {
                double position = args.GetPosition(this).Y - ActualHeight;

                if (Math.Abs(position) < 10)
                {
                    return;
                }

                if (position > 0)
                {
                    Height = ActualHeight + position;
                }
                else if ((position < 0) && (ActualHeight + position > MinHeight))
                {
                    position = (ActualHeight + position < MinHeight) ? MinHeight - ActualHeight : position;
                    Height = ActualHeight + position;
                }
            }
        }

        /// <summary>
        /// Gets the child control from the template
        /// </summary>
        /// <typeparam name="T">Type of control requested</typeparam>
        /// <param name="controlName">Name of the control</param>
        /// <returns>Control instance if there is one with the specified name; null otherwise</returns>
        private T GetChildControl<T>(string controlName) where T : DependencyObject
        {
            T control = GetTemplateChild(controlName) as T;
            return control;
        }

        /// <summary>
        /// Updates the border visibility.
        /// </summary>
        /// <param name="border">Border</param>
        /// <param name="visibility">Visibility</param>
        private void UpdateBorderVisibility(UIElement border, Visibility visibility)
        {
            if (border != null)
            {
                border.Visibility = visibility;
            }
        }

        /// <summary>
        /// Updates the restore bounds
        /// </summary>
        private void UpdateRestoreBounds()
        {
            RestoreBounds = new Rect(new Point(Left, Top), new Point(Left + ActualWidth, Top + ActualHeight));
        }

        /// <summary>
        /// Applies the restore bounds to the window
        /// </summary>
        private void ApplyRestoreBounds()
        {
            Left = RestoreBounds.Left;
            Top = RestoreBounds.Top;
            Width = RestoreBounds.Width;
            Height = RestoreBounds.Height;
        }

        /// <summary>
        /// Gets the size and location of a window before being either minimized or maximized.
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.Windows.Rect"/> that specifies the size and location of a window before being either minimized or maximized.</returns>
        private new Rect RestoreBounds { get; set; }

        /// <summary>
        /// Close button
        /// </summary>
        private Button CloseButton { get; set; }

        /// <summary>
        /// Minimize button
        /// </summary>
        private Button MinimizeButton { get; set; }

        /// <summary>
        /// Maximize / restore button
        /// </summary>
        /// <value>The maximize restore button.</value>
        private Button MaximizeRestoreButton { get; set; }

        private UIElement TitleBarContent;

        /// <summary>
        /// Title bar
        /// </summary>
        private UIElement TitleBar { get; set; }

        /// <summary>
        /// Left border
        /// </summary>
        private UIElement LeftBorder { get; set; }

        /// <summary>
        /// Right border
        /// </summary>
        private UIElement RightBorder { get; set; }

        /// <summary>
        /// Top border
        /// </summary>
        private UIElement TopBorder { get; set; }

        /// <summary>
        /// Bottom border
        /// </summary>
        private UIElement BottomBorder { get; set; }

        /// <summary>
        /// Indicates whether window is currently resizing
        /// </summary>
        /// <value>
        /// 	<c>true</c> If window is currently resizing; otherwise, <c>false</c>.
        /// </value>
        private bool IsResizing { get; set; }

        /// <summary>
        /// Minimize Command
        /// </summary>
        private readonly RoutedUICommand MinimizeCommand = new RoutedUICommand("Minimize", "Minimize", typeof(AdvancedWindow));

        /// <summary>
        /// Maximize / Restore command
        /// </summary>
        private readonly RoutedUICommand MaximizeRestoreCommand = new RoutedUICommand("MaximizeRestore", "MaximizeRestore", typeof(AdvancedWindow));
    }


    public class ClosedEventArgs : EventArgs
    {
        public bool Closed { get; } = true;
    }
}
