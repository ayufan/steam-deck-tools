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
        private const double hapticTriggerDelta = 6000;       // Distance required for haptic feedback on each pad
        private const double hapticTriggerThreshold = 600;    // Only triggers haptic if moved this much, prevents triggering on minor drifts

        // Functional constants (derived once)
        private readonly double gestureRadiusSq = gestureRadius * gestureRadius;
        private readonly double minGlideMagnitudeSq = minGlideMagnitude * minGlideMagnitude;
        private readonly double minGlideVelocitySq = minGlideVelocity * minGlideVelocity;
        private readonly double hapticTriggerThresholdSq = hapticTriggerThreshold * hapticTriggerThreshold;

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
        private int hapticDriftBufferSize = 250;

        private double hapticDeltaR = 0;
        double[] hapticDriftRX = new double[250];
        double[] hapticDriftRY = new double[250];
        int hapticDriftRIndex = 0;
        double hapticDriftSumRX = 0;
        double hapticDriftSumRY = 0;

        private double hapticDeltaL = 0;
        double[] hapticDriftLX = new double[250];
        double[] hapticDriftLY = new double[250];
        int hapticDriftLIndex = 0;
        double hapticDriftSumLX = 0;
        double hapticDriftSumLY = 0;

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
                // Update circular buffer and drift sums
                hapticDriftSumRX -= hapticDriftRX[hapticDriftRIndex];
                hapticDriftSumRY -= hapticDriftRY[hapticDriftRIndex];

                hapticDriftRX[hapticDriftRIndex] = dx;
                hapticDriftRY[hapticDriftRIndex] = dy;

                hapticDriftSumRX += dx;
                hapticDriftSumRY += dy;

                hapticDriftRIndex = (hapticDriftRIndex + 1) % hapticDriftBufferSize;

                // Compute drift magnitude squared
                double driftMagSq = hapticDriftSumRX * hapticDriftSumRX + hapticDriftSumRY * hapticDriftSumRY;

                if (driftMagSq > hapticTriggerThresholdSq)
                {
                    // Compute instantaneous movement magnitude
                    double deltaMagSq = dx * dx + dy * dy;
                    if(hapticDeltaR < hapticTriggerDelta)
                        hapticDeltaR += Math.Sqrt(deltaMagSq);

                    if (hapticDeltaR >= hapticTriggerDelta)
                    {
                        // Reset drift buffer and trigger haptic
                        hapticDriftSumRX = hapticDriftSumRY = 0;
                        hapticDriftRIndex = 0;
                        Array.Clear(hapticDriftRX, 0, hapticDriftBufferSize);
                        Array.Clear(hapticDriftRY, 0, hapticDriftBufferSize);

                        hapticDeltaR -= hapticTriggerDelta;

                        return true;
                    }
                }
            }
            else
            {
                // Reset everything on release
                hapticDriftSumRX = hapticDriftSumRY = 0;
                hapticDriftRIndex = 0;
                Array.Clear(hapticDriftRX, 0, hapticDriftBufferSize);
                Array.Clear(hapticDriftRY, 0, hapticDriftBufferSize);

                hapticDeltaR = 0;
            }

            return false;
        }

        public bool HapticDragLFauxLizard(double dx, double dy, bool isTouched)
        {
            if (isTouched)
            {
                // Update circular buffer and drift sums
                hapticDriftSumLX -= hapticDriftLX[hapticDriftLIndex];
                hapticDriftSumLY -= hapticDriftLY[hapticDriftLIndex];

                hapticDriftLX[hapticDriftLIndex] = dx;
                hapticDriftLY[hapticDriftLIndex] = dy;

                hapticDriftSumLX += dx;
                hapticDriftSumLY += dy;

                hapticDriftLIndex = (hapticDriftLIndex + 1) % hapticDriftBufferSize;

                // Compute drift magnitude squared
                double driftMagSq = hapticDriftSumLX * hapticDriftSumLX + hapticDriftSumLY * hapticDriftSumLY;

                if (driftMagSq > hapticTriggerThresholdSq)
                {
                    // Compute instantaneous movement magnitude
                    double deltaMagSq = dx * dx + dy * dy;
                    if (hapticDeltaL < hapticTriggerDelta)
                        hapticDeltaL += Math.Sqrt(deltaMagSq);

                    if (hapticDeltaL >= hapticTriggerDelta)
                    {
                        // Reset drift buffer and trigger haptic
                        hapticDriftSumLX = hapticDriftSumLY = 0;
                        hapticDriftLIndex = 0;
                        Array.Clear(hapticDriftLX, 0, hapticDriftBufferSize);
                        Array.Clear(hapticDriftLY, 0, hapticDriftBufferSize);

                        hapticDeltaL -= hapticTriggerDelta;

                        return true;
                    }
                }
            }
            else
            {
                // Reset everything on release
                hapticDriftSumLX = hapticDriftSumLY = 0;
                hapticDriftLIndex = 0;
                Array.Clear(hapticDriftLX, 0, hapticDriftBufferSize);
                Array.Clear(hapticDriftLY, 0, hapticDriftBufferSize);

                hapticDeltaL = 0;
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