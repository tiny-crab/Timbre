using System;
using System.Linq;
using System.Collections.Generic;

public static class Utils {

    public static Dictionary<T, double> NormalizeDict<T>(Dictionary<T, double> dict, double scaleMin = 0, double scaleMax = 1) {
        var normalizedData = dict;

        var valueMax = dict.Values.Max();
        var valueMin = dict.Values.Min();
        var valueRange = valueMax - valueMin;
        var scaleRange = scaleMax - scaleMin;

        var buffer = dict.Values.Min();
        var ratio = 1f / dict.Values.Max();
        dict.Keys.ToList().ForEach(key => {
            var normalizedValue = valueRange != 0 ? ((scaleRange * (dict[key] - valueMin)) / valueRange) + scaleMin : scaleMax;
            normalizedData[key] = normalizedValue;
        });
        return normalizedData;
    }

    public static void Each<T>(this IEnumerable<T> ie, Action<T, int> action)
    {
        var i = 0;
        foreach (var e in ie) action(e, i++);
    }

    public static T RandomElement<T>(this IEnumerable<T> list) {
        var rnd = new System.Random();
        return list.OrderBy(i => rnd.Next()).First();
    }

    public static List<T> ManyRandomElements<T>(this IEnumerable<T> list, int number) {
        var rnd = new System.Random();
        return list.OrderBy(i => rnd.Next()).Take(number).ToList();
    }

}