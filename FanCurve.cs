using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FanPWMControl
{
    public interface IFanSpeedConverter
    {
        float FanRPM(float temp);
        bool IsTemperatureWithinRange(float temp);
    }
    public interface ICPULoadObserver
    {
        void SetCPULoad(float cpuload);
    }

    public class FanCurveLine : IFanSpeedConverter
    {
        public const float MINIMUM_SPEED = 0.3f;
        public const float MAXIMUM_SPEED = 1.0f;
        private float mintemp;
        private float maxtemp;
        private float minspeed;
        private float maxspeed;
        public FanCurveLine(float mintemp, float maxtemp, float minspeed, float maxspeed)
        {
            this.mintemp = mintemp;
            this.maxtemp = maxtemp;
            this.minspeed = minspeed;
            this.maxspeed = maxspeed;
        }

        public bool IsTemperatureWithinRange(float temp)
        {
            return ((temp >= mintemp) && (temp <= maxtemp));
        }

        public float FanRPM(float temp)
        {
            if (!IsTemperatureWithinRange(temp))
            {
                if (temp < mintemp) return MINIMUM_SPEED;//Speed outside of our curve
                if (temp > maxtemp) return MAXIMUM_SPEED;
            }
            float adjtemp = temp - mintemp;//0-based in our temperature range
            adjtemp = adjtemp / (maxtemp - mintemp);//rescale into 0..1 range for our temperature range
            var rpm = adjtemp * (maxspeed - minspeed);//Convert into fan speed range(0-based)
            rpm = rpm + minspeed;//Back to normal RPM from 0-based RPM
            if (rpm > maxspeed) rpm = maxspeed;
            if (rpm < minspeed) rpm = minspeed;
            return rpm;
        }
    }
    public class FanCurve : IFanSpeedConverter
    {
        protected List<FanCurveLine> fanline = new List<FanCurveLine>();
        public FanCurve()
        {
        }
        public void AddCurveLine(FanCurveLine p)
        {
            fanline.Add(p);
        }

        public bool IsTemperatureWithinRange(float temp)
        {
            foreach (FanCurveLine p in fanline)
            {
                if (p.IsTemperatureWithinRange(temp)) return true;
            }
            return false;
        }

        public float FanRPM(float temp)
        {
            foreach (FanCurveLine p in fanline)
            {
                if (p.IsTemperatureWithinRange(temp)) return p.FanRPM(temp);
            }
            throw new Exception(String.Format("No fan speed defined for temperature {0}", temp));
        }
    }
    public class FanCurvePerLoad: IFanSpeedConverter,ICPULoadObserver
    {
        protected class CurveLoadRecord
        {
            public float minimum_load;
            public float maximum_load;
            public FanCurve curve;

            public CurveLoadRecord(float minload,float maxload,FanCurve curve)
            {
                this.minimum_load = minload;
                this.maximum_load = maxload;
                this.curve = curve;
            }
            public bool IsLoadWithinRange(float load)
            {
                return ((maximum_load >= load) && (minimum_load <= load));
            }

        }
        protected List<CurveLoadRecord> curves = new List<CurveLoadRecord>();
        protected FanCurve current_curve;

        public bool IsTemperatureWithinRange(float temp)
        {
            return current_curve.IsTemperatureWithinRange(temp);
        }

        public void AddCurve(FanCurve curve, float minload,float maxload)
        {
            curves.Add(new CurveLoadRecord(minload, maxload, curve));
        }

        public float FanRPM(float temp)
        {
            return current_curve.FanRPM(temp);
        }
        public void SetCPULoad(float cpuload)
        {
            foreach (CurveLoadRecord c in curves)
            {
                if (c.IsLoadWithinRange(cpuload))
                {
                    current_curve = c.curve;
                    return;
                }
            }
            throw new Exception(String.Format("No load curve defined for cpuload {0}", cpuload));
        }
    }

}
