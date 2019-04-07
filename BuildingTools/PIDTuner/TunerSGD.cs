using BrilliantSkies.Core.Control;
using System;
using UnityEngine;

namespace BuildingTools.PIDTuner
{
    public class TunerSGD : Tuner
    {
        public float TotalError => totalError;
        public bool AutoMode => inAuto;
        public float SampleTime
        {
            get => sampleTime;
            set
            {
                if (value >= 0)
                    sampleTime = value;
            }
        }

        protected float KpLearningRate;
        protected float KiLearningRate;
        protected float KdLearningRate;
        protected float timeSpan;
        protected float lastInput = 0;
        protected float lastError = 0;
        protected float lastDInput = 0;

        protected float lastTime;
        protected float outputSum;

        protected float sampleTime = 100;
        protected float outMin = -1;
        protected float outMax = 1;
        protected bool inAuto = false;
        protected bool PonM = true;
        protected float maxLoss;
        protected float totalErrorBeforeLastErrorPoint = 0;
        protected float totalError = 0;
        protected float lastErrorPoint;
        protected float lastOutput;
        protected bool stopLearning = false;

        protected Func<float> time;
        protected Action<object> log;


        public TunerSGD(PidStandardForm pid, float maxLoss, float learningRate, bool PonM = false,
            Func<float> getTime = null, Action<object> logger = null)
        {
            Pid = pid;
            KpLearningRate = KiLearningRate = KdLearningRate = learningRate;

            time = getTime ?? (() => Time.time * 1000);
            log = logger ?? ((x) => Debug.Log("[Autotune] " + x));

            log("Start");

            this.PonM = PonM;

            lastErrorPoint = time();
            lastTime = time() - sampleTime;
        }

        public override bool Update(float input, float setpoint)
        {
            if (!inAuto)
                return false;

            float now = time();
            float timeChange = now - lastTime;

            if (timeChange >= sampleTime)
            {
                float error = setpoint - input;

                if (Mathf.Abs(error) < maxLoss)
                {
                    Output = lastOutput;
                    ResetTotal(now);
                    return true;
                }

                float dInput = input - lastInput;
                float lastOutputSum = outputSum;

                Ki = CalcSGD(totalError, totalError + error, Ki, KiLearningRate);

                totalError += error;

                if (lastError / error < 0)
                    // just changed to the opposite error, so previous total error doesn't apply
                    ResetTotal(now);
                else
                    outputSum = Ki * timeChange / 1000 * totalError;

                // Add Proportional on Measurement, if P_ON_M is specified
                if (PonM)
                {
                    // TODO: do we need to call CalcSGD on Kp?
                    outputSum -= Kp * input;
                }

                outputSum = Mathf.Clamp(outputSum, outMin, outMax);

                Output = 0;

                // Add Proportional on Error, if P_ON_E is specified
                if (!PonM)
                {
                    lastError = setpoint - lastInput;
                    Kp = CalcSGD(lastError, error, Kp, KpLearningRate);

                    Output = Kp * error;
                }
                else
                    Output = 0;

                // Compute Rest of PID Output
                Kd = CalcSGD(lastDInput, dInput, Kd, KdLearningRate);

                Output += outputSum - Kd / timeChange / 1000 * dInput;

                lastDInput = dInput;

                Output = Mathf.Clamp(Output, outMin, outMax);

                lastOutput = Output;
                lastInput = input;
                lastTime = now;

                return true;
            }

            return false;
        }

        public TunerSGD SetTunings(float Kp, float Ki, float Kd, bool PonM)
        {
            this.PonM = PonM;

            this.Kp = Kp;
            this.Ki = Ki;
            this.Kd = Kd;

            return this;
        }

        public TunerSGD SetTunings(float Kp, float Ki, float Kd) =>
            SetTunings(Kp, Ki, Kd, PonM);

        public TunerSGD SetOutputLimits(float min, float max)
        {
            outMin = min;
            outMax = max;

            Output = Mathf.Clamp(Output, min, max);
            outputSum = Mathf.Clamp(outputSum, min, max);

            return this;
        }

        protected override void Init(float input, float output)
        {
            if (inAuto)
                return;

            inAuto = true;
            outputSum = Mathf.Clamp(output, outMin, outMax);
            lastInput = input;
            lastDInput = input;
        }

        public override void Interrupt()
        {
            inAuto = false;
        }

        protected float CalcSGD(float prevFeedback, float newFeedback, float theta, float learningRate)
        {
            // English refence: page 7 at: https://mycourses.aalto.fi/pluginfile.php/393629/mod_resource/content/1/Lecture8.pdf
            // or, in Chinese: http://blog.csdn.net/lilyth_lilyth/article/details/8973972
            // simplify it as:
            // h(x) = gTheta * x, h(x) is the PID input for next episode
            // where, x is prevFeedback, can be set to anything (like 0.0) at beginning
            // y is newFeedback, which is the actual feedback from sensor (environment)
            // we need to train theta

            if (stopLearning)
                return theta;

            if (Mathf.Abs(theta * prevFeedback - newFeedback) < maxLoss)
                return theta;

            float prevTheta = theta;

            // This is the enhancement part: supposely 
            // 
            //	lDiff = iLearningRate * (iTheta * iPrevFeedback - iNewFeedback) * iPrevFeedback;
            // 
            // should be used to calculate lDiff, from theory in the URLs
            // However, I noticed that if input feedback is large, say, in magnitude of 100,
            // then "(iTheta * iPrevFeedback - iNewFeedback) * iPrevFeedback"
            // could end up with very large number, and make the lDiff becomes larger and
            // does not converge.  Therefore, I used "iTheta * iLearningRate" to guarantee
            // that lDiff will converge
            float lDiff = theta * learningRate;

            // lOrigDiff is used to decide if iTheta should be larger or smaller
            float lOrigDiff = (theta * prevFeedback - newFeedback) * prevFeedback;
            if (lOrigDiff < 0)
            {
                lDiff = -1 * lDiff;
            }

            theta = theta - lDiff;

            return theta;
        }

        protected void ResetTotal(float now)
        {
            outputSum = 0;
            totalError = 0;
            lastErrorPoint = now;
            totalErrorBeforeLastErrorPoint = totalError;
        }
    }
}
