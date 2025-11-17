# Code Style Guide

## Formatting

- Recommended line width: < 80 characters
- Hard maximum line width: < 100 characters

## C#

Coding conventions should follow
[Godot C# style guide](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_style_guide.html)
with the following specifics:

### General Formatting

- **Spaces** instead of tabs for indentation (4 spaces per indent level)
- Allman braces on new lines
- UTF-8 encoding
- Unix-style line endings (LF `\n`, not CRLF `\r\n`)
- One line feed at end of each file, except for `.csproj` files

```csharp
/** Follow Allman brace style **/
if (condition)
{
    // code
}

/** NOT this **/
if (condition) {
    // code
}
```

### Spaces

- Space after keywords (e.g., `if`, `for`, `while`, `switch`, etc.)
- Space after commas in lists, and after semicolons in `for` loops
- Space surrounding operators (e.g., `x + y`)
- Space between an empty brace pair: `{ }`
- No space before commas, semicolons, colons
- No space between method name and opening parenthesis
- No spaces surrounded by parentheses
- No space after unary operators (e.g., `!flag`, `-value`)
- No space after type cast parentheses (e.g., `(int)x`)

### Naming Conventions

- Classes: `PascalCase`
- Interfaces: `IPascalCase` (e.g. `IDamageable`)
- Namespaces: `PascalCase`
- Methods: `PascalCase`
- Properties: `PascalCase`
- Public Fields: `PascalCase`
- Private / Protected Fields: `_underscoreCamelCase` (e.g. `_currentState`,
  `_movementSpeed`)
- Local Variables (inside methods): `camelCase`
- Constants: `PascalCase`

### Additional Notes

Please use an external editor or IDE with official C# support instead of
Godot's built-in script editor as it lacks many essential features for C#
development.

## GDScript

Coding conventions should follow Godot's
[GDScript style guide](https://docs.godotengine.org/en/stable/tutorials/scripting/gdscript/gdscript_styleguide.html)
with the following specifics:

- Classes: `PascalCase`
- General identifiers (classes, methods, variables): `snake_case`
- Constants: `UPPER_SNAKE_CASE`
- **Tabs** instead of spaces for indentation (1 tab per indent level)
- Opt for type hints wherever possible

# File Organization

- File system should mirror C# namespaces
    - Although GDScript does not use namespaces, organize GDScript files in a similar
      manner for consistency
- Game assets (textures, sounds, etc.) should be placed in the `Assets`
  directory with appropriate subdirectories

Example file structure:

```
Assets/
    Textures/
        CharacterSprite.png
    Sounds/
        JumpSound.wav
States/
    Characters/
        CharacterMoveState.cs
        CharacterIdleState.gd
Characters/
    Character.cs
    CloneableComponent.cs
    HealthComponent.cs
    Controllers/
        CharacterController.cs
        UserController.cs
        AIController.gd
```

# Commit Messages

- First line summary/message:
    - Use the present tense ("Add feature" not "Added feature")
    - Use the imperative mood ("Move cursor to..." not "Moves cursor to...")
    - Limit to 50 characters
- Body:
    - Limit to 72 characters per line

# Pulling from Upstream

- **Use rebase instead of merge when updating your branch from upstream**
  to maintain a cleaner commit history
- Regularly pull from the upstream repository to stay updated with the
  latest changes
- Before pushing your changes, ensure your branch is up to date with the
  upstream main branch to minimize merge conflicts

# Pull Requests

- Provide a clear and concise title
- Include a detailed description of the changes made
- For reviewers:
    - **Squash and merge** if commits are minor fixes or adjustments, if the
      commit history is not clean, or there is a commit in history that is
      incomplete or broken
    - Otherwise, use **Rebase and merge** to maintain individual commits
- GitHub will automatically create a title and description based on the commit
  messages, but feel free to edit them for clarity and completeness.
