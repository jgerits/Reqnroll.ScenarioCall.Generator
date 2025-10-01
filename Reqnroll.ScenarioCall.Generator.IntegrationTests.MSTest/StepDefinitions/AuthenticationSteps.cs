using Reqnroll;

namespace Reqnroll.ScenarioCall.Generator.IntegrationTests.StepDefinitions;

[Binding]
public class AuthenticationSteps
{
    private readonly ScenarioContext _scenarioContext;
    private string? _registeredUsername;
    private string? _registeredPassword;
    private bool _loginSuccessful;
    private bool _userAuthenticated;

    public AuthenticationSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Given(@"there is a user with username ""([^""]*)"" and password ""([^""]*)""")]
    public void GivenThereIsAUserWithUsernameAndPassword(string username, string password)
    {
        _registeredUsername = username;
        _registeredPassword = password;
        _scenarioContext["RegisteredUsername"] = username;
        _scenarioContext["RegisteredPassword"] = password;
    }

    [When(@"the user attempts to log in with username ""([^""]*)"" and password ""([^""]*)""")]
    public void WhenTheUserAttemptsToLogInWithUsernameAndPassword(string username, string password)
    {
        var registeredUsername = _scenarioContext.ContainsKey("RegisteredUsername") 
            ? _scenarioContext["RegisteredUsername"] as string 
            : _registeredUsername;
        var registeredPassword = _scenarioContext.ContainsKey("RegisteredPassword") 
            ? _scenarioContext["RegisteredPassword"] as string 
            : _registeredPassword;

        if (username == registeredUsername && password == registeredPassword)
        {
            _loginSuccessful = true;
            _userAuthenticated = true;
            _scenarioContext["LoginSuccessful"] = true;
            _scenarioContext["UserAuthenticated"] = true;
        }
        else
        {
            _loginSuccessful = false;
            _userAuthenticated = false;
            _scenarioContext["LoginSuccessful"] = false;
            _scenarioContext["UserAuthenticated"] = false;
        }

        // Track login attempts
        if (!_scenarioContext.ContainsKey("LoginAttempts"))
        {
            _scenarioContext["LoginAttempts"] = new List<string>();
        }
        var attempts = _scenarioContext["LoginAttempts"] as List<string>;
        attempts?.Add($"{username}:{password}:{_loginSuccessful}");
    }

    [Then(@"the login should be successful")]
    public void ThenTheLoginShouldBeSuccessful()
    {
        var isSuccessful = _scenarioContext.ContainsKey("LoginSuccessful") 
            ? (bool)_scenarioContext["LoginSuccessful"] 
            : _loginSuccessful;

        if (!isSuccessful)
        {
            throw new Exception("Expected login to be successful, but it failed");
        }
    }

    [Then(@"the login should fail")]
    public void ThenTheLoginShouldFail()
    {
        var isSuccessful = _scenarioContext.ContainsKey("LoginSuccessful") 
            ? (bool)_scenarioContext["LoginSuccessful"] 
            : _loginSuccessful;

        if (isSuccessful)
        {
            throw new Exception("Expected login to fail, but it was successful");
        }
    }

    [Then(@"the user should be authenticated")]
    public void ThenTheUserShouldBeAuthenticated()
    {
        var isAuthenticated = _scenarioContext.ContainsKey("UserAuthenticated") 
            ? (bool)_scenarioContext["UserAuthenticated"] 
            : _userAuthenticated;

        if (!isAuthenticated)
        {
            throw new Exception("Expected user to be authenticated, but they are not");
        }
    }

    [Then(@"the user should not be authenticated")]
    public void ThenTheUserShouldNotBeAuthenticated()
    {
        var isAuthenticated = _scenarioContext.ContainsKey("UserAuthenticated") 
            ? (bool)_scenarioContext["UserAuthenticated"] 
            : _userAuthenticated;

        if (isAuthenticated)
        {
            throw new Exception("Expected user to not be authenticated, but they are");
        }
    }

    [When(@"I navigate to the dashboard")]
    public void WhenINavigateToTheDashboard()
    {
        _scenarioContext["NavigatedToDashboard"] = true;
    }

    [Then(@"I should see the welcome message")]
    public void ThenIShouldSeeTheWelcomeMessage()
    {
        if (!_scenarioContext.ContainsKey("NavigatedToDashboard"))
        {
            throw new Exception("Dashboard was not navigated to");
        }

        var isAuthenticated = _scenarioContext.ContainsKey("UserAuthenticated") 
            ? (bool)_scenarioContext["UserAuthenticated"] 
            : _userAuthenticated;

        if (!isAuthenticated)
        {
            throw new Exception("User is not authenticated, cannot see welcome message");
        }

        // Success - user is authenticated and navigated to dashboard
    }

    [When(@"I perform some action")]
    public void WhenIPerformSomeAction()
    {
        _scenarioContext["ActionPerformed"] = true;
    }

    [Then(@"I should see both login attempts were recorded")]
    public void ThenIShouldSeeBothLoginAttemptsWereRecorded()
    {
        if (!_scenarioContext.ContainsKey("LoginAttempts"))
        {
            throw new Exception("No login attempts were recorded");
        }

        var attempts = _scenarioContext["LoginAttempts"] as List<string>;
        if (attempts == null || attempts.Count < 2)
        {
            throw new Exception($"Expected at least 2 login attempts, but found {attempts?.Count ?? 0}");
        }
    }
}
