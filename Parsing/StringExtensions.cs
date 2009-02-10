
namespace Flatiron.Parsing
{
    static class StringExtensions
    {
        // important that "\r\n" is first
        static string[] newlineTokens = { "\r\n", "\r", "\n" };

        internal static int GetNumLines(this string str)
        {
            int i = 0;
            int result = 0;
            bool foundNoTokens = false;

            while (i < str.Length && !foundNoTokens)
            {
                foundNoTokens = true;
                foreach (string t in newlineTokens)
                {
                    int j = str.IndexOf(t, i);
                    if (j != -1)
                    {
                        foundNoTokens = false;
                        i = j + t.Length;
                        result++;
                        break;
                    }
                }
            }
            return result;
        }
    }
}
