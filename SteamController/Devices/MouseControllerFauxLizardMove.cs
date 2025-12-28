namespace SteamController.Devices
{
    public partial class MouseController
    {
        // Adjustable parameters (tweakable settings)
        private const int BufferSize = 4;                     // Input smoothing buffer size
        private const int GestureRadius = 30;                 // Gesture commit threshold (pixels)
        private const double GestureFlushRate = 0.1;          // Rate at which gesture flush decays
        private const int VelocityWindowSize = 20;            // Velocity smoothing window size
        private const double MinGlideMagnitude = 0.8;         // Minimum velocity to start glide
        private const double MinGlideVelocity = 0.15;         // Minimum velocity to continue glide

        // Functional constants (derived once)
        private static readonly double GestureRadiusSq = GestureRadius * GestureRadius;
        private static readonly double MinGlideMagnitudeSq = MinGlideMagnitude * MinGlideMagnitude;
        private static readonly double MinGlideVelocitySq = MinGlideVelocity * MinGlideVelocity;

        // Runtime state RPad (movement)
        private readonly double[] bufX = new double[BufferSize];
        private int bufXIndex = 0, bufXCount = 0;
        private double bufXSum = 0;

        private readonly double[] bufY = new double[BufferSize];
        private int bufYIndex = 0, bufYCount = 0;
        private double bufYSum = 0;

        private double totalDeltaX = 0, totalDeltaY = 0;
        private bool gestureCommitted = false;
        private double gestureFlushX = 0, gestureFlushY = 0;

        private bool isGliding = false;
        private bool wasTouched = false;
        private long releaseTicks;
        private double releaseVelocityX = 0, releaseVelocityY = 0;

        private readonly double[] velocityX = new double[VelocityWindowSize];
        private int velocityXIndex = 0, velocityXCount = 0;
        private double velocityXSum = 0;

        private readonly double[] velocityY = new double[VelocityWindowSize];
        private int velocityYIndex = 0, velocityYCount = 0;
        private double velocityYSum = 0;

        // Main movement logic
        public void MoveByFauxLizard(double dx, double dy, bool isTouched)
        {
            if (!isTouched)
            {
                // Start glide if gesture was committed and velocity is high enough
                if (!isGliding && gestureCommitted)
                {
                    double magSq = releaseVelocityX * releaseVelocityX + releaseVelocityY * releaseVelocityY;
                    if (magSq >= MinGlideMagnitudeSq)
                    {
                        releaseTicks = System.Diagnostics.Stopwatch.GetTimestamp();
                        isGliding = true;
                    }
                }

                // Continue glide if active
                if (isGliding)
                {
                    double elapsed =
                        (System.Diagnostics.Stopwatch.GetTimestamp() - releaseTicks)
                        / (double)System.Diagnostics.Stopwatch.Frequency;
                    double glideX = ApplyReleaseGlide(releaseVelocityX, elapsed);
                    double glideY = ApplyReleaseGlide(releaseVelocityY, elapsed);

                    double glideMagSq = glideX * glideX + glideY * glideY;
                    if (glideMagSq < MinGlideVelocitySq)
                    {
                        isGliding = false;
                        releaseVelocityX = releaseVelocityY = 0;
                    }
                    else
                    {
                        MoveBy(glideX, glideY);
                    }
                }

                // Reset gesture state ONCE on touch release
                if (wasTouched)
                {
                    ResetFauxLizardGesture();
                    wasTouched = false;
                }

                return;
            }

            // Touch just started
            wasTouched = true;
            isGliding = false;

            // Smooth input deltas
            double smoothedX = SmoothRing(dx, bufX, ref bufXIndex, ref bufXCount, ref bufXSum);
            double smoothedY = SmoothRing(dy, bufY, ref bufYIndex, ref bufYCount, ref bufYSum);

            // Apply acceleration ramp
            double rampedX = ApplyAccelerationRamp(smoothedX);
            double rampedY = ApplyAccelerationRamp(smoothedY);

            // Accumulate gesture until committed
            if (!gestureCommitted)
            {
                totalDeltaX += rampedX;
                totalDeltaY += rampedY;

                double gestureMagSq = totalDeltaX * totalDeltaX + totalDeltaY * totalDeltaY;
                if (gestureMagSq > GestureRadiusSq)
                {
                    gestureCommitted = true;
                    gestureFlushX = totalDeltaX;
                    gestureFlushY = totalDeltaY;

                    velocityXIndex = velocityXCount = 0; velocityXSum = 0;
                    velocityYIndex = velocityYCount = 0; velocityYSum = 0;
                }
                else return;
            }

            // Track velocity using rolling sum
            releaseVelocityX = SmoothRing(rampedX, velocityX, ref velocityXIndex, ref velocityXCount, ref velocityXSum);
            releaseVelocityY = SmoothRing(rampedY, velocityY, ref velocityYIndex, ref velocityYCount, ref velocityYSum);

            // Apply gesture flush
            double flushedX = gestureFlushX * GestureFlushRate;
            double flushedY = gestureFlushY * GestureFlushRate;
            gestureFlushX -= flushedX;
            gestureFlushY -= flushedY;

            // Final movement vector
            double finalX = flushedX + rampedX;
            double finalY = flushedY + rampedY;

            MoveBy(finalX, finalY);
        }

        // Buffer smoothing helper
        private static double SmoothRing(double v, double[] buffer, ref int index, ref int count, ref double sum)
        {
            int window = buffer.Length;

            if (count < window)
            {
                buffer[index] = v;
                sum += v;
                count++;
            }
            else
            {
                sum -= buffer[index];
                buffer[index] = v;
                sum += v;
            }

            index++;
            if (index == window)
                index = 0;

            return sum / count;
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

        //Resets all values for a new touch
        private void ResetFauxLizardGesture()
        {
            gestureCommitted = false;

            bufXIndex = bufXCount = 0; bufXSum = 0;
            bufYIndex = bufYCount = 0; bufYSum = 0;

            velocityXIndex = velocityXCount = 0; velocityXSum = 0;
            velocityYIndex = velocityYCount = 0; velocityYSum = 0;

            totalDeltaX = totalDeltaY = 0;
        }
    }
}