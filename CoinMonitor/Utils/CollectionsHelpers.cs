using System.Collections.Generic;
using System;

namespace CoinMonitor.Utils
{
    public class CollectionsHelpers
    {
        public static List<List<string>> SplitList(List<string> requestParams, int chunkSize = 30)
        {
            var list = new List<List<string>>();

            for (var i = 0; i < requestParams.Count; i += chunkSize)
                list.Add(requestParams.GetRange(i, Math.Min(chunkSize, requestParams.Count - i)));

            return list;
        }
    }
}