using BrilliantSkies.Core.Control;

namespace BuildingTools.PIDTuner
{
    public abstract class Tuner
    {
        // https://knowledge.ni.com/KnowledgeArticleDetails?id=kA00Z0000019L5TSAU
        //
        // Ideal PID (PidStandardForm)
        //   Kc (error + integral / Ti + Td derivative)
        //
        // Parallel PID
        //   Kp error + Ki integral + Kd derivative
        public PidStandardForm Pid { get; set; }

        public float Kc
        {
            get => Pid.kP;
            set => Pid.kP.Us = value;
        }
        public float Ti
        {
            get => Pid.kI;
            set => Pid.kI.Us = value;
        }
        public float Td
        {
            get => Pid.kD;
            set => Pid.kD.Us = value;
        }
        public float Kp
        {
            get => Kc;
            set
            {
                Td *= Kc / value;
                Ti *= value / Kc;
                Kc = value;
            }
        }
        public float Ki
        {
            get => Kc / Ti;
            set => Ti = Kc / value;
        }
        public float Kd
        {
            get => Kc * Td;
            set => Td = value / Kc;
        }
        public virtual float Output { get; set; }
        public virtual bool Ended { get; protected set; }
        public bool Initialized { get; private set; }

        public bool Initialize (float input, float output)
        {
            if (Initialized) return false;

            Initialized = true;
            Init(input, output);

            return true;
        }

        protected virtual void Init(float input, float output) { }

        public abstract bool Update(float input, float setpoint);

        public virtual void Interrupt() { }
    }
}
