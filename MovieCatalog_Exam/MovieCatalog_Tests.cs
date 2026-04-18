using System.Net;
using System.Text.Json;
using MovieCatalog_Exam.DTOs;
using RestSharp;
using RestSharp.Authenticators;

namespace MovieCatalog_Exam
{
    [TestFixture]
    public class MovieCatalog_Tests
    {
        private RestClient client;

        private const string BaseUrl = "http://144.91.123.158:5000/api";
        private const string Email = "angeorgieva@mail.com";
        private const string Password = "123123";

        private static string? lastMovieId;

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken(Email, Password);

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("/User/Authentication", Method.Post);

            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(new { email, password });

            var response = tempClient.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK || string.IsNullOrWhiteSpace(response.Content))
                throw new Exception($"Login failed: {response.Content}");

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            if (!json.TryGetProperty("accessToken", out var tokenElement))
                throw new InvalidOperationException("Token not found in response.");

            var token = tokenElement.GetString();

            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("Token is empty.");

            return token;
        }

        [Test, Order(1)]
        public void CreateMovieWithRequiredFields_ShouldReturnCreated()
        {
            var movie = new MovieDTO
            {
                Title = "Test Exam Movie",
                Description = "This is a test movie."
            };

            var request = new RestRequest("/Movie/Create", Method.Post);
            request.AddJsonBody(movie);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Is.Not.Null.And.Not.Empty);

            var data = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content!);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Movie.Id, Is.Not.Null.And.Not.Empty, "Movie ID should be returned");
            Assert.That(data.Msg, Is.EqualTo("Movie created successfully!"));

            lastMovieId = data.Movie.Id;
            Assert.That(lastMovieId, Is.Not.Null.And.Not.Empty);
        }

        [Test, Order(2)]
        public void EditMovie_ShouldReturnSuccess()
        {
            Assert.That(lastMovieId, Is.Not.Null.And.Not.Empty, "Movie ID is not set from previous test.");

            var updatedMovie = new MovieDTO
            {
                Title = "Edited Test Movie",
                Description = "This movie has been edited."
            };

            var request = new RestRequest("/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", lastMovieId);
            request.AddJsonBody(updatedMovie);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Is.Not.Null.And.Not.Empty);

            var data = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content!);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Test, Order(3)]
        public void GetAllMovies_ShouldReturnNonEmptyArray()
        {
            var request = new RestRequest("/Catalog/All", Method.Get);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Is.Not.Null.And.Not.Empty);

            var data = JsonSerializer.Deserialize<List<MovieDTO>>(response.Content!);

            Assert.That(data, Is.Not.Null.And.Not.Empty);
        }

        [Test, Order(4)]
        public void DeleteMovie_ShouldReturnSuccess()
        {
            Assert.That(lastMovieId, Is.Not.Null.And.Not.Empty, "Movie ID is not set.");

            var request = new RestRequest("/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", lastMovieId);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Is.Not.Null.And.Not.Empty);

            var data = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content!);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Test, Order(5)]
        public void CreateMovieWithoutRequiredFields_ShouldReturnBadRequest()
        {
            var movie = new MovieDTO();

            var request = new RestRequest("/Movie/Create", Method.Post);
            request.AddJsonBody(movie);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditNonExistingMovie_ShouldReturnBadRequest()
        {
            var fakeId = "12345678";

            var updatedMovie = new MovieDTO
            {
                Title = "Non Existing Movie",
                Description = "Non Existing Movie Desc"
            };

            var request = new RestRequest("/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", fakeId);
            request.AddJsonBody(updatedMovie);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Is.Not.Null.And.Not.Empty);

            var data = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content!);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Msg, Is.EqualTo(
                "Unable to edit the movie! Check the movieId parameter or user verification!"));
        }

        [Test, Order(7)]
        public void DeleteNonExistingMovie_ShouldReturnBadRequest()
        {
            var fakeId = "12345678";

            var request = new RestRequest("/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", fakeId);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Is.Not.Null.And.Not.Empty);

            var data = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content!);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Msg, Is.EqualTo(
                "Unable to delete the movie! Check the movieId parameter or user verification!"));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            client?.Dispose();
        }
    }
}