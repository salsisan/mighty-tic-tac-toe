using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Mighty_Tick_Tac_Toe
{
    class Animations
    {
        static public double inAnimationDurationSec = 0.4;
        static public EasingFunctionBase popInEasing = new BackEase();
        static public EasingFunctionBase popOutEasing = new BackEase();

        public static void PopUIElement(
            Windows.UI.Xaml.UIElement element,
            double scaleWidthFrom,
            double scaleWidthTo,
            double scaleHeightFrom,
            double scaleHeightTo,
            double moveWidthFrom,
            double moveWidthTo,
            double moveHeightFrom,
            double moveHeightTo,
            EasingFunctionBase easingFunc)
        {
            // animation
            element.RenderTransform = new CompositeTransform();
            Storyboard sb = new Storyboard();
            DoubleAnimation scalex = new DoubleAnimation()
            {
                From = scaleWidthFrom,
                To = scaleWidthTo,
                Duration = TimeSpan.FromSeconds(inAnimationDurationSec),
                EasingFunction = easingFunc
            };
            DoubleAnimation scaley = new DoubleAnimation()
            {
                From = scaleHeightFrom,
                To = scaleHeightTo,
                Duration = TimeSpan.FromSeconds(inAnimationDurationSec),
                EasingFunction = easingFunc
            };
            DoubleAnimation movex = new DoubleAnimation()
            {
                // move from the center of the cell to its left
                From = moveWidthFrom,
                To = moveWidthTo,
                Duration = TimeSpan.FromSeconds(inAnimationDurationSec),
                EasingFunction = easingFunc
            };
            DoubleAnimation movey = new DoubleAnimation()
            {
                // move from the center of the cell to its top
                From = moveHeightFrom,
                To = moveHeightTo,
                Duration = TimeSpan.FromSeconds(inAnimationDurationSec),
                EasingFunction = easingFunc
            };
            sb.Children.Add(scalex);
            sb.Children.Add(scaley);
            Storyboard.SetTargetProperty(scalex, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");
            Storyboard.SetTargetProperty(scaley, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)");
            Storyboard.SetTarget(scalex, element);
            Storyboard.SetTarget(scaley, element);

            sb.Children.Add(movex);
            sb.Children.Add(movey);
            Storyboard.SetTargetProperty(movex, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
            Storyboard.SetTargetProperty(movey, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
            Storyboard.SetTarget(movex, element);
            Storyboard.SetTarget(movey, element);

            sb.Begin();
        }

        public static void SlideUIElement(UIElement newGameImg)
        {
            Storyboard sb = new Storyboard();
            DoubleAnimation movey = new DoubleAnimation()
            {
                // move from the center of the cell to its top
                From = 100,
                To = 0,
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = Animations.popInEasing
            };
            sb.Children.Add(movey);
            Storyboard.SetTargetProperty(movey, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
            Storyboard.SetTarget(movey, newGameImg);

            sb.Begin();
        }

    }
}
