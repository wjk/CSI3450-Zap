using OpenQA.Selenium;
using MySqlConnector;
using Xunit;

using Zap.Specs.Drivers;
using Zap.Specs.PageObjects;

namespace Zap.Specs.Steps;

[Binding]
public class SignInStepDefinitions : IDisposable
{
    private readonly IWebDriver _driver;
    private SignInPageObject? _pageObject;

    public SignInStepDefinitions()
    {
        var factory = new BrowserDriver();
        _driver = factory.CreateWebDriver();
    }

    public void Dispose()
    {
        _driver.Dispose();
    }

    [Given("the default test user exists")]
    public void CreateDefaultTestUser()
    {
        using MySqlConnection connection = new MySqlConnection(
            "Server=localhost;" +
            "Database=Zap;" +
            "User=Zap;" +
            "Password=Zap1234;"
        );
        
        connection.Open();

        using (MySqlCommand command =
               new MySqlCommand("DELETE FROM USER WHERE EMAIL = \"testuser@example.com\"", connection))
        {
            command.ExecuteNonQuery();
        }
        
        // password: testuser
        using (MySqlCommand command =
               new MySqlCommand(
                   "INSERT INTO USER (EMAIL, FIRST_NAME, LAST_NAME, HASHED_PASSWORD) VALUES (@email, @first, @last, @password)", connection))
        {
            command.Parameters.Add(new MySqlParameter("email", "testuser@example.com"));
            command.Parameters.Add(new MySqlParameter("first", "Unit Test"));
            command.Parameters.Add(new MySqlParameter("last", "Example User"));
            command.Parameters.Add(new MySqlParameter("password", "AQAAAAEAACcQAAAAEOZlm4EihkZazrGyeSplhVdxShMOc6/bmgxbS95B25/gDI/0pVTvqc9y15rVyKUykg=="));

            command.ExecuteNonQuery();
        }

        connection.Close();
    }

    [Given("the login page is loaded")]
    public void LoadPage()
    {
        _pageObject = new SignInPageObject(_driver);
        _pageObject.Initialize();
    }

    [Given("the email address is \"(.*)\"")]
    public void GivenTheEmailAddressIs(string emailAddress)
    {
        ArgumentNullException.ThrowIfNull(_pageObject);
        _pageObject.EnterEmailAddress(emailAddress);
    }

    [Given("the password is \"(.*)\"")]
    public void GivenThePasswordIs(string password)
    {
        ArgumentNullException.ThrowIfNull(_pageObject);
        _pageObject.EnterPassword(password);
    }

    [When("the login is submitted")]
    public void WhenTheLoginIsSubmitted()
    {
        ArgumentNullException.ThrowIfNull(_pageObject);
        _pageObject.Submit();
    }

    [Then("the user should be logged in")]
    public void ThenTheUserShouldBeLoggedIn()
    {
        Assert.Equal("https://localhost:5001/Home/Chatbot", _driver.Url);
        
        var element = _driver.FindElement(By.Id("login-friendly"));
        Assert.Equal("Test User", element.Text);
    }

    [Then("the login should fail")]
    public void ThenTheLoginShouldFail()
    {
        Assert.Equal("https://localhost:5001/Account/SignIn", _driver.Url);

        var elements = _driver.FindElements(By.Id("login-error-box"));
        Assert.NotNull(elements.Single());
    }
}