using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.WpfControls.Utilities
{
    public class StringUtility
    {
        /// <summary>
        /// This function split a string on the basis of delimiters.
        /// </summary>
        /// <see cref="https://www.lucidchart.com/documents/edit/41d20574-d843-41ce-ae44-2d3e29fbc716/0_0?beaconFlowId=E9C38964142A86C1"/>
        /// <param name="s"></param>
        /// <param name="delimiters"></param>
        /// <returns></returns>
        public static IEnumerable<string> SplitAndKeep(string s, params string[] delimiters)
        {
            List<string> result = new List<string>();

            string data = string.Empty;
            string delimiteStr = string.Empty;

            foreach (var ch in s)
            {
                if (delimiters.Contains(ch + delimiteStr))
                {
                    if (data.Length > 0) result.Add(data);
                    data = string.Empty;
                    delimiteStr += ch;
                }
                else if (delimiteStr.Length > 0)
                {
                    result.Add(delimiteStr);

                    delimiteStr = string.Empty;
                    if (delimiters.Contains(ch.ToString())) delimiteStr = ch.ToString();
                    else data += ch;
                }
                else data += ch;
            }

            if (delimiteStr.Length > 0) result.Add(delimiteStr);
            if (data.Length > 0) result.Add(data);

            return result;
        }
    }
}
