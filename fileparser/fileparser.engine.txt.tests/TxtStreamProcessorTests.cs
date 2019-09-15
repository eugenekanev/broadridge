using fileparser.wordfrequencycaclculation;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace fileparser.engine.txt.tests
{
    public class TxtStreamProcessorTests
    {
        [Fact]
        public async Task ProcessLinesAsync_CorrectTxtFile_AppropriateDictionaryOfWordsIsCalculated()
        {
            //prepare
            var writerThreshold = 32768;
            var wordFrequencyCalculatorMock = new Mock<IWordFrequencyCalculator>();
            IDictionary<string, int> actualResult = null;
            wordFrequencyCalculatorMock.Setup(x => x.MergeInfo(It.IsAny<IDictionary<string, int>>()))
                .Callback((IDictionary<string, int> input) => {
                    if (input.Count != 0)
                    actualResult = new Dictionary<string, int>(input);
                });

            string testFileName = "testtext.txt";
            string testData = "Hello World world is beatiful\n Is that Right?\r\n";
            PrepareTestData(testData, testFileName);

            //act
            var txtProcessor = new TxtFileProcessor(wordFrequencyCalculatorMock.Object, writerThreshold);
            await txtProcessor.ProcessLinesAsync(testFileName);

            //check

            var expectedDictionary = new Dictionary<string, int>();
            expectedDictionary.Add("hello", 1);
            expectedDictionary.Add("world", 2);
            expectedDictionary.Add("is", 2);
            expectedDictionary.Add("beatiful", 1);
            expectedDictionary.Add("that", 1);
            expectedDictionary.Add("right?", 1);

            actualResult.Should().BeEquivalentTo(expectedDictionary);

        }



        private static void PrepareTestData(string text, string testTextFileName)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            File.WriteAllBytes(testTextFileName, Encoding.GetEncoding("Windows-1251").GetBytes(text));
        }
    }
}
