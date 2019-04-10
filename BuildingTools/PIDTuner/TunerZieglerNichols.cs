using BrilliantSkies.Core.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingTools.PIDTuner
{
    public class TunerZieglerNichols : Tuner
    {
        PID_ATune tune;

        public TunerZieglerNichols(PidStandardForm pid)
        {
            Pid = pid;
        }

        protected override void Init(float input, float output)
        {
            tune = new PID_ATune(input, output);
            tune.SetControlType(PID_ATune.AnonymousEnum.ZIEGLER_NICHOLS_PID);
            tune.SetLookbackSec(5);
            tune.SetNoiseBand(0.002);
            tune.SetOutputStep(10);
        }

        public override bool Update(float input, float setpoint, float dt)
        {
            tune.input = input;
            tune.setpoint = setpoint;

            if (!tune.Runtime())
                return false;

            Kc = (float)tune.GetKp();
            Ti = (float)tune.GetKi();
            Td = (float)tune.GetKd();

            Output = (float)tune.output;

            return true;
        }

        public override void Interrupt()
        {
            tune.Cancel();
        }
    }
}
