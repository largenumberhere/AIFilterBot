
using Newtonsoft.Json;
using OperationResult;
using static OperationResult.Helpers;
using System.IO.Compression;
using System.Text;

namespace AIFilterBot
{
    class DataWriter
    {
        public static void CreateDirectoryOnce(string pathToFile)
        {
            if (File.Exists(pathToFile)){ return; }

            if (!Path.Exists(pathToFile))
            {
                string directoryName = Path.GetDirectoryName(pathToFile);
                if (!Path.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                File.Create(pathToFile).Dispose();
            }

            return;
        }


        string _Path;
        private DataWriter(string path)
        {
            this._Path = path;
        }
        
        public static Result<DataWriter,FileNotFoundException> Create(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return Error(new FileNotFoundException(filePath));
            }

            DataWriter dataWriter = new DataWriter(filePath);
            return dataWriter;
        }





        /// <summary>
        /// load all data from file, overwriting any cached data. Optimised for quick reads from disk at expense of increased memory usage. Not optimal for huge files
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T?> Read<T>() {
            T? t;

            //load the file into memory
            using MemoryStream stream = new MemoryStream();
            byte[] bytes = await File.ReadAllBytesAsync(this._Path);
            await stream.WriteAsync(bytes, 0, bytes.Length);
            stream.Position = 0;
            await stream.FlushAsync();

            //process the data
            using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress))
            {
                using TextReader textReader = new StreamReader(deflateStream, encoding: Encoding.UTF8);
                //await Console.Out.WriteLineAsync("steam: '"+await textReader.ReadLineAsync()+"'");
                using JsonTextReader jsonTextReader = new JsonTextReader(textReader);

                JsonSerializer serializer = new JsonSerializer();
                t = serializer.Deserialize<T>(jsonTextReader);
/*
                if(t == null)
                {
                    throw new Exception("failed to deserialize");
                }*/
/*
                foreach (var item in typeof(T) .GetProperties()) {
                    string name = item.Name;    

                    //await Console.Out.WriteLineAsync(item.Name);
                    object? value = item.GetValue(t,);


                    if (item.GetValue(t) == null) {
                        throw new Exception(item.Name + " is null");
                    }
                }*/
            }

            //return the result
            return t;

        }
        
        /// <summary>
        /// overwrite file contents. Optimised for quick writes to disk at expense of memory usage. Not optimal for huge files
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public async Task Write<T>(T data,CancellationToken cancellationToken=default) {

            //prepare the data to write
            using MemoryStream memoryStream = new MemoryStream();
            using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionLevel.SmallestSize)) { 
                using TextWriter textWriter = new StreamWriter(deflateStream, Encoding.UTF8);
                using JsonWriter jsonWriter = new JsonTextWriter(textWriter);

                JsonSerializer jsonSerializer = new JsonSerializer();
                jsonSerializer.Serialize(jsonWriter, data);
                
            }
            
            //write the data
            await File.WriteAllBytesAsync(_Path, memoryStream.ToArray(),cancellationToken);
        }

        
        
    }



}
