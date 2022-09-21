﻿using PaintDotNet.Direct2D1;
using PaintDotNet.Direct2D1.Effects;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;

namespace PaintDotNet.Effects.Gpu.Samples;

// A simple effect that renders a drop shadow using the Direct2D ShadowEffect.
// This effect requires that the "object" have a transparent background. It will not
// work if all pixels are opaque.
internal sealed class DropShadowGpuEffect
    : PropertyBasedGpuImageEffect
{
    public DropShadowGpuEffect()
        : base(
            "Drop Shadow (GPU Sample)",
            "GPU Samples",
            new GpuImageEffectOptions()
            {
                Flags = EffectFlags.Configurable
            })
    {
    }

    private enum PropertyNames
    {
        BlurRadius
    }

    protected override PropertyCollection OnCreatePropertyCollection()
    {
        List<Property> properties = new List<Property>();

        properties.Add(new Int32Property(PropertyNames.BlurRadius, 3, 0, 100));

        return new PropertyCollection(properties);
    }

    protected override void OnSetToken(PropertyBasedEffectConfigToken newToken)
    {
        this.blurRadius = newToken.GetProperty<Int32Property>(PropertyNames.BlurRadius).Value;
        base.OnSetToken(newToken);
    }

    private int blurRadius;

    protected override IDeviceImage OnCreateOutput(IDeviceContext deviceContext)
    {
        // Set up a simple transform graph.
        // SourceImage is plugged into ShadowEffect, which will rendered the shadow.
        // Then, CompositeEffect, which is used as the output, is used to blend SourceImage on top of ShadowEffect.

        ShadowEffect shadowEffect = new ShadowEffect(deviceContext);
        shadowEffect.Properties.Input.Set(this.Environment.SourceImage);
        shadowEffect.Properties.Optimization.SetValue(ShadowOptimization.Quality);
        shadowEffect.Properties.BlurStandardDeviation.SetValue(StandardDeviation.FromRadius(this.blurRadius));
        
        CompositeEffect compositeEffect = new CompositeEffect(deviceContext);
        compositeEffect.Properties.Destination.Set(shadowEffect);
        compositeEffect.Properties.Sources.Add(this.Environment.SourceImage);
        compositeEffect.Properties.Mode.SetValue(CompositeMode.SourceOver);

        return compositeEffect;
    }
}
