NWaves effects are UWP-friendly. All we need to do is just adapt audio effect object to ```IBasicAudioEffect``` interface (i.e. Adapter pattern (GOF)) and add few lines of code:

```C#
public sealed class NWavesEffect : IBasicAudioEffect
{
    // aggregate AutoWah effect

    AutowahEffect autowahEffect;

    // ...

    unsafe public void ProcessFrame(ProcessAudioFrameContext context)
    {
        // update effect settings if necessary

        var q = Q;
        var maxFrequency = MaxFrequency;

        if (maxFrequency != autowahEffect.MaxFrequency)
        {
            autowahEffect.MaxFrequency = maxFrequency;
        }
        if (q != autowahEffect.Q)
        {
            autowahEffect.Q = q;
        }


        // this is a standard UWP code for casting byte arrays to float (unsafe) arrays:


        AudioFrame inputFrame = context.InputFrame;
        AudioFrame outputFrame = context.OutputFrame;

        using (AudioBuffer inputBuffer = inputFrame.LockBuffer(AudioBufferAccessMode.Read),
                           outputBuffer = outputFrame.LockBuffer(AudioBufferAccessMode.Write))
        using (IMemoryBufferReference inputReference = inputBuffer.CreateReference(),
                                      outputReference = outputBuffer.CreateReference())
        {
            ((IMemoryBufferByteAccess)inputReference).GetBuffer(out byte* inputDataInBytes, out uint inputCapacity);
            ((IMemoryBufferByteAccess)outputReference).GetBuffer(out byte* outputDataInBytes, out uint outputCapacity);

            float* inputDataInFloat = (float*)inputDataInBytes;
            float* outputDataInFloat = (float*)outputDataInBytes;

            int dataInFloatLength = (int)inputBuffer.Length / sizeof(float);


            // ========= the simplest approach -
            // === just call Process() method in the loop:


            for (int i = 0; i < dataInFloatLength; i++)
            {
                outputDataInFloat[i] = autowahEffect.Process(inputDataInFloat[i]);
            }
        }
    }
}
```
