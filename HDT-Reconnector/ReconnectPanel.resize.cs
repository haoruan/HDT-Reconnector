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

namespace HDT_Reconnector
{
    public partial class ReconnectPanel
    {
        private bool resizeElement = false;
        private ResizeGrip resizeGrip;
        private bool lmbDown = false;
        private Point mousePos;

        private void AddToOverlayWindowPrivate()
        {
            Dictionary<UIElement, ResizeGrip> _movableElements = (Dictionary<UIElement, ResizeGrip>)Utils.GetFieldValue(Core.OverlayWindow, "_movableElements");
            _movableElements.Add(ReconnectButton, resizeGrip);
            Utils.SetFieldValue(Core.OverlayWindow, "_movableElements", _movableElements);

            List<FrameworkElement> _clickableElements = (List<FrameworkElement>)Utils.GetFieldValue(Core.OverlayWindow, "_clickableElements");
            _clickableElements.Add(ReconnectButton);
            Utils.SetFieldValue(Core.OverlayWindow, "_clickableElements", _clickableElements);

            List<FrameworkElement> _hoverableElements = (List<FrameworkElement>)Utils.GetFieldValue(Core.OverlayWindow, "_hoverableElements");
            _clickableElements.Add(ReconnectButton);
            Utils.SetFieldValue(Core.OverlayWindow, "_hoverableElements", _hoverableElements);
        }

        private void RemoveFromOverlayWindowPrivate()
        {
            Dictionary<UIElement, ResizeGrip> _movableElements = (Dictionary<UIElement, ResizeGrip>)Utils.GetFieldValue(Core.OverlayWindow, "_movableElements");
            _movableElements.Remove(ReconnectButton);
            Utils.SetFieldValue(Core.OverlayWindow, "_movableElements", _movableElements);

            List<FrameworkElement> _clickableElements = (List<FrameworkElement>)Utils.GetFieldValue(Core.OverlayWindow, "_clickableElements");
            _clickableElements.Remove(ReconnectButton);
            Utils.SetFieldValue(Core.OverlayWindow, "_clickableElements", _clickableElements);

            List<FrameworkElement> _hoverableElements = (List<FrameworkElement>)Utils.GetFieldValue(Core.OverlayWindow, "_hoverableElements");
            _clickableElements.Remove(ReconnectButton);
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
            Log.Info("ResizeGrip_MouseDown");
            resizeGrip.CaptureMouse();
            lmbDown = true;
            var pos = User32.GetMousePos();
            mousePos = new Point(pos.X, pos.Y);
            var relativePos = PointFromScreen(mousePos);

            if (Utils.PointInsideControl(relativePos, ReconnectButton.Width, ReconnectButton.Height, new Thickness(0)))
            {
                if (Math.Abs(relativePos.X - ReconnectButton.Width) < 30
                       && Math.Abs(relativePos.Y - ReconnectButton.Height) < 30)
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
                var ratio = Math.Max(6 / Settings.Instance.FontSize, 1 + delta.Y / ReconnectButton.Height);
                Settings.Instance.Height *= ratio;
                Settings.Instance.Width *= ratio;
                Settings.Instance.FontSize *= ratio;
            }
            else
            {
                var scaleY = Core.OverlayWindow.Height / Utils.Resolution.Y;
                var scaleX = Core.OverlayWindow.Width / Utils.Resolution.X;

                // Ensure the top and left are inside the game window
                Settings.Instance.Top = Math.Max(0, Math.Min(Utils.Resolution.Y - Settings.Instance.Height,
                    Settings.Instance.Top + delta.Y / scaleY));
                Settings.Instance.Left = Math.Max(0, Math.Min(Utils.Resolution.X - Settings.Instance.Width,
                    Settings.Instance.Left + delta.X / scaleX));
            }

            UpdatePosition(true);
        }

        private void UpdatePosition(bool updateResizeGrip = false)
        {
            var scaleY = Core.OverlayWindow.Height / Utils.Resolution.Y;
            var scaleX = Core.OverlayWindow.Width / Utils.Resolution.X;

            // To keep the button's shape and size, here just apply autoscaling (0.8~1.3)
            ReconnectButton.Width = Settings.Instance.Width * Core.OverlayWindow.AutoScaling;
            ReconnectButton.Height = Settings.Instance.Height * Core.OverlayWindow.AutoScaling;
            ReconnectText.FontSize = Settings.Instance.FontSize * Core.OverlayWindow.AutoScaling;

            // Ensure the button is inside the game window
            var newTop = Math.Max(0, Math.Min(Core.OverlayWindow.Height - ReconnectButton.Height,
                Settings.Instance.Top * scaleY));
            var newLeft = Math.Max(0, Math.Min(Core.OverlayWindow.Width - ReconnectButton.Width,
                Settings.Instance.Left * scaleX));

            Canvas.SetTop(this, newTop);
            Canvas.SetLeft(this, newLeft);
            Canvas.SetTop(ReconnectButton, newTop);
            Canvas.SetLeft(ReconnectButton, newLeft);

            if (updateResizeGrip)
            {
                resizeGrip.Width = ReconnectButton.Width;
                resizeGrip.Height = ReconnectButton.Height;
                Canvas.SetTop(resizeGrip, newTop);
                Canvas.SetLeft(resizeGrip, newLeft);
            }
        }


    }
}
