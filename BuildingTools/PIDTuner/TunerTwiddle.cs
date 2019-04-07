using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BuildingTools.PIDTuner
{
    public class TunerTwiddle : Tuner
    {
        private float[] p = new[] { 0f, 0f, 0f };
        private float[] dp = new[] { 1f, 1f, 1f };

        private float sampleTime = 0.1f;
        private float lastTime = 0;

        private float threshold = 0.001f;

        private float best_err;
        private float err;
        private float last_err;
        private bool firstRun = true;

        private IEnumerator twiddle;

        private float integral = 0;

        protected override void Init(float input, float output)
        {
            twiddle = Twiddle();
        }

        public override bool Update(float input, float setpoint)
        {
            float now = Time.time;
            float dt = now - lastTime;
            if (dt < sampleTime)
                return false;
            lastTime = now;

            err = setpoint - input;
            if (firstRun)
            {
                best_err = err;
                last_err = err;
                firstRun = false;
            }
            
            twiddle.MoveNext();

            integral = Mathf.Clamp(
                integral + (p[1] * err * dt),
                Pid.LowerOutputLimit, Pid.UpperOutputLimit);

            Output = (p[0] * err) + integral + (p[2] * (err - last_err) / dt);

            Kp = p[0];
            Ki = p[1];
            Kd = p[2];

            last_err = err;

            return true;
        }

        public IEnumerator Twiddle()
        {
            while (dp.Sum() > threshold)
            {
                for (int i = 0; i < p.Length; i++)
                {
                    p[i] += dp[i];
                    yield return true;

                    if (err < best_err)
                    {
                        best_err = err;
                        dp[i] *= 1.05f;
                    }
                    else
                    {
                        p[i] -= 2 * dp[i];
                        yield return true;

                        if (err < best_err)
                        {
                            best_err = err;
                            dp[i] *= 1.05f;
                        }
                        else
                        {
                            p[i] += dp[i];
                            dp[i] *= 0.95f;
                        }
                    }
                }
            }
            Ended = true;
            yield break;
        }
    }
}
