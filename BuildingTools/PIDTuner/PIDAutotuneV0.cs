namespace BuildingTools.PIDTuner
{
    using BrilliantSkies.Common.Circuits.ComponentTypes;
    using System;
    using UnityEngine;

    // defining this option implements relay bias
    // this is useful to adjust the relay output values
    // during the auto tuning to recover symmetric
    // oscillations 
    // this can compensate for load disturbance
    // and equivalent signals arising from nonlinear
    // or non-stationary processes 
    // any improvement in the tunings seems quite modest 
    // but sometimes unbalanced oscillations can be 
    // persuaded to converge where they might not 
    // otherwise have done so

    // average amplitude of successive peaks must differ by no more than this proportion
    // relative to half the difference between maximum and minimum of last 2 cycles

    // ratio of up/down relay step duration should differ by no more than this tolerance
    // biasing the relay con give more accurate estimates of the tuning parameters but
    // setting the tolerance too low will prolong the autotune procedure unnecessarily
    // this parameter also sets the minimum bias in the relay as a proportion of its amplitude

    // auto tune terminates if waiting too long between peaks or relay steps
    // set larger value for processes with long delays or time constants

    // Ziegler-Nichols type auto tune rules
    // in tabular form
    public class Tuning
    {
        public byte[] _divisor = Arrays.InitializeWithDefaultInstances<byte>(3);

        public Tuning(byte a, byte b, byte c)
        {
            _divisor[0] = a;
            _divisor[1] = b;
            _divisor[2] = c;
        }

        public bool PI_controller()
        {
            return _divisor[2] == 0;
        }

        public double divisor(byte index)
        {
            return _divisor[index] * 0.05;
        }
    }

    public class PID_ATune
    {


        // constants ***********************************************************************************

        // auto tune method
        //C++ TO C# CONVERTER CRACKED BY X-CRACKER 2017 NOTE: Enums must be named in C#, so the following enum has been named AnonymousEnum:
        public enum AnonymousEnum
        {
            ZIEGLER_NICHOLS_PI = 0,
            ZIEGLER_NICHOLS_PID = 1,
            TYREUS_LUYBEN_PI,
            TYREUS_LUYBEN_PID,
            CIANCONE_MARLIN_PI,
            CIANCONE_MARLIN_PID,
            AMIGOF_PI,
            PESSEN_INTEGRAL_PID,
            SOME_OVERSHOOT_PID,
            NO_OVERSHOOT_PID
        }

        // peak type
        public enum Peak
        {
            MINIMUM = -1,
            NOT_A_PEAK = 0,
            MAXIMUM = 1
        }

        // auto tuner state
        public enum AutoTunerState
        {
            AUTOTUNER_OFF = 0,
            STEADY_STATE_AT_BASELINE = 1,
            STEADY_STATE_AFTER_STEP_UP = 2,
            RELAY_STEP_UP = 4,
            RELAY_STEP_DOWN = 8,
            CONVERGED = 16,
            FAILED = 128
        }

        // tuning rule divisor
        //C++ TO C# CONVERTER CRACKED BY X-CRACKER 2017 NOTE: Enums must be named in C#, so the following enum has been named AnonymousEnum2:
        public enum AnonymousEnum2
        {
            KP_DIVISOR = 0,
            TI_DIVISOR = 1,
            TD_DIVISOR = 2
        }

        // irrational constants
        public const double CONST_PI = 3.14159265358979323846;
        public const double CONST_SQRT2_DIV_2 = 0.70710678118654752440;

        // commonly used methods ***********************************************************************
        public PID_ATune(double Input, double Output)
        {
            input = Input;
            output = Output;

            // constructor defaults
            controlType = (byte)AnonymousEnum.ZIEGLER_NICHOLS_PI;
            noiseBand = 0.5;
            state = AutoTunerState.AUTOTUNER_OFF;
            oStep = 10.0;
            SetLookbackSec(10);
        }
        public bool Runtime()
        {
            // check ready for new input
            uint now = (uint)(Time.time * 1000);

            if (state == AutoTunerState.AUTOTUNER_OFF)
            {
                // initialize working variables the first time around
                peakType = Peak.NOT_A_PEAK;
                inputCount = 0;
                peakCount = 0;
                setpoint = input;
                outputStart = output;
                lastPeakTime[0] = now;
                workingNoiseBand = noiseBand;
                newWorkingNoiseBand = noiseBand;
                workingOstep = oStep;

#if AUTOTUNE_RELAY_BIAS
      relayBias = 0.0;
      stepCount = 0;
      lastStepTime[0] = now;
      sumInputSinceLastStep[0] = 0.0;
#endif

                // move to new state
                if (controlType == (byte)AnonymousEnum.AMIGOF_PI)
                {
                    state = AutoTunerState.STEADY_STATE_AT_BASELINE;
                }
                else
                {
                    state = AutoTunerState.RELAY_STEP_UP;
                }
            }

            // otherwise check ready for new input
            else if ((now - lastTime) < sampleTime)
            {
                return false;
            }

            // get new input
            lastTime = now;
            double refVal = input;

#if AUTOTUNE_RELAY_BIAS
    // used to calculate relay bias
    sumInputSinceLastStep[0] += refVal;
#endif

            // local flag variable
            bool justChanged = false;

            // check input and change relay state if necessary
            if ((state == AutoTunerState.RELAY_STEP_UP) && (refVal > setpoint + workingNoiseBand))
            {
                state = AutoTunerState.RELAY_STEP_DOWN;
                justChanged = true;
            }
            else if ((state == AutoTunerState.RELAY_STEP_DOWN) && (refVal < setpoint - workingNoiseBand))
            {
                state = AutoTunerState.RELAY_STEP_UP;
                justChanged = true;
            }
            if (justChanged)
            {
                workingNoiseBand = newWorkingNoiseBand;

#if AUTOTUNE_RELAY_BIAS
      // check symmetry of oscillation
      // and introduce relay bias if necessary
      if (stepCount > 4)
      {
        double avgStep1 = 0.5 * (double)((lastStepTime[0] - lastStepTime[1]) + (lastStepTime[2] - lastStepTime[3]));
        double avgStep2 = 0.5 * (double)((lastStepTime[1] - lastStepTime[2]) + (lastStepTime[3] - lastStepTime[4]));
        if ((avgStep1 > 1e-10) && (avgStep2 > 1e-10))
        {
          double asymmetry = (avgStep1 > avgStep2) ? (avgStep1 - avgStep2) / avgStep1 : (avgStep2 - avgStep1) / avgStep2;

          if (asymmetry > DefineConstants.AUTOTUNE_STEP_ASYMMETRY_TOLERANCE)
          {
            // relay steps are asymmetric
            // calculate relay bias using
            // "Autotuning of PID Controllers: A Relay Feedback Approach",
            //  by Cheng-Ching Yu, 2nd Edition, equation 7.39, p. 148

            // calculate change in relay bias
            double deltaRelayBias = - processValueOffset(avgStep1, avgStep2) * workingOstep;
            if (state == AutoTunerState.RELAY_STEP_DOWN)
            {
              deltaRelayBias = -deltaRelayBias;
            }

            if (Math.Abs(deltaRelayBias) > workingOstep * DefineConstants.AUTOTUNE_STEP_ASYMMETRY_TOLERANCE)
            {
              // change is large enough to bother with
              relayBias += deltaRelayBias;

              /*
              // adjust step height with respect to output limits
              // commented out because the auto tuner does not
              // necessarily know what the output limits are
              double relayHigh = outputStart + workingOstep + relayBias;
              double relayLow  = outputStart - workingOstep + relayBias;
              if (relayHigh > outMax)
              {
                relayHigh = outMax;
              }
              if (relayLow  < outMin)
              {
                relayHigh = outMin;
              }
              workingOstep = 0.5 * (relayHigh - relayLow);
              relayBias = relayHigh - outputStart - workingOstep;
              */

              // reset relay step counter
              // to give the process value oscillation
              // time to settle with the new relay bias value
              stepCount = 0;
            }
          }
        }
      }

      // shift step time and integrated process value arrays
      for (byte i = (stepCount > 4 ? 4 : stepCount); i > 0; i--)
      {
        lastStepTime[i] = lastStepTime[i - 1];
        sumInputSinceLastStep[i] = sumInputSinceLastStep[i - 1];
      }
      stepCount++;
      lastStepTime[0] = now;
      sumInputSinceLastStep[0] = 0.0;

#endif

            } // if justChanged

            // set output
            // FIXME need to respect output limits
            // not knowing output limits is one reason
            // to pass entire PID object to autotune method(s)
            if (((byte)(int)state & ((int)AutoTunerState.STEADY_STATE_AFTER_STEP_UP | (int)AutoTunerState.RELAY_STEP_UP)) > 0)
            {

#if AUTOTUNE_RELAY_BIAS
      output = outputStart + workingOstep + relayBias;
#else
                output = outputStart + workingOstep;
#endif

            }
            else if (state == AutoTunerState.RELAY_STEP_DOWN)
            {

#if AUTOTUNE_RELAY_BIAS
      output = outputStart - workingOstep + relayBias;
#else
                output = outputStart - workingOstep;
#endif

            }

            // store initial inputs
            // we don't want to trust the maxes or mins
            // until the input array is full
            inputCount++;
            if (inputCount <= nLookBack)
            {
                Debug.Log("inputCount <= nLookBack");
                Debug.Log(inputCount);
                Debug.Log(nLookBack);
                lastInputs[nLookBack - inputCount] = refVal;
                return false;
            }

            // shift array of process values and identify peaks
            inputCount = nLookBack;
            bool isMax = true;
            bool isMin = true;
            for (int i = inputCount - 1; i >= 0; i--)
            {
                double val = lastInputs[i];
                if (isMax)
                {
                    isMax = (refVal >= val);
                }
                if (isMin)
                {
                    isMin = (refVal <= val);
                }
                lastInputs[i + 1] = val;
            }
            lastInputs[0] = refVal;

            // for AMIGOf tuning rule, perform an initial
            // step change to calculate process gain K_process
            // this may be very slow for lag-dominated processes
            // and may never terminate for integrating processes
            if (((byte)(int)state & ((int)AutoTunerState.STEADY_STATE_AT_BASELINE | (int)AutoTunerState.STEADY_STATE_AFTER_STEP_UP)) > 0)
            {
                // check that all the recent inputs are
                // equal give or take expected noise
                double iMax = lastInputs[0];
                double iMin = lastInputs[0];
                double avgInput = 0.0;
                for (byte i = 0; i <= inputCount; i++)
                {
                    double val = lastInputs[i];
                    if (iMax < val)
                    {
                        iMax = val;
                    }
                    if (iMin > val)
                    {
                        iMin = val;
                    }
                    avgInput += val;
                }
                avgInput /= (double)(inputCount + 1);

                // if recent inputs are stable
                if ((iMax - iMin) <= 2.0 * workingNoiseBand)
                {
                    Debug.Log("(iMax - iMin) <= 2.0 * workingNoiseBand");
                    Debug.Log(iMax - iMin);
                    Debug.Log(2.0 * workingNoiseBand);

#if AUTOTUNE_RELAY_BIAS
        lastStepTime[0] = now;
#endif

                    if (state == AutoTunerState.STEADY_STATE_AT_BASELINE)
                    {
                        state = AutoTunerState.STEADY_STATE_AFTER_STEP_UP;
                        lastPeaks[0] = avgInput;
                        inputCount = 0;
                        return false;
                    }
                    // else state == STEADY_STATE_AFTER_STEP_UP
                    // calculate process gain
                    K_process = (avgInput - lastPeaks[0]) / workingOstep;

                    // bad estimate of process gain
                    if (K_process < 1e-10) // zero
                    {
                        state = AutoTunerState.AUTOTUNER_OFF;
                        return false;
                    }
                    state = AutoTunerState.RELAY_STEP_DOWN;

#if AUTOTUNE_RELAY_BIAS
        sumInputSinceLastStep[0] = 0.0;
#endif

                    return false;
                }
                else
                {
                    return false;
                }
            }

            // increment peak count
            // and record peak time
            // for both maxima and minima
            justChanged = false;
            if (isMax)
            {
                if (peakType == Peak.MINIMUM)
                {
                    justChanged = true;
                }
                peakType = Peak.MAXIMUM;
            }
            else if (isMin)
            {
                if (peakType == Peak.MAXIMUM)
                {
                    justChanged = true;
                }
                peakType = Peak.MINIMUM;
            }

            // update peak times and values
            if (justChanged)
            {
                peakCount++;

                // shift peak time and peak value arrays
                for (int i = peakCount > 4 ? 4 : peakCount; i > 0; i--)
                {
                    lastPeakTime[i] = lastPeakTime[i - 1];
                    lastPeaks[i] = lastPeaks[i - 1];
                }
            }
            if (isMax || isMin)
            {
                lastPeakTime[0] = now;
                lastPeaks[0] = refVal;
            }

            // check for convergence of induced oscillation
            // convergence of amplitude assessed on last 4 peaks (1.5 cycles)
            double inducedAmplitude = 0.0;
            double phaseLag;
            //C++ TO C# CONVERTER CRACKED BY X-CRACKER 2017 TODO TASK: Statements that are interrupted by preprocessor statements are not converted by C++ to C# Converter Cracked By X-Cracker 2017:
            if (justChanged && (peakCount > 4))

            {

#if AUTOTUNE_RELAY_BIAS
  //C++ TO C# CONVERTER CRACKED BY X-CRACKER 2017 TODO TASK: Statements that are interrupted by preprocessor statements are not converted by C++ to C# Converter Cracked By X-Cracker 2017:
      (stepCount > 4) &&
#endif


                {
                    double absMax = lastPeaks[1];
                    double absMin = lastPeaks[1];
                    for (byte i = 2; i <= 4; i++)
                    {
                        double val = lastPeaks[i];
                        inducedAmplitude += Math.Abs(val - lastPeaks[i - 1]);
                        if (absMax < val)
                        {
                            absMax = val;
                        }
                        if (absMin > val)
                        {
                            absMin = val;
                        }
                    }
                    inducedAmplitude /= 6.0;

                    // source for AMIGOf PI auto tuning method:
                    // "Revisiting the Ziegler-Nichols tuning rules for PI control —
                    //  Part II. The frequency response method."
                    // T. Hägglund and K. J. Åström
                    // Asian Journal of Control, Vol. 6, No. 4, pp. 469-482, December 2004
                    // http://www.ajc.org.tw/pages/paper/6.4PD/AC0604-P469-FR0371.pdf
                    if (controlType == (byte)AnonymousEnum.AMIGOF_PI)
                    {
                        phaseLag = calculatePhaseLag(inducedAmplitude);

                        // check that phase lag is within acceptable bounds, ideally between 120° and 140°
                        // but 115° to 145° will just about do, and might converge quicker
                        if (Math.Abs(phaseLag - CONST_PI * 130.0 / 180.0) > (CONST_PI * 15.0 / 180.0))
                        {
                            // phase lag outside the desired range
                            // set noiseBand to new estimate
                            // aiming for 135° = 0.75 * pi (radians)
                            // sin(135°) = sqrt(2)/2
                            // NB noiseBand = 0.5 * hysteresis
                            newWorkingNoiseBand = 0.5 * inducedAmplitude * CONST_SQRT2_DIV_2;


                            Debug.Log("Math.Abs(phaseLag - CONST_PI * 130.0 / 180.0) > (CONST_PI * 15.0 / 180.0)");
                            Debug.Log(Math.Abs(phaseLag - CONST_PI * 130.0 / 180.0));
                            Debug.Log(CONST_PI * 15.0 / 180.0);

#if AUTOTUNE_RELAY_BIAS
          // we could reset relay step counter because we can't rely
          // on constant phase lag for calculating
          // relay bias having changed noiseBand
          // but this would essentially preclude using relay bias
          // with AMIGOf tuning, which is already a compile option
          /*
          stepCount = 0;
          */
#endif

                            return false;
                        }
                    }

                    // check convergence criterion for amplitude of induced oscillation
                    if (((0.5 * (absMax - absMin) - inducedAmplitude) / inducedAmplitude) < DefineConstants.AUTOTUNE_PEAK_AMPLITUDE_TOLERANCE)
                    {
                        state = AutoTunerState.CONVERGED;
                    }
                }
            }

            // if the autotune has not already converged
            // terminate after 10 cycles
            // or if too long between peaks
            // or if too long between relay steps
            //C++ TO C# CONVERTER CRACKED BY X-CRACKER 2017 TODO TASK: Statements that are interrupted by preprocessor statements are not converted by C++ to C# Converter Cracked By X-Cracker 2017:
            if (((now - lastPeakTime[0]) > (uint)(DefineConstants.AUTOTUNE_MAX_WAIT_MINUTES * 60000)) || (peakCount >= 20))

            {

#if AUTOTUNE_RELAY_BIAS
  //C++ TO C# CONVERTER CRACKED BY X-CRACKER 2017 TODO TASK: Statements that are interrupted by preprocessor statements are not converted by C++ to C# Converter Cracked By X-Cracker 2017:
      || ((now - lastStepTime[0]) > (uint)(DefineConstants.AUTOTUNE_MAX_WAIT_MINUTES * 60000))
#endif


                {
                    state = AutoTunerState.FAILED;
                }
            }

            if (((byte)(int)state & ((int)AutoTunerState.CONVERGED | (int)AutoTunerState.FAILED)) == 0)
            {
                Debug.Log("Converged or failed");
                Debug.Log(state);
                return false;
            }

            // autotune algorithm has terminated
            // reset autotuner variables
            output = outputStart;

            if (state == AutoTunerState.FAILED)
            {
                // do not calculate gain parameters

                return true;
            }

            // finish up by calculating tuning parameters

            // calculate ultimate gain
            double Ku = 4.0 * workingOstep / (inducedAmplitude * CONST_PI);

            // calculate ultimate period in seconds
            double Pu = (double)0.5 * ((lastPeakTime[1] - lastPeakTime[3]) + (lastPeakTime[2] - lastPeakTime[4])) / 1000.0;

            // calculate gain parameters using tuning rules
            // NB PID generally outperforms PI for lag-dominated processes

            // AMIGOf is slow to tune, especially for lag-dominated processes, because it
            // requires an estimate of the process gain which is implemented in this
            // routine by steady state change in process variable after step change in set point
            // It is intended to give robust tunings for both lag- and delay- dominated processes
            if (controlType == (byte)AnonymousEnum.AMIGOF_PI)
            {
                // calculate gain ratio
                double kappa_phi = (1.0 / Ku) / K_process;

                // calculate phase lag
                phaseLag = calculatePhaseLag(inducedAmplitude);

                // calculate tunings
                Kp = ((2.50 - 0.92 * phaseLag) / (1.0 + (10.75 - 4.01 * phaseLag) * kappa_phi)) * Ku;
                Ti = ((-3.05 + 1.72 * phaseLag) / Math.Pow(1.0 + (-6.10 + 3.44 * phaseLag) * kappa_phi, 2)) * Pu;
                Td = 0.0;

                // converged
                return true;
            }

            Kp = Ku / (double)GlobalMembers.tuningRule[controlType].divisor((byte)AnonymousEnum2.KP_DIVISOR);
            Ti = Pu / (double)GlobalMembers.tuningRule[controlType].divisor((byte)AnonymousEnum2.TI_DIVISOR);
            Td = GlobalMembers.tuningRule[controlType].PI_controller() ? 0.0 : Pu / (double)GlobalMembers.tuningRule[controlType].divisor((byte)AnonymousEnum2.TD_DIVISOR);

            // converged
            return true;
        }
        //   returns true when done, otherwise returns false
        public void Cancel()
        {
            state = AutoTunerState.AUTOTUNER_OFF;
        }

        public void SetOutputStep(double Step)
        {
            oStep = Step;
        }
        //   the output step?   
        public double GetOutputStep()
        {
            return oStep;
        }

        public void SetControlType(AnonymousEnum type)
        {
            controlType = (byte)type;
        }
        public byte GetControlType()
        {
            return controlType;
        }

        public void SetLookbackSec(int value)
        {
            if (value < 1)
            {
                value = 1;
            }
            if (value < 25)
            {
                nLookBack = (byte)(value * 4);
                sampleTime = 250;
            }
            else
            {
                nLookBack = 100;
                sampleTime = (byte)(value * 10);
            }
        }
        public int GetLookbackSec()
        {
            return (int)(nLookBack * sampleTime / 1000.0);
        }

        public void SetNoiseBand(double band)
        {
            noiseBand = band;
        }
        //   than this value
        public double GetNoiseBand()
        {
            return noiseBand;
        }

        public double GetKp()
        {
            return Kp;
        }
        public double GetKi()
        {
            return Kp / Ti;
        }
        public double GetKd()
        {
            return Kp * Td;
        }



#if AUTOTUNE_RELAY_BIAS
  private double processValueOffset(double avgStep1, double avgStep2)
  {
    // calculate offset of oscillation in process value
    // as a proportion of the amplitude
    // approximation assumes a trapezoidal oscillation
    // that is stationary over the last 2 relay cycles
    // needs constant phase lag, so recent changes to noiseBand are bad

    if (avgStep1 < 1e-10)
    {
      return 1.0;
    }
    if (avgStep2 < 1e-10)
    {
      return -1.0;
    }
    // ratio of step durations
    double r1 = avgStep1 / avgStep2;

    double s1 = (sumInputSinceLastStep[1] + sumInputSinceLastStep[3]);
    double s2 = (sumInputSinceLastStep[2] + sumInputSinceLastStep[4]);
    if (s1 < 1e-10)
    {
      return 1.0;
    }
    if (s2 < 1e-10)
    {
      return -1.0;
    }
    // ratio of integrated process values
    double r2 = s1 / s2;

    // estimate process value offset assuming a trapezoidal response curve
    //
    // assume trapezoidal wave with amplitude a, cycle period t, time at minimum/maximum m * t (0 <= m <= 1)
    //
    // with no offset:
    // area under half wave of process value given by
    //   a * m * t/2 + a/2 * (1 - m) * t/2 = a * (1 + m) * t / 4
    //
    // now with offset d * a (-1 <= d <= 1):
    // step time of relay half-cycle given by
    //   m * t/2 + (1 - d) * (1 - m) * t/2 = (1 - d + d * m) * t/2
    //
    // => ratio of step times in cycle given by:
    // (1) r1 = (1 - d + d * m) / (1 + d - d * m)
    //
    // area under offset half wave = a * (1 - d) * m * t/2 + a/2 * (1 - d) * (1 - d) * (1 - m) * t/2
    //                             = a * (1 - d) * (1 - d + m * (1 + d)) * t/4
    //
    // => ratio of area under offset half waves given by:
    // (2) r2 = (1 - d) * (1 - d + m * (1 + d)) / ((1 + d) * (1 + d + m * (1 - d)))
    //
    // want to calculate d as a function of r1, r2; not interested in m
    //
    // rearranging (1) gives:
    // (3) m = 1 - (1 / d) * (1 - r1) / (1 + r1)
    //
    // substitute (3) into (2):
    // r2 = ((1 - d) * (1 - d + 1 + d - (1 + d) / d * (1 - r1) / (1 + r1)) / ((1 + d) * (1 + d + 1 - d - (1 - d) / d * (1 - r1) / (1 + r1)))
    //
    // after much algebra, we arrive at:
    // (4) (r1 * r2 + 3 * r1 + 3 * r2 + 1) * d^2 - 2 * (1 + r1)(1 - r2) * d + (1 - r1) * (1 - r2) = 0
    //
    // quadratic solution to (4):
    // (5) d = ((1 + r1) * (1 - r2) +/- 2 * sqrt((1 - r2) * (r1^2 - r2))) / (r1 * r2 + 3 * r1 + 3 * r2 + 1)

    // estimate offset as proportion of amplitude
    double discriminant = (1.0 - r2) * (Math.Pow(r1, 2) - r2);
    if (discriminant < 1e-10)
    {
      // catch negative values
      discriminant = 0.0;
    }

    // return estimated process value offset
    return ((1.0 + r1) * (1.0 - r2) + ((r1 > 1.0) ? 1.0 : -1.0) * Math.Sqrt(discriminant)) / (r1 * r2 + 3.0 * r1 + 3.0 * r2 + 1.0);
  }
#endif

        //C++ TO C# CONVERTER CRACKED BY X-CRACKER 2017 TODO TASK: C# does not have an equivalent to pointers to value types:
        //ORIGINAL LINE: double *input;
        public double input;
        //C++ TO C# CONVERTER CRACKED BY X-CRACKER 2017 TODO TASK: C# does not have an equivalent to pointers to value types:
        //ORIGINAL LINE: double *output;
        public double output;
        public double setpoint;

        private double oStep;
        private double noiseBand;
        private byte nLookBack = new byte();
        private byte controlType = new byte(); // * selects autotune algorithm

        private AutoTunerState state; // * state of autotuner finite state machine
        private uint lastTime;
        private uint sampleTime;
        private Peak peakType;
        private uint[] lastPeakTime = new uint[5]; // * peak time, most recent in array element 0
        private double[] lastPeaks = new double[5]; // * peak value, most recent in array element 0
        private byte peakCount = new byte();
        private double[] lastInputs = new double[101]; // * process values, most recent in array element 0
        private byte inputCount = new byte();
        private double outputStart;
        private double workingNoiseBand;
        private double workingOstep;
        private double inducedAmplitude;
        private double Kp;
        private double Ti;
        private double Td;

        // used by AMIGOf tuning rule
        private double calculatePhaseLag(double inducedAmplitude)
        {
            // calculate phase lag
            // NB hysteresis = 2 * noiseBand;
            double ratio = 2.0 * workingNoiseBand / inducedAmplitude;
            if (ratio > 1.0)
            {
                return CONST_PI / 2.0;
            }
            else
            {
                //return CONST_PI - asin(ratio);
                return CONST_PI - fastArcTan(ratio / Math.Sqrt(1.0 - Math.Pow(ratio, 2)));
            }
        }
        private double fastArcTan(double x)
        {
            // source: "Efficient approximations for the arctangent function", Rajan, S. Sichun Wang Inkol, R. Joyal, A., May 2006
            //return CONST_PI / 4.0 * x - x * (abs(x) - 1.0) * (0.2447 + 0.0663 * abs(x));

            // source: "Understanding Digital Signal Processing", 2nd Ed, Richard G. Lyons, eq. 13-107
            return x / (1.0 + 0.28125 * Math.Pow(x, 2));
        }
        private double newWorkingNoiseBand;
        private double K_process;

#if AUTOTUNE_RELAY_BIAS
  private double relayBias;
  private uint[] lastStepTime = new uint[5]; // * step time, most recent in array element 0
  private double[] sumInputSinceLastStep = new double[5]; // * integrated process values, most recent in array element 0
  private byte stepCount = new byte();
#endif

    }

    //----------------------------------------------------------------------------------------
    //	Copyright ©  - 2017 Tangible Software Solutions Inc.
    //	This class can be used by anyone provided that the copyright notice remains intact.
    //
    //	This class provides the ability to initialize and delete array elements.
    //----------------------------------------------------------------------------------------
    internal static class Arrays
    {
        internal static T[] InitializeWithDefaultInstances<T>(int length) where T : new()
        {
            T[] array = new T[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = new T();
            }
            return array;
        }

        internal static void DeleteArray<T>(T[] array) where T : System.IDisposable
        {
            foreach (T element in array)
            {
                if (element != null)
                    element.Dispose();
            }
        }
    }

    internal static class DefineConstants
    {
        public const double AUTOTUNE_PEAK_AMPLITUDE_TOLERANCE = 0.05;
        public const double AUTOTUNE_STEP_ASYMMETRY_TOLERANCE = 0.20;
        public const int AUTOTUNE_MAX_WAIT_MINUTES = 5;
    }

    public static class GlobalMembers
    {
        // source of Tyreus-Luyben and Ciancone-Marlin rules:
        // "Autotuning of PID Controllers: A Relay Feedback Approach",
        //  by Cheng-Ching Yu, 2nd Edition, p.18
        // Tyreus-Luyben is more conservative than Ziegler-Nichols
        // and is preferred for lag dominated processes
        // Ciancone-Marlin is preferred for delay dominated processes
        // Ziegler-Nichols is intended for best disturbance rejection
        // can lack robustness especially for lag dominated processes

        // source for Pessen Integral, Some Overshoot, and No Overshoot rules:
        // "Rule-Based Autotuning Based on Frequency Domain Identification" 
        // by Anthony S. McCormack and Keith R. Godfrey
        // IEEE Transactions on Control Systems Technology, vol 6 no 1, January 1998.
        // as reported on http://www.mstarlabs.com/control/znrule.html

        // order must be match enumerated type for auto tune methods
        public static Tuning[] tuningRule = new[]
    {

            new Tuning(44, 24, 0),
            new Tuning(34, 40, 160),
            new Tuning(64, 9, 0),
            new Tuning(44, 9, 126),
            new Tuning(66, 80, 0),
            new Tuning(66, 88, 162),
            new Tuning(28, 50, 133),
            new Tuning(60, 40, 60),
            new Tuning(100, 40, 60)
    };


    }
}
