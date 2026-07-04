using Example.Common.Const;
using Example.Common.Enums;
using Example.Common.Models;
using Example.Common.Models.AppSetting;
using Example.Common.Utilities;
using Example.Common.Utilities.Helper;
using Confluent.Kafka;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.DataModel.Notification;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reactive.Disposables;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Example.Common.Storage
{
    public class UploadFile
    {
        public static async Task<List<MinIOImageModel>> Upload(List<IFormFile> files, string prefix, UploadFileModel uploadConfig)
        {
            try
            {
                if (files == null)
                {
                    return new List<MinIOImageModel>();
                }

                var config = uploadConfig != null ? uploadConfig : GetDefaultConfig();

                using var minio = new MinioClient()
                            .WithEndpoint(config.EndPoint)
                            .WithCredentials(config.AccessKey, config.SecretKey)
                            .WithSSL(config.IsSSL)
                            .Build();


                var bucketName = config.BucketName;
                // Make a bucket on the server, if not already present.
                var beArgs = new BucketExistsArgs()
                    .WithBucket(bucketName);
                bool found = await minio.BucketExistsAsync(beArgs).ConfigureAwait(false);
                if (!found)
                {
                    var mbArgs = new MakeBucketArgs()
                        .WithBucket(bucketName);
                    await minio.MakeBucketAsync(mbArgs).ConfigureAwait(false);
                }
                var lstUrls = new List<MinIOImageModel>();
                int index = 0;
                foreach (var file in files)
                {
                    index++;
                    var fileName = DateTimeHelper.DateTimeToUnixTime(DateTime.Now) + index;
                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    var objectName = $"{prefix}{fileName}{extension}";

                    // Upload a file to bucket.
                    using (var stream = file.OpenReadStream())
                    {
                        var putObjectArgs = new PutObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                        .WithObjectSize(file.Length)
                        .WithStreamData(stream)
                        .WithContentType(file.ContentType);
                        await minio.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
                    }

                    //Lấy url với expired time = 1 days
                    PresignedGetObjectArgs args = new PresignedGetObjectArgs()
                                      .WithBucket(bucketName)
                                      .WithObject(objectName)
                                      .WithExpiry(60 * 60 * 24);
                    var url = await minio.PresignedGetObjectAsync(args);
                    lstUrls.Add(new MinIOImageModel()
                    {
                        Url = url,
                        BucketName = bucketName,
                        ObjectName = objectName
                    });
                }
                return lstUrls;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
                //_logger.LogError(ex, ex.Message);
                return new List<MinIOImageModel>();
            }
        }

        public static async Task<List<MinIOImageModel>> UploadUserImages(List<string> files, string prefix, UploadFileModel uploadConfig = null)
        {
            try
            {
                var filePath = StaticVariable.CommonSetting.UserImagePath;
                var config = uploadConfig != null ? uploadConfig : GetDefaultConfig();

                using var minio = new MinioClient()
                            .WithEndpoint(config.EndPoint)
                            .WithCredentials(config.AccessKey, config.SecretKey)
                            .WithSSL(config.IsSSL)
                            .Build();
                var bucketName = config.BucketName;
                // Make a bucket on the server, if not already present.
                var beArgs = new BucketExistsArgs()
                    .WithBucket(bucketName);
                bool found = await minio.BucketExistsAsync(beArgs).ConfigureAwait(false);
                if (!found)
                {
                    var mbArgs = new MakeBucketArgs()
                        .WithBucket(bucketName);
                    await minio.MakeBucketAsync(mbArgs).ConfigureAwait(false);
                }
                var lstUrls = new List<MinIOImageModel>();
                // Lấy tất cả tên file trong thư mục
                var filesInFolder = Directory.GetFiles(filePath).Select(Path.GetFileName).ToList();

                // Lấy tất cả file ảnh có ký tự bắt đầu trùng với input
                files = filesInFolder.Where(x => x.StartsWith(files[0] + ".") || x.StartsWith(files[0] + "_")).ToList();
                foreach (var fileName in files)
                {
                    //var fileName = filesInFolder.Find(x => x.StartsWith(file));
                    //if (string.IsNullOrEmpty(fileName))
                    //{
                    //    continue;
                    //}
                    var objectName = $"{prefix}{fileName}";
                    string contentType;
                    new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType);
                    contentType = contentType ?? "application/octet-stream";

                    // Upload a file to bucket.
                    var putObjectArgs = new PutObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                        .WithFileName(filePath + fileName)
                        .WithContentType(contentType);
                    await minio.PutObjectAsync(putObjectArgs).ConfigureAwait(false);

                    //Lấy url với expired time = 1 days
                    PresignedGetObjectArgs args = new PresignedGetObjectArgs()
                                      .WithBucket(bucketName)
                                      .WithObject(objectName)
                                      .WithExpiry(60 * 60 * 24);
                    var url = await minio.PresignedGetObjectAsync(args);
                    lstUrls.Add(new MinIOImageModel()
                    {
                        Url = url,
                        BucketName = bucketName,
                        ObjectName = objectName
                    });
                }
                return lstUrls;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
                //_logger.LogError(ex, ex.Message);
                return new List<MinIOImageModel>();
            }
        }

        public static async Task<bool> Remove(List<MinIOImageModel> lstImages, UploadFileModel uploadConfig)
        {
            try
            {
                if (lstImages == null)
                {
                    return false;
                }

                var config = uploadConfig != null ? uploadConfig : GetDefaultConfig();

                using var minio = new MinioClient()
                            .WithEndpoint(config.EndPoint)
                            .WithCredentials(config.AccessKey, config.SecretKey)
                            .WithSSL(config.IsSSL)
                            .Build();

                foreach (var img in lstImages)
                {
                    var args = new RemoveObjectArgs()
                            .WithBucket(config.BucketName)
                            .WithObject(img.ObjectName);

                    await minio.RemoveObjectAsync(args).ConfigureAwait(false);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static async Task<List<MinIOImageModel>> GetUrl(List<MinIOImageModel> lstImages, UploadFileModel uploadConfig, int mediaType = 1)
        {
            try
            {
                var config = uploadConfig != null ? uploadConfig : GetDefaultConfig();

                using var minio = new MinioClient()
                            .WithEndpoint(config.EndPoint)
                            .WithCredentials(config.AccessKey, config.SecretKey)
                            .WithSSL(config.IsSSL)
                            .Build();

                foreach (var img in lstImages)
                {
                    if (!string.IsNullOrEmpty(StaticVariable.DomainMinioProxy))
                    {
                        if (mediaType == NotifyMediaType.Image.GetHashCode())
                        {
                            var memStream = new MemoryStream();

                            GetObjectArgs args = new GetObjectArgs()
                                          .WithBucket(img.BucketName)
                                          .WithObject(img.ObjectName)
                                          .WithCallbackStream((stream) => { stream.CopyTo(memStream); })
                            ;

                            await minio.GetObjectAsync(args);

                            img.Url = $"data:image/png;base64,{Convert.ToBase64String(memStream.ToArray())}";
                        }
                        else
                        {
                            //Lấy url với expired time = 1 days
                            PresignedGetObjectArgs args = new PresignedGetObjectArgs()
                                              .WithBucket(img.BucketName)
                                              .WithObject(img.ObjectName)
                                              .WithExpiry(60 * 60 * 24);

                            string url = await minio.PresignedGetObjectAsync(args);

                            img.Url = $"{StaticVariable.DomainMinioProxy}/get?link={HttpUtility.UrlEncode(url)}";
                        }
                    }
                    else
                    {
                        //Lấy url với expired time = 1 days
                        PresignedGetObjectArgs args = new PresignedGetObjectArgs()
                                          .WithBucket(img.BucketName)
                                          .WithObject(img.ObjectName)
                                          .WithExpiry(60 * 60 * 24);

                        img.Url = await minio.PresignedGetObjectAsync(args);
                    }
                }
                return lstImages;
            }
            catch (Exception ex)
            {
                return new List<MinIOImageModel>();
            }
        }

        private static UploadFileModel GetDefaultConfig()
        {
            var config = new UploadFileModel
            {
                EndPoint = StaticVariable.minIOConfig.EndPoint,
                Port = StaticVariable.minIOConfig.Port,
                AccessKey = StaticVariable.minIOConfig.AccessKey,
                SecretKey = StaticVariable.minIOConfig.SecretKey,
                BucketName = StaticVariable.minIOConfig.BucketName,
                IsSSL = StaticVariable.minIOConfig.IsSSL,
            };

            return config;
        }

        public static async Task<(MemoryStream? MemoryStream, ObjectStat? ObjectStat)> GetStream(string bucketName, string objectName, UploadFileModel uploadConfig = null)
        {
            try
            {
                var memStream = new MemoryStream();
                ObjectStat? objectStat = null;

                var config = uploadConfig != null ? uploadConfig : GetDefaultConfig();

                using var minio = new MinioClient()
                            .WithEndpoint(config.EndPoint)
                            .WithCredentials(config.AccessKey, config.SecretKey)
                            .WithSSL(config.IsSSL)
                            .Build();

                GetObjectArgs args = new GetObjectArgs()
                                      .WithBucket(bucketName)
                                      .WithObject(objectName)
                                      .WithCallbackStream((stream) => { stream.CopyTo(memStream); });

                objectStat = await minio.GetObjectAsync(args);

                return (memStream, objectStat);
            }
            catch (Exception ex)
            {
                return default;
            }
        }
    }
}
