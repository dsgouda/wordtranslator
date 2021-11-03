using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Cosmos.Table;
using Azure.Data.Tables;

namespace WordTranslatorInc.Modules
{
    using Models;

    public static class WordToIntTranslator
    {
        static Dictionary<string, int> wordMap = new Dictionary<string, int>();

        static readonly string connString = string.Empty;

        static readonly string tableName = "translations";

        static TableClient tableClient;

        // Init the word to integer map
        static WordToIntTranslator()
        {
            wordMap.Add("one", 1);
            wordMap.Add("two", 2);
            wordMap.Add("three", 3);
            wordMap.Add("four", 4);
            wordMap.Add("five", 5);
            wordMap.Add("six", 6);
            wordMap.Add("seven", 7);
            wordMap.Add("eight", 8);
            wordMap.Add("nine", 9);
            wordMap.Add("ten", 10);
            wordMap.Add("eleven", 11);
            wordMap.Add("twelve", 12);
            wordMap.Add("thirteen", 13);
            wordMap.Add("fourteen", 14);
            wordMap.Add("fifteen", 15);
            wordMap.Add("sixteen", 16);
            wordMap.Add("seventeen", 17);
            wordMap.Add("eighteen", 18);
            wordMap.Add("nineteen", 19);
            wordMap.Add("twenty", 20);
            wordMap.Add("thirty", 30);
            wordMap.Add("forty", 40);
            wordMap.Add("fifty", 50);
            wordMap.Add("sixty", 60);
            wordMap.Add("seventy", 70);
            wordMap.Add("eighty", 80);
            wordMap.Add("ninety", 90);
            wordMap.Add("hundred", 100);
            wordMap.Add("thousand", 1000);
            wordMap.Add("million", 1000000);
            wordMap.Add("billion", 1000000000);

        }

        /// <summary>
        /// Translates word into a number
        /// </summary>
        /// <param name="word"></param>
        /// <returns>The translated number</returns>
        public static int Translate(string word)
        {
            // Empty string
            if (string.IsNullOrWhiteSpace(word))
            {
                throw new InvalidOperationException(string.Format("Word {0} cannot be translated to an int", word));
            }

            // Handle words in a case insesitive manner
            word = word.ToLower().Trim();

            // Decimal and negative values are not supported
            if (word.StartsWith("minus") || word.Contains("point"))
            {
                throw new InvalidOperationException(string.Format("Unsupported word {0}. Decimal values or negative integers are not supported", word));
            }

            // The smallest possible number can be a direct match
            if (word == "zero")
            {
                return 0;
            }

            // Split the word into individual parseable tokens
            var tokens = word.Split(" ").Select(w => w.Trim()).Where(w=>w!="and").Reverse().ToList();

            // The running sum we keep on adding to powers of 10
            int curr = 0;
            
            // The current integer token translated from wordMap
            int intToken = 0;

            // The current power of 10 
            int currPow = 0;

            // The index of the token fetched from word
            int i = 0;

            // Loop through tokens
            while(i<tokens.Count())
            {

                // If an invalid token found in string, number cannot be parsed
                if (!wordMap.ContainsKey(tokens[i]))
                {
                    throw new InvalidOperationException(string.Format("Encountered token {0} in word, cannot be translated to int", word));
                }

                intToken = wordMap[tokens[i]];

                // if IntToken<10, simply multiply the token with the current power of 10
                // and add the number parsed so far mod by current power of 10.
                // This is because the power of 10 will be added whenever
                // a power of ten token is encountered below. eg: (3*100)+42
                if (intToken < 10)
                {
                    curr = (intToken * (int)Math.Pow(10, currPow)) + ((currPow == 0) ? curr : (curr % (int)Math.Pow(10, currPow)));
                }
                // if IntToken<100, it's a number between 10 and 100, indicating a ten's place-like number (eg: 20 or 15000)
                // Simply multiply this with the current power of 10 and add the current number parsed so far
                else if (intToken < 100)
                {
                    curr = (intToken * (int)Math.Pow(10, currPow)) + curr;
                }
                // if IntToken==100, the "hundred" must be preceded by the value at that power of 10, multiply that to the 
                // power of 10 which is a multiple of 3
                else if (intToken == 100)
                {
                    if (wordMap[tokens[i + 1]] > 10)
                    {
                        throw new InvalidOperationException(string.Format("Word {0} is not in proper form, Use proper designations for digit places. For eg.: Eleven hundred must be One thousand one hundred", word));
                    }

                    curr += wordMap[tokens[i + 1]] * intToken * (int)Math.Pow(10, (currPow / 3) * 3);
                    i++;
                }
                // if IntToken>100, it indicates a power of 10, simply add current sum to this
                else
                {
                    curr += wordMap[tokens[i + 1]] * intToken;
                    if(wordMap[tokens[i+1]]>=10)
                    {
                        i++;
                    }
                }

                currPow = curr.ToString().Length - 1;

                // Handle out of range numbers
                if (currPow > 9)
                {
                    throw new InvalidOperationException(string.Format("Word {0} cannot be parsed as an int, the max size of ints is {1}", word, int.MaxValue));
                }
                else if (currPow == 9)
                {
                    int greatestDigit = intToken / ((int)Math.Pow(10, currPow));
                    if (greatestDigit>2)
                    {
                        throw new InvalidOperationException(string.Format("Word {0} cannot be parsed as an int, the max size of ints is {1}", word, int.MaxValue));
                    }
                    else if(greatestDigit==2)
                    {
                        if (curr % ((int)Math.Pow(10, currPow)) > 147483647)
                        {
                            throw new InvalidOperationException(string.Format("Word {0} cannot be parsed as an int, the max size of ints is {1}", word, int.MaxValue));
                        }
                    }
                }
                i++;
            }

            return curr;
        }

        /// <summary>
        /// Adds two numbers in word form
        /// </summary>
        /// <param name="word1"></param>
        /// <param name="word2"></param>
        /// <returns>The two words' sum</returns>
        public static int AddWords(string word1, string word2)
        {
            int a1 = WordToIntTranslator.Translate(word1);
            int a2 = WordToIntTranslator.Translate(word2);
            
            if (a1 + a2 > int.MaxValue)
            {
                throw new InvalidOperationException("Addition of words results in a number beyond valid range of integer");
            }
            
            return a1 + a2;
        }

        public static void WriteToDatabase(string userName, string word, int num)
        {
            if (tableClient == null)
            {
                InitializeTableClient();
            }

            tableClient.AddEntity(new Translation(userName, word, num));
            
        }

        private static void InitializeTableClient()
        {
            tableClient = new TableClient(connString, tableName);
        }
    }
}
