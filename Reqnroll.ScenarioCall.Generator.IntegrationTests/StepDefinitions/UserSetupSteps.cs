using Reqnroll;

namespace Reqnroll.ScenarioCall.Generator.IntegrationTests.StepDefinitions;

[Binding]
public class UserSetupSteps
{
    private readonly ScenarioContext _scenarioContext;
    private string? _username;

    public UserSetupSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Given(@"the user management system is initialized")]
    public void GivenTheUserManagementSystemIsInitialized()
    {
        _scenarioContext["UserManagementInitialized"] = true;
    }

    [Given(@"the storage is ready")]
    public void GivenTheStorageIsReady()
    {
        _scenarioContext["StorageReady"] = true;
    }

    [Given(@"I create a user account with username ""([^""]*)""")]
    public void GivenICreateAUserAccountWithUsername(string username)
    {
        _username = username;
        _scenarioContext["CreatedUsername"] = username;
    }

    [When(@"the account is saved")]
    public void WhenTheAccountIsSaved()
    {
        _scenarioContext["AccountSaved"] = true;
    }

    [Then(@"the account should exist")]
    public void ThenTheAccountShouldExist()
    {
        if (!_scenarioContext.ContainsKey("AccountSaved"))
        {
            throw new Exception("Account was not saved");
        }
    }

    [Given(@"a user account exists with username ""([^""]*)""")]
    public void GivenAUserAccountExistsWithUsername(string username)
    {
        _username = username;
        _scenarioContext["ExistingUsername"] = username;
    }

    [When(@"I check if the user exists")]
    public void WhenICheckIfTheUserExists()
    {
        _scenarioContext["UserExistenceChecked"] = true;
    }

    [Then(@"the user should be found")]
    public void ThenTheUserShouldBeFound()
    {
        if (!_scenarioContext.ContainsKey("UserExistenceChecked"))
        {
            throw new Exception("User existence was not checked");
        }
    }

    [Then(@"the verification should succeed")]
    public void ThenTheVerificationShouldSucceed()
    {
        // Verify that background was executed for both scenario calls
        if (!_scenarioContext.ContainsKey("UserManagementInitialized"))
        {
            throw new Exception("Background step 'user management system is initialized' was not executed");
        }
        if (!_scenarioContext.ContainsKey("StorageReady"))
        {
            throw new Exception("Background step 'storage is ready' was not executed");
        }
        
        // Verify both scenarios were called
        if (!_scenarioContext.ContainsKey("AccountSaved"))
        {
            throw new Exception("Create User Account scenario was not executed");
        }
        if (!_scenarioContext.ContainsKey("UserExistenceChecked"))
        {
            throw new Exception("Verify User Exists scenario was not executed");
        }
    }
}
