
namespace Flatiron.Parsing
{
    static class StringExtensions
    {
        // important that "\r\n" is first
        static string[] newlineTokens = { "\r\n", "\r", "\n" };

        internal static int GetNumLines(this string str)
        {
            int index = 0;
            int result = 0;
            bool foundTokens = true;

            while (index < str.Length && foundTokens)
            {
                foreach (string t in newlineTokens)
                {
                    int j = str.IndexOf(t, index);
                    if (j != -1)
                    {
                        foundTokens = true;
                        index = j + t.Length;
                        result++;
                        break;
                    }
                    else
                        foundTokens = false;
                }
            }
            return result;
        }
    }
}
