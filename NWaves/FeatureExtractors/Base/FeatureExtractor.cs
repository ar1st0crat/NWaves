using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NWaves.FeatureExtractors.Options;
using NWaves.Signals;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.FeatureExtractors.Base
{
    /// <summary>
    /// Abstract class for all feature extractors.
    /// 
    /// NOTE.
    /// All fields of FeatureExtractor class and its subclasses are made protected.
    /// Conceptually they should be private, especially internal buffers,
    /// but making them protected allows developers to extend extractors
    /// more efficiently by reusing memory already allocated in base classes.
    /// 
    /// </summary>
    public abstract class FeatureExtractor
    {
        /// <summary>
        /// Number of features to extract
        /// </summary>
        public int FeatureCount { get; protected set; }

        /// <summary>
        /// String annotations (or simply names) of features
        /// </summary>
        public abstract List<string> FeatureDescriptions { get; }

        /// <summary>
        /// String annotations (or simply names) of delta features (1st order derivatives)
        /// </summary>
        public virtual List<string> DeltaFeatureDescriptions
        {
            get { return FeatureDescriptions.Select(d => "delta_" + d).ToList(); }
        }

        /// <summary>
        /// String annotations (or simply names) of delta-delta features (2nd order derivatives)
        /// </summary>
        public virtual List<string> DeltaDeltaFeatureDescriptions
        {
            get { return FeatureDescriptions.Select(d => "delta_delta_" + d).ToList(); }
        }

        /// <summary>
        /// Length of analysis frame (in seconds)
        /// </summary>
        public double FrameDuration { get; protected set; }

        /// <summary>
        /// Hop length (in seconds)
        /// </summary>
        public double HopDuration { get; protected set; }

        /// <summary>
        /// Size of analysis frame (in samples)
        /// </summary>
        public int FrameSize { get; protected set; }

        /// <summary>
        /// Hop size (in samples)
        /// </summary>
        public int HopSize { get; protected set; }

        /// <summary>
        /// Sampling rate that the processed signals are expected to have
        /// </summary>
        public int SamplingRate { get; protected set; }

        /// <summary>
        /// Size of the block for processing at each step.
        /// This field is usually set in subclass methods.
        /// </summary>
        protected int _blockSize;

        /// <summary>
        /// Pre-emphasis coefficient
        /// </summary>
        protected float _preEmphasis;

        /// <summary>
        /// Type of the window function
        /// </summary>
        protected readonly WindowTypes _window;

        /// <summary>
        /// Window samples
        /// </summary>
        protected readonly float[] _windowSamples;

        /// <summary>
        /// Construct extractor from sampling rate, frame duration and hop duration (in seconds)
        /// </summary>
        /// <param name="samplingRate"></param>
        protected FeatureExtractor(FeatureExtractorOptions options)
        {
            if (options.Errors.Count > 0)
            {
                throw new ArgumentException("Invalid configuration:\r\n" + string.Join("\r\n", options.Errors));
            }

            SamplingRate = options.SamplingRate;

            if (options.FrameSize > 0)  // frame size has priority over frame duration 
            {
                FrameSize = options.FrameSize;
                FrameDuration = (double)FrameSize / SamplingRate;
            }
            else
            {
                FrameDuration = options.FrameDuration;
                FrameSize = (int)Math.Round(SamplingRate * FrameDuration, MidpointRounding.AwayFromZero);
            }

            if (options.HopSize > 0)  // hop size has priority over hop duration 
            {
                HopSize = options.HopSize;
                HopDuration = (double)HopSize / SamplingRate;
            }
            else
            {
                HopDuration = options.HopDuration;
                HopSize = (int)Math.Round(SamplingRate * HopDuration, MidpointRounding.AwayFromZero);
            }

            _blockSize = FrameSize;
            _preEmphasis = (float)options.PreEmphasis;
            _window = options.Window;

            if (_window != WindowTypes.Rectangular)
            {
                _windowSamples = Window.OfType(_window, FrameSize);
            }
        }

        /// <summary>
        /// Compute the sequence of feature vectors from some part of array of samples.
        /// </summary>
        /// <param name="samples">Array of real-valued samples</param>
        /// <param name="startSample">The offset (position) of the first sample for processing</param>
        /// <param name="endSample">The offset (position) of last sample for processing</param>
        /// <param name="vectors">Pre-allocated sequence of feature vectors</param>
        public virtual int ComputeFrom(float[] samples, int startSample, int endSample, IList<float[]> vectors)
        {
            Guard.AgainstInvalidRange(startSample, endSample, "starting pos", "ending pos");

            var frameSize = FrameSize;
            var hopSize = HopSize;
            var prevSample = startSample > 0 ? samples[startSample - 1] : 0f;
            var lastSample = endSample - frameSize;

            var block = new float[_blockSize];


            // Main processing loop:

            // at each iteration one frame is processed;
            // the frame is contained within a block which, in general, can have larger size
            // (usually it's a zero-padded frame for radix-2 FFT);
            // this block array is reused so the frame needs to be zero-padded at each iteration.
            // Array.Clear() is quite slow for *small* arrays compared to zero-fill in a for-loop.
            // Since usually the frame size is chosen to be close to block (FFT) size 
            // we don't need to pad very big number of zeros, so we use for-loop here.

            var i = 0;

            for (int sample = startSample; sample <= lastSample; sample += hopSize, i++)
            {
                // prepare new block for processing ======================================================

                samples.FastCopyTo(block, frameSize, sample);  // copy FrameSize samples to 'block' buffer

                for (var k = frameSize; k < block.Length; block[k++] = 0) { }    // pad zeros to blockSize


                // (optionally) do pre-emphasis ==========================================================

                if (_preEmphasis > 1e-10f)
                {
                    for (var k = 0; k < frameSize; k++)
                    {
                        var y = block[k] - prevSample * _preEmphasis;
                        prevSample = block[k];
                        block[k] = y;
                    }
                    prevSample = samples[sample + hopSize - 1];
                }

                // (optionally) apply window

                if (_windowSamples != null)
                {
                    block.ApplyWindow(_windowSamples);
                }


                // process this block and compute features =============================================

                ProcessFrame(block, vectors[i]);
            }

            return i;
        }

        /// <summary>
        /// Compute the sequence of feature vectors from some part of array of samples.
        /// </summary>
        /// <param name="samples">Array of real-valued samples</param>
        /// <param name="startSample">The offset (position) of the first sample for processing</param>
        /// <param name="endSample">The offset (position) of last sample for processing</param>
        /// <returns>Sequence of feature vectors or null, if the number of samples is less than the size of analysis frame</returns>
        public virtual List<float[]> ComputeFrom(float[] samples, int startSample, int endSample)
        {
            Guard.AgainstInvalidRange(startSample, endSample, "starting pos", "ending pos");

            if (endSample - startSample < FrameSize)
            {
                return null;
            }

            // pre-allocate memory for data:

            var totalCount = (endSample - FrameSize - startSample) / HopSize + 1;

            var featureVectors = new List<float[]>(totalCount);
            for (var i = 0; i < totalCount; i++)
            {
                featureVectors.Add(new float[FeatureCount]);
            }

            ComputeFrom(samples, startSample, endSample, featureVectors);

            return featureVectors;
        }

        /// <summary>
        /// Time markers
        /// </summary>
        /// <param name="vectorCount">Number of feature vectors</param>
        /// <param name="startFrom">Starting time position</param>
        /// <returns>List of time markers</returns>
        public virtual List<double> TimeMarkers(int vectorCount, double startFrom = 0)
        {
            return Enumerable.Range(0, vectorCount)
                             .Select(x => startFrom + x * HopDuration)
                             .ToList();
        }

        /// <summary>
        /// Process one frame in block of data at each step
        /// (in general block can be longer than frame, e.g. zero-padded block for FFT)
        /// </summary>
        /// <param name="block">Block of data</param>
        /// <param name="features">Features computed in the block</param>
        public abstract void ProcessFrame(float[] block, float[] features);

        /// <summary>
        /// Compute the sequence of feature vectors from the entire array of samples
        /// </summary>
        /// <param name="samples">Array of real-valued samples</param>
        /// <returns>Sequence of feature vectors</returns>
        public List<float[]> ComputeFrom(float[] samples)
        {
            return ComputeFrom(samples, 0, samples.Length);
        }

        /// <summary>
        /// Compute the sequence of feature vectors from some fragment of a signal
        /// </summary>
        /// <param name="signal">Discrete real-valued signal</param>
        /// <param name="startSample">The offset (position) of the first sample for processing</param>
        /// <param name="endSample">The offset (position) of the last sample for processing</param>
        /// <returns>Sequence of feature vectors</returns>
        public List<float[]> ComputeFrom(DiscreteSignal signal, int startSample, int endSample)
        {
            return ComputeFrom(signal.Samples, startSample, endSample);
        }

        /// <summary>
        /// Compute the sequence of feature vectors from the entire DiscreteSignal
        /// </summary>
        /// <param name="signal">Discrete real-valued signal</param>
        /// <returns>Sequence of feature vectors</returns>
        public List<float[]> ComputeFrom(DiscreteSignal signal)
        {
            return ComputeFrom(signal, 0, signal.Length);
        }

        /// <summary>
        /// Reset feature extractor's state
        /// </summary>
        public virtual void Reset()
        {
        }
        
        #region parallelization

        /// <summary>
        /// True if computations can be done in parallel
        /// </summary>
        /// <returns></returns>
        public virtual bool IsParallelizable() => false;

        /// <summary>
        /// Copy of current extractor that can work in parallel
        /// </summary>
        /// <returns></returns>
        public virtual FeatureExtractor ParallelCopy() => null;

        /// <summary>
        /// Parallel computation (returns chunks of fecture vector lists)
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="startSample"></param>
        /// <param name="endSample"></param>
        /// <param name="parallelThreads"></param>
        /// <returns></returns>
        public virtual List<float[]>[] ParallelChunksComputeFrom(float[] samples, int startSample, int endSample, int parallelThreads = 0)
        {
            if (!IsParallelizable())
            {
                throw new NotImplementedException("Current configuration of the extractor does not support parallel computation");
            }

            var threadCount = parallelThreads > 0 ? parallelThreads : Environment.ProcessorCount;
            var chunkSize = (endSample - startSample) / threadCount;

            if (chunkSize < FrameSize)  // don't parallelize too short signals
            {
                return new List<float[]>[] { ComputeFrom(samples, startSample, endSample) };
            }

            var extractors = new FeatureExtractor[threadCount];
            extractors[0] = this;
            for (var i = 1; i < threadCount; i++)
            {
                extractors[i] = ParallelCopy();
            }

            // ============== carefully define the sample positions for merging ===============

            var startPositions = new int[threadCount];
            var endPositions = new int[threadCount];

            var hopCount = (chunkSize - FrameSize) / HopSize;

            var lastPosition = startSample - 1;
            for (var i = 0; i < threadCount; i++)
            {
                startPositions[i] = lastPosition + 1;
                endPositions[i] = lastPosition + hopCount * HopSize + FrameSize;
                lastPosition = endPositions[i] - FrameSize;
            }

            endPositions[threadCount - 1] = endSample;

            // =========================== actual parallel computing ===========================

            var featureVectors = new List<float[]>[threadCount];

            Parallel.For(0, threadCount, i =>
            {
                featureVectors[i] = extractors[i].ComputeFrom(samples, startPositions[i], endPositions[i]);
            });

            return featureVectors;
        }

        /// <summary>
        /// Parallel computation (joins chunks of feature vector lists into one list)
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="startSample"></param>
        /// <param name="endSample"></param>
        /// <param name="parallelThreads"></param>
        /// <returns></returns>
        public virtual List<float[]> ParallelComputeFrom(float[] samples, int startSample, int endSample, int parallelThreads = 0)
        {
            var chunks = ParallelChunksComputeFrom(samples, startSample, endSample, parallelThreads);

            var featureVectors = new List<float[]>();

            foreach (var vectors in chunks)
            {
                featureVectors.AddRange(vectors);
            }

            return featureVectors;
        }

        /// <summary>
        /// Parallel computation (joins chunks of feature vector lists into one list)
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="parallelThreads"></param>
        /// <returns></returns>
        public virtual List<float[]> ParallelComputeFrom(float[] samples, int parallelThreads = 0)
        {
            return ParallelComputeFrom(samples, 0, samples.Length, parallelThreads);
        }

        /// <summary>
        /// Compute the sequence of feature vectors from some fragment of a signal
        /// </summary>
        /// <param name="signal">Discrete real-valued signal</param>
        /// <param name="startSample">The offset (position) of the first sample for processing</param>
        /// <param name="endSample">The offset (position) of the last sample for processing</param>
        /// <param name="parallelThreads">Number of threads</param>
        /// <returns>Sequence of feature vectors</returns>
        public List<float[]> ParallelComputeFrom(DiscreteSignal signal, int startSample, int endSample, int parallelThreads = 0)
        {
            return ParallelComputeFrom(signal.Samples, startSample, endSample, parallelThreads);
        }

        /// <summary>
        /// Compute the sequence of feature vectors from the entire DiscreteSignal
        /// </summary>
        /// <param name="signal">Discrete real-valued signal</param>
        /// <param name="parallelThreads">Number of threads</param>
        /// <returns>Sequence of feature vectors</returns>
        public List<float[]> ParallelComputeFrom(DiscreteSignal signal, int parallelThreads = 0)
        {
            return ParallelComputeFrom(signal.Samples, 0, signal.Length, parallelThreads);
        }

        #endregion
    }
}
