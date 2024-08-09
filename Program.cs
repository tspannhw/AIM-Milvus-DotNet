using Milvus.Client;

string Host = "192.168.1.153";
int Port = 19530; // This is Milvus's default port
bool UseSsl = false; // Default value is false

// See documentation for other constructor paramters
MilvusClient milvusClient = new MilvusClient(Host, Port, UseSsl);
MilvusHealthState result = await milvusClient.HealthAsync();

Console.WriteLine("Connected");

string collectionName = "book";
MilvusCollection collection = milvusClient.GetCollection(collectionName);

//Check if this collection exists
var hasCollection = await milvusClient.HasCollectionAsync(collectionName);

if(hasCollection){
    await collection.DropAsync();
    Console.WriteLine("Drop collection {0}",collectionName);
}

await milvusClient.CreateCollectionAsync(
            collectionName,
            new[] {
                FieldSchema.Create<long>("book_id", isPrimaryKey:true),
                FieldSchema.Create<long>("word_count"),
                FieldSchema.CreateVarchar("book_name", 256),
                FieldSchema.CreateFloatVector("book_intro", 2)
            }
        );

Random ran = new ();
List<long> bookIds = new ();
List<long> wordCounts = new ();
List<ReadOnlyMemory<float>> bookIntros = new ();
List<string> bookNames = new ();
for (long i = 0L; i < 2000; ++i)
{
    bookIds.Add(i);
    wordCounts.Add(i + 10000);
    bookNames.Add($"Book Name {i}");

    float[] vector = new float[2];
    for (int k = 0; k < 2; ++k)
    {
        vector[k] = ran.Next();
    }
    bookIntros.Add(vector);
}

MilvusCollection collection2 = milvusClient.GetCollection(collectionName);

MutationResult result2 = await collection2.InsertAsync(
    new FieldData[]
    {
        FieldData.Create("book_id", bookIds),
        FieldData.Create("word_count", wordCounts),
        FieldData.Create("book_name", bookNames),
        FieldData.CreateFloatVector("book_intro", bookIntros),
    });

// Check result
Console.WriteLine("Insert status: {0},", result2.ToString());
