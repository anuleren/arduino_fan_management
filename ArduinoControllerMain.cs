using System;

using System.Threading;

using System.IO.Ports;

using System.IO;

public class ArduinoController
{

    SerialPort currentPort;
    bool portFound;

    ~ArduinoController()
    {
        if (portFound) currentPort.Close();
    }
    public void SendRawData(char data)
    {
        while (!portFound)
        {
            SetComPort();
        }
        var buf = new byte[1];
        buf[0] = Convert.ToByte(data);
        currentPort.Write(buf, 0, 1);
    }
    public char ReadRawData()
    {
        while (!portFound)
        {
            SetComPort();
        }
        var buf = new byte[1];
        if (currentPort.BytesToRead <= 0) return Convert.ToChar(0);
        currentPort.Read(buf, 0, 1);
        return Convert.ToChar(buf[0]);
    }
    private void SetComPort()
    {
        try
        {
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                currentPort = new SerialPort(port, 9600);
                if (DetectArduino())
                {
                    portFound = true;
                    currentPort.Open();
                    break;
                }
                else
                {
                    portFound = false;
                }
            }
        }
        catch (Exception e)
        {
        }
    }
    private bool DetectArduino()
    {
        try
        {
            //The below setting are for the Hello handshake
            byte[] buffer = new byte[1];
            buffer[0] = Convert.ToByte('k');
            int intReturnASCII = 0;
            char charReturnValue = (Char)intReturnASCII;
            currentPort.Open();
            currentPort.Write(buffer, 0, 1);
            int i = 0;
            while ((currentPort.BytesToRead==0)&&(i++<10)) Thread.Sleep(100);
            int count = currentPort.BytesToRead;
            string returnMessage = "";
            while (count > 0)
            {
                intReturnASCII = currentPort.ReadByte();
                returnMessage = returnMessage + Convert.ToChar(intReturnASCII);
                count--;
            }
            currentPort.Close();
            if (returnMessage.Contains("OK"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public static string RPM2Char(float speed)
    {
        if (speed >= 0.99f) return "l";
        var mainchar = '9';
        if (speed >= 0.7f)
        {
            mainchar = 'm';
            speed = speed - 0.7f;
        }
        else
          if (speed >= 0.3f)
        {
            mainchar = 's';
            speed = speed - 0.3f;
        }
        string extra = "";

        while (speed >= 0.20f)
        {
            extra = extra + "d";
            speed = speed - 0.2f;
        }
        while (speed >= 0.10f)
        {
            extra = extra + "c";
            speed = speed - 0.1f;
        }
        while (speed >= 0.05f)
        {
            extra = extra + "p";
            speed = speed - 0.05f;
        }
        while (speed >= 0.02f)
        {
            speed = speed - 0.02f;
            extra = extra + "j";
        }
        while (speed >= 0.01f)
        {
            speed = speed - 0.01f;
            extra = extra + "i";
        }
        return mainchar + extra;
    }
}