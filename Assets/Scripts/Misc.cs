using System;
using System.Collections.Generic;

public static class Misc
{
    // return randomised array of positions
    public static int[] GetPermArray(int n)
    {
        int[] arr = NewNaturalArray(n);

        Random rnd = new Random();

        for(int i = 0; i < n - 1; i++)
        {
            int j = rnd.Next(i, n);
            int temp = arr[i];
            arr[i] = arr[j];
            arr[j] = temp;
        }

        return arr;
    }

    public static int[] GetSortArray(int[] arr)
    {
        int[] indices = NewNaturalArray(arr.Length);

        Comparer<int> comparer = Comparer<int>.Default;
        Array.Sort(indices, (x, y) => comparer.Compare(arr[x], arr[y]));

        return indices;
    }

    // returns array in the form: [0, 1, 2, 3, ... length-1]
    public static int[] NewNaturalArray(int length)
    {
        int[] arr = new int[length];
        for(int i = 0; i < length; i++)
            arr[i] = i;
        return arr;
    }

    // actually working modulo function, (thanks microsoft)
    public static int Mod(int a, int n)
    {
        int r = a%n;
        return r<0 ? r+n : r;
    }

    public static Boolean RegexHelper(string text, string pattern, int options)
    {
        switch (options)
        {
            case 0:
                return System.Text.RegularExpressions.Regex.IsMatch(text, pattern, System.Text.RegularExpressions.RegexOptions.None);
            case 1:
                return System.Text.RegularExpressions.Regex.IsMatch(text, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            default:
                return false;
        }
    }
}
