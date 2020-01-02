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
            var normalizedValue = ((scaleRange * (dict[key] - valueMin)) / valueRange) + scaleMin;
            normalizedData[key] = normalizedValue;
        });
        return normalizedData;
    }

}