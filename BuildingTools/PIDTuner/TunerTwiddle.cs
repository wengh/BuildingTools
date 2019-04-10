using BrilliantSkies.Core.Control;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace BuildingTools.PIDTuner
{
    public class TunerTwiddle : Tuner
    {
        private float[] p = new[] { 0f, 250f, 0f };
        private float[] dp = new[] { 0.01f, 0.5f, 0.05f };

        private float sampleTime = 0f;
        private float lastTime = 0;

        private float threshold = 0.005f;

        private float bestErr;
        private float err;
        private bool firstRun = true;

        private IEnumerator twiddle;

        public TunerTwiddle(PidStandardForm pid)
        {
            Pid = pid;
        }

        protected override void Init(float input, float output)
        {
            p[0] = Kc;
            p[1] = Ti;
            p[2] = Td;
            twiddle = Twiddle();
        }

        public override bool Update(float input, float setpoint, float dt)
        {
            err = setpoint - input;
            if (firstRun)
            {
                bestErr = err;
                firstRun = false;
            }

            float now = Time.time;
            if (now - lastTime > sampleTime)
            {
                lastTime = now;
                twiddle.MoveNext();
                Debug.Log($"PID parameters: {p[0]} {p[1]} {p[2]}");
                Kc = p[0];
                Ti = p[1];
                Td = p[2];
            }
            return false;
        }

        public override void Interrupt()
        {
            Kc = p[0];
            Ti = p[1];
            Td = p[2];
        }

        public IEnumerator Twiddle()
        {
            while (dp.Sum() > threshold)
            {
                for (int i = 0; i < p.Length; i++)
                {
                    p[i] += dp[i];
                    yield return true;

                    if (Mathf.Abs(err) < bestErr)
                    {
                        bestErr = Mathf.Abs(err);
                        dp[i] *= 1.05f;
                    }
                    else
                    {
                        p[i] -= 2 * dp[i];
                        yield return true;

                        if (Mathf.Abs(err) < bestErr)
                        {
                            bestErr = Mathf.Abs(err);
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
