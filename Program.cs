using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FanPWMControl
{
    class Program
    {
        static void Main(string[] args)
        {
            var curve_idle = new FanCurve();
            curve_idle.AddCurveLine(new FanCurveLine(0, 50, 0.3f, 0.3f));
            curve_idle.AddCurveLine(new FanCurveLine(50, 70, 0.3f, 0.7f));
            var curve_mid = new FanCurve();
            curve_mid.AddCurveLine(new FanCurveLine(0, 55, 0.3f, 0.3f));
            curve_mid.AddCurveLine(new FanCurveLine(55, 75, 0.3f, 0.8f));
            var curve_high = new FanCurve();
            curve_high.AddCurveLine(new FanCurveLine(0, 50, 0.3f, 0.3f));
            curve_high.AddCurveLine(new FanCurveLine(50, 70, 0.3f, 0.6f));
            curve_high.AddCurveLine(new FanCurveLine(70, 80, 0.6f, 1.0f));
            var global_curve = new FanCurvePerLoad();
            global_curve.AddCurve(curve_idle, 0, 0.2f);
            global_curve.AddCurve(curve_mid, 0.2f, 0.5f);
            global_curve.AddCurve(curve_high, 0.5f, 1.0f);
            var ard = new ArduinoController();
            var c1 = new CPUFanController(ard,global_curve, 1);
            var c2 = new CPUFanController(ard,global_curve, 0);
            DateTime lasttime = DateTime.Now;
            var coreInfo = new CoreInfoProducer();
            while (true)
            {
                
                var cores=coreInfo.GetCoreInfo();
                
                var cpu0 = cores.FindAll(s => s.cpuno == 0);//Classify cores by CPU
                var cpu1 = cores.FindAll(s => s.cpuno == 1);

                var cpu0Temp = cpu0.Max(s => s.temp).Value;
                var cpu1Temp = cpu1.Max(s => s.temp).Value;
                var cpu0Load = cpu0.Average(s => s.load).Value;
                var cpu1Load = cpu1.Average(s => s.load).Value;
                global_curve.SetCPULoad(cpu0Load);
                c1.SendSpeed(cpu0Temp);
                global_curve.SetCPULoad(cpu1Load);
                c2.SendSpeed(cpu1Temp);          

                while (ard.ReadRawData() != Convert.ToChar(0)) { };//Remove all pending return information
                double waittime = 500f;
                while (true)
                {
                    Thread.Sleep(50);
                    DateTime newtime = DateTime.Now;
                    double timediff = (newtime - lasttime).TotalMilliseconds;
                    if (timediff>= waittime)
                    {
                        lasttime=lasttime.AddMilliseconds(timediff);
                        break;
                    }
                }

                Console.WriteLine(cpu0Temp);
                Console.WriteLine(cpu1Temp);
                Console.WriteLine(c1.LastFanSpeed());
                Console.WriteLine(c2.LastFanSpeed());
                Console.WriteLine("===");
            }
        }
    }
}