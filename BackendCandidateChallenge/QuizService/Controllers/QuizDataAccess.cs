using Dapper;
using QuizService.Model;
using QuizService.Model.Domain;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace QuizService.Controllers
{
    public class QuizDataAccess : IQuizDataAccess
    {
        private readonly IDbConnection _connection;

        public QuizDataAccess(IDbConnection connection)
        {
            _connection = connection;
        }

        public QuizResponseModel GetQuizById(int id)
        {
            const string quizSql = "SELECT * FROM Quiz WHERE Id = @Id;";

            // had to change this to QuerySingleOrDefault in order for tests to pass, otherwise exception was thrown, related to task 2
            var quiz = _connection.QuerySingleOrDefault<Quiz>(quizSql, new { Id = id });
            if (quiz == null)
            {
                return null;
            }
            const string questionsSql = "SELECT * FROM Question WHERE QuizId = @QuizId;";
            var questions = _connection.Query<Question>(questionsSql, new { QuizId = id });
            const string answersSql = "SELECT a.Id, a.Text, a.QuestionId FROM Answer a INNER JOIN Question q ON a.QuestionId = q.Id WHERE q.QuizId = @QuizId;";
            var answers = _connection.Query<Answer>(answersSql, new { QuizId = id })
            .Aggregate(new Dictionary<int, IList<Answer>>(), (dict, answer) => {
                if (!dict.ContainsKey(answer.QuestionId))
                    dict.Add(answer.QuestionId, new List<Answer>());
                dict[answer.QuestionId].Add(answer);
                return dict;
            });
            return new QuizResponseModel
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Questions = questions.Select(question => new QuizResponseModel.QuestionItem
                {
                    Id = question.Id,
                    Text = question.Text,
                    Answers = answers.ContainsKey(question.Id)
                        ? answers[question.Id].Select(answer => new QuizResponseModel.AnswerItem
                        {
                            Id = answer.Id,
                            Text = answer.Text
                        })
                        : new QuizResponseModel.AnswerItem[0],
                    CorrectAnswerId = question.CorrectAnswerId
                }),
                Links = new Dictionary<string, string>
            {
                {"self", $"/api/quizzes/{id}"},
                {"questions", $"/api/quizzes/{id}/questions"}
            }
            };
        }
    }
}
