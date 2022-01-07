using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Home_Cam_Backend.Controllers;
using Microsoft.Extensions.Hosting;

namespace Home_Cam_Backend.BackgroundTasks
{

    public class ImageCaptureBackgroundTask : BackgroundService
    {
        private byte[] lengthDelimByteArray = Encoding.UTF8.GetBytes("length=");
        private byte[] timeDelimByteArray = Encoding.UTF8.GetBytes("time=");
        private byte[] endDelimByteArray = Encoding.UTF8.GetBytes("X");

        private int LengthTimeParser(byte[] buffer, int bufferLength, ref int length, ref long time)
        {
            int dataIndex = 0;
            ReadOnlySpan<byte> ro = new(buffer, 0, bufferLength);



            // find length
            var i = ro.IndexOf(lengthDelimByteArray);
            if (i == -1)
            {
                return -1;
            }



            ro = ro.Slice(i + 7);
            dataIndex += (i + 7);

            i = ro.IndexOf(endDelimByteArray);
            if (i == -1)
            {
                return -1;
            }



            length = int.Parse(Encoding.UTF8.GetString(ro.Slice(0, i).ToArray()));



            ro = ro.Slice(i + 1);
            dataIndex += (i + 1);

            i = ro.IndexOf(timeDelimByteArray);
            if (i == -1)
            {
                return -1;
            }



            ro = ro.Slice(i + 5);
            dataIndex += (i + 5);

            i = ro.IndexOf(endDelimByteArray);
            if (i == -1)
            {
                return -1;
            }

            time = long.Parse(Encoding.UTF8.GetString(ro.Slice(0, i).ToArray()));

            dataIndex += (i + 1);



            return dataIndex;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string ImageFolderPath = "D:/Download/Test_Images";
            long timeoutLimitMilliseconds = 20000;

            while (!stoppingToken.IsCancellationRequested)
            {

                // read appropiate number of bytes based on state
                List<Task<int>> readBufferResult = new();
                for (int i = 0; i < CamController.ActiveCameras.Count; i++)
                {
                    switch (CamController.ActiveCameras[i].MyParsingStatus)
                    {
                        case Esp32Cam.ParsingStatus.LookingForLengthAndTime:
                            {
                                readBufferResult.Add(
                                    CamController.ActiveCameras[i].CamStream.ReadAsync(
                                        CamController.ActiveCameras[i].StreamBuffer,
                                        0,
                                        Esp32Cam.StreamBufferSize
                                    )
                                );
                                break;
                            }
                        case Esp32Cam.ParsingStatus.GettingData:
                            {
                                readBufferResult.Add(
                                    CamController.ActiveCameras[i].CamStream.ReadAsync(
                                        CamController.ActiveCameras[i].StreamBuffer,
                                        0,
                                        Esp32Cam.StreamBufferSize > CamController.ActiveCameras[i].RemainingData ?
                                            CamController.ActiveCameras[i].RemainingData :
                                            Esp32Cam.StreamBufferSize
                                    )
                                );
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }


                for (int i = 0; i < CamController.ActiveCameras.Count; i++)
                {
                    int bytesRead = await readBufferResult[i];
                    // if nothing read
                    if (bytesRead == 0)
                    {
                        if (CamController.ActiveCameras[i].ImageBuffer[CamController.ActiveCameras[i].ImageBufferHeadIndex].valid)
                        {
                            // camera stream not responding longer than timeout limit
                            if (
                                (new DateTimeOffset(DateTime.UtcNow)).ToUnixTimeMilliseconds()
                                -
                                CamController.ActiveCameras[i].ImageBuffer[CamController.ActiveCameras[i].ImageBufferHeadIndex].CreatedDate.ToUnixTimeMilliseconds()
                                >
                                timeoutLimitMilliseconds)
                            {
                                // try to get a new stream
                                try
                                {
                                    await CamController.ActiveCameras[i].Streaming();
                                }
                                catch
                                {
                                    // fail to get new stream, remove camera
                                    CamController.ActiveCameras.RemoveAt(i);
                                    i--;
                                }
                            }
                        }

                    }
                    else
                    {
                        switch (CamController.ActiveCameras[i].MyParsingStatus)
                        {
                            case Esp32Cam.ParsingStatus.LookingForLengthAndTime:
                                {
                                    int dataLength = 0;
                                    long imageTime = 0;
                                    int dataIndex = LengthTimeParser(CamController.ActiveCameras[i].StreamBuffer, bytesRead, ref dataLength, ref imageTime);
                                    if (dataIndex != -1)
                                    {
                                        CamController.ActiveCameras[i].RemainingData = dataLength;

                                        int nextImageBufferIndex = (CamController.ActiveCameras[i].ImageBufferHeadIndex + 1) % Esp32Cam.ImageBufferMaxSize;

                                        // update creation time
                                        CamController.ActiveCameras[i].ImageBuffer[nextImageBufferIndex].CreatedDate = DateTimeOffset.FromUnixTimeMilliseconds(
                                            CamController.ActiveCameras[i].DiscoverTimeServer.ToUnixTimeMilliseconds() + (imageTime / 1000 - CamController.ActiveCameras[i].DiscoverTimeCameraMilliseconds)
                                        );

                                        // add data
                                        int currentDataLength = bytesRead - dataIndex;

                                        CamController.ActiveCameras[i].ImageBuffer[nextImageBufferIndex].image = new byte[dataLength];

                                        Array.Copy(
                                            CamController.ActiveCameras[i].StreamBuffer,
                                            dataIndex,
                                            CamController.ActiveCameras[i].ImageBuffer[nextImageBufferIndex].image,
                                            CamController.ActiveCameras[i].CurrentImageByteIndex,
                                            currentDataLength
                                        );

                                        CamController.ActiveCameras[i].CurrentImageByteIndex += currentDataLength;
                                        // need to read more data
                                        if (dataLength > bytesRead - dataIndex)
                                        {
                                            CamController.ActiveCameras[i].MyParsingStatus = Esp32Cam.ParsingStatus.GettingData;
                                            CamController.ActiveCameras[i].RemainingData = dataLength - currentDataLength;
                                        }
                                        // this stream contains all data
                                        else
                                        {
                                            // make this image valid
                                            CamController.ActiveCameras[i].ImageBuffer[nextImageBufferIndex].valid = true;
                                            // update image buffer head pointer
                                            CamController.ActiveCameras[i].ImageBufferHeadIndex = nextImageBufferIndex;
                                            // make next image invalid
                                            nextImageBufferIndex = (CamController.ActiveCameras[i].ImageBufferHeadIndex + 1) % Esp32Cam.ImageBufferMaxSize;
                                            CamController.ActiveCameras[i].ImageBuffer[nextImageBufferIndex].valid = false;
                                            CamController.ActiveCameras[i].ImageBuffer[nextImageBufferIndex].image = null;
                                        }
                                    }
                                    break;
                                }
                            case Esp32Cam.ParsingStatus.GettingData:
                                {

                                    // add data
                                    int nextImageBufferIndex = (CamController.ActiveCameras[i].ImageBufferHeadIndex + 1) % Esp32Cam.ImageBufferMaxSize;

                                    int currentDataLength = bytesRead;

                                    Array.Copy(
                                        CamController.ActiveCameras[i].StreamBuffer,
                                        0,
                                        CamController.ActiveCameras[i].ImageBuffer[nextImageBufferIndex].image,
                                        CamController.ActiveCameras[i].CurrentImageByteIndex,
                                        currentDataLength
                                    );

                                    CamController.ActiveCameras[i].CurrentImageByteIndex += currentDataLength;
                                    CamController.ActiveCameras[i].RemainingData -= currentDataLength;

                                    if (CamController.ActiveCameras[i].RemainingData == 0)
                                    {
                                        CamController.ActiveCameras[i].MyParsingStatus = Esp32Cam.ParsingStatus.LookingForLengthAndTime;
                                        // reset curr image buffer pointer
                                        CamController.ActiveCameras[i].CurrentImageByteIndex = 0;
                                        // make this image valid
                                        CamController.ActiveCameras[i].ImageBuffer[nextImageBufferIndex].valid = true;
                                        // update image buffer head pointer
                                        CamController.ActiveCameras[i].ImageBufferHeadIndex = nextImageBufferIndex;
                                        // make next image invalid
                                        nextImageBufferIndex = (CamController.ActiveCameras[i].ImageBufferHeadIndex + 1) % Esp32Cam.ImageBufferMaxSize;
                                        CamController.ActiveCameras[i].ImageBuffer[nextImageBufferIndex].valid = false;
                                        CamController.ActiveCameras[i].ImageBuffer[nextImageBufferIndex].image = null;

                                        string rawMacAddr = CamController.ActiveCameras[i].UniqueId;
                                        Regex pattern = new Regex("[:]");
                                        rawMacAddr=pattern.Replace(rawMacAddr, "");

                                        string filePath = $"{ImageFolderPath}/{rawMacAddr}/{(new DateTimeOffset(DateTime.UtcNow)).ToUnixTimeMilliseconds()}.jpg";

                                        System.IO.FileInfo file = new System.IO.FileInfo(filePath);
                                        file.Directory.Create();

                                        // save image
                                        var _ = File.WriteAllBytesAsync(file.FullName, CamController.ActiveCameras[i].ImageBuffer[CamController.ActiveCameras[i].ImageBufferHeadIndex].image);
                                    }

                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                    }

                }

                await Task.Delay(100);
            }

            return;
        }

    }
}