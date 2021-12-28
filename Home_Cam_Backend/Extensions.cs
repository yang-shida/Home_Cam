using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Home_Cam_Backend.Controllers;
using Home_Cam_Backend.Dtos;
using Home_Cam_Backend.Entities;

namespace Home_Cam_Backend
{
    public static class Extensions
    {
        public static (string gatewayAddress, string subnetMask) getGatewayAddressAndSubnetMask()
        {
            string gatewayAddress="", subnetMask="";
            foreach (NetworkInterface f in NetworkInterface.GetAllNetworkInterfaces())  
            {  
                IPInterfaceProperties ipInterface = f.GetIPProperties(); 
                var gatewayAddresses = ipInterface.GatewayAddresses;
                var unicastAddresses = ipInterface.UnicastAddresses;
                if(gatewayAddresses.Count>0)
                {
                    gatewayAddress=gatewayAddresses[0].Address.ToString();
                    foreach(var unicastAddress in unicastAddresses)
                    {
                        if(unicastAddress.Address.AddressFamily.ToString()=="InterNetwork")
                        {
                            subnetMask=unicastAddress.IPv4Mask.ToString();
                        }
                    }
                }
            }  
            return (gatewayAddress: gatewayAddress, subnetMask: subnetMask);
        }

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

            (string gatewayAddress, string subnetMask) = getGatewayAddressAndSubnetMask();
            
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
    }
}
