using fileparser.wordfrequencycaclculation;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace fileparser.engine.txt
{
    public class TxtFileProcessor : FileProcessor, IFileProcessor
    {
        private readonly IWordFrequencyCalculator _wordFrequencyCalculator;
        private readonly IDictionary<string, int> _wordFrequency;
        private readonly Encoding _encoding;
        public TxtFileProcessor(IWordFrequencyCalculator wordFrequencyCalculator, int writerThreshold) : base(writerThreshold)
        {
            _wordFrequencyCalculator = wordFrequencyCalculator;
            _wordFrequency = new Dictionary<string, int>();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _encoding = Encoding.GetEncoding("Windows-1251");
        }

        protected override ReadOnlySequence<byte> ProcessByteBlock(ReadOnlySequence<byte> buffer)
        {
            SequencePosition? position = null;
            do
            {
                position = buffer.PositionOf((byte)'\n');

                if (position != null)
                {
                    ProcessLine(buffer.Slice(0, position.Value));
                    buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
                }
            }
            while (position != null);
            _wordFrequencyCalculator.MergeInfo(_wordFrequency);
            _wordFrequency.Clear();

            return buffer;
        }

        private void ProcessLine(ReadOnlySequence<byte> line)
        {
            var str = GetString(line);
            string[] wordsinline = str.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            foreach (string word in wordsinline)
            {
                var wordToCheck = word.Trim('\r').ToLowerInvariant();
                if (_wordFrequency.ContainsKey(wordToCheck))
                {
                    _wordFrequency[wordToCheck]++;
                }
                else
                {
                    _wordFrequency.Add(wordToCheck, 1);
                }
            }
        }

        string GetString(ReadOnlySequence<byte> buffer)
        {
            if (buffer.IsSingleSegment)
            {
                return _encoding.GetString(buffer.First.Span);
            }


            return string.Create((int)buffer.Length, buffer, (span, sequence) =>
            {
                foreach (var segment in sequence)
                {
                    _encoding.GetChars(segment.Span, span);


                    span = span.Slice(segment.Length);
                }
            });
        }
    }
}
