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
                Client = InitializeBucket();
            }
        }

        internal AmazonS3Client Client { get; private set; }

        public AmazonS3Client InitializeBucket()
        {
            AmazonS3Client client = new AmazonS3Client();
            ListBucketsResponse response = client.ListBuckets();
            bool found = response.Buckets.Any(bucket => bucket.BucketName == BucketName);

            if (found == false)
            {
                client.PutBucket(new PutBucketRequest().WithBucketName(BucketName));
            }
            return client;
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
            Parallel.ForEach(events, l => UploadEvent(l, Client));
        }

        private void UploadEvent(LoggingEvent loggingEvent, AmazonS3 client)
        {
            string key = Guid.NewGuid().ToString();
            var xml = Utility.GetXmlString(loggingEvent);

            PutObjectRequest request = new PutObjectRequest();
            request.WithBucketName(_bucketName);
            request.WithKey(Filename(key));
            request.WithContentBody(xml);
            client.PutObject(request);
        }

        private static string Filename(string key)
        {
            return string.Format("{0}.log4net.xml", key);
        }
    }
}
