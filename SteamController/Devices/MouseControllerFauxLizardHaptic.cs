namespace SteamController.Devices
{
    public partial class MouseController
    {
        // Adjustable parameters (tweakable settings)
        private const int HapticBufferSize = 10;              // Haptic smoothing and jitter reduction buffer
        private const double HapticTriggerDelta = 30;         // Distance required for haptic feedback on each pad
        private const int HapticResetTime = 5;                // Number of seconds to invalidate a haptic feedback, prevents triggering on drag jitter

        // Functional constants (derived once)
        private static readonly double HapticBufferMidpoint = HapticBufferSize / 2.0;

        // Runtime state haptics
        private struct HapticState
        {
            public readonly int[] DxSign;
            public readonly int[] DySign;
            public readonly int[] DxFlipFlag;
            public readonly int[] DyFlipFlag;
            public readonly double[] Mag;

            public double Delta;
            public int BufferCount;
            public int BufferIndex;
            public int DxFlipCount;
            public int DyFlipCount;
            public int DxZeroCount;
            public int DyZeroCount;
            public double MagSum;
            public DateTime LastTime;
            public bool Cleared;

            public HapticState(int n)
            {
                DxSign = new int[n];
                DySign = new int[n];
                DxFlipFlag = new int[n];
                DyFlipFlag = new int[n];
                Mag = new double[n];

                Delta = 0;
                BufferCount = 0;
                BufferIndex = 0;
                DxFlipCount = 0;
                DyFlipCount = 0;
                DxZeroCount = 0;
                DyZeroCount = 0;
                MagSum = 0;
                LastTime = DateTime.MinValue;
                Cleared = true;
            }

            public void ResetBuffers()
            {
                Delta = 0;
                BufferCount = 0;
                BufferIndex = 0;
                DxFlipCount = 0;
                DyFlipCount = 0;
                DxZeroCount = 0;
                DyZeroCount = 0;
                MagSum = 0;
            }
        }

        private HapticState hapticR = new HapticState(HapticBufferSize);
        private HapticState hapticL = new HapticState(HapticBufferSize);

        // Checks whether the pads have been dragged enough to trigger a haptic feedback
        public bool HapticDragRFauxLizard(double dx, double dy, bool isTouched)
            => HapticDragFauxLizard(ref hapticR, dx, dy, isTouched);

        public bool HapticDragLFauxLizard(double dx, double dy, bool isTouched)
            => HapticDragFauxLizard(ref hapticL, dx, dy, isTouched);

        private bool HapticDragFauxLizard(ref HapticState s, double dx, double dy, bool isTouched)
        {
            if (isTouched)
            {
                s.Cleared = false;

                // Timeout check: reset delta if accumulation is too slow
                DateTime now = DateTime.UtcNow;
                if (s.BufferCount == 0)
                {
                    s.LastTime = now;
                }
                else if ((now - s.LastTime).TotalSeconds > HapticResetTime)
                {
                    s.Delta = 0;
                    s.LastTime = now;
                }

                // If buffer is full, remove array contributions at this slot
                if (s.BufferCount == HapticBufferSize)
                {
                    int idx = s.BufferIndex;
                    s.DxFlipCount -= s.DxFlipFlag[idx];
                    s.DyFlipCount -= s.DyFlipFlag[idx];
                    if (s.DxSign[idx] == 0) s.DxZeroCount--;
                    if (s.DySign[idx] == 0) s.DyZeroCount--;
                    s.MagSum -= s.Mag[idx];
                }
                else
                {
                    s.BufferCount++;
                }

                // Compute current signs
                int curDxSign = Math.Sign(dx);
                int curDySign = Math.Sign(dy);

                // Compute flips against previous signs
                int dxFlip = 0, dyFlip = 0;
                if (s.BufferCount > 1)
                {
                    int prevIndex = (s.BufferIndex - 1 + HapticBufferSize) % HapticBufferSize;
                    int prevDxSign = s.DxSign[prevIndex];
                    int prevDySign = s.DySign[prevIndex];

                    if (prevDxSign != 0 && curDxSign != 0 && prevDxSign != curDxSign) dxFlip = 1;
                    if (prevDySign != 0 && curDySign != 0 && prevDySign != curDySign) dyFlip = 1;
                }

                // Compute current magnitude
                double mag = Math.Sqrt(dx * dx + dy * dy);

                // Store signs and flip flags and magnitude in array for rolling sums
                int i = s.BufferIndex;
                s.DxSign[i] = curDxSign;
                s.DySign[i] = curDySign;
                s.DxFlipFlag[i] = dxFlip;
                s.DyFlipFlag[i] = dyFlip;
                s.Mag[i] = mag;

                // Increment rolling sums
                if (curDxSign == 0) s.DxZeroCount++;
                if (curDySign == 0) s.DyZeroCount++;
                s.DxFlipCount += dxFlip;
                s.DyFlipCount += dyFlip;
                s.MagSum += mag;

                // Store new buffer index for later
                s.BufferIndex++;
                if (s.BufferIndex == HapticBufferSize) s.BufferIndex = 0;

                // No need to proceed as buffer is not full yet
                if (s.BufferCount != HapticBufferSize)
                    return false;

                // Compute average magnitude over buffer for smoothing
                double avgMag = s.MagSum / s.BufferCount;

                // Calculate a penalty for the magnitude
                double bufferSpan = HapticBufferSize - HapticBufferMidpoint;
                double invSpan = 1.0 / bufferSpan;
                bool dxZeroEdge = s.DxZeroCount == s.BufferCount;
                bool dyZeroEdge = s.DyZeroCount == s.BufferCount;
                double factor = (dxZeroEdge || dyZeroEdge) ? 1.0 : 0.5;

                double dxFactor = dxZeroEdge
                    ? 0.0
                    : (s.DxFlipCount < HapticBufferMidpoint
                        ? factor
                        : -factor * ((s.DxFlipCount - HapticBufferMidpoint) * invSpan));

                double dyFactor = dyZeroEdge
                    ? 0.0
                    : (s.DyFlipCount < HapticBufferMidpoint
                        ? factor
                        : -factor * ((s.DyFlipCount - HapticBufferMidpoint) * invSpan));

                // Final penalty
                s.Delta += avgMag * (dxFactor + dyFactor);

                if (s.Delta >= HapticTriggerDelta)
                {
                    s.Delta -= Math.Floor(s.Delta / HapticTriggerDelta) * HapticTriggerDelta;
                    s.LastTime = now;
                    return true;
                }
                else if (s.Delta < 0)
                {
                    s.Delta = 0;
                }
            }
            else
            {
                if (!s.Cleared)
                {
                    s.Cleared = true;
                    s.ResetBuffers();
                }
            }

            return false;
        }
    }
}