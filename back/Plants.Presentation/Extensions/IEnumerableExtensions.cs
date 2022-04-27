using System.Collections.Generic;

namespace Plants.Presentation.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IList<IList<T>> Permutate<T>(this T[] nums)
        {
            var list = new List<IList<T>>();
            return DoPermute(nums, 0, nums.Length - 1, list);
        }

        static IList<IList<T>> DoPermute<T>(T[] nums, int start, int end, IList<IList<T>> list)
        {
            if (start == end)
            {
                // We have one of our possible n! solutions,
                // add it to the list.
                list.Add(new List<T>(nums));
            }
            else
            {
                for (var i = start; i <= end; i++)
                {
                    Swap(ref nums[start], ref nums[i]);
                    DoPermute(nums, start + 1, end, list);
                    Swap(ref nums[start], ref nums[i]);
                }
            }

            return list;
        }

        static void Swap<T>(ref T a, ref T b)
        {
            var temp = a;
            a = b;
            b = temp;
        }

    }
}
