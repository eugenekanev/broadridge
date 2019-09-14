using Serilog;
using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace fileparser.engine
{
    public abstract class FileProcessor
    {
        private readonly int _writerThreshold;
        public FileProcessor(int WriterThreshold)
        {
            _writerThreshold = WriterThreshold;
        }


        private readonly ILogger Log = Serilog.Log.ForContext<FileProcessor>();
        async Task FillPipeAsync(Stream streamreader, PipeWriter writer)
        {

            while (true)
            {
                Memory<byte> memory = writer.GetMemory(1024);
                try
                {
                    int bytesRead = await streamreader.ReadAsync(memory, CancellationToken.None);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    writer.Advance(bytesRead);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error occured while reading from file");
                    break;
                }

                FlushResult result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    break;
                }
            }

            writer.Complete();
        }

        async Task ReadPipeAsync(PipeReader reader)
        {
            while (true)
            {
                ReadResult result = await reader.ReadAsync();

                ReadOnlySequence<byte> buffer = result.Buffer;
                buffer = ProcessByteBlock(buffer);
                reader.AdvanceTo(buffer.Start, buffer.End);
                if (result.IsCompleted)
                {
                    break;
                }
            }

            reader.Complete();
        }

        public async Task ProcessLinesAsync(string inputfilepath)
        {

            using (var inputStream = File.OpenRead(inputfilepath))
            {
                Log.Debug($"Starting processing the file with name : {inputfilepath}");
                var pipe = new Pipe(new PipeOptions (pauseWriterThreshold: _writerThreshold, resumeWriterThreshold: _writerThreshold/2));
                Task writing = FillPipeAsync(inputStream, pipe.Writer);
                Task reading = ReadPipeAsync(pipe.Reader);
                await Task.WhenAll(reading, writing);
                Log.Debug($"Finished processing the file with name : {inputfilepath}");
            }

               
        }

        protected abstract ReadOnlySequence<byte> ProcessByteBlock(ReadOnlySequence<byte> buffer);
        
    }
}
