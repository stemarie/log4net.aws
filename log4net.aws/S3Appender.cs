using Amazon.S3;
using Amazon.S3.Model;
using log4net.Appender.Language;
using log4net.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace log4net.Appender
{
    public class S3Appender : BufferingAppenderSkeleton
    {
        private string _bucketName;

        public string BucketName
        {
            get
            {
                if (String.IsNullOrEmpty(_bucketName))
                    throw new ApplicationException(Resource.BucketNameNotSpecified);
                return _bucketName;
            }
            set
            {
                _bucketName = value;
            }
        }

        private bool _createBucket = true;

        /// <summary>
        /// If true, checks whether the bucket already exists and if not creates it.
        /// If false, assumes that the bucket is already created and does not check.
        /// Set this to false if the AWS credentials used by the S3Appender do not
        /// have sufficient privileges to call ListBuckets() or PutBucket()
        /// </summary>
        public bool CreateBucket
        {
            get
            {
                return _createBucket;
            }
            set
            {
                _createBucket = value;
            }
        }

        internal AmazonS3Client Client { get; private set; }

        public override void ActivateOptions()
        {
            base.ActivateOptions();

            Client = new AmazonS3Client(Amazon.RegionEndpoint.USWest2);

            if (CreateBucket)
            {
                InitializeBucket();
            }
        }

        public AmazonS3Client InitializeBucket()
        {
            ListBucketsResponse response = Client.ListBuckets();
            bool found = response.Buckets.Any(bucket => bucket.BucketName == BucketName);

            if (found == false)
            {
                Client.PutBucket(new PutBucketRequest() { BucketName = BucketName });
            }
            return Client;
        }

        /// <summary>
        /// Sends the events.
        /// </summary>
        /// <param name="events">The events that need to be send.</param>
        /// <remarks>
        /// <para>
        /// The subclass must override this method to process the buffered events.
        /// </para>
        /// </remarks>
        protected override void SendBuffer(LoggingEvent[] events)
        {
            var fileAppender = new log4net.Appender.FileAppender(base.Layout, @"C:\Users\dmenezes\Desktop\Logs\teste.txt", true);

            Parallel.ForEach(events, l => fileAppender.Writer.WriteLine(l.RenderedMessage));
            //Parallel.ForEach(events, l => UploadEvent(l, Client));
        }

        private void UploadEvent(LoggingEvent loggingEvent, AmazonS3Client client)
        {
            string key = Guid.NewGuid().ToString();
            var xml = Utility.GetXmlString(loggingEvent);

            PutObjectRequest request = new PutObjectRequest();            
            request.BucketName = _bucketName;
            request.Key = Filename(key);
            request.ContentBody = xml;
            client.PutObject(request);

            // log.txt
            // log.1.txt
        }

        private static string Filename(string key)
        {
            return string.Format("{0}.log4net.xml", key);
        }
    }
}
