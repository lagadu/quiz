
using QuizService.Model;

namespace QuizService.Controllers
{
    // Need an interface for dep. injection
    public interface IQuizDataAccess
    {
        QuizResponseModel GetQuizById(int id);
    }
}
