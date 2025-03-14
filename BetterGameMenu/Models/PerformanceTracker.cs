using System;
using System.Diagnostics;

namespace Leclair.Stardew.BetterGameMenu.Models;

internal sealed class PerformanceTracker {

	private readonly uint[] Samples;
	private byte LastIndex = byte.MaxValue;
	private uint SampleCount = 0;

	private readonly Stopwatch Timer = new();

	public PerformanceTracker() {
		Samples = GC.AllocateArray<uint>(byte.MaxValue + 1);
	}

	public void Start() {
		Timer.Restart();
	}

	public void Stop() {
		Timer.Stop();
		LastIndex++;
		Samples[LastIndex] = (uint) Timer.ElapsedTicks;
		if (SampleCount < Samples.Length)
			SampleCount++;
	}

	public uint LastSample => SampleCount > 0 ? Samples[LastIndex] : 0;

	public uint Average {
		get {
			if (SampleCount == 0)
				return 0;

			ulong sum = 0;
			for(int i = 0; i < SampleCount; i++)
				sum += Samples[i];

			return (uint) (sum / SampleCount);
		}
	}

	public uint Minimum {
		get {
			if (SampleCount == 0)
				return 0;

			uint minimum = uint.MaxValue;
			for(int i = 0; i < SampleCount; i++) {
				uint sample = Samples[i];
				if (sample < minimum) minimum = sample;
			}

			return minimum;
		}
	}

	public uint Maximum {
		get {
			if (SampleCount == 0)
				return 0;

			uint maximum = uint.MinValue;
			for (int i = 0; i < SampleCount; i++) {
				uint sample = Samples[i];
				if (sample > maximum) maximum = sample;
			}

			return maximum;
		}
	}

	public (uint Average, uint Minimum, uint Maximum) Statistics {
		get {
			if (SampleCount == 0)
				return (0, 0, 0);

			ulong sum = 0;
			uint minimum = uint.MaxValue;
			uint maximum = uint.MinValue;

			for (int i = 0; i < SampleCount; i++) {
				uint sample = Samples[i];
				if (sample < minimum) minimum = sample;
				if (sample > maximum) maximum = sample;
				sum += sample;
			}

			return ((uint)(sum / SampleCount), minimum, maximum);
		}
	}

	public string StatString {
		get {
			var stats = Statistics;
			return $"min:{stats.Minimum}, avg: {stats.Average}, max:{stats.Maximum}, last:{Samples[LastIndex]}";
		}
	}

}
