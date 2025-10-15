# language: nl
Functionaliteit: Gebruikersbeheer
    Als beheerder
    Wil ik gebruikersaccounts beheren
    Zodat ik de toegang tot het systeem kan controleren

Scenario: Nieuw gebruikersaccount aanmaken
    Gegeven ik roep scenario "Inloggen met geldige inloggegevens" aan uit functionaliteit "Authenticatie"
    Als ik naar het gebruikersbeheersectie navigeer
    En ik klik op "Nieuwe gebruiker toevoegen" knop
    En ik het formulier voor nieuwe gebruikers invul:
        | Veld          | Waarde                  |
        | Gebruikersnaam| jane.jansen             |
        | E-mail        | jane.jansen@test.nl     |
        | Rol           | Standaard gebruiker     |
    En ik klik op "Gebruiker aanmaken" knop
    Dan zou ik "Gebruiker succesvol aangemaakt" bericht moeten zien
    En zou ik "jane.jansen" in de gebruikerslijst moeten zien
    En ik roep scenario "Uitloggen" aan uit functionaliteit "Authenticatie"

Scenario: Gebruikersaccount verwijderen
    Gegeven ik roep scenario "Inloggen met geldige inloggegevens" aan uit functionaliteit "Authenticatie"
    Als ik naar het gebruikersbeheersectie navigeer
    En ik selecteer "jane.jansen" uit de lijst
    En ik klik op "Gebruiker verwijderen" knop
    En ik bevestig de verwijdering
    Dan zou ik "Gebruiker succesvol verwijderd" bericht moeten zien
    En zou ik "jane.jansen" niet in de gebruikerslijst moeten zien
    En ik roep scenario "Uitloggen" aan uit functionaliteit "Authenticatie"

Scenario: Gebruikersmachtigingen bijwerken
    Gegeven ik roep scenario "Inloggen met geldige inloggegevens" aan uit functionaliteit "Authenticatie"
    Als ik naar het gebruikersbeheersectie navigeer
    En ik selecteer "jane.jansen" uit de lijst
    En ik klik op "Machtigingen bewerken" knop
    En ik wijzig de rol naar "Beheerder"
    En ik klik op "Wijzigingen opslaan" knop
    Dan zou ik "Machtigingen succesvol bijgewerkt" bericht moeten zien
    En zou ik "Beheerder" rol voor "jane.jansen" moeten zien
    En ik roep scenario "Uitloggen" aan uit functionaliteit "Authenticatie"
