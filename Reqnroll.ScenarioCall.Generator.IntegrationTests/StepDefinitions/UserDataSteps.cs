using Reqnroll;
using Xunit;

namespace Reqnroll.ScenarioCall.Generator.IntegrationTests.StepDefinitions;

[Binding]
public class UserDataSteps
{
    private readonly ScenarioContext _scenarioContext;
    private Dictionary<string, string>? _userData;
    private bool _userCreated;

    public UserDataSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Given(@"I have the following user data:")]
    public void GivenIHaveTheFollowingUserData(Table table)
    {
        _userData = new Dictionary<string, string>();
        foreach (var row in table.Rows)
        {
            _userData[row["Field"]] = row["Value"];
        }
        _scenarioContext["UserData"] = _userData;
    }

    [When(@"I create the user with the data")]
    public void WhenICreateTheUserWithTheData()
    {
        var userData = _scenarioContext.ContainsKey("UserData") 
            ? _scenarioContext["UserData"] as Dictionary<string, string> 
            : _userData;

        if (userData != null && userData.ContainsKey("Username"))
        {
            _userCreated = true;
            _scenarioContext["UserCreated"] = true;
            _scenarioContext["CreatedUserData"] = userData;
        }
    }

    [Then(@"the user should exist with the provided data")]
    public void ThenTheUserShouldExistWithTheProvidedData()
    {
        var userCreated = _scenarioContext.ContainsKey("UserCreated") 
            ? (bool)_scenarioContext["UserCreated"] 
            : _userCreated;

        Assert.True(userCreated, "User was not created");
        
        var userData = _scenarioContext["CreatedUserData"] as Dictionary<string, string>;
        Assert.NotNull(userData);
        Assert.Contains("Username", userData.Keys);
        Assert.Contains("Email", userData.Keys);
        Assert.Contains("Role", userData.Keys);
    }

    [When(@"I verify the user was created")]
    public void WhenIVerifyTheUserWasCreated()
    {
        var userCreated = _scenarioContext.ContainsKey("UserCreated") 
            ? (bool)_scenarioContext["UserCreated"] 
            : false;

        Assert.True(userCreated, "User was not created in the called scenario");
    }

    [Then(@"the user should be in the system")]
    public void ThenTheUserShouldBeInTheSystem()
    {
        var userData = _scenarioContext.ContainsKey("CreatedUserData") 
            ? _scenarioContext["CreatedUserData"] as Dictionary<string, string> 
            : null;

        Assert.NotNull(userData);
        Assert.Equal("john.doe", userData["Username"]);
        Assert.Equal("john.doe@example.com", userData["Email"]);
        Assert.Equal("Admin", userData["Role"]);
    }
}
