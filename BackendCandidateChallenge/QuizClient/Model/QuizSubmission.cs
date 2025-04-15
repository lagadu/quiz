using System.Collections.Generic;

namespace QuizClient.Model
{
    public class QuizSubmission
    {
        public Dictionary<int, int> Answers { get; set; } = new Dictionary<int, int>();
    }
}
