using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Models;
using QuizClient.Model;
using QuizService.Controllers;
using QuizService.Model;
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

    private int ParseId(Uri location)
    {
        var segments = location.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries);
        return int.Parse(segments[segments.Length - 1]);
    }

    [Fact]
    public async Task TakingQuiz_WithTwoQuestions_ReturnsCorrectScore()
    {
        var quizClient = new QuizClient(_mockProviderServiceBaseUri, Client);
        var quizCreate = new QuizCreateModel("Score Quiz");
        var postQuizResponse = await quizClient.PostQuizAsync(new Quiz { Title = quizCreate.Title }, CancellationToken.None);
        Assert.Equal(HttpStatusCode.Created, postQuizResponse.StatusCode);
        int quizId = ParseId(postQuizResponse.Value);

        // questrion 1
        var question1Create = new QuestionCreateModel("question 1");
        var postQ1Response = await quizClient.PostQuestionAsync(quizId, new QuizQuestion { Text = question1Create.Text }, CancellationToken.None);
        Assert.Equal(HttpStatusCode.Created, postQ1Response.StatusCode);
        int question1Id = ParseId(postQ1Response.Value);

        var answer1Correct = new Answer() { Id = 1, QuestionId = 1, Text = "a1correct" };
        var postAnswer1CorrectResponse = await quizClient.PostAnswerAsync(quizId, question1Id, answer1Correct, CancellationToken.None);
        Assert.Equal(HttpStatusCode.Created, postAnswer1CorrectResponse.StatusCode);
        int answer1CorrectId = ParseId(postAnswer1CorrectResponse.Value);

        var answer1Incorrect = new Answer() { Id = 2, QuestionId = 1, Text = "a1wrong" };
        var postAnswer1IncorrectResponse = await quizClient.PostAnswerAsync(quizId, question1Id, answer1Incorrect, CancellationToken.None);
        Assert.Equal(HttpStatusCode.Created, postAnswer1IncorrectResponse.StatusCode);
        int answer1IncorrectId = ParseId(postAnswer1IncorrectResponse.Value);

        var question1Update = new QuestionUpdateModel { Text = question1Create.Text, CorrectAnswerId = answer1CorrectId };
        var putQ1Response = await quizClient.PutQuestionAsync(quizId, question1Id, new QuizQuestion { Text = question1Update.Text }, CancellationToken.None);
        Assert.Equal(HttpStatusCode.NoContent, putQ1Response.StatusCode);

        // question 2
        var question2Create = new QuestionCreateModel("question 2");
        var postQ2Response = await quizClient.PostQuestionAsync(quizId, new QuizQuestion { Text = question2Create.Text }, CancellationToken.None);
        Assert.Equal(HttpStatusCode.Created, postQ2Response.StatusCode);
        int question2Id = ParseId(postQ2Response.Value);

        var answer2Incorrect = new Answer() { Id = 3, QuestionId = 2, Text = "a2wrong" };
        var postAnswer2CorrectResponse = await quizClient.PostAnswerAsync(quizId, question2Id, answer2Incorrect, CancellationToken.None);
        Assert.Equal(HttpStatusCode.Created, postAnswer2CorrectResponse.StatusCode);
        int answer2CorrectId = ParseId(postAnswer2CorrectResponse.Value);

        var answer2Correct = new Answer() { Id = 4, QuestionId = 2, Text = "a2correct" };
        var postAnswer2IncorrectResponse = await quizClient.PostAnswerAsync(quizId, question2Id, answer2Correct, CancellationToken.None);
        Assert.Equal(HttpStatusCode.Created, postAnswer2IncorrectResponse.StatusCode);
        int answer2IncorrectId = ParseId(postAnswer2IncorrectResponse.Value);

        var question2Update = new QuestionUpdateModel { Text = question2Create.Text, CorrectAnswerId = answer2CorrectId };
        var putQ2Response = await quizClient.PutQuestionAsync(quizId, question2Id, new QuizQuestion { Text = question2Update.Text }, CancellationToken.None);
        Assert.Equal(HttpStatusCode.NoContent, putQ2Response.StatusCode);

        // act: we take the quiz, the idea is to get exactly one question right
        var submission = new QuizSubmission
        {
            Answers = new Dictionary<int, int>
                {
                    { question1Id, answer1CorrectId },   // Correct answer for Q1.
                    { question2Id, answer2IncorrectId }    // Incorrect answer for Q2.
                }
        };
        var submissionPayload = new StringContent(JsonConvert.SerializeObject(submission), Encoding.UTF8, "application/json");
        var submissionRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(_mockProviderServiceBaseUri, $"/api/quizzes/{quizId}/responses"))
        {
            Content = submissionPayload
        };
        submissionRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var submissionResponse = await Client.SendAsync(submissionRequest, CancellationToken.None);
        Assert.Equal(HttpStatusCode.OK, submissionResponse.StatusCode);

        var resultJson = await submissionResponse.Content.ReadAsStringAsync();
        var quizResult = JsonConvert.DeserializeObject<QuizResult>(resultJson);

        // one question right so score should be 1
        Assert.Equal(1, quizResult.Score);
    }
}