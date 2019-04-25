using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace FanPWMControl
{
    class CPUFanController
    {
        ArduinoController arduino;
        IFanSpeedConverter speedConverter;
        int cpuNumber;
        float lastSpeed;
        int fanStuckCounter;
        public CPUFanController(ArduinoController arduino,IFanSpeedConverter speedConverter,int cpuNumber)
        {
            this.arduino = arduino;
            this.speedConverter = speedConverter;
            this.cpuNumber = cpuNumber;
            fanStuckCounter = 0;
        }

        public static string ConvertRPMForCpu(string rpm_data,int cpuNumber)
        {
            if (cpuNumber == 0) return rpm_data;
            if (cpuNumber == 1) return rpm_data.ToUpper();
            throw new Exception(String.Format("Invalid CPU number {0}", cpuNumber));
        }

        protected float SwingSpeed(float newSpeed)
        {
            float diff = newSpeed - lastSpeed;//>0 means will have to increase
            float swingSpeed = 0.025f;
            if (diff > 0.1f) swingSpeed = 0.1f;//Higher swing speed if we are quickly rising fan speed
            float setSpeed = lastSpeed + (diff * swingSpeed);
            if (setSpeed < FanCurveLine.MINIMUM_SPEED) setSpeed = FanCurveLine.MINIMUM_SPEED;
            return setSpeed;
        }

        public float LastFanSpeed()
        {
            return lastSpeed;
        }

        public void SendSpeed(float temp)
        {
            float speed = speedConverter.FanRPM(temp);
            speed = SwingSpeed(speed);
            lastSpeed = speed;
            fanStuckCounter++;
            if (fanStuckCounter>10)
            {
                fanStuckCounter = 0;
                if (speed<0.6f)
                {
                    arduino.SendRawData(ConvertRPMForCpu("f", cpuNumber)[0]);//Send full power to that fan for some time
                    Thread.Sleep(200);
                }
            }
            string rpm_send = ArduinoController.RPM2Char(speed);
            rpm_send = ConvertRPMForCpu(rpm_send, cpuNumber);
            foreach (char c in rpm_send)
            {
                arduino.SendRawData(c);
            }
        }
    }
}
