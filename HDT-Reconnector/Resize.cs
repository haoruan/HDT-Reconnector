using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Utility.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using Core = Hearthstone_Deck_Tracker.API.Core;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace HDT_Reconnector
{
    public class Resize
    {
        public ResizeGrip resizeGrip;
        private bool resizeElement = false;
        private bool lmbDown = false;
        private Point mousePos;
        private UserControl panel;
        private FrameworkElement button;
        private OutlinedTextBlock buttonText;
        public ResizeSettings settings;

        public Resize(UserControl panel,
            FrameworkElement button,
            OutlinedTextBlock buttonText,
            ResizeSettings settings)
        {
            this.panel = panel;
            this.button = button;
            this.buttonText = buttonText;
            this.settings = settings;

            resizeGrip = new ResizeGrip();
            resizeGrip.MouseDown += ResizeGrip_MouseDown;
            resizeGrip.MouseMove += ResizeGrip_MouseMove;
            resizeGrip.MouseUp += ResizeGrip_MouseUp;
            resizeGrip.Visibility = Visibility.Collapsed;

            OverlayExtensions.SetIsOverlayHitTestVisible(resizeGrip, true);
        }

        public void AddToOverlayWindowPrivate()
        {
            Dictionary<UIElement, ResizeGrip> _movableElements = (Dictionary<UIElement, ResizeGrip>)Utils.GetFieldValue(Core.OverlayWindow, "_movableElements");
            _movableElements.Add(button, resizeGrip);
            Utils.SetFieldValue(Core.OverlayWindow, "_movableElements", _movableElements);

            List<FrameworkElement> _clickableElements = (List<FrameworkElement>)Utils.GetFieldValue(Core.OverlayWindow, "_clickableElements");
            _clickableElements.Add(button);
            Utils.SetFieldValue(Core.OverlayWindow, "_clickableElements", _clickableElements);

            List<FrameworkElement> _hoverableElements = (List<FrameworkElement>)Utils.GetFieldValue(Core.OverlayWindow, "_hoverableElements");
            _clickableElements.Add(button);
            Utils.SetFieldValue(Core.OverlayWindow, "_hoverableElements", _hoverableElements);
        }

        public void RemoveFromOverlayWindowPrivate()
        {
            Dictionary<UIElement, ResizeGrip> _movableElements = (Dictionary<UIElement, ResizeGrip>)Utils.GetFieldValue(Core.OverlayWindow, "_movableElements");
            _movableElements.Remove(button);
            Utils.SetFieldValue(Core.OverlayWindow, "_movableElements", _movableElements);

            List<FrameworkElement> _clickableElements = (List<FrameworkElement>)Utils.GetFieldValue(Core.OverlayWindow, "_clickableElements");
            _clickableElements.Remove(button);
            Utils.SetFieldValue(Core.OverlayWindow, "_clickableElements", _clickableElements);

            List<FrameworkElement> _hoverableElements = (List<FrameworkElement>)Utils.GetFieldValue(Core.OverlayWindow, "_hoverableElements");
            _clickableElements.Remove(button);
            Utils.SetFieldValue(Core.OverlayWindow, "_hoverableElements", _hoverableElements);
        }

        private void ResizeGrip_MouseUp(object sender, MouseButtonEventArgs e)
        {
            lmbDown = false;
            resizeElement = false;
            resizeGrip.ReleaseMouseCapture();
        }

        private void ResizeGrip_MouseDown(object sender, MouseButtonEventArgs e)
        {
            resizeGrip.CaptureMouse();
            lmbDown = true;
            var pos = User32.GetMousePos();
            mousePos = new Point(pos.X, pos.Y);
            var relativePos = panel.PointFromScreen(mousePos);

            if (Utils.PointInsideControl(relativePos, button.Width, button.Height, new Thickness(0)))
            {
                if (Math.Abs(relativePos.X - button.Width) < 30
                       && Math.Abs(relativePos.Y - button.Height) < 30)
                {
                    resizeElement = true;
                }
            }
        }

        private void ResizeGrip_MouseMove(object sender, MouseEventArgs e)
        {
            if (!lmbDown)
                return;

            var pos = User32.GetMousePos();
            var newPos = new Point(pos.X, pos.Y);
            var delta = new Point((newPos.X - mousePos.X), (newPos.Y - mousePos.Y));
            mousePos = newPos;

            if (resizeElement)
            {
                // Maintain the button's shape, and fontsize >= 6
                var ratio = Math.Max(6 / settings.FontSize, 1 + delta.Y / button.Height);
                settings.Height *= ratio;
                settings.Width *= ratio;
                settings.FontSize *= ratio;
            }
            else
            {
                var scaleY = Core.OverlayWindow.Height / Utils.Resolution.Y;
                var scaleX = Core.OverlayWindow.Width / Utils.Resolution.X;

                // Ensure the top and left are inside the game window
                settings.Top = Math.Max(0, Math.Min(Utils.Resolution.Y - settings.Height,
                    settings.Top + delta.Y / scaleY));
                settings.Left = Math.Max(0, Math.Min(Utils.Resolution.X - settings.Width,
                    settings.Left + delta.X / scaleX));
            }

            UpdatePosition(true);
        }

        public void UpdatePosition(bool updateResizeGrip = false)
        {
            var scaleY = Core.OverlayWindow.Height / Utils.Resolution.Y;
            var scaleX = Core.OverlayWindow.Width / Utils.Resolution.X;

            // To keep the button's shape and size, here just apply autoscaling (0.8~1.3)
            button.Width = settings.Width * Core.OverlayWindow.AutoScaling;
            button.Height = settings.Height * Core.OverlayWindow.AutoScaling;
            buttonText.FontSize = settings.FontSize * Core.OverlayWindow.AutoScaling;

            // Ensure the button is inside the game window
            var newTop = Math.Max(0, Math.Min(Core.OverlayWindow.Height - button.Height,
                settings.Top * scaleY));
            var newLeft = Math.Max(0, Math.Min(Core.OverlayWindow.Width - button.Width,
                settings.Left * scaleX));

            Canvas.SetTop(panel, newTop);
            Canvas.SetLeft(panel, newLeft);
            Canvas.SetTop(button, newTop);
            Canvas.SetLeft(button, newLeft);

            if (updateResizeGrip)
            {
                resizeGrip.Width = button.Width;
                resizeGrip.Height = button.Height;
                Canvas.SetTop(resizeGrip, newTop);
                Canvas.SetLeft(resizeGrip, newLeft);
            }
        }


    }
}
