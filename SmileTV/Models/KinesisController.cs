using Amazon;
using Amazon.Kinesis.Model;
using SmileTV.EventArguments;

namespace SmileTV.Models
{
    public class KinesisController
    {
        public static KinesisController Instance { get; set; }

        public event EventHandler<KinesisDataRecievedEventArgs> MessageRecieved;


        public Amazon.Kinesis.AmazonKinesisClient client = null;
        Consumer consumer = null;
        //System.Timers.Timer timer = null;

        bool consumerReady = false;

        public KinesisController()
        {
            client = new Amazon.Kinesis.AmazonKinesisClient(RegionEndpoint.EUWest2);

            GetData();
        }

        private async void Register()
        {
            consumer = await RegisterConsumer(client);

            await AwaitConsumer(client, consumer);
        }
        private async Task AwaitConsumer(Amazon.Kinesis.AmazonKinesisClient client, Consumer consumer)
        {
            DescribeStreamConsumerRequest request = new DescribeStreamConsumerRequest() { 
                ConsumerARN = consumer.ConsumerARN
            };

            DescribeStreamConsumerResponse response = await client.DescribeStreamConsumerAsync(request);

            while (response.ConsumerDescription.ConsumerStatus != Amazon.Kinesis.ConsumerStatus.ACTIVE)
            {
                Thread.Sleep(500);

                response = await client.DescribeStreamConsumerAsync(request);
            }
            
            consumerReady = true;
        }

        private async Task<Consumer> RegisterConsumer(Amazon.Kinesis.AmazonKinesisClient client)
        {
            RegisterStreamConsumerRequest request = new RegisterStreamConsumerRequest() {
                ConsumerName = "fan-out-consumer",
                StreamARN = "arn:aws:kinesis:eu-west-2:587376775772:stream/bens-stream"
            };

            RegisterStreamConsumerResponse response = await client.RegisterStreamConsumerAsync(request);

            return response.Consumer;
        }

        public async void GetData()
        {
            GetShardIteratorRequest req = new GetShardIteratorRequest();
            req.StreamName = "bens-stream";
            req.ShardId = "shardId-000000000001";
            req.ShardIteratorType = Amazon.Kinesis.ShardIteratorType.TRIM_HORIZON;
            
            GetShardIteratorResponse resp = await client.GetShardIteratorAsync(req);
            string iterator = resp.ShardIterator;

            while (true)
            {
                GetRecordsRequest recReq = new GetRecordsRequest();
                recReq.ShardIterator = iterator;

                GetRecordsResponse recResp = await client.GetRecordsAsync(recReq);

                List<Record> records = recResp.Records;

                string result = "From Kinesis: ";

                foreach (var record in records)
                {
                    StreamReader reader = new StreamReader(record.Data);
                    string data = reader.ReadToEnd();

                    result += data + ", ";
                }

                if (records.Count > 0)
                {
                    result = result.Substring(0, result.Length - 2) + "\n";
                    MessageRecieved?.Invoke(this, new KinesisDataRecievedEventArgs(result));
                }
                
                Thread.Sleep(200);
                iterator = recResp.NextShardIterator;
            }
        }
    }
}
