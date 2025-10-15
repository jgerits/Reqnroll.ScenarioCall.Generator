# language: nl
Functionaliteit: Bestellingenbeheer
    Als klant
    Wil ik mijn bestellingen beheren
    Zodat ik producten kan kopen
    
    Deze functionaliteit demonstreert het aanroepen van scenario's uit GedeeldeAuthenticatiebibliotheek
    (een apart project) met behulp van de Reqnroll.ScenarioCall.Generator plugin.

Scenario: Bestelling plaatsen als geauthenticeerde gebruiker
    # Deze scenario-aanroep verwijst naar het GedeeldeAuthenticatiebibliotheek-project
    # Het scenario wordt tijdens het bouwen uitgebreid door de generator-plugin
    Gegeven ik roep scenario "Inloggen met geldige inloggegevens" aan uit functionaliteit "Gedeelde Authenticatie"
    Als ik naar de productenpagina navigeer
    En ik voeg "Widget" toe aan winkelwagen
    En ik voeg "Gadget" toe aan winkelwagen
    En ik ga verder naar afrekenen
    Dan zou ik bestelbevestiging moeten zien
    En zou ik "Bestelling #12345" in mijn bestelgeschiedenis moeten zien

Scenario: Bestelgeschiedenis bekijken
    # Een ander voorbeeld van het aanroepen van een cross-project scenario
    Gegeven ik roep scenario "Inloggen met geldige inloggegevens" aan uit functionaliteit "Gedeelde Authenticatie"
    Als ik naar mijn accountpagina navigeer
    En ik klik op "Bestelgeschiedenis"
    Dan zou ik een lijst van mijn eerdere bestellingen moeten zien
    En zou ik besteldatums en totalen moeten zien

Scenario: Bestelling annuleren
    # Cross-project scenario-aanroep gevolgd door bedrijfslogica
    Gegeven ik roep scenario "Inloggen met geldige inloggegevens" aan uit functionaliteit "Gedeelde Authenticatie"
    Als ik naar mijn accountpagina navigeer
    En ik klik op "Bestelgeschiedenis"
    En ik selecteer bestelling "12345"
    En ik klik op "Bestelling annuleren" knop
    Dan zou ik "Bestelling succesvol geannuleerd" bericht moeten zien
    En bestelling "12345" zou status "Geannuleerd" moeten hebben
