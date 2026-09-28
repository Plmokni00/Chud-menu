using System.Collections.Generic;
namespace Chud.UI
{
    public static class PageSlicing
    {
        public const int DefaultPageSize = 7;

        public static int PageCount(int count, int pageSize)
        {
            if (pageSize < 1)
            {
                pageSize = DefaultPageSize;
            }

            if (count <= 0)
            {
                return 1;
            }

            int pages = (count + pageSize - 1) / pageSize;
            return pages < 1 ? 1 : pages;
        }

        public static int Clamp(int page, int pageCount)
        {
            if (pageCount < 1)
            {
                pageCount = 1;
            }

            if (page < 0)
            {
                return 0;
            }

            return page > pageCount - 1 ? pageCount - 1 : page;
        }

        public static string Truncate(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return " ";
            }

            return text.Length <= 40 ? text : text.Substring(0, 39) + "…";
        }

        public static int FirstIndex(int page, int pageSize) => page * pageSize;

        public static int LastIndex(int first, int count, int pageSize)
        {
            int end = first + pageSize;
            return end > count ? count : end;
        }

        public static void Fill<T>(IReadOnlyList<T> source, int first, int last, List<T> destination)
        {
            destination.Clear();
            if (source == null)
            {
                return;
            }

            for (int i = first; i < last && i < source.Count; i++)
            {
                destination.Add(source[i]);
            }
        }
    }
}