using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TensorflowTrainingTools
{
    public class FeatureHelper
    {
        public static int Compare(string x, string y)
        {
            char[] X = x.ToCharArray();
            char[] Y = y.ToCharArray();
            for (int i = 0; i < X.Length && i < Y.Length; i++)
            {
                if (X[i] == Y[i])
                    continue;
                if ((X[i] == '0' || X[i] == '1' || X[i] == '2' || X[i] == '3' || X[i] == '4' || X[i] == '5' || X[i] == '6' || X[i] == '7' || X[i] == '8' || X[i] == '9') &&
                    (Y[i] == '0' || Y[i] == '1' || Y[i] == '2' || Y[i] == '3' || Y[i] == '4' || Y[i] == '5' || Y[i] == '6' || Y[i] == '7' || Y[i] == '8' || Y[i] == '9'))
                {
                    int NumberX = int.Parse(Regex.Match(x.Substring(i), @"^[0-9]+").Value);
                    int NumberY = int.Parse(Regex.Match(y.Substring(i), @"^[0-9]+").Value);
                    return NumberX - NumberY;
                }
                return X[i] - Y[i];
            }
            return 0;
        }
    }
}
