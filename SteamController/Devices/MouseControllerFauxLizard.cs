using System;
using static SteamController.Devices.SteamController;

namespace SteamController.Devices
{
    public partial class MouseController : IDisposable
    {
        // Adjustable parameters (tweakable settings)
        private const int bufferSize = 4;                     // Input smoothing buffer size
        private const int gestureRadius = 30;                 // Gesture commit threshold (pixels)
        private const double gestureFlushRate = 0.1;          // Rate at which gesture flush decays
        private const int velocityWindowSize = 20;            // Velocity smoothing window size
        private const double minGlideMagnitude = 0.8;         // Minimum velocity to start glide
        private const double minGlideVelocity = 0.15;         // Minimum velocity to continue glide
        private const int hapticBufferSize = 10;             // Haptic smoothing and jitter reduction buffer
        private const double hapticTriggerDelta = 30;         // Distance required for haptic feedback on each pad

        // Functional constants (derived once)
        private readonly double gestureRadiusSq = gestureRadius * gestureRadius;
        private readonly double minGlideMagnitudeSq = minGlideMagnitude * minGlideMagnitude;
        private readonly double minGlideVelocitySq = minGlideVelocity * minGlideVelocity;
        private readonly double hapticBufferMidpoint = hapticBufferSize / 2.0;

        // Runtime state RPad
        private Queue<double> bufX = new(), bufY = new();
        private double totalDeltaX = 0, totalDeltaY = 0;
        private bool gestureCommitted = false;
        private double gestureFlushX = 0, gestureFlushY = 0;

        private bool isGliding = false;
        private DateTime releaseTime;
        private double releaseVelocityX = 0, releaseVelocityY = 0;
        private Queue<double> velocityHistoryX = new(), velocityHistoryY = new();
        private double velocitySumX = 0, velocitySumY = 0;

        // Runtime state haptics
        private readonly int[] hapticDxSignBufferR = new int[hapticBufferSize];
        private readonly int[] hapticDySignBufferR = new int[hapticBufferSize];
        private readonly int[] hapticDxFlipFlagBufferR = new int[hapticBufferSize];
        private readonly int[] hapticDyFlipFlagBufferR = new int[hapticBufferSize];
        private readonly double[] hapticMagBufferR = new double[hapticBufferSize];

        private double hapticDeltaR = 0;
        private int hapticBufferCountR = 0;
        private int hapticBufferIndexR = 0;
        private int hapticDxFlipCountR = 0;
        private int hapticDyFlipCountR = 0;
        private int hapticDxZeroCountR = 0;
        private int hapticDyZeroCountR = 0;
        private double hapticMagSumR = 0;

        private bool hapticClearedR = true;

        private readonly int[] hapticDxSignBufferL = new int[hapticBufferSize];
        private readonly int[] hapticDySignBufferL = new int[hapticBufferSize];
        private readonly int[] hapticDxFlipFlagBufferL = new int[hapticBufferSize];
        private readonly int[] hapticDyFlipFlagBufferL = new int[hapticBufferSize];
        private readonly double[] hapticMagBufferL = new double[hapticBufferSize];

        private double hapticDeltaL = 0;
        private int hapticBufferCountL = 0;
        private int hapticBufferIndexL = 0;
        private int hapticDxFlipCountL = 0;
        private int hapticDyFlipCountL = 0;
        private int hapticDxZeroCountL = 0;
        private int hapticDyZeroCountL = 0;
        private double hapticMagSumL = 0;

        private bool hapticClearedL = true;

        // Main movement logic
        public void MoveByFauxLizard(double dx, double dy, bool isTouched)
        {
            if (!isTouched)
            {
                // Start glide if gesture was committed and velocity is high enough
                if (!isGliding && gestureCommitted)
                {
                    double magSq = releaseVelocityX * releaseVelocityX + releaseVelocityY * releaseVelocityY;
                    if (magSq >= minGlideMagnitudeSq)
                    {
                        releaseTime = DateTime.Now;
                        isGliding = true;
                    }
                }

                // Continue glide if active
                if (isGliding)
                {
                    double elapsed = (DateTime.Now - releaseTime).TotalSeconds;
                    double glideX = ApplyReleaseGlide(releaseVelocityX, elapsed);
                    double glideY = ApplyReleaseGlide(releaseVelocityY, elapsed);

                    double glideMagSq = glideX * glideX + glideY * glideY;
                    if (glideMagSq < minGlideVelocitySq)
                    {
                        isGliding = false;
                        releaseVelocityX = releaseVelocityY = 0;
                    }
                    else
                    {
                        MoveBy(glideX, glideY);
                    }
                }

                // Reset gesture state
                gestureCommitted = false;
                totalDeltaX = totalDeltaY = 0;
                return;
            }

            isGliding = false;

            // Smooth input deltas
            double smoothedX = SmoothDelta(dx, bufX);
            double smoothedY = SmoothDelta(dy, bufY);

            // Apply acceleration ramp
            double rampedX = ApplyAccelerationRamp(smoothedX);
            double rampedY = ApplyAccelerationRamp(smoothedY);

            // Accumulate gesture until committed
            if (!gestureCommitted)
            {
                totalDeltaX += rampedX;
                totalDeltaY += rampedY;

                double gestureMagSq = totalDeltaX * totalDeltaX + totalDeltaY * totalDeltaY;
                if (gestureMagSq > gestureRadiusSq)
                {
                    gestureCommitted = true;
                    gestureFlushX = totalDeltaX;
                    gestureFlushY = totalDeltaY;

                    velocityHistoryX.Clear();
                    velocityHistoryY.Clear();
                    velocitySumX = velocitySumY = 0;
                }
                else return;
            }

            // Track velocity using rolling sum
            velocityHistoryX.Enqueue(rampedX);
            velocitySumX += rampedX;
            if (velocityHistoryX.Count > velocityWindowSize)
                velocitySumX -= velocityHistoryX.Dequeue();

            velocityHistoryY.Enqueue(rampedY);
            velocitySumY += rampedY;
            if (velocityHistoryY.Count > velocityWindowSize)
                velocitySumY -= velocityHistoryY.Dequeue();

            releaseVelocityX = velocitySumX / velocityHistoryX.Count;
            releaseVelocityY = velocitySumY / velocityHistoryY.Count;

            // Apply gesture flush
            double flushedX = gestureFlushX * gestureFlushRate;
            double flushedY = gestureFlushY * gestureFlushRate;
            gestureFlushX -= flushedX;
            gestureFlushY -= flushedY;

            // Final movement vector
            double finalX = flushedX + rampedX;
            double finalY = flushedY + rampedY;

            MoveBy(finalX, finalY);
        }

        // Checks whether the pads have been dragged enough to trigger a haptic feedback
        public bool HapticDragRFauxLizard(double dx, double dy, bool isTouched)
        {
            if (isTouched)
            {
                hapticClearedR = false;

                // If buffer is full, remove array contributions at this slot
                if (hapticBufferCountR == hapticBufferSize)
                {
                    int idx = hapticBufferIndexR;
                    hapticDxFlipCountR -= hapticDxFlipFlagBufferR[idx];
                    hapticDyFlipCountR -= hapticDyFlipFlagBufferR[idx];
                    if (hapticDxSignBufferR[idx] == 0) hapticDxZeroCountR--;
                    if (hapticDySignBufferR[idx] == 0) hapticDyZeroCountR--;
                    hapticMagSumR -= hapticMagBufferR[idx];
                }
                else
                {
                    hapticBufferCountR++;
                }

                // Compute current signs
                int curDxSign = Math.Sign(dx);
                int curDySign = Math.Sign(dy);

                // Compute flips against previous signs
                int dxFlip = 0, dyFlip = 0;
                if (hapticBufferCountR > 1)
                {
                    int prevIndex = (hapticBufferIndexR - 1 + hapticBufferSize) % hapticBufferSize;
                    int prevDxSign = hapticDxSignBufferR[prevIndex];
                    int prevDySign = hapticDySignBufferR[prevIndex];

                    if (prevDxSign != 0 && curDxSign != 0 && prevDxSign != curDxSign) dxFlip = 1;
                    if (prevDySign != 0 && curDySign != 0 && prevDySign != curDySign) dyFlip = 1;
                }

                // Compute current magnitude
                double mag = Math.Sqrt(dx * dx + dy * dy);

                // Store signs and flip flags and magnitude in array for rolling sums
                int i = hapticBufferIndexR;
                hapticDxSignBufferR[i] = curDxSign;
                hapticDySignBufferR[i] = curDySign;
                hapticDxFlipFlagBufferR[i] = dxFlip;
                hapticDyFlipFlagBufferR[i] = dyFlip;
                hapticMagBufferR[i] = mag;

                // Increment rolling sums
                if (curDxSign == 0) hapticDxZeroCountR++;
                if (curDySign == 0) hapticDyZeroCountR++;
                hapticDxFlipCountR += dxFlip;
                hapticDyFlipCountR += dyFlip;
                hapticMagSumR += mag;

                // Store new buffer index for later
                hapticBufferIndexR++;
                if (hapticBufferIndexR == hapticBufferSize) hapticBufferIndexR = 0;

                // No need to proceed as buffer is not full yet
                if (hapticBufferCountR != hapticBufferSize)
                    return false;

                // Compute average magnitude over buffer for smoothing
                double avgMag = hapticMagSumR / hapticBufferCountR;

                // Calculate a penalty for the magnitude
                double bufferSpan = hapticBufferSize - hapticBufferMidpoint;
                double invSpan = 1.0 / bufferSpan;
                bool dxZeroEdge = hapticDxZeroCountR == hapticBufferCountR;
                bool dyZeroEdge = hapticDyZeroCountR == hapticBufferCountR;
                double factor = (dxZeroEdge || dyZeroEdge) ? 1.0 : 0.5;

                double dxFactor = dxZeroEdge
                    ? 0.0
                    : (hapticDxFlipCountR < hapticBufferMidpoint
                        ? factor
                        : -factor * ((hapticDxFlipCountR - hapticBufferMidpoint) * invSpan));

                double dyFactor = dyZeroEdge
                    ? 0.0
                    : (hapticDyFlipCountR < hapticBufferMidpoint
                        ? factor
                        : -factor * ((hapticDyFlipCountR - hapticBufferMidpoint) * invSpan));

                // Final penalty
                hapticDeltaR += avgMag * (dxFactor + dyFactor);

                if (hapticDeltaR >= hapticTriggerDelta)
                {
                    hapticDeltaR -= hapticTriggerDelta;
                    return true;
                }
                else if (hapticDeltaR < 0)
                {
                    hapticDeltaR = 0;
                }
            }
            else
            {
                if (!hapticClearedR)
                {
                    hapticClearedR = true;

                    // Reset all buffers when not touched
                    hapticDeltaR = 0;
                    hapticBufferCountR = 0;
                    hapticBufferIndexR = 0;
                    hapticDxFlipCountR = 0;
                    hapticDyFlipCountR = 0;
                    hapticDxZeroCountR = 0;
                    hapticDyZeroCountR = 0;
                    hapticMagSumR = 0;
                }
            }

            return false;
        }

        public bool HapticDragLFauxLizard(double dx, double dy, bool isTouched)
        {
            if (isTouched)
            {
                hapticClearedL = false;

                // If buffer is full, remove array contributions at this slot
                if (hapticBufferCountL == hapticBufferSize)
                {
                    int idx = hapticBufferIndexL;
                    hapticDxFlipCountL -= hapticDxFlipFlagBufferL[idx];
                    hapticDyFlipCountL -= hapticDyFlipFlagBufferL[idx];
                    if (hapticDxSignBufferL[idx] == 0) hapticDxZeroCountL--;
                    if (hapticDySignBufferL[idx] == 0) hapticDyZeroCountL--;
                    hapticMagSumL -= hapticMagBufferL[idx];
                }
                else
                {
                    hapticBufferCountL++;
                }

                // Compute current signs
                int curDxSign = Math.Sign(dx);
                int curDySign = Math.Sign(dy);

                // Compute flips against previous signs
                int dxFlip = 0, dyFlip = 0;
                if (hapticBufferCountL > 1)
                {
                    int prevIndex = (hapticBufferIndexL - 1 + hapticBufferSize) % hapticBufferSize;
                    int prevDxSign = hapticDxSignBufferL[prevIndex];
                    int prevDySign = hapticDySignBufferL[prevIndex];

                    if (prevDxSign != 0 && curDxSign != 0 && prevDxSign != curDxSign) dxFlip = 1;
                    if (prevDySign != 0 && curDySign != 0 && prevDySign != curDySign) dyFlip = 1;
                }

                // Compute current magnitude
                double mag = Math.Sqrt(dx * dx + dy * dy);

                // Store signs and flip flags and magnitude in array for rolling sums
                int i = hapticBufferIndexL;
                hapticDxSignBufferL[i] = curDxSign;
                hapticDySignBufferL[i] = curDySign;
                hapticDxFlipFlagBufferL[i] = dxFlip;
                hapticDyFlipFlagBufferL[i] = dyFlip;
                hapticMagBufferL[i] = mag;

                // Increment rolling sums
                if (curDxSign == 0) hapticDxZeroCountL++;
                if (curDySign == 0) hapticDyZeroCountL++;
                hapticDxFlipCountL += dxFlip;
                hapticDyFlipCountL += dyFlip;
                hapticMagSumL += mag;

                // Store new buffer index for later
                hapticBufferIndexL++;
                if (hapticBufferIndexL == hapticBufferSize) hapticBufferIndexL = 0;

                // No need to proceed as buffer is not full yet
                if (hapticBufferCountL != hapticBufferSize)
                    return false;

                // Compute average magnitude over buffer for smoothing
                double avgMag = hapticMagSumL / hapticBufferCountL;

                // Calculate a penalty for the magnitude
                double bufferSpan = hapticBufferSize - hapticBufferMidpoint;
                double invSpan = 1.0 / bufferSpan;
                bool dxZeroEdge = hapticDxZeroCountL == hapticBufferCountL;
                bool dyZeroEdge = hapticDyZeroCountL == hapticBufferCountL;
                double factor = (dxZeroEdge || dyZeroEdge) ? 1.0 : 0.5;

                double dxFactor = dxZeroEdge
                    ? 0.0
                    : (hapticDxFlipCountL < hapticBufferMidpoint
                        ? factor
                        : -factor * ((hapticDxFlipCountL - hapticBufferMidpoint) * invSpan));

                double dyFactor = dyZeroEdge
                    ? 0.0
                    : (hapticDyFlipCountL < hapticBufferMidpoint
                        ? factor
                        : -factor * ((hapticDyFlipCountL - hapticBufferMidpoint) * invSpan));

                // Final penalty
                hapticDeltaL += avgMag * (dxFactor + dyFactor);

                if (hapticDeltaL >= hapticTriggerDelta)
                {
                    hapticDeltaL -= hapticTriggerDelta;
                    return true;
                }
                else if (hapticDeltaL < 0)
                {
                    hapticDeltaL = 0;
                }
            }
            else
            {
                if (!hapticClearedL)
                {
                    hapticClearedL = true;

                    // Reset all buffers when not touched
                    hapticDeltaL = 0;
                    hapticBufferCountL = 0;
                    hapticBufferIndexL = 0;
                    hapticDxFlipCountL = 0;
                    hapticDyFlipCountL = 0;
                    hapticDxZeroCountL = 0;
                    hapticDyZeroCountL = 0;
                    hapticMagSumL = 0;
                }
            }

            return false;
        }

        // Input smoothing
        private double SmoothDelta(double raw, Queue<double> buffer)
        {
            buffer.Enqueue(raw);
            if (buffer.Count > bufferSize) buffer.Dequeue();

            double sum = 0;
            foreach (var v in buffer) sum += v;
            return sum / buffer.Count;
        }

        // Acceleration ramp
        private double ApplyAccelerationRamp(double delta)
        {
            double steepness = 0.1;
            double midpoint = 6.0;
            double maxBoost = 2.0;
            double baseBoost = 1.7;

            double absDelta = Math.Abs(delta);
            double sigmoidBoost = 1.0 + (maxBoost - 1.0) / (1.0 + Math.Exp(-steepness * (absDelta - midpoint)));
            return delta * Math.Max(sigmoidBoost, baseBoost);
        }

        // Glide decay
        private double ApplyReleaseGlide(double velocity, double elapsed)
        {
            double rampedRate = 3.0 + Math.Pow(elapsed * 7.0, 2);
            return velocity * Math.Exp(-elapsed * rampedRate);
        }
    }
}