using OperationResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIFilterBot
{
    internal class DataBuilder<T> where T : new()
    {
        string? _Path = null;
        
        public DataBuilder<T> SetPath(string path) {
            _Path = path;
            return this;
        }

        
        
        bool promptIfEmpty = false;
        public DataBuilder<T> PromptIfEmpty()
        {
            promptIfEmpty = true;
            return this;
        }

        bool createIfNotExists = false;
        public DataBuilder<T> CreateIfNotExists() {
            createIfNotExists = true;
            return this;
        }

        bool resetOnStart = false;
        public DataBuilder<T> Clear()
        {
            resetOnStart = true;
            return this;
        }

        

        public T Build() {

            Result<DataWriter,FileNotFoundException> dataWriterResult;

            if (!File.Exists(_Path))
            {
                throw new FileNotFoundException(_Path);
            } 

            dataWriterResult = DataWriter.Create(_Path);
            if(dataWriterResult.IsError)
            {

                if (this.createIfNotExists)
                {
                    DataWriter.CreateDirectoryOnce(_Path);
                    dataWriterResult = DataWriter.Create(_Path);

                    if (dataWriterResult.IsError)
                    {
                        throw new Exception("failed to create file");
                    }
                }

                else
                {
                    throw new Exception("file does not exist");
                }
            }

            DataWriter dataWriter = dataWriterResult.Value;
            T? data = dataWriter.Read<T>().GetAwaiter().GetResult();

            bool dataIsEmpty = data == null;
            

            if (dataIsEmpty | resetOnStart)
            {
                if (promptIfEmpty)
                {
                    T t = new();

                    foreach(var property in t.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                    {
                        if(property.PropertyType != typeof(string))
                        {
                            throw new Exception($"Cannot prompt for unhandled type: {property.PropertyType.FullName}");
                        }

                        Console.Write($"Please enter the value for '{property.Name}' and then hit enter: ");
                        string? newValue = Console.ReadLine();
                        property.SetValue(t, newValue);
                    }

                    dataWriter.Write(t).GetAwaiter().GetResult();
                    return dataWriter.Read<T>().GetAwaiter().GetResult();

                }
                else
                {
                    throw new Exception($"file was probably empty. failed to convert it to {nameof(T)}");
                }
            }

            else
            {



                return data;
            }
        }
    }
}
