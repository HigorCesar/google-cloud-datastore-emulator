using System;
using Xunit;
using Google.Cloud.Datastore.Emulator;
using Google.Cloud.Datastore.V1;
using Grpc.Core;
using System.Threading.Tasks;
using Google.Api.Gax.Grpc;

namespace Google.Cloud.Datastore.Emulator.Tests
{
    public class DatastoreEmulatorTests
    {
        [Fact]
        public async Task StartEmulator_and_run_command()
        {
            var emulator = new Core.DataStoreEmulator();
            var emulatorOutput = emulator.Start();

            // Instantiates a client
            var testDatastoreClient = DatastoreClient.Create(new Channel("localhost", emulatorOutput.Port, ChannelCredentials.Insecure));
            var db = DatastoreDb.Create("travix-com", "", testDatastoreClient);

            var keyFactory = db.CreateKeyFactory("Task");
            var key = keyFactory.CreateKey("sampletask1");

            var task = new Entity { Key = key, ["description"] = "Buy milk" };
            using (DatastoreTransaction transaction = db.BeginTransaction())
            {
                transaction.Upsert(task);
                transaction.Commit();
            }
            var response = await db.LookupAsync(key, null);
            Assert.Equal("Buy milk",response["description"].StringValue);
            emulator.Stop();
        }
    }
}
