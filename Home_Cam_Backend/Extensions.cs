using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FFMediaToolkit.Graphics;
using Home_Cam_Backend.Controllers;
using Home_Cam_Backend.Dtos;
using Home_Cam_Backend.Entities;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Home_Cam_Backend
{
    public static class Extensions
    {

        public static TaskCompletionSource<string> tcs = null;

        private static Mutex mut = new Mutex();

        private static IConfiguration configuration;

        public static void Init(IConfiguration _configuration)
        {
            configuration=_configuration;
        }

        // public static (string gatewayAddress, string subnetMask) getGatewayAddressAndSubnetMask()
        // {
        //     string gatewayAddress="", subnetMask="";
        //     bool hasValidNetwork = false;
        //     foreach (NetworkInterface f in NetworkInterface.GetAllNetworkInterfaces())  
        //     {  
        //         IPInterfaceProperties ipInterface = f.GetIPProperties(); 
        //         var gatewayAddresses = ipInterface.GatewayAddresses;
        //         var unicastAddresses = ipInterface.UnicastAddresses;
        //         if(gatewayAddresses.Count>0)
        //         {
        //             gatewayAddress=gatewayAddresses[0].Address.AddressFamily.ToString()=="InterNetwork"?
        //                             gatewayAddresses[0].Address.ToString():
        //                             "N/A";
        //             foreach(var unicastAddress in unicastAddresses)
        //             {
        //                 if(unicastAddress.Address.AddressFamily.ToString()=="InterNetwork")
        //                 {
        //                     hasValidNetwork = true;
        //                     subnetMask=unicastAddress.IPv4Mask.ToString();
        //                 }
        //             }
        //         }
        //     }
        //     if(!hasValidNetwork)
        //     {
        //         Console.WriteLine("Stop finding camera. Not in IPv4 network.");
        //         gatewayAddress="N/A";
        //         subnetMask="N/A";
        //     }  
        //     return (gatewayAddress: gatewayAddress, subnetMask: subnetMask);
        // }

        public static int[] ipAddrToOcts(string ipAddress)
        {
            int[] oct = new int[4];
            oct[0]=Int32.Parse(ipAddress.Substring(0, ipAddress.IndexOf('.')));
            ipAddress=ipAddress.Substring(ipAddress.IndexOf('.')+1);
            oct[1]=Int32.Parse(ipAddress.Substring(0, ipAddress.IndexOf('.')));
            ipAddress=ipAddress.Substring(ipAddress.IndexOf('.')+1);
            oct[2]=Int32.Parse(ipAddress.Substring(0, ipAddress.IndexOf('.')));
            ipAddress=ipAddress.Substring(ipAddress.IndexOf('.')+1);
            oct[3]=Int32.Parse(ipAddress);
            return oct;
        }

        public static Task<List<string>> getListOfSubnetIpAddresses(bool withBroadcastingAddress)
        {
            List<string> ipList = new();

            // (string gatewayAddress, string subnetMask) = getGatewayAddressAndSubnetMask();
            string gatewayAddress = configuration.GetSection("FindCamIPRangeSettings").GetValue<string>("GatewayAddress");
            string subnetMask = configuration.GetSection("FindCamIPRangeSettings").GetValue<string>("SubnetMask");

            if(gatewayAddress=="N/A" || subnetMask=="N/A"){
                return Task.FromResult(ipList);
            }

            // oct[0].oct[1].oct[2].oct[3]
            int[] subnetOct = ipAddrToOcts(subnetMask);
            int[] gatewayOct = ipAddrToOcts(gatewayAddress);

            int[] minIpAddr = new int[4];
            int[] maxIpAddr = new int[4];

            for(int i=0; i<4; i++)
            {
                minIpAddr[i]=gatewayOct[i]&subnetOct[i];
                maxIpAddr[i]=minIpAddr[i]+(~subnetOct[i]&0xFF);
                if(!withBroadcastingAddress && i==3)
                {
                    maxIpAddr[3]-=1;
                }
            }
            int count=0;
            int[] currIp = new int[4];
            Array.Copy(minIpAddr, currIp, 4);
            while(currIp[0]!=maxIpAddr[0] || currIp[1]!=maxIpAddr[1] || currIp[2]!=maxIpAddr[2] || currIp[3]!=maxIpAddr[3])
            {
                Array.Copy(minIpAddr, currIp, 4);
                int tempCount=count;
                for(int i=3; i>=0; i--)
                {
                    currIp[i]+=(tempCount%256);
                    tempCount/=256;
                }
                ipList.Add(currIp[0].ToString()+"."+currIp[1].ToString()+"."+currIp[2].ToString()+"."+currIp[3].ToString());
                count++;
            }

            return Task.FromResult(ipList);
        }

        public static CamSettingDto AsDto(this EEsp32CamSetting camSetting)
        {
            return new CamSettingDto
            {
                UniqueId=camSetting.UniqueId,
                Location=camSetting.Location,
                FrameSize=camSetting.FrameSize,
                FlashLightOn=camSetting.FlashLightOn,
                HorizontalMirror=camSetting.HorizontalMirror,
                VerticalMirror=camSetting.VerticalMirror
            };
        }

        public static CamDto AsDto(this Esp32Cam cam)
        {
            return new CamDto
            {
                IpAddr=cam.IpAddr,
                UniqueId=cam.UniqueId
            };
        }

        public static void WriteToLogFile(string content, string path = "")
        {
            if(path=="")
            {
                path=configuration.GetSection("BackendLog").GetValue<string>("LogFilePath");
            }
            mut.WaitOne();
            using(FileStream ostrm = new(path, FileMode.OpenOrCreate | FileMode.Append, FileAccess.Write))
            {
                using(StreamWriter writer = new StreamWriter (ostrm))
                {
                    TextWriter oldOut = Console.Out;
                    Console.SetOut (writer);
                    Console.WriteLine(content);
                    Console.SetOut (oldOut);
                }
            }
            mut.ReleaseMutex();
            tcs?.TrySetResult(content);
        }

        public static Image<Bgr24> ToBitmap(this ImageData imageData)
        {
            return Image.LoadPixelData<Bgr24>(imageData.Data, imageData.ImageSize.Width, imageData.ImageSize.Height);
        }

        public static byte[] hexStrToByteArray(string hex)
        {
            // https://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array
            return Enumerable.Range(0, hex.Length)
                                .Where(x=>x%2==0)
                                .Select(x=>Convert.ToByte(hex.Substring(x,2), 16))
                                .ToArray();
        }

        public static string restoreMacAddr(this string mac) {
            if(mac.Length!=12){
                throw new Exception($"[restoreMacAddr] Invalid input MAC address: {mac}");
            }

            return Regex.Replace(mac, @"(\w{2})(\w{2})(\w{2})(\w{2})(\w{2})(\w{2})", @"$1:$2:$3:$4:$5:$6");
        }
    }
}
