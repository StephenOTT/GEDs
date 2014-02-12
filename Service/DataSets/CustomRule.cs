using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Entities.Models;

namespace Service.DataSets
{
    internal class CustomRule
    {
        public string LookupColumn { get; set; }
        public string RegularExpression { get; set; }
        public RuleActionType RuleAction { get; set; }
        public string ActionColumn { get; set; }
        public string ActionReplaceValue { get; set; }
        public string ActionReplaceColumn { get; set; } //replace with column value instead
    }

    internal class RuleMatch
    {
        public RuleMatch(int matchLocation, int matchLength)
        {
            Location = matchLocation;
            Lengh = matchLength;
        }

        public int Location { get; set; }
        public int Lengh { get; set; }
        public string Value { get; set; }
        public RuleMatch[] Groups { set; get; }
    }

    internal static class RuleMatcher
    {
        public static RuleMatch[] GroupsInRegexSet(string key)
        {
            RuleMatch[] ruleMatches = null;
            if (string.IsNullOrEmpty(key))
                return ruleMatches;

            Regex regex = new Regex(@"((?<!\\)\\([0-9]+))");

            MatchCollection matches = regex.Matches(key);

            if (matches.Count > 0)
            {
                ruleMatches = new RuleMatch[matches.Count];

                for (int x = 0; x < matches.Count; x++)
                {
                    Match match = matches[x];

                    RuleMatch ruleMatch = new RuleMatch(match.Index, match.Length);
                    ruleMatch.Value = match.Groups[2].Value; //digit
                    ruleMatches[x] = ruleMatch;
                }
            }

            return ruleMatches;
        }
    }
}
