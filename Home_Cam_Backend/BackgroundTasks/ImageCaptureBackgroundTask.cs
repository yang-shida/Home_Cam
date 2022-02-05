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
using Home_Cam_Backend.Entities;
using Home_Cam_Backend.Repositories;
using Home_Cam_Backend.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SixLabors.ImageSharp;

namespace Home_Cam_Backend.BackgroundTasks
{

    public class ImageCaptureBackgroundTask : BackgroundService
    {
        private byte[] lengthDelimByteArray = Encoding.UTF8.GetBytes("length=");
        private byte[] timeDelimByteArray = Encoding.UTF8.GetBytes("time=");
        private byte[] endDelimByteArray = Encoding.UTF8.GetBytes("X");
        private byte[] jpegStartSeq = { 0xFF, 0xD8, 0xFF };

        private readonly ICapturedImagesRepository capturedImageInfoRepository;
        private readonly IConfiguration Configuration;

        public ImageCaptureBackgroundTask(ICapturedImagesRepository repo, IConfiguration configuration)
        {
            this.capturedImageInfoRepository = repo;
            this.Configuration = configuration;
        }

        private int LengthTimeParser(byte[] buffer, ref int bufferHeadIndex, int bufferLength, ref int length, ref long time)
        {
            // length=....Xtime=....X
            int dataIndex = bufferHeadIndex;
            int bufferBeginningIndex = bufferHeadIndex;
            ReadOnlySpan<byte> ro = new(buffer, bufferHeadIndex, bufferLength);

            // find length
            var i = ro.IndexOf(lengthDelimByteArray);
            if (i == -1)
            {
                bufferHeadIndex = bufferBeginningIndex + bufferLength;
                return -1;
            }



            ro = ro.Slice(i + 7);
            dataIndex += (i + 7);
            bufferHeadIndex = dataIndex;

            // find length X
            i = ro.IndexOf(endDelimByteArray);
            if (i == -1)
            {
                bufferHeadIndex = bufferBeginningIndex + bufferLength;
                return -1;
            }

            length = int.Parse(Encoding.UTF8.GetString(ro.Slice(0, i).ToArray()));

            ro = ro.Slice(i + 1);
            dataIndex += (i + 1);
            bufferHeadIndex = dataIndex;

            // find time
            i = ro.IndexOf(timeDelimByteArray);
            if (i == -1)
            {
                bufferHeadIndex = bufferBeginningIndex + bufferLength;
                return -1;
            }

            ro = ro.Slice(i + 5);
            dataIndex += (i + 5);
            bufferHeadIndex = dataIndex;

            // find time X
            i = ro.IndexOf(endDelimByteArray);
            if (i == -1)
            {
                bufferHeadIndex = bufferBeginningIndex + bufferLength;
                return -1;
            }

            time = long.Parse(Encoding.UTF8.GetString(ro.Slice(0, i).ToArray()));

            ro = ro.Slice(i + 1);
            dataIndex += (i + 1);
            bufferHeadIndex = dataIndex;

            // check if the chunk is possibly aligned correctly
            if (bufferHeadIndex == bufferBeginningIndex + bufferLength || buffer[bufferHeadIndex] == 0xFF)
            {
                return dataIndex;
            }
            // the chunk must be bad
            else
            {
                return -1;
            }
        }

        private async void DoWork(CancellationToken stoppingToken)
        {
            string ImageFolderPath = Configuration.GetSection("ImageStorageSettings").GetValue<string>("Path");

            int singleReadTimeoutLimitMilliseconds = 50;

            while (!stoppingToken.IsCancellationRequested)
            {

                var delayTask = Task.Delay(Configuration.GetSection("BackgroundTasksTimingSettings").GetValue<int>("ImageCaptureMillisecs"));

                // read appropiate number of bytes based on state
                List<Task<int>> readBufferResult = new();

                for (int i = 0; i < CamController.ActiveCameras.Count; i++)
                {
                    // Console.WriteLine("send req");
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
                    // Console.WriteLine("new buffer --------------------------------------------");
                    // Console.WriteLine("await req");
                    int bytesRead = 0;
                    try
                    {
                        using ((new CancellationTokenSource(singleReadTimeoutLimitMilliseconds)).Token.Register(() => CamController.ActiveCameras[i].CamStream.Close()))
                        {
                            bytesRead = await readBufferResult[i];
                            if (bytesRead == 0)
                            {
                                throw new Exception();
                            }
                        }
                    }
                    catch
                    {
                        // try to get a new stream
                        try
                        {
                            // Console.WriteLine("try to recover");
                            await CamController.ActiveCameras[i].Streaming();
                            // Console.WriteLine("recover success");
                        }
                        catch
                        {
                            // Console.WriteLine("recover fail");
                            // fail to get new stream, remove camera
                            Extensions.WriteToLogFile($"[{DateTime.Now.ToString("MM/dd/yyyy-hh:mm:ss")}] Camera connection lost with MAC = {CamController.ActiveCameras[i].UniqueId} and IP = {CamController.ActiveCameras[i].IpAddr}");
                            CamController.ActiveCameras.RemoveAt(i);
                            i--;
                        }
                    }

                    if (bytesRead > 0)
                    {
                        // using(FileStream ostrm = new("D:/Download/RawResponse1.txt", FileMode.OpenOrCreate | FileMode.Append, FileAccess.Write))
                        // {
                        //     await ostrm.WriteAsync(CamController.ActiveCameras[i].StreamBuffer, 0, bytesRead);
                        // }
                        int bufferHeadIndex = 0;
                        while (bufferHeadIndex < bytesRead)
                        {
                            // Console.WriteLine("parsing");
                            switch (CamController.ActiveCameras[i].MyParsingStatus)
                            {
                                case Esp32Cam.ParsingStatus.LookingForLengthAndTime:
                                    {
                                        // Console.WriteLine("parse length and time");
                                        int dataLength = 0;
                                        long imageTime = 0;
                                        int dataIndex = LengthTimeParser(CamController.ActiveCameras[i].StreamBuffer, ref bufferHeadIndex, bytesRead - bufferHeadIndex, ref dataLength, ref imageTime);
                                        if (dataIndex != -1)
                                        {
                                            CamController.ActiveCameras[i].RemainingData = dataLength;

                                            int nextImageBufferIndex = (CamController.ActiveCameras[i].ImageBufferHeadIndex + 1) % Esp32Cam.ImageBufferMaxSize;

                                            // update creation time
                                            CamController.ActiveCameras[i].ImageBuffer[nextImageBufferIndex].CreatedDate = DateTimeOffset.FromUnixTimeMilliseconds(
                                                CamController.ActiveCameras[i].DiscoverTimeServer.ToUnixTimeMilliseconds() + (imageTime / 1000 - CamController.ActiveCameras[i].DiscoverTimeCameraMilliseconds)
                                            );

                                            CamController.ActiveCameras[i].ImageBuffer[nextImageBufferIndex].image = new byte[dataLength];

                                            CamController.ActiveCameras[i].MyParsingStatus = Esp32Cam.ParsingStatus.GettingData;
                                        }
                                        break;
                                    }
                                case Esp32Cam.ParsingStatus.GettingData:
                                    {
                                        // Console.WriteLine("parse data");
                                        // add data
                                        int nextImageBufferIndex = (CamController.ActiveCameras[i].ImageBufferHeadIndex + 1) % Esp32Cam.ImageBufferMaxSize;

                                        int dataLength = CamController.ActiveCameras[i].RemainingData;

                                        int currentDataLength = dataLength > (bytesRead - bufferHeadIndex) ? (bytesRead - bufferHeadIndex) : dataLength;

                                        Array.Copy(
                                            CamController.ActiveCameras[i].StreamBuffer,
                                            bufferHeadIndex,
                                            CamController.ActiveCameras[i].ImageBuffer[nextImageBufferIndex].image,
                                            CamController.ActiveCameras[i].CurrentImageByteIndex,
                                            currentDataLength
                                        );

                                        bufferHeadIndex += currentDataLength;

                                        CamController.ActiveCameras[i].CurrentImageByteIndex += currentDataLength;
                                        CamController.ActiveCameras[i].RemainingData -= currentDataLength;

                                        if (CamController.ActiveCameras[i].RemainingData == 0)
                                        {
                                            // Console.WriteLine($"Frame time: {CamController.ActiveCameras[i].ImageBuffer[nextImageBufferIndex].CreatedDate.ToUnixTimeMilliseconds()-CamController.ActiveCameras[i].ImageBuffer[CamController.ActiveCameras[i].ImageBufferHeadIndex].CreatedDate.ToUnixTimeMilliseconds()} ms");

                                            CamController.ActiveCameras[i].MyParsingStatus = Esp32Cam.ParsingStatus.LookingForLengthAndTime;
                                            // reset curr image buffer pointer
                                            CamController.ActiveCameras[i].CurrentImageByteIndex = 0;

                                            int imageLen = CamController.ActiveCameras[i].ImageBuffer[nextImageBufferIndex].image.Length;

                                            // check if this JPEG is valid
                                            if (CamController.ActiveCameras[i].ImageBuffer[nextImageBufferIndex].image[0] == 0xFF &&
                                                CamController.ActiveCameras[i].ImageBuffer[nextImageBufferIndex].image[1] == 0xD8 &&
                                                CamController.ActiveCameras[i].ImageBuffer[nextImageBufferIndex].image[2] == 0xFF &&
                                                CamController.ActiveCameras[i].ImageBuffer[nextImageBufferIndex].image[imageLen - 2] == 0xFF &&
                                                CamController.ActiveCameras[i].ImageBuffer[nextImageBufferIndex].image[imageLen - 1] == 0xD9
                                            )
                                            {
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
                                                rawMacAddr = pattern.Replace(rawMacAddr, "");

                                                string filePath = $"{ImageFolderPath}/{rawMacAddr}/{(new DateTimeOffset(DateTime.UtcNow)).ToUnixTimeMilliseconds()}.jpg";

                                                System.IO.FileInfo file = new System.IO.FileInfo(filePath);
                                                file.Directory.Create();

                                                // save image
                                                await File.WriteAllBytesAsync(file.FullName, CamController.ActiveCameras[i].ImageBuffer[CamController.ActiveCameras[i].ImageBufferHeadIndex].image);

                                                ECapturedImageInfo newCapturedImageInfo = new()
                                                {
                                                    CamId = CamController.ActiveCameras[i].UniqueId,
                                                    ImageFileLocation = filePath,
                                                    CreatedDate = CamController.ActiveCameras[i].ImageBuffer[CamController.ActiveCameras[i].ImageBufferHeadIndex].CreatedDate,
                                                    Size = CamController.ActiveCameras[i].ImageBuffer[CamController.ActiveCameras[i].ImageBufferHeadIndex].image.Length
                                                };

                                                // Console.WriteLine(newCapturedImageInfo.CreatedDate.ToString());

                                                await capturedImageInfoRepository.CreateImageInfo(newCapturedImageInfo);
                                            }
                                            // corrupted image
                                            else
                                            {
                                                CamController.ActiveCameras[i].ImageBuffer[nextImageBufferIndex].image = null;
                                            }

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

                }

                await delayTask;
            }

            return;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() => DoWork(stoppingToken));
        }

    }
}