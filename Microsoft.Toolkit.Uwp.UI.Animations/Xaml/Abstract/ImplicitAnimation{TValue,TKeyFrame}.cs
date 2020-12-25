﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable

using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using static Microsoft.Toolkit.Uwp.UI.Animations.Extensions.AnimationExtensions;

namespace Microsoft.Toolkit.Uwp.UI.Animations.Xaml
{
    /// <summary>
    /// A base model representing a typed animation that can be used as an implicit composition animation.
    /// </summary>
    /// <inheritdoc cref="Animation{TValue, TKeyFrame}"/>
    public abstract class ImplicitAnimation<TValue, TKeyFrame> : Animation<TValue, TKeyFrame>, IImplicitTimeline
        where TKeyFrame : unmanaged
    {
        /// <summary>
        /// Gets or sets the optional implicit target for the animation.
        /// </summary>
        public string? ImplicitTarget { get; set; }

        /// <summary>
        /// Gets the target for the animation.
        /// </summary>
        protected abstract string Target { get; }

        /// <inheritdoc/>
        public CompositionAnimation GetAnimation(UIElement element, out string? target)
        {
            NormalizedKeyFrameAnimationBuilder<TKeyFrame>.Composition builder = new(
                Target,
                Delay ?? DefaultDuration,
                Duration ?? DefaultDuration);

            var (to, from) = GetParsedValues();

            // If there are no values set for the animation at all (no initial/target values, nor
            // keyframes), we just manually insert a single expression keyframe pointing to the final
            // value for the current animation. This is often the case with implicit animations, as
            // it is used to smoothly transition between two discrete property changes for a visual.
            if (to is null && from is null && KeyFrames.Count == 0)
            {
                builder.ExpressionKeyFrame(1.0, "this.FinalValue", DefaultEasingType, DefaultEasingMode);
            }
            else
            {
                // Otherwise, we just insert the keyframes for the initial/target values, as well as the
                // other keyframes that might be present into the current animation. The order is not
                // important when inserting keyframes, as each one stores the normalized progress value.
                if (to is not null)
                {
                    builder.KeyFrame(1.0, to.Value, EasingType ?? DefaultEasingType, EasingMode ?? DefaultEasingMode);
                }

                if (from is not null)
                {
                    builder.KeyFrame(0.0, from.Value, default, default);
                }

                foreach (var keyFrame in KeyFrames)
                {
                    keyFrame.AppendToBuilder(builder);
                }
            }

            target = ImplicitTarget;

            return builder.GetAnimation(element.GetVisual(), out _);
        }

        /// <summary>
        /// Gets the parsed <typeparamref name="TKeyFrame"/> values from <see cref="Animation{TValue,TKeyFrame}"/>.
        /// </summary>
        /// <returns>The parsed animation values as <typeparamref name="TKeyFrame"/>.</returns>
        protected abstract (TKeyFrame? To, TKeyFrame? From) GetParsedValues();
    }
}
