using Newtonsoft.Json;

public class Program
{
    public static void Main()
    {
        string teamName = "Paris Saint-Germain";
        int year = 2013;
        int totalGoals = getTotalScoredGoals(teamName, year);

        Console.WriteLine("Team "+ teamName +" scored "+ totalGoals.ToString() + " goals in "+ year);

        teamName = "Chelsea";
        year = 2014;
        totalGoals = getTotalScoredGoals(teamName, year);

        Console.WriteLine("Team " + teamName + " scored " + totalGoals.ToString() + " goals in " + year);

        // Output expected:
        // Team Paris Saint - Germain scored 109 goals in 2013
        // Team Chelsea scored 92 goals in 2014
    }

    public static int getTotalScoredGoals(string team, int year)
    {
        int totalGoals = 0;
        totalGoals += GetGoalsBySide(team, year, "team1");
        totalGoals += GetGoalsBySide(team, year, "team2");
        return totalGoals;
    }

    private static int GetGoalsBySide(string team, int year, string side)
    {
        int page = 1;
        int goals = 0;

        using (var client = new HttpClient())
        {
            while (true)
            {
                string url = $"https://jsonmock.hackerrank.com/api/football_matches?year={year}&{side}={Uri.EscapeDataString(team)}&page={page}";
                var response = client.GetAsync(url).Result;
                var content = response.Content.ReadAsStringAsync().Result;

                var result = JsonConvert.DeserializeObject<ApiResponse>(content);

                if (result.data == null || result.data.Count == 0)
                    break;

                foreach (var match in result.data)
                {
                    if (side == "team1")
                        goals += int.Parse(match.team1goals);
                    else
                        goals += int.Parse(match.team2goals);
                }

                if (page >= result.total_pages)
                    break;

                page++;
            }
        }

        return goals;
    }

    public class Match
    {
        public string team1 { get; set; }
        public string team2 { get; set; }
        public string team1goals { get; set; }
        public string team2goals { get; set; }
    }

    public class ApiResponse
    {
        public int page { get; set; }
        public int per_page { get; set; }
        public int total { get; set; }
        public int total_pages { get; set; }
        public List<Match> data { get; set; }
    }

}