using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Models;
using QuizClient.Model;
using Xunit;

namespace QuizClient.Tests;

public class QuizClientTests : IClassFixture<QuizServiceApiPact>
{
    private readonly IMockProviderService _mockProviderService;
    private readonly Uri _mockProviderServiceBaseUri;
    private static readonly HttpClient Client = new HttpClient();

    public QuizClientTests(QuizServiceApiPact data)
    {
        _mockProviderService = data.MockProviderService;
        _mockProviderService.ClearInteractions();
        _mockProviderServiceBaseUri = data.MockProviderServiceBaseUri;
    }

    [Fact]
    public async Task GetQuizzes_WhenSomeQuizzesExists_ReturnsTheQuizzes()
    {
        _mockProviderService
            .Given("There are some quizzes")
            .UponReceiving("A GET request to retrieve the quizzes")
            .With(new ProviderServiceRequest
            {
                Method = HttpVerb.Get,
                Path = "/api/quizzes",
                Headers = new Dictionary<string, object>
                {
                    { "Accept", "application/json" }
                }
            })
            .WillRespondWith(new ProviderServiceResponse
            {
                Status = 200,
                Headers = new Dictionary<string, object>
                {
                    { "Content-Type", "application/json; charset=utf-8" }
                },
                Body = new[]
                {
                    new
                    {
                        id = 123,
                        title = "This is quiz 123"
                    },
                    new {
                        id = 124,
                        title = "This is quiz 124"
                    }
                }
            });

        var consumer = new QuizClient(_mockProviderServiceBaseUri, Client);

        var result = await consumer.GetQuizzesAsync(CancellationToken.None);
        Assert.True(string.IsNullOrEmpty(result.ErrorMessage), result.ErrorMessage);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotEmpty(result.Value);
        Assert.Equal(2, result.Value.Count());

        _mockProviderService.VerifyInteractions();
    }

    [Fact]
    public async Task GetQuiz_WhenAQuizWExists_ReturnsTheQuiz()
    {
        _mockProviderService
            .Given("There are some quizzes")
            .UponReceiving("A GET request to retrieve the quiz")
            .With(new ProviderServiceRequest
            {
                Method = HttpVerb.Get,
                Path = "/api/quizzes/123",
                Headers = new Dictionary<string, object>
                {
                    { "Accept", "application/json" }
                }
            })
            .WillRespondWith(new ProviderServiceResponse
            {
                Status = 200,
                Headers = new Dictionary<string, object>
                {
                    { "Content-Type", "application/json; charset=utf-8" }
                },
                Body = new
                {
                    id = 123,
                    title = "This is quiz 123"
                }
            });

        var consumer = new QuizClient(_mockProviderServiceBaseUri, Client);

        var result = await consumer.GetQuizAsync(123, CancellationToken.None);
        Assert.True(string.IsNullOrEmpty(result.ErrorMessage), result.ErrorMessage);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotEqual(Quiz.NotFound, result.Value);
        Assert.Equal("This is quiz 123", result.Value.Title);

        _mockProviderService.VerifyInteractions();
    }

    [Fact]
    public async Task PostQuiz_Returns201CreatedAndLocationHeader()
    {
        _mockProviderService
            .Given("There are some quizzes")
            .UponReceiving("A POST quiz request")
            .With(new ProviderServiceRequest
            {
                Method = HttpVerb.Post,
                Path = "/api/quizzes",
                Headers = new Dictionary<string, object>
                {
                    { "Content-Type", "application/json" }
                }
            })
            .WillRespondWith(new ProviderServiceResponse
            {
                Status = 201,
                Headers = new Dictionary<string, object>
                {
                    { "Content-Type", "application/json" },
                    { "Location", PactNet.Matchers.Match.Regex("/api/quizzes/1", "quizzes\\/[0-9]*") }
                }
            });

        var consumer = new QuizClient(_mockProviderServiceBaseUri, Client);

        var result = await consumer.PostQuizAsync(new Quiz { Title = "This is quiz 999" }, CancellationToken.None);
        Assert.True(string.IsNullOrEmpty(result.ErrorMessage), result.ErrorMessage);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        Assert.NotNull(result.Value);

        _mockProviderService.VerifyInteractions();
    }

    [Fact]
    public async Task PostQuestion_Returns201CreatedAndLocationHeader()
    {
        _mockProviderService
            .Given("There are some quizzes")
            .UponReceiving("A POST request to quiz 123 questions collection")
            .With(new ProviderServiceRequest
            {
                Method = HttpVerb.Post,
                Path = "/api/quizzes/123/questions",
                Headers = new Dictionary<string, object>
                {
                    { "Content-Type", "application/json" }
                }
            })
            .WillRespondWith(new ProviderServiceResponse
            {
                Status = 201,
                Headers = new Dictionary<string, object>
                {
                    { "Content-Type", "application/json" },
                    { "Location", PactNet.Matchers.Match.Regex("/api/quizzes/123/questions/1", "quizzes\\/123\\/questions\\/[0-9]*") }
                }
            });

        var consumer = new QuizClient(_mockProviderServiceBaseUri, Client);

        var result = await consumer.PostQuestionAsync(123, new QuizQuestion { Text = "This is a question" }, CancellationToken.None);
        Assert.True(string.IsNullOrEmpty(result.ErrorMessage), result.ErrorMessage);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        Assert.NotNull(result.Value);

        _mockProviderService.VerifyInteractions();
    }

    [Fact]
    public async Task PutQuestion_WhenAQuestionWExists_UpdatesTheQuestion()
    {
        _mockProviderService
            .Given("There are some quizzes")
            .UponReceiving("A PUT request to update a quiz question with id = 1")
            .With(new ProviderServiceRequest
            {
                Method = HttpVerb.Put,
                Path = "/api/quizzes/123/questions/1",
                Headers = new Dictionary<string, object>
                {
                    { "Content-Type", "application/json" }
                }
            })
            .WillRespondWith(new ProviderServiceResponse
            {
                Status = 204,
                Headers = new Dictionary<string, object>
                {
                    { "Content-Type", "application/json; charset=utf-8" }
                }
            });

        var consumer = new QuizClient(_mockProviderServiceBaseUri, Client);

        var result = await consumer.PutQuestionAsync(123, 1, new QuizQuestion { Text = "Updated text" }, CancellationToken.None);
        Assert.True(string.IsNullOrEmpty(result.ErrorMessage), result.ErrorMessage);
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        Assert.NotEqual(Quiz.NotFound, result.Value);

        _mockProviderService.VerifyInteractions();
    }
		
    [Fact]
    public async Task PostAnswers_Returns201CreatedAndLocationHeader()
    {
        _mockProviderService
            .Given("There are some quizzes")
            .UponReceiving("A POST request")
            .With(new ProviderServiceRequest
            {
                Method = HttpVerb.Post,
                Path = "/api/quizzes",
                Headers = new Dictionary<string, object>
                {
                    { "Content-Type", "application/json" }
                }
            })
            .WillRespondWith(new ProviderServiceResponse
            {
                Status = 201,
                Headers = new Dictionary<string, object>
                {
                    { "Content-Type", "application/json" },
                    { "Location", PactNet.Matchers.Match.Regex("/api/quizzes/1", "quizzes\\/[0-9]*") }
                },
                Body = new
                {
                    title = "This is quiz 999"
                }
            });

        var consumer = new QuizClient(_mockProviderServiceBaseUri, Client);

        var result = await consumer.PostQuizAsync(new Quiz { Title = "This is quiz 999" }, CancellationToken.None);
        Assert.True(string.IsNullOrEmpty(result.ErrorMessage), result.ErrorMessage);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        Assert.NotNull(result.Value);

        _mockProviderService.VerifyInteractions();
    }

    [Fact]
    public async Task GivenThatAQuizExistsPostingAnAnswerCreatesAQuizResponse()
    {
        _mockProviderService
            .Given("There is a quiz with id '123'")
            .UponReceiving("A POST request creates a quiz response")
            .With(new ProviderServiceRequest
            {
                Method = HttpVerb.Post,
                Path = "/api/quizzes/123/responses",
                Headers = new Dictionary<string, object>
                {
                    { "Content-Type", "application/json" }
                }
            })
            .WillRespondWith(new ProviderServiceResponse
            {
                Status = 201,
                Headers = new Dictionary<string, object>
                {
                    { "Content-Type", "application/json" },
                    { "Location", PactNet.Matchers.Match.Regex("/api/quizzes/123/responses/1", "responses\\/[0-9]*") }
                }
            });

        var consumer = new QuizClient(_mockProviderServiceBaseUri, Client);

        var result = await consumer.PostQuizResponseAsync(new QuestionResponse(),123);
        Assert.True(string.IsNullOrEmpty(result.ErrorMessage), result.ErrorMessage);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        Assert.NotNull(result.Value);

        _mockProviderService.VerifyInteractions();
    }

    //[Fact]
    //public async Task TakingQuiz_WithTwoQuestions_ReturnsCorrectScore()
    //{
    //    // create a quiz with minimum two questions as testdata for the test,
    //    // take the quiz and assert that you have the correct score based on number of correct answers.
    //    // 1 point per correct answer.
    //    _mockProviderService
    //        .Given("A quiz with id '123' exists and has two questions")
    //        .UponReceiving("A POST request to submit quiz responses with one correct answer")
    //        .With(new ProviderServiceRequest
    //        {
    //            Method = HttpVerb.Post,
    //            Path = "/api/quizzes/123/responses",
    //            Headers = new Dictionary<string, object>
    //            {
    //                    { "Content-Type", "application/json" }
    //            },

    //            Body = new
    //            {
    //                answers = new Dictionary<string, int>
    //                {
    //                        { "1", 10 },
    //                        { "2", 20 }
    //                }
    //            }
    //        })
    //        .WillRespondWith(new ProviderServiceResponse
    //        {
    //            Status = 200,
    //            Headers = new Dictionary<string, object>
    //            {
    //                    { "Content-Type", "application/json; charset=utf-8" }
    //            },
    //            Body = new
    //            {
    //                score = 1
    //            }
    //        });

    //    var submission = new QuizSubmission
    //    {
    //        Answers = new Dictionary<int, int>
    //            {
    //                { 1, 10 },   // For question 1, answer 10 is submitted (assume correct)
    //                { 2, 20 }    // For question 2, answer 20 is submitted (assume incorrect)
    //            }
    //    };
    //    var content = new StringContent(JsonConvert.SerializeObject(submission), Encoding.UTF8, "application/json");
    //    var response = await Client.PostAsync(new Uri(_mockProviderServiceBaseUri, "/api/quizzes/123/responses"), content);

    //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //    var responseBody = await response.Content.ReadAsStringAsync();
    //    var result = JsonConvert.DeserializeObject<QuizResult>(responseBody);
    //    Assert.Equal(1, result.Score);

    //    _mockProviderService.VerifyInteractions();
    //}
}