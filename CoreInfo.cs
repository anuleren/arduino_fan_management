using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenHardwareMonitor.Hardware;

namespace FanPWMControl
{
    class CoreInfo
    {
        public float? temp;
        public float? load;
        public int? cpuno;
    }
    class CoreInfoProducer
    {
        private class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }
        private Computer computer = new Computer();
        public CoreInfoProducer()
        {
            computer.Open();
        }
        ~CoreInfoProducer()
        {
            computer.Close();
        }
        public List<CoreInfo> GetCoreInfo()
        {
            UpdateVisitor updateVisitor = new UpdateVisitor();
            computer.CPUEnabled = true;
            computer.Accept(updateVisitor);
            int cpuno = 0;
            List<CoreInfo> returnlist = new List<CoreInfo>();
            for (int i = 0; i < computer.Hardware.Length; i++)
            {
                if (computer.Hardware[i].HardwareType == HardwareType.CPU)//Enumerating CPU
                {
                    var cpu = computer.Hardware[i];
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)//CPU sensors
                    {

                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
                        {
                            var sensor = computer.Hardware[i].Sensors[j];
                            var sensorlist = computer.Hardware[i].Sensors;
                            var mload = sensorlist.Single(s => (s.SensorType == SensorType.Load) && (s.Index == sensor.Index)).Value;//Find correct load sensor for that temperature sensor
                            var core = new CoreInfo();
                            core.load = mload/100;
                            core.cpuno = cpuno;
                            core.temp = sensor.Value;
                            returnlist.Add(core);
                        }

                    }
                    cpuno++;
                }
            }
            return returnlist;
        }
    }
}
