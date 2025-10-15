using Reqnroll;

namespace MSTestExample.StepDefinitions;

[Binding]
public class AuthenticationSteps
{
    private readonly ScenarioContext _scenarioContext;

    public AuthenticationSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Given(@"I am on the login page")]
    [Given(@"ik ben op de inlogpagina")]
    public void GivenIAmOnTheLoginPage()
    {
        Console.WriteLine("Navigating to login page");
        _scenarioContext["OnLoginPage"] = true;
    }

    [When(@"I enter username ""(.*)""")]
    [When(@"ik gebruikersnaam ""(.*)"" invoer")]
    public void WhenIEnterUsername(string username)
    {
        Console.WriteLine($"Entering username: {username}");
        _scenarioContext["Username"] = username;
    }

    [When(@"I enter password ""(.*)""")]
    [When(@"ik wachtwoord ""(.*)"" invoer")]
    public void WhenIEnterPassword(string password)
    {
        Console.WriteLine($"Entering password: {password}");
        _scenarioContext["Password"] = password;
    }

    [When(@"I click the login button")]
    [When(@"ik op de inlogknop klik")]
    public void WhenIClickTheLoginButton()
    {
        Console.WriteLine("Clicking login button");
        var username = _scenarioContext["Username"] as string;
        var password = _scenarioContext["Password"] as string;
        
        // Simulate login logic
        if (username == "john.doe@example.com" && password == "SecurePassword123")
        {
            _scenarioContext["LoggedIn"] = true;
            _scenarioContext["LoginError"] = null;
        }
        else
        {
            _scenarioContext["LoggedIn"] = false;
            _scenarioContext["LoginError"] = "Invalid credentials";
        }
    }

    [Then(@"I should be redirected to the dashboard")]
    [Then(@"zou ik naar het dashboard geleid moeten worden")]
    public void ThenIShouldBeRedirectedToTheDashboard()
    {
        Console.WriteLine("Verifying redirect to dashboard");
        if (!_scenarioContext.ContainsKey("LoggedIn") || !(bool)_scenarioContext["LoggedIn"])
        {
            throw new Exception("User was not logged in, cannot redirect to dashboard");
        }
        _scenarioContext["OnDashboard"] = true;
    }

    [Then(@"I should see ""(.*)"" message")]
    [Then(@"zou ik ""(.*)"" bericht moeten zien")]
    public void ThenIShouldSeeMessage(string message)
    {
        Console.WriteLine($"Verifying message: {message}");
        // In a real test, you would verify the message is displayed
    }

    [Given(@"I am logged into the system")]
    [Given(@"ik ben ingelogd in het systeem")]
    public void GivenIAmLoggedIntoTheSystem()
    {
        Console.WriteLine("Setting up logged in state");
        _scenarioContext["LoggedIn"] = true;
        _scenarioContext["Username"] = "john.doe@example.com";
    }

    [When(@"I click the logout button")]
    [When(@"ik op de uitlogknop klik")]
    public void WhenIClickTheLogoutButton()
    {
        Console.WriteLine("Clicking logout button");
        _scenarioContext["LoggedIn"] = false;
        _scenarioContext["OnLoginPage"] = true;
    }

    [Then(@"I should be redirected to the login page")]
    [Then(@"zou ik naar de inlogpagina geleid moeten worden")]
    public void ThenIShouldBeRedirectedToTheLoginPage()
    {
        Console.WriteLine("Verifying redirect to login page");
        if (!_scenarioContext.ContainsKey("OnLoginPage") || !(bool)_scenarioContext["OnLoginPage"])
        {
            throw new Exception("User was not redirected to login page");
        }
    }

    [Then(@"I should see ""(.*)"" error message")]
    [Then(@"zou ik ""(.*)"" foutmelding moeten zien")]
    public void ThenIShouldSeeErrorMessage(string errorMessage)
    {
        Console.WriteLine($"Verifying error message: {errorMessage}");
        if (!_scenarioContext.ContainsKey("LoginError"))
        {
            throw new Exception($"Expected error message '{errorMessage}' but no error was found");
        }
    }

    [Then(@"I should remain on the login page")]
    [Then(@"zou ik op de inlogpagina moeten blijven")]
    public void ThenIShouldRemainOnTheLoginPage()
    {
        Console.WriteLine("Verifying still on login page");
        if (_scenarioContext.ContainsKey("LoggedIn") && (bool)_scenarioContext["LoggedIn"])
        {
            throw new Exception("User should not be logged in");
        }
    }
}
