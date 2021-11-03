using System;
using WordTranslatorInc.Modules;
using Xunit;

namespace WordTranslatorInc.ModuleTests
{
    public class TranslatorFunctionalTests
    {
        [Fact]
        public void RegularInputTests()
        {
            string word = "One Thousand Three hundred forty two";
            int result = WordToIntTranslator.Translate(word);
            Assert.Equal(1342,result);

            word = "Seven million seven hundred seventy seven thousand seven hundred seventy seven";
            result = WordToIntTranslator.Translate(word);
            Assert.Equal(7777777, result);

            word = "One billion four hundred ninety eight million two hundred eleven thousand six hundred twenty seven";
            result = WordToIntTranslator.Translate(word);
            Assert.Equal(1498211627, result);
        }

        [Fact]
        public void EdgeCaseInputTests()
        {
            // Sparse Numbers
            string word = "One Thousand and one";
            int result = WordToIntTranslator.Translate(word);
            Assert.Equal(1001, result);

            word = "Seven million two hundred and six";
            result = WordToIntTranslator.Translate(word);
            Assert.Equal(7000206, result);

            // Single digit number
            word = "One";
            result = WordToIntTranslator.Translate(word);
            Assert.Equal(1, result);

            // Smallest supported number
            word = "zero";
            result = WordToIntTranslator.Translate(word);
            Assert.Equal(0, result);

            // Numbers with reapeated sequence of digits
            word = "Eleven million eleven thousand two hundred";
            result = WordToIntTranslator.Translate(word);
            Assert.Equal(11011200, result);

            word = "Two Hundred million two hundred thousand two hundred";
            result = WordToIntTranslator.Translate(word);
            Assert.Equal(200200200, result);

        }

        [Fact]
        public void InvalidInputTests()
        {
            // Incorrectly formed number words
            string word = "One zero zero one";
            Assert.Throws<InvalidOperationException>(()=>WordToIntTranslator.Translate(word));

            // Garbage string
            word = "doobie doobie doo";
            Assert.Throws<InvalidOperationException>(() => WordToIntTranslator.Translate(word));
            
            // Negative integers
            word = "Minus One hundred and twenty four";
            Assert.Throws<InvalidOperationException>(() => WordToIntTranslator.Translate(word));

            // Decimal values
            word = "zero point three seven";
            Assert.Throws<InvalidOperationException>(() => WordToIntTranslator.Translate(word));

            // Out of range value
            word = "Two Billion nine hundred million eleven thousand two hundred";
            Assert.Throws<InvalidOperationException>(() => WordToIntTranslator.Translate(word));

            // Incorrect format
            word = "Eleven hundred thousand eleven hundred twenty four";
            Assert.Throws<InvalidOperationException>(() => WordToIntTranslator.Translate(word));

        }

        [Fact]
        public void CreateItemTest()
        {
            string word = "One thousand two hundred and seventy four";
            int number = 1274;
            string userid = "testUser";

            WordToIntTranslator.WriteToDatabase(userid, word, number);

        }
    }
}
