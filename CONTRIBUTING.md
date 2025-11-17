# Code Style

## Formatting

- Recommended line width: < 80 characters
- Hard maximum line width: < 100 characters

## Naming Conventions

### C#

- Classes: `PascalCase`
- Namespaces: `PascalCase`
- Methods: `PascalCase`
- Properties: `PascalCase`
- Public Fields: `PascalCase`
- Private / Protected Fields: `_underscoreCamelCase` (example: `_currentState`, `_movementSpeed`)
- Local Variables (inside methods): `camelCase`
- Constants: `UPPER_SNAKE_CASE`

### GDScript

- Classes: `PascalCase`
- General identifiers (classes, methods, variables): `snake_case`
- Constants: `UPPER_SNAKE_CASE`

# Commit Messages

- First line summary/message:
    - Use the present tense ("Add feature" not "Added feature")
    - Use the imperative mood ("Move cursor to..." not "Moves cursor to...")
    - Limit to 50 characters
- Body:
    - Limit to 72 characters per line

# Pull Requests

- Provide a clear and concise title
- Include a detailed description of the changes made
- For reviewers: **squash commits when merging**
- GitHub will automatically create a title and description based on the commit
  messages, but feel free to edit them for clarity and completeness.
