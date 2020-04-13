﻿using System;
using System.Collections.Generic;
using Avalonia.Xaml.Interactivity;

namespace Avalonia.ExtendedToolkit.Helper
{
    /// <summary>
    /// collection of behaviours
    /// </summary>
    public class Behaviors : List<Behavior>
    {
    }

    /// <summary>
    /// class which can be used for adding behaviours to styles
    /// </summary>
    public static class StyledInteraction
    {
        static StyledInteraction()
        {
            BehaviorsProperty.Changed.AddClassHandler((Action<AvaloniaObject, AvaloniaPropertyChangedEventArgs>)((o, e) => OnBehaviorsPropertyChanged(o, e)));
        }

        /// <summary>
        /// add the behaviors to the collection
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private static void OnBehaviorsPropertyChanged(AvaloniaObject o, AvaloniaPropertyChangedEventArgs e)
        {
            var behaviors = Interaction.GetBehaviors(o);
            foreach (var behavior in e.NewValue as Behaviors)
            {
                behaviors.Add(behavior);
            }
        }

        /// <summary>
        /// attached properties which holds the definied behaviours
        /// </summary>
        public static readonly AttachedProperty<Behaviors> BehaviorsProperty =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, Behaviors>("Behaviors",typeof(StyledInteraction));

        public static Behaviors GetBehaviors(AvaloniaObject element)
        {
            return element.GetValue(BehaviorsProperty);
        }

        public static void SetBehaviors(AvaloniaObject element, Behaviors value)
        {
            element.SetValue(BehaviorsProperty, value);
        }
    }
}
