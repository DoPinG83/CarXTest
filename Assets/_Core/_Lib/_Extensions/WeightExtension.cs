namespace Core.Common.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public static class WeightExtension
    {
        public static IWeighted GetRandomWeight(this IEnumerable<IWeighted> itemsWithWeight)
        {
            var weightTotal = itemsWithWeight.Sum(i => i.Weight);
            var weightRoll = Random.Range(0, weightTotal);
            var index = -1;
            while (weightRoll > -1)
            {
                index++;
                weightRoll -= itemsWithWeight.ElementAt(index).Weight;
            }
            return itemsWithWeight.ElementAt(index);
        }

        public static IWeighted GetRandomWeight(this IWeighted[] itemsWithWeight)
        {
            var weightTotal = itemsWithWeight.Sum(i => i.Weight);
            var weightRoll = Random.Range(0, weightTotal);
            var index = -1;
            while (weightRoll > -1)
            {
                index++;
                weightRoll -= itemsWithWeight[index].Weight;
            }
            return itemsWithWeight[index];
        }
    }
}