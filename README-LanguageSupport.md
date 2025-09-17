## Language Support

The plugin supports multiple languages through Gherkin's language specification. You can use localized step keywords while keeping the scenario call syntax in English.

### Supported Languages

The plugin supports all languages that Gherkin supports, including:

- **English**: `Given`, `When`, `Then`, `And`, `But`
- **German**: `Angenommen`, `Wenn`, `Dann`, `Und`, `Aber`
- **French**: `Soit`, `Quand`, `Alors`, `Et`, `Mais`
- **Spanish**: `Dado`, `Cuando`, `Entonces`, `Y`, `Pero`
- **Portuguese**: `Dado`, `Quando`, `Então`, `E`, `Mas`
- **Italian**: `Dato`, `Quando`, `Allora`, `E`, `Ma`
- And many more...

### Usage with Different Languages

#### German Example

```gherkin
#language: de-DE
Feature: Benutzeranmeldung
  Als Benutzer möchte ich mich anmelden können

Scenario: Erfolgreiche Anmeldung
  Angenommen I call scenario "Vorbereitung" from feature "Setup"
  Und I call scenario "Login" from feature "Authentication"
  Dann sollte ich auf der Startseite sein
```

Referenced German feature file (`Authentication.feature`):
```gherkin
#language: de-DE
Feature: Authentication
Scenario: Login
  Angenommen ich bin auf der Anmeldeseite
  Wenn ich gültige Anmeldedaten eingebe
  Dann sollte ich erfolgreich angemeldet sein
```

#### French Example

```gherkin
#language: fr-FR
Feature: Authentification utilisateur
  En tant qu'utilisateur, je veux pouvoir me connecter

Scenario: Connexion réussie
  Soit I call scenario "Préparation" from feature "Configuration"
  Et I call scenario "Connexion" from feature "Authentification"
  Alors je devrais voir la page d'accueil
```

#### Mixed Language Support

You can call scenarios from features written in different languages:

```gherkin
#language: de-DE
Feature: Internationaler Test
Scenario: Gemischte Sprachen
  Angenommen I call scenario "Setup" from feature "EnglishFeature"
  Und I call scenario "Vorbereitung" from feature "GermanFeature"
  Dann sollte alles funktionieren
```

### Language Detection

The plugin automatically detects the language from:

1. **Language directive**: `#language: de-DE` at the top of feature files
2. **Fallback mechanism**: `de-DE` → `de` → `en` if specific culture isn't found
3. **Default**: English (`en`) if no language is specified

### Notes

- The scenario call syntax `I call scenario "..." from feature "..."` remains in English for consistency
- Only the Gherkin step keywords (`Given`, `When`, etc.) are localized
- The plugin supports both specific cultures (`de-DE`) and base languages (`de`)
- Language detection follows the same rules as Reqnroll itself