This demo shows how we can add NWaves audio effects, filters and block convolvers to UWP projects for online audio processing. In this example we work with ```AutowahEffect``` and allow user online-tweaking only couple of its parameters: maximum LFO frequency and Q factor.

![UWP](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/uwp.png)

Most of the code simply repeats [AudioCreation UWP sample code](https://github.com/microsoft/Windows-universal-samples/tree/master/Samples/AudioCreation/cs).

Effect is added to the ```AudioGraph``` here:

```C#
private void AddCustomEffect()
{
    PropertySet wahwahProperties = new PropertySet
    {
        { "Max frequency", 2000f },
        { "Q", 0.5f }
    };

    AudioEffectDefinition wahwahDefinition =
        new AudioEffectDefinition(typeof(NWavesEffect).FullName, wahwahProperties);

    fileInputNode.EffectDefinitions.Add(wahwahDefinition);
}
```


According to MS documentation, custom sound effects must be implemented in separate projects as Windows runtime components. [This project contains implementation of the effect](https://github.com/ar1st0crat/NWaves/tree/master/NWaves.DemoUwpEffect).
