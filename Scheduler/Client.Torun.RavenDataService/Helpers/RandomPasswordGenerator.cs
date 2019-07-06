using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Client.Torun.RavenDataService.Helpers
{
    public static class RandomPasswordGenerator
    {
        public static string GeneratePassword(int passwordLength)
        {
            StringBuilder stringBuilder = new StringBuilder();

            var random = new Random();

            var ranges = new [,] { { 65, 91 }, { 97, 123 }, { 48, 58 } }; 

            for (int i = 0; i < passwordLength; i++)
            {
                // Wybieramy zakres ASCII (A-Z, a-z, 0-9)

                int row = random.Next(3);
                int[] range = new int [2];

                for (int j = 0; j < 2; j++)
                {
                    range[j] = ranges[row, j];
                }

                // Wybiermy znak z zakresu

                int asciiDecimalCode = random.Next(range[0], range[1]);

                // konwerujemy na 'char'

                char sign = Convert.ToChar(asciiDecimalCode);

                stringBuilder.Append(sign);


            }

            return stringBuilder.ToString();
            

        }

        
    }
}
