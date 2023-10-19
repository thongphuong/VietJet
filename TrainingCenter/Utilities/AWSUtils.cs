using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using Utilities;

namespace TrainingCenter.Utilities
{

    public static class AWSUtils
    {
        #region AWS S3
        //AWS
        public static string bucketName { get { return AppUtils.getAppSetting("AWSBucket"); } }
        //Put file to S3
        public static void AWS_DeleteFile(string path)
        {
            var region = ConfigurationManager.AppSettings["AWSRegion"];
            var client = new AmazonS3Client();
            switch (region)
            {
                case "USEast1": client = new AmazonS3Client(Amazon.RegionEndpoint.USEast1); break;
                case "MESouth1": client = new AmazonS3Client(Amazon.RegionEndpoint.MESouth1); break;
                case "CACentral1": client = new AmazonS3Client(Amazon.RegionEndpoint.CACentral1); break;
                case "CNNorthWest1": client = new AmazonS3Client(Amazon.RegionEndpoint.CNNorthWest1); break;
                case "CNNorth1": client = new AmazonS3Client(Amazon.RegionEndpoint.CNNorth1); break;
                case "USGovCloudWest1": client = new AmazonS3Client(Amazon.RegionEndpoint.USGovCloudWest1); break;
                case "USGovCloudEast1": client = new AmazonS3Client(Amazon.RegionEndpoint.USGovCloudEast1); break;
                case "SAEast1": client = new AmazonS3Client(Amazon.RegionEndpoint.SAEast1); break;
                case "APSoutheast1": client = new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast1); break;
                case "APSouth1": client = new AmazonS3Client(Amazon.RegionEndpoint.APSouth1); break;
                case "APNortheast3": client = new AmazonS3Client(Amazon.RegionEndpoint.APNortheast3); break;
                case "APSoutheast2": client = new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast2); break;
                case "APNortheast1": client = new AmazonS3Client(Amazon.RegionEndpoint.APNortheast1); break;
                case "USEast2": client = new AmazonS3Client(Amazon.RegionEndpoint.USEast2); break;
                case "APNortheast2": client = new AmazonS3Client(Amazon.RegionEndpoint.APNortheast2); break;
                case "USWest2": client = new AmazonS3Client(Amazon.RegionEndpoint.USWest2); break;
                case "EUNorth1": client = new AmazonS3Client(Amazon.RegionEndpoint.EUNorth1); break;
                case "EUWest1": client = new AmazonS3Client(Amazon.RegionEndpoint.EUWest1); break;
                case "USWest1": client = new AmazonS3Client(Amazon.RegionEndpoint.USWest1); break;
                case "EUWest3": client = new AmazonS3Client(Amazon.RegionEndpoint.EUWest3); break;
                case "EUCentral1": client = new AmazonS3Client(Amazon.RegionEndpoint.EUCentral1); break;
                case "APEast1": client = new AmazonS3Client(Amazon.RegionEndpoint.APEast1); break;
                case "EUWest2": client = new AmazonS3Client(Amazon.RegionEndpoint.EUWest2); break;
                default: client = new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast1); break;
            }
            var findFolderRequest = new ListObjectsRequest();
            findFolderRequest.BucketName = bucketName;
            findFolderRequest.Prefix = path;
            var findFolderResponse = client.ListObjects(findFolderRequest).S3Objects.Where(a => a.Size > 0);
            foreach (var file in findFolderResponse)
            {
                    var request2 = new DeleteObjectRequest
                    {
                        BucketName = bucketName,
                        Key = file.Key
                    };
                    var response2 = client.DeleteObjectAsync(request2);
            }
        }
        public static void AWS_PutObject(string pathFile, System.IO.Stream inputStream)
        {
            var region = ConfigurationManager.AppSettings["AWSRegion"];
            var client = new AmazonS3Client();
            switch (region)
            {
                case "USEast1": client = new AmazonS3Client(Amazon.RegionEndpoint.USEast1); break;
                    case "MESouth1": client = new AmazonS3Client(Amazon.RegionEndpoint.MESouth1); break;
                    case "CACentral1": client = new AmazonS3Client(Amazon.RegionEndpoint.CACentral1); break;
                    case "CNNorthWest1": client = new AmazonS3Client(Amazon.RegionEndpoint.CNNorthWest1); break;
                    case "CNNorth1": client = new AmazonS3Client(Amazon.RegionEndpoint.CNNorth1); break;
                    case "USGovCloudWest1": client = new AmazonS3Client(Amazon.RegionEndpoint.USGovCloudWest1); break;
                    case "USGovCloudEast1": client = new AmazonS3Client(Amazon.RegionEndpoint.USGovCloudEast1); break;
                    case "SAEast1": client = new AmazonS3Client(Amazon.RegionEndpoint.SAEast1); break;
                    case "APSoutheast1": client = new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast1); break;
                    case "APSouth1": client = new AmazonS3Client(Amazon.RegionEndpoint.APSouth1); break;
                    case "APNortheast3": client = new AmazonS3Client(Amazon.RegionEndpoint.APNortheast3); break;
                    case "APSoutheast2": client = new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast2); break;
                    case "APNortheast1": client = new AmazonS3Client(Amazon.RegionEndpoint.APNortheast1); break;
                    case "USEast2": client = new AmazonS3Client(Amazon.RegionEndpoint.USEast2); break;
                    case "APNortheast2": client = new AmazonS3Client(Amazon.RegionEndpoint.APNortheast2); break;
                    case "USWest2": client = new AmazonS3Client(Amazon.RegionEndpoint.USWest2); break;
                    case "EUNorth1": client = new AmazonS3Client(Amazon.RegionEndpoint.EUNorth1); break;
                    case "EUWest1": client = new AmazonS3Client(Amazon.RegionEndpoint.EUWest1); break;
                    case "USWest1": client = new AmazonS3Client(Amazon.RegionEndpoint.USWest1); break;
                    case "EUWest3": client = new AmazonS3Client(Amazon.RegionEndpoint.EUWest3); break;
                    case "EUCentral1": client = new AmazonS3Client(Amazon.RegionEndpoint.EUCentral1); break;
                    case "APEast1": client = new AmazonS3Client(Amazon.RegionEndpoint.APEast1); break;
                    case "EUWest2": client = new AmazonS3Client(Amazon.RegionEndpoint.EUWest2); break;
                default: client = new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast1); break;
            }


            try
            {
                var folderRequest = new PutObjectRequest();
                folderRequest.BucketName = bucketName;
                folderRequest.Key = pathFile;
                //folderRequest.FilePath = pathclient;
                folderRequest.InputStream = inputStream;
                var folderResponse = client.PutObject(folderRequest);

            }
            catch (AmazonS3Exception e)
            {

            }

        }
        //Check Folder Exist
        public static bool AWS_CheckFolderExists(string pathFolder)
        {
            var client = new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast1);
            try
            {
                var findFolderRequest = new ListObjectsRequest();
                findFolderRequest.BucketName = bucketName;
                findFolderRequest.Delimiter = "/";
                findFolderRequest.Prefix = pathFolder;
                var findFolderResponse = client.ListObjects(findFolderRequest);
                var commonPrefixes = findFolderResponse.CommonPrefixes;
                var folderExists = commonPrefixes.Count > 0 || findFolderResponse.S3Objects.Any();
                if (folderExists)
                {
                    return true;
                }

                return false;
            }
            catch (AmazonS3Exception e)
            {
                return false;
            }
        }

        //Create Folder
        public static void AWS_RunFolderCreationDemo(string path)
        {
            var client = new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast1);
            try
            {
                PutObjectRequest folderRequest = new PutObjectRequest();
                folderRequest.BucketName = bucketName;
                folderRequest.Key = path;
                folderRequest.InputStream = new MemoryStream(new byte[0]);
                PutObjectResponse folderResponse = client.PutObject(folderRequest);
            }
            catch (AmazonS3Exception e)
            {
            }

        }
        #endregion
    }
}