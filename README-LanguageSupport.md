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
  Angenommen ich rufe Szenario "Vorbereitung" aus Feature "Setup"
  Und ich rufe Szenario "Login" aus Feature "Authentication"
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
  Soit j'appelle le scénario "Préparation" de la fonctionnalité "Configuration"
  Et j'appelle le scénario "Connexion" de la fonctionnalité "Authentification"
  Alors je devrais voir la page d'accueil
```

#### Mixed Language Support

You can call scenarios from features written in different languages:

```gherkin
#language: de-DE
Feature: Internationaler Test
Scenario: Gemischte Sprachen
  Angenommen ich rufe Szenario "Setup" aus Feature "EnglishFeature"
  Und ich rufe Szenario "Vorbereitung" aus Feature "GermanFeature"
  Dann sollte alles funktionieren
```

#### Backward Compatibility

For backward compatibility, you can still use English scenario call phrases with localized step keywords:

```gherkin
#language: de-DE
Feature: Backward Compatibility Test
Scenario: Mixed Phrases
  Angenommen I call scenario "Setup" from feature "EnglishFeature"
  Und ich rufe Szenario "Vorbereitung" aus Feature "GermanFeature"
```

### Language Detection

The plugin automatically detects the language from:

1. **Language directive**: `#language: de-DE` at the top of feature files
2. **Fallback mechanism**: `de-DE` → `de` → `en` if specific culture isn't found
3. **Default**: English (`en`) if no language is specified

### Localized Scenario Call Phrases

The plugin now supports fully localized scenario call phrases:

| Language | Phrase Template |
|----------|----------------|
| English | `I call scenario "..." from feature "..."` |
| German | `ich rufe Szenario "..." aus Feature "..."` |
| French | `j'appelle le scénario "..." de la fonctionnalité "..."` |
| Spanish | `llamo al escenario "..." de la funcionalidad "..."` |
| Italian | `chiamo lo scenario "..." dalla funzionalità "..."` |
| Portuguese | `chamo o cenário "..." da funcionalidade "..."` |
| Dutch | `ik roep scenario "..." van feature "..."` |

### Notes

- **Localized phrases**: Both step keywords AND scenario call phrases are now localized for natural language experience
- **Backward compatibility**: English scenario call phrases still work with localized step keywords
- The plugin supports both specific cultures (`de-DE`) and base languages (`de`)
- Language detection follows the same rules as Reqnroll itself
- **New in this version**: Fully localized scenario call syntax for international teams