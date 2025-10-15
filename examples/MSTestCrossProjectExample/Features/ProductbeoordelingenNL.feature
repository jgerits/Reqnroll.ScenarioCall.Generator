# language: nl
Functionaliteit: Productbeoordelingen
    Als klant
    Wil ik productbeoordelingen schrijven en lezen
    Zodat ik mijn ervaring met andere klanten kan delen

Scenario: Productbeoordeling indienen
    # Hergebruik het inlogscenario uit de gedeelde authenticatiebibliotheek
    Gegeven ik roep scenario "Inloggen met geldige inloggegevens" aan uit functionaliteit "Gedeelde Authenticatie"
    Als ik naar product "Widget" navigeer
    En ik klik op "Schrijf een beoordeling" knop
    En ik voer beoordelingstitel "Geweldig product!" in
    En ik voer beoordelingstekst "Deze widget werkt perfect en overtrof mijn verwachtingen." in
    En ik selecteer waardering "5" sterren
    En ik dien de beoordeling in
    Dan zou ik "Bedankt voor uw beoordeling" bericht moeten zien
    En zou mijn beoordeling op de productpagina moeten verschijnen

Scenario: Mijn beoordeling bewerken
    # Een ander voorbeeld van hergebruik van cross-project scenario
    Gegeven ik roep scenario "Inloggen met geldige inloggegevens" aan uit functionaliteit "Gedeelde Authenticatie"
    Als ik naar mijn accountpagina navigeer
    En ik klik op "Mijn beoordelingen"
    En ik selecteer beoordeling voor product "Widget"
    En ik klik op "Beoordeling bewerken" knop
    En ik wijzig beoordelingstekst naar "Bijgewerkt: Nog steeds dol op deze widget na 6 maanden!"
    En ik sla de beoordeling op
    Dan zou ik "Beoordeling succesvol bijgewerkt" bericht moeten zien

Scenario: Ongepaste beoordeling rapporteren
    # Gebruik het inlogscenario en vervolgens rapportagefunctionaliteit
    Gegeven ik roep scenario "Inloggen met geldige inloggegevens" aan uit functionaliteit "Gedeelde Authenticatie"
    Als ik naar product "Gadget" navigeer
    En ik zie een beoordeling met ongepaste inhoud
    En ik klik op "Beoordeling rapporteren" link
    En ik selecteer reden "Spam of ongepaste inhoud"
    En ik dien het rapport in
    Dan zou ik "Rapport ingediend" bericht moeten zien
    En zou de beoordeling gemarkeerd moeten worden voor moderatie
